namespace Tests.Processor;

public class Xh3bextmInstructionTests
{
    const uint RD = 8;
    const uint RS1 = 9;
    const uint RS2 = 10;

    [Fact]
    public void h3_bextm()
    {
        using var rp2350 = RP2350Builder.Create();

        var x = rp2350.Registers;

        rp2350.MemoryWrite(0x20000000, InstructionBuilder.H3bextm(RD, RS1, RS2, size: 7));

        x[RD] = 0x00000000;
        x[RS1] = 0x1234ABCD;
        x[RS2] = 0x00000008;

        rp2350.PC = 0x20000000;

        rp2350.Step();

        Assert.Equal((uint)0x000000AB, x[RD]);
    }

    [Fact]
    public void h3_bextmi()
    {
        using var rp2350 = RP2350Builder.Create();

        var x = rp2350.Registers;

        rp2350.MemoryWrite(0x20000000, InstructionBuilder.H3bextmi(RD, RS1, 8, size: 7));

        x[RD] = 0x00000000;
        x[RS1] = 0x1234ABCD;

        rp2350.PC = 0x20000000;

        rp2350.Step();

        Assert.Equal((uint)0x000000AB, x[RD]);
    }
}
