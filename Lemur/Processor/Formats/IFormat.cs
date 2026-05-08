using System;

namespace Lemur.Processor.Formats;

public class IFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b1100111, 0b0000011, 0b0010011, 0b1110011, 0b0001111];

    public override FormatBase? Decode(uint instruction)
    {
        var opcode = GetOpcode(instruction);
        var funct3 = instruction.ExtractBits(12, 3);
        var funct7 = instruction.ExtractBits(25, 7);

        var mnemonic = "";

        switch (opcode)
        {
            case 0b1100111:
                mnemonic = IFormat.JALR;
                break;

            case 0b0000011:
                switch (funct3)
                {
                    case 0b000:
                        mnemonic = IFormat.LB;
                        break;
                    case 0b001:
                        mnemonic = IFormat.LH;
                        break;
                    case 0b010:
                        mnemonic = IFormat.LW;
                        break;
                    case 0b100:
                        mnemonic = IFormat.LBU;
                        break;
                    case 0b101:
                        mnemonic = IFormat.LHU;
                        break;
                }
                break;

            case 0b0010011:
                switch (funct3)
                {
                    case 0b000:
                        mnemonic = IFormat.ADDI;
                        break;
                    case 0b001:
                        switch (funct7)
                        {
                            case 0b0000000:
                                mnemonic = IFormat.SLLI;
                                break;
                        }
                        break;
                    case 0b010:
                        mnemonic = IFormat.SLTI;
                        break;
                    case 0b011:
                        mnemonic = IFormat.SLTIU;
                        break;
                    case 0b100:
                        mnemonic = IFormat.XORI;
                        break;
                    case 0b101:
                        switch(funct7)
                        {
                            case 0b0000000:
                                mnemonic = IFormat.SRLI;
                                break;
                            case 0b0100000:
                                mnemonic = IFormat.SRAI;
                                break;
                        }
                        break;
                    case 0b110:
                        mnemonic = IFormat.ORI;
                        break;
                    case 0b111:
                        mnemonic = IFormat.ANDI;
                        break;
                }
                break;

            case 0b1110011:
                switch (funct3)
                {
                    case 0b000:
                        switch (instruction)
                        {
                            case 0x30200073:
                                mnemonic = IFormat.MRET;
                                break;
                            case 0x00000073:
                                mnemonic = IFormat.ECALL;
                                break;
                            case 0x00100073:
                                mnemonic = IFormat.EBREAK;
                                break;
                            case 0x10500073:
                                mnemonic = IFormat.WFI;
                                break;
                        }
                        break;
                    case 0b001:
                        return CreateCSR(ICSRFormat.CSRRW, instruction);
                    case 0b010:
                        return CreateCSR(ICSRFormat.CSRRS, instruction);
                    case 0b011:
                        return CreateCSR(ICSRFormat.CSRRC, instruction);
                    case 0b101:
                        return CreateCSR(ICSRFormat.CSRRWI, instruction);
                    case 0b110:
                        return CreateCSR(ICSRFormat.CSRRSI, instruction);
                    case 0b111:
                        return CreateCSR(ICSRFormat.CSRRCI, instruction);
                }
                break;

            case 0b0001111:
                switch (funct3)
                {
                    case 0x000:
                        mnemonic = IFormat.FENCE;
                        break;
                    case 0x001:
                        mnemonic = IFormat.FENCEI;
                        break;
                }
                break;
        }

        if (mnemonic == "")
        {
            return null;
        }

        return new IFormat
        {
            Mnemonic = mnemonic,
            rd = instruction.ExtractBits(7, 5),
            rs1 = instruction.ExtractBits(15, 5),
            imm = (int)instruction >> 20, // Sign is preserved when shifting signed int
        };
    }

    private ICSRFormat CreateCSR(string mnemonic, uint instruction)
    {
        return new ICSRFormat
        {
            Mnemonic = mnemonic,
            rd = instruction.ExtractBits(7, 5),
            rs1 = instruction.ExtractBits(15, 5),
            csr = (ushort)(instruction >>> 20) // Zero extend
        };
    }
}

public class ICSRFormat : FormatBase
{
    public const string CSRRW = "csrrw";
    public const string CSRRS = "csrrs";
    public const string CSRRC = "csrrc";
    public const string CSRRWI = "csrrwi";
    public const string CSRRSI = "csrrsi";
    public const string CSRRCI = "csrrci";

    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required ushort csr { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case CSRRW:
                if (rd != 0)
                {
                    var value = e.CSR.Get(csr, e.CurrentPrivilege);
                    e.CSR.Set(csr, x[rs1], e.CurrentPrivilege);
                    x[rd] = value;
                }
                else
                {
                    e.CSR.Set(csr, x[rs1], e.CurrentPrivilege);
                }
                return;

            case CSRRS:
                {
                    uint value = e.CSR.Get(csr, e.CurrentPrivilege);
                    if (rs1 != 0)
                    {
                        e.CSR.Set(csr, value | x[rs1], e.CurrentPrivilege);
                    }
                    x[rd] = value;
                }
                return;

            case CSRRC:
                {
                    uint value = e.CSR.Get(csr, e.CurrentPrivilege);
                    if (rs1 != 0)
                    {
                        e.CSR.Set(csr, value & ~x[rs1], e.CurrentPrivilege);
                    }
                    x[rd] = value;
                }
                return;

            case CSRRWI:
                if (rd != 0)
                {
                    x[rd] = e.CSR.Get(csr, e.CurrentPrivilege);
                }
                e.CSR.Set(csr, rs1, e.CurrentPrivilege);
                return;

            case CSRRSI:
                {
                    uint value = e.CSR.Get(csr, e.CurrentPrivilege);
                    e.CSR.Set(csr, value | rs1, e.CurrentPrivilege);
                    x[rd] = value;
                }
                return;

            case CSRRCI:
                {
                    uint value = e.CSR.Get(csr, e.CurrentPrivilege);
                    e.CSR.Set(csr, value & ~rs1, e.CurrentPrivilege);
                    x[rd] = value;
                }
                return; ;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in IFormat.");
        }
    }
}

public class IFormat : FormatBase
{
    public const string JALR = "jalr";
    public const string LB = "lb";
    public const string LH = "lh";
    public const string LW = "lw";
    public const string LBU = "lbu";
    public const string LHU = "lhu";

    public const string ADDI = "addi";
    public const string SLLI = "slli";
    public const string SLTI = "slti";
    public const string SLTIU = "sltiu";
    public const string XORI = "xori";
    public const string SRLI = "srli";
    public const string SRAI = "srai";
    public const string ORI = "ori";
    public const string ANDI = "andi";

    public const string MRET = "mret";
    public const string ECALL = "ecall";
    public const string EBREAK = "ebreak";
    public const string WFI = "wfi";

    public const string FENCE = "fence";
    public const string FENCEI = "fence.i";

    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required int imm { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case JALR:
                uint targetAddress = (uint)((int)x[rs1] + imm);
                x[rd] = e.PC + StepSize;
                e.PC = targetAddress & 0b11111111_11111111_11111111_11111110;
                return;

            case LB:
                x[rd] = (uint)(sbyte)e.Memory.ReadByte((uint)(x[rs1] + imm));
                return;

            case LH:
                x[rd] = (uint)(short)e.Memory.ReadHalfWord((uint)(x[rs1] + imm));
                return;

            case LW:
                x[rd] = (uint)(int)e.Memory.ReadWord((uint)(x[rs1] + imm));
                return;

            case LBU:
                x[rd] = e.Memory.ReadByte((uint)(x[rs1] + imm));
                return;

            case LHU:
                x[rd] = e.Memory.ReadHalfWord((uint)(x[rs1] + imm));
                return;

            case ADDI:
                x[rd] = (uint)((int)x[rs1] + imm);
                return;

            case SLLI:
                x[rd] = x[rs1] << imm;
                return;

            case SLTI:
                x[rd] = (int)x[rs1] < imm ? (uint)1 : 0;
                return;

            case SLTIU:
                x[rd] = x[rs1] < (uint)imm ? (uint)1 : 0;
                return;

            case XORI:
                x[rd] = x[rs1] ^ (uint)imm;
                return;

            case SRLI:
                x[rd] = x[rs1] >> imm;
                return;

            case SRAI:
                x[rd] = (uint)((int)x[rs1] >> imm);
                return;

            case ORI:
                x[rd] = x[rs1] | (uint)imm;
                return;

            case ANDI:
                x[rd] = x[rs1] & (uint)imm;
                return;

            case MRET:
                // MRET — return from machine-level trap handler.
                // Spec: RISC-V Privileged ISA Section 3.3.2
                //
                // 1. Restore privilege mode from MPP; reset MPP to U.
                // 2. Restore MSTATUS: MIE = MPIE, MPIE = 1.
                // 3. Restore PC from MEPC.
                // 4. Xh3irq: if MEICONTEXT.MRETEIRQ is set, pop the preemption priority stack.
                //    Spec §3.8.6.1.5: "A trap exit where MEICONTEXT.MRETEIRQ is set…"
                {
                    e.CurrentPrivilege = (PrivilegeMode)e.CSR.Mstatus.Mpp;
                    e.CSR.Mstatus.Mpp  = (uint)PrivilegeMode.User;
                    e.CSR.Mstatus.Mie  = e.CSR.Mstatus.Mpie;
                    e.CSR.Mstatus.Mpie = true;
                    e.PC = e.CSR.Mepc.Read();

                    if (e.CSR.Meicontext.Mreteirq)
                        e.CSR.Meicontext.RestoreOnMret();
                }
                return;

            case ECALL:
                e.RaiseECall();
                throw new RiscVException(ExceptionCause.EnvironmentCallFromMMode, e.PC, "ECALL instruction executed");

            case EBREAK:
                e.RaiseEBreak();
                throw new RiscVException(ExceptionCause.Breakpoint, e.PC, "EBREAK instruction executed");

            case WFI:
                // Wait for interrupt - nothing to do
                return;

            case FENCE:
                return;

            case FENCEI:
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in IFormat.");
        }
    }
}

public record ECallParams(uint ServiceNumber, uint Argument);
