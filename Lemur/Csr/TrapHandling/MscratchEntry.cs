namespace Lemur.Csr.TrapHandling;

// mscratch — 0x340
public class MscratchEntry : CsrEntry
{
    private uint m_Value;
    public MscratchEntry() : base(0x340, "mscratch", "Trap") { }
    protected override uint ReadCore()     => m_Value;
    protected override void WriteCore(uint value) => m_Value = value;
}
