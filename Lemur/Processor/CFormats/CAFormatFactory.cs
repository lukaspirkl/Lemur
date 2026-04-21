using Lemur.Processor.Formats;
using System;

namespace Lemur.Processor.CFormats;

public class CAFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b01];

    public override uint[] ForFunct3 => [0b100];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        var rs1 = instruction.ExtractBits(7, 3) + 8;
        var rd = rs1;
        var rs2 = instruction.ExtractBits(2, 3) + 8;
        var funct2 = instruction.ExtractBits(5, 2);
        var funct6 = instruction.ExtractBits(10, 6);

        switch (funct6)
        {
            case 0b100011:
                switch(funct2)
                {
                    case 0b00:
                        // C.SUB -> sub rd′, rd′, rs2′
                        return new RFormat
                        { 
                            Mnemonic = RFormat.SUB,
                            rd = rd,
                            rs1 = rs1,
                            rs2 = rs2,
                            StepSize = 2,
                        };
                    case 0b01:
                        // C.XOR -> xor rd′, rd′, rs2′
                        return new RFormat
                        {
                            Mnemonic = RFormat.XOR,
                            rd = rd,
                            rs1 = rs1,
                            rs2 = rs2,
                            StepSize = 2,
                        };
                    case 0b10:
                        // C.OR -> or rd′, rd′, rs2′
                        return new RFormat
                        {
                            Mnemonic = RFormat.OR,
                            rd = rd,
                            rs1 = rs1,
                            rs2 = rs2,
                            StepSize = 2,
                        };
                    case 0b11:
                        // C.AND -> and rd′, rd′, rs2′
                        return new RFormat
                        {
                            Mnemonic = RFormat.AND,
                            rd = rd,
                            rs1 = rs1,
                            rs2 = rs2,
                            StepSize = 2,
                        };
                }

                return null;

            case 0b100111:
                switch (funct2)
                {
                    case 0b00:
                        throw new NotImplementedException("C.SUBW");
                    case 0b01:
                        throw new NotImplementedException("C.ADDW");
                    case 0b10:
                        return null; // reserved
                    case 0b11:
                        return null; // reserved
                }

                return null;

        }

        return null;
    }
}
