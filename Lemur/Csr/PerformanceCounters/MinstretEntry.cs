namespace Lemur.Csr.PerformanceCounters;

// minstret — 0xb02 — lower 32 bits of 64-bit instruction retire counter
public class MinstretEntry : CsrEntry
{
    internal readonly Counter64 m_Counter = new();

    [EntryValue("31:0", "Lower 32 bits of 64-bit instruction retire counter")]
    public uint Low => (uint)m_Counter.Value;

    public MinstretEntry() : base(0xb02, "minstret", "Performance") { }

    protected override uint ReadCore()     => (uint)m_Counter.Value;
    protected override void WriteCore(uint value) => m_Counter.Value = (m_Counter.Value & 0xFFFF_FFFF_0000_0000UL) | value;
}
