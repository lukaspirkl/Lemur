namespace Venture.Csr.TrapHandling;

// mcounteren — 0x306 — U-mode counter access control
public class McounterenEntry : CsrEntry
{
    [EntryValue("2", "instret access (U-mode)")]
    public bool Ir { get; set; }

    [EntryValue("1", "time access (U-mode)")]
    public bool Tm { get; set; }

    [EntryValue("0", "cycle access (U-mode)")]
    public bool Cy { get; set; }

    public McounterenEntry() : base(0x306, "mcounteren", "Trap") { }
    public override uint Read()            => (Ir ? 4u : 0u) | (Tm ? 2u : 0u) | (Cy ? 1u : 0u);
    public override void Write(uint value) { Ir = (value & 4u) != 0; Tm = (value & 2u) != 0; Cy = (value & 1u) != 0; }
}
