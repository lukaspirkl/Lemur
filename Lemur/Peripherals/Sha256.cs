using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lemur.Peripherals;

/// <summary>
/// RP2350 SHA-256 accelerator at 0x400f8000.
///
/// Write START to CSR to initialise state, then write 16 32-bit words per block to WDATA.
/// The 256-bit result is available in SUM0–SUM7 when CSR.SUM_VLD is high.
/// In the emulator, digest completes instantly — WDATA_RDY never goes low.
///
/// Spec: RP2350 Datasheet §12.13 — "SHA-256 Accelerator".
/// </summary>
public class Sha256 : PeripheralBase
{
    private readonly List<byte> m_Wdata = new(64);
    private readonly uint[] m_H = new uint[8];

    private bool m_WdataRdy;
    private bool m_SumVld;
    private bool m_ErrWdataNotRdy;
    private DmaSize m_DmaSize = DmaSize.bit32;
    private bool m_Bswap = true;

    private enum DmaSize
    {
        bit8 = 0x0,
        bit16 = 0x1,
        bit32 = 0x2,
    }

    public Sha256(uint baseAddress, string name, ILogger<Sha256> logger) : base(baseAddress, name, logger)
    {
        // CSR (0x00): control and status.
        // START(0)=SC, WDATA_RDY(1)=RO, SUM_VLD(2)=RO, ERR_WDATA_NOT_RDY(4)=WC,
        // DMA_SIZE(9:8)=RW reset=2, BSWAP(12)=RW reset=1
        AddRegister(0x00, "CSR")
            .Field(8, 2, () => (uint)m_DmaSize, v => m_DmaSize = (DmaSize)v)
            .Field(12, () => m_Bswap, v => m_Bswap = v)
            .OnRead(() =>
            {
                uint v = 0;
                if (m_WdataRdy) v |= 1u << 1;
                if (m_SumVld) v |= 1u << 2;
                if (m_ErrWdataNotRdy) v |= 1u << 4;
                v |= (uint)m_DmaSize << 8;
                if (m_Bswap) v |= 1u << 12;
                return v;
            })
            .OnWrite(v =>
            {
                if ((v & 1u) != 0)
                    DoStart();
                if ((v & (1u << 4)) != 0)
                    m_ErrWdataNotRdy = false;
                if (m_DmaSize != DmaSize.bit32)
                    throw new NotImplementedException("Only 32-bit DMA_SIZE is implemented.");
            });

        // WDATA (0x04): write-only. Accumulates 32-bit words; digest runs after every 16th word.
        AddRegister(0x04, "WDATA")
            .OnRead(() => 0u)
            .OnWrite(HandleWdataWrite);

        // SUM0–SUM7 (0x08–0x24): read-only 256-bit hash output.
        for (int i = 0; i < 8; i++)
        {
            int idx = i;
            AddRegister((uint)(0x08 + i * 4), $"SUM{i}")
                .OnRead(() => m_H[idx]);
        }
    }

    private void DoStart()
    {
        m_Wdata.Clear();
        m_WdataRdy = true;
        m_SumVld = true;
        m_InitialH.CopyTo(m_H);
    }

    private void HandleWdataWrite(uint data)
    {
        if (!m_WdataRdy)
        {
            m_ErrWdataNotRdy = true;
            return;
        }

        if (m_Wdata.Count == 0)
            m_SumVld = false;

        m_Wdata.AddRange(BitConverter.GetBytes(data));

        if (m_Wdata.Count == 64)
        {
            Digest();
            m_Wdata.Clear();
            m_SumVld = true;
        }
    }

    private void Digest()
    {
        var M = new UInt32[16];
        var span = CollectionsMarshal.AsSpan(m_Wdata);
        for (int i = 0; i < 16; i++)
        {
            uint word = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(i * 4, 4));

            if (m_Bswap)
                word = BinaryPrimitives.ReverseEndianness(word);

            M[i] = word;
        }

        System.Diagnostics.Debug.Assert(M.Length == 16);

        // 1. Prepare the message schedule W[t]:
        UInt32[] W = new UInt32[64];
        for (int t = 0; t < 16; ++t)
            W[t] = M[t];

        for (int t = 16; t < 64; ++t)
            W[t] = sigma1(W[t - 2]) + W[t - 7] + sigma0(W[t - 15]) + W[t - 16];

        // 2. Initialize working variables from the previous hash value:
        UInt32 a = m_H[0], b = m_H[1], c = m_H[2], d = m_H[3],
               e = m_H[4], f = m_H[5], g = m_H[6], h = m_H[7];

        // 3. Compression rounds t=0..63:
        for (int t = 0; t < 64; ++t)
        {
            UInt32 T1 = h + Sigma1(e) + Ch(e, f, g) + m_K[t] + W[t];
            UInt32 T2 = Sigma0(a) + Maj(a, b, c);
            h = g; g = f; f = e; e = d + T1;
            d = c; c = b; b = a; a = T1 + T2;
        }

        // 4. Compute the intermediate hash value H:
        m_H[0] += a; m_H[1] += b; m_H[2] += c; m_H[3] += d;
        m_H[4] += e; m_H[5] += f; m_H[6] += g; m_H[7] += h;
    }

    private static readonly UInt32[] m_InitialH = [
        0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19
    ];

    private static readonly UInt32[] m_K = [
        0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5, 0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
        0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3, 0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
        0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC, 0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
        0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7, 0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
        0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13, 0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
        0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3, 0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
        0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5, 0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
        0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208, 0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
    ];

    private static UInt32 ROTR(UInt32 x, byte n)
    {
        System.Diagnostics.Debug.Assert(n < 32);
        return (x >> n) | (x << (32 - n));
    }

    private static UInt32 Ch(UInt32 x, UInt32 y, UInt32 z) => (x & y) ^ (~x & z);

    private static UInt32 Maj(UInt32 x, UInt32 y, UInt32 z) => (x & y) ^ (x & z) ^ (y & z);

    private static UInt32 Sigma0(UInt32 x) => ROTR(x, 2) ^ ROTR(x, 13) ^ ROTR(x, 22);

    private static UInt32 Sigma1(UInt32 x) => ROTR(x, 6) ^ ROTR(x, 11) ^ ROTR(x, 25);

    private static UInt32 sigma0(UInt32 x) => ROTR(x, 7) ^ ROTR(x, 18) ^ (x >> 3);

    private static UInt32 sigma1(UInt32 x) => ROTR(x, 17) ^ ROTR(x, 19) ^ (x >> 10);
}
