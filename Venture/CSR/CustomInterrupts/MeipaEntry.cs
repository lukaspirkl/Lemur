namespace Venture.Csr.CustomInterrupts;

// meipa — 0xBE1 — external interrupt pending array (computed, software read-only)
// Effective pending = hardware_asserted | meifa (force). Software cannot clear pending bits
// directly; the IRQ source must deassert or meifa must be cleared.
public class MeipaEntry : CsrEntry, ICsrWindowed
{
    private readonly uint[]     m_Hardware = new uint[32]; // driven by hardware peripherals
    private readonly MeifaEntry m_Meifa;
    private int m_Index;

    public MeipaEntry(MeifaEntry meifa) : base(0xBE1, "meipa", "Custom IRQ")
        => m_Meifa = meifa;

    public int  WindowCount   => 32;
    public int  BitsPerWindow => 16;
    public int  BitsPerItem   => 1;
    public uint PeekWindow(int index) => GetWindow(index);

    private uint ComputeWindow(int i) => m_Hardware[i] | m_Meifa.GetWindow(i);

    public override uint Read()            => ComputeWindow(m_Index) << 16;
    public override void Write(uint value) => m_Index = (int)(value & 0x1Fu); // index-only; window is read-only

    public bool IsPending(int irq)   => (ComputeWindow(irq >> 4) & (1u << (irq & 0xF))) != 0;
    public uint GetWindow(int index) =>  ComputeWindow(index);

    /// <summary>Called by hardware peripherals to assert or deassert an IRQ line.</summary>
    public void SetHardwarePending(Irq irq, bool pending)
    {
        int n = (int)irq;
        if (pending) m_Hardware[n >> 4] |=  (1u << (n & 0xF));
        else         m_Hardware[n >> 4] &= ~(1u << (n & 0xF));
    }
}
