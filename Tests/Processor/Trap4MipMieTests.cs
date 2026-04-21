using Lemur;

namespace Tests.Processor;

/// <summary>
/// Chunk 4 — MIP and MIE: Per-Source Interrupt Controls
///
/// Once the global gate (MSTATUS.MIE, Chunk 3) is open, each interrupt
/// source has its own pair of bits — one in MIP (pending) and one in MIE
/// (enable). An interrupt fires only when BOTH of its bits are set:
///
///   MIP (Machine Interrupt Pending)  — written by hardware/peripherals
///   MIE (Machine Interrupt Enable)   — written by software to opt in
///
/// There are three standard interrupt types:
///
///   Bit  3 — MSI  (Machine Software Interrupt)
///   Bit  7 — MTI  (Machine Timer Interrupt)
///   Bit 11 — MEI  (Machine External Interrupt)
///
/// When an interrupt fires, MCAUSE bit 31 = 1 (interrupt flag) and
/// bits 30:0 hold the cause number (3, 7, or 11).
///
/// In tests, MIP is written directly via SetCSR to simulate a peripheral
/// asserting its line, without involving any actual peripheral.
///
/// NOTE: RunTo(TRAP_HANDLER) is used instead of Step() for tests that
/// expect a trap, because dcsr.stepie is hardwired to 0 on RP2350 and
/// suppresses all interrupts during single-step execution.
/// </summary>
public class Trap4MipMieTests
{
    private const uint SRAM         = 0x20000000;
    private const uint TRAP_HANDLER = SRAM + 0x100;

    private const ushort MSTATUS_CSR = 0x300;
    private const ushort MIE_CSR     = 0x304;
    private const ushort MTVEC_CSR   = 0x305;
    private const ushort MEPC_CSR    = 0x341;
    private const ushort MCAUSE_CSR  = 0x342;
    private const ushort MIP_CSR     = 0x344;

    // Bit positions shared by MIP and MIE
    private const int MSTATUS_MIE_BIT = 3;
    private const int MSIP_BIT        = 3;  // software interrupt
    private const int MTIP_BIT        = 7;  // timer interrupt
    private const int MEIP_BIT        = 11; // external interrupt

    private IDebuggable Setup()
    {
        var sut = RP2350Builder.Create();
        sut.MemoryWrite(SRAM, InstructionBuilder.NOP());
        sut.MemoryWrite(TRAP_HANDLER, InstructionBuilder.MRET());
        sut.SetCSR(MTVEC_CSR, TRAP_HANDLER);
        sut.Registers[32] = SRAM;
        // Real hardware resets with MSTATUS.MIE = 0; open the global gate
        // so interrupt tests don't have to repeat this setup.
        sut.SetCSR(MSTATUS_CSR, sut.GetCSR(MSTATUS_CSR) | (1u << MSTATUS_MIE_BIT));
        return sut;
    }

    // ── Both gates must be open ───────────────────────────────────────────────

    [Fact]
    public void MipSet_MieNotSet_NoTrap()
    {
        // A peripheral signals that an interrupt is pending (MIP.MEIP = 1),
        // but the software has not enabled external interrupts (MIE.MEIE = 0).
        // Result: no trap is taken.
        using var sut = Setup();

        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT); // hardware signals: something happened
        // MIE.MEIE left at 0 — software hasn't opted in yet

        sut.Step();

        Assert.Equal(SRAM + 4, sut.Registers[32]); // NOP advanced PC; no trap
    }

    [Fact]
    public void MieSet_MipNotSet_NoTrap()
    {
        // Software has enabled external interrupts (MIE.MEIE = 1) but no
        // peripheral has asserted its line yet (MIP.MEIP = 0).
        // Result: no trap is taken.
        using var sut = Setup();

        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT); // software: "I'm ready to handle it"
        // MIP.MEIP left at 0 — no peripheral has fired

        sut.Step();

        Assert.Equal(SRAM + 4, sut.Registers[32]);
    }

    // ── External interrupt (MEI, bit 11) ─────────────────────────────────────

    [Fact]
    public void ExternalInterrupt_BothSet_TrapTaken()
    {
        // With both gates open and MSTATUS.MIE = 1 (set by Setup), the trap fires.
        using var sut = Setup();

        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.RunTo(TRAP_HANDLER);

        Assert.Equal(TRAP_HANDLER, sut.Registers[32]);
    }

    [Fact]
    public void ExternalInterrupt_McauseBit31Set_AndCauseNumber11()
    {
        // MCAUSE for an interrupt always has bit 31 = 1 (interrupt flag).
        // External interrupt cause number = 11.
        // So MCAUSE = 0x8000_0000 | 11 = 0x8000_000B.
        using var sut = Setup();

        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.RunTo(TRAP_HANDLER);

        Assert.Equal(0x8000_000Bu, sut.GetCSR(MCAUSE_CSR));
    }

    // ── Timer interrupt (MTI, bit 7) ─────────────────────────────────────────

    [Fact]
    public void TimerInterrupt_SetsMcause_0x80000007()
    {
        // The platform timer fires when MTIME >= MTIMECMP (handled by SIO).
        // In this test we bypass SIO and set MIP.MTIP directly.
        // MCAUSE = 0x8000_0000 | 7 = 0x8000_0007.
        using var sut = Setup();

        sut.SetCSR(MIE_CSR, 1u << MTIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MTIP_BIT);

        sut.RunTo(TRAP_HANDLER);

        Assert.Equal(0x8000_0007u, sut.GetCSR(MCAUSE_CSR));
    }

    // ── Software interrupt (MSI, bit 3) ──────────────────────────────────────

    [Fact]
    public void SoftwareInterrupt_SetsMcause_0x80000003()
    {
        // Software interrupts are raised by writing MSIP in SIO (RISCV_SOFTIRQ).
        // MCAUSE = 0x8000_0000 | 3 = 0x8000_0003.
        using var sut = Setup();

        sut.SetCSR(MIE_CSR, 1u << MSIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MSIP_BIT);

        sut.RunTo(TRAP_HANDLER);

        Assert.Equal(0x8000_0003u, sut.GetCSR(MCAUSE_CSR));
    }

    // ── MEPC for asynchronous interrupts ─────────────────────────────────────

    [Fact]
    public void ExternalInterrupt_Mepc_PointsToNextInstruction()
    {
        // For asynchronous interrupts, the interrupt fires AFTER the current
        // instruction retires. MEPC = address of the instruction that would
        // have executed next — not the one that just ran.
        //
        // Sequence:
        //   1. NOP at SRAM executes → PC advances to SRAM+4
        //   2. Interrupt check sees MIP.MEIP pending
        //   3. EnterTrap saves PC (= SRAM+4) into MEPC
        //
        // After MRET the CPU resumes at SRAM+4, not at SRAM.
        using var sut = Setup();

        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.RunTo(TRAP_HANDLER);

        Assert.Equal(SRAM + 4, sut.GetCSR(MEPC_CSR));
    }

    // ── Priority ─────────────────────────────────────────────────────────────

    [Fact]
    public void ExternalInterrupt_HasPriorityOverTimer_WhenBothPending()
    {
        // Priority order (highest first): MEI(11) > MSI(3) > MTI(7).
        // Spec: RISC-V Privileged ISA Table 3.7
        // When both external and timer interrupts are pending simultaneously,
        // the external interrupt wins and its cause appears in MCAUSE.
        using var sut = Setup();

        sut.SetCSR(MIE_CSR, (1u << MEIP_BIT) | (1u << MTIP_BIT));
        sut.SetCSR(MIP_CSR, (1u << MEIP_BIT) | (1u << MTIP_BIT));

        sut.RunTo(TRAP_HANDLER);

        Assert.Equal(0x8000_000Bu, sut.GetCSR(MCAUSE_CSR));
    }

    [Fact]
    public void SoftwareInterrupt_HasPriorityOverTimer_WhenBothPending()
    {
        // MSI(3) > MTI(7): software interrupt beats timer.
        using var sut = Setup();

        sut.SetCSR(MIE_CSR, (1u << MSIP_BIT) | (1u << MTIP_BIT));
        sut.SetCSR(MIP_CSR, (1u << MSIP_BIT) | (1u << MTIP_BIT));

        sut.RunTo(TRAP_HANDLER);

        Assert.Equal(0x8000_0003u, sut.GetCSR(MCAUSE_CSR));
    }
}
