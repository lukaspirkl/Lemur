using Microsoft.Extensions.Logging;

namespace Venture.Processor;

public class Hazard3Processor
{
    private readonly IDecoder decoder = new CDecoder(new Decoder());
    private readonly IEmuLogger<Hazard3Processor> logger;
    private bool m_IsPCModified = false;

    public IBusFabric Memory { get; }
    public Registers Registers { get; }
    public CSR CSR { get; }

    public event EventHandler? EBreak;

    public void RaiseEBreak()
    {
        EBreak?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ECall;

    public void RaiseECall()
    {
        ECall?.Invoke(this, EventArgs.Empty);
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

    public Hazard3Processor(IBusFabric memory, IEmuLogger<Hazard3Processor> logger, Registers registers, CSR csr)
    {
        Memory = memory;
        this.logger = logger;
        Registers = registers;
        CSR = csr;
    }

    public void Step()
    {
        using (logger.BeginScope("PC: {PC}", PC.ToHex()))
        {
            var currentPC = PC;
            try
            {
                var instruction = Memory.ReadInstruction(PC);

                var format = decoder.Decode(instruction);

                using (logger.BeginScope("Execute {instruction} {mnemonic}", instruction.ToHex(), format.Mnemonic))
                {
                    logger.LogInstructionExecute(PC, instruction, format.Mnemonic);

                    m_IsPCModified = false;
                    format.Execute(this);
                    if (!m_IsPCModified)
                    {
                        PC = PC + format.StepSize;
                    }
                }
            }
            catch (RiscVException e)
            {
                CSR.Set(0x341, currentPC); // MEPC
                CSR.Set(0x342, (uint)e.Cause); // MCAUSE 
                CSR.Set(0x343, e.TrapValue); // MTVAL // TODO: This is hardwired to zero on Hazard3 (it should be configurable)
                PC = CSR.Get(0x305); // MTVEC
            }
        }
    }
}
