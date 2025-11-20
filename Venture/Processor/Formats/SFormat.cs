using System;

namespace Venture.Processor.Formats;

public class SFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0100011];

    public override FormatBase? Decode(uint instruction)
    {
        var opcode = GetOpcode(instruction);
        var funct3 = instruction.ExtractBits(12, 3);

        var mnemonic = "";

        switch(funct3)
        {
            case 0b000:
                mnemonic = SFormat.sb;
                break;
            case 0b001:
                mnemonic = SFormat.sh;
                break;
            case 0b010:
                mnemonic = SFormat.sw;
                break;
        }

        if (mnemonic == "")
        {
            return null;
        }

        var rd = instruction.ExtractBits(7, 5);

        return new SFormat
        {
            Mnemonic = mnemonic,
            imm = (int)((uint)((int)instruction >> 25 << 5) | rd),
            rs1 = instruction.ExtractBits(15, 5),
            rs2 = instruction.ExtractBits(20, 5),
        };
    }
}

public class SFormat : FormatBase
{
    public const string sb = "sb";
    public const string sh = "sh";
    public const string sw = "sw";

    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }
    public required int imm { get; init; }

    public override void Execute(IProcessor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case sb:
                e.Memory.WriteByte((uint)(x[rs1] + imm), BitConverter.GetBytes(x[rs2])[0]);
                return;

            case sh:
                e.Memory.Write((uint)(x[rs1] + imm), BitConverter.GetBytes(x[rs2]).Take(2).ToArray());
                return;

            case sw:
                e.Memory.WriteWord((uint)(x[rs1] + imm), x[rs2]);
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in SFormat.");
        }
    }
}
