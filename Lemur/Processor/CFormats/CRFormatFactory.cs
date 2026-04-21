using Lemur.Processor.Formats;

namespace Lemur.Processor.CFormats;

public class CRFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b10];

    public override uint[] ForFunct3 => [0b100];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        var rs1 = instruction.ExtractBits(7, 5);
        var rd = rs1;
        var rs2 = instruction.ExtractBits(2, 5);
        var funct4 = instruction.ExtractBits(12, 4);
        
        switch (funct4)
        {
            case 0b1000:
                if (rs1 != 0 && rs2 == 0)
                {
                    // C.JR -> jalr x0, 0(rs1)
                    return new IFormat
                    {
                        Mnemonic = IFormat.JALR,
                        imm = 0,
                        rd = 0,
                        rs1 = rs1,
                        StepSize = 2,
                    };
                }

                if (rs1 != 0 && rs2 != 0)
                {
                    // C.MV -> add rd, x0, rs2
                    return new RFormat
                    {
                        Mnemonic = RFormat.ADD,
                        rd = rd,
                        rs1 = 0,
                        rs2 = rs2,
                        StepSize = 2,
                    };
                }

                if (rs1 == 0 && rs2 != 0)
                {
                    // Hints
                    return m_Nop;
                }

                return null;

            case 0b1001:
                if (rs1 == 0 && rs2 == 0)
                {
                    // C.EBREAK
                    //throw new NotImplementedException("C.EBREAK");
                    return new IFormat
                    {
                        Mnemonic = IFormat.EBREAK,
                        rd = 0,
                        imm = 0,
                        rs1 = 0,
                        StepSize = 2,
                    };
                }

                if (rs1 != 0 && rs2 == 0)
                {
                    // C.JALR -> jalr x1, 0(rs1)
                    return new IFormat
                    {
                        Mnemonic = IFormat.JALR,
                        imm = 0,
                        rd = 1,
                        rs1 = rs1,
                        StepSize = 2,
                    };
                }

                if (rs1 != 0 && rs2 != 0)
                {
                    // C.ADD -> add rd, rd, rs2
                    return new RFormat
                    {
                        Mnemonic = RFormat.ADD,
                        rd = rd,
                        rs1 = rd,
                        rs2 = rs2,
                        StepSize = 2,
                    };
                }

                if (rs1 == 0 && rs2 != 0)
                {
                    // Hints
                    return m_Nop;
                }

                return null;
        }

        return null;
    }
}
