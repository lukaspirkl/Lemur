namespace Lemur.Csr.TrapHandling;

// Catch-all for registers hardwired to 0 with no side-effects.
public class HardwiredZeroEntry : CsrEntry
{
    public HardwiredZeroEntry(ushort address, string name, string group)
        : base(address, name, group) { }
    protected override uint ReadCore()     => 0;
    protected override void WriteCore(uint value) { }
}
