using Venture;

namespace Tests;

public static class InstructionBuilder
{
    public static byte[] MRET()
    {
        return BitConverter.GetBytes(0x30200073u);
    }

    public static byte[] EBREAK()
    {
        return BitConverter.GetBytes(0x00100073);
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
}