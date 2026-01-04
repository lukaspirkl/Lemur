namespace Venture.Processor.Formats;

public class RFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0110011];

    public override FormatBase? Decode(uint instruction)
    {
        var opcode = GetOpcode(instruction);

        var funct3 = instruction.ExtractBits(12, 3);
        var funct7 = instruction.ExtractBits(25, 7);

        var mnemonic = "";

        switch (funct3)
        {
            case 0b000:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.ADD;
                        break;
                    case 0b0100000:
                        mnemonic = RFormat.SUB;
                        break;
                }
                break;
            case 0b001:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.SLL;
                        break;
                }
                break;
            case 0b010:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.SLT;
                        break;
                }
                break;
            case 0b011:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.SLTU;
                        break;
                }
                break;
            case 0b100:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.XOR;
                        break;
                }
                break;
            case 0b101:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.SRL;
                        break;
                    case 0b0100000:
                        mnemonic = RFormat.SRA;
                        break;
                }
                break;
            case 0b110:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.OR;
                        break;
                }
                break;
            case 0b111:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.AND;
                        break;
                }
                break;

        }

        if (mnemonic == "")
        {
            return null;
        }

        return new RFormat
        {
            Mnemonic = mnemonic,
            rd = instruction.ExtractBits(7, 5),
            rs1 = instruction.ExtractBits(15, 5),
            rs2 = instruction.ExtractBits(20, 5),
        };
    }
}

public class RFormat : FormatBase
{
    public const string ADD = "add";
    public const string SUB = "sub";
    public const string SLL = "sll";
    public const string SLT = "slt";
    public const string SLTU = "sltu";
    public const string XOR = "xor";
    public const string SRL = "srl";
    public const string SRA = "sra";
    public const string OR = "or";
    public const string AND = "and";


    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }
    

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case ADD:
                x[rd] = x[rs1] + x[rs2];
                return;

            case SUB:
                x[rd] = x[rs1] - x[rs2];
                return;

            case SLL:
                x[rd] = x[rs1] << (int)x[rs2].ExtractBits(0, 5);
                return;

            case SLT:
                x[rd] = ((int)x[rs1] < (int)x[rs2]) ? (uint)1 : 0;
                return;

            case SLTU:
                x[rd] = (x[rs1] < x[rs2]) ? (uint)1 : 0;
                return;

            case XOR:
                x[rd] = x[rs1] ^ x[rs2];
                return;

            case SRL:
                x[rd] = x[rs1] >> (int)x[rs2].ExtractBits(0, 5);
                return;

            case SRA:
                x[rd] = (uint)((int)x[rs1] >> (int)x[rs2].ExtractBits(0, 5));
                return;

            case OR:
                x[rd] = x[rs1] | x[rs2];
                return;

            case AND:
                x[rd] = x[rs1] & x[rs2];
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in RFormat.");
        }
    }
}
