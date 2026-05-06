using System;

namespace Lemur.Csr.MemoryProtection;

// pmpaddr0…15 — 0x3b0 through 0x3bf — PMP region address registers (30-bit each)
// A register is read-only while its corresponding pmpcfg region has the L (lock) bit set.
public class PmpaddrEntry : CsrEntry
{
    private uint m_Value;
    private readonly Func<bool> m_IsLocked;

    [EntryValue("29:0", "PMP address in units of 4 bytes (30 significant bits)")]
    public uint Addr => m_Value;

    public PmpaddrEntry(ushort address, int index, Func<bool> isLocked)
        : base(address, $"pmpaddr{index}", "Memory Protection")
    {
        m_IsLocked = isLocked;
    }

    protected override uint ReadCore() => m_Value;

    protected override void WriteCore(uint value)
    {
        if (!m_IsLocked()) m_Value = value & 0x3FFF_FFFFu;
    }
}
