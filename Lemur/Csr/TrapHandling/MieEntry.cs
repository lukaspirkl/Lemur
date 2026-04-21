namespace Lemur.Csr.TrapHandling;

// mie — 0x304 — per-source interrupt enables
public class MieEntry : CsrEntry
{
    private bool m_Meie;
    private bool m_Mtie;
    private bool m_Msie;

    [EntryValue("11", "external interrupt enable")]
    public bool Meie { get => m_Meie; set => Set(ref m_Meie, value); }

    [EntryValue("7",  "timer interrupt enable")]
    public bool Mtie { get => m_Mtie; set => Set(ref m_Mtie, value); }

    [EntryValue("3",  "software interrupt enable")]
    public bool Msie { get => m_Msie; set => Set(ref m_Msie, value); }

    public MieEntry() : base(0x304, "mie", "Trap") { }

    protected override uint ReadCore()
    {
        uint v = 0;
        if (m_Meie) v |= 1u << 11;
        if (m_Mtie) v |= 1u <<  7;
        if (m_Msie) v |= 1u <<  3;
        return v;
    }

    protected override void WriteCore(uint value)
    {
        m_Meie = (value >> 11 & 1u) != 0;
        m_Mtie = (value >>  7 & 1u) != 0;
        m_Msie = (value >>  3 & 1u) != 0;
    }
}
