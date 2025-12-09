using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Venture.Processor;

public class Hazard3Processor
{
    private readonly IDecoder decoder = new CDecoder(new Decoder());
    private readonly ILogger<Hazard3Processor> logger;
    private bool m_IsPCModified = false;

    public IBusFabric Memory { get; }
    public Registers Registers { get; }
    public CSR CSR { get; } = new CSR();

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
        get;
        set
        {
            m_IsPCModified = true;
            field = value;
        }
    }

    public Hazard3Processor(IBusFabric memory, ILogger<Hazard3Processor> logger, Registers registers)
    {
        Memory = memory;
        this.logger = logger;
        Registers = registers;
    }

    public void Step()
    {
        using var activity = Program.ActivitySource.StartActivity("Instruction");
        activity?.AddTag("PC", PC.ToHex());

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
