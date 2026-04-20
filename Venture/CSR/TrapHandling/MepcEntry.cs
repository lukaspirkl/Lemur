namespace Venture.Csr.TrapHandling;

// mepc — 0x341
public class MepcEntry : CsrEntry
{
    private uint m_Value;
    public MepcEntry() : base(0x341, "mepc", "Trap") { }
    protected override uint ReadCore()     => m_Value;
    protected override void WriteCore(uint value) => m_Value = value & ~0x3u; // bits 1:0 hardwired 0 (no C ext)
}
