namespace Lemur.Csr.TrapHandling;

// medeleg — 0x302 / mideleg — 0x303 — illegal instruction on access (no S-mode)
public class UnimplementedCsrEntry : CsrEntry
{
    public UnimplementedCsrEntry(ushort address, string name, string group = "Trap") : base(address, name, group) { }
    protected override uint ReadCore()            => throw new RiscVException(ExceptionCause.IllegalInstruction, 0, $"{Name}: illegal instruction");
    protected override void WriteCore(uint value) => throw new RiscVException(ExceptionCause.IllegalInstruction, 0, $"{Name}: illegal instruction");
    public override uint Peek() => 0;
}
