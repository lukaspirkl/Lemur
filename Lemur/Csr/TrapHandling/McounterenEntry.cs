namespace Lemur.Csr.TrapHandling;

// mcounteren — 0x306 — U-mode counter access control
public class McounterenEntry : CsrEntry
{
    private bool m_Ir;
    private bool m_Tm;
    private bool m_Cy;

    [EntryValue("2", "instret access (U-mode)")]
    public bool Ir { get => m_Ir; set => Set(ref m_Ir, value); }

    [EntryValue("1", "time access (U-mode)")]
    public bool Tm { get => m_Tm; set => Set(ref m_Tm, value); }

    [EntryValue("0", "cycle access (U-mode)")]
    public bool Cy { get => m_Cy; set => Set(ref m_Cy, value); }

    public McounterenEntry() : base(0x306, "mcounteren", "Trap") { }
    protected override uint ReadCore()     => (m_Ir ? 4u : 0u) | (m_Tm ? 2u : 0u) | (m_Cy ? 1u : 0u);
    protected override void WriteCore(uint value) { m_Ir = (value & 4u) != 0; m_Tm = (value & 2u) != 0; m_Cy = (value & 1u) != 0; }
}
