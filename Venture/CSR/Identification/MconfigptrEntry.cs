namespace Venture.Csr.Identification;

// mconfigptr — 0xf15 — read-only, pointer to configuration data structure or 0
public class MconfigptrEntry : CsrEntry
{
    private readonly uint m_Value;

    [EntryValue("31:0", "Pointer to configuration data structure (4-byte aligned), or 0")]
    public uint Ptr => m_Value;

    public MconfigptrEntry(uint value = 0) : base(0xf15, "mconfigptr", "Identification")
        => m_Value = value;

    protected override uint ReadCore()     => m_Value;
    protected override void WriteCore(uint value) { }
}
