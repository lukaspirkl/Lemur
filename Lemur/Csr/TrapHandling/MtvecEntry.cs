namespace Lemur.Csr.TrapHandling;

// mtvec — 0x305
public class MtvecEntry : CsrEntry
{
    private uint m_Base;
    private uint m_Mode; // bits[1:0]: 0=direct, 1=vectored; bit 1 preserved for readback

    [EntryValue("31:2", "trap vector base address")]
    public uint Base { get => m_Base; set => Set(ref m_Base, value); }

    [EntryValue("1:0", "mode (0=direct, 1=vectored; bit 1 stored for round-trip)")]
    public bool Vectored => (m_Mode & 1u) != 0;

    public MtvecEntry() : base(0x305, "mtvec", "Trap") { }

    protected override uint ReadCore()          => (m_Base << 2) | m_Mode;
    protected override void WriteCore(uint value) { m_Base = value >> 2; m_Mode = value & 3u; }
}
