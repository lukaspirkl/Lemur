namespace Lemur.Csr.TrapHandling;

// mstatus — 0x300
public class MstatusEntry : CsrEntry
{
    private bool m_Tw;
    private bool m_Mprv;
    private uint m_Mpp;
    private bool m_Mpie;
    private bool m_Mie;

    [EntryValue("21", "timeout wait (U-mode only)")]
    public bool Tw   { get => m_Tw;   set => Set(ref m_Tw,   value); }

    [EntryValue("17", "modify privilege (U-mode only)")]
    public bool Mprv { get => m_Mprv; set => Set(ref m_Mprv, value); }

    [EntryValue("12:11", "previous privilege (0=U, 3=M)")]
    public uint Mpp  { get => m_Mpp;  set => Set(ref m_Mpp,  value); }

    [EntryValue("7", "previous interrupt enable")]
    public bool Mpie { get => m_Mpie; set => Set(ref m_Mpie, value); }

    [EntryValue("3", "interrupt enable")]
    public bool Mie  { get => m_Mie;  set => Set(ref m_Mie,  value); }

    public MstatusEntry() : base(0x300, "mstatus", "Trap") { }

    protected override uint ReadCore()
    {
        uint v = 0;
        if (m_Tw)   v |= 1u << 21;
        if (m_Mprv) v |= 1u << 17;
        v |= (m_Mpp & 0x3u) << 11;
        if (m_Mpie) v |= 1u << 7;
        if (m_Mie)  v |= 1u << 3;
        return v;
    }

    protected override void WriteCore(uint value)
    {
        m_Tw   = (value >> 21 & 1u) != 0;
        m_Mprv = (value >> 17 & 1u) != 0;
        uint mpp = value >> 11 & 0x3u;
        m_Mpp  = mpp is 0 or 3 ? mpp : 3u; // round unsupported mode to M-mode
        m_Mpie = (value >>  7 & 1u) != 0;
        m_Mie  = (value >>  3 & 1u) != 0;
    }
}
