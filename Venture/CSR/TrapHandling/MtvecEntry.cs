namespace Venture.Csr.TrapHandling;

// mtvec — 0x305
public class MtvecEntry : CsrEntry
{
    private uint m_Base;
    private bool m_Vectored;

    [EntryValue("31:2", "trap vector base address")]
    public uint Base     { get => m_Base;     set => Set(ref m_Base,     value); }

    [EntryValue("0",    "vectored mode (0=direct, 1=vectored)")]
    public bool Vectored { get => m_Vectored; set => Set(ref m_Vectored, value); }

    public MtvecEntry() : base(0x305, "mtvec", "Trap") { }

    protected override uint ReadCore()     => (m_Base << 2) | (m_Vectored ? 1u : 0u);
    protected override void WriteCore(uint value) { m_Base = value >> 2; m_Vectored = (value & 1u) != 0; }
}
