namespace Lemur.Csr.TrapHandling;

// mcause — 0x342
public class McauseEntry : CsrEntry
{
    private uint m_Value;

    [EntryValue("31", "vectored mode (0=direct, 1=vectored)")]
    public bool IsInterrupt => (m_Value & 0x8000_0000u) != 0;

    [EntryValue("30:0", "vectored mode (0=direct, 1=vectored)")]
    public uint CauseCode   =>  m_Value & 0x7FFF_FFFFu;

    public McauseEntry() : base(0x342, "mcause", "Trap") { }

    protected override uint ReadCore()     => m_Value;

    // Bit 31 + 5 LSBs cover all Hazard3 exception and interrupt causes.
    protected override void WriteCore(uint value) => m_Value = value & 0x8000_001Fu;
}
