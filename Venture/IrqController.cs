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

    // 64-bit software-force mask: bit N is set when IRQ N is forced via MEIFA.
    // Forced bits are ORed into the effective pending set used by MEIPA / MEINEXT /
    // MIP.MEIP. A forced bit is cleared automatically by hardware when a MEINEXT
    // read returns that IRQ number (§3.8.3).
    private ulong m_MeifaForce;

    // Currently selected 16-IRQ window for MEIEA/MEIPA/MEIFA reads (bits [4:0]).
    // Set by any write to those CSR addresses.
    private int m_MeiaWindow;

    // Per-IRQ priority array, 4 bits per IRQ, 512 IRQs total (§3.8.4).
    // Bit-width matches Hazard3's max of 16 preemption priority levels.
    // Reset value is 0 so all IRQs have the lowest priority until configured.
    private readonly byte[] m_MeipraPriorities = new byte[512];

    // Currently selected 4-IRQ window for MEIPRA reads (bits [6:0]).
    private int m_MeipraWindow;

    public IrqController(Hazard3Processor processor)
    {
        m_Processor = processor;

        processor.CSR.AddGetter(0xBE0, ReadMeiea);
        processor.CSR.AddSetter(0xBE0, WriteMeiea);
        processor.CSR.AddGetter(0xBE1, ReadMeipa);
        // MEIPA is read-only; writes update the window selector but not the pending bits.
        processor.CSR.AddSetter(0xBE1, v => m_MeiaWindow = (int)(v & 0x1F));
        processor.CSR.AddGetter(0xBE2, ReadMeifa);
        processor.CSR.AddSetter(0xBE2, WriteMeifa);
        processor.CSR.AddGetter(0xBE3, ReadMeipra);
        processor.CSR.AddSetter(0xBE3, WriteMeipra);
        processor.CSR.AddGetter(0xBE4, ReadMeinext);
        processor.CSR.AddSetter(0xBE4, WriteMeinext);
        processor.CSR.AddGetter(CSR.MEICONTEXT, ReadMeicontext);
        processor.CSR.AddSetter(CSR.MEICONTEXT, WriteMeicontext);

        // Register MEI trap hooks so Hazard3Processor can delegate priority-aware
        // decisions (trap gating, vector-entry stack push) to us.
        processor.MeiTrapGate       = HasEligibleMeiAtOrAbove;
        processor.OnMeiVectorEntry  = PushPreemptStackOnVectorEntry;
    }

    /// <summary>
    /// Effective pending mask seen by MEIPA / MEINEXT / MIP.MEIP:
    /// the OR of hardware-asserted pending lines and software-forced bits.
    /// </summary>
    private ulong EffectivePending => m_Pending | m_MeifaForce;

    /// <summary>
    /// Effective enabled+pending mask (gated by MEIEA).
    /// </summary>
    private ulong EffectiveEnabledPending => EffectivePending & m_MeieaEnables;

    /// <summary>
    /// Recomputes MIP.MEIP from the effective enabled+pending mask.
    /// Called whenever pending/force/enable state changes.
    /// </summary>
    private void RecomputeMeip()
    {
        m_Processor.SetMip(CSR.MxP_MEIP_BIT, EffectiveEnabledPending != 0);
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
        RecomputeMeip();
    }

    /// <summary>
    /// Reads the MEIPA CSR (read-only mirror of <see cref="m_Pending"/>).
    /// Same windowed layout as MEIEA; the window is selected by <see cref="m_MeiaWindow"/>.
    /// </summary>
    private uint ReadMeipa()
    {
        // Forced bits appear pending in MEIPA (§3.8.3 — MEIFA writes make the
        // corresponding bits "become pending in meipa").
        uint window16 = (uint)((EffectivePending >> (m_MeiaWindow * 16)) & 0xFFFF);
        return (uint)m_MeiaWindow | (window16 << 16);
    }

    // -------------------------------------------------------------------------
    // MEIFA — External interrupt force array (0xBE2)
    // Spec §3.8.3
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads the MEIFA CSR. Bits [31:16] hold the current force-array window;
    /// bits [4:0] hold the current window index.
    /// </summary>
    private uint ReadMeifa()
    {
        uint window16 = (uint)((m_MeifaForce >> (m_MeiaWindow * 16)) & 0xFFFF);
        return (uint)m_MeiaWindow | (window16 << 16);
    }

    /// <summary>
    /// Writes the MEIFA CSR. Updates the selected 16-bit window of the force
    /// array. Setting a bit "causes the corresponding bit to become pending in
    /// meipa" and asserts MIP.MEIP (subject to enable+priority filtering);
    /// clearing a bit removes the software-induced pending.
    /// </summary>
    private void WriteMeifa(uint value)
    {
        m_MeiaWindow = (int)(value & 0x1F);
        uint force = (value >> 16) & 0xFFFF;
        ulong mask = 0xFFFFUL << (m_MeiaWindow * 16);
        m_MeifaForce = (m_MeifaForce & ~mask) | ((ulong)force << (m_MeiaWindow * 16));
        RecomputeMeip();
    }

    // -------------------------------------------------------------------------
    // MEIPRA — External interrupt priority array (0xBE3)
    // Spec §3.8.4
    //
    // Each IRQ has a 4-bit priority (16 preemption levels). A 16-bit window
    // covers 4 consecutive IRQs, so the window index is 7 bits (bits [6:0]).
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads the MEIPRA CSR. Bits [31:16] hold the current priority window
    /// (four 4-bit priorities); bits [6:0] hold the current window index.
    /// </summary>
    private uint ReadMeipra()
    {
        int baseIrq = m_MeipraWindow * 4;
        uint window16 = 0;
        for (int i = 0; i < 4; i++)
            window16 |= (uint)(m_MeipraPriorities[baseIrq + i] & 0xF) << (i * 4);
        return (uint)m_MeipraWindow | (window16 << 16);
    }

    /// <summary>
    /// Writes the MEIPRA CSR. Updates the four 4-bit priorities in the
    /// selected window. Recomputes MIP.MEIP because a priority change may
    /// allow or block a pending IRQ from asserting the external trap.
    /// </summary>
    private void WriteMeipra(uint value)
    {
        m_MeipraWindow = (int)(value & 0x7F);
        uint window16 = (value >> 16) & 0xFFFF;
        int baseIrq = m_MeipraWindow * 4;
        if (baseIrq + 3 < m_MeipraPriorities.Length)
        {
            for (int i = 0; i < 4; i++)
                m_MeipraPriorities[baseIrq + i] = (byte)((window16 >> (i * 4)) & 0xF);
        }
        RecomputeMeip();
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
        // §3.8.5: return the highest-priority interrupt that is pending+enabled
        // and has priority >= PPREEMPT. Tie-break: lowest IRQ number wins.
        var meicontext = m_Processor.CSR.RawGet(CSR.MEICONTEXT);
        uint ppreempt = (meicontext >> 24) & 0xFu;

        int bestIrq = SelectHighestPriorityIrq(ppreempt);
        if (bestIrq < 0)
            return 0x8000_0000u; // NOIRQ

        // §3.8.3: a MEIFA bit is cleared automatically when a read of MEINEXT
        // returns the corresponding IRQ number, regardless of whether
        // MEINEXT.UPDATE is written.
        ulong irqMask = 1UL << bestIrq;
        if ((m_MeifaForce & irqMask) != 0)
        {
            m_MeifaForce &= ~irqMask;
            RecomputeMeip();
        }

        return (uint)(bestIrq << 2);
    }

    /// <summary>
    /// Scans the effective enabled+pending mask and returns the IRQ number of
    /// the highest-priority entry whose priority is >= <paramref name="minPriority"/>,
    /// or -1 if none exists. Ties on priority are broken by lowest IRQ number.
    /// </summary>
    private int SelectHighestPriorityIrq(uint minPriority)
    {
        ulong effective = EffectiveEnabledPending;
        int bestIrq = -1;
        int bestPrio = -1;
        while (effective != 0)
        {
            int irq = BitOperations.TrailingZeroCount(effective);
            effective &= effective - 1;
            int prio = m_MeipraPriorities[irq] & 0xF;
            if (prio < (int)minPriority) continue;
            if (prio > bestPrio)
            {
                bestPrio = prio;
                bestIrq  = irq;
                // Lower IRQ numbers iterate first, so a later IRQ only wins
                // on strictly higher priority — ties already go to the lower IRQ.
            }
        }
        return bestIrq;
    }

    /// <summary>
    /// MEI trap gate used by <see cref="Hazard3Processor.CheckInterrupts"/>.
    /// Returns true when some IRQ is pending, enabled and has priority >= PREEMPT,
    /// i.e. the core should actually take the external interrupt trap.
    /// Spec §3.8.4: "an interrupt with priority lower than meicontext.preempt …
    /// mip.meip will not [assert], so the processor will ignore this interrupt".
    /// </summary>
    private bool HasEligibleMeiAtOrAbove(uint preempt)
        => SelectHighestPriorityIrq(preempt) >= 0;

    /// <summary>
    /// Called by <see cref="Hazard3Processor.EnterTrap"/> when entering the MEI
    /// vector. Pushes the preemption priority stack and sets PREEMPT to one
    /// level above the highest-priority eligible IRQ (or 0x10 if none is
    /// present, which disables preemption). Also sets MRETEIRQ so the
    /// matching MRET pops the stack. Spec §3.8.6, §3.8.6.1.5.
    /// </summary>
    private void PushPreemptStackOnVectorEntry()
    {
        var meicontext = m_Processor.CSR.RawGet(CSR.MEICONTEXT);
        uint oldPpreempt = (meicontext >> 24) & 0xFu;
        uint oldPreempt  = (meicontext >> 16) & 0x1Fu;

        // Compute the new PREEMPT value from the highest-priority IRQ that
        // is still visible after applying the outgoing ppreempt gate. This
        // matches the hardware behaviour described in §3.8.6 for vector entry.
        uint newPreempt;
        int irq = SelectHighestPriorityIrq(oldPpreempt);
        if (irq >= 0)
            newPreempt = (uint)((m_MeipraPriorities[irq] & 0xF) + 1);
        else
            newPreempt = 0x10; // no visible IRQ — block all preemption

        meicontext &= 0x0000_FFFFu;          // clear [31:16]
        meicontext |= (oldPreempt  << 24);   // PPREEMPT  ← old PREEMPT
        meicontext |= (oldPpreempt << 28);   // PPPREEMPT ← old PPREEMPT
        meicontext |= (newPreempt  << 16);   // PREEMPT   ← irq_priority + 1
        meicontext |= 1u;                    // MRETEIRQ  = 1
        m_Processor.CSR.RawSet(CSR.MEICONTEXT, meicontext);
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
            // IRQ present: store IRQ number and set PREEMPT = irq_priority + 1
            // (read from MEIPRA). Clamp to the 5-bit field.
            uint prio = (uint)(m_MeipraPriorities[irq & 0x1FF] & 0xF);
            meicontext |= (irq << 4);           // IRQ bits[12:4]
            meicontext |= ((prio + 1) & 0x1Fu) << 16;
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
        // RecomputeMeip only asserts when the new pending bit is also enabled
        // in MEIEA. The processor will check this after each instruction.
        RecomputeMeip();
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
