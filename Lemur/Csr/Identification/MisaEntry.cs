namespace Lemur.Csr.Identification;

// misa — 0x301 — read-only, ISA capability register
public class MisaEntry : CsrEntry
{
    [EntryValue("31:30", "MXL: machine XLEN (always 1 = 32-bit)")]
    public uint Mxl => 1;

    [EntryValue("23", "X: custom extension present")]
    public bool X { get; }

    [EntryValue("20", "U: user mode supported")]
    public bool U { get; }

    [EntryValue("12", "M: integer multiply/divide extension")]
    public bool M { get; }

    [EntryValue("2",  "C: compressed instruction extension")]
    public bool C { get; }

    [EntryValue("0",  "A: atomic instruction extension")]
    public bool A { get; }

    public MisaEntry(bool hasUMode = false, bool hasMExt = false, bool hasCExt = false,
                     bool hasAExt = false, bool hasCustom = false)
        : base(0x301, "misa", "Identification")
    {
        U = hasUMode; M = hasMExt; C = hasCExt; A = hasAExt; X = hasCustom;
    }

    protected override uint ReadCore()
    {
        uint v = 1u << 30; // MXL = 1
        if (X) v |= 1u << 23;
        if (U) v |= 1u << 20;
        if (M) v |= 1u << 12;
        if (C) v |= 1u <<  2;
        if (A) v |= 1u;
        return v;
    }

    protected override void WriteCore(uint value) { }
}
