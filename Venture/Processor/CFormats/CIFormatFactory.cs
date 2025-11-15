using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class CIFormatFactory : CFormatFactoryBase
{
    public override uint ForQuadrant => 0b01;

    public override uint[] ForFunct3 => [0b000, 0b010, 0b011];

    public override FormatBase Decode(uint funct3, uint instruction)
    {
        var imm5 = instruction.ExtractBits(12, 1);
        var imm = (int)(instruction.ExtractBits(2, 5) | (imm5 << 5));
        if (imm5 == 1)
        {
            // Extend the sign bit
            imm |= unchecked((int)0xFFFF_FFFF << 6);
        }

        var rd = instruction.ExtractBits(7, 5);

        switch (funct3)
        {
            case 0b000:
                if (rd != 0 && imm != 0)
                {
                    // C.ADDI -> addi rd, rd, imm
                    return new IFormat
                    {
                        Mnemonic = IFormat.addi,
                        rd = rd,
                        rs1 = rd,
                        imm = imm,
                        StepSize = 2,
                    };
                }

                if (rd == 0)
                {
                    // C.NOP
                    return nop;
                }

                // Reserved for hints - no-op
                return nop;

            case 0b010:
                if (rd == 0)
                {
                    // Reserved for hints - no-op
                    return nop;
                }

                // C.LI -> addi rd, x0, imm
                return new IFormat
                {
                    Mnemonic = IFormat.addi,
                    rd = rd,
                    rs1 = 0,
                    imm = imm,
                    StepSize = 2,
                };

            case 0b011:

                if (rd == 0)
                {
                    // Reserved for hints - no-op
                    return nop;
                }

                if (rd == 2)
                {
                    // C.ADDI16SP
                    throw new NotImplementedException("C.ADDI16SP");
                }

                // C.LUI -> lui rd, imm
                return new UFormat
                {
                    Mnemonic = UFormat.lui,
                    rd = rd,
                    imm = (uint)imm << 12,
                    StepSize = 2,
                };

        }

        throw new NotImplementedException($"Unknown funct3:{funct3.ToBin(3)} in CIFormat. Instruction: {instruction.ToHex(4)}");
    }
}
