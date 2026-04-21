using System;

namespace Lemur.Csr.TrapHandling;

// medeleg — 0x302 / mideleg — 0x303 — illegal instruction on access (no S-mode)
public class UnimplementedCsrEntry : CsrEntry
{
    public UnimplementedCsrEntry(ushort address, string name, string group = "Trap") : base(address, name, group) { }
    // Real emulator catches these and raises an illegal-instruction trap.
    protected override uint ReadCore()     => throw new InvalidOperationException($"{Name}: illegal instruction");
    protected override void WriteCore(uint value) => throw new InvalidOperationException($"{Name}: illegal instruction");
    public override uint Peek() => 0;
}
