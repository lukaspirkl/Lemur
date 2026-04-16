using System.Numerics;
using Venture.Processor;

namespace Venture;

/// <summary>
/// System-level IRQ aggregator implementing the Hazard3 Xh3irq external interrupt
/// controller for the RP2350 (§3.8).
///
/// Peripherals call <see cref="RaiseIrq"/>/<see cref="ClearIrq"/> to assert/deassert
/// their individual IRQ line (0–51). The controller maintains per-IRQ enable, pending,
/// force, and priority state via the Xh3irq CSRs and drives MIP.MEIP accordingly.
///
/// State ownership:
///   The per-IRQ arrays (enable, force, priority) are stored inside the
///   <see cref="CsrWindowedEntry"/> objects registered in <see cref="CSR"/> — the CSR
///   subsystem is the single source of truth. IrqController holds direct references to
///   those entries for efficient bitwise operations that would be awkward through the
///   windowed access protocol.
///
///   <see cref="m_Pending"/> tracks hardware-asserted IRQ lines from peripherals and
///   remains here because it is driven by physical signal transitions, not firmware writes.
///
/// Spec: RP2350 Datasheet §3.8 — "Processor subsystem interrupts"; §3.8.6.1 — "Xh3irq".
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
    /// <summary>IRQ 15 — PIO0 SM 0/1 interrupt 0.</summary>
    public const int PIO0_IRQ_0       = 15;
    /// <summary>IRQ 16 — PIO0 SM 2/3 interrupt 1.</summary>
    public const int PIO0_IRQ_1       = 16;
    /// <summary>IRQ 17 — PIO1 SM 0/1 interrupt 0.</summary>
    public const int PIO1_IRQ_0       = 17;
    /// <summary>IRQ 18 — PIO1 SM 2/3 interrupt 1.</summary>
    public const int PIO1_IRQ_1       = 18;
    /// <summary>IRQ 19 — PIO2 SM 0/1 interrupt 0.</summary>
    public const int PIO2_IRQ_0       = 19;
    /// <summary>IRQ 20 — PIO2 SM 2/3 interrupt 1.</summary>
    public const int PIO2_IRQ_1       = 20;
    /// <summary>IRQ 21 — GPIO Bank 0 (Secure). Core-local.</summary>
    public const int IO_IRQ_BANK0     = 21;
    /// <summary>IRQ 22 — GPIO Bank 0 (Non-secure). Core-local.</summary>
    public const int IO_IRQ_BANK0_NS  = 22;
    /// <summary>IRQ 23 — QSPI GPIO (Secure).</summary>
    public const int IO_IRQ_QSPI      = 23;
    /// <summary>IRQ 24 — QSPI GPIO (Non-secure).</summary>
    public const int IO_IRQ_QSPI_NS   = 24;
    /// <summary>IRQ 25 — SIO inter-processor FIFO (Secure). Core-local.</summary>
    public const int SIO_IRQ_FIFO     = 25;
    /// <summary>IRQ 26 — SIO doorbell (Secure). Core-local.</summary>
    public const int SIO_IRQ_BELL     = 26;
    /// <summary>IRQ 27 — SIO FIFO (Non-secure). Core-local.</summary>
    public const int SIO_IRQ_FIFO_NS  = 27;
    /// <summary>IRQ 28 — SIO doorbell (Non-secure). Core-local.</summary>
    public const int SIO_IRQ_BELL_NS  = 28;
    /// <summary>IRQ 29 — RISC-V platform timer compare. Core-local.</summary>
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
    // IRQs 46–51: SPAREIRQ_IRQ_0..5 — software-triggered only, not modelled here.

    // ---------------------------------------------------------------------------
    // State
    // ---------------------------------------------------------------------------

    // Hardware-asserted pending lines (set by RaiseIrq / ClearIrq from peripherals).
    // This lives here rather than in CSR because it is driven by physical signal
    // transitions, not by firmware CSR writes.
    private ulong m_Pending;

    // CSR access — used for MEICONTEXT, MIP, MIE reads and writes.
    private readonly ICsrAccess m_Csr;

    // Direct references to the windowed CSR entries that store per-IRQ arrays.
    // Data lives in the entries (CSR owns the state); we access it here via
    // efficient direct methods rather than the windowed window protocol.
    private readonly CsrWindowedEntry m_MeieaEntry;   // 0xBE0 — enable array
    private readonly CsrWindowedEntry m_MeifaEntry;   // 0xBE2 — force array
    private readonly CsrWindowedEntry m_MeipraEntry;  // 0xBE3 — priority array

    public IrqController(CSR csr, Hazard3Processor processor)
    {
        m_Csr = csr;

        // Retrieve windowed entries (registered by CSR.SetupRegisters).
        m_MeieaEntry  = (CsrWindowedEntry)csr.GetEntry(0xBE0)!;
        m_MeifaEntry  = (CsrWindowedEntry)csr.GetEntry(0xBE2)!;
        m_MeipraEntry = (CsrWindowedEntry)csr.GetEntry(0xBE3)!;

        // MEIPA (0xBE1) is a read-only derived view of pending | force.
        // Wire its computed reader now that we hold references to both sources.
        var meipaEntry = (CsrWindowedEntry)csr.GetEntry(0xBE1)!;
        meipaEntry.SetComputedReader(windowIndex =>
        {
            ulong effective = m_Pending | m_MeifaEntry.GetUlong();
            return (uint)((effective >> (windowIndex * 16)) & 0xFFFF);
        });

        // MEINEXT and MEICONTEXT have complex read/write logic; attach live-value
        // delegates so CSR.Get/Set dispatch through our handlers.
        csr.GetEntry(0xBE4)!.LiveValue(ReadMeinext, WriteMeinext);
        csr.GetEntry(CSR.MEICONTEXT)!.LiveValue(ReadMeicontext, WriteMeicontext);

        // Register MEI trap hooks so Hazard3Processor delegates priority-aware
        // decisions (trap gating, vector-entry stack push) to us.
        processor.MeiTrapGate      = HasEligibleMeiAtOrAbove;
        processor.OnMeiVectorEntry = PushPreemptStackOnVectorEntry;
    }

    /// <summary>Effective pending = hardware-asserted | software-forced.</summary>
    private ulong EffectivePending => m_Pending | m_MeifaEntry.GetUlong();

    /// <summary>Effective enabled+pending (gated by MEIEA).</summary>
    private ulong EffectiveEnabledPending => EffectivePending & m_MeieaEntry.GetUlong();

    /// <summary>Recomputes MIP.MEIP from the enabled+pending mask.</summary>
    private void RecomputeMeip()
    {
        bool asserted = EffectiveEnabledPending != 0;
        var mip = m_Csr.Get(CSR.MIP);
        if (asserted) mip |=  (1u << CSR.MxP_MEIP_BIT);
        else          mip &= ~(1u << CSR.MxP_MEIP_BIT);
        // Use ForceWrite so we don't trigger the software CSR path / logging.
        // We update the backing store directly since MIP has no setter hook.
        ((CSR)m_Csr).ForceWrite(CSR.MIP, mip);
    }

    // ── MEINEXT (0xBE4) ───────────────────────────────────────────────────────

    /// <summary>
    /// Reads MEINEXT. Returns the highest-priority pending+enabled IRQ (irq &lt;&lt; 2),
    /// or 0x8000_0000 (NOIRQ) if none qualifies.
    /// Spec §3.8.5.
    /// </summary>
    private uint ReadMeinext()
    {
        uint ppreempt = (m_Csr.Get(CSR.MEICONTEXT) >> 24) & 0xFu;

        int bestIrq = SelectHighestPriorityIrq(ppreempt);
        if (bestIrq < 0)
            return 0x8000_0000u; // NOIRQ

        // §3.8.3: a MEIFA bit is cleared automatically when MEINEXT returns
        // that IRQ number, regardless of whether UPDATE is written.
        if (m_MeifaEntry.GetBit(bestIrq))
        {
            m_MeifaEntry.SetBit(bestIrq, false);
            RecomputeMeip();
        }

        return (uint)(bestIrq << 2);
    }

    /// <summary>
    /// Handles writes to MEINEXT. When bit 0 (UPDATE) is set, atomically updates
    /// MEICONTEXT with the IRQ number, NOIRQ flag, and new PREEMPT value.
    /// Spec §3.8.6.1.5.
    /// </summary>
    private void WriteMeinext(uint value)
    {
        if ((value & 1u) == 0) return; // UPDATE bit not set — no-op

        bool noirq = (value & 0x8000_0000u) != 0;
        uint irq   = (value >> 2) & 0x1FFu;

        var meicontext = m_Csr.Get(CSR.MEICONTEXT);
        meicontext &= ~0x001F_9FF0u; // clear PREEMPT[20:16], NOIRQ[15], IRQ[12:4]

        if (noirq)
        {
            meicontext |= (1u << 15);    // NOIRQ = 1
            meicontext |= (0x10u << 16); // PREEMPT = 0x10 (blocks all preemption)
        }
        else
        {
            uint prio = (uint)(m_MeipraEntry.GetNibble((int)(irq & 0x1FF)) & 0xF);
            meicontext |= (irq << 4);
            meicontext |= ((prio + 1) & 0x1Fu) << 16;
        }

        ((CSR)m_Csr).ForceWrite(CSR.MEICONTEXT, meicontext);
    }

    // ── MEICONTEXT (0xBE5) ────────────────────────────────────────────────────

    /// <summary>
    /// Reads MEICONTEXT. MTIESAVE (bit 3) and MSIESAVE (bit 2) always reflect the
    /// live MIE.MTIE/MSIE values so a csrrsi with CLEARTS captures current state.
    /// Spec §3.8.9 Table 421.
    /// </summary>
    private uint ReadMeicontext()
    {
        var stored = ((CSR)m_Csr).ForceRead(CSR.MEICONTEXT);
        // Overlay live MIE.MTIE/MSIE into bits 3:2
        stored &= ~0b1100u;
        var mie = m_Csr.Get(CSR.MIE);
        if ((mie & (1u << CSR.MxP_MTIP_BIT)) != 0) stored |= (1u << 3); // MTIESAVE
        if ((mie & (1u << CSR.MxP_MSIP_BIT)) != 0) stored |= (1u << 2); // MSIESAVE
        return stored;
    }

    /// <summary>
    /// Handles writes to MEICONTEXT. CLEARTS clears MIE.MTIE/MSIE; MTIESAVE/MSIESAVE
    /// restore them. CLEARTS is self-clearing (not stored in the backing register).
    /// Spec §3.8.9 Table 421.
    /// </summary>
    private void WriteMeicontext(uint value)
    {
        var mie = m_Csr.Get(CSR.MIE);

        if ((value & (1u << 3)) != 0) mie |= (1u << CSR.MxP_MTIP_BIT); // MTIESAVE → MIE.MTIE
        if ((value & (1u << 2)) != 0) mie |= (1u << CSR.MxP_MSIP_BIT); // MSIESAVE → MIE.MSIE

        if ((value & (1u << 1)) != 0) // CLEARTS takes precedence
        {
            mie &= ~(1u << CSR.MxP_MTIP_BIT);
            mie &= ~(1u << CSR.MxP_MSIP_BIT);
        }

        ((CSR)m_Csr).ForceWrite(CSR.MIE, mie);
        ((CSR)m_Csr).ForceWrite(CSR.MEICONTEXT, value & ~(1u << 1)); // strip CLEARTS
    }

    // ── Priority selection ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the IRQ number of the highest-priority pending+enabled IRQ
    /// with priority >= <paramref name="minPriority"/>, or -1 if none.
    /// Ties broken by lowest IRQ number.
    /// </summary>
    private int SelectHighestPriorityIrq(uint minPriority)
    {
        ulong effective = EffectiveEnabledPending;
        int   bestIrq   = -1;
        int   bestPrio  = -1;

        while (effective != 0)
        {
            int irq  = BitOperations.TrailingZeroCount(effective);
            effective &= effective - 1;
            int prio = m_MeipraEntry.GetNibble(irq);
            if (prio < (int)minPriority) continue;
            if (prio > bestPrio)
            {
                bestPrio = prio;
                bestIrq  = irq;
            }
        }
        return bestIrq;
    }

    // ── Processor hooks ───────────────────────────────────────────────────────

    /// <summary>
    /// MEI trap gate for <see cref="Hazard3Processor.CheckInterrupts"/>.
    /// Returns true when a pending+enabled IRQ has priority >= PREEMPT.
    /// Spec §3.8.4.
    /// </summary>
    private bool HasEligibleMeiAtOrAbove(uint preempt)
        => SelectHighestPriorityIrq(preempt) >= 0;

    /// <summary>
    /// Called by <see cref="Hazard3Processor.EnterTrap"/> when entering the MEI vector.
    /// Pushes the preemption priority stack. Spec §3.8.6, §3.8.6.1.5.
    /// </summary>
    private void PushPreemptStackOnVectorEntry()
    {
        var meicontext  = ((CSR)m_Csr).ForceRead(CSR.MEICONTEXT);
        uint oldPpreempt = (meicontext >> 24) & 0xFu;
        uint oldPreempt  = (meicontext >> 16) & 0x1Fu;

        uint newPreempt;
        int irq = SelectHighestPriorityIrq(oldPpreempt);
        newPreempt = irq >= 0
            ? (uint)((m_MeipraEntry.GetNibble(irq) & 0xF) + 1)
            : 0x10u; // no visible IRQ — block all preemption

        meicontext &= 0x0000_FFFFu;
        meicontext |= (oldPreempt  << 24); // PPREEMPT  ← old PREEMPT
        meicontext |= (oldPpreempt << 28); // PPPREEMPT ← old PPREEMPT
        meicontext |= (newPreempt  << 16); // PREEMPT   ← irq_priority + 1
        meicontext |= 1u;                  // MRETEIRQ  = 1
        ((CSR)m_Csr).ForceWrite(CSR.MEICONTEXT, meicontext);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Asserts IRQ line <paramref name="irqNumber"/> (0–51).
    /// Sets the pending bit and drives MIP.MEIP if the IRQ is enabled.
    /// </summary>
    public void RaiseIrq(int irqNumber)
    {
        m_Pending |= 1UL << irqNumber;
        RecomputeMeip();
    }

    /// <summary>
    /// Deasserts IRQ line <paramref name="irqNumber"/> (0–51).
    /// Lowers MIP.MEIP once all lines are clear.
    /// </summary>
    public void ClearIrq(int irqNumber)
    {
        m_Pending &= ~(1UL << irqNumber);
        RecomputeMeip();
    }

    /// <summary>
    /// Returns the 64-bit hardware-asserted pending mask (one bit per IRQ line).
    /// Does not include software-forced bits (MEIFA).
    /// </summary>
    public ulong GetPending() => m_Pending;

    /// <summary>
    /// Asserts or clears MIP.MTIP for the RISC-V platform timer.
    /// Called by SIO when MTIME reaches MTIMECMP.
    /// </summary>
    public void SetTimerInterrupt(bool pending)
    {
        var mip = m_Csr.Get(CSR.MIP);
        if (pending) mip |=  (1u << CSR.MxP_MTIP_BIT);
        else         mip &= ~(1u << CSR.MxP_MTIP_BIT);
        ((CSR)m_Csr).ForceWrite(CSR.MIP, mip);
    }
}
