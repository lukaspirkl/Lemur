namespace Lemur.Csr.CustomPower;

// msleep — 0xbf0 — M-mode sleep control (resets to 0)
public class MsleepEntry : CsrEntry
{
    private bool m_Sleeponblock;
    private bool m_Powerdown;
    private bool m_Deepsleep;

    [EntryValue("2", "sleeponblock: enter deep sleep on h3.block as well as wfi")]
    public bool Sleeponblock { get => m_Sleeponblock; set => Set(ref m_Sleeponblock, value); }

    [EntryValue("1", "powerdown: release external power request when sleeping")]
    public bool Powerdown { get => m_Powerdown; set => Set(ref m_Powerdown, value); }

    [EntryValue("0", "deepsleep: deassert clock enable when entering sleep state")]
    public bool Deepsleep { get => m_Deepsleep; set => Set(ref m_Deepsleep, value); }

    public MsleepEntry() : base(0xbf0, "msleep", "Custom Power") { }

    protected override uint ReadCore()
    {
        uint v = 0;
        if (m_Sleeponblock) v |= 1u << 2;
        if (m_Powerdown)    v |= 1u << 1;
        if (m_Deepsleep)    v |= 1u;
        return v;
    }

    protected override void WriteCore(uint value)
    {
        m_Sleeponblock = (value >> 2 & 1u) != 0;
        m_Powerdown    = (value >> 1 & 1u) != 0;
        m_Deepsleep    =  (value     & 1u) != 0;
    }
}
