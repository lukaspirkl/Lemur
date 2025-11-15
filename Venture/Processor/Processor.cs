namespace Venture.Processor;

public interface IProcessor
{
    IMemory Memory { get; }
    Registers Registers { get; }
    Dictionary<ushort, uint> CSR { get; }
    uint PC { get; set; }

    event EventHandler? EBreak;
    event EventHandler<ECallEventArgs>? ECall;

    void RaiseEBreak();
    void RaiseECall(uint serviceNumber, uint argument);
}

public class Processor : IProcessor
{
    private readonly IDecoder decoder = new CDecoder(new Decoder());

    public IMemory Memory { get; }
    public Registers Registers { get; }
    public Dictionary<ushort, uint> CSR { get; } = new Dictionary<ushort, uint>();

    private bool m_IsPCModified = false;
    private uint m_PC;

    public event EventHandler? EBreak;

    public void RaiseEBreak()
    {
        EBreak?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<ECallEventArgs>? ECall;

    public void RaiseECall(uint serviceNumber, uint argument)
    {
        ECall?.Invoke(this, new ECallEventArgs(serviceNumber, argument));
    }

    public uint PC
    {
        get { return m_PC; }
        set
        {
            m_IsPCModified = true;
            m_PC = value;
        }
    }

    public Processor(IMemory memory)
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

        var format = decoder.Decode(instruction);
        
        m_IsPCModified = false;
        format.Execute(this);
        if (!m_IsPCModified)
        {
            PC = PC + format.StepSize;
        }
        
    }
}


public class ECallEventArgs : EventArgs
{
    public uint ServiceNumber { get; }
    public uint Argument { get; }

    public ECallEventArgs(uint serviceNumber, uint argument)
    {
        ServiceNumber = serviceNumber;
        Argument = argument;
    }
}