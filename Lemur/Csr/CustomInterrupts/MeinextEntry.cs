namespace Lemur.Csr.CustomInterrupts;

// meinext — 0xBE4 — get next interrupt
public class MeinextEntry : CsrEntry
{
    private readonly MeipaEntry      m_Meipa;
    private readonly MeieaEntry      m_Meiea;
    private readonly MeipraEntry     m_Meipra;
    private readonly MeifaEntry      m_Meifa;
    private readonly MeicontextEntry m_Meicontext;

    public MeinextEntry(MeipaEntry meipa, MeieaEntry meiea, MeipraEntry meipra,
                        MeifaEntry meifa, MeicontextEntry meicontext)
        : base(0xBE4, "meinext", "Custom IRQ")
    {
        m_Meipa      = meipa;
        m_Meiea      = meiea;
        m_Meipra     = meipra;
        m_Meifa      = meifa;
        m_Meicontext = meicontext;
    }

    // Finds the highest-priority IRQ that is both pending and enabled, with
    // priority >= ppreempt (so a preempting frame does not re-service the preemptee's IRQs).
    // Ties are broken by lowest IRQ number.
    private (bool noIrq, int irq) FindNext()
    {
        int  best         = -1;
        byte bestPriority = 0;
        byte ppreempt     = m_Meicontext.Ppreempt;

        for (int i = 0; i < 512; i++)
        {
            if (!m_Meipa.IsPending(i) || !m_Meiea.IsEnabled(i)) continue;
            byte p = m_Meipra.GetPriority(i);
            if (p < ppreempt) continue;
            if (best == -1 || p > bestPriority || (p == bestPriority && i < best))
                (best, bestPriority) = (i, p);
        }

        return best == -1 ? (true, 0) : (false, best);
    }

    protected override uint ReadCore()
    {
        var (noIrq, irq) = FindNext();
        // meifa force bit is cleared whenever meinext is read and that IRQ is returned.
        if (!noIrq && m_Meifa.IsForced(irq))
            m_Meifa.ClearForced(irq);
        return noIrq ? 0x8000_0000u : (uint)(irq << 2) & 0x7FCu;
        // bit 0 (update) is write-only self-clearing, always reads 0
    }

    public override uint Peek()
    {
        // Same like Read() but without the side-effect (clear forced flag)
        var (noIrq, irq) = FindNext();
        return noIrq ? 0x8000_0000u : (uint)(irq << 2) & 0x7FCu;
        // bit 0 (update) is write-only self-clearing, always reads 0
    }

    protected override void WriteCore(uint value)
    {
        if ((value & 1u) == 0) return; // only the update bit has effect
        var (noIrq, irq) = FindNext();
        if (!noIrq && m_Meifa.IsForced(irq))
            m_Meifa.ClearForced(irq);
        m_Meicontext.ApplyUpdate(noIrq, irq, m_Meipra);
    }
}
