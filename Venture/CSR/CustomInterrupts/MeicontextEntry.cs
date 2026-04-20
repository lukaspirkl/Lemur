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
    [EntryValue("31:28", "previous-previous preemption priority")]
    public byte Pppreempt { get; private set; }

    [EntryValue("27:24", "previous preemption priority")]
    public byte Ppreempt  { get; private set; }

    [EntryValue("20:16", "current preemption priority")]
    public byte Preempt   { get; private set; }

    [EntryValue("15",   "not in interrupt context")]
    public bool NoIrq    { get; private set; }

    [EntryValue("12:4", "current IRQ number")]
    public uint Irq      { get; private set; }

    [EntryValue("0",    "restore priority stack on mret")]
    public bool Mreteirq { get; private set; }

    // mtiesave (bit 3) and msiesave (bit 2) are pass-through reads of mie.Mtie/Msie.
    // Because the emulator calls Get() before Set() in a csrrs, Read() captures the
    // pre-clearts value naturally — no special GetForWrite needed.

    public MeicontextEntry(MieEntry mie) : base(0xBE5, "meicontext", "Custom IRQ")
    {
        m_Mie = mie;
        NoIrq = true; // reset value: not in interrupt
    }

    public override uint Read()
    {
        uint v = 0;
        v |= (uint)(Pppreempt & 0xFu) << 28;
        v |= (uint)(Ppreempt  & 0xFu) << 24;
        v |= (uint)(Preempt  & 0x1Fu) << 16;
        if (NoIrq)       v |= 1u << 15;
        v |= (Irq & 0x1FFu) << 4;
        if (m_Mie.Mtie)  v |= 1u << 3;  // mtiesave: live read of mie.Mtie
        if (m_Mie.Msie)  v |= 1u << 2;  // msiesave: live read of mie.Msie
        // bit 1 (clearts): always 0 on read (write-only self-clearing)
        if (Mreteirq)    v |= 1u;
        return v;
    }

    public override void Write(uint value)
    {
        Pppreempt = (byte)(value >> 28 & 0xFu);
        Ppreempt  = (byte)(value >> 24 & 0xFu);
        Preempt   = (byte)(value >> 16 & 0x1Fu);
        NoIrq     = (value >> 15 & 1u) != 0;
        Irq       =  value >>  4 & 0x1FFu;
        Mreteirq  = (value        & 1u) != 0;

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
