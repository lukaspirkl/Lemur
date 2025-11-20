using System;
using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class ZcbExtensionFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b00];

    public override uint[] ForFunct3 => [0b100];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        var rs1 = instruction.ExtractBits(7, 3) + 8;
        var rd = instruction.ExtractBits(2, 3) + 8;

        var funct6 = instruction.ExtractBits(10, 6);

        switch (funct6)
        {
            case 0b100001:

                if (instruction.ExtractBits(6, 1) == 0b0)
                {
                    // C.LHU -> lhu rd, offset(rs1)

                    uint offset = 0;
                    offset |= (instruction >> 5 & 0x1) << 1;

                    return new IFormat
                    {
                        Mnemonic = IFormat.lhu,
                        imm = (int)offset,
                        rd = rd,
                        rs1 = rs1,
                        StepSize = 2,
                    };
                }
                else
                {
                    throw new NotImplementedException("C.LH");
                }
        }

        return null;
    }
}
