using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Venture.Processor;

public interface IProcessor
{
    IMemory Memory { get; }
    Registers Registers { get; }
    CSR CSR { get; }
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
    public CSR CSR { get; } = new CSR();

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
        Console.WriteLine($"");
        Console.WriteLine($"PC: {PC.ToHex()}");

        var instruction = Memory.ReadWord(PC);

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