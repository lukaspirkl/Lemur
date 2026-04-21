using System;

namespace Lemur.Processor.Formats;

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
                // LR.W: rs2 (bits 24:20) must be 0 per spec; treat non-zero encodings as illegal.
                // Spec: RISC-V Unprivileged ISA Section 8.2
                if (instruction.ExtractBits(20, 5) != 0b00000)
                {
                    return null;
                }
                mnemonic = AExtensionFormat.LR_W;
                break;
            case 0b00011:
                mnemonic = AExtensionFormat.SC_W;
                break;
            case 0b00001:
                mnemonic = AExtensionFormat.AMOSWAP_W;
                break;
            case 0b00000:
                mnemonic = AExtensionFormat.AMOADD_W;
                break;
            case 0b00100:
                mnemonic = AExtensionFormat.AMOXOR_W;
                break;
            case 0b01100:
                mnemonic = AExtensionFormat.AMOAND_W;
                break;
            case 0b01000:
                mnemonic = AExtensionFormat.AMOOR_W;
                break;
            case 0b10000:
                mnemonic = AExtensionFormat.AMOMIN_W;
                break;
            case 0b10100:
                mnemonic = AExtensionFormat.AMOMAX_W;
                break;
            case 0b11000:
                mnemonic = AExtensionFormat.AMOMINU_W;
                break;
            case 0b11100:
                mnemonic = AExtensionFormat.AMOMAXU_W;
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
    public const string LR_W = "lr.w";
    public const string SC_W = "sc.w";
    public const string AMOSWAP_W = "amoswap.w";
    public const string AMOADD_W = "amoadd.w";
    public const string AMOXOR_W = "amoxor.w";
    public const string AMOAND_W = "amoand.w";
    public const string AMOOR_W = "amoor.w";
    public const string AMOMIN_W = "amomin.w";
    public const string AMOMAX_W = "amomax.w";
    public const string AMOMINU_W = "amominu.w";
    public const string AMOMAXU_W = "amomaxu.w";

    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }
    public required bool aq { get; init; }
    public required bool rl { get; init; }


    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case LR_W:
                {
                    // LR.W — Load-Reserved Word
                    // Spec: RISC-V Unprivileged ISA Section 8.2
                    //
                    // Loads a word from the address in rs1 into rd and places a reservation on
                    // that address. The reservation is later consumed by a matching SC.W.
                    //
                    // aq/rl bits control memory-ordering (acquire/release). In the emulator there
                    // is only a single thread of execution, so ordering constraints are always met.
                    var addr = x[rs1];
                    x[rd] = e.Memory.ReadWord(addr);
                    e.Reservation = addr;
                }
                break;

            case SC_W:
                {
                    // SC.W — Store-Conditional Word
                    // Spec: RISC-V Unprivileged ISA Section 8.2
                    //
                    // Conditionally stores rs2 to the address in rs1 only if a valid reservation
                    // on that exact address still exists (placed by a prior LR.W).
                    //
                    // On success: stores the word and writes 0 to rd.
                    // On failure: does not store and writes 1 to rd.
                    // The reservation is always cleared regardless of outcome.
                    //
                    // Typical use pattern (spinlock acquire):
                    //   lr.w  t0, (a0)      ; load-reserved
                    //   bnez  t0, retry     ; occupied — retry
                    //   sc.w  t0, a1, (a0)  ; try to claim
                    //   bnez  t0, retry     ; lost reservation — retry
                    var addr = x[rs1];
                    if (e.Reservation == addr)
                    {
                        e.Memory.WriteWord(addr, x[rs2]);
                        x[rd] = 0; // success
                    }
                    else
                    {
                        x[rd] = 1; // failure — reservation was stolen or never set
                    }
                    e.Reservation = null; // always clear
                }
                break;

            case AMOSWAP_W:
                {
                    var addr = x[rs1];
                    var old = e.Memory.ReadWord(addr);
                    e.Memory.WriteWord(addr, x[rs2]);
                    x[rd] = old;
                }
                break;
            case AMOADD_W:
                {
                    var addr = x[rs1];
                    var old = e.Memory.ReadWord(addr);
                    e.Memory.WriteWord(addr, (uint)(old + (int)x[rs2]));
                    x[rd] = old;
                }
                break;
            case AMOXOR_W:
                {
                    var addr = x[rs1];
                    var old = e.Memory.ReadWord(addr);
                    e.Memory.WriteWord(addr, old ^ x[rs2]);
                    x[rd] = old;
                }
                break;
            case AMOAND_W:
                {
                    var addr = x[rs1];
                    var old = e.Memory.ReadWord(addr);
                    e.Memory.WriteWord(addr, old & x[rs2]);
                    x[rd] = old;
                }
                break;
            case AMOOR_W:
                {
                    var addr = x[rs1];
                    var old = e.Memory.ReadWord(addr);
                    e.Memory.WriteWord(addr, old | x[rs2]);
                    x[rd] = old;
                }
                break;
            case AMOMIN_W:
                {
                    var addr = x[rs1];
                    var old = e.Memory.ReadWord(addr);
                    e.Memory.WriteWord(addr, (uint)Math.Min((int)old, (int)x[rs2]));
                    x[rd] = old;
                }
                break;

            case AMOMINU_W:
                {
                    var addr = x[rs1];
                    var old = e.Memory.ReadWord(addr);
                    e.Memory.WriteWord(addr, Math.Min(old, x[rs2]));
                    x[rd] = old;
                }
                break;
            case AMOMAX_W:
                {
                    var addr = x[rs1];
                    var old = e.Memory.ReadWord(addr);
                    e.Memory.WriteWord(addr, (uint)Math.Max((int)old, (int)x[rs2]));
                    x[rd] = old;
                }
                break;

            case AMOMAXU_W:
                {
                    var addr = x[rs1];
                    var old = e.Memory.ReadWord(addr);
                    e.Memory.WriteWord(addr, Math.Max(old, x[rs2]));
                    x[rd] = old;
                }
                break;
            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in AExtensionFormat.");
        }
    }
}