using System;
using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class CSFormatFactory : CFormatFactoryBase
{
    public override uint ForQuadrant => 0b00;

    public override uint[] ForFunct3 => [0b110];

    public override FormatBase Decode(uint funct3, uint instruction)
    {
        // Compressed instructions are using only registers x8–x15
        var rs1 = instruction.ExtractBits(7, 3) + 8;
        var rs2 = instruction.ExtractBits(2, 3) + 8;

        uint uimm = 0;
        uimm |= (instruction >> 12 & 0x1) << 5;
        uimm |= (instruction >> 6 & 0x1) << 4;
        uimm |= (instruction >> 5 & 0x1) << 3;
        uimm |= (instruction >> 11 & 0x1) << 2;
        uimm |= (instruction >> 10 & 0x1) << 1;
        uimm |= 0;               // implicit low bit
        var offset = uimm << 2;  // because C.SW uses word offset

        switch (funct3)
        {
            case 0b110:
                // C.SW -> sw rs2, offset(rs1)
                // TODO: Verify
                return new SFormat
                {
                    Mnemonic = SFormat.sw,
                    imm = (int)offset,
                    rs2 = rs2,
                    rs1 = rs1,
                    StepSize = 2,
                };
        }

        throw new NotImplementedException($"Unknown funct3:{funct3.ToBin(3)} in CSFormat. Instruction: {instruction.ToHex(4)}");
    }
}
