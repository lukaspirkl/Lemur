namespace Lemur.Processor.Formats;

public class Xh3bextmExtensionFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0001011];

    public override FormatBase? Decode(uint instruction)
    {
        var funct3 = instruction.ExtractBits(12, 3);
        switch (funct3)
        {
            case 0b000:
                return new Hazard3Bextm
                {
                    Mnemonic = "h3.bextm",
                    rd = instruction.ExtractBits(7, 5),
                    rs1 = instruction.ExtractBits(15, 5),
                    rs2 = instruction.ExtractBits(20, 5),
                    NBits = instruction.ExtractBits(26, 3) + 1, // values 0→7 encode 1→8 bits.
                };
            case 0b100:
                return new Hazard3Bextmi
                {
                    Mnemonic = "h3.bextmi",
                    rd = instruction.ExtractBits(7, 5),
                    rs1 = instruction.ExtractBits(15, 5),
                    Shamt = instruction.ExtractBits(20, 5),
                    NBits = instruction.ExtractBits(26, 3) + 1 // values 0→7 encode 1→8 bits.
                };
            default:
                return null;
        }
    }
}

public class Hazard3Bextm : FormatBase
{
    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }
    public required uint NBits { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        // Logic: rd = (rs1 >> rs2[4:0]) & ((1 << nbits) - 1)

        var shiftAmount = (int)x[rs2].ExtractBits(0, 5);
        var mask = (1u << (int)NBits) - 1;

        x[rd] = (x[rs1] >> shiftAmount) & mask;
    }
}

public class Hazard3Bextmi : FormatBase
{
    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint Shamt { get; init; }
    public required uint NBits { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        // Logic: rd = (rs1 >> shamt) & ((1 << nbits) - 1)

        var mask = (1u << (int)NBits) - 1;

        x[rd] = (x[rs1] >> (int)Shamt) & mask;
    }
}