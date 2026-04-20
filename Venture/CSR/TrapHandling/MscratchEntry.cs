namespace Venture.Csr.TrapHandling;

// mscratch — 0x340
public class MscratchEntry : CsrEntry
{
    private uint m_Value;
    public MscratchEntry() : base(0x340, "mscratch", "Trap") { }
    public override uint Read()            => m_Value;
    public override void Write(uint value) => m_Value = value;
}
