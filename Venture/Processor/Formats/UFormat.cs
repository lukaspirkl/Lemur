namespace Venture.Processor.Formats;

public class UFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0110111, 0b0010111];

    public override FormatBase? Decode(uint instruction)
    {
        var opcode = GetOpcode(instruction);

        return new UFormat
        {
            Mnemonic = opcode == 0b0110111 ? UFormat.LUI : UFormat.AUIPC,
            imm = instruction & 0xfffff000,
            rd = instruction.ExtractBits(7, 5),
        };
    }
}

public class UFormat : FormatBase
{
    public const string LUI = "lui";
    public const string AUIPC = "auipc";

    public required uint rd { get; init; }
    public required uint imm { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case LUI:
                x[rd] = imm;
                return;

            case AUIPC:
                x[rd] = e.PC + imm;
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in UFormat.");
        }
    }
}
