namespace Venture.Csr.TrapHandling;

// mie — 0x304 — per-source interrupt enables
public class MieEntry : CsrEntry
{
    [EntryValue("11", "external interrupt enable")]
    public bool Meie { get; set; }

    [EntryValue("7",  "timer interrupt enable")]
    public bool Mtie { get; set; }

    [EntryValue("3",  "software interrupt enable")]
    public bool Msie { get; set; }

    public MieEntry() : base(0x304, "mie", "Trap") { }

    public override uint Read()
    {
        uint v = 0;
        if (Meie) v |= 1u << 11;
        if (Mtie) v |= 1u <<  7;
        if (Msie) v |= 1u <<  3;
        return v;
    }

    public override void Write(uint value)
    {
        Meie = (value >> 11 & 1u) != 0;
        Mtie = (value >>  7 & 1u) != 0;
        Msie = (value >>  3 & 1u) != 0;
    }
}
