using System;
using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class ZcbExtensionFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b00, 0b01];

    public override uint[] ForFunct3 => [0b100];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        var rs1 = instruction.ExtractBits(7, 3) + 8;
        var rs2 = instruction.ExtractBits(2, 3) + 8;

        var funct2 = instruction.ExtractBits(5, 2);
        var funct6 = instruction.ExtractBits(10, 6);

        switch (instruction.ExtractBits(0, 2))
        {
            case 0b00:
                switch (funct6)
                {
                    case 0b100000:
                        {
                            // C.LBU -> lbu rd', uimm(rs1')
                            uint uimm = 0;
                            uimm |= (instruction >> 5 & 0x1) << 1;
                            uimm |= (instruction >> 6 & 0x1) << 0;

                            return new IFormat
                            {
                                Mnemonic = IFormat.LBU,
                                rd = rs2,
                                rs1 = rs1,
                                imm = (int)uimm,
                                StepSize = 2,
                            };
                        }
                    case 0b100001:
                        {
                            uint uimm = 0;
                            uimm |= (instruction >> 5 & 0x1) << 1;

                            if (instruction.ExtractBits(6, 1) == 0b0)
                            {
                                // C.LHU -> lhu rd', uimm(rs1')
                                return new IFormat
                                {
                                    Mnemonic = IFormat.LHU,
                                    imm = (int)uimm,
                                    rd = rs2,
                                    rs1 = rs1,
                                    StepSize = 2,
                                };
                            }
                            else
                            {
                                // C.LH -> lh rd', uimm(rs1')
                                return new IFormat
                                {
                                    Mnemonic = IFormat.LH,
                                    imm = (int)uimm,
                                    rd = rs2,
                                    rs1 = rs1,
                                    StepSize = 2,
                                };
                            }
                        }
                    case 0b100010:
                        {
                            // C.SB -> sb rs2', uimm(rs1')
                            uint uimm = 0;
                            uimm |= (instruction >> 5 & 0x1) << 1;
                            uimm |= (instruction >> 6 & 0x1) << 0;

                            return new SFormat
                            {
                                Mnemonic = SFormat.SB,
                                rs1 = rs1,
                                rs2 = rs2,
                                imm = (int)uimm,
                                StepSize = 2,
                            };
                        }
                    case 0b100011:
                        {
                            uint uimm = 0;
                            uimm |= (instruction >> 5 & 0x1) << 1;

                            if (instruction.ExtractBits(6, 1) == 0b0)
                            {
                                // C.SH -> sh rs2', uimm(rs1')
                                return new SFormat
                                {
                                    Mnemonic = SFormat.SH,
                                    imm = (int)uimm,
                                    rs1 = rs1,
                                    rs2 = rs2,
                                    StepSize = 2,
                                };
                            }
                            else
                            {
                                return null;
                            }
                        }
                }
                break;
            case 0b01:
                switch (funct6)
                {
                    case 0b100111:
                        switch (funct2)
                        {
                            case 0b10:
                                // C.MUL -> mul rd', rd', rs2
                                return new MExtensionFormat
                                {
                                    Mnemonic = MExtensionFormat.MUL,
                                    rd = rs1,
                                    rs1 = rs1,
                                    rs2 = rs2,
                                    StepSize = 2,
                                };
                            case 0b11:
                                switch (rs2-8)
                                {
                                    case 0b000:
                                        // C.ZEXT.B -> andi rd', rd', 0xFF
                                        return new IFormat
                                        {
                                            Mnemonic = IFormat.ANDI,
                                            rd = rs1,
                                            rs1 = rs1,
                                            imm = 0xFF,
                                            StepSize = 2,
                                        };
                                    case 0b001:
                                        // C.SEXT.B -> sext.b rd', rd'
                                        return new BExtensionFormat
                                        {
                                            Mnemonic = BExtensionFormat.SEXT_B,
                                            rd = rs1,
                                            rs1 = rs1,
                                            rs2 = rs1,
                                            StepSize = 2,
                                        };
                                    case 0b010:
                                        // C.ZEXT.H -> pack rd', rd', 0
                                        return new BExtensionFormat
                                        {
                                            Mnemonic = BExtensionFormat.PACK,
                                            rd = rs1,
                                            rs1 = rs1,
                                            rs2 = 0,
                                            StepSize = 2,
                                        };
                                    case 0b011:
                                        // C.SEXT.H -> sext.h rd', rd'
                                        return new BExtensionFormat
                                        {
                                            Mnemonic = BExtensionFormat.SEXT_H,
                                            rd = rs1,
                                            rs1 = rs1,
                                            rs2 = rs1,
                                            StepSize = 2,
                                        };
                                    case 0b101:
                                        // C.NOT -> xori rd', rd', -1
                                        return new IFormat
                                        {
                                            Mnemonic = IFormat.XORI,
                                            rd = rs1,
                                            rs1 = rs1,
                                            imm = -1,
                                            StepSize = 2,
                                        };

                                }
                                break;
                        }
                        break;
                }
                break;
        }
        return null;
    }
}
