namespace Venture.Csr.CustomInterrupts;

// meipra — 0xBE3 — external interrupt priority array (4-bit priority per IRQ)
public class MeipraEntry : CsrEntry, ICsrWindowed
{
    // 128 windows × 4 IRQs each = 512 IRQs with 4-bit priorities
    private readonly uint[] m_Windows = new uint[128];
    private int m_Index;

    public MeipraEntry() : base(0xBE3, "meipra", "Custom IRQ") { }

    public int  WindowCount   => 128;
    public int  BitsPerWindow => 16;
    public int  BitsPerItem   => 4;
    public uint PeekWindow(int index) => m_Windows[index];

    protected override uint ReadCore()     => m_Windows[m_Index] << 16;
    protected override void WriteCore(uint value) { m_Index = (int)(value & 0x7Fu); m_Windows[m_Index] = value >> 16 & 0xFFFF; }

    public byte GetPriority(int irq) => (byte)(m_Windows[irq >> 2] >> ((irq & 3) << 2) & 0xF);
}
