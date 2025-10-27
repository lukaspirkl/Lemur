namespace Venture;

public interface IEmulator
{
    IMemory Memory { get; }
    Registers Registers { get; }
    Dictionary<ushort, uint> CSR { get; }
    uint PC { get; set; }
    Dictionary<uint, Func<uint, bool>> Instructions { get; }
}

public class Emulator : IEmulator
{
    public IMemory Memory { get; }
    public Registers Registers { get; }
    public Dictionary<ushort, uint> CSR { get; } = new Dictionary<ushort, uint>();
    public uint PC { get; set; }
    public Dictionary<uint, Func<uint, bool>> Instructions { get; } = new();

    public Emulator(IMemory memory)
    {
        Memory = memory;
        Registers = new Registers();
        PC = memory.InitialPC;
    }

    public void Step()
    {
        var instruction = Memory.ReadWord(PC);
        Console.WriteLine($"PC: {PC.ToHex()} Instruction: {instruction.ToHex()} {instruction.ToBin()}");
        //Console.WriteLine($"registers[5] {registers[5].ToHex()}");

        if (ExecuteInstruction(instruction))
        {
            PC = PC + 4;
        }
    }

    public bool ExecuteInstruction(uint instruction)
    {
        var opcode = instruction.ExtractBits(0, 7);

        if (Instructions.TryGetValue(opcode, out var func))
        {
            return func(instruction);
        }

        throw new NotImplementedException($"Instruction {instruction.ToHex()} not implemented.");
    }
}
