using System;
using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class CSSFormatFactory : CFormatFactoryBase
{
    public override uint ForQuadrant => 0b10;

    public override uint[] ForFunct3 => [0b110];

    public override FormatBase Decode(uint funct3, uint instruction)
    {
        var rs2 = instruction.ExtractBits(2, 5);

        uint uimm = 0;
        uimm |= (instruction >> 8 & 0x1) << 5;
        uimm |= (instruction >> 7 & 0x1) << 4;
        uimm |= (instruction >> 11 & 0x1) << 3;
        uimm |= (instruction >> 10 & 0x1) << 2;
        uimm |= (instruction >> 9 & 0x1) << 1;
        uimm |= 0;
        var offset = uimm << 2; // scaled by 4

        switch (funct3)
        {
            case 0b110:
                // C.SWSP -> sw rs2, offset(x2)
                // TODO: Verify
                return new SFormat
                {
                    Mnemonic = SFormat.sw,
                    imm = (int)offset,
                    rs2 = rs2,
                    rs1 = 2,
                    StepSize = 2,
                };
        }

        throw new NotImplementedException($"Unknown funct3:{funct3.ToBin(3)} in CSSFormat. Instruction: {instruction.ToHex(4)}");
    }
}
