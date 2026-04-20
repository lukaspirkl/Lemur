namespace Venture.Csr.PerformanceCounters;

// mcycle — 0xb00 — lower 32 bits of 64-bit cycle counter
public class McycleEntry : CsrEntry
{
    internal readonly Counter64 m_Counter = new();

    [EntryValue("31:0", "Lower 32 bits of 64-bit cycle counter")]
    public uint Low => (uint)m_Counter.Value;

    public McycleEntry() : base(0xb00, "mcycle", "Performance") { }

    public override uint Read()            => (uint)m_Counter.Value;
    public override void Write(uint value) => m_Counter.Value = (m_Counter.Value & 0xFFFF_FFFF_0000_0000UL) | value;
}
