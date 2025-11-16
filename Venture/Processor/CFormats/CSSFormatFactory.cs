using System;
using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class CSSFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b10];

    public override uint[] ForFunct3 => [0b110];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        var rs2 = instruction.ExtractBits(2, 5);

        switch (funct3)
        {
            case 0b110:
                // C.SWSP -> sw rs2, offset(x2)

                uint offset = 0;
                offset |= (instruction >> 8 & 0x1) << 7;
                offset |= (instruction >> 7 & 0x1) << 6;
                offset |= (instruction >> 12 & 0x1) << 5;
                offset |= (instruction >> 11 & 0x1) << 4;
                offset |= (instruction >> 10 & 0x1) << 3;
                offset |= (instruction >> 9 & 0x1) << 2;

                return new SFormat
                {
                    Mnemonic = SFormat.sw,
                    imm = (int)offset,
                    rs2 = rs2,
                    rs1 = 2,
                    StepSize = 2,
                };
        }

        return null;
    }
}
