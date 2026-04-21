using Lemur.Processor.Formats;

namespace Lemur.Processor.CFormats;

public class CBFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b01];

    public override uint[] ForFunct3 => [0b100, 0b110, 0b111];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        var rs1 = instruction.ExtractBits(7, 3) + 8;
        var rd = rs1;
        var funct2 = instruction.ExtractBits(10, 2);

        uint uimm = 0;
        uimm |= instruction.ExtractBits(2, 5);
        uimm |= instruction.ExtractBits(12, 1) << 5;

        uint uoffset = 0;
        uoffset |= (instruction >> 12 & 0x1) << 8;
        uoffset |= (instruction >> 6 & 0x1) << 7;
        uoffset |= (instruction >> 5 & 0x1) << 6;
        uoffset |= (instruction >> 2 & 0x1) << 5;
        uoffset |= (instruction >> 11 & 0x1) << 4;
        uoffset |= (instruction >> 10 & 0x1) << 3;
        uoffset |= (instruction >> 4 & 0x1) << 2;
        uoffset |= (instruction >> 3 & 0x1) << 1;
        int offset = ((int)uoffset).SignExtend(9);

        switch (funct3)
        {
            case 0b100:

                switch (funct2)
                {
                    case 0b00:
                        // C.SRLI -> srli rd′, rd′, shamt
                        return new IFormat
                        {
                            Mnemonic = IFormat.SRLI,
                            rd = rd,
                            rs1 = rd,
                            imm = (int)uimm,
                            StepSize = 2,
                        };

                    case 0b01:
                        // C.SRAI ->  srai rd′, rd′, shamt
                        return new IFormat
                        {
                            Mnemonic = IFormat.SRAI,
                            rd = rd,
                            rs1 = rd,
                            imm = (int)uimm,
                            StepSize = 2,
                        };

                    case 0b10:
                        // C.ANDI -> andi rd′, rd′, imm
                        return new IFormat
                        {
                            Mnemonic = IFormat.ANDI,
                            rd = rd,
                            rs1 = rd,
                            imm = ((int)uimm).SignExtend(6),
                            StepSize = 2,
                        };
                }

                return null;

            case 0b110:
                // C.BEQZ -> beq rs1′, x0, offset
                return new BFormat
                {
                    Mnemonic = BFormat.BEQ,
                    rs1 = rs1,
                    rs2 = 0,
                    imm_b = offset,
                    StepSize = 2,
                };

            case 0b111:
                // C.BNEZ -> bne rs1′, x0, offset
                return new BFormat
                {
                    Mnemonic = BFormat.BNE,
                    rs1 = rs1,
                    rs2 = 0,
                    imm_b = offset,
                    StepSize = 2,
                };
        }

        return null;
    }
}
