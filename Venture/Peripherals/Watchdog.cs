using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Venture.Processor;

namespace Venture.Peripherals;

/// <summary>
/// RP2350 Watchdog timer at 0x400D8000.
///
/// The watchdog counts down from the value written to LOAD (in microseconds) toward
/// zero. If CTRL.ENABLE is set and the countdown reaches zero before firmware writes
/// LOAD again ("pets the watchdog"), a system reset is triggered.
///
/// Writing 1 to CTRL.TRIGGER forces an immediate reset without waiting for the
/// countdown (used by the SDK's watchdog_reboot() function).
///
/// Reset cause is recorded in the REASON register and survives soft resets. Firmware
/// reads REASON on boot to determine whether it is recovering from a watchdog event.
///
/// SCRATCH0–7 are eight general-purpose registers that survive watchdog resets but
/// are cleared by a power-on reset. The bootrom and SDK use them to pass data across
/// reboots (e.g., to communicate a requested reboot destination).
///
/// The TICK register controls the tick generator that divides clk_ref down to the
/// 1 MHz tick used by the watchdog and system timers. In the emulator the tick
/// generator is not physically simulated — fields are stored for SDK readback and
/// RUNNING always reads 1.
///
/// Spec: RP2350 Datasheet §12.9 — "Watchdog".
/// </summary>
public class Watchdog : PeripheralBase
{
    // ─── CTRL fields ──────────────────────────────────────────────────────────

    // ENABLE (bit 30): when 1 the countdown is active and a timeout causes a reset.
    private bool m_Enable;

    // PAUSE_DBG1 (bit 26): pause countdown while processor 1 is in debug mode.
    // PAUSE_DBG0 (bit 25): pause countdown while processor 0 is in debug mode.
    // PAUSE_JTAG (bit 24): pause countdown while JTAG is accessing the bus.
    // All three default to 1 (pause on debug) per Table 1247.
    // Stored for readback; the emulator never pauses the countdown.
    private bool m_PauseDbg1 = true;
    private bool m_PauseDbg0 = true;
    private bool m_PauseJtag = true;

    // ─── Countdown state ──────────────────────────────────────────────────────

    // Value written to LOAD; the countdown starts from here.
    // Units: microseconds. Maximum value: 0xFFFFFF ≈ 16.7 s. (Table 1248)
    private uint m_LoadValueUs;

    // Stopwatch epoch for the current countdown interval.
    // Used to compute CTRL.TIME = m_LoadValueUs - elapsed since last LOAD write.
    private readonly Stopwatch m_CountdownStopwatch = new();

    // .NET timer that fires when the countdown reaches zero.
    private System.Threading.Timer? m_WatchdogTimer;

    // ─── REASON ───────────────────────────────────────────────────────────────

    // REASON.TIMER (bit 0): set when a watchdog timeout (countdown reached zero) caused
    // the last reset. Cleared by power-on reset or debugger warm reset.
    private bool m_ReasonTimer;

    // REASON.FORCE (bit 1): set when a software-triggered (CTRL.TRIGGER=1) reset
    // caused the last reset.
    private bool m_ReasonForce;

    // ─── SCRATCH registers ────────────────────────────────────────────────────

    // Eight 32-bit scratch registers preserved across watchdog resets.
    // The SDK uses SCRATCH4–7 for bootloader parameters; SCRATCH0–3 for application use.
    private readonly uint[] m_Scratch = new uint[8];

    // ─── TICK register ────────────────────────────────────────────────────────

    // CYCLES (bits 7:0): clk_ref cycles per 1 MHz tick. Typical value: 12 (12 MHz XOSC).
    private uint m_TickCycles;

    // ENABLE (bit 8): enable the tick generator.
    private bool m_TickEnable;

    // ─── Reset callback ───────────────────────────────────────────────────────

    private readonly Hazard3Processor m_Processor;

    /// <summary>
    /// Raised whenever a watchdog reset is triggered (either by timeout or by
    /// writing CTRL.TRIGGER=1). Subscribers can reset emulator state accordingly.
    /// The processor PC is reset to the RISC-V boot entry point before this event fires.
    /// </summary>
    public event Action? WatchdogTriggered;

    public Watchdog(uint baseAddress, string name, ILogger<Watchdog> logger, Hazard3Processor processor)
        : base(baseAddress, name, logger)
    {
        m_Processor = processor;

        // CTRL (0x00) — main control register.
        // Bit 31 TRIGGER: SC (self-clearing) — writing 1 causes an immediate reset.
        //   Reads always return 0 (the bit does not latch).
        // Bit 30 ENABLE:     countdown active; timeout causes reset.
        // Bit 29:27          Reserved.
        // Bit 26 PAUSE_DBG1: pause on core-1 debug halt. Default 1.
        // Bit 25 PAUSE_DBG0: pause on core-0 debug halt. Default 1.
        // Bit 24 PAUSE_JTAG: pause during JTAG access. Default 1.
        // Bits 23:0 TIME:    current countdown value (µs, read-only; computed live).
        //
        // Spec: Table 1247.
        AddRegister(0x00, "CTRL", resetValue: (1u << 26) | (1u << 25) | (1u << 24))
            .Field(lsb: 31, getter: () => false)                             // TRIGGER (always reads 0)
            .Field(lsb: 30, getter: () => m_Enable,    setter: SetEnable)
            .Field(lsb: 26, getter: () => m_PauseDbg1, setter: v => m_PauseDbg1 = v)
            .Field(lsb: 25, getter: () => m_PauseDbg0, setter: v => m_PauseDbg0 = v)
            .Field(lsb: 24, getter: () => m_PauseJtag, setter: v => m_PauseJtag = v)
            .Field(0, 24,   getter: ComputeTimeRemaining)
            .OnWrite(v =>
            {
                // TRIGGER (bit 31) is a strobe — check raw written value after field setters ran.
                if ((v & (1u << 31)) != 0)
                    TriggerReset(force: true);
            });

        // LOAD (0x04) — reload the countdown timer.
        // Writing this register reloads the countdown from the new value and restarts
        // the countdown if ENABLE=1. This is how firmware "pets" the watchdog.
        // Bits 23:0 are used; bits 31:24 are reserved and ignored.
        // Spec: Table 1248.
        AddRegister(0x04, "LOAD")
            .OnRead(() => 0u)       // write-only in practice
            .OnWrite(v =>
            {
                m_LoadValueUs = v & 0x00FF_FFFFu; // 24-bit value, units = µs
                ReloadCountdown();
            });

        // REASON (0x08) — last reset cause. Read-only.
        // Both bits are 0 after a clean power-on reset.
        // A debugger warm reset (SYSRESETREQ / hartreset) also clears this register
        // so firmware under the debugger does not see a stale watchdog reason.
        // Spec: Table 1249.
        AddRegister(0x08, "REASON")
            .OnRead(() => (m_ReasonTimer ? 1u : 0u) | (m_ReasonForce ? 2u : 0u));

        // SCRATCH0–7 (0x0C–0x28) — 32-bit read/write scratch registers.
        // Preserved across watchdog resets; cleared by power-on reset (POR).
        // Spec: Table 1250.
        for (int i = 0; i < 8; i++)
        {
            int idx = i;
            AddRegister((uint)(0x0C + i * 4), $"SCRATCH{i}")
                .Field(0, 32, () => m_Scratch[idx], v => m_Scratch[idx] = v);
        }

        // TICK (0x2C) — tick generator control.
        //
        // The tick generator divides clk_ref (e.g. 12 MHz XOSC) to produce a 1 MHz
        // reference used by the watchdog and the system timers. The SDK sets CYCLES to
        // the crystal frequency in MHz (typically 12) and sets ENABLE=1.
        //
        // Bit 8     ENABLE:  enable the tick generator.
        // Bits 7:0  CYCLES:  clk_ref cycles per tick (9-bit field, but bits 8:0 overlap
        //                    with ENABLE; the datasheet shows CYCLES as bits 7:0).
        //                    The SDK sets this to clk_ref_MHz.
        // Bit 9     RUNNING: read-only; 1 = tick generator is running.
        //                    In the emulator, mirrors ENABLE (always running when enabled).
        // Bits 19:10 COUNT:  read-only; current counter value (0 to CYCLES-1).
        //                    Always 0 in the emulator (not physically modelled).
        //
        // Spec: §12.9.
        AddRegister(0x2C, "TICK")
            .Field(0, 8, () => m_TickCycles, v => m_TickCycles = v)         // CYCLES [7:0]
            .Field(lsb: 8, getter: () => m_TickEnable, setter: v => m_TickEnable = v) // ENABLE
            .Field(lsb: 9, getter: () => m_TickEnable);                     // RUNNING (mirrors ENABLE)
    }

    // ─── Countdown management ─────────────────────────────────────────────────

    /// <summary>
    /// Enables or disables the watchdog countdown.
    /// Enabling starts (or restarts) the countdown from m_LoadValueUs.
    /// Disabling cancels the pending timer.
    /// </summary>
    private void SetEnable(bool enable)
    {
        m_Enable = enable;
        if (enable)
            ReloadCountdown();
        else
            CancelTimer();
    }

    /// <summary>
    /// Reloads the countdown from m_LoadValueUs and schedules the timeout timer.
    /// Called when LOAD is written or ENABLE transitions to true.
    /// If ENABLE is not set, the countdown is reset but the timer is not started.
    /// </summary>
    private void ReloadCountdown()
    {
        m_CountdownStopwatch.Restart();
        CancelTimer();

        if (!m_Enable || m_LoadValueUs == 0)
            return;

        // Schedule a .NET timer to fire when the countdown expires.
        // The countdown is in µs; convert to ms, rounding up.
        int delayMs = Math.Max(1, (int)((m_LoadValueUs + 999u) / 1000u));
        m_WatchdogTimer = new System.Threading.Timer(
            _ => OnCountdownExpired(),
            null,
            delayMs,
            System.Threading.Timeout.Infinite);
    }

    /// <summary>Cancels the pending countdown timer without triggering a reset.</summary>
    private void CancelTimer()
    {
        m_WatchdogTimer?.Dispose();
        m_WatchdogTimer = null;
    }

    /// <summary>
    /// Returns the current countdown value (µs remaining), clamped to [0, 0xFFFFFF].
    /// Read by CTRL.TIME. Returns 0 when the countdown is disabled or has expired.
    /// </summary>
    private uint ComputeTimeRemaining()
    {
        if (!m_Enable || m_LoadValueUs == 0)
            return 0;

        long elapsed = m_CountdownStopwatch.ElapsedTicks * 1_000_000L / Stopwatch.Frequency;
        long remaining = m_LoadValueUs - elapsed;
        if (remaining <= 0) return 0;
        return (uint)Math.Min(remaining, 0xFF_FFFFu);
    }

    /// <summary>
    /// Called when the countdown reaches zero (timer fires).
    /// Triggers a watchdog reset with REASON.TIMER=1.
    /// </summary>
    private void OnCountdownExpired()
    {
        TriggerReset(force: false);
    }

    // ─── Reset logic ──────────────────────────────────────────────────────────

    /// <summary>
    /// Performs a watchdog reset. Sets the appropriate REASON bit, resets the
    /// processor PC to the boot entry point, and fires <see cref="WatchdogTriggered"/>.
    ///
    /// <paramref name="force"/> = true when triggered by CTRL.TRIGGER=1 (software);
    /// false when triggered by a countdown timeout.
    ///
    /// Spec: §12.9 — "When the watchdog is triggered, a system reset is generated."
    /// </summary>
    private void TriggerReset(bool force)
    {
        CancelTimer();
        m_Enable = false;

        if (force)
            m_ReasonForce = true;
        else
            m_ReasonTimer = true;

        // Reset the processor PC to the RISC-V boot entry point.
        // The bootrom checks REASON on startup to decide whether to call the watchdog
        // recovery path or perform a normal boot sequence.
        m_Processor.PC = 0x00007DFC; // riscv_entry_point

        WatchdogTriggered?.Invoke();
    }
}
