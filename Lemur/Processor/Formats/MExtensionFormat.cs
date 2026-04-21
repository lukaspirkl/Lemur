using System;

namespace Lemur.Processor.Formats;

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
                mnemonic = MExtensionFormat.MUL;
                break;
            case 0b001:
                mnemonic = MExtensionFormat.MULH;
                break;
            case 0b010:
                mnemonic = MExtensionFormat.MULHSU;
                break;
            case 0b011:
                mnemonic = MExtensionFormat.MULHU;
                break;
            case 0b100:
                mnemonic = MExtensionFormat.DIV;
                break;
            case 0b101:
                mnemonic = MExtensionFormat.DIVU;
                break;
            case 0b110:
                mnemonic = MExtensionFormat.REM;
                break;
            case 0b111:
                mnemonic = MExtensionFormat.REMU;
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
    public const string MUL = "mul";
    public const string MULH = "mulh";
    public const string MULHSU = "mulhsu";
    public const string MULHU = "mulhu";
    public const string DIV = "div";
    public const string DIVU = "divu";
    public const string REM = "rem";
    public const string REMU = "remu";

    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }
    public required uint rd { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case MUL:
                // Multiplication of XLEN bits, keep lower XLEN bits.
                // Signedness does not affect the lower 32 bits of the product.
                x[rd] = unchecked(x[rs1] * x[rs2]);
                break;

            case MULH:
                // Signed x Signed multiplication, return upper XLEN bits.
                // Cast to long to perform 64-bit math, shift down to get high bits.
                long productH = (long)(int)x[rs1] * (long)(int)x[rs2];
                x[rd] = (uint)(productH >> 32);
                break;

            case MULHSU:
                // Signed x Unsigned multiplication, return upper XLEN bits.
                // C# implicitly handles the mixed sign correctly if we cast rs2 to long 
                // (which treats it as positive because it fits in the positive range of int64).
                long productHSU = (long)(int)x[rs1] * (long)x[rs2];
                x[rd] = (uint)(productHSU >> 32);
                break;

            case MULHU:
                // Unsigned x Unsigned multiplication, return upper XLEN bits.
                ulong productHU = (ulong)x[rs1] * (ulong)x[rs2];
                x[rd] = (uint)(productHU >> 32);
                break;

            case DIV:
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

            case DIVU:
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

            case REM:
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

            case REMU:
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
