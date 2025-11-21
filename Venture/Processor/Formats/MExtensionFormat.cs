using System;

namespace Venture.Processor.Formats;

public class MExtensionFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0110011];

    public override FormatBase? Decode(uint instruction)
    {
        if (instruction.ExtractBits(25,7) != 0b0000001)
        {
            return null;
        }

        var opcode = GetOpcode(instruction);
        var funct3 = instruction.ExtractBits(12, 3);

        var mnemonic = "";

        switch (funct3)
        {
            case 0b000:
                mnemonic = MExtensionFormat.mul;
                break;
            case 0b001:
                mnemonic = MExtensionFormat.mulh;
                break;
            case 0b010:
                mnemonic = MExtensionFormat.mulhsu;
                break;
            case 0b011:
                mnemonic = MExtensionFormat.mulhu;
                break;
            case 0b100:
                mnemonic = MExtensionFormat.div;
                break;
            case 0b101:
                mnemonic = MExtensionFormat.divu;
                break;
            case 0b110:
                mnemonic = MExtensionFormat.rem;
                break;
            case 0b111:
                mnemonic = MExtensionFormat.remu;
                break;
        }

        if (mnemonic == "")
        {
            return null;
        }

        return new MExtensionFormat
        {
            Mnemonic = mnemonic,
            rd = instruction.ExtractBits(7, 5),
            rs1 = instruction.ExtractBits(15, 5),
            rs2 = instruction.ExtractBits(20, 5),
        };
    }
}

public class MExtensionFormat : FormatBase
{
    public const string mul = "mul";
    public const string mulh = "mulh";
    public const string mulhsu = "mulhsu";
    public const string mulhu = "mulhu";
    public const string div = "div";
    public const string divu = "divu";
    public const string rem = "rem";
    public const string remu = "remu";

    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }
    public required uint rd { get; init; }

    public override void Execute(IProcessor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case mul:
                // Multiplication of XLEN bits, keep lower XLEN bits.
                // Signedness does not affect the lower 32 bits of the product.
                x[rd] = unchecked(x[rs1] * x[rs2]);
                break;

            case mulh:
                // Signed x Signed multiplication, return upper XLEN bits.
                // Cast to long to perform 64-bit math, shift down to get high bits.
                long productH = (long)(int)x[rs1] * (long)(int)x[rs2];
                x[rd] = (uint)(productH >> 32);
                break;

            case mulhsu:
                // Signed x Unsigned multiplication, return upper XLEN bits.
                // C# implicitly handles the mixed sign correctly if we cast rs2 to long 
                // (which treats it as positive because it fits in the positive range of int64).
                long productHSU = (long)(int)x[rs1] * (long)x[rs2];
                x[rd] = (uint)(productHSU >> 32);
                break;

            case mulhu:
                // Unsigned x Unsigned multiplication, return upper XLEN bits.
                ulong productHU = (ulong)x[rs1] * (ulong)x[rs2];
                x[rd] = (uint)(productHU >> 32);
                break;

            case div:
                // Signed division.
                if ((int)x[rs2] == 0)
                {
                    // Division by zero semantics: return -1 (all bits set).
                    x[rd] = 0xFFFFFFFF;
                }
                else if ((int)x[rs1] == int.MinValue && (int)x[rs2] == -1)
                {
                    // Signed overflow semantics (MinInt / -1): return MinInt.
                    x[rd] = x[rs1];
                }
                else
                {
                    x[rd] = (uint)((int)x[rs1] / (int)x[rs2]);
                }
                break;

            case divu:
                // Unsigned division.
                if (x[rs2] == 0)
                {
                    // Division by zero semantics: return (2^L)-1 (all bits set).
                    x[rd] = 0xFFFFFFFF;
                }
                else
                {
                    x[rd] = x[rs1] / x[rs2];
                }
                break;

            case rem:
                // Signed remainder.
                if ((int)x[rs2] == 0)
                {
                    // Division by zero semantics: return dividend.
                    x[rd] = x[rs1];
                }
                else if ((int)x[rs1] == int.MinValue && (int)x[rs2] == -1)
                {
                    // Signed overflow semantics: return 0.
                    x[rd] = 0;
                }
                else
                {
                    x[rd] = (uint)((int)x[rs1] % (int)x[rs2]);
                }
                break;

            case remu:
                // Unsigned remainder.
                if (x[rs2] == 0)
                {
                    // Division by zero semantics: return dividend.
                    x[rd] = x[rs1];
                }
                else
                {
                    x[rd] = x[rs1] % x[rs2];
                }
                break;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in MExtensionFormat.");
        }
    }
}
