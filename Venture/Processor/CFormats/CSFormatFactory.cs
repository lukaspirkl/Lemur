using System;
using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class CSFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b00];

    public override uint[] ForFunct3 => [0b110];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        var rs1 = instruction.ExtractBits(7, 3) + 8;
        var rs2 = instruction.ExtractBits(2, 3) + 8;

        switch (funct3)
        {
            case 0b110:
                // C.SW -> sw rs2′, offset(rs1′)

                uint offset = 0;
                offset |= (instruction >> 5 & 0x1) << 6;
                offset |= (instruction >> 12 & 0x1) << 5;
                offset |= (instruction >> 11 & 0x1) << 4;
                offset |= (instruction >> 10 & 0x1) << 3;
                offset |= (instruction >> 6 & 0x1) << 2;

                return new SFormat
                {
                    Mnemonic = SFormat.SW,
                    imm = (int)offset,
                    rs2 = rs2,
                    rs1 = rs1,
                    StepSize = 2,
                };
        }

        return null;
    }
}
