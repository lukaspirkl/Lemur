using Venture;

namespace Tests.Peripherals;

public class SIOTests
{
    private const uint SIO_BASE = 0xd0000000;
    private const uint GPIO_OUT = SIO_BASE + 0x010;
    private const uint GPIO_OUT_XOR = SIO_BASE + 0x028;
    private const uint GPIO_OE = SIO_BASE + 0x030;
    private const uint GPIO_OE_XOR = SIO_BASE + 0x048;

    [Fact]
    public void ValuesFromHardware()
    {
        using var sut = RP2350Builder.CreateOpenOCD();

        //sut.MemoryWrite32(GPIO_OUT_XOR, 0b10000000000000000000000000);
        sut.MemoryWrite32(GPIO_OE_XOR, 0b10000000000000000000000000);

        Console.WriteLine(sut.MemoryRead32(GPIO_OE).ToBin(8));
        Console.WriteLine(sut.MemoryRead32(GPIO_OUT).ToBin(8));
    }
}
