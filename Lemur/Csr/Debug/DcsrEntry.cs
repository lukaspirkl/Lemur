namespace Lemur.Csr.Debug;

// dcsr — 0x7b0 — debug control and status
// xdebugver hardwired 4; stepie hardwired 0; stopcount/stoptime hardwired 1.
public class DcsrEntry : CsrEntry
{
    [EntryValue("31:28", "xdebugver: external debug spec version (hardwired to 4)")]
    public uint Xdebugver => 4;

    private bool m_Ebreakm;
    private bool m_Ebreaku;
    private uint m_Cause;
    private bool m_Step;
    private uint m_Prv = 3;

    [EntryValue("15", "ebreakm: ebreak in M-mode enters debug mode instead of trapping")]
    public bool Ebreakm { get => m_Ebreakm; set => Set(ref m_Ebreakm, value); }

    [EntryValue("12", "ebreaku: ebreak in U-mode enters debug mode (hardwired 0 if no U-mode)")]
    public bool Ebreaku { get => m_Ebreaku; set => Set(ref m_Ebreaku, value); }

    [EntryValue("8:6", "cause: reason for debug mode entry (read-only, set by hardware)")]
    public uint Cause { get => m_Cause; private set => Set(ref m_Cause, value); }

    [EntryValue("2", "step: re-enter debug mode after each M-mode instruction")]
    public bool Step { get => m_Step; set => Set(ref m_Step, value); }

    [EntryValue("1:0", "prv: privilege level at debug entry / exit (3=M, 0=U)")]
    public uint Prv { get => m_Prv; set => Set(ref m_Prv, value); }

    public DcsrEntry() : base(0x7b0, "dcsr", "Debug") { }

    protected override uint ReadCore()
    {
        uint v = 4u << 28;           // xdebugver
        if (m_Ebreakm) v |= 1u << 15;
        if (m_Ebreaku) v |= 1u << 12;
        // stepie = 0 (hardwired)
        v |= 1u << 10;               // stopcount hardwired 1
        v |= 1u <<  9;               // stoptime hardwired 1
        v |= (m_Cause & 0x7u) << 6;
        if (m_Step) v |= 1u << 2;
        v |= m_Prv & 0x3u;
        return v;
    }

    protected override void WriteCore(uint value)
    {
        m_Ebreakm = (value >> 15 & 1u) != 0;
        m_Ebreaku = (value >> 12 & 1u) != 0;
        m_Step    = (value >>  2 & 1u) != 0;
        uint prv = value & 0x3u;
        m_Prv = prv is 0 or 3 ? prv : 3u;
    }

    internal void SetCause(uint cause) => Cause = cause & 0x7u;
}
