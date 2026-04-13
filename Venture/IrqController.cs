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

    public IrqController(Hazard3Processor processor)
    {
        m_Processor = processor;
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
