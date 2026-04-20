namespace Venture.Csr.TrapHandling;

// Catch-all for registers hardwired to 0 with no side-effects.
public class HardwiredZeroEntry : CsrEntry
{
    public HardwiredZeroEntry(ushort address, string name, string group)
        : base(address, name, group) { }
    public override uint Read()            => 0;
    public override void Write(uint value) { }
}
