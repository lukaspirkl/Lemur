namespace Venture;

public interface IEmulator
{
    IMemory Memory { get; }
    Registers Registers { get; }
    Dictionary<ushort, uint> CSR { get; }
    uint PC { get; set; }
    public void AddInstructionSet(Action<IInstructionSetBuider> factory);
}

public class Emulator : IEmulator
{
    private readonly InstructionSetCollection instructionSetCollection = new InstructionSetCollection();

    public IMemory Memory { get; }
    public Registers Registers { get; }
    public Dictionary<ushort, uint> CSR { get; } = new Dictionary<ushort, uint>();

    private bool m_IsPCModified = false;
    private uint m_PC;

    public uint PC
    {
        get { return m_PC; }
        set
        {
            m_IsPCModified = true;
            m_PC = value;
        }
    }

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

        m_IsPCModified = false;

        instructionSetCollection.Execute(instruction);

        if (!m_IsPCModified)
        {
            PC = PC + 4;
        }
    }

    public void AddInstructionSet(Action<IInstructionSetBuider> factory)
    {
        factory(instructionSetCollection);
    }
}
