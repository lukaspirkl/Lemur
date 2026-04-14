using Venture;

namespace Tests.Processor;

/// <summary>
/// Chunk 5 — MTVEC: Direct vs Vectored Mode
///
/// MTVEC controls where the CPU jumps when a trap fires.
///
///   Bits [31:2] = BASE  — the handler base address (must be 4-byte aligned)
///   Bits  [1:0] = MODE  — 0 = Direct, 1 = Vectored
///
/// ── Direct mode (MODE = 0) ──────────────────────────────────────────────────
/// ALL traps — both synchronous exceptions and asynchronous interrupts —
/// jump to BASE.
///
/// ── Vectored mode (MODE = 1) ────────────────────────────────────────────────
/// Asynchronous INTERRUPTS jump to BASE + 4 × cause_number.
/// Synchronous EXCEPTIONS still jump to BASE even in vectored mode.
/// Spec: RISC-V Privileged ISA Section 3.1.7
///
/// NOTE: Exception tests use ECALL (not EBREAK) because EBREAK is
/// intercepted by OpenOCD as a debug halt and never reaches MTVEC.
/// Interrupt tests use RunTo(handler) because dcsr.stepie is hardwired
/// to 0 on RP2350, suppressing interrupts during single-step.
/// </summary>
public class Trap5MtvecTests
{
    private const uint SRAM = 0x20000000;

    // A well-aligned address for the trap handler table.
    // Offset of 0x200 keeps MTVEC bits [1:0] clean for the mode field.
    private const uint BASE = SRAM + 0x200;

    private const ushort MSTATUS_CSR = 0x300;
    private const ushort MIE_CSR     = 0x304;
    private const ushort MTVEC_CSR   = 0x305;
    private const ushort MIP_CSR     = 0x344;

    // MTVEC mode bits (written into bits [1:0] of MTVEC)
    private const uint MODE_DIRECT   = 0u;
    private const uint MODE_VECTORED = 1u;

    // MIP/MIE bit positions
    private const int MSTATUS_MIE_BIT = 3;
    private const int MSIP_BIT        = 3;
    private const int MTIP_BIT        = 7;
    private const int MEIP_BIT        = 11;

    private IDebuggable Setup()
    {
        var sut = RP2350Builder.Create();
        // NOP gives the CPU one instruction to retire before the interrupt
        // check fires (relevant for asynchronous interrupt tests).
        sut.MemoryWrite(SRAM, InstructionBuilder.NOP());
        sut.Registers[32] = SRAM;
        // Real hardware resets with MSTATUS.MIE = 0; open the global gate
        // so interrupt tests don't need to repeat this.
        sut.SetCSR(MSTATUS_CSR, sut.GetCSR(MSTATUS_CSR) | (1u << MSTATUS_MIE_BIT));
        return sut;
    }

    // ── Direct mode ──────────────────────────────────────────────────────────

    [Fact]
    public void DirectMode_Exception_JumpsToBase()
    {
        // In direct mode, synchronous exceptions jump straight to BASE.
        // ECALL is used as the trigger (EBREAK goes to debug halt with OCD).
        using var sut = Setup();

        sut.SetCSR(MTVEC_CSR, BASE | MODE_DIRECT);
        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL());

        sut.Step();

        Assert.Equal(BASE, sut.Registers[32]);
    }

    [Fact]
    public void DirectMode_Interrupt_AlsoJumpsToBase()
    {
        // In direct mode, asynchronous interrupts also jump to BASE —
        // there is no per-cause offset. A single handler must inspect MCAUSE.
        using var sut = Setup();

        sut.SetCSR(MTVEC_CSR, BASE | MODE_DIRECT);
        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.RunTo(BASE);

        Assert.Equal(BASE, sut.Registers[32]);
    }

    // ── Vectored mode — exceptions ───────────────────────────────────────────

    [Fact]
    public void VectoredMode_Exception_StillJumpsToBase_NoOffset()
    {
        // Vectored mode only offsets ASYNCHRONOUS interrupts.
        // Synchronous exceptions (ECALL, misaligned access, …) always
        // jump to BASE, never BASE + 4×cause.
        // Spec: "In vectored mode, synchronous exceptions go to BASE."
        // ECALL is used as the trigger (EBREAK goes to debug halt with OCD).
        using var sut = Setup();

        sut.SetCSR(MTVEC_CSR, BASE | MODE_VECTORED);
        sut.MemoryWrite(SRAM, InstructionBuilder.ECALL()); // synchronous, cause = 11

        sut.Step();

        // Must be BASE, not BASE + 4×11 = BASE + 44.
        Assert.Equal(BASE, sut.Registers[32]);
    }

    // ── Vectored mode — interrupts ───────────────────────────────────────────

    [Fact]
    public void VectoredMode_ExternalInterrupt_JumpsToBase_Plus44()
    {
        // External interrupt has cause number 11.
        // Handler address = BASE + 4 × 11 = BASE + 44.
        using var sut = Setup();

        sut.SetCSR(MTVEC_CSR, BASE | MODE_VECTORED);
        sut.SetCSR(MIE_CSR, 1u << MEIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MEIP_BIT);

        sut.RunTo(BASE + 44u);

        Assert.Equal(BASE + 44u, sut.Registers[32]);
    }

    [Fact]
    public void VectoredMode_TimerInterrupt_JumpsToBase_Plus28()
    {
        // Timer interrupt has cause number 7.
        // Handler address = BASE + 4 × 7 = BASE + 28.
        using var sut = Setup();

        sut.SetCSR(MTVEC_CSR, BASE | MODE_VECTORED);
        sut.SetCSR(MIE_CSR, 1u << MTIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MTIP_BIT);

        sut.RunTo(BASE + 28u);

        Assert.Equal(BASE + 28u, sut.Registers[32]);
    }

    [Fact]
    public void VectoredMode_SoftwareInterrupt_JumpsToBase_Plus12()
    {
        // Software interrupt has cause number 3.
        // Handler address = BASE + 4 × 3 = BASE + 12.
        using var sut = Setup();

        sut.SetCSR(MTVEC_CSR, BASE | MODE_VECTORED);
        sut.SetCSR(MIE_CSR, 1u << MSIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MSIP_BIT);

        sut.RunTo(BASE + 12u);

        Assert.Equal(BASE + 12u, sut.Registers[32]);
    }

    // ── Offset formula verification ──────────────────────────────────────────

    [Fact]
    public void VectoredMode_HandlerAddress_Is_Base_Plus_4_Times_CauseNumber()
    {
        // Generic check of the formula BASE + 4 × cause.
        // Also verifies that the MODE bits are masked out of BASE when
        // computing the destination (BASE | MODE_VECTORED = BASE + 1,
        // but EnterTrap strips them: baseAddr = mtvec & ~0x3u = BASE).
        using var sut = Setup();

        sut.SetCSR(MTVEC_CSR, BASE | MODE_VECTORED);
        sut.SetCSR(MIE_CSR, 1u << MTIP_BIT);
        sut.SetCSR(MIP_CSR, 1u << MTIP_BIT);

        sut.RunTo(BASE + (4u * 7u)); // timer, cause = 7 → BASE + 28

        Assert.Equal(BASE + (4u * 7u), sut.Registers[32]);
    }
}
