namespace Venture.InstructionFormats;

public class STypeInstructionFormat : InstructionFormatBase
{
    public int imm_s { get; }

    public STypeInstructionFormat(uint instruction) 
        : base(instruction)
    {
        imm_s = (int)((uint)((int)instruction >> 25 << 5) | rd);
    }
}