namespace Venture.Csr.TrapHandling;

// mepc — 0x341
public class MepcEntry : CsrEntry
{
    private uint m_Value;
    public MepcEntry() : base(0x341, "mepc", "Trap") { }
    public override uint Read()            => m_Value;
    public override void Write(uint value) => m_Value = value & ~0x3u; // bits 1:0 hardwired 0 (no C ext)
}
