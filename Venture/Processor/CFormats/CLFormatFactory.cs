using System;
using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class CLFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b00];

    public override uint[] ForFunct3 => [0b010];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        var rs1 = instruction.ExtractBits(7, 3) + 8;
        var rd = instruction.ExtractBits(2, 3) + 8;

        switch (funct3)
        {
            case 0b010:
                // C.LW -> lw rd′, offset(rs1′)

                uint offset = 0;
                offset |= (instruction >> 5 & 0x1) << 6;
                offset |= (instruction >> 12 & 0x1) << 5;
                offset |= (instruction >> 11 & 0x1) << 4;
                offset |= (instruction >> 10 & 0x1) << 3;
                offset |= (instruction >> 6 & 0x1) << 2;

                return new IFormat
                {
                    Mnemonic = IFormat.lw,
                    imm = (int)offset,
                    rd = rd,
                    rs1 = rs1,
                    StepSize = 2,
                };
        }

        return null;
    }
}
