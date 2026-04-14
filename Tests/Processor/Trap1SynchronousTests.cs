using Venture;

namespace Tests.Processor;

/// <summary>
/// Chunk 1 — Synchronous Traps (Exceptions)
///
/// A synchronous trap fires because of something that went wrong during the
/// execution of a specific instruction: a bad opcode, a misaligned address,
/// a deliberate breakpoint, etc.
///
/// The CPU reacts atomically:
///   1. Saves the faulting instruction's address in MEPC.
///   2. Writes a cause code into MCAUSE (bit 31 = 0 for exceptions).
///   3. For memory faults, writes the bad address into MTVAL (Hazard3: hardwired 0).
///   4. Saves MSTATUS.MIE into MSTATUS.MPIE, then clears MSTATUS.MIE.
///   5. Jumps to the address in MTVEC.
///
/// Synchronous traps fire regardless of whether interrupts are globally
/// enabled (MSTATUS.MIE). They are NOT the same as interrupts.
///
/// NOTE: EBREAK cannot be used to trigger a machine-mode trap when an
/// OpenOCD debugger is attached — EBREAK causes a debug halt instead.
/// All synchronous-exception tests use ECALL (cause 11) as the trigger.
/// </summary>
public class Trap1SynchronousTests
{
    // ── memory layout ────────────────────────────────────────────────────────
    private const uint SRAM         = 0x20000000;
    private const uint TRAP_HANDLER = SRAM + 0x100; // where we point MTVEC

    // ── CSR addresses (RISC-V Privileged ISA Table 2.1) ──────────────────────
    private const ushort MSTATUS = 0x300;
    private const ushort MTVEC   = 0x305;
    private const ushort MEPC    = 0x341;
    private const ushort MCAUSE  = 0x342;
    private const ushort MTVAL   = 0x343;

    // ── MSTATUS bit positions ────────────────────────────────────────────────
    // bit 3 (MIE)  — global machine-level interrupt enable
    // bit 7 (MPIE) — previous MIE, saved automatically on trap entry
    private const int MIE_BIT  = 3;
    private const int MPIE_BIT = 7;

    /// <summary>
    /// Creates a fresh emulator with MTVEC pointing at TRAP_HANDLER.
    /// A MRET is placed at the handler so the CPU has a valid instruction
    /// to execute if a test calls Step() a second time.
    /// </summary>
    private IDebuggable Setup()
    {
        var sut = RP2350Builder.Create();
        sut.SetCSR(MTVEC, TRAP_HANDLER);
        sut.MemoryWrite(TRAP_HANDLER, InstructionBuilder.MRET());
        return sut;
    }

    // ── ECALL (synchronous exception, cause 11) ──────────────────────────────

    [Fact]
    public void Exception_JumpsToMtvec()
    {
        // Any synchronous exception causes the CPU to jump to MTVEC.
        // We use ECALL as the trigger (cause 11, EnvironmentCallFromMMode).
        using var sut = Setup();
        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL());
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(TRAP_HANDLER, sut.Registers[32]);
    }

    [Fact]
    public void ECALL_SetsMcause_11()
    {
        // ECALL in M-mode raises cause 11 (EnvironmentCallFromMMode).
        // Bit 31 = 0 means exception (not interrupt). Bits 30:0 = 11.
        // Spec: RISC-V Privileged ISA Table 3.6
        using var sut = Setup();
        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL());
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(11u, sut.GetCSR(MCAUSE));
    }

    [Fact]
    public void Exception_SetsMepc_ToFaultingAddress()
    {
        // For synchronous exceptions, MEPC = address of the faulting instruction.
        // The handler reads MEPC to know where to return (or skip past the fault).
        // Spec: RISC-V Privileged ISA Section 3.1.14
        using var sut = Setup();
        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL());
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(SRAM, sut.GetCSR(MEPC));
    }

    // ── Misaligned load ──────────────────────────────────────────────────────

    [Fact]
    public void MisalignedLoad_SetsMcause_4()
    {
        // LW (32-bit load) requires 4-byte alignment. Loading from an address
        // where address % 4 != 0 causes exception cause 4 (LoadAddressMisaligned).
        // Spec: RISC-V Privileged ISA Table 3.6
        using var sut = Setup();

        // x1 = SRAM + 1 — intentionally misaligned
        sut.Registers[1] = SRAM + 1;
        // LW x2, 0(x1) — load word from the address held in x1
        sut.MemoryWrite(SRAM, InstructionBuilder.LW(rd: 2, rs1: 1));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(4u, sut.GetCSR(MCAUSE));
    }

    [Fact]
    public void MisalignedLoad_Mtval_IsHardwiredZero()
    {
        // On Hazard3/RP2350, MTVAL is hardwired to zero (datasheet §3.8.9).
        // The faulting address is NOT written to MTVAL, contrary to what the
        // RISC-V privileged spec permits as an optional implementation choice.
        using var sut = Setup();

        sut.Registers[1] = SRAM + 1;
        sut.MemoryWrite(SRAM, InstructionBuilder.LW(rd: 2, rs1: 1));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(0u, sut.GetCSR(MTVAL));
    }

    // ── MSTATUS side-effects on trap entry ───────────────────────────────────

    [Fact]
    public void TrapEntry_ClearsGlobalInterruptEnable()
    {
        // MSTATUS.MIE is cleared on trap entry so the handler runs with
        // interrupts disabled. This prevents a recursive trap entry from
        // a peripheral interrupt firing inside the handler.
        // Spec: RISC-V Privileged ISA Section 3.1.6.1
        using var sut = Setup();

        // Ensure MIE = 1 before the trap so we can observe it being cleared.
        sut.SetCSR(MSTATUS, sut.GetCSR(MSTATUS) | (1u << MIE_BIT));

        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL());
        sut.Registers[32] = SRAM;

        sut.Step();

        var mie = (sut.GetCSR(MSTATUS) >> MIE_BIT) & 1u;
        Assert.Equal(0u, mie);
    }

    [Fact]
    public void TrapEntry_SavesOldMieIntoMpie()
    {
        // Before jumping to the handler the CPU saves the old MSTATUS.MIE
        // into MSTATUS.MPIE. MRET uses MPIE to restore MIE, so the
        // interrupted context resumes with the same interrupt-enable state.
        // Here MIE was 1 before the trap, so MPIE should be 1 after.
        using var sut = Setup();

        // Be explicit: set MIE = 1.
        sut.SetCSR(MSTATUS, sut.GetCSR(MSTATUS) | (1u << MIE_BIT));

        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL());
        sut.Registers[32] = SRAM;

        sut.Step();

        var mpie = (sut.GetCSR(MSTATUS) >> MPIE_BIT) & 1u;
        Assert.Equal(1u, mpie); // old MIE = 1 was saved here
    }

    [Fact]
    public void TrapEntry_WhenMieWas0_SavesMpie_As0()
    {
        // Counterpart of the test above: if MIE was 0 before the trap
        // (e.g., inside a nested handler), MPIE receives 0. This is only
        // reachable for synchronous exceptions since asynchronous interrupts
        // cannot fire while MSTATUS.MIE = 0.
        using var sut = Setup();

        // Clear both MIE and MPIE to start from a known state.
        var mstatus = sut.GetCSR(MSTATUS);
        mstatus &= ~(1u << MIE_BIT);
        mstatus &= ~(1u << MPIE_BIT);
        sut.SetCSR(MSTATUS, mstatus);

        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL());
        sut.Registers[32] = SRAM;

        sut.Step();

        var mpie = (sut.GetCSR(MSTATUS) >> MPIE_BIT) & 1u;
        Assert.Equal(0u, mpie); // old MIE = 0 was saved here
    }
}
