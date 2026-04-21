namespace Lemur.Processor.Formats;

public abstract class FormatFactoryBase
{
    public abstract uint[] ForOpcodes { get; }

    public abstract FormatBase? Decode(uint instruction);

    protected uint GetOpcode(uint instruction)
    {
        return instruction.ExtractBits(0, 7);
    }
}

public abstract class FormatBase
{
    public required string Mnemonic { get; init; }

    public abstract void Execute(Hazard3Processor e);

    public uint StepSize { get; set; } = 4;
}

