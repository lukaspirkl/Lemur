using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class CIFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b00, 0b01, 0b10];

    public override uint[] ForFunct3 => [0b000, 0b010, 0b011];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        return instruction.ExtractBits(0, 2) switch
        {
            0b00 => Decode00(funct3, instruction),
            0b01 => Decode01(funct3, instruction),
            0b10 => Decode10(funct3, instruction),
            _ => null,
        };
    }

    private FormatBase? Decode00(uint funct3, uint instruction)
    {
        switch (funct3)
        {
            case 0b000:
                // C.ADDI4SPN -> addi rd′, x2, nzuimm[9:2]

                var rd = instruction.ExtractBits(2, 3) + 8;

                uint nzuimm = 0;
                nzuimm |= (instruction >> 10 & 0x1) << 9;
                nzuimm |= (instruction >> 9 & 0x1) << 8;
                nzuimm |= (instruction >> 8 & 0x1) << 7;
                nzuimm |= (instruction >> 7 & 0x1) << 6;
                nzuimm |= (instruction >> 12 & 0x1) << 5;
                nzuimm |= (instruction >> 11 & 0x1) << 4;
                nzuimm |= (instruction >> 5 & 0x1) << 3;
                nzuimm |= (instruction >> 6 & 0x1) << 2;

                if (nzuimm != 0)
                {
                    return new IFormat
                    {
                        Mnemonic = IFormat.addi,
                        rd = rd,
                        rs1 = 2,
                        imm = (int)nzuimm,
                        StepSize = 2,
                    };
                }
                else
                {
                    throw new NotImplementedException("reserved");
                }
        }

        return null;
    }

    private FormatBase? Decode01(uint funct3, uint instruction)
    {
        uint uimm = instruction.ExtractBits(2, 5);
        uimm |= instruction.ExtractBits(12, 1) << 5;

        int imm = ((int)uimm).SignExtend(6);

        var rd = instruction.ExtractBits(7, 5);

        switch (funct3)
        {
            case 0b000:
                if (rd != 0 && imm != 0)
                {
                    // C.ADDI -> addi rd, rd, imm
                    return new IFormat
                    {
                        Mnemonic = IFormat.addi,
                        rd = rd,
                        rs1 = rd,
                        imm = imm,
                        StepSize = 2,
                    };
                }

                if (rd == 0)
                {
                    // C.NOP
                    return nop;
                }

                // Reserved for hints - no-op
                return nop;

            case 0b010:
                if (rd == 0)
                {
                    // Reserved for hints - no-op
                    return nop;
                }

                // C.LI -> addi rd, x0, imm
                return new IFormat
                {
                    Mnemonic = IFormat.addi,
                    rd = rd,
                    rs1 = 0,
                    imm = imm,
                    StepSize = 2,
                };

            case 0b011:

                if (rd == 0)
                {
                    // Reserved for hints - no-op
                    return nop;
                }

                if (rd == 2)
                {
                    // C.ADDI16SP -> addi x2, x2, nzimm[9:4]

                    uint nzimm = 0;
                    nzimm |= (instruction >> 12 & 0x1) << 9;
                    nzimm |= (instruction >> 4 & 0x1) << 8;
                    nzimm |= (instruction >> 3 & 0x1) << 7;
                    nzimm |= (instruction >> 5 & 0x1) << 6;
                    nzimm |= (instruction >> 2 & 0x1) << 5;
                    nzimm |= (instruction >> 6 & 0x1) << 4;

                    if (nzimm != 0)
                    {
                        return new IFormat
                        {
                            Mnemonic = IFormat.addi,
                            rd = 2,
                            rs1 = 2,
                            imm = ((int)nzimm).SignExtend(10),
                            StepSize = 2,
                        };
                    }
                    else
                    {
                        throw new NotImplementedException("reserved");
                    }
                }

                // C.LUI -> lui rd, imm
                return new UFormat
                {
                    Mnemonic = UFormat.lui,
                    rd = rd,
                    imm = (uint)imm << 12,
                    StepSize = 2,
                };

        }

        return null;
    }

    private FormatBase? Decode10(uint funct3, uint instruction)
    {
        var rd = instruction.ExtractBits(7, 5);

        switch (funct3)
        {
            case 0b000:
                if (rd != 0)
                {
                    // C.SLLI -> slli rd, rd, shamt[5:0]

                    uint shamt = 0;
                    shamt |= (instruction >> 2) & 0x1F;      // shamt[4:0] is in instruction bits [6:2]
                    shamt |= (instruction >> 12 & 0x1) << 5; // shamt[5] is in instruction bit [12]

                    return new IFormat
                    {
                        Mnemonic = IFormat.slli,
                        rd = rd,
                        rs1 = rd,
                        imm = (int)shamt,
                        StepSize = 2,
                    };
                }

                return null;

            case 0b010:
                if (rd != 0)
                {
                    // C.LWSP -> lw rd, offset(x2)

                    uint uoffset = 0;
                    uoffset |= (instruction >> 3 & 0x1) << 7;
                    uoffset |= (instruction >> 2 & 0x1) << 6;
                    uoffset |= (instruction >> 12 & 0x1) << 5;
                    uoffset |= (instruction >> 6 & 0x1) << 4;
                    uoffset |= (instruction >> 5 & 0x1) << 3;
                    uoffset |= (instruction >> 4 & 0x1) << 2;

                    return new IFormat
                    {
                        Mnemonic = IFormat.lw,
                        rd = rd,
                        rs1 = 2,
                        imm = (int)uoffset,
                        StepSize = 2,
                    };
                }

                return null;
        }

        return null;
    }
}
