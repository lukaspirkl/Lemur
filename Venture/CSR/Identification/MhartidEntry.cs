namespace Venture.Csr.Identification;

// mhartid — 0xf14 — read-only configurable constant (per-core identifier)
public class MhartidEntry : CsrEntry
{
    private readonly uint m_Value;

    [EntryValue("31:0", "Hart identifier (unique per core, assigned consecutively from 0)")]
    public uint HartId => m_Value;

    public MhartidEntry(uint hartId = 0) : base(0xf14, "mhartid", "Identification")
        => m_Value = hartId;

    public override uint Read()            => m_Value;
    public override void Write(uint value) { }
}
