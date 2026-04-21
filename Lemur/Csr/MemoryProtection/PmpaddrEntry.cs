namespace Lemur.Csr.MemoryProtection;

// pmpaddr0…15 — 0x3b0 through 0x3bf — PMP region address registers (30-bit each)
public class PmpaddrEntry : CsrEntry
{
    private uint m_Value;

    [EntryValue("29:0", "PMP address in units of 4 bytes (30 significant bits)")]
    public uint Addr => m_Value;

    public PmpaddrEntry(ushort address, int index)
        : base(address, $"pmpaddr{index}", "Memory Protection") { }

    protected override uint ReadCore()     => m_Value;
    protected override void WriteCore(uint value) => m_Value = value & 0x3FFF_FFFFu;
}
