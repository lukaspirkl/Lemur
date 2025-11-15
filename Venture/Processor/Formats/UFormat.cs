namespace Venture.Processor.Formats;

public class UFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0110111, 0b0010111];

    public override FormatBase Decode(uint instruction)
    {
        var opcode = GetOpcode(instruction);

        return new UFormat
        {
            Mnemonic = opcode == 0b0110111 ? UFormat.lui : UFormat.auipc,
            imm = instruction & 0xfffff000,
            rd = instruction.ExtractBits(7, 5),
        };
    }
}

public class UFormat : FormatBase
{
    public const string lui = "lui";
    public const string auipc = "auipc";

    public required uint rd { get; init; }
    public required uint imm { get; init; }

    public override void Execute(IProcessor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case lui:
                x[rd] = imm;
                return;

            case auipc:
                x[rd] = e.PC + imm;
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in UFormat.");
        }
    }
}
