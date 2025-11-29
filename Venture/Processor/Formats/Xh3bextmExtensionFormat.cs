namespace Venture.Processor.Formats;

public class Xh3bextmExtensionFormatFactory : FormatFactoryBase
{
    // custom0 opcode used by Hazard3 extensions
    public override uint[] ForOpcodes => [0b0001011];

    public override FormatBase? Decode(uint instruction)
    {
        var opcode = GetOpcode(instruction);
        var funct3 = instruction.ExtractBits(12, 3);

        // Decoding based on funct3 as specified in datasheet section 4.4
        switch (funct3)
        {
            case 0b000: // h3.bextm
                return DecodeBextm(instruction);
            case 0b100: // h3.bextmi
                return DecodeBextmi(instruction);
            default:
                return null;
        }
    }

    private FormatBase DecodeBextm(uint instruction)
    {
        // Encoding (R-type):
        // 31:29 Reserved (0)
        // 28:26 size (encodes nbits-1)
        // 25    Reserved (0)
        // 24:20 rs2
        // 19:15 rs1
        // 14:12 funct3 (000)
        // 11:7  rd
        // 6:2   opcode (custom0)

        var sizeField = instruction.ExtractBits(26, 3);

        return new Hazard3Bextm
        {
            Mnemonic = "h3.bextm",
            rd = instruction.ExtractBits(7, 5),
            rs1 = instruction.ExtractBits(15, 5),
            rs2 = instruction.ExtractBits(20, 5),
            NBits = (int)(sizeField + 1) // 0->1 bit, 7->8 bits
        };
    }

    private FormatBase DecodeBextmi(uint instruction)
    {
        // Encoding (I-type):
        // 31:29 Reserved (0)
        // 28:26 size (encodes nbits-1)
        // 25    Reserved (0)
        // 24:20 shamt
        // 19:15 rs1
        // 14:12 funct3 (100)
        // 11:7  rd
        // 6:2   opcode (custom0)

        // Note: In the I-Type macro, the immediate is constructed as:
        // (size << 6) | shamt. 
        // This places 'size' in bits 26-28 and 'shamt' in 20-24.

        var sizeField = instruction.ExtractBits(26, 3);
        var shamt = instruction.ExtractBits(20, 5);

        return new Hazard3Bextmi
        {
            Mnemonic = "h3.bextmi",
            rd = instruction.ExtractBits(7, 5),
            rs1 = instruction.ExtractBits(15, 5),
            Shamt = (int)shamt,
            NBits = (int)(sizeField + 1)
        };
    }
}

public class Hazard3Bextm : FormatBase
{
    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }
    public required int NBits { get; init; }

    //public override string Mnemonic => "h3.bextm";

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        // Logic: rd = (rs1 >> rs2[4:0]) & ((1 << nbits) - 1)

        var shiftAmount = (int)x[rs2].ExtractBits(0, 5);
        var mask = (1u << NBits) - 1;

        x[rd] = (x[rs1] >> shiftAmount) & mask;
    }
}

public class Hazard3Bextmi : FormatBase
{
    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required int Shamt { get; init; }
    public required int NBits { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        // Logic: rd = (rs1 >> shamt) & ((1 << nbits) - 1)

        var mask = (1u << NBits) - 1;

        x[rd] = (x[rs1] >> Shamt) & mask;
    }
}