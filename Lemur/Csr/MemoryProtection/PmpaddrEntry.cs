using System;

namespace Lemur.Csr.MemoryProtection;

// pmpaddr0…15 — 0x3b0 through 0x3bf — PMP region address registers (30-bit each)
// A register is read-only while its corresponding pmpcfg region has the L (lock) bit set.
public class PmpaddrEntry : CsrEntry
{
    private uint m_Value;
    private readonly Func<bool> m_IsLocked;
    private readonly int m_Grain;
    private readonly Func<byte> m_GetA;

    [EntryValue("29:0", "PMP address in units of 4 bytes (30 significant bits)")]
    public uint Addr => m_Value;

    public PmpaddrEntry(ushort address, int index, Func<bool> isLocked, int grain, Func<byte> getA)
        : base(address, $"pmpaddr{index}", "Memory Protection")
    {
        m_IsLocked = isLocked;
        m_Grain = grain;
        m_GetA = getA;
    }

    protected override uint ReadCore()
    {
        if (m_Grain <= 1) return m_Value;

        byte a = m_GetA();
        uint mask = (1u << (m_Grain - 1)) - 1u;

        if (a == 0) // OFF
            return m_Value & ~mask;
        if (a == 3) // NAPOT
            return m_Value | mask;

        return m_Value;
    }

    protected override void WriteCore(uint value)
    {
        if (m_IsLocked()) return;

        if (m_Grain > 1)
        {
            byte a = m_GetA();
            uint mask = (1u << (m_Grain - 1)) - 1u;
            if (a == 0) value &= ~mask;
            else if (a == 3) value |= mask;
        }

        m_Value = value & 0x3FFF_FFFFu;
    }
}
