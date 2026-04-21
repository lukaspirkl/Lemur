namespace Lemur.Processor.Formats;

public class JFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b1101111];

    public override FormatBase? Decode(uint instruction)
    {
        var opcode = GetOpcode(instruction);

        uint imm_11 = instruction >> 20 & 0x01; // Bit 20 -> imm[11]
        uint imm_20 = instruction >> 31 & 0x01; // Bit 31 -> imm[20]
        uint imm_10_1 = instruction >> 21 & 0x3FF; // Bits 30-21 -> imm[10:1]
        uint imm_19_12 = instruction >> 12 & 0xFF;  // Bits 19-12 -> imm[19:12]
        uint unsigned_imm_j = imm_20 << 20    // imm[20]
                   | imm_19_12 << 12 // imm[19:12]
                   | imm_11 << 11    // imm[11]
                   | imm_10_1 << 1;  // imm[10:1] (shifted left by 1 for imm[0]=0)

        var imm_j = (int)unsigned_imm_j;
        if (imm_20 == 1)
        {
            // Extend the sign bit from bit 20 upwards
            imm_j |= unchecked((int)0xFFE00000); // Mask for bits 31 down to 21
        }

        return new JFormat
        {
            Mnemonic = JFormat.JAL,
            imm = imm_j,
            rd = instruction.ExtractBits(7, 5),
        };
    }
}

public class JFormat : FormatBase
{
    public const string JAL = "jal";

    public required uint rd { get; init; }
    public required int imm { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        x[rd] = e.PC + StepSize;
        e.PC = (uint)(e.PC + imm);
    }
}
