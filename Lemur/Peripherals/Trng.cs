using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Lemur.Peripherals;

/// <summary>
/// RP2350 True Random Number Generator at 0x400f0000.
///
/// On write to RND_SOURCE_ENABLE, 192 bits of random data are filled into
/// EHR_DATA[0..5] immediately (no timing simulation). TRNG_BUSY always reads 0.
/// Reading EHR_DATA5 clears all six EHR registers and resets EHR_VALID.
/// Writing TRNG_SW_RESET resets all registers to their reset-state values.
///
/// Spec: RP2350 Datasheet §12.12 — "TRNG".
/// </summary>
public class Trng : PeripheralBase
{
    private uint m_Imr = 0xF;
    private uint m_Isr;
    private uint m_Config;
    private bool m_RndSrcEn;
    private uint m_SampleCnt1 = 0x0000_FFFF;
    private uint m_AutocorrStatistic;
    private uint m_DebugControl;
    private bool m_DebugEnInput;
    private bool m_EhrValid;
    private readonly uint[] m_Ehr = new uint[6];

    public Trng(uint baseAddress, string name, ILogger<Trng> logger)
        : base(baseAddress, name, logger)
    {
        // RNG_IMR (0x100): all four interrupt sources masked by default (reset = 0xF).
        AddRegister(0x100, "RNG_IMR", resetValue: 0xF)
            .Field(0, 4, () => m_Imr, v => m_Imr = v);

        // RNG_ISR (0x104): read-only status; bits set by hardware events.
        AddRegister(0x104, "RNG_ISR")
            .OnRead(() => m_Isr);

        // RNG_ICR (0x108): write 1 to clear corresponding ISR bit.
        // Bit 1 (AUTOCORR_ERR) is sticky — only a reset clears it.
        AddRegister(0x108, "RNG_ICR")
            .OnRead(() => 0u)
            .OnWrite(v => m_Isr &= ~(v & 0b1101u)); // skip bit 1

        // TRNG_CONFIG (0x10c): ROSC chain length selection, bits 1:0.
        AddRegister(0x10c, "TRNG_CONFIG")
            .Field(0, 2, () => m_Config, v => m_Config = v);

        // TRNG_VALID (0x110): EHR_VALID bit 0, read-only.
        AddRegister(0x110, "TRNG_VALID")
            .OnRead(() => m_EhrValid ? 1u : 0u);

        // EHR_DATA0–EHR_DATA4 (0x114–0x124): read-only, return 0 until valid.
        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            AddRegister((uint)(0x114 + i * 4), $"EHR_DATA{i}")
                .OnRead(() => m_EhrValid ? m_Ehr[idx] : 0u);
        }

        // EHR_DATA5 (0x128): reading this register clears all EHR registers and EHR_VALID.
        // Spec: "read the last result register, EHR_DATA[5] to clear all result registers".
        AddRegister(0x128, "EHR_DATA5")
            .OnRead(() =>
            {
                uint value = m_EhrValid ? m_Ehr[5] : 0u;
                ClearEhr();
                return value;
            });

        // RND_SOURCE_ENABLE (0x12c): writing bit 0 = 1 starts generation.
        // In the emulator, generation completes instantly — no TRNG_BUSY pulse.
        AddRegister(0x12c, "RND_SOURCE_ENABLE")
            .Field(lsb: 0, getter: () => m_RndSrcEn, setter: v => m_RndSrcEn = v)
            .OnWrite(v =>
            {
                if ((v & 1u) != 0)
                    GenerateRandom();
            });

        // SAMPLE_CNT1 (0x130): clocks between ROSC samples, reset = 0xFFFF.
        AddRegister(0x130, "SAMPLE_CNT1", resetValue: 0x0000_FFFF)
            .Field(0, 32, () => m_SampleCnt1, v => m_SampleCnt1 = v);

        // AUTOCORR_STATISTIC (0x134): any write resets both counters.
        AddRegister(0x134, "AUTOCORR_STATISTIC")
            .OnRead(() => m_AutocorrStatistic)
            .OnWrite(_ => m_AutocorrStatistic = 0);

        // TRNG_DEBUG_CONTROL (0x138): bypass flags at bits 3:1 (bit 0 reserved).
        AddRegister(0x138, "TRNG_DEBUG_CONTROL")
            .Field(1, 3, () => m_DebugControl, v => m_DebugControl = v);

        // TRNG_SW_RESET (0x140): write 1 to reset the entire TRNG block.
        AddRegister(0x140, "TRNG_SW_RESET")
            .OnRead(() => 0u)
            .OnWrite(v =>
            {
                if ((v & 1u) != 0)
                    ApplySoftwareReset();
            });

        // RNG_DEBUG_EN_INPUT (0x1b4): debug mode enable, bit 0.
        AddRegister(0x1b4, "RNG_DEBUG_EN_INPUT")
            .Field(lsb: 0, getter: () => m_DebugEnInput, setter: v => m_DebugEnInput = v);

        // TRNG_BUSY (0x1b8): always 0 — generation completes instantly in the emulator.
        AddRegister(0x1b8, "TRNG_BUSY")
            .OnRead(() => 0u);

        // RST_BITS_COUNTER (0x1bc): resets the in-flight sampling counter.
        // Does not clear already-completed EHR data (confirmed on real hardware).
        AddRegister(0x1bc, "RST_BITS_COUNTER")
            .OnRead(() => 0u)
            .OnWrite(_ => { });

        // RNG_VERSION (0x1c0): read-only capability flags. EHR_WIDTH_192 (bit 0) = 1.
        AddRegister(0x1c0, "RNG_VERSION")
            .OnRead(() => 0x1u);

        // RNG_BIST_CNTR_0/1/2 (0x1e0–0x1e8): BIST counters, read-only, always 0.
        AddRegister(0x1e0, "RNG_BIST_CNTR_0");
        AddRegister(0x1e4, "RNG_BIST_CNTR_1");
        AddRegister(0x1e8, "RNG_BIST_CNTR_2");
    }

    private void GenerateRandom()
    {
        RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(m_Ehr.AsSpan()));
        m_EhrValid = true;
        m_Isr |= 1u; // EHR_VALID
    }

    private void ClearEhr()
    {
        Array.Clear(m_Ehr);
        m_EhrValid = false;
    }

    private void ApplySoftwareReset()
    {
        m_Imr = 0xF;
        m_Isr = 0;
        m_Config = 0;
        m_RndSrcEn = false;
        m_SampleCnt1 = 0x0000_FFFF;
        m_AutocorrStatistic = 0;
        m_DebugControl = 0;
        m_DebugEnInput = false;
        ClearEhr();
    }
}
