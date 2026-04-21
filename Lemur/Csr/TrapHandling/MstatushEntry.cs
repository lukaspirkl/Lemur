namespace Lemur.Csr.TrapHandling;

// mstatush — 0x310 — hardwired to 0
public class MstatushEntry : CsrEntry
{
    public MstatushEntry() : base(0x310, "mstatush", "Trap") { }
    protected override uint ReadCore()     => 0;
    protected override void WriteCore(uint value) { }
}
