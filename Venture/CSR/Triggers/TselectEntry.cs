namespace Venture.Csr.Triggers;

// tselect — 0x7a0 — unimplemented: reads as 0, write causes illegal instruction
public class TselectEntry : CsrEntry
{
    public TselectEntry() : base(0x7a0, "tselect", "Trigger") { }
    public override uint Read()            => 0;
    public override uint Peek()            => 0;
    public override void Write(uint value) => throw new InvalidOperationException("tselect: illegal instruction");
}
