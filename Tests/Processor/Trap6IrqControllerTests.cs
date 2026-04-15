using Microsoft.Extensions.DependencyInjection;
using Venture;

namespace Tests.Processor;

/// <summary>
/// Chunk 6 — IrqController: Bridging 52 Peripheral IRQ Lines to MIP.MEIP
///
/// The RISC-V spec gives the CPU a single "external interrupt" bit: MIP.MEIP
/// (bit 11). But the RP2350 has 52 separate IRQ lines — GPIO bank 0, UART0,
/// UART1, SPI, DMA, etc. Something must bridge the two worlds.
///
/// That bridge is IrqController. It works as follows:
///
///   • Peripherals call RaiseIrq(N) / ClearIrq(N) to assert/deassert IRQ N.
///   • IrqController keeps a 64-bit pending mask (one bit per IRQ line).
///   • When any bit is set it drives MIP.MEIP = 1.
///   • MIP.MEIP is cleared only when ALL IRQ lines are deasserted.
///
/// After the CPU enters the MEI handler, it needs to know WHICH of the 52
/// lines fired. Hazard3 adds custom CSRs for this:
///
///   MEIEA   (0xBE0) — enable individual IRQ lines (like ARM's NVIC_ISER)
///   MEIPA   (0xBE1) — read which lines are pending (mirrors m_Pending)
///   MEINEXT (0xBE4) — returns (irq_number × 4) of the next eligible IRQ,
///                     or bit 31 set (NOIRQ) if none qualify.
///
/// IRQ 21 = IO_IRQ_BANK0, the line that UserBankIO (GPIO) uses.
///
/// These tests use CreateServiceProvider() to get both IDebuggable and
/// IrqController from the same DI container.
/// </summary>
public class Trap6IrqControllerTests
{
    private const uint SRAM         = 0x20000000;
    private const uint TRAP_HANDLER = SRAM + 0x100;

    private const ushort MTVEC_CSR   = 0x305;
    private const ushort MIP_CSR     = 0x344;
    private const ushort MIE_CSR     = 0x304;
    private const ushort MEIEA_CSR   = 0xBE0; // external interrupt enable array
    private const ushort MEINEXT_CSR = 0xBE4; // next IRQ to service

    private const int MEIP_BIT = 11;

    // ── MEIEA helper ─────────────────────────────────────────────────────────
    // MEIEA uses a windowed interface: the 52 IRQ enable bits are split into
    // groups of 16. To enable IRQ N:
    //   window        = N / 16   (selects which group of 16)
    //   bit_in_window = N % 16   (which bit within that group)
    //   write value   = window | ((1 << bit_in_window) << 16)
    //
    // The hardware extracts bits[4:0] as the window index and bits[31:16]
    // as the 16-bit enable mask for that window.
    private static uint MeieaEnableValue(int irqNumber)
    {
        int window = irqNumber / 16;
        int bit    = irqNumber % 16;
        return (uint)window | ((1u << bit) << 16);
    }

    // ── MIP.MEIP signal ──────────────────────────────────────────────────────

    [Fact]
    public void RaiseIrq_SetsMipMeip()
    {
        // RaiseIrq() on an IRQ enabled in MEIEA must set MIP bit 11 (MEIP) so
        // CheckInterrupts() can see that an external interrupt is waiting.
        using var sp  = RP2350Builder.CreateServiceProvider();
        var emu = sp.GetRequiredService<IDebuggable>();
        var irq = sp.GetRequiredService<IrqController>();

        emu.SetCSR(MEIEA_CSR, MeieaEnableValue(IrqController.IO_IRQ_BANK0));
        irq.RaiseIrq(IrqController.IO_IRQ_BANK0); // assert IRQ 21

        var mip = emu.GetCSR(MIP_CSR);
        Assert.NotEqual(0u, mip & (1u << MEIP_BIT));
    }

    [Fact]
    public void ClearIrq_ClearsMipMeip_WhenNoPendingIrqs()
    {
        // Once the last pending IRQ is cleared, MIP.MEIP drops back to 0
        // so the CPU stops taking the interrupt on every step.
        using var sp  = RP2350Builder.CreateServiceProvider();
        var emu = sp.GetRequiredService<IDebuggable>();
        var irq = sp.GetRequiredService<IrqController>();

        emu.SetCSR(MEIEA_CSR, MeieaEnableValue(IrqController.IO_IRQ_BANK0));
        irq.RaiseIrq(IrqController.IO_IRQ_BANK0);
        irq.ClearIrq(IrqController.IO_IRQ_BANK0);

        var mip = emu.GetCSR(MIP_CSR);
        Assert.Equal(0u, mip & (1u << MEIP_BIT));
    }

    [Fact]
    public void ClearOneIrq_WithAnotherStillPending_MeipRemainsSet()
    {
        // MEIP is an OR of all enabled pending lines. Clearing one IRQ while
        // another is still asserted must NOT lower MEIP — the other peripheral
        // is still waiting for service.
        using var sp  = RP2350Builder.CreateServiceProvider();
        var emu = sp.GetRequiredService<IDebuggable>();
        var irq = sp.GetRequiredService<IrqController>();

        // Enable both IRQs (they live in different 16-bit windows, so we need
        // two writes).
        emu.SetCSR(MEIEA_CSR, MeieaEnableValue(IrqController.IO_IRQ_BANK0));
        emu.SetCSR(MEIEA_CSR, MeieaEnableValue(IrqController.UART0_IRQ));
        irq.RaiseIrq(IrqController.IO_IRQ_BANK0); // GPIO (IRQ 21)
        irq.RaiseIrq(IrqController.UART0_IRQ);    // UART0 (IRQ 33)

        irq.ClearIrq(IrqController.IO_IRQ_BANK0); // GPIO cleared; UART0 still pending

        var mip = emu.GetCSR(MIP_CSR);
        Assert.NotEqual(0u, mip & (1u << MEIP_BIT)); // MEIP must stay high
    }

    [Fact]
    public void RaiseIrq_NotEnabledInMeiea_DoesNotSetMipMeip()
    {
        // Spec §3.8.2: MIP.MEIP is only asserted when an IRQ is both pending
        // in MEIPA *and* enabled in MEIEA. Raising a disabled IRQ line should
        // leave MEIP clear.
        using var sp  = RP2350Builder.CreateServiceProvider();
        var emu = sp.GetRequiredService<IDebuggable>();
        var irq = sp.GetRequiredService<IrqController>();

        irq.RaiseIrq(IrqController.IO_IRQ_BANK0);

        var mip = emu.GetCSR(MIP_CSR);
        Assert.Equal(0u, mip & (1u << MEIP_BIT));
    }

    // ── End-to-end trap path ─────────────────────────────────────────────────

    [Fact]
    public void RaiseIrq_WithMieEnabled_CausesTrap()
    {
        // Full path a GPIO interrupt takes through the system:
        //   UserBankIO detects edge
        //   → IrqController.RaiseIrq(21)
        //   → MIP.MEIP = 1
        //   → CheckInterrupts() after next instruction
        //   → EnterTrap → PC = TRAP_HANDLER
        // Here we call RaiseIrq() directly, bypassing UserBankIO.
        using var sp  = RP2350Builder.CreateServiceProvider();
        var emu = sp.GetRequiredService<IDebuggable>();
        var irq = sp.GetRequiredService<IrqController>();

        emu.MemoryWrite(SRAM, InstructionBuilder.NOP());
        emu.MemoryWrite(TRAP_HANDLER, InstructionBuilder.MRET());
        emu.SetCSR(MTVEC_CSR, TRAP_HANDLER);
        emu.Registers[32] = SRAM;

        emu.SetCSR(MIE_CSR, 1u << MEIP_BIT); // software enables external interrupts
        emu.SetCSR(MEIEA_CSR, MeieaEnableValue(IrqController.IO_IRQ_BANK0));
        irq.RaiseIrq(IrqController.IO_IRQ_BANK0);

        emu.Step();

        Assert.Equal(TRAP_HANDLER, emu.Registers[32]);
    }

    // ── MEINEXT — identifying which IRQ fired ────────────────────────────────

    [Fact]
    public void Meinext_WithIrqEnabledAndPending_ReturnsIrqNumberShiftedBy2()
    {
        // Once inside the MEI handler, firmware reads MEINEXT to find out
        // which of the 52 IRQ lines actually fired. MEINEXT returns
        // (irq_number × 4) so it can be used as a byte offset directly into
        // a 4-bytes-per-entry handler table.
        //
        // For IRQ 21 (GPIO bank 0): MEINEXT = 21 × 4 = 84.
        //
        // MEINEXT filters by both:
        //   • pending   — IRQ must be asserted in m_Pending
        //   • enabled   — IRQ must be enabled in MEIEA
        // If only one condition is met, MEINEXT reports NOIRQ (bit 31 set).
        using var sp  = RP2350Builder.CreateServiceProvider();
        var emu = sp.GetRequiredService<IDebuggable>();
        var irq = sp.GetRequiredService<IrqController>();

        // Enable IRQ 21 in MEIEA (window 1, bit 5).
        emu.SetCSR(MEIEA_CSR, MeieaEnableValue(IrqController.IO_IRQ_BANK0));
        irq.RaiseIrq(IrqController.IO_IRQ_BANK0);

        var meinext = emu.GetCSR(MEINEXT_CSR);

        Assert.Equal(0u,  meinext & 0x8000_0000u); // bit 31 (NOIRQ) must be clear
        Assert.Equal(84u, meinext & ~0x8000_0000u); // 21 × 4 = 84
    }

    [Fact]
    public void Meinext_WithIrqPendingButNotEnabledInMeiea_ReturnsNoirqFlag()
    {
        // If an IRQ is raised but not enabled in MEIEA, MEINEXT will not
        // dispatch it — it returns bit 31 set (NOIRQ). The interrupt controller
        // effectively masks it out. MIP.MEIP may still be set (the line is
        // asserted) but the handler won't see it via MEINEXT.
        using var sp  = RP2350Builder.CreateServiceProvider();
        var emu = sp.GetRequiredService<IDebuggable>();
        var irq = sp.GetRequiredService<IrqController>();

        // Raise the IRQ — but do NOT write to MEIEA, so it stays disabled.
        irq.RaiseIrq(IrqController.IO_IRQ_BANK0);

        var meinext = emu.GetCSR(MEINEXT_CSR);

        Assert.NotEqual(0u, meinext & 0x8000_0000u); // NOIRQ flag must be set
    }

    [Fact]
    public void Meinext_WithNoIrqsPending_ReturnsNoirqFlag()
    {
        // Baseline: when nothing is pending at all, MEINEXT = NOIRQ.
        using var sp  = RP2350Builder.CreateServiceProvider();
        var emu = sp.GetRequiredService<IDebuggable>();

        var meinext = emu.GetCSR(MEINEXT_CSR);

        Assert.NotEqual(0u, meinext & 0x8000_0000u);
    }
}
