namespace Venture.Csr.CustomPower;

// msleep — 0xbf0 — M-mode sleep control (resets to 0)
public class MsleepEntry : CsrEntry
{
    [EntryValue("2", "sleeponblock: enter deep sleep on h3.block as well as wfi")]
    public bool Sleeponblock { get; set; }

    [EntryValue("1", "powerdown: release external power request when sleeping")]
    public bool Powerdown { get; set; }

    [EntryValue("0", "deepsleep: deassert clock enable when entering sleep state")]
    public bool Deepsleep { get; set; }

    public MsleepEntry() : base(0xbf0, "msleep", "Custom Power") { }

    public override uint Read()
    {
        uint v = 0;
        if (Sleeponblock) v |= 1u << 2;
        if (Powerdown)    v |= 1u << 1;
        if (Deepsleep)    v |= 1u;
        return v;
    }

    public override void Write(uint value)
    {
        Sleeponblock = (value >> 2 & 1u) != 0;
        Powerdown    = (value >> 1 & 1u) != 0;
        Deepsleep    =  (value     & 1u) != 0;
    }
}
