namespace Venture.Processor.Formats;

public class IFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b1100111, 0b0000011, 0b0010011, 0b1110011, 0b0001111];

    public override FormatBase Decode(uint instruction)
    {
        var opcode = GetOpcode(instruction);
        var funct3 = instruction.ExtractBits(12, 3);
        var funct7 = instruction.ExtractBits(25, 7);

        var mnemonic = "";

        switch (opcode)
        {
            case 0b1100111:
                mnemonic = IFormat.jalr;
                break;

            case 0b0000011:
                switch (funct3)
                {
                    case 0b000:
                        mnemonic = IFormat.lb;
                        break;
                    case 0b001:
                        mnemonic = IFormat.lh;
                        break;
                    case 0b010:
                        mnemonic = IFormat.lw;
                        break;
                    case 0b100:
                        mnemonic = IFormat.lbu;
                        break;
                    case 0b101:
                        mnemonic = IFormat.lhu;
                        break;
                }
                break;

            case 0b0010011:
                switch (funct3)
                {
                    case 0b000:
                        mnemonic = IFormat.addi;
                        break;
                    case 0b001:
                        switch (funct7)
                        {
                            case 0b0000000:
                                mnemonic = IFormat.slli;
                                break;
                        }
                        break;
                    case 0b010:
                        mnemonic = IFormat.slti;
                        break;
                    case 0b011:
                        mnemonic = IFormat.sltiu;
                        break;
                    case 0b100:
                        mnemonic = IFormat.xori;
                        break;
                    case 0b101:
                        switch(funct7)
                        {
                            case 0b0000000:
                                mnemonic = IFormat.srli;
                                break;
                            case 0b0100000:
                                mnemonic = IFormat.srai;
                                break;
                        }
                        break;
                    case 0b110:
                        mnemonic = IFormat.ori;
                        break;
                    case 0b111:
                        mnemonic = IFormat.andi;
                        break;
                }
                break;

            case 0b1110011:
                switch (funct3)
                {
                    case 0b000:
                        switch (instruction)
                        {
                            case 0x30200073:
                                mnemonic = IFormat.mret;
                                break;
                            case 0x00000073:
                                mnemonic = IFormat.ecall;
                                break;
                            case 0x00100073:
                                mnemonic = IFormat.ebreak;
                                break;
                        }
                        break;
                    case 0b001:
                        mnemonic = IFormat.csrrw;
                        break;
                    case 0b010:
                        mnemonic = IFormat.csrrs;
                        break;
                    case 0b101:
                        mnemonic = IFormat.csrrwi;
                        break;
                }
                break;

            case 0b0001111:
                switch (instruction)
                {
                    case 0x0ff0000f:
                        mnemonic = IFormat.fence;
                        break;
                    case 0x0000100f:
                        mnemonic = IFormat.fencei;
                        break;
                }
                break;
        }

        if (mnemonic == "")
        {
            throw new NotImplementedException($"Unknown opcode:{opcode.ToBin(7)} funct3:{funct3.ToBin(3)} funct7:{funct7.ToBin(7)} in IFormat. Instruction: {instruction.ToHex()}");
        }

        return new IFormat
        {
            Mnemonic = mnemonic,
            rd = instruction.ExtractBits(7, 5),
            rs1 = instruction.ExtractBits(15, 5),
            imm = (int)instruction >> 20, // Sign is preserved when shifting signed int
        };
    }
}

public class IFormat : FormatBase
{
    public const string jalr = "jalr";
    public const string lb = "lb";
    public const string lh = "lh";
    public const string lw = "lw";
    public const string lbu = "lbu";
    public const string lhu = "lhu";

    public const string addi = "addi";
    public const string slli = "slli";
    public const string slti = "slti";
    public const string sltiu = "sltiu";
    public const string xori = "xori";
    public const string srli = "srli";
    public const string srai = "srai";
    public const string ori = "ori";
    public const string andi = "andi";

    public const string mret = "mret";
    public const string ecall = "ecall";
    public const string ebreak = "ebreak";
    public const string csrrw = "csrrw";
    public const string csrrs = "csrrs";
    public const string csrrwi = "csrrwi";

    public const string fence = "fence";
    public const string fencei = "fence.i";

    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required int imm { get; init; }

    public override void Execute(IProcessor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case jalr:
                uint targetAddress = (uint)((int)x[rs1] + imm);
                x[rd] = e.PC + StepSize;
                e.PC = targetAddress & 0b11111111_11111111_11111111_11111110;
                return;

            case lb:
                x[rd] = (uint)(sbyte)e.Memory.ReadByte((uint)(x[rs1] + imm));
                return;

            case lh:
                x[rd] = (uint)(short)e.Memory.ReadHalfWord((uint)(x[rs1] + imm));
                return;

            case lw:
                x[rd] = (uint)(int)e.Memory.ReadWord((uint)(x[rs1] + imm));
                return;

            case lbu:
                x[rd] = e.Memory.ReadByte((uint)(x[rs1] + imm));
                return;

            case lhu:
                x[rd] = e.Memory.ReadHalfWord((uint)(x[rs1] + imm));
                return;

            case addi:
                x[rd] = (uint)((int)x[rs1] + imm);
                return;

            case slli:
                x[rd] = x[rs1] << imm;
                return;

            case slti:
                x[rd] = (int)x[rs1] < imm ? (uint)1 : 0;
                return;

            case sltiu:
                x[rd] = x[rs1] < (uint)imm ? (uint)1 : 0;
                return;

            case xori:
                x[rd] = x[rs1] ^ (uint)imm;
                return;

            case srli:
                x[rd] = x[rs1] >> imm;
                return;

            case srai:
                x[rd] = (uint)((int)x[rs1] >> imm);
                return;

            case ori:
                x[rd] = x[rs1] | (uint)imm;
                return;

            case andi:
                x[rd] = x[rs1] & (uint)imm;
                return;

            case csrrw:
                x[rd] = e.CSR.Get((ushort)imm);
                e.CSR.Set((ushort)imm, x[rs1]);
                return;

            case csrrs:
                uint original_csr_value = e.CSR.Get((ushort)imm);

                if (rs1 != 0)
                {
                    uint rs1_mask = x[rs1];
                    uint new_csr_value = original_csr_value | rs1_mask;
                    e.CSR.Set((ushort)imm, new_csr_value);
                }

                x[rd] = original_csr_value;
                return;

            case csrrwi:
                x[rd] = e.CSR.Get((ushort)imm);
                e.CSR.Set((ushort)imm, (uint)imm);
                return;

            case mret:
                e.PC = e.CSR.Get(0x341);
                return;

            case ecall:
                e.RaiseECall(x[17], x[10]);
                return;

            case ebreak:
                e.RaiseEBreak();
                return;

            case fence:
                return;

            case fencei:
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in IFormat.");
        }
    }
}

public record ECallParams(uint ServiceNumber, uint Argument);
