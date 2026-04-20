namespace Venture.Csr.PerformanceCounters;

// minstreth — 0xb82 — upper 32 bits of instruction retire counter
public class MinstrethEntry : CsrEntry
{
    private readonly Counter64 m_Counter;

    [EntryValue("31:0", "Upper 32 bits of 64-bit instruction retire counter")]
    public uint High => (uint)(m_Counter.Value >> 32);

    public MinstrethEntry(MinstretEntry minstret) : base(0xb82, "minstreth", "Performance")
        => m_Counter = minstret.m_Counter;

    public override uint Read()            => (uint)(m_Counter.Value >> 32);
    public override void Write(uint value) => m_Counter.Value = ((ulong)value << 32) | (uint)m_Counter.Value;
}
