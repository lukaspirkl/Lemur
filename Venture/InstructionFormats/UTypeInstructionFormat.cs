namespace Venture.InstructionFormats;

public class UTypeInstructionFormat : InstructionFormatBase
{
    public uint imm_u { get; }

    public UTypeInstructionFormat(uint instruction) 
        : base(instruction)
    {
        imm_u = instruction & 0xfffff000;
    }
}