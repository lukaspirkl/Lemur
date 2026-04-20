namespace Venture.Csr.Debug;

// dpc — 0x7b1 — debug program counter (R/W in debug mode)
public class DpcEntry : CsrEntry
{
    private uint m_Value;

    [EntryValue("31:0", "Debug program counter")]
    public uint Pc => m_Value;

    public DpcEntry() : base(0x7b1, "dpc", "Debug") { }
    public override uint Read()            => m_Value;
    public override void Write(uint value) => m_Value = value & ~0x1u; // bit 0 hardwired 0
}
