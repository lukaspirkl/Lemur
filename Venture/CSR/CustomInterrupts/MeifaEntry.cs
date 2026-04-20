namespace Venture.Csr.CustomInterrupts;

// meifa — 0xBE2 — external interrupt force array (1 bit per IRQ, R/W)
// Declared before meipa because meipa depends on it.
public class MeifaEntry : CsrEntry, ICsrWindowed
{
    private readonly uint[] m_Windows = new uint[32];
    private int m_Index;

    public MeifaEntry() : base(0xBE2, "meifa", "Custom IRQ") { }

    public int  WindowCount   => 32;
    public int  BitsPerWindow => 16;
    public int  BitsPerItem   => 1;
    public uint PeekWindow(int index) => GetWindow(index);

    public override uint Read()            => m_Windows[m_Index] << 16;
    public override void Write(uint value) { m_Index = (int)(value & 0x1Fu); m_Windows[m_Index] = value >> 16 & 0xFFFF; }

    public bool IsForced(int irq)    => (m_Windows[irq >> 4] &  (1u << (irq & 0xF))) != 0;
    public void ClearForced(int irq) =>  m_Windows[irq >> 4] &= ~(1u << (irq & 0xF));
    public uint GetWindow(int index) =>  m_Windows[index];
}
