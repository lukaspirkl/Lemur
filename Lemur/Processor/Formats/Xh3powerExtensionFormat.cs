using System;

namespace Lemur.Processor.Formats;

public class Xh3powerExtensionFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0110011];

    public override FormatBase? Decode(uint instruction)
    {
        var opcode = GetOpcode(instruction);

        var funct3 = instruction.ExtractBits(12, 3);
        var funct7 = instruction.ExtractBits(25, 7);
        var rd = instruction.ExtractBits(7, 5);
        var rs1 = instruction.ExtractBits(15, 5);
        var rs2 = instruction.ExtractBits(20, 5);

        if (funct3 == 0b010 && funct7 == 0b0000000 && rd == 0 && rs1 == 0)
        {
            switch (rs2)
            {
                case 0:
                    return new Xh3powerExtensionFormat { Mnemonic = Xh3powerExtensionFormat.H3_BLOCK };
                case 1:
                    return new Xh3powerExtensionFormat { Mnemonic = Xh3powerExtensionFormat.H3_UNBLOCK };
            }
        }

        return null;
    }
}

public class Xh3powerExtensionFormat : FormatBase
{
    public const string H3_BLOCK = "h3.block";
    public const string H3_UNBLOCK = "h3.unblock";

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case H3_BLOCK:
                return;

            case H3_UNBLOCK:
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in Xh3powerExtensionFormat.");
        }
    }
}
