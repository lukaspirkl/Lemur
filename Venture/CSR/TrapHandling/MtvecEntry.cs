namespace Venture.Csr.TrapHandling;

// mtvec — 0x305
public class MtvecEntry : CsrEntry
{
    [EntryValue("31:2", "trap vector base address")]
    public uint Base     { get; set; }

    [EntryValue("0",    "vectored mode (0=direct, 1=vectored)")]
    public bool Vectored { get; set; }

    public MtvecEntry() : base(0x305, "mtvec", "Trap") { }

    public override uint Read()            => (Base << 2) | (Vectored ? 1u : 0u);
    public override void Write(uint value) { Base = value >> 2; Vectored = (value & 1u) != 0; }
}
