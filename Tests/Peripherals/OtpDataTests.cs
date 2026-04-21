using Lemur;

namespace Tests.Peripherals;

public class OtpDataTests
{
    [Fact]
    public void HardcodedRandomValues()
    {
        using var rp2350 = RP2350Builder.Create();

        uint StartAddress = 0x40130000;

        Assert.Equal("0xB0", rp2350.MemoryRead(StartAddress + 0x00, 1).ToHex());
        Assert.Equal("0xF9", rp2350.MemoryRead(StartAddress + 0x01, 1).ToHex());
        Assert.Equal("0xAF", rp2350.MemoryRead(StartAddress + 0x02, 1).ToHex());
        Assert.Equal("0xCF", rp2350.MemoryRead(StartAddress + 0x03, 1).ToHex());

        //Assert.Equal("0xF9B0", rp2350.MemoryRead(StartAddress + 0x00, 2).ToHex());
        //Assert.Equal("0xAFF9", rp2350.MemoryRead(StartAddress + 0x01, 2).ToHex());
        //Assert.Equal("0xCFAF", rp2350.MemoryRead(StartAddress + 0x02, 2).ToHex());
        //Assert.Equal("0x4BCF", rp2350.MemoryRead(StartAddress + 0x03, 2).ToHex());

        //Assert.Equal("0xAFF9B0", rp2350.MemoryRead(StartAddress + 0x00, 3).ToHex());
        //Assert.Equal("0xCFAFF9", rp2350.MemoryRead(StartAddress + 0x01, 3).ToHex());
        //Assert.Equal("0x4BCFAF", rp2350.MemoryRead(StartAddress + 0x02, 3).ToHex());
        //Assert.Equal("0x2E4BCF", rp2350.MemoryRead(StartAddress + 0x03, 3).ToHex());

        //Assert.Equal("0xCFAFF9B0", rp2350.MemoryRead(StartAddress + 0x00, 4).ToHex());
        //Assert.Equal("0x9F6D2E4B", rp2350.MemoryRead(StartAddress + 0x04, 4).ToHex());


        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x00, 1).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x01, 1).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x02, 1).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x03, 1).ToHex());

        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x00, 2).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x01, 2).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x02, 2).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x03, 2).ToHex());

        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x00, 3).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x01, 3).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x02, 3).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x03, 3).ToHex());

        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x00, 4).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x04, 4).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x08, 4).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x0c, 4).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x10, 4).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x14, 4).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x18, 4).ToHex());
        //Console.WriteLine(rp2350.MemoryRead(StartAddress + 0x1c, 4).ToHex());
    }
}
