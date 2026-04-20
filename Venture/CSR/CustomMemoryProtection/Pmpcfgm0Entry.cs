namespace Venture.Csr.CustomMemoryProtection;

// pmpcfgm0 — 0xbd0 — PMP M-mode configuration (one bit per region, no locking)
public class Pmpcfgm0Entry : CsrEntry
{
    [EntryValue("15:0", "M-mode apply bit per PMP region (OR'd with pmpcfg.L; does not lock)")]
    public ushort M { get; set; }

    public Pmpcfgm0Entry() : base(0xbd0, "pmpcfgm0", "Custom Memory") { }

    public override uint Read()            => M;
    public override void Write(uint value) => M = (ushort)(value & 0xFFFFu);
}
