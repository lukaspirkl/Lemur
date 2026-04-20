namespace Venture.Csr.Debug;

// dpc — 0x7b1 — debug program counter (R/W in debug mode)
public class DpcEntry : CsrEntry
{
    private uint m_Value;

    [EntryValue("31:0", "Debug program counter")]
    public uint Pc => m_Value;

    public DpcEntry() : base(0x7b1, "dpc", "Debug") { }
    protected override uint ReadCore()     => m_Value;
    protected override void WriteCore(uint value) => m_Value = value & ~0x1u; // bit 0 hardwired 0
}
