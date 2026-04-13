using System.Numerics;
using Venture.Processor;

namespace Venture;

/// <summary>
/// System-level IRQ aggregator for the RP2350.
///
/// The RP2350 presents 52 IRQ lines (0–51) to the interrupt controller of each core.
/// Peripherals call <see cref="RaiseIrq"/>/<see cref="ClearIrq"/> to assert or deassert
/// their individual IRQ line. Whenever any line is pending, <c>MIP.MEIP</c> (bit 11) is
/// set on the Hazard3 core so the processor enters the external-interrupt trap.
///
/// The actual per-IRQ enable / priority logic lives in Hazard3-custom CSRs (MEIEA, MEIPA,
/// MEIPRA — see §3.8.9). Those CSRs read the pending mask via <see cref="GetPending"/>.
/// For now (Phase 2) all lines are treated as enabled; per-IRQ masking is added in Phase 10.
///
/// Core-local IRQs (same number on each core, different physical source):
///   SIO_IRQ_FIFO (25), SIO_IRQ_BELL (26), SIO_IRQ_FIFO_NS (27), SIO_IRQ_BELL_NS (28),
///   SIO_IRQ_MTIMECMP (29), IO_IRQ_BANK0 (21), IO_IRQ_BANK0_NS (22),
///   IO_IRQ_QSPI (23), IO_IRQ_QSPI_NS (24).
/// Non-core-local IRQs should be enabled in only one core's interrupt controller at a time.
///
/// Spec: RP2350 Datasheet §3.2 — "Interrupts", Table 94.
/// </summary>
public class IrqController
{
    // ---------------------------------------------------------------------------
    // IRQ line numbers — RP2350 Datasheet Table 94 (§3.2)
    // ---------------------------------------------------------------------------

    /// <summary>IRQ 0 — Timer 0 alarm 0.</summary>
    public const int TIMER0_IRQ_0     =  0;
    /// <summary>IRQ 1 — Timer 0 alarm 1.</summary>
    public const int TIMER0_IRQ_1     =  1;
    /// <summary>IRQ 2 — Timer 0 alarm 2.</summary>
    public const int TIMER0_IRQ_2     =  2;
    /// <summary>IRQ 3 — Timer 0 alarm 3.</summary>
    public const int TIMER0_IRQ_3     =  3;
    /// <summary>IRQ 4 — Timer 1 alarm 0.</summary>
    public const int TIMER1_IRQ_0     =  4;
    /// <summary>IRQ 5 — Timer 1 alarm 1.</summary>
    public const int TIMER1_IRQ_1     =  5;
    /// <summary>IRQ 6 — Timer 1 alarm 2.</summary>
    public const int TIMER1_IRQ_2     =  6;
    /// <summary>IRQ 7 — Timer 1 alarm 3.</summary>
    public const int TIMER1_IRQ_3     =  7;
    /// <summary>IRQ 8 — PWM slice 0 wrap.</summary>
    public const int PWM_IRQ_WRAP_0   =  8;
    /// <summary>IRQ 9 — PWM slice 1 wrap.</summary>
    public const int PWM_IRQ_WRAP_1   =  9;
    /// <summary>IRQ 10 — DMA channel 0.</summary>
    public const int DMA_IRQ_0        = 10;
    /// <summary>IRQ 11 — DMA channel 1.</summary>
    public const int DMA_IRQ_1        = 11;
    /// <summary>IRQ 12 — DMA channel 2.</summary>
    public const int DMA_IRQ_2        = 12;
    /// <summary>IRQ 13 — DMA channel 3.</summary>
    public const int DMA_IRQ_3        = 13;
    /// <summary>IRQ 14 — USB controller.</summary>
    public const int USBCTRL_IRQ      = 14;
    /// <summary>IRQ 15 — PIO0 state machine 0/1 interrupt 0.</summary>
    public const int PIO0_IRQ_0       = 15;
    /// <summary>IRQ 16 — PIO0 state machine 2/3 interrupt 1.</summary>
    public const int PIO0_IRQ_1       = 16;
    /// <summary>IRQ 17 — PIO1 state machine 0/1 interrupt 0.</summary>
    public const int PIO1_IRQ_0       = 17;
    /// <summary>IRQ 18 — PIO1 state machine 2/3 interrupt 1.</summary>
    public const int PIO1_IRQ_1       = 18;
    /// <summary>IRQ 19 — PIO2 state machine 0/1 interrupt 0.</summary>
    public const int PIO2_IRQ_0       = 19;
    /// <summary>IRQ 20 — PIO2 state machine 2/3 interrupt 1.</summary>
    public const int PIO2_IRQ_1       = 20;
    /// <summary>
    /// IRQ 21 — GPIO Bank 0 (Secure). Core-local: each core has its own enable
    /// registers (PROC0_INTE/INTF) feeding this IRQ line on that core.
    /// Spec: §9.5.
    /// </summary>
    public const int IO_IRQ_BANK0     = 21;
    /// <summary>IRQ 22 — GPIO Bank 0 (Non-secure). Core-local.</summary>
    public const int IO_IRQ_BANK0_NS  = 22;
    /// <summary>IRQ 23 — QSPI GPIO (Secure).</summary>
    public const int IO_IRQ_QSPI      = 23;
    /// <summary>IRQ 24 — QSPI GPIO (Non-secure).</summary>
    public const int IO_IRQ_QSPI_NS   = 24;
    /// <summary>
    /// IRQ 25 — SIO inter-processor FIFO (Secure). Core-local: asserted on a core
    /// when that core's incoming FIFO has data (VLD) or a sticky error (ROE/WOF).
    /// Spec: §3.1.5.
    /// </summary>
    public const int SIO_IRQ_FIFO     = 25;
    /// <summary>
    /// IRQ 26 — SIO doorbell (Secure). Core-local: asserted when the core's
    /// incoming doorbell register is non-zero. Spec: §3.1.6.
    /// </summary>
    public const int SIO_IRQ_BELL     = 26;
    /// <summary>IRQ 27 — SIO FIFO (Non-secure). Core-local.</summary>
    public const int SIO_IRQ_FIFO_NS  = 27;
    /// <summary>IRQ 28 — SIO doorbell (Non-secure). Core-local.</summary>
    public const int SIO_IRQ_BELL_NS  = 28;
    /// <summary>
    /// IRQ 29 — RISC-V platform timer compare. Core-local: asserted when
    /// MTIME >= MTIMECMP. Spec: §3.1.8.
    /// </summary>
    public const int SIO_IRQ_MTIMECMP = 29;
    /// <summary>IRQ 30 — Clocks subsystem.</summary>
    public const int CLOCKS_IRQ       = 30;
    /// <summary>IRQ 31 — SPI0.</summary>
    public const int SPI0_IRQ         = 31;
    /// <summary>IRQ 32 — SPI1.</summary>
    public const int SPI1_IRQ         = 32;
    /// <summary>IRQ 33 — UART0.</summary>
    public const int UART0_IRQ        = 33;
    /// <summary>IRQ 34 — UART1.</summary>
    public const int UART1_IRQ        = 34;
    /// <summary>IRQ 35 — ADC FIFO.</summary>
    public const int ADC_IRQ_FIFO     = 35;
    /// <summary>IRQ 36 — I2C0.</summary>
    public const int I2C0_IRQ         = 36;
    /// <summary>IRQ 37 — I2C1.</summary>
    public const int I2C1_IRQ         = 37;
    /// <summary>IRQ 38 — OTP programming done / error.</summary>
    public const int OTP_IRQ          = 38;
    /// <summary>IRQ 39 — True Random Number Generator.</summary>
    public const int TRNG_IRQ         = 39;
    /// <summary>IRQ 40 — Cortex-M33 cross-trigger (core 0).</summary>
    public const int PROC0_IRQ_CTI    = 40;
    /// <summary>IRQ 41 — Cortex-M33 cross-trigger (core 1).</summary>
    public const int PROC1_IRQ_CTI    = 41;
    /// <summary>IRQ 42 — System PLL lock / unlock.</summary>
    public const int PLL_SYS_IRQ      = 42;
    /// <summary>IRQ 43 — USB PLL lock / unlock.</summary>
    public const int PLL_USB_IRQ      = 43;
    /// <summary>IRQ 44 — Power manager power-state change.</summary>
    public const int POWMAN_IRQ_POW   = 44;
    /// <summary>IRQ 45 — Power manager always-on timer.</summary>
    public const int POWMAN_IRQ_TIMER = 45;
    // IRQs 46–51: SPAREIRQ_IRQ_0..5 — hardwired to 0 from peripheral sources.
    // Software may self-interrupt via these lines (e.g. for deferred "bottom half"
    // processing) using the Hazard3 MEIFA CSR or Arm NVIC_ISPR0 register.
    // Not modelled here; they are only triggered by explicit software action.

    // ---------------------------------------------------------------------------
    // State
    // ---------------------------------------------------------------------------

    // 64-bit pending mask: bit N is set when IRQ line N is asserted by its source.
    // Only bits 0–51 correspond to real IRQ lines; bits 52–63 are always 0.
    private ulong m_Pending;
    private readonly Hazard3Processor m_Processor;

    // -------------------------------------------------------------------------
    // Xh3irq — Hazard3 external interrupt controller CSRs
    // Spec: RP2350 Datasheet §3.8.6.1 — "Xh3irq: Hazard3 interrupt controller"
    //
    // MEIEA (0xBE0) — External interrupt enable array.
    //   Each bit gates whether the corresponding IRQ signal can reach the core.
    //   Accessed via a windowed interface: the lower 5 bits of the write data select
    //   which 16-IRQ window is exposed/updated in bits [31:16] of the same instruction.
    //   e.g. csrs 0xbe0, (window | (bit << 16)) enables IRQ (window*16 + bit).
    //
    // MEIPA (0xBE1) — External interrupt pending array (read-only mirror of m_Pending).
    //   Same windowed interface as MEIEA; bits reflect whether the peripheral's IRQ
    //   line is currently asserted, regardless of enable state.
    //
    // MEINEXT (0xBE4) — Next IRQ to service.
    //   Returns IRQ_number << 2 for the lowest-numbered IRQ that is both pending
    //   (m_Pending bit set), enabled (m_MeieaEnables bit set), and has priority
    //   >= MEICONTEXT.PPREEMPT (to avoid re-entering in-progress handlers).
    //   Bit 31 (NOIRQ) is set when no such IRQ exists.
    //   Writing bit 0 (UPDATE) atomically updates MEICONTEXT (NOIRQ, IRQ, PREEMPT).
    //
    // MEICONTEXT (0xBE5) — External interrupt context register.
    //   Manages the three-level preemption priority stack (PPPREEMPT/PPREEMPT/PREEMPT),
    //   current IRQ/NOIRQ tracking, and MRETEIRQ/CLEARTS/MTIESAVE/MSIESAVE.
    // -------------------------------------------------------------------------

    // 64-bit enable mask: bit N is set when IRQ N is enabled via MEIEA.
    private ulong m_MeieaEnables;

    // Currently selected 16-IRQ window for MEIEA/MEIPA reads (bits [4:0]).
    // Set by any write to the MEIEA or MEIPA CSR addresses.
    private int m_MeiaWindow;

    public IrqController(Hazard3Processor processor)
    {
        m_Processor = processor;

        processor.CSR.AddGetter(0xBE0, ReadMeiea);
        processor.CSR.AddSetter(0xBE0, WriteMeiea);
        processor.CSR.AddGetter(0xBE1, ReadMeipa);
        // MEIPA is read-only; writes update the window selector but not the pending bits.
        processor.CSR.AddSetter(0xBE1, v => m_MeiaWindow = (int)(v & 0x1F));
        processor.CSR.AddGetter(0xBE4, ReadMeinext);
        processor.CSR.AddSetter(0xBE4, WriteMeinext);
        processor.CSR.AddGetter(CSR.MEICONTEXT, ReadMeicontext);
        processor.CSR.AddSetter(CSR.MEICONTEXT, WriteMeicontext);
    }

    // -------------------------------------------------------------------------
    // MEIEA / MEIPA windowed CSR read helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads the MEIEA CSR.
    /// Bits [31:16] contain the 16-bit enable window for <see cref="m_MeiaWindow"/>.
    /// Bits [4:0] contain the current window index.
    ///
    /// Note on windowed-CSR read ordering: the Hazard3 spec says "the window is indexed
    /// by the LSBs of the write data for the same CSR instruction". In practice the
    /// pico-sdk always writes the correct window index before (or in the same instruction
    /// as) a read, so returning the currently-stored window index here is correct for all
    /// SDK-generated access patterns.
    /// </summary>
    private uint ReadMeiea()
    {
        uint window16 = (uint)((m_MeieaEnables >> (m_MeiaWindow * 16)) & 0xFFFF);
        return (uint)m_MeiaWindow | (window16 << 16);
    }

    /// <summary>
    /// Writes the MEIEA CSR.
    /// For CSRRS/CSRRC/CSRRW, the CPU calls <c>Set(computed_value)</c> where
    /// <c>computed_value</c> already encodes the result of OR/AND-NOT/replace.
    /// Bits [4:0] select the window; bits [31:16] replace the 16 enable bits for that window.
    /// </summary>
    private void WriteMeiea(uint value)
    {
        m_MeiaWindow = (int)(value & 0x1F);
        uint enables = (value >> 16) & 0xFFFF;
        ulong mask = 0xFFFFUL << (m_MeiaWindow * 16);
        m_MeieaEnables = (m_MeieaEnables & ~mask) | ((ulong)enables << (m_MeiaWindow * 16));
    }

    /// <summary>
    /// Reads the MEIPA CSR (read-only mirror of <see cref="m_Pending"/>).
    /// Same windowed layout as MEIEA; the window is selected by <see cref="m_MeiaWindow"/>.
    /// </summary>
    private uint ReadMeipa()
    {
        uint window16 = (uint)((m_Pending >> (m_MeiaWindow * 16)) & 0xFFFF);
        return (uint)m_MeiaWindow | (window16 << 16);
    }

    /// <summary>
    /// Reads the MEINEXT CSR.
    /// Returns the IRQ number of the lowest-numbered IRQ that is pending, enabled,
    /// and has priority >= MEICONTEXT.PPREEMPT (so a preempted handler is not re-entered).
    /// The value is the IRQ number left-shifted by 2, usable as a byte offset into
    /// the soft vector table (each entry is 4 bytes).
    /// Bit 31 (NOIRQ) is set when no eligible IRQ exists.
    /// Spec §3.8.6.1.3: "the IRQ number … left-shifted by two".
    /// </summary>
    private uint ReadMeinext()
    {
        ulong pendingAndEnabled = m_Pending & m_MeieaEnables;
        if (pendingAndEnabled == 0)
            return 0x8000_0000u; // NOIRQ

        // Rule 3 (§3.8.6.1.2): IRQ must have priority >= PPREEMPT.
        // All IRQ priorities are 0 (MEIPRA not implemented); only eligible when PPREEMPT == 0.
        var meicontext = m_Processor.CSR.RawGet(CSR.MEICONTEXT);
        uint ppreempt = (meicontext >> 24) & 0xFu;
        if (ppreempt > 0)
            return 0x8000_0000u; // NOIRQ: all IRQs blocked by PPREEMPT gate

        int irq = BitOperations.TrailingZeroCount(pendingAndEnabled);
        return (uint)(irq << 2);
    }

    /// <summary>
    /// Handles writes to the MEINEXT CSR.
    /// When bit 0 (UPDATE) is set, atomically updates MEICONTEXT with the IRQ number,
    /// NOIRQ flag, and new PREEMPT value for the interrupt about to be dispatched.
    /// Spec §3.8.6.1.5: "Writing 1 to MEINEXT.UPDATE updates MEICONTEXT as follows…"
    /// </summary>
    private void WriteMeinext(uint value)
    {
        if ((value & 1u) == 0) return; // UPDATE bit not set — no-op

        // The written value is (old MEINEXT | UPDATE). Extract IRQ state from it.
        bool noirq = (value & 0x8000_0000u) != 0;
        uint irq   = (value >> 2) & 0x1FFu; // MEINEXT bits[10:2] = IRQ number

        var meicontext = m_Processor.CSR.RawGet(CSR.MEICONTEXT);

        // Clear PREEMPT[20:16], NOIRQ[15], IRQ[12:4]
        meicontext &= ~0x001F_9FF0u;

        if (noirq)
        {
            // No eligible IRQ: set NOIRQ, set PREEMPT to 0x10 (blocks all future preemption).
            meicontext |= (1u << 15);           // NOIRQ = 1
            meicontext |= (0x10u << 16);        // PREEMPT = 0x10
        }
        else
        {
            // IRQ present: store IRQ number and set PREEMPT = irq_priority + 1.
            // All priorities are 0 (MEIPRA not implemented), so PREEMPT = 1.
            meicontext |= (irq << 4);           // IRQ bits[12:4]
            meicontext |= (1u << 16);           // PREEMPT = 0 + 1 = 1
        }

        m_Processor.CSR.RawSet(CSR.MEICONTEXT, meicontext);
    }

    /// <summary>
    /// Reads the MEICONTEXT CSR.
    /// MTIESAVE (bit 3) and MSIESAVE (bit 2) always reflect the current MIE.MTIE/MSIE
    /// values so that a <c>csrrsi a2, meicontext, CLEARTS</c> captures the live MIE state
    /// for later restoration.
    /// Spec §3.8.9 Table 421.
    /// </summary>
    private uint ReadMeicontext()
    {
        var stored = m_Processor.CSR.RawGet(CSR.MEICONTEXT);
        // Overlay live MIE.MTIE/MSIE into bits 3:2
        stored &= ~0b1100u;
        var mie = m_Processor.CSR.RawGet(CSR.MIE);
        if ((mie & (1u << CSR.MxP_MTIP_BIT)) != 0) stored |= (1u << 3); // MTIESAVE
        if ((mie & (1u << CSR.MxP_MSIP_BIT)) != 0) stored |= (1u << 2); // MSIESAVE
        return stored;
    }

    /// <summary>
    /// Handles writes to the MEICONTEXT CSR.
    /// CLEARTS (bit 1): clears MIE.MTIE and MIE.MSIE (takes precedence over MTIESAVE/MSIESAVE).
    /// MTIESAVE (bit 3) / MSIESAVE (bit 2): ORed into MIE.MTIE / MIE.MSIE respectively.
    /// CLEARTS is self-clearing and not stored in the backing register.
    /// Spec §3.8.9 Table 421.
    /// </summary>
    private void WriteMeicontext(uint value)
    {
        var mie = m_Processor.CSR.RawGet(CSR.MIE);

        // Apply MTIESAVE/MSIESAVE restores first…
        if ((value & (1u << 3)) != 0) mie |= (1u << CSR.MxP_MTIP_BIT);  // MTIESAVE → MIE.MTIE
        if ((value & (1u << 2)) != 0) mie |= (1u << CSR.MxP_MSIP_BIT);  // MSIESAVE → MIE.MSIE

        // …then CLEARTS takes precedence (clears MTIE/MSIE even if MTIESAVE was also set).
        if ((value & (1u << 1)) != 0)
        {
            mie &= ~(1u << CSR.MxP_MTIP_BIT);
            mie &= ~(1u << CSR.MxP_MSIP_BIT);
        }

        m_Processor.CSR.RawSet(CSR.MIE, mie);

        // Store MEICONTEXT; CLEARTS is self-clearing so strip it from the backing value.
        m_Processor.CSR.RawSet(CSR.MEICONTEXT, value & ~(1u << 1));
    }

    // ---------------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Asserts IRQ line <paramref name="irqNumber"/> (0–51).
    /// Sets the corresponding bit in the pending mask and drives MIP.MEIP high.
    /// Idempotent — raising an already-asserted line is a no-op.
    /// </summary>
    public void RaiseIrq(int irqNumber)
    {
        m_Pending |= 1UL << irqNumber;
        // Drive MIP.MEIP (bit 11) to signal an external interrupt is pending.
        // The processor will check this after each instruction (CheckInterrupts).
        m_Processor.SetMip(CSR.MxP_MEIP_BIT, true);
    }

    /// <summary>
    /// Deasserts IRQ line <paramref name="irqNumber"/> (0–51).
    /// Clears the corresponding bit in the pending mask. MIP.MEIP is only lowered
    /// once all IRQ lines have been deasserted (pending mask reaches zero).
    /// Idempotent — clearing an already-clear line is a no-op.
    /// </summary>
    public void ClearIrq(int irqNumber)
    {
        m_Pending &= ~(1UL << irqNumber);
        if (m_Pending == 0)
            m_Processor.SetMip(CSR.MxP_MEIP_BIT, false);
    }

    /// <summary>
    /// Returns the 64-bit pending mask (one bit per IRQ line).
    /// The Hazard3 MEIPA CSR reads this so interrupt handlers can identify which
    /// peripheral fired without polling every peripheral's status register.
    /// Spec: Hazard3 custom CSR MEIPA (§3.8.9 — "External interrupt pending array").
    /// </summary>
    public ulong GetPending() => m_Pending;

    /// <summary>
    /// Asserts or clears MIP.MTIP (machine timer interrupt pending) for the RISC-V
    /// platform timer. Called by SIO when MTIME reaches MTIMECMP.
    ///
    /// MTIP is separate from the external IRQ array (m_Pending / MEIP) — it is a
    /// standard RISC-V machine timer interrupt that software enables via MIE.MTIE (bit 7).
    ///
    /// Spec: RISC-V Privileged ISA §3.1.9 (MIP.MTIP); RP2350 Datasheet §3.1.8
    ///       "RISC-V Platform Timer".
    /// </summary>
    public void SetTimerInterrupt(bool pending)
    {
        m_Processor.SetMip(CSR.MxP_MTIP_BIT, pending);
    }
}
