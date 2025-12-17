namespace Venture.Processor.Formats;

public class AExtensionFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0101111];

    public override FormatBase? Decode(uint instruction)
    {
        var funct5 = instruction.ExtractBits(27, 5);
        var funct3 = instruction.ExtractBits(12, 3);
        
        if (funct3 != 0b010)
        {
            return null;
        }

        var mnemonic = "";

        switch (funct5)
        {
            case 0b00010:
                if (instruction.ExtractBits(20, 5) == 0b00000)
                {
                    return null;
                }
                mnemonic = AExtensionFormat.lr_d;
                break;
            case 0b00011:
                mnemonic = AExtensionFormat.sc_d;
                break;
            case 0b00001:
                mnemonic = AExtensionFormat.amoswap_d;
                break;
            case 0b00000:
                mnemonic = AExtensionFormat.amoadd_d;
                break;
            case 0b00100:
                mnemonic = AExtensionFormat.amoxor_d;
                break;
            case 0b01100:
                mnemonic = AExtensionFormat.amoand_d;
                break;
            case 0b01000:
                mnemonic = AExtensionFormat.amoor_d;
                break;
            case 0b10000:
                mnemonic = AExtensionFormat.amomin_d;
                break;
            case 0b10100:
                mnemonic = AExtensionFormat.amomax_d;
                break;
            case 0b11000:
                mnemonic = AExtensionFormat.amominu_d;
                break;
            case 0b11100:
                mnemonic = AExtensionFormat.amomaxu_d;
                break;
            default:
                return null;
        }

        return new AExtensionFormat
        {
            Mnemonic = mnemonic,
            rd = instruction.ExtractBits(7, 5),
            rs1 = instruction.ExtractBits(15, 5),
            rs2 = instruction.ExtractBits(20, 5),
            aq = instruction.ExtractBits(25, 1) == 1,
            rl = instruction.ExtractBits(26, 1) == 1,
            StepSize = 4
        };
    }
}

public class AExtensionFormat : FormatBase
{
    public const string lr_w = "lr.w";
    public const string sc_w = "sc.w";
    public const string amoswap_w = "amoswap.w";
    public const string amoadd_w = "amoadd.w";
    public const string amoxor_w = "amoxor.w";
    public const string amoand_w = "amoand.w";
    public const string amoor_w = "amoor.w";
    public const string amomin_w = "amomin.w";
    public const string amomax_w = "amomax.w";
    public const string amominu_w = "amominu.w";
    public const string amomaxu_w = "amomaxu.w";

    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }
    public required bool aq { get; init; }
    public required bool rl { get; init; }


    public override void Execute(Hazard3Processor e)
    {
        throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in AExtensionFormat.");
    }
}