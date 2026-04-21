using Lemur.Csr.CustomInterrupts;

namespace Lemur.Csr.TrapHandling;

// mip — 0x344 — read-only; meip is computed, mtip/msip are driven by hardware
public class MipEntry : CsrEntry
{
    private readonly MeicontextEntry m_Meicontext;
    private readonly MeipaEntry m_Meipa;
    private readonly MeieaEntry m_Meiea;
    private readonly MeipraEntry m_Meipra;

    // mip.meip: asserted when any IRQ is pending, enabled, and has priority >= preempt.
    [EntryValue("11", "external interrupt pending (computed)")]
    public bool Meip
    {
        get
        {
            byte preempt = m_Meicontext.Preempt;
            for (int i = 0; i < 512; i++)
                if (m_Meipa.IsPending(i) && m_Meiea.IsEnabled(i) && m_Meipra.GetPriority(i) >= preempt)
                    return true;
            return false;
        }
    }

    private bool m_Mtip;
    private bool m_Msip;

    [EntryValue("7",  "timer interrupt pending")]
    public bool Mtip { get => m_Mtip; set => Set(ref m_Mtip, value); }

    [EntryValue("3",  "software interrupt pending")]
    public bool Msip { get => m_Msip; set => Set(ref m_Msip, value); }

    public MipEntry(MeicontextEntry meicontext, MeipaEntry meipa, MeieaEntry meiea, MeipraEntry meipra) : base(0x344, "mip", "Trap")
    {
        m_Meicontext = meicontext;
        m_Meipa = meipa;
        m_Meiea = meiea;
        m_Meipra = meipra;
    }

    protected override uint ReadCore()
    {
        uint v = 0;
        if (Meip) v |= 1u << 11;
        if (Mtip) v |= 1u <<  7;
        if (Msip) v |= 1u <<  3;
        return v;
    }

    protected override void WriteCore(uint value) { } // writable bits exist only for S/U modes
}
