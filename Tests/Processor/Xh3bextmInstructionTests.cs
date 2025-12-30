namespace Tests.Processor;

public class Xh3bextmInstructionTests
{
    const uint rd = 8;
    const uint rs1 = 9;
    const uint rs2 = 10;

    [Fact]
    public void h3_bextm()
    {
        using var rp2350 = RP2350Builder.Create();

        var x = rp2350.Registers;

        rp2350.MemoryWrite(0x20000000, InstructionBuilder.H3bextm(rd, rs1, rs2, size: 7));

        x[rd] = 0x00000000;
        x[rs1] = 0x1234ABCD;
        x[rs2] = 0x00000008;

        rp2350.PC = 0x20000000;

        rp2350.Step();

        Assert.Equal((uint)0x000000AB, x[rd]);
    }

    [Fact]
    public void h3_bextmi()
    {
        using var rp2350 = RP2350Builder.Create();

        var x = rp2350.Registers;

        rp2350.MemoryWrite(0x20000000, InstructionBuilder.H3bextmi(rd, rs1, 8, size: 7));

        x[rd] = 0x00000000;
        x[rs1] = 0x1234ABCD;

        rp2350.PC = 0x20000000;

        rp2350.Step();

        Assert.Equal((uint)0x000000AB, x[rd]);
    }
}
