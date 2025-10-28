namespace Venture.InstructionFormats;

public class InstructionFormatBase
{
    public uint instruction { get; }
    public uint opcode { get; }
    public uint rd { get; }
    public uint funct3 { get; }
    public uint rs1 { get; }
    public uint rs2 { get; }


    public InstructionFormatBase(uint instruction)
    {
        this.instruction = instruction;
        opcode = instruction.ExtractBits(0, 7);
        rd = instruction.ExtractBits(7, 5);
        funct3 = instruction.ExtractBits(12, 3);
        rs1 = instruction.ExtractBits(15, 5);
        rs2 = instruction.ExtractBits(20, 5);
    }

    public override string ToString()
    {
        return $"opcode: {opcode.ToBin(7)} rd: {rd.ToBin(5)} funct3: {funct3.ToBin(3)} rs1: {rs1.ToBin(5)} rs2: {rs2.ToBin(5)}";
    }
}
