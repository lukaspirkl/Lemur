namespace Venture.Csr.PerformanceCounters;

// mcycleh — 0xb80 — upper 32 bits of cycle counter (shares Counter64 with mcycle)
public class McyclehEntry : CsrEntry
{
    private readonly Counter64 m_Counter;

    [EntryValue("31:0", "Upper 32 bits of 64-bit cycle counter")]
    public uint High => (uint)(m_Counter.Value >> 32);

    public McyclehEntry(McycleEntry mcycle) : base(0xb80, "mcycleh", "Performance")
        => m_Counter = mcycle.m_Counter;

    public override uint Read()            => (uint)(m_Counter.Value >> 32);
    public override void Write(uint value) => m_Counter.Value = ((ulong)value << 32) | (uint)m_Counter.Value;
}
