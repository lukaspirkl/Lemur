namespace Venture.InstructionFormats;

public class RTypeInstructionFormat : InstructionFormatBase
{
    public static RTypeInstructionFormat Parse(uint instruction)
    {
        return new RTypeInstructionFormat(instruction);
    }

    public uint funct7 { get; }

    public RTypeInstructionFormat(uint instruction)
        : base(instruction)
    {
        funct7 = instruction.ExtractBits(25, 7);
    }
}