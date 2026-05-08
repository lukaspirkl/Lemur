namespace Lemur.Csr.TrapHandling;

// mepc — 0x341
public class MepcEntry : CsrEntry
{
    private uint m_Value;
    public MepcEntry() : base(0x341, "mepc", "Trap") { }
    protected override uint ReadCore()     => m_Value;
    // C/Zca extension enabled in misa: only bit 0 is hardwired to 0; bit 1 is preserved.
    protected override void WriteCore(uint value) => m_Value = value & ~0x1u;
}
