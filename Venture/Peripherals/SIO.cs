using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Venture.Peripherals;

public class SIO : PeripheralBase, IGpioSource
{
    private readonly GpioLine[] m_GpioLines = Enumerable.Range(0, 48).Select(x => new GpioLine()).ToArray();

    private readonly bool[] m_GpioValues = new bool[48];
    private readonly bool[] m_GpioEnable = new bool[48];

    // GPIO input sources — wired back by UserBankIO and IoQSPI after both peripherals are
    // constructed (they depend on SIO, so SIO is instantiated first).
    // Until set, GPIO_IN / GPIO_HI_IN return 0.
    private UserBankIO? m_GpioInputSource;
    private IoQSPI? m_QspiInputSource;

    // -------------------------------------------------------------------------
    // Inter-processor FIFOs
    // Spec: RP2350 Datasheet §3.1.5 — "Inter-processor FIFOs (Mailboxes)"
    //
    // Two physically separate FIFOs, each 32 bits wide and 4 entries deep:
    //   m_Core0ToCore1Fifo: written by core 0 (FIFO_WR), read by core 1.
    //   m_Core1ToCore0Fifo: written by core 1, read by core 0 (FIFO_RD).
    //
    // From core 0's perspective:
    //   FIFO_WR  → enqueues into m_Core0ToCore1Fifo (TX).
    //   FIFO_RD  → dequeues from m_Core1ToCore0Fifo (RX).
    //   FIFO_ST  → status flags computed from both FIFOs and sticky error bits.
    //
    // FIFO_ST bit layout (§3.1.5):
    //   bit 0  VLD — incoming (core1→core0) FIFO contains data.
    //   bit 1  RDY — outgoing (core0→core1) FIFO has room (not full).
    //   bit 2  ROE — incoming FIFO read while empty (sticky; write any to FIFO_ST to clear).
    //   bit 3  WOF — outgoing FIFO written while full (sticky; write any to FIFO_ST to clear).
    //
    // The FIFO IRQ (SIO_IRQ_FIFO, IRQ 25) is asserted when (VLD | ROE | WOF) != 0.
    // It clears automatically when all three bits are 0 again.
    //
    // Note: RP2350 has separate Secure/Non-secure FIFOs (§3.1.1). Only the Secure
    // FIFO is implemented here; the Non-secure SIO bank is a stub.
    // -------------------------------------------------------------------------

    /// <summary>Maximum number of entries in each FIFO. Spec: 4 entries deep (§3.1.5).</summary>
    private const int FifoDepth = 4;

    private readonly Queue<uint> m_Core0ToCore1Fifo = new();
    private readonly Queue<uint> m_Core1ToCore0Fifo = new();

    // Sticky error flags — cleared by writing any value to FIFO_ST.
    private bool m_FifoRoe; // RX overrun: core 0 read while empty
    private bool m_FifoWof; // TX overflow: core 0 wrote while full

    // -------------------------------------------------------------------------
    // Doorbells
    // Spec: RP2350 Datasheet §3.1.6 — "Doorbells"
    //
    // Each direction has 8 independent flags (one byte).
    //
    //   m_DoorbellIn  — bits set by the opposite core (or by DOORBELL_IN_SET self-ring).
    //                   Read via DOORBELL_IN_SET / DOORBELL_IN_CLR (both return same value).
    //                   Cleared by writing to DOORBELL_IN_CLR (write-1-to-clear).
    //                   Non-zero → SIO_IRQ_BELL (IRQ 26) asserted on this core.
    //
    //   m_DoorbellOut — bits set by this core via DOORBELL_OUT_SET.
    //                   Read via DOORBELL_OUT_SET / DOORBELL_OUT_CLR (both return same value).
    //                   Cleared by writing to DOORBELL_OUT_CLR (write-1-to-clear).
    //                   These bits would raise SIO_IRQ_BELL on core 1 (not implemented yet).
    //
    // Write semantics differ from the atomic-alias pattern (the aliases XOR/OR/AND the
    // *existing* value, whereas SET/CLR registers have their own defined write action).
    // -------------------------------------------------------------------------
    private byte m_DoorbellIn;
    private byte m_DoorbellOut;

    private readonly IrqController m_IrqController;

    // -------------------------------------------------------------------------
    // RISC-V Platform Timer (MTIME / MTIMECMP)
    // Spec: RP2350 Datasheet §3.1.8 — "RISC-V Platform Timer"
    //
    // MTIME is a free-running 64-bit counter driven at 1 MHz (clk_tick).
    // The emulator uses wall-clock Stopwatch time in microseconds as the source.
    //
    // MTIMECMP is a 64-bit compare value. When MTIME >= MTIMECMP, MIP.MTIP (bit 7)
    // is set, triggering a machine-timer interrupt if MIE.MTIE is also set.
    //
    // Atomic write sequence:
    //   The RISC-V Privileged ISA recommends writing MTIMECMP_HIGH = 0xFFFFFFFF first,
    //   then MTIMECMP_LOW, then MTIMECMP_HIGH again to prevent a spurious interrupt
    //   during the two-write sequence. We handle this by re-evaluating the IRQ on every
    //   write — the interim state may produce a spurious MTIP pulse, but since MIE.MTIE
    //   is usually disabled while reconfiguring the timer this is safe in practice.
    //
    // MTIME_CTRL (0x1a4): bit 0 = EN (enable counting); the emulator always counts.
    // -------------------------------------------------------------------------
    private readonly Stopwatch m_MtimeStopwatch = Stopwatch.StartNew();
    // Separate offset for MTIME if software writes to it (rarely done in practice).
    private long m_MtimeOffsetMicros;
    // Current MTIMECMP value (64-bit).
    private uint m_MtimeCmpLo = 0xFFFF_FFFF;
    private uint m_MtimeCmpHi = 0xFFFF_FFFF;
    // Background timer used to fire MIP.MTIP when MTIME reaches MTIMECMP.
    private System.Threading.Timer? m_MtimeTimer;

    /// <summary>Current MTIME value in microseconds (wall-clock + offset).</summary>
    private long GetMtimeMicros() =>
        m_MtimeStopwatch.ElapsedTicks * 1_000_000L / Stopwatch.Frequency + m_MtimeOffsetMicros;

    /// <summary>
    /// Re-evaluates MIP.MTIP when MTIMECMP changes. Clears any existing background
    /// timer and either sets MTIP immediately (if MTIME >= MTIMECMP) or schedules a
    /// timer to set it when the condition becomes true.
    /// </summary>
    private void UpdateMtimeCmpIrq()
    {
        m_MtimeTimer?.Dispose();
        m_MtimeTimer = null;

        ulong mtime    = (ulong)GetMtimeMicros();
        ulong mtimecmp = ((ulong)m_MtimeCmpHi << 32) | m_MtimeCmpLo;

        if (mtime >= mtimecmp)
        {
            m_IrqController.SetTimerInterrupt(true);
        }
        else
        {
            // Clear MTIP — the condition is no longer satisfied.
            m_IrqController.SetTimerInterrupt(false);
            ulong delayUs = mtimecmp - mtime;
            // Cap at ~49 days (uint.MaxValue µs) to stay within int range for the timer.
            int delayMs = delayUs > 2_000_000_000UL
                ? int.MaxValue
                : Math.Max(1, (int)((delayUs + 999UL) / 1000UL));
            m_MtimeTimer = new System.Threading.Timer(
                _ => m_IrqController.SetTimerInterrupt(true),
                null,
                delayMs,
                System.Threading.Timeout.Infinite);
        }
    }

    /// <summary>
    /// Called by <see cref="UserBankIO"/> at the end of its constructor to complete the
    /// circular SIO ↔ UserBankIO wiring. Once set, GPIO_IN reads reflect live pad values.
    /// </summary>
    public void SetGpioInputSource(UserBankIO source) => m_GpioInputSource = source;

    /// <summary>
    /// Called by <see cref="IoQSPI"/> at the end of its constructor to complete the
    /// circular SIO ↔ IoQSPI wiring. Once set, GPIO_HI_IN bits [31:26] reflect live
    /// QSPI pin states (SCLK=26, SS_N=27, SD0=28, SD1=29, SD2=30, SD3=31).
    /// </summary>
    public void SetQspiInputSource(IoQSPI source) => m_QspiInputSource = source;

    public IGpioLine GetGpioLine(int index)
    {
        return m_GpioLines[index];
    }

    public SIO(uint baseAddress, string name, ILogger<SIO> logger, IrqController irqController)
        : base(baseAddress, name, logger)
    {
        m_IrqController = irqController;

        AddRegister(0x000, "CPUID", 0);

        // GPIO_IN (0x004) — reflects current input values for GPIO 0–31.
        // GPIO_HI_IN (0x008) — reflects GPIO 32–47 in bits [15:0] and the 6 QSPI
        //   pins in bits [31:26]: SCLK=26, SS_N=27, SD0=28, SD1=29, SD2=30, SD3=31.
        // Both read from UserBankIO and IoQSPI (the pad layers) so that firmware polling works.
        // Spec: RP2350 Datasheet §3.1.4 (SIO GPIO input registers).
        AddRegister(0x004, "GPIO_IN").OnRead(ComputeGpioIn);
        AddRegister(0x008, "GPIO_HI_IN").OnRead(ComputeGpioHiIn);

        GpioOutFields(AddRegister(0x010, "GPIO_OUT"), (i, value) => value);
        GpioHiOutFields(AddRegister(0x014, "GPIO_HI_OUT"), (i, value) => value);
        GpioOutFields(AddRegister(0x018, "GPIO_OUT_SET").OnRead(() => 0), (i, value) => value ? true : m_GpioValues[i]);
        GpioHiOutFields(AddRegister(0x01c, "GPIO_HI_OUT_SET").OnRead(() => 0), (i, value) => value ? true : m_GpioValues[i]);
        GpioOutFields(AddRegister(0x020, "GPIO_OUT_CLR").OnRead(() => 0), (i, value) => value ? false : m_GpioValues[i]);
        GpioHiOutFields(AddRegister(0x024, "GPIO_HI_OUT_CLR").OnRead(() => 0), (i, value) => value ? false : m_GpioValues[i]);
        GpioOutFields(AddRegister(0x028, "GPIO_OUT_XOR").OnRead(() => 0), (i, value) => m_GpioValues[i] ^ value);
        GpioHiOutFields(AddRegister(0x02c, "GPIO_HI_OUT_XOR").OnRead(() => 0), (i, value) => m_GpioValues[i] ^ value);

        GpioOeFields(AddRegister(0x030, "GPIO_OE"), (i, value) => value);
        GpioHiOeFields(AddRegister(0x034, "GPIO_HI_OE"), (i, value) => value);
        GpioOeFields(AddRegister(0x038, "GPIO_OE_SET").OnRead(() => 0), (i, value) => value ? true : m_GpioEnable[i]);
        GpioHiOeFields(AddRegister(0x03c, "GPIO_HI_OE_SET").OnRead(() => 0), (i, value) => value ? true : m_GpioEnable[i]);
        GpioOeFields(AddRegister(0x040, "GPIO_OE_CLR").OnRead(() => 0), (i, value) => value ? false : m_GpioEnable[i]);
        GpioHiOeFields(AddRegister(0x044, "GPIO_HI_OE_CLR").OnRead(() => 0), (i, value) => value ? false : m_GpioEnable[i]);
        GpioOeFields(AddRegister(0x048, "GPIO_OE_XOR").OnRead(() => 0), (i, value) => m_GpioEnable[i] ^ value);
        GpioHiOeFields(AddRegister(0x04c, "GPIO_HI_OE_XOR").OnRead(() => 0), (i, value) => m_GpioEnable[i] ^ value);

        // -------------------------------------------------------------------------
        // FIFO registers (0x050–0x058)
        // -------------------------------------------------------------------------

        // FIFO_ST — computed status; writing any value clears the ROE and WOF sticky bits.
        AddRegister(0x050, "FIFO_ST")
            .OnRead(ReadFifoSt)
            .OnWrite(_ =>
            {
                // Spec §3.1.5: "To clear the ROE and WOF flags, write any value to FIFO_ST."
                m_FifoRoe = false;
                m_FifoWof = false;
                UpdateFifoIrq();
            });

        // FIFO_WR — write-only from core 0's perspective; enqueues to the core0→core1 FIFO.
        // Reading is architecturally undefined; return 0.
        AddRegister(0x054, "FIFO_WR")
            .OnRead(() => 0)
            .OnWrite(WriteFifoWr);

        // FIFO_RD — read-only from core 0's perspective; dequeues from the core1→core0 FIFO.
        // Writing is architecturally undefined; ignored.
        AddRegister(0x058, "FIFO_RD")
            .OnRead(ReadFifoRd);

        AddRegister(0x05c, "SPINLOCK_ST");
        AddRegister(0x080, "INTERP0_ACCUM0");
        AddRegister(0x084, "INTERP0_ACCUM1");
        AddRegister(0x088, "INTERP0_BASE0");
        AddRegister(0x08c, "INTERP0_BASE1");
        AddRegister(0x090, "INTERP0_BASE2");
        AddRegister(0x094, "INTERP0_POP_LANE0");
        AddRegister(0x098, "INTERP0_POP_LANE1");
        AddRegister(0x09c, "INTERP0_POP_FULL");
        AddRegister(0x0a0, "INTERP0_PEEK_LANE0");
        AddRegister(0x0a4, "INTERP0_PEEK_LANE1");
        AddRegister(0x0a8, "INTERP0_PEEK_FULL");
        AddRegister(0x0ac, "INTERP0_CTRL_LANE0");
        AddRegister(0x0b0, "INTERP0_CTRL_LANE1");
        AddRegister(0x0b4, "INTERP0_ACCUM0_ADD");
        AddRegister(0x0b8, "INTERP0_ACCUM1_ADD");
        AddRegister(0x0bc, "INTERP0_BASE_1AND0");
        AddRegister(0x0c0, "INTERP1_ACCUM0");
        AddRegister(0x0c4, "INTERP1_ACCUM1");
        AddRegister(0x0c8, "INTERP1_BASE0");
        AddRegister(0x0cc, "INTERP1_BASE1");
        AddRegister(0x0d0, "INTERP1_BASE2");
        AddRegister(0x0d4, "INTERP1_POP_LANE0");
        AddRegister(0x0d8, "INTERP1_POP_LANE1");
        AddRegister(0x0dc, "INTERP1_POP_FULL");
        AddRegister(0x0e0, "INTERP1_PEEK_LANE0");
        AddRegister(0x0e4, "INTERP1_PEEK_LANE1");
        AddRegister(0x0e8, "INTERP1_PEEK_FULL");
        AddRegister(0x0ec, "INTERP1_CTRL_LANE0");
        AddRegister(0x0f0, "INTERP1_CTRL_LANE1");
        AddRegister(0x0f4, "INTERP1_ACCUM0_ADD");
        AddRegister(0x0f8, "INTERP1_ACCUM1_ADD");
        AddRegister(0x0fc, "INTERP1_BASE_1AND0");

        foreach (uint i in Enumerable.Range(0, 32))
        {
            AddRegister(0x100 + (i * 4), $"SPINLOCK{i}");
        }

        // -------------------------------------------------------------------------
        // Doorbell registers (0x180–0x18c)
        // Spec: §3.1.6 — "Doorbells"
        //
        // OUT registers refer to doorbell flags that core 0 posts *to* core 1.
        // IN  registers refer to doorbell flags that have been posted *to* core 0.
        //
        // Both OUT_SET and OUT_CLR read the same value (current m_DoorbellOut state).
        // Both IN_SET  and IN_CLR  read the same value (current m_DoorbellIn state).
        //
        // Write semantics:
        //   DOORBELL_OUT_SET: write ORs bits into m_DoorbellOut  (sets flags on core 1).
        //   DOORBELL_OUT_CLR: write clears bits from m_DoorbellOut.
        //   DOORBELL_IN_SET:  write ORs bits into m_DoorbellIn   (self-ring; sets IRQ on this core).
        //   DOORBELL_IN_CLR:  write clears bits from m_DoorbellIn; lowers IRQ when all clear.
        // -------------------------------------------------------------------------

        AddRegister(0x180, "DOORBELL_OUT_SET")
            .OnRead(() => m_DoorbellOut)
            .OnWrite(v =>
            {
                m_DoorbellOut |= (byte)(v & 0xFF);
                // TODO (Phase 9): notify core 1 by raising SIO_IRQ_BELL on it.
            });

        AddRegister(0x184, "DOORBELL_OUT_CLR")
            .OnRead(() => m_DoorbellOut)
            .OnWrite(v => { m_DoorbellOut &= (byte)(~v & 0xFF); });

        AddRegister(0x188, "DOORBELL_IN_SET")
            .OnRead(() => m_DoorbellIn)
            .OnWrite(v =>
            {
                m_DoorbellIn |= (byte)(v & 0xFF);
                UpdateDoorbellIrq();
            });

        AddRegister(0x18c, "DOORBELL_IN_CLR")
            .OnRead(() => m_DoorbellIn)
            .OnWrite(v =>
            {
                m_DoorbellIn &= (byte)(~v & 0xFF);
                UpdateDoorbellIrq();
            });

        AddRegister(0x190, "PERI_NONSEC");
        AddRegister(0x1a0, "RISCV_SOFTIRQ");

        // MTIME_CTRL (0x1a4) — controls which clock source drives MTIME.
        // Bit 0 EN:             enable MTIME counting (emulator always counts).
        // Bit 1 FULLSPEED:      run at clk_sys instead of clk_tick.
        // Bit 2 DBGPAUSE_CORE0: pause MTIME when core 0 is in debug mode.
        // Bit 3 DBGPAUSE_CORE1: pause MTIME when core 1 is in debug mode.
        // Stored for readback; the emulator uses wall-clock time regardless.
        // Spec: §3.1.8.
        AddRegister(0x1a4, "MTIME_CTRL");

        // MTIME (0x1b0) / MTIMEH (0x1b4) — 64-bit free-running µs counter.
        // Writing is architecturally permitted but rarely used; we apply an offset
        // so subsequent reads reflect the written value.
        // Spec: §3.1.8 — "a 64-bit read/write register that increments at 1 MHz".
        AddRegister(0x1b0, "MTIME")
            .OnRead(() => (uint)(GetMtimeMicros() & 0xFFFF_FFFF))
            .OnWrite(v =>
            {
                // Replace the low 32 bits; keep the high 32 bits unchanged.
                ulong current = (ulong)GetMtimeMicros();
                ulong target  = (current & 0xFFFF_FFFF_0000_0000UL) | v;
                m_MtimeOffsetMicros += (long)target - (long)current;
                UpdateMtimeCmpIrq();
            });

        AddRegister(0x1b4, "MTIMEH")
            .OnRead(() => (uint)((ulong)GetMtimeMicros() >> 32))
            .OnWrite(v =>
            {
                // Replace the high 32 bits; keep the low 32 bits unchanged.
                ulong current = (ulong)GetMtimeMicros();
                ulong target  = ((ulong)v << 32) | (current & 0xFFFF_FFFFUL);
                m_MtimeOffsetMicros += (long)target - (long)current;
                UpdateMtimeCmpIrq();
            });

        // MTIMECMP (0x1b8) / MTIMECMPH (0x1bc) — 64-bit compare register.
        // MIP.MTIP is set when MTIME >= MTIMECMP. MTIP is cleared by writing a
        // new MTIMECMP value that places the threshold in the future.
        // Default 0xFFFFFFFF_FFFFFFFF (max) so MTIP is not asserted at boot.
        // Spec: §3.1.8; RISC-V Privileged ISA §3.1.9.
        AddRegister(0x1b8, "MTIMECMP")
            .OnRead(() => m_MtimeCmpLo)
            .OnWrite(v => { m_MtimeCmpLo = v; UpdateMtimeCmpIrq(); });

        AddRegister(0x1bc, "MTIMECMPH")
            .OnRead(() => m_MtimeCmpHi)
            .OnWrite(v => { m_MtimeCmpHi = v; UpdateMtimeCmpIrq(); });
        AddRegister(0x1c0, "TMDS_CTRL");
        AddRegister(0x1c4, "TMDS_WDATA");
        AddRegister(0x1c8, "TMDS_PEEK_SINGLE");
        AddRegister(0x1cc, "TMDS_POP_SINGLE");
        AddRegister(0x1d0, "TMDS_PEEK_DOUBLE_L0");
        AddRegister(0x1d4, "TMDS_POP_DOUBLE_L0");
        AddRegister(0x1d8, "TMDS_PEEK_DOUBLE_L1");
        AddRegister(0x1dc, "TMDS_POP_DOUBLE_L1");
        AddRegister(0x1e0, "TMDS_PEEK_DOUBLE_L2");
        AddRegister(0x1e4, "TMDS_POP_DOUBLE_L2");
    }

    // -------------------------------------------------------------------------
    // FIFO implementation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads FIFO_ST: VLD (bit 0), RDY (bit 1), ROE (bit 2), WOF (bit 3).
    /// VLD and RDY are combinational (reflect current FIFO levels).
    /// ROE and WOF are sticky and cleared by writing any value to FIFO_ST.
    /// </summary>
    private uint ReadFifoSt()
    {
        uint st = 0;
        if (m_Core1ToCore0Fifo.Count > 0) st |= (1u << 0); // VLD
        if (m_Core0ToCore1Fifo.Count < FifoDepth) st |= (1u << 1); // RDY
        if (m_FifoRoe) st |= (1u << 2); // ROE
        if (m_FifoWof) st |= (1u << 3); // WOF
        return st;
    }

    /// <summary>
    /// Handles FIFO_WR: core 0 enqueues a word into the core0→core1 FIFO.
    /// If the FIFO is already full, the write is dropped and WOF is set (sticky).
    /// Spec §3.1.5: "Writing to the outgoing FIFO while full does not affect the FIFO state."
    /// </summary>
    private void WriteFifoWr(uint value)
    {
        if (m_Core0ToCore1Fifo.Count >= FifoDepth)
        {
            // TX overflow: data is lost, sticky flag set.
            m_FifoWof = true;
        }
        else
        {
            m_Core0ToCore1Fifo.Enqueue(value);
        }
        UpdateFifoIrq();
    }

    /// <summary>
    /// Handles FIFO_RD: core 0 dequeues a word from the core1→core0 FIFO.
    /// If the FIFO is empty, the read returns 0 and ROE is set (sticky).
    /// Spec §3.1.5: "Reading from the incoming FIFO while empty does not affect the FIFO state."
    /// </summary>
    private uint ReadFifoRd()
    {
        if (m_Core1ToCore0Fifo.Count == 0)
        {
            // RX underrun: no data, sticky flag set.
            m_FifoRoe = true;
            UpdateFifoIrq();
            return 0;
        }
        var value = m_Core1ToCore0Fifo.Dequeue();
        UpdateFifoIrq();
        return value;
    }

    /// <summary>
    /// Evaluates the SIO_IRQ_FIFO condition and raises or clears IRQ 25 accordingly.
    /// The IRQ is asserted when any of VLD, ROE, or WOF is set.
    /// Spec §3.1.5: "Each IRQ output is the logical OR of the VLD, ROE and WOF bits."
    /// </summary>
    private void UpdateFifoIrq()
    {
        bool pending = m_Core1ToCore0Fifo.Count > 0 || m_FifoRoe || m_FifoWof;
        if (pending)
            m_IrqController.RaiseIrq(IrqController.SIO_IRQ_FIFO);
        else
            m_IrqController.ClearIrq(IrqController.SIO_IRQ_FIFO);
    }

    /// <summary>
    /// Allows external code (e.g. a simulated core 1) to push a word into core 0's
    /// incoming FIFO, which makes FIFO_RD readable and asserts SIO_IRQ_FIFO on core 0.
    /// </summary>
    public void PushToCore0Fifo(uint value)
    {
        if (m_Core1ToCore0Fifo.Count < FifoDepth)
            m_Core1ToCore0Fifo.Enqueue(value);
        UpdateFifoIrq();
    }

    /// <summary>
    /// Allows simulated core 1 to read the next word that core 0 wrote to FIFO_WR.
    /// Returns null if the core0→core1 FIFO is empty.
    /// </summary>
    public uint? PopFromCore1Fifo()
    {
        if (m_Core0ToCore1Fifo.Count == 0) return null;
        var value = m_Core0ToCore1Fifo.Dequeue();
        UpdateFifoIrq(); // RDY may have changed
        return value;
    }

    // -------------------------------------------------------------------------
    // Doorbell implementation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Raises or clears SIO_IRQ_BELL (IRQ 26) based on whether any incoming
    /// doorbell bits are set. Spec §3.1.6.
    /// </summary>
    private void UpdateDoorbellIrq()
    {
        if (m_DoorbellIn != 0)
            m_IrqController.RaiseIrq(IrqController.SIO_IRQ_BELL);
        else
            m_IrqController.ClearIrq(IrqController.SIO_IRQ_BELL);
    }

    /// <summary>
    /// Allows simulated core 1 (or boot sequence code) to post doorbell flags to
    /// core 0, which raises SIO_IRQ_BELL on core 0.
    /// </summary>
    public void PostDoorbellToCore0(byte flags)
    {
        m_DoorbellIn |= flags;
        UpdateDoorbellIrq();
    }

    // -------------------------------------------------------------------------
    // GPIO implementation (unchanged)
    // -------------------------------------------------------------------------

    private void GpioOutFields(Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        foreach (var i in Enumerable.Range(0, 32))
        {
            GpioOutField(i, i, reg, computeNewValue);
        }
    }

    private void GpioHiOutFields(Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        foreach (var i in Enumerable.Range(32, 16))
        {
            GpioOutField(i - 32, i, reg, computeNewValue);
        }
    }

    private void GpioOutField(int field, int i, Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        reg.Field(field, () => m_GpioValues[i], value =>
        {
            m_GpioValues[i] = computeNewValue(i, value);
            if (m_GpioEnable[i])
            {
                m_GpioLines[i].Value = m_GpioValues[i] ? GpioValue.High : GpioValue.Low;
            }
        });
    }

    private void GpioOeFields(Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        foreach (var i in Enumerable.Range(0, 32))
        {
            GpioOeField(i, i, reg, computeNewValue);
        }
    }

    private void GpioHiOeFields(Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        foreach (var i in Enumerable.Range(32, 16))
        {
            GpioOeField(i - 32, i, reg, computeNewValue);
        }
    }

    private void GpioOeField(int field, int i, Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        reg.Field(field, () => m_GpioEnable[i], value =>
        {
            m_GpioEnable[i] = computeNewValue(i, value);
            m_GpioLines[i].Value = m_GpioEnable[i] ? (m_GpioValues[i] ? GpioValue.High : GpioValue.Low) : GpioValue.HiZ;
        });
    }

    /// <summary>
    /// Computes the GPIO_IN register value (GPIO 0–31).
    /// Returns 0 if the input source (UserBankIO) has not been connected yet.
    /// Each bit reflects the pad input after INOVER overrides have been applied.
    /// </summary>
    private uint ComputeGpioIn()
    {
        // Input source is set by UserBankIO after both peripherals are constructed.
        // During early DI resolution this may still be null; return 0 in that case.
        if (m_GpioInputSource == null) return 0;

        uint result = 0;
        for (int i = 0; i < 32; i++)
        {
            if (m_GpioInputSource.ReadInputValue(i) == GpioValue.High)
                result |= 1u << i;
        }
        return result;
    }

    /// <summary>
    /// Computes the GPIO_HI_IN register value.
    /// Bits [15:0]  — GPIO 32–47 input values (from UserBankIO).
    /// Bits [31:26] — QSPI pin input values (from IoQSPI):
    ///                SCLK=26, SS_N=27, SD0=28, SD1=29, SD2=30, SD3=31.
    /// Returns 0 for any source that has not been connected yet.
    /// Spec: RP2350 Datasheet §3.1.4.
    /// </summary>
    private uint ComputeGpioHiIn()
    {
        uint result = 0;

        // GPIO 32-47 → bits [15:0]
        if (m_GpioInputSource != null)
        {
            for (int i = 0; i < 16; i++)
            {
                if (m_GpioInputSource.ReadInputValue(32 + i) == GpioValue.High)
                    result |= 1u << i;
            }
        }

        // QSPI bank → bits [31:26]
        if (m_QspiInputSource != null)
        {
            for (int i = 0; i < 6; i++)
            {
                if (m_QspiInputSource.ReadInputValue(i) == GpioValue.High)
                    result |= 1u << (26 + i);
            }
        }

        return result;
    }
}
