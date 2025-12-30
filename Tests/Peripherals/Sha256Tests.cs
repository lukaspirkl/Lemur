namespace Tests.Peripherals;

public class Sha256Tests
{
    [Fact]
    public void WhenUsedFromBootROM()
    {
        using var rp2350 = RP2350Builder.Create();

        uint StartAddress = 0x400f8000;

        rp2350.MemoryWrite(StartAddress + 0x00, BitConverter.GetBytes((uint)0x00001207));

        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x6A9554A9));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x52A5956A));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x552AAA54));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0xA9524A95));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0xAA5456AB));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x52ADA56A));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x00000001));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x00000000));

        // SUM0–SUM7 are initialized to constants when there is less then 16 input values
        Assert.Equal((uint)0x6A09E667, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x08, 4)));
        Assert.Equal((uint)0xBB67AE85, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x0c, 4)));
        Assert.Equal((uint)0x3C6EF372, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x10, 4)));
        Assert.Equal((uint)0xA54FF53A, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x14, 4)));
        Assert.Equal((uint)0x510E527F, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x18, 4)));
        Assert.Equal((uint)0x9B05688C, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x1c, 4)));
        Assert.Equal((uint)0x1F83D9AB, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x20, 4)));
        Assert.Equal((uint)0x5BE0CD19, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x24, 4)));

        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x4AD5A952));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x54AAA952));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0xAB564A95));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x5A95D52A));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x954A2A54));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x4AD452A5));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x00000001));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x00000000));

        // After 16 inputs the new values should be calculated
        Assert.Equal((uint)0x236A8338, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x08, 4)));
        Assert.Equal((uint)0x87126642, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x0c, 4)));
        Assert.Equal((uint)0x5EACD1D4, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x10, 4)));
        Assert.Equal((uint)0x512A65A4, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x14, 4)));
        Assert.Equal((uint)0x750BFEE7, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x18, 4)));
        Assert.Equal((uint)0xD8FAF389, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x1c, 4)));
        Assert.Equal((uint)0x1DC332F3, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x20, 4)));
        Assert.Equal((uint)0xF2EF9CDD, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x24, 4)));

        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0xB326A55A));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x66CD366C));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x9932C99B));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x664CB326));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0xC99B6CD9));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0xC9AC9336));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x00000001));
        rp2350.MemoryWrite(StartAddress + 0x04, BitConverter.GetBytes((uint)0x00000000));

        // The value is still like the one before, because it is recalculated only after 16 values
        Assert.Equal((uint)0x236A8338, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x08, 4)));
        Assert.Equal((uint)0x87126642, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x0c, 4)));
        Assert.Equal((uint)0x5EACD1D4, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x10, 4)));
        Assert.Equal((uint)0x512A65A4, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x14, 4)));
        Assert.Equal((uint)0x750BFEE7, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x18, 4)));
        Assert.Equal((uint)0xD8FAF389, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x1c, 4)));
        Assert.Equal((uint)0x1DC332F3, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x20, 4)));
        Assert.Equal((uint)0xF2EF9CDD, BitConverter.ToUInt32(rp2350.MemoryRead(StartAddress + 0x24, 4)));
    }
}
