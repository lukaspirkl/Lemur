namespace Venture.Csr.Identification;

// mimpid — 0xf13 — read-only configurable constant (git hash or 0)
public class MimpidEntry : CsrEntry
{
    private readonly uint m_Value;

    [EntryValue("31:0", "Implementation ID (git hash of synthesised Hazard3 revision, or 0)")]
    public uint ImpId => m_Value;

    public MimpidEntry(uint value = 0) : base(0xf13, "mimpid", "Identification")
        => m_Value = value;

    protected override uint ReadCore()     => m_Value;
    protected override void WriteCore(uint value) { }
}
