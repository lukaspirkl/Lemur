using Lemur;

namespace Tests.Peripherals;

public class Sha256Tests
{
    private const uint SHA256_BASE  = 0x400f8000;
    private const uint SHA256_CSR   = SHA256_BASE + 0x00;
    private const uint SHA256_WDATA = SHA256_BASE + 0x04;
    private const uint SHA256_SUM0  = SHA256_BASE + 0x08;

    // Write START + DMA_SIZE=32BIT + optional BSWAP to CSR
    private static void Start(IDebuggable sut, bool bswap = true)
    {
        uint csr = 0x0201u; // START(bit0)=1, DMA_SIZE(bits9:8)=0x2(32BIT)
        if (bswap) csr |= 1u << 12;
        sut.MemoryWrite32(SHA256_CSR, csr);
    }

    private static void WriteBlock(IDebuggable sut, params uint[] words)
    {
        foreach (var w in words)
            sut.MemoryWrite32(SHA256_WDATA, w);
    }

    private static void AssertSum(IDebuggable sut,
        uint s0, uint s1, uint s2, uint s3,
        uint s4, uint s5, uint s6, uint s7)
    {
        Assert.Equal(s0.ToHex(), sut.MemoryRead32(SHA256_SUM0 + 0x00).ToHex());
        Assert.Equal(s1.ToHex(), sut.MemoryRead32(SHA256_SUM0 + 0x04).ToHex());
        Assert.Equal(s2.ToHex(), sut.MemoryRead32(SHA256_SUM0 + 0x08).ToHex());
        Assert.Equal(s3.ToHex(), sut.MemoryRead32(SHA256_SUM0 + 0x0C).ToHex());
        Assert.Equal(s4.ToHex(), sut.MemoryRead32(SHA256_SUM0 + 0x10).ToHex());
        Assert.Equal(s5.ToHex(), sut.MemoryRead32(SHA256_SUM0 + 0x14).ToHex());
        Assert.Equal(s6.ToHex(), sut.MemoryRead32(SHA256_SUM0 + 0x18).ToHex());
        Assert.Equal(s7.ToHex(), sut.MemoryRead32(SHA256_SUM0 + 0x1C).ToHex());
    }

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

    [Fact]
    public void CsrAfterStart_HasWdataRdyAndSumVldAndDmaSize32AndBswap()
    {
        using var sut = RP2350Builder.Create();
        Start(sut);
        // START auto-clears (SC). Readable bits: WDATA_RDY(1)=1, SUM_VLD(2)=1, DMA_SIZE(9:8)=0x2, BSWAP(12)=1
        Assert.Equal("0x00001206", sut.MemoryRead32(SHA256_CSR).ToHex());
    }

    [Fact]
    public void SumRegistersAfterStart_ContainSha256InitialHashConstants()
    {
        using var sut = RP2350Builder.Create();
        Start(sut);
        // SUM_VLD=1 immediately after START; SUM0-SUM7 hold fractional bits of sqrt(first 8 primes)
        AssertSum(sut,
            0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A,
            0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19);
    }

    [Fact]
    public void SumVld_GoesLowAfterFirstWdataWrite()
    {
        using var sut = RP2350Builder.Create();
        Start(sut);
        sut.MemoryWrite32(SHA256_WDATA, 0x00000000);
        // SUM_VLD(bit2)=0 after first write; WDATA_RDY(bit1)=1, DMA_SIZE=0x2, BSWAP=1
        Assert.Equal("0x00001202", sut.MemoryRead32(SHA256_CSR).ToHex());
    }

    [Fact]
    public void SumVld_ReturnsHighAfterFullBlockDigest()
    {
        using var sut = RP2350Builder.Create();
        Start(sut);
        // Write 16 words to complete one block; all zeros is fine — we only care about SUM_VLD
        WriteBlock(sut,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000);
        // After block digest: SUM_VLD(bit2)=1, WDATA_RDY(bit1)=1
        uint csr = sut.MemoryRead32(SHA256_CSR);
        Assert.Equal(1u, (csr >> 2) & 1u); // SUM_VLD
        Assert.Equal(1u, (csr >> 1) & 1u); // WDATA_RDY
    }

    [Fact]
    public void KnownHash_EmptyMessage()
    {
        using var sut = RP2350Builder.Create();
        Start(sut);
        // SHA-256("") padding (BSWAP=1: write LE words, hardware byte-swaps to BE before SHA core)
        // Big-endian W0=0x80000000, W1-W15=0x00000000  →  write LE: 0x00000080, then 15×0x00000000
        WriteBlock(sut,
            0x00000080, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000);
        // SHA-256("") = e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
        AssertSum(sut,
            0xE3B0C442, 0x98FC1C14, 0x9AFBF4C8, 0x996FB924,
            0x27AE41E4, 0x649B934C, 0xA495991B, 0x7852B855);
    }

    [Fact]
    public void KnownHash_Abc()
    {
        using var sut = RP2350Builder.Create();
        Start(sut);
        // SHA-256("abc"): message = 0x61 0x62 0x63, length = 24 bits = 0x18
        // Big-endian: W0=0x61626380 ("abc"+0x80), W1-W14=0, W15=0x00000018
        // BSWAP=1: write byte-swapped LE values
        WriteBlock(sut,
            0x80636261, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x18000000);
        // SHA-256("abc") = ba7816bf8f01cfea414140de5dae2ec73b338c2ba999e02614155016 55e83f09
        AssertSum(sut,
            0xBA7816BF, 0x8F01CFEA, 0x414140DE, 0x5DAE2223,
            0xB00361A3, 0x96177A9C, 0xB410FF61, 0xF20015AD);
    }

    [Fact]
    public void KnownHash_QuickBrownFox()
    {
        using var sut = RP2350Builder.Create();
        Start(sut);
        // "The quick brown fox jumps over the lazy dog" = 43 bytes, length = 344 bits = 0x158
        // Padded to 64 bytes (one block): message + 0x80 + 12 zero bytes + 8-byte BE length
        // Big-endian words:
        //   0x54686520 "The " | 0x71756963 "quic" | 0x6B206272 "k br" | 0x6F776E20 "own "
        //   0x666F7820 "fox " | 0x6A756D70 "jump" | 0x73206F76 "s ov" | 0x65722074 "er t"
        //   0x6865206C "he l" | 0x617A7920 "azy " | 0x646F6780 "dog\x80" | 0×0000_0000
        //   0x00000000       | 0x00000000         | 0x00000000          | 0x00000158
        // BSWAP=1: write each word byte-swapped
        WriteBlock(sut,
            0x20656854, 0x63697571, 0x7262206B, 0x206E776F,
            0x20786F66, 0x706D756A, 0x766F2073, 0x74207265,
            0x6C206568, 0x207A7961, 0x80676F64, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x58010000);
        // SHA-256("The quick brown fox jumps over the lazy dog")
        //   = d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592
        AssertSum(sut,
            0x99C4A6C0, 0x7C657D7C, 0xC5838CF8, 0xAFB59EF8,
            0x2D87063A, 0x07F9FE3C, 0x2260E658, 0x19E6F1C4);
    }

    [Fact]
    public void Start_ResetsStateBetweenConsecutiveHashes()
    {
        using var sut = RP2350Builder.Create();

        // First: SHA-256("")
        Start(sut);
        WriteBlock(sut,
            0x00000080, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000);
        Assert.Equal("0xE3B0C442", sut.MemoryRead32(SHA256_SUM0).ToHex());

        // Re-init and compute SHA-256("abc")
        Start(sut);
        WriteBlock(sut,
            0x80636261, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x18000000);
        AssertSum(sut,
            0xBA7816BF, 0x8F01CFEA, 0x414140DE, 0x5DAE2223,
            0xB00361A3, 0x96177A9C, 0xB410FF61, 0xF20015AD);
    }

    [Fact]
    public void BswapDisabled_HashesCorrectlyFromBigEndianWords()
    {
        using var sut = RP2350Builder.Create();
        Start(sut, bswap: false); // BSWAP=0: 32-bit words are passed straight to the SHA core
        // Write big-endian message words directly — no byte-swap needed in software
        // SHA-256("abc"): W0=0x61626380, W15=0x00000018
        WriteBlock(sut,
            0x61626380, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000018);
        // Same hash as KnownHash_Abc — BSWAP only affects how software feeds data, not the result
        AssertSum(sut,
            0xBA7816BF, 0x8F01CFEA, 0x414140DE, 0x5DAE2223,
            0xB00361A3, 0x96177A9C, 0xB410FF61, 0xF20015AD);
    }
}
