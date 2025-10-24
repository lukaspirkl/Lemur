namespace Venture.InstructionFormats;

public class ITypeInstructionFormat : InstructionFormatBase
{
    public int imm_i_signed { get; }
    public uint imm_i_unsigned { get; }

    public uint shamt_i { get; }

    public ushort csr => (ushort)imm_i_signed;

    public ITypeInstructionFormat(uint instruction) 
        : base(instruction)
    {
        imm_i_signed = (int)instruction >> 20; // Sign is preserved when shifting signed int
        imm_i_unsigned = instruction >> 20; // Left part is zeroed when shifting unsigned int

        shamt_i = instruction.ExtractBits(20, 5);
    }
}

