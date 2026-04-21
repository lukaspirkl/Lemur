using Lemur;

namespace Tests.Processor;

public class TrapTests
{
    private const uint SRAM = 0x20000000;

    private const ushort MSTATUS = 0x300;
    private const ushort MTVEC = 0x305;
    private const ushort MEPC = 0x341;
    private const ushort MCAUSE = 0x342;

    private const uint TRAP_HANDLER = SRAM + 0x100;

    
    [Fact(Skip = "For manual experiments")]
    public void First()
    {
        using var sut = RP2350Builder.Create();

        Console.WriteLine($"MSTATUS: {sut.GetCSR(MSTATUS).ToHex()}");
        Console.WriteLine($"MCAUSE: {sut.GetCSR(MCAUSE).ToHex()}");
        Console.WriteLine($"MEPC: {sut.GetCSR(MEPC).ToHex()}");
        Console.WriteLine($"MTVEC: {sut.GetCSR(MTVEC).ToHex()}");

        sut.SetCSR(MTVEC, TRAP_HANDLER);
        sut.MemoryWrite(TRAP_HANDLER, InstructionBuilder.MRET());

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = SRAM + 0x2;

        sut.MemoryWrite(SRAM, InstructionBuilder.LW(1, 2));
        sut.Registers[32] = SRAM;

        Console.WriteLine("STEP");
        sut.Step();

        Console.WriteLine($"MSTATUS: {sut.GetCSR(MSTATUS).ToHex()}");
        Console.WriteLine($"MCAUSE: {sut.GetCSR(MCAUSE).ToHex()}");
        Console.WriteLine($"MEPC: {sut.GetCSR(MEPC).ToHex()}");
        Console.WriteLine($"MTVEC: {sut.GetCSR(MTVEC).ToHex()}");
        Console.WriteLine($"PC: {sut.Registers[32].ToHex()}");

        Console.WriteLine("STEP");
        sut.Step();

        Console.WriteLine($"MSTATUS: {sut.GetCSR(MSTATUS).ToHex()}");
        Console.WriteLine($"MCAUSE: {sut.GetCSR(MCAUSE).ToHex()}");
        Console.WriteLine($"MEPC: {sut.GetCSR(MEPC).ToHex()}");
        Console.WriteLine($"MTVEC: {sut.GetCSR(MTVEC).ToHex()}");
        Console.WriteLine($"PC: {sut.Registers[32].ToHex()}");
    }

    [Fact(Skip = "For manual experiments")]
    public void Second()
    {
        using var sut = RP2350Builder.Create();

        sut.SetCSR(MTVEC, TRAP_HANDLER);
        sut.MemoryWrite(TRAP_HANDLER, InstructionBuilder.MRET());

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = SRAM + 0x2;

        sut.MemoryWrite(SRAM, InstructionBuilder.EBREAK());
        sut.Registers[32] = SRAM;

        Console.WriteLine($"MSTATUS: {sut.GetCSR(MSTATUS).ToHex()}");
        Console.WriteLine($"MCAUSE: {sut.GetCSR(MCAUSE).ToHex()}");
        Console.WriteLine($"MEPC: {sut.GetCSR(MEPC).ToHex()}");
        Console.WriteLine($"MTVEC: {sut.GetCSR(MTVEC).ToHex()}");
        Console.WriteLine($"PC: {sut.Registers[32].ToHex()}");

        Console.WriteLine("STEP");
        sut.Step();

        Console.WriteLine($"MSTATUS: {sut.GetCSR(MSTATUS).ToHex()}");
        Console.WriteLine($"MCAUSE: {sut.GetCSR(MCAUSE).ToHex()}");
        Console.WriteLine($"MEPC: {sut.GetCSR(MEPC).ToHex()}");
        Console.WriteLine($"MTVEC: {sut.GetCSR(MTVEC).ToHex()}");
        Console.WriteLine($"PC: {sut.Registers[32].ToHex()}");

        Console.WriteLine("STEP");
        sut.Step();

        Console.WriteLine($"MSTATUS: {sut.GetCSR(MSTATUS).ToHex()}");
        Console.WriteLine($"MCAUSE: {sut.GetCSR(MCAUSE).ToHex()}");
        Console.WriteLine($"MEPC: {sut.GetCSR(MEPC).ToHex()}");
        Console.WriteLine($"MTVEC: {sut.GetCSR(MTVEC).ToHex()}");
        Console.WriteLine($"PC: {sut.Registers[32].ToHex()}");
    }
}
