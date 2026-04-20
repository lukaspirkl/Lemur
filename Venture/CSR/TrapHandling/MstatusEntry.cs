namespace Venture.Csr.TrapHandling;

// mstatus — 0x300
public class MstatusEntry : CsrEntry
{
    [EntryValue("21", "timeout wait (U-mode only)")]
    public bool Tw   { get; set; }

    [EntryValue("17", "modify privilege (U-mode only)")]
    public bool Mprv { get; set; }

    [EntryValue("12:11", "previous privilege (0=U, 3=M)")]
    public uint Mpp  { get; set; }

    [EntryValue("7", "previous interrupt enable")]
    public bool Mpie { get; set; }

    [EntryValue("3", "interrupt enable")]
    public bool Mie  { get; set; }

    public MstatusEntry() : base(0x300, "mstatus", "Trap") { }

    public override uint Read()
    {
        uint v = 0;
        if (Tw)   v |= 1u << 21;
        if (Mprv) v |= 1u << 17;
        v |= (Mpp & 0x3u) << 11;
        if (Mpie) v |= 1u << 7;
        if (Mie)  v |= 1u << 3;
        return v;
    }

    public override void Write(uint value)
    {
        Tw   = (value >> 21 & 1u) != 0;
        Mprv = (value >> 17 & 1u) != 0;
        uint mpp = value >> 11 & 0x3u;
        Mpp  = mpp is 0 or 3 ? mpp : 3u; // round unsupported mode to M-mode
        Mpie = (value >>  7 & 1u) != 0;
        Mie  = (value >>  3 & 1u) != 0;
    }
}
