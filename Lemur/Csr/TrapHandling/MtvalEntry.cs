namespace Lemur.Csr.TrapHandling;

// mtval — 0x343
// Hazard3 HW hardwires this to zero, but the emulator populates it for compliance.
public class MtvalEntry : CsrEntry
{
    private uint m_Value;

    public MtvalEntry() : base(0x343, "mtval", "Trap") { }
    protected override uint ReadCore()            => m_Value;
    protected override void WriteCore(uint value) => m_Value = value;
}
