using Lemur;

namespace Tests.Processor;

/// <summary>
/// Chunk 3 — MSTATUS: The Global Interrupt Gate
///
/// MSTATUS bit 3 (MIE — Machine Interrupt Enable) is the master switch for
/// all asynchronous interrupts. While MIE = 0, no interrupt can be taken
/// regardless of what is pending in MIP or enabled in MIE CSR.
///
/// This is different from the per-source enables (Chunk 4). You can think of
/// it as two locks in series:
///
///   MSTATUS.MIE ──── [global gate] ────┐
///                                       ├──► interrupt taken
///   MIP & MIE ──── [per-source gate] ──┘
///
/// The CPU manages MIE automatically on every trap:
///   Entry:  MPIE ← MIE,  MIE ← 0   (disable while in handler)
///   MRET:   MIE  ← MPIE, MPIE ← 1  (restore before resuming)
///
/// NOTE: Real hardware (Hazard3/RP2350) resets with MSTATUS.MIE = 0.
/// Setup() sets MIE = 1 explicitly so interrupt tests start from a known
/// enabled state. Tests that need MIE = 0 clear it themselves.
///
/// Interrupt tests use RunTo(TRAP_HANDLER) instead of Step() because
/// single-step mode has dcsr.stepie hardwired to 0 on RP2350, which
/// suppresses all interrupts during a step.
/// </summary>
public class Trap3MstatusTests
{
    private const uint SRAM         = 0x20000000;
    private const uint TRAP_HANDLER = SRAM + 0x100;

    private const ushort MSTATUS_CSR = 0x300;
    private const ushort MIE_CSR     = 0x304; // per-source enable register
    private const ushort MTVEC_CSR   = 0x305;
    private const ushort MIP_CSR     = 0x344; // interrupt-pending register

    private const int MSTATUS_MIE_BIT  = 3;
    private const int MSTATUS_MPIE_BIT = 7;
    private const int MEIP_BIT         = 11; // external interrupt bit in MIP/MIE


    private IDebuggable Setup()
    {
        var sut = RP2350Builder.Create();
        sut.MemoryWrite(SRAM, InstructionBuilder.NOP());
        sut.MemoryWrite(TRAP_HANDLER, InstructionBuilder.MRET());
        sut.SetCSR(MTVEC_CSR, TRAP_HANDLER);
        sut.Registers[32] = SRAM;
        // Real hardware resets with MIE = 0; set it to 1 so interrupt
        // tests start with the global gate open.
        sut.SetCSR(MSTATUS_CSR, sut.GetCSR(MSTATUS_CSR) | (1u << MSTATUS_MIE_BIT));
        return sut;
    }

    // ── The global gate ──────────────────────────────────────────────────────

    [Fact]
    public void WithMstatusIE_Clear_PendingInterrupt_NoTrapTaken()
    {
        // Even with MIP.MEIP and MIE.MEIE both set, the CPU will NOT take
        // the interrupt while MSTATUS.MIE = 0. This is what lets an interrupt
        // handler run without being interrupted by another interrupt.
        using var sut = Setup();

        // Explicitly close the global gate.
        sut.SetCSR(MSTATUS_CSR, sut.GetCSR(MSTATUS_CSR) & ~(1u << MSTATUS_MIE_BIT));

        // Open both per-source gates — would normally trigger a trap.
        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.Step(); // NOP executes; interrupt blocked by MIE=0

        // PC should be SRAM+4 (NOP advanced it), not the trap handler.
        Assert.Equal(SRAM + 4, sut.Registers[32]);
    }

    [Fact]
    public void WithMstatusIE_Set_PendingInterrupt_TrapIsTaken()
    {
        // Identical setup to the test above except the global gate is open.
        // This confirms that MSTATUS.MIE is the decisive difference.
        // RunTo is used because dcsr.stepie=0 suppresses interrupts during
        // single-step on RP2350.
        using var sut = Setup();

        // MIE = 1 already from Setup(); be explicit for documentation.
        sut.SetCSR(MSTATUS_CSR, sut.GetCSR(MSTATUS_CSR) | (1u << MSTATUS_MIE_BIT));

        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.RunTo(TRAP_HANDLER);

        Assert.Equal(TRAP_HANDLER, sut.Registers[32]);
    }

    // ── Automatic save/restore of MIE ────────────────────────────────────────

    [Fact]
    public void TrapEntry_ClearsGlobalInterruptEnable()
    {
        // The CPU atomically clears MSTATUS.MIE when entering any trap.
        // Without this, the handler itself could be interrupted immediately.
        using var sut = Setup();

        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.RunTo(TRAP_HANDLER); // interrupt fires; trap entry clears MIE

        var mie = (sut.GetCSR(MSTATUS_CSR) >> MSTATUS_MIE_BIT) & 1u;
        Assert.Equal(0u, mie);
    }

    [Fact]
    public void TrapEntry_WhenMieWas1_SavesMpieAs1()
    {
        // The old MIE value is saved in MPIE so MRET can restore it.
        // MIE was 1 before the trap, so MPIE = 1 after trap entry.
        using var sut = Setup();

        // MIE = 1 already from Setup(); explicit for documentation.
        sut.SetCSR(MSTATUS_CSR, sut.GetCSR(MSTATUS_CSR) | (1u << MSTATUS_MIE_BIT));

        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.RunTo(TRAP_HANDLER);

        var mpie = (sut.GetCSR(MSTATUS_CSR) >> MSTATUS_MPIE_BIT) & 1u;
        Assert.Equal(1u, mpie); // old MIE = 1 is preserved here for MRET
    }

    [Fact]
    public void TrapEntry_WhenMieWas0_SavesMpieAs0()
    {
        // If interrupts were already disabled before the trap, MPIE = 0.
        // This can only happen via a synchronous exception (since asynchronous
        // interrupts require MIE = 1 to fire). MRET will restore MIE = 0.
        // ECALL is used as the synchronous trigger (EBREAK goes to debug halt
        // when OpenOCD is attached).
        using var sut = Setup();

        var mstatus = sut.GetCSR(MSTATUS_CSR);
        mstatus &= ~(1u << MSTATUS_MIE_BIT);  // MIE  = 0
        mstatus &= ~(1u << MSTATUS_MPIE_BIT); // MPIE = 0 (known starting state)
        sut.SetCSR(MSTATUS_CSR, mstatus);

        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL());

        sut.Step();

        var mpie = (sut.GetCSR(MSTATUS_CSR) >> MSTATUS_MPIE_BIT) & 1u;
        Assert.Equal(0u, mpie); // old MIE = 0 was saved
    }

    [Fact]
    public void MretAfterTrap_RestoresMieFromMpie()
    {
        // Full MSTATUS round-trip: verify that the MIE value firmware
        // had before being interrupted is correctly restored on return.
        using var sut = Setup();

        // Ensure global interrupt enable is set
        var mstatus = sut.GetCSR(MSTATUS_CSR);
        mstatus |= (1u << MSTATUS_MIE_BIT);
        sut.SetCSR(MSTATUS_CSR, mstatus);

        // MIE = 1 (from Setup), interrupt fires → MIE = 0, MPIE = 1.
        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.RunTo(TRAP_HANDLER); // trap taken → PC = TRAP_HANDLER, MIE = 0

        // A real handler clears the interrupt source before returning, otherwise
        // MRET restoring MIE = 1 would immediately re-enter the trap.
        sut.SetCSR(MIP_CSR, 0u);

        sut.Step(); // MRET → MIE = MPIE = 1

        var mie = (sut.GetCSR(MSTATUS_CSR) >> MSTATUS_MIE_BIT) & 1u;
        Assert.Equal(1u, mie);
    }
}
