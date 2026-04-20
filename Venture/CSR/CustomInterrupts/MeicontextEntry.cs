using Venture.Csr.TrapHandling;

namespace Venture.Csr.CustomInterrupts;

// meicontext — 0xBE5 — external interrupt context register
// Declared before meinext because meinext calls ApplyUpdate on it.
public class MeicontextEntry : CsrEntry
{
    private readonly MieEntry m_Mie;

    // Three-level preemption priority stack — saved on vector entry, restored on mret.
    // preempt is 5 bits (bits 20:16); ppreempt/pppreempt are 4 bits each (bits 27:24, 31:28).
    // Values > 15 in preempt (e.g. 16 = "disable all") are truncated when shifted to ppreempt.
    // Software must save/restore meicontext to a memory stack for arbitrary nesting.
    private byte m_Pppreempt;
    private byte m_Ppreempt;
    private byte m_Preempt;
    private bool m_NoIrq;
    private uint m_Irq;
    private bool m_Mreteirq;

    [EntryValue("31:28", "previous-previous preemption priority")]
    public byte Pppreempt { get => m_Pppreempt; private set => Set(ref m_Pppreempt, value); }

    [EntryValue("27:24", "previous preemption priority")]
    public byte Ppreempt  { get => m_Ppreempt;  private set => Set(ref m_Ppreempt,  value); }

    [EntryValue("20:16", "current preemption priority")]
    public byte Preempt   { get => m_Preempt;   private set => Set(ref m_Preempt,   value); }

    [EntryValue("15",   "not in interrupt context")]
    public bool NoIrq    { get => m_NoIrq;    private set => Set(ref m_NoIrq,    value); }

    [EntryValue("12:4", "current IRQ number")]
    public uint Irq      { get => m_Irq;      private set => Set(ref m_Irq,      value); }

    [EntryValue("0",    "restore priority stack on mret")]
    public bool Mreteirq { get => m_Mreteirq; private set => Set(ref m_Mreteirq, value); }

    // mtiesave (bit 3) and msiesave (bit 2) are pass-through reads of mie.Mtie/Msie.
    // Because the emulator calls Get() before Set() in a csrrs, Read() captures the
    // pre-clearts value naturally — no special GetForWrite needed.

    public MeicontextEntry(MieEntry mie) : base(0xBE5, "meicontext", "Custom IRQ")
    {
        m_Mie   = mie;
        m_NoIrq = true; // reset value: not in interrupt
    }

    protected override uint ReadCore()
    {
        uint v = 0;
        v |= (uint)(m_Pppreempt & 0xFu) << 28;
        v |= (uint)(m_Ppreempt  & 0xFu) << 24;
        v |= (uint)(m_Preempt  & 0x1Fu) << 16;
        if (m_NoIrq)     v |= 1u << 15;
        v |= (m_Irq & 0x1FFu) << 4;
        if (m_Mie.Mtie)  v |= 1u << 3;  // mtiesave: live read of mie.Mtie
        if (m_Mie.Msie)  v |= 1u << 2;  // msiesave: live read of mie.Msie
        // bit 1 (clearts): always 0 on read (write-only self-clearing)
        if (m_Mreteirq)  v |= 1u;
        return v;
    }

    protected override void WriteCore(uint value)
    {
        m_Pppreempt = (byte)(value >> 28 & 0xFu);
        m_Ppreempt  = (byte)(value >> 24 & 0xFu);
        m_Preempt   = (byte)(value >> 16 & 0x1Fu);
        m_NoIrq     = (value >> 15 & 1u) != 0;
        m_Irq       =  value >>  4 & 0x1FFu;
        m_Mreteirq  = (value        & 1u) != 0;

        // mtiesave/msiesave writes are ORed into mie; clearts takes precedence if both written.
        if ((value >> 3 & 1u) != 0) m_Mie.Mtie = true;
        if ((value >> 2 & 1u) != 0) m_Mie.Msie = true;
        if ((value >> 1 & 1u) != 0) { m_Mie.Mtie = false; m_Mie.Msie = false; }
    }

    /// <summary>
    /// Shifts the priority stack and records the current IRQ.
    /// Called by <see cref="MeinextEntry"/> when the update bit is written,
    /// and by <see cref="OnExternalVectorEntry"/> when hardware enters the IRQ vector.
    /// </summary>
    internal void ApplyUpdate(bool noIrq, int irq, MeipraEntry meipra)
    {
        Pppreempt = Ppreempt;
        Ppreempt  = Preempt;
        // 0x10 (16) is one above the maximum 4-bit priority (15), disabling preemption.
        Preempt   = noIrq ? (byte)0x10 : (byte)(meipra.GetPriority(irq) + 1);
        NoIrq     = noIrq;
        Irq       = (uint)irq;
    }

    /// <summary>Called by the CPU when hardware takes the external interrupt vector.</summary>
    public void OnExternalVectorEntry(bool noIrq, int irq, MeipraEntry meipra)
    {
        ApplyUpdate(noIrq, irq, meipra);
        Mreteirq = true;
    }

    /// <summary>Called by the CPU on mret when mreteirq is set.</summary>
    public void RestoreOnMret()
    {
        Preempt   = Ppreempt;
        Ppreempt  = Pppreempt;
        Pppreempt = 0;
        Mreteirq  = false;
    }
}
