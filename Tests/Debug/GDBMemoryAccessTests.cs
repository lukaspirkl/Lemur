using Tests;
using Venture;

namespace Tests.Debug;

public class GDBMemoryAccessTests
{
    const uint POWMAN_BASE = 0x40100000;
    const uint SCRATCH0 = POWMAN_BASE + 0xB0;
    const uint SCRATCH1 = POWMAN_BASE + 0xB4;
    const uint SCRATCH2 = POWMAN_BASE + 0xB8;

    [Fact]
    public void WriteThreeBytes()
    {
        using var sut = RP2350Builder.Create();

        // Clean up
        sut.MemoryWrite(SCRATCH0, new byte[4]);

        sut.MemoryWrite(SCRATCH0 + 1, [0xAB, 0xCD, 0xEF]);

        Assert.Equal("0xEFCDEFCD", sut.MemoryRead(SCRATCH0, 4).ToHex());
    }

    [Fact]
    public void SingleByteNarrow()
    {
        using var sut = RP2350Builder.Create();

        // Clean up
        sut.MemoryWrite(SCRATCH0, new byte[4]);
        sut.MemoryWrite(SCRATCH1, new byte[4]);

        sut.MemoryWrite(SCRATCH0, [(byte)0xAB]);

        Assert.Equal("0xABABABAB", sut.MemoryRead(SCRATCH0, 4).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead(SCRATCH1, 4).ToHex());
    }

    [Fact]
    public void SingleByteZeros()
    {
        using var sut = RP2350Builder.Create();

        // Clean up
        sut.MemoryWrite(SCRATCH0, new byte[4]);
        sut.MemoryWrite(SCRATCH1, new byte[4]);

        sut.MemoryWrite(SCRATCH0 + 0x4000, [(byte)0xAB]);

        Assert.Equal("0x000000AB", sut.MemoryRead(SCRATCH0, 4).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead(SCRATCH1, 4).ToHex());
    }

    [Fact]
    public void MisalignedWord1()
    {
        using var sut = RP2350Builder.Create();

        // Clean up
        sut.MemoryWrite(SCRATCH0, new byte[4]);
        sut.MemoryWrite(SCRATCH1, new byte[4]);

        sut.MemoryWrite(SCRATCH0+1, BitConverter.GetBytes((uint)0x12345678));

        Assert.Equal("0x34563456", sut.MemoryRead(SCRATCH0, 4).ToHex());
        Assert.Equal("0x12121212", sut.MemoryRead(SCRATCH1, 4).ToHex());

        Console.WriteLine(sut.MemoryRead(SCRATCH0, 4).ToHex());
        Console.WriteLine(sut.MemoryRead(SCRATCH1, 4).ToHex());
    }

    [Fact]
    public void MisalignedWord2()
    {
        using var sut = RP2350Builder.Create();

        // Clean up
        sut.MemoryWrite(SCRATCH0, new byte[4]);
        sut.MemoryWrite(SCRATCH1, new byte[4]);

        sut.MemoryWrite(SCRATCH0 + 2, BitConverter.GetBytes((uint)0x12345678));

        Assert.Equal("0x56785678", sut.MemoryRead(SCRATCH0, 4).ToHex());
        Assert.Equal("0x12341234", sut.MemoryRead(SCRATCH1, 4).ToHex());

        Console.WriteLine(sut.MemoryRead(SCRATCH0, 4).ToHex());
        Console.WriteLine(sut.MemoryRead(SCRATCH1, 4).ToHex());
    }

    [Fact]
    public void WriteMoreData()
    {
        using var sut = RP2350Builder.Create();

        // Clean up
        sut.MemoryWrite(SCRATCH0, new byte[4]);
        sut.MemoryWrite(SCRATCH1, new byte[4]);

        sut.MemoryWrite(SCRATCH0, [0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44]);

        Assert.Equal("0xDDCCBBAA", sut.MemoryRead(SCRATCH0, 4).ToHex());
        Assert.Equal("0x44332211", sut.MemoryRead(SCRATCH1, 4).ToHex());

        Console.WriteLine(sut.MemoryRead(SCRATCH0, 4).ToHex());
        Console.WriteLine(sut.MemoryRead(SCRATCH1, 4).ToHex());
    }

    [Fact]
    public void WriteMoreDataMisaligned()
    {
        using var sut = RP2350Builder.Create();

        // Clean up
        sut.MemoryWrite(SCRATCH0, new byte[4]);
        sut.MemoryWrite(SCRATCH1, new byte[4]);
        sut.MemoryWrite(SCRATCH2, new byte[4]);

        sut.MemoryWrite(SCRATCH0+1, [0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66]);

        Assert.Equal("0xCCBBCCBB", sut.MemoryRead(SCRATCH0, 4).ToHex());
        Assert.Equal("0x332211DD", sut.MemoryRead(SCRATCH1, 4).ToHex());
        Assert.Equal("0x66666666", sut.MemoryRead(SCRATCH2, 4).ToHex());
    }

    [Fact]
    public void ReadMoreData()
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(SCRATCH0, [0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44]);

        Assert.Equal(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 }, sut.MemoryRead(SCRATCH0, 8));
    }

    [Fact]
    public void ReadMoreData2()
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(SCRATCH0, [0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44, 0x66, 0x77, 0x88, 0x99]);

        Assert.Equal(new byte[] { 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44, 0x66, 0x77 }, sut.MemoryRead(SCRATCH0 + 2, 8));
    }
}
