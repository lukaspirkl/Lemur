using System;

namespace Lemur.Csr.Triggers;

// tselect — 0x7a0 — unimplemented: reads as 0, write causes illegal instruction
public class TselectEntry : CsrEntry
{
    public TselectEntry() : base(0x7a0, "tselect", "Trigger") { }
    protected override uint ReadCore()     => 0;
    public override uint Peek()            => 0;
    protected override void WriteCore(uint value) => throw new InvalidOperationException("tselect: illegal instruction");
}
