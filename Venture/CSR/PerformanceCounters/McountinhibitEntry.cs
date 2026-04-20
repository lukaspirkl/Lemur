namespace Venture.Csr.PerformanceCounters;

// mcountinhibit — 0x320 — counter inhibit (ir and cy reset to 1)
public class McountinhibitEntry : CsrEntry
{
    [EntryValue("2", "inhibit minstret/minstreth counting (resets to 1)")]
    public bool Ir { get; set; } = true;

    [EntryValue("0", "inhibit mcycle/mcycleh counting (resets to 1)")]
    public bool Cy { get; set; } = true;

    public McountinhibitEntry() : base(0x320, "mcountinhibit", "Performance") { }

    public override uint Read()            => (Ir ? 4u : 0u) | (Cy ? 1u : 0u);
    public override void Write(uint value) { Ir = (value & 4u) != 0; Cy = (value & 1u) != 0; }
}
