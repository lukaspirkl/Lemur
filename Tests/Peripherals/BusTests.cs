using Lemur;

namespace Tests.Peripherals;

public class BusTests
{
    // The simplest way to test this on real hardware is to use the scratch registers in
    // powman as they don't have any side effects and they can be safely set to any value.

    private const uint POWMAN_BASE = 0x40100000;
    private const uint SCRATCH0 = POWMAN_BASE + 0xB0;
    private const uint SCRATCH1 = POWMAN_BASE + 0xB4;
    private const uint SCRATCH2 = POWMAN_BASE + 0xB8;
    private const uint SCRATCH3 = POWMAN_BASE + 0xBC;
    private const uint SCRATCH4 = POWMAN_BASE + 0xC0;
    private const uint SCRATCH5 = POWMAN_BASE + 0xC4;
    private const uint SCRATCH6 = POWMAN_BASE + 0xC8;
    private const uint SCRATCH7 = POWMAN_BASE + 0xCC;

    private const uint SRAM = 0x20000000;

    private const ushort MCAUSE = 0x342;

    private const uint LOAD_ADDRESS_MISALIGNED = 4;

    [Fact]
    public void LoadAddressMisalignedWhenLW()
    {
        using var sut = RP2350Builder.Create();

        sut.SetCSR(MCAUSE, 0);

        sut.MemoryWrite(SCRATCH0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(SCRATCH1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = SCRATCH0 + 0x2;

        sut.MemoryWrite(SRAM, InstructionBuilder.LW(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        // TODO: Verify trap handler somehow
        var mcause = sut.GetCSR(MCAUSE);
        Assert.Equal(LOAD_ADDRESS_MISALIGNED, mcause);
        Assert.Equal("0x00000000", sut.Registers[1].ToHex());
    }

    [Fact]
    public void LoadAddressMisalignedWhenLH1()
    {
        using var sut = RP2350Builder.Create();

        sut.SetCSR(MCAUSE, 0);

        sut.MemoryWrite(SCRATCH0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(SCRATCH1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = SCRATCH0 + 0x1;

        sut.MemoryWrite(SRAM, InstructionBuilder.LH(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        // TODO: Verify trap handler somehow
        var mcause = sut.GetCSR(MCAUSE);
        Assert.Equal(LOAD_ADDRESS_MISALIGNED, mcause);
        Assert.Equal("0x00000000", sut.Registers[1].ToHex());
    }

    [Fact]
    public void LoadAddressMisalignedWhenLH3()
    {
        using var sut = RP2350Builder.Create();

        sut.SetCSR(MCAUSE, 0);

        sut.MemoryWrite(SCRATCH0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(SCRATCH1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = SCRATCH0 + 0x1;

        sut.MemoryWrite(SRAM, InstructionBuilder.LH(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        // TODO: Verify trap handler somehow
        var mcause = sut.GetCSR(MCAUSE);
        Assert.Equal(LOAD_ADDRESS_MISALIGNED, mcause);
        Assert.Equal("0x00000000", sut.Registers[1].ToHex());
    }

    [Fact]
    public void LoadByte()
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(SCRATCH0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(SCRATCH1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = SCRATCH0 + 0x3;

        sut.MemoryWrite(SRAM, InstructionBuilder.LB(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal("0x00000012", sut.Registers[1].ToHex());
    }

    [Theory]
    [InlineData(0x0000, "0x12345678")]
    [InlineData(0x1000, "0x00000000")]
    [InlineData(0x2000, "0x00000000")]
    [InlineData(0x3000, "0x00000000")]
    [InlineData(0x4000, "0x12345678")]
    public void LoadWord(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.SetCSR(MCAUSE, 0);

        sut.MemoryWrite(SCRATCH0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(SCRATCH1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = SCRATCH0 + addressOffset;

        sut.MemoryWrite(SRAM, InstructionBuilder.LW(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        var mcause = sut.GetCSR(MCAUSE);

        Assert.Equal(expectedValue, sut.Registers[1].ToHex());
    }

    [Theory]
    [InlineData(0x0000, "0x00005678")]
    [InlineData(0x1000, "0x00000000")]
    [InlineData(0x2000, "0x00000000")]
    [InlineData(0x3000, "0x00000000")]
    [InlineData(0x4000, "0x00005678")]
    [InlineData(0x0002, "0x00001234")]
    [InlineData(0x0004, "0xFFFFEF00")]
    public void LoadHalfWord(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(SCRATCH0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(SCRATCH1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = SCRATCH0 + addressOffset + 0x0;

        sut.MemoryWrite(SRAM, InstructionBuilder.LH(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(expectedValue, sut.Registers[1].ToHex());
    }






    [Theory]
    [InlineData(0x0000, "0x10101010")] // normal write access - byte is narrow and it is replicated
    [InlineData(0x0001, "0x10101010")]
    [InlineData(0x0002, "0x10101010")]
    [InlineData(0x0003, "0x10101010")]
    [InlineData(0x4000, "0x00000010")] // offser 0x4000 will fill zeroes instead of replication
    [InlineData(0x4001, "0x00001000")]
    [InlineData(0x4002, "0x00100000")]
    [InlineData(0x4003, "0x10000000")]
    public void WriteByte(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(SCRATCH0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(SCRATCH1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = SCRATCH0 + addressOffset;
        sut.Registers[2] = 0x76543210;

        sut.MemoryWrite(SRAM, InstructionBuilder.SB(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(expectedValue, sut.MemoryRead(SCRATCH0, 4).ToHex());
    }

    [Theory]
    [InlineData(0x1000, "0x0F0FF0F0")]
    [InlineData(0x2000, "0x0F0FFFFF")]
    [InlineData(0x3000, "0x0000F0F0")]
    public void WriteAtomicByte(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(SCRATCH0, BitConverter.GetBytes((uint)0x0000FFFF));

        sut.Registers[1] = SCRATCH0 + addressOffset;
        sut.Registers[2] = 0x0000000F;

        sut.MemoryWrite(SRAM, InstructionBuilder.SB(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(expectedValue, sut.MemoryRead(SCRATCH0, 4).ToHex());
    }

    [Theory]
    [InlineData(0x1000, "0x0F0FF0F0")]
    [InlineData(0x2000, "0x0F0FFFFF")]
    [InlineData(0x3000, "0x0000F0F0")]
    public void WriteAtomicWord(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(SCRATCH0, BitConverter.GetBytes((uint)0x0000FFFF));

        sut.Registers[1] = SCRATCH0 + addressOffset;
        sut.Registers[2] = 0x0F0F0F0F;

        sut.MemoryWrite(SRAM, InstructionBuilder.SW(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(expectedValue, sut.MemoryRead(SCRATCH0, 4).ToHex());
    }



}
