namespace Venture.Csr.CustomInterrupts;

// meiea — 0xBE0 — external interrupt enable array (1 bit per IRQ)
public class MeieaEntry : CsrEntry, ICsrWindowed
{
    private readonly uint[] m_Windows = new uint[32]; // 32 × 16 bits = 512 IRQ enables
    private int m_Index;

    public MeieaEntry() : base(0xBE0, "meiea", "Custom IRQ") { }

    public int  WindowCount   => 32;
    public int  BitsPerWindow => 16;
    public int  BitsPerItem   => 1;
    public uint PeekWindow(int index) => GetWindow(index);

    protected override uint ReadCore()     => m_Windows[m_Index] << 16;
    protected override void WriteCore(uint value) { m_Index = (int)(value & 0x1Fu); m_Windows[m_Index] = value >> 16 & 0xFFFF; }

    public bool IsEnabled(int irq)   => (m_Windows[irq >> 4] &  (1u << (irq & 0xF))) != 0;
    public uint GetWindow(int index) =>  m_Windows[index];
}
