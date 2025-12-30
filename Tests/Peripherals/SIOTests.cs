namespace Tests.Peripherals;

public class SIOTests
{
    [Fact]
    public void ValuesFromHardware()
    {
        using var rp2350 = RP2350Builder.Create();

        uint StartAddress = 0xd0000000;

        Assert.Equal((uint)0x00000000, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x00, 4)));
        Assert.Equal((uint)0x02000000, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x04, 4)));
        Assert.Equal((uint)0xC8000000, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x08, 4)));
        //Assert.Equal((uint)0xC7163275, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x0c, 4)));
        //Assert.Equal((uint)0xEFE1B77B, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x10, 4)));
        //Assert.Equal((uint)0x413AEE51, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x14, 4)));
        //Assert.Equal((uint)0x00000000, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x18, 4)));
        //Assert.Equal((uint)0x00000000, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x1c, 4)));

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
