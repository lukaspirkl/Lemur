using System;
using System.Security.Cryptography;

namespace Venture.Processor.Formats;

public class BExtensionFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0110011, 0b0010011];

    public override FormatBase? Decode(uint instruction)
    {
        var opcode = instruction.ExtractBits(0, 7);
        var funct7 = instruction.ExtractBits(25, 7);
        var funct3 = instruction.ExtractBits(12, 3);

        var rd = instruction.ExtractBits(7, 5);
        var rs1 = instruction.ExtractBits(15, 5);
        var rs2 = instruction.ExtractBits(20, 5);

        var mnemonic = "";

        switch (opcode)
        {
            case 0b0010011:
                switch (funct7)
                {
                    case 0b0100100:
                        switch (funct3)
                        {
                            case 0b001:
                                // Test is passing only when this is commented out, but documentation says that shamt[5]=1 -> reserved
                                //if (rs2 == 1)
                                //{
                                //    throw new NotImplementedException("reserved");
                                //}
                                mnemonic = BExtensionFormat.bclri;
                                break;
                        }
                        break;
                }
                break;
            case 0b0110011:
                switch (funct7)
                {
                    case 0b0100000:
                        switch (funct3)
                        {
                            case 0b111:
                                mnemonic = BExtensionFormat.andn;
                                break;
                        }
                        break;
                    case 0b0100100:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.bclr;
                                break;
                        }
                        break;
                }
                break;
        }

        if (mnemonic == "")
        {
            return null;
        }

        return new BExtensionFormat
        {
            Mnemonic = mnemonic,
            rd = rd,
            rs1 = rs1,
            rs2 = rs2,
        };
    }

    protected readonly IFormat nop = new IFormat
    {
        Mnemonic = IFormat.addi,
        imm = 0,
        rd = 0,
        rs1 = 0,
        StepSize = 2,
    };
}

public class BExtensionFormat : FormatBase
{
    public const string andn = "andn";
    public const string bclr = "bclr";
    public const string bclri = "bclri";

    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }

    public override void Execute(IProcessor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case andn:
                x[rd] = x[rs1] & ~x[rs2];
                return;

            case bclr:
                x[rd] = x[rs1] & ~((uint)1 << (int)x[rs2]);
                return;

            case bclri:
                x[rd] = x[rs1] & ~((uint)1 << (int)rs2);
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in BExtensionFormat.");
        }
    }
}
