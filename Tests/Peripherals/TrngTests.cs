using Lemur;

namespace Tests.Peripherals;

public class TrngTests
{
    private const uint TRNG_BASE = 0x400f0000;

    private const uint RNG_IMR            = TRNG_BASE + 0x100;
    private const uint RNG_ISR            = TRNG_BASE + 0x104;
    private const uint RNG_ICR            = TRNG_BASE + 0x108;
    private const uint TRNG_CONFIG        = TRNG_BASE + 0x10c;
    private const uint TRNG_VALID         = TRNG_BASE + 0x110;
    private const uint EHR_DATA0          = TRNG_BASE + 0x114;
    private const uint EHR_DATA1          = TRNG_BASE + 0x118;
    private const uint EHR_DATA2          = TRNG_BASE + 0x11c;
    private const uint EHR_DATA3          = TRNG_BASE + 0x120;
    private const uint EHR_DATA4          = TRNG_BASE + 0x124;
    private const uint EHR_DATA5          = TRNG_BASE + 0x128;
    private const uint RND_SOURCE_ENABLE  = TRNG_BASE + 0x12c;
    private const uint SAMPLE_CNT1        = TRNG_BASE + 0x130;
    private const uint TRNG_DEBUG_CONTROL = TRNG_BASE + 0x138;
    private const uint TRNG_SW_RESET      = TRNG_BASE + 0x140;
    private const uint TRNG_BUSY          = TRNG_BASE + 0x1b8;
    private const uint RST_BITS_COUNTER   = TRNG_BASE + 0x1bc;

    private static void SwReset(IDebuggable sut)
    {
        sut.MemoryWrite32(TRNG_SW_RESET, 1);
    }

    private static void EnableAndWaitForGeneration(IDebuggable sut)
    {
        sut.MemoryWrite32(RND_SOURCE_ENABLE, 1);
        for (int i = 0; i < 100_000; i++)
            if (sut.MemoryRead32(TRNG_BUSY) == 0)
                break;
    }

    [Fact]
    public void AfterSwReset_RegistersAreAtDefaults()
    {
        using var sut = RP2350Builder.Create();

        SwReset(sut);

        Assert.Equal("0x0000000F", sut.MemoryRead32(RNG_IMR).ToHex());
        Assert.Equal("0x0000FFFF", sut.MemoryRead32(SAMPLE_CNT1).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(RNG_ISR).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(TRNG_VALID).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(TRNG_BUSY).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(RND_SOURCE_ENABLE).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(TRNG_DEBUG_CONTROL).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA0).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA1).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA2).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA3).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA4).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA5).ToHex());
    }

    [Fact]
    public void WhenEnabled_GenerationCompletesAndEhrValidIsSet()
    {
        using var sut = RP2350Builder.Create();

        SwReset(sut);
        EnableAndWaitForGeneration(sut);

        Assert.Equal("0x00000000", sut.MemoryRead32(TRNG_BUSY).ToHex());
        Assert.Equal("0x00000001", sut.MemoryRead32(TRNG_VALID).ToHex());
        Assert.Equal("0x00000001", sut.MemoryRead32(RNG_ISR).ToHex());

        sut.MemoryRead32(EHR_DATA5); // clear EHR
        sut.MemoryWrite32(RND_SOURCE_ENABLE, 0);
    }

    [Fact]
    public void ReadingEhrData5_ClearsAllEhrRegistersAndTrngValid()
    {
        using var sut = RP2350Builder.Create();

        SwReset(sut);
        EnableAndWaitForGeneration(sut);

        sut.MemoryRead32(EHR_DATA0);
        sut.MemoryRead32(EHR_DATA1);
        sut.MemoryRead32(EHR_DATA2);
        sut.MemoryRead32(EHR_DATA3);
        sut.MemoryRead32(EHR_DATA4);
        sut.MemoryRead32(EHR_DATA5); // triggers the clear

        Assert.Equal("0x00000000", sut.MemoryRead32(TRNG_VALID).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA0).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA1).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA2).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA3).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA4).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA5).ToHex());

        sut.MemoryWrite32(RND_SOURCE_ENABLE, 0);
    }

    [Fact]
    public void RngIcr_ClearsEhrValidBitInIsr()
    {
        using var sut = RP2350Builder.Create();

        SwReset(sut);
        EnableAndWaitForGeneration(sut);

        Assert.Equal("0x00000001", sut.MemoryRead32(RNG_ISR).ToHex());

        sut.MemoryWrite32(RNG_ICR, 0x1);

        Assert.Equal("0x00000000", sut.MemoryRead32(RNG_ISR).ToHex());

        sut.MemoryRead32(EHR_DATA5); // clear EHR
        sut.MemoryWrite32(RND_SOURCE_ENABLE, 0);
    }

    [Fact]
    public void SwReset_ClearsGeneratedData()
    {
        using var sut = RP2350Builder.Create();

        SwReset(sut);
        EnableAndWaitForGeneration(sut);

        Assert.Equal("0x00000001", sut.MemoryRead32(TRNG_VALID).ToHex()); // sanity

        SwReset(sut);

        Assert.Equal("0x00000000", sut.MemoryRead32(TRNG_VALID).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(RNG_ISR).ToHex());
        Assert.Equal("0x00000000", sut.MemoryRead32(EHR_DATA0).ToHex());
    }

    [Fact]
    public void TrngConfig_StoresTwoBits()
    {
        using var sut = RP2350Builder.Create();

        SwReset(sut);

        sut.MemoryWrite32(TRNG_CONFIG, 0x3);
        Assert.Equal("0x00000003", sut.MemoryRead32(TRNG_CONFIG).ToHex());

        sut.MemoryWrite32(TRNG_CONFIG, 0x2);
        Assert.Equal("0x00000002", sut.MemoryRead32(TRNG_CONFIG).ToHex());

        sut.MemoryWrite32(TRNG_CONFIG, 0x1);
        Assert.Equal("0x00000001", sut.MemoryRead32(TRNG_CONFIG).ToHex());

        sut.MemoryWrite32(TRNG_CONFIG, 0x0);
        Assert.Equal("0x00000000", sut.MemoryRead32(TRNG_CONFIG).ToHex());
    }

    [Fact]
    public void RstBitsCounter_DoesNotClearAlreadyCompletedResult()
    {
        using var sut = RP2350Builder.Create();

        SwReset(sut);
        EnableAndWaitForGeneration(sut);

        Assert.Equal("0x00000001", sut.MemoryRead32(TRNG_VALID).ToHex()); // sanity

        sut.MemoryWrite32(RND_SOURCE_ENABLE, 0);
        sut.MemoryWrite32(RST_BITS_COUNTER, 1);

        // RST_BITS_COUNTER resets the in-flight sampling counter, not completed EHR data
        Assert.Equal("0x00000001", sut.MemoryRead32(TRNG_VALID).ToHex());

        sut.MemoryRead32(EHR_DATA5); // clear EHR
    }
}
