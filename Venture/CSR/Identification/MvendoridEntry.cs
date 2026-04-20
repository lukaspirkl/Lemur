namespace Venture.Csr.Identification;

// mvendorid — 0xf11 — read-only configurable constant
public class MvendoridEntry : CsrEntry
{
    private readonly uint m_Value;

    [EntryValue("31:7", "JEDEC JEP106 continuation code count (bank − 1)")]
    public uint Bank   => m_Value >> 7;

    [EntryValue("6:0",  "Vendor ID within bank (parity bit not stored)")]
    public uint Offset => m_Value & 0x7Fu;

    public MvendoridEntry(uint value = 0) : base(0xf11, "mvendorid", "Identification")
        => m_Value = value;

    public override uint Read()            => m_Value;
    public override void Write(uint value) { }
}
