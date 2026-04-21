namespace Lemur.Csr.TrapHandling;

// mtval — 0x343 — hardwired to 0
public class MtvalEntry : CsrEntry
{
    public MtvalEntry() : base(0x343, "mtval", "Trap") { }
    protected override uint ReadCore()     => 0;
    protected override void WriteCore(uint value) { }
}
