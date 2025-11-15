using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public abstract class CFormatFactoryBase
{
    public abstract uint ForQuadrant { get; }
    public abstract uint[] ForFunct3 { get; }

    public abstract FormatBase Decode(uint funct3, uint instruction);

    protected readonly IFormat nop = new IFormat
    {
        Mnemonic = IFormat.addi,
        imm = 0,
        rd = 0,
        rs1 = 0,
        StepSize = 2,
    };
}
