using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class CJFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b01];

    public override uint[] ForFunct3 => [0b101, 0b001];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        uint uoffset = 0;
        uoffset |= (instruction >> 12 & 0x1) << 11;
        uoffset |= (instruction >> 8 & 0x1) << 10;
        uoffset |= (instruction >> 10 & 0x1) << 9;
        uoffset |= (instruction >> 9 & 0x1) << 8;
        uoffset |= (instruction >> 6 & 0x1) << 7;
        uoffset |= (instruction >> 7 & 0x1) << 6;
        uoffset |= (instruction >> 2 & 0x1) << 5;
        uoffset |= (instruction >> 11 & 0x1) << 4;
        uoffset |= (instruction >> 5 & 0x1) << 3;
        uoffset |= (instruction >> 4 & 0x1) << 2;
        uoffset |= (instruction >> 3 & 0x1) << 1;

        int offset = ((int)uoffset).SignExtend(12);
        
        switch (funct3)
        {
            case 0b001:

                // C.JAL -> jal x1, offset
                return new JFormat
                {
                    Mnemonic = JFormat.jal,
                    rd = 1,
                    imm = offset,
                    StepSize = 2,
                };

            case 0b101:
                // C.J -> jal x0, offset
                return new JFormat
                {
                    Mnemonic = JFormat.jal,
                    rd = 0,
                    imm = offset,
                    StepSize = 2,
                };

        }

        return null;
    }
}
