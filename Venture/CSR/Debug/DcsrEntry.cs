namespace Venture.Csr.Debug;

// dcsr — 0x7b0 — debug control and status
// xdebugver hardwired 4; stepie hardwired 0; stopcount/stoptime hardwired 1.
public class DcsrEntry : CsrEntry
{
    [EntryValue("31:28", "xdebugver: external debug spec version (hardwired to 4)")]
    public uint Xdebugver => 4;

    [EntryValue("15", "ebreakm: ebreak in M-mode enters debug mode instead of trapping")]
    public bool Ebreakm { get; set; }

    [EntryValue("12", "ebreaku: ebreak in U-mode enters debug mode (hardwired 0 if no U-mode)")]
    public bool Ebreaku { get; set; }

    [EntryValue("8:6", "cause: reason for debug mode entry (read-only, set by hardware)")]
    public uint Cause { get; private set; }

    [EntryValue("2", "step: re-enter debug mode after each M-mode instruction")]
    public bool Step { get; set; }

    [EntryValue("1:0", "prv: privilege level at debug entry / exit (3=M, 0=U)")]
    public uint Prv { get; set; } = 3;

    public DcsrEntry() : base(0x7b0, "dcsr", "Debug") { }

    public override uint Read()
    {
        uint v = 4u << 28;           // xdebugver
        if (Ebreakm) v |= 1u << 15;
        if (Ebreaku) v |= 1u << 12;
        // stepie = 0 (hardwired)
        v |= 1u << 10;               // stopcount hardwired 1
        v |= 1u <<  9;               // stoptime hardwired 1
        v |= (Cause & 0x7u) << 6;
        if (Step) v |= 1u << 2;
        v |= Prv & 0x3u;
        return v;
    }

    public override void Write(uint value)
    {
        Ebreakm = (value >> 15 & 1u) != 0;
        Ebreaku = (value >> 12 & 1u) != 0;
        Step    = (value >>  2 & 1u) != 0;
        uint prv = value & 0x3u;
        Prv = prv is 0 or 3 ? prv : 3u;
    }

    internal void SetCause(uint cause) => Cause = cause & 0x7u;
}
