namespace Lemur.Csr.CustomMemoryProtection;

// pmpcfgm0 — 0xbd0 — PMP M-mode configuration (one bit per region, no locking)
public class Pmpcfgm0Entry : CsrEntry
{
    private ushort m_M;

    [EntryValue("15:0", "M-mode apply bit per PMP region (OR'd with pmpcfg.L; does not lock)")]
    public ushort M { get => m_M; set => Set(ref m_M, value); }

    public Pmpcfgm0Entry() : base(0xbd0, "pmpcfgm0", "Custom Memory") { }

    protected override uint ReadCore()     => m_M;
    protected override void WriteCore(uint value) => m_M = (ushort)(value & 0xFFFFu);
}
