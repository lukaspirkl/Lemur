using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Venture.Peripherals;

public class Sha256 : PeripheralBase
{
    private readonly FileLogger fileLogger = new FileLogger("sha256.log");

    // Control and status register
    private const uint SCR = 0x00;

    private List<byte> wdata = new List<byte>(64);
    private uint[] H = new uint[8];

    private bool wdata_rdy = false;
    private bool sum_vld = false;
    private DmaSize dma_size = DmaSize.bit32;
    private bool bswap = true;

    private enum DmaSize
    {
        bit8 = 0x0,
        bit16 = 0x1,
        bit32 = 0x2,
    }

    public Sha256() : base(0x400f8000)
    {
        fileLogger.Clear();
    }

    protected override byte[] HandleRead(uint offset, int count)
    {
        fileLogger.Log($"READ offset: {offset.ToHex()} count: {count}");

        if (offset == SCR)
        {
            uint value = 0;

            if (wdata_rdy)
            {
                value |= (uint)1 << 1;
            }

            if (sum_vld)
            {
                value |= (uint)1 << 2;
            }

            value |= (uint)dma_size << 8;

            if (bswap)
            {
                value |= (uint)1 << 12;
            }

            return BitConverter.GetBytes(value);
        }
        else if (0x08 <= offset && offset <= 0x024) // SUM0-SUM7
        {
            var index = (offset - 0x08) / 4;
            return BitConverter.GetBytes(H[index]);
        }
        else
        {
            Console.WriteLine($"WARNING: Reading from {offset.ToHex()} - Sha256");
            return new byte[count];
            //throw new NotImplementedException();
        }
    }

    protected override void HandleWrite(uint offset, byte[] data)
    {
        fileLogger.Log($"WRITE offset: {offset.ToHex()} data: {data.ToHex()}");


        if (offset == SCR)
        {
            var value = BitConverter.ToUInt32(data);
            if (value.ExtractBits(0, 1) == 1) // START
            {
                wdata.Clear();
                wdata_rdy = false;
                sum_vld = false;
                InitialH.CopyTo(H);
            }

            if (value.ExtractBits(4, 1) == 1) // ERR_WDATA_NOT_RDY
            {
                // Set when a write occurs whilst the SHA-256 core is not ready for data(WDATA_RDY is low).Write one to clear.
            }

            dma_size = (DmaSize)value.ExtractBits(8, 2);
            if (dma_size != DmaSize.bit32)
            {
                throw new NotImplementedException("Anything other than 32bit is probably not working. It should be verified!");
            }

            bswap = value.ExtractBits(12, 1) == 1;
        }
        else if (offset == 0x04) // WDATA register
        {
            wdata.AddRange(data);
            if (wdata.Count == 64)
            {
                Digest();
                wdata.Clear();
            }
            wdata_rdy = true;
        }
        else
        {
            var value = BitConverter.ToUInt32(data);
            Console.WriteLine($"WARNING: Writing to {offset.ToHex()} data {value.ToHex()} - Sha256");
            //throw new NotImplementedException();
        }
    }

    private void Digest()
    {
        var M = new UInt32[16];
        var span = CollectionsMarshal.AsSpan(wdata);
        for (int i = 0; i < 16; i++)
        {
            uint word = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(i * 4, 4));

            if (bswap)
            {
                word = BinaryPrimitives.ReverseEndianness(word);
                
            }

            M[i] = word;
        }

        System.Diagnostics.Debug.Assert(M.Length == 16);

        // 1. Prepare the message schedule (W[t]):
        UInt32[] W = new UInt32[64];
        for (int t = 0; t < 16; ++t)
        {
            W[t] = M[t];
        }

        for (int t = 16; t < 64; ++t)
        {
            W[t] = sigma1(W[t - 2]) + W[t - 7] + sigma0(W[t - 15]) + W[t - 16];
        }

        // 2. Initialize the eight working variables with the (i-1)-st hash value:
        UInt32 a = H[0],
               b = H[1],
               c = H[2],
               d = H[3],
               e = H[4],
               f = H[5],
               g = H[6],
               h = H[7];

        // 3. For t=0 to 63:
        for (int t = 0; t < 64; ++t)
        {
            UInt32 T1 = h + Sigma1(e) + Ch(e, f, g) + K[t] + W[t];
            UInt32 T2 = Sigma0(a) + Maj(a, b, c);
            h = g;
            g = f;
            f = e;
            e = d + T1;
            d = c;
            c = b;
            b = a;
            a = T1 + T2;
        }

        // 4. Compute the intermediate hash value H:
        H[0] = a + H[0];
        H[1] = b + H[1];
        H[2] = c + H[2];
        H[3] = d + H[3];
        H[4] = e + H[4];
        H[5] = f + H[5];
        H[6] = g + H[6];
        H[7] = h + H[7];
    }

    private static readonly UInt32[] InitialH = new UInt32[8] {
        0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19
    };

    private static readonly UInt32[] K = new UInt32[64] {
        0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5, 0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
        0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3, 0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
        0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC, 0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
        0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7, 0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
        0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13, 0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
        0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3, 0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
        0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5, 0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
        0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208, 0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
    };

    private static UInt32 ROTL(UInt32 x, byte n)
    {
        System.Diagnostics.Debug.Assert(n < 32);
        return (x << n) | (x >> (32 - n));
    }

    private static UInt32 ROTR(UInt32 x, byte n)
    {
        System.Diagnostics.Debug.Assert(n < 32);
        return (x >> n) | (x << (32 - n));
    }

    private static UInt32 Ch(UInt32 x, UInt32 y, UInt32 z)
    {
        return (x & y) ^ ((~x) & z);
    }

    private static UInt32 Maj(UInt32 x, UInt32 y, UInt32 z)
    {
        return (x & y) ^ (x & z) ^ (y & z);
    }

    private static UInt32 Sigma0(UInt32 x)
    {
        return ROTR(x, 2) ^ ROTR(x, 13) ^ ROTR(x, 22);
    }

    private static UInt32 Sigma1(UInt32 x)
    {
        return ROTR(x, 6) ^ ROTR(x, 11) ^ ROTR(x, 25);
    }

    private static UInt32 sigma0(UInt32 x)
    {
        return ROTR(x, 7) ^ ROTR(x, 18) ^ (x >> 3);
    }

    private static UInt32 sigma1(UInt32 x)
    {
        return ROTR(x, 17) ^ ROTR(x, 19) ^ (x >> 10);
    }
}
