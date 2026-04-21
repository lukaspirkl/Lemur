namespace Lemur.Csr.Identification;

// mhartid — 0xf14 — read-only configurable constant (per-core identifier)
public class MhartidEntry : CsrEntry
{
    private readonly uint m_Value;

    [EntryValue("31:0", "Hart identifier (unique per core, assigned consecutively from 0)")]
    public uint HartId => m_Value;

    public MhartidEntry(uint hartId = 0) : base(0xf14, "mhartid", "Identification")
        => m_Value = hartId;

    protected override uint ReadCore()     => m_Value;
    protected override void WriteCore(uint value) { }
}
