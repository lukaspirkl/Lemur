using Venture;

namespace Tests.Peripherals;

public class BootRAMTests
{
    // BootRAM is connected differently. It supports atomic XOR/SET/CLEAR but there are no narrow writes.

    private const uint BOOTRAM_BASE = 0x400e0000;
    private const uint BOOTRAM0 = BOOTRAM_BASE + 0xB0;
    private const uint BOOTRAM1 = BOOTRAM_BASE + 0xB4;

    private const uint SRAM = 0x20000000;

    private const ushort MCAUSE = 0x342;

    private const uint LOAD_ADDRESS_MISALIGNED = 4;

    [Fact]
    public void LoadAddressMisalignedWhenLW()
    {
        using var sut = RP2350Builder.Create();

        sut.SetCSR(MCAUSE, 0);

        sut.MemoryWrite(BOOTRAM0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(BOOTRAM1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = BOOTRAM0 + 0x2;

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

        sut.MemoryWrite(BOOTRAM0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(BOOTRAM1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = BOOTRAM0 + 0x1;

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

        sut.MemoryWrite(BOOTRAM0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(BOOTRAM1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = BOOTRAM0 + 0x1;

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

        sut.MemoryWrite(BOOTRAM0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(BOOTRAM1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = BOOTRAM0 + 0x3;

        sut.MemoryWrite(SRAM, InstructionBuilder.LB(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal("0x00000012", sut.Registers[1].ToHex());
    }

    [Theory]
    [InlineData(0x0000, "0x12345678")]
    [InlineData(0x1000, "0x12345678")]
    [InlineData(0x2000, "0x12345678")]
    [InlineData(0x3000, "0x12345678")]
    [InlineData(0x4000, "0x12345678")]
    public void LoadWord(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.SetCSR(MCAUSE, 0);

        sut.MemoryWrite(BOOTRAM0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(BOOTRAM1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = BOOTRAM0 + addressOffset;

        sut.MemoryWrite(SRAM, InstructionBuilder.LW(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        var mcause = sut.GetCSR(MCAUSE);

        Assert.Equal(expectedValue, sut.Registers[1].ToHex());
    }

    [Theory]
    [InlineData(0x0000, "0x00005678")]
    [InlineData(0x1000, "0x00005678")]
    [InlineData(0x2000, "0x00005678")]
    [InlineData(0x3000, "0x00005678")]
    [InlineData(0x4000, "0x00005678")]
    [InlineData(0x0002, "0x00001234")]
    [InlineData(0x0004, "0xFFFFEF00")]
    public void LoadHalfWord(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(BOOTRAM0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(BOOTRAM1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = 0x00000000;
        sut.Registers[2] = BOOTRAM0 + addressOffset + 0x0;

        sut.MemoryWrite(SRAM, InstructionBuilder.LH(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(expectedValue, sut.Registers[1].ToHex());
    }






    [Theory]
    [InlineData(0x0000, "0x123456AB")] // there is no narrow write so only single byte is changed
    [InlineData(0x0001, "0x1234AB78")]
    [InlineData(0x0002, "0x12AB5678")]
    [InlineData(0x0003, "0xAB345678")]
    [InlineData(0x4000, "0x123456AB")] // nothing is filled with zero so only single byte is changed
    [InlineData(0x4001, "0x1234AB78")]
    [InlineData(0x4002, "0x12AB5678")]
    [InlineData(0x4003, "0xAB345678")]
    public void WriteByte(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(BOOTRAM0, BitConverter.GetBytes((uint)0x12345678));
        sut.MemoryWrite(BOOTRAM1, BitConverter.GetBytes((uint)0xABCDEF00));

        sut.Registers[1] = BOOTRAM0 + addressOffset;
        sut.Registers[2] = 0x000000AB;

        sut.MemoryWrite(SRAM, InstructionBuilder.SB(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(expectedValue, sut.MemoryRead(BOOTRAM0, 4).ToHex());
    }

    [Theory]
    [InlineData(0x0000, "0x0000FF0F")]
    [InlineData(0x1000, "0x0000FFF0")]
    [InlineData(0x2000, "0x0000FFFF")]
    [InlineData(0x3000, "0x0000FFF0")]
    [InlineData(0x0003, "0x0F00FFFF")]
    [InlineData(0x1003, "0x0F00FFFF")]
    [InlineData(0x2003, "0x0F00FFFF")]
    [InlineData(0x3003, "0x0000FFFF")]
    public void WriteAtomicByte(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(BOOTRAM0, BitConverter.GetBytes((uint)0x0000FFFF));

        sut.Registers[1] = BOOTRAM0 + addressOffset;
        sut.Registers[2] = 0x0000000F;

        sut.MemoryWrite(SRAM, InstructionBuilder.SB(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(expectedValue, sut.MemoryRead(BOOTRAM0, 4).ToHex());
    }

    [Theory]
    [InlineData(0x1000, "0x0F0FF0F0")]
    [InlineData(0x2000, "0x0F0FFFFF")]
    [InlineData(0x3000, "0x0000F0F0")]
    public void WriteAtomicWord(uint addressOffset, string expectedValue)
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite(BOOTRAM0, BitConverter.GetBytes((uint)0x0000FFFF));

        sut.Registers[1] = BOOTRAM0 + addressOffset;
        sut.Registers[2] = 0x0F0F0F0F;

        sut.MemoryWrite(SRAM, InstructionBuilder.SW(1, 2));
        sut.Registers[32] = SRAM;

        sut.Step();

        Assert.Equal(expectedValue, sut.MemoryRead(BOOTRAM0, 4).ToHex());
    }



}
