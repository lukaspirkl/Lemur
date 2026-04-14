using Venture;

namespace Tests.Processor;

/// <summary>
/// Chunk 2 — MRET: Returning from a Trap
///
/// MRET is the "return from machine-level trap handler" instruction. It is
/// the mirror image of trap entry and does two things atomically:
///
///   1. PC ← MEPC  (resume at the address saved when the trap was taken)
///   2. MSTATUS.MIE ← MSTATUS.MPIE  (re-enable interrupts as they were before)
///      MSTATUS.MPIE ← 1             (spec mandates this after MRET)
///
/// These tests isolate MRET completely — CSRs are set up manually so there
/// is no need to trigger an actual trap first.
/// </summary>
public class Trap2ReturnTests
{
    private const uint SRAM   = 0x20000000;
    private const uint TARGET = SRAM + 0x200; // arbitrary resume address

    private const ushort MSTATUS = 0x300;
    private const ushort MTVEC   = 0x305;
    private const ushort MEPC    = 0x341;

    private const int MIE_BIT  = 3;
    private const int MPIE_BIT = 7;

    // ── PC restoration ───────────────────────────────────────────────────────

    [Fact]
    public void MRET_RestoresPcFromMepc()
    {
        // The primary job of MRET: jump back to wherever the trap was taken.
        // Setting MEPC manually lets us verify the restore in isolation.
        using var sut = RP2350Builder.Create();

        sut.SetCSR(MEPC, TARGET);
        sut.MemoryWrite(SRAM, InstructionBuilder.MRET());
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(TARGET, sut.Registers[32]);
    }

    // ── MSTATUS restoration ──────────────────────────────────────────────────

    [Fact]
    public void MRET_RestoresMieFromMpie()
    {
        // MSTATUS.MIE ← MSTATUS.MPIE re-enables interrupts if they were
        // enabled before the trap. Without this, every trip through a handler
        // would permanently disable interrupts.
        // Scenario: MPIE = 1, MIE = 0 → after MRET, MIE should be 1.
        using var sut = RP2350Builder.Create();

        var mstatus = sut.GetCSR(MSTATUS);
        mstatus |=  (1u << MPIE_BIT); // MPIE = 1 (saved "was enabled" state)
        mstatus &= ~(1u << MIE_BIT);  // MIE  = 0 (currently in handler)
        sut.SetCSR(MSTATUS, mstatus);

        sut.SetCSR(MEPC, TARGET);
        sut.MemoryWrite(SRAM, InstructionBuilder.MRET());
        sut.Registers[32] = SRAM;

        sut.Step();

        var mie = (sut.GetCSR(MSTATUS) >> MIE_BIT) & 1u;
        Assert.Equal(1u, mie);
    }

    [Fact]
    public void MRET_WhenMpieWas0_LeavesMieAs0()
    {
        // Counterpart: if MPIE = 0 (interrupts were disabled before the trap),
        // MRET leaves MIE = 0. The interrupted context stays non-interruptible.
        using var sut = RP2350Builder.Create();

        var mstatus = sut.GetCSR(MSTATUS);
        mstatus &= ~(1u << MPIE_BIT); // MPIE = 0
        mstatus &= ~(1u << MIE_BIT);  // MIE  = 0
        sut.SetCSR(MSTATUS, mstatus);

        sut.SetCSR(MEPC, TARGET);
        sut.MemoryWrite(SRAM, InstructionBuilder.MRET());
        sut.Registers[32] = SRAM;

        sut.Step();

        var mie = (sut.GetCSR(MSTATUS) >> MIE_BIT) & 1u;
        Assert.Equal(0u, mie);
    }

    [Fact]
    public void MRET_AlwaysSetsMpieToOne()
    {
        // The RISC-V spec mandates that MRET sets MPIE to 1 regardless of
        // what it was before. This ensures a subsequent trap + MRET sequence
        // always restores MIE correctly even without an explicit write to MPIE.
        // Spec: RISC-V Privileged ISA Section 3.3.2
        using var sut = RP2350Builder.Create();

        // Start with MPIE = 0 to confirm MRET unconditionally sets it.
        var mstatus = sut.GetCSR(MSTATUS) & ~(1u << MPIE_BIT);
        sut.SetCSR(MSTATUS, mstatus);

        sut.SetCSR(MEPC, TARGET);
        sut.MemoryWrite(SRAM, InstructionBuilder.MRET());
        sut.Registers[32] = SRAM;

        sut.Step();

        var mpie = (sut.GetCSR(MSTATUS) >> MPIE_BIT) & 1u;
        Assert.Equal(1u, mpie);
    }

    // ── Round-trip ───────────────────────────────────────────────────────────

    [Fact]
    public void ExceptionThenMret_PcReturnsToFaultingInstruction()
    {
        // Full round-trip: take a synchronous exception, then MRET back.
        // For exceptions, MEPC = address of the faulting instruction itself,
        // so MRET re-executes it. Handlers that want to skip past the fault
        // must manually advance MEPC (e.g. MEPC += 4).
        //
        // ECALL is used as the trigger: EBREAK goes to debug halt when
        // OpenOCD is attached and never reaches the machine-mode trap path.
        using var sut = RP2350Builder.Create();

        const uint HANDLER = SRAM + 0x100;
        sut.SetCSR(MTVEC, HANDLER);
        // ECALL at SRAM is the faulting instruction (MEPC = SRAM)
        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL());
        // Handler just returns immediately
        sut.MemoryWrite(HANDLER, InstructionBuilder.MRET());
        sut.Registers[32] = SRAM;

        sut.Step(); // ECALL → trap → PC = HANDLER, MEPC = SRAM
        sut.Step(); // MRET  → PC = MEPC = SRAM

        Assert.Equal(SRAM, sut.Registers[32]);
    }
}
