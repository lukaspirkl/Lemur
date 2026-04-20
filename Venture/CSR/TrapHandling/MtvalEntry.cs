namespace Venture.Csr.TrapHandling;

// mtval — 0x343 — hardwired to 0
public class MtvalEntry : CsrEntry
{
    public MtvalEntry() : base(0x343, "mtval", "Trap") { }
    public override uint Read()            => 0;
    public override void Write(uint value) { }
}
