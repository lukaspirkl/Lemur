namespace Venture.Csr.PerformanceCounters;

// mcountinhibit — 0x320 — counter inhibit (ir and cy reset to 1)
public class McountinhibitEntry : CsrEntry
{
    private bool m_Ir = true;
    private bool m_Cy = true;

    [EntryValue("2", "inhibit minstret/minstreth counting (resets to 1)")]
    public bool Ir { get => m_Ir; set => Set(ref m_Ir, value); }

    [EntryValue("0", "inhibit mcycle/mcycleh counting (resets to 1)")]
    public bool Cy { get => m_Cy; set => Set(ref m_Cy, value); }

    public McountinhibitEntry() : base(0x320, "mcountinhibit", "Performance") { }

    protected override uint ReadCore()     => (m_Ir ? 4u : 0u) | (m_Cy ? 1u : 0u);
    protected override void WriteCore(uint value) { m_Ir = (value & 4u) != 0; m_Cy = (value & 1u) != 0; }
}
