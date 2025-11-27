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
                        mnemonic = RFormat.add;
                        break;
                    case 0b0100000:
                        mnemonic = RFormat.sub;
                        break;
                }
                break;
            case 0b001:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.sll;
                        break;
                }
                break;
            case 0b010:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.slt;
                        break;
                }
                break;
            case 0b011:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.sltu;
                        break;
                }
                break;
            case 0b100:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.xor;
                        break;
                }
                break;
            case 0b101:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.srl;
                        break;
                    case 0b0100000:
                        mnemonic = RFormat.sra;
                        break;
                }
                break;
            case 0b110:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.or;
                        break;
                }
                break;
            case 0b111:
                switch (funct7)
                {
                    case 0b0000000:
                        mnemonic = RFormat.and;
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
    public const string add = "add";
    public const string sub = "sub";
    public const string sll = "sll";
    public const string slt = "slt";
    public const string sltu = "sltu";
    public const string xor = "xor";
    public const string srl = "srl";
    public const string sra = "sra";
    public const string or = "or";
    public const string and = "and";


    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }
    

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case add:
                x[rd] = x[rs1] + x[rs2];
                return;

            case sub:
                x[rd] = x[rs1] - x[rs2];
                return;

            case sll:
                x[rd] = x[rs1] << (int)x[rs2].ExtractBits(0, 5);
                return;

            case slt:
                x[rd] = ((int)x[rs1] < (int)x[rs2]) ? (uint)1 : 0;
                return;

            case sltu:
                x[rd] = (x[rs1] < x[rs2]) ? (uint)1 : 0;
                return;

            case xor:
                x[rd] = x[rs1] ^ x[rs2];
                return;

            case srl:
                x[rd] = x[rs1] >> (int)x[rs2].ExtractBits(0, 5);
                return;

            case sra:
                x[rd] = (uint)((int)x[rs1] >> (int)x[rs2].ExtractBits(0, 5));
                return;

            case or:
                x[rd] = x[rs1] | x[rs2];
                return;

            case and:
                x[rd] = x[rs1] & x[rs2];
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in RFormat.");
        }
    }
}
