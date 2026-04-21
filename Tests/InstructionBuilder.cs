using Lemur;

namespace Tests;

public static class InstructionBuilder
{
    public static byte[] MRET()
    {
        return BitConverter.GetBytes(0x30200073u);
    }

    public static byte[] EBREAK()
    {
        return BitConverter.GetBytes(0x00100073u);
    }

    /// <summary>
    /// NOP — no operation. Encoded as ADDI x0, x0, 0.
    /// Advances PC by 4 without touching any register or memory.
    /// Useful in tests as a "do nothing" instruction to let CheckInterrupts() run.
    /// </summary>
    public static byte[] NOP()
    {
        return BitConverter.GetBytes(0x00000013u);
    }

    /// <summary>
    /// ECALL — environment call. Raises a synchronous exception with cause 11
    /// (EnvironmentCallFromMMode) when executing in M-mode. Used by firmware as
    /// the system-call instruction to ask the operating environment for services.
    /// </summary>
    public static byte[] ECALL()
    {
        return BitConverter.GetBytes(0x00000073u);
    }

    /// <summary>
    /// ADDI rd, rs1, imm — add immediate.
    /// I-type format: rd = rs1 + sign_extend(imm[11:0]).
    /// </summary>
    public static byte[] ADDI(uint rd, uint rs1, int imm)
    {
        uint encoding = 0b0010011u
            | (rd  & 0x1Fu) << 7
            | 0b000u        << 12
            | (rs1 & 0x1Fu) << 15
            | ((uint)(imm & 0xFFF)) << 20;
        return BitConverter.GetBytes(encoding);
    }

    public static byte[] LB(uint rd, uint rs1)
    {
        return BitConverter.GetBytes(
            0b0000011
             | rd.ExtractBits(0, 5) << 7
             | 0b000 << 12
             | rs1.ExtractBits(0, 5) << 15
        );
    }

    public static byte[] LH(uint rd, uint rs1)
    {
        return BitConverter.GetBytes(
             0b0000011
             | rd.ExtractBits(0, 5) << 7
             | 0b001 << 12
             | rs1.ExtractBits(0, 5) << 15
        );
    }

    public static byte[] LW(uint rd, uint rs1)
    {
        return BitConverter.GetBytes(
            0b0000011
            | rd.ExtractBits(0, 5) << 7
            | 0b010 << 12
            | rs1.ExtractBits(0, 5) << 15
        );
    }

    public static byte[] SB(uint rs1, uint rs2)
    {
        return BitConverter.GetBytes(
            0b0100011
            | 0b000 << 12
            | rs1.ExtractBits(0, 5) << 15
            | rs2.ExtractBits(0, 5) << 20
        );
    }

    public static byte[] SH(uint rs1, uint rs2)
    {
        return BitConverter.GetBytes(
            0b0100011
            | 0b001 << 12
            | rs1.ExtractBits(0, 5) << 15
            | rs2.ExtractBits(0, 5) << 20
         );
    }

    public static byte[] SW(uint rs1, uint rs2)
    {
        return BitConverter.GetBytes(
            0b0100011
            | 0b010 << 12
            | rs1.ExtractBits(0, 5) << 15
            | rs2.ExtractBits(0, 5) << 20
        );
    }

    public static byte[] H3bextm(uint rd, uint rs1, uint rs2, uint size)
    {
        return BitConverter.GetBytes(
            0b0001011
            | rd.ExtractBits(0, 5) << 7
            | 0b000 << 12 // funct3
            | rs1.ExtractBits(0, 5) << 15
            | rs2.ExtractBits(0, 5) << 20
            | size.ExtractBits(0, 3) << 26
        );
    }

    public static byte[] H3bextmi(uint rd, uint rs1, uint shamt, uint size)
    {
        return BitConverter.GetBytes(
            0b0001011
            | rd.ExtractBits(0, 5) << 7
            | 0b100 << 12 // funct3
            | rs1.ExtractBits(0, 5) << 15
            | shamt.ExtractBits(0, 5) << 20
            | size.ExtractBits(0, 3) << 26
        );
    }
}