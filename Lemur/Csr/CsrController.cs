using System.Diagnostics.CodeAnalysis;
using Lemur.Csr.MemoryProtection;
using Lemur.Csr.PerformanceCounters;
using Lemur.Csr.TrapHandling;
using Lemur.Csr.Identification;
using Lemur.Csr.Debug;
using Lemur.Csr.CustomInterrupts;
using Lemur.Csr.Triggers;
using Lemur.Csr.CustomMemoryProtection;
using Lemur.Csr.CustomPower;
using System.Collections.Generic;

namespace Lemur.Csr;

// ═════════════════════════════════════════════════════════════════════════════
// CSR container
// ═════════════════════════════════════════════════════════════════════════════

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public class CsrController
{
    // 3.1 — Standard M-mode Identification
    public MvendoridEntry        Mvendorid  { get; }
    public MarchidEntry          Marchid    { get; }
    public MimpidEntry           Mimpid     { get; }
    public MhartidEntry          Mhartid    { get; }
    public MconfigptrEntry       Mconfigptr { get; }
    public MisaEntry             Misa       { get; }

    // 3.2 — Standard M-mode Trap Handling
    public MstatusEntry          Mstatus    { get; }
    public MstatushEntry         Mstatush   { get; }
    public UnimplementedCsrEntry Medeleg    { get; }
    public UnimplementedCsrEntry Mideleg    { get; }
    public MieEntry              Mie        { get; }
    public MipEntry              Mip        { get; }
    public MtvecEntry            Mtvec      { get; }
    public MscratchEntry         Mscratch   { get; }
    public MepcEntry             Mepc       { get; }
    public McauseEntry           Mcause     { get; }
    public CsrEntry              Mtval      { get; }
    public McounterenEntry       Mcounteren { get; }

    // 3.3 — Standard Memory Protection (arrays registered manually in constructor)
    public PmpcfgEntry[]         Pmpcfg     { get; } = new PmpcfgEntry[4];
    public PmpaddrEntry[]        Pmpaddr    { get; } = new PmpaddrEntry[16];

    // 3.4 — Standard M-mode Performance Counters
    public McycleEntry           Mcycle        { get; }
    public McyclehEntry          Mcycleh       { get; }
    public MinstretEntry         Minstret      { get; }
    public MinstrethEntry        Minstreth     { get; }
    public McountinhibitEntry    Mcountinhibit { get; }

    // 3.5 — Standard Trigger CSRs
    public TselectEntry          Tselect    { get; }

    // 3.6 — Standard Debug Mode CSRs
    public DcsrEntry             Dcsr       { get; }
    public DpcEntry              Dpc        { get; }

    // 3.8 — Custom Interrupt Handling (Xh3irq)
    public MeieaEntry            Meiea      { get; }
    public MeifaEntry            Meifa      { get; }
    public MeipraEntry           Meipra     { get; }
    public MeipaEntry            Meipa      { get; }
    public MeicontextEntry       Meicontext { get; }
    public MeinextEntry          Meinext    { get; }

    // 3.9 — Custom Memory Protection
    public Pmpcfgm0Entry         Pmpcfgm0   { get; }

    // 3.10 — Custom Power Control
    public MsleepEntry           Msleep     { get; }

    private readonly Dictionary<ushort, CsrEntry> m_Entries = new();

    public IEnumerable<CsrEntry> AllEntries => m_Entries.Values;

    public CsrController(RiscVConfig riscVConfig)
    {
        // 3.1 — Identification (all read-only constants)
        Mvendorid  = new();
        Marchid    = new();
        Mimpid     = new();
        Mhartid    = new();
        Mconfigptr = new();
        Misa       = new(hasMExt: true, hasCExt: true, hasAExt: true, hasCustom: true);

        // 3.2 — Trap Handling
        Mstatus    = new();
        Mstatush   = new();
        Medeleg    = new(0x302, "medeleg");
        Mideleg    = new(0x303, "mideleg");
        Mie        = new();
        Mtvec      = new();
        Mscratch   = new();
        Mepc       = new();
        Mcause     = new();
        Mtval      = riscVConfig.MtvalHardwiredToZero ? new MtvalZeroEntry() : new MtvalEntry();
        Mcounteren = new();

        // 3.3 — Memory Protection (arrays registered separately below)
        for (int i = 0; i < 4; i++)
            Pmpcfg[i] = new((ushort)(0x3a0 + i), i, isTorEnabled: () => riscVConfig.TorEnabled);
        for (int i = 0; i < 16; i++)
        {
            int region = i;
            // pmpaddr[i] is read-only while the L bit of its pmpcfg byte is set.
            Pmpaddr[i] = new((ushort)(0x3b0 + i), i, isLocked: () =>
                ((Pmpcfg[region / 4].Read() >> ((region % 4) * 8)) & 0x80u) != 0);
        }

        // 3.4 — Performance Counters
        Mcycle        = new();
        Mcycleh       = new(Mcycle);
        Minstret      = new();
        Minstreth     = new(Minstret);
        Mcountinhibit = new();

        // 3.5 — Triggers
        Tselect = new();

        // 3.6 — Debug
        Dcsr = new();
        Dpc  = new();

        // 3.8 — Custom Interrupt Handling
        Meiea      = new();
        Meifa      = new();
        Meipra     = new();
        Meipa      = new(Meifa);
        Meicontext = new(Mie);
        Meinext    = new(Meipa, Meiea, Meipra, Meifa, Meicontext);
        Mip        = new(Meicontext, Meipa, Meiea, Meipra);

        // 3.9 — Custom Memory Protection
        Pmpcfgm0 = new();

        // 3.10 — Custom Power Control
        Msleep = new();

        // Register all single CsrEntry properties via reflection
        foreach (var prop in GetType().GetProperties())
            if (prop.GetValue(this) is CsrEntry entry)
                m_Entries[entry.Address] = entry;

        // Register PMP array entries (arrays are not caught by the reflection loop above)
        foreach (var e in Pmpcfg)  m_Entries[e.Address] = e;
        foreach (var e in Pmpaddr) m_Entries[e.Address] = e;

        // mhpmcounter3…31 (0xb03–0xb1f) and upper halves (0xb83–0xb9f) — hardwired 0
        for (int i = 3; i <= 31; i++)
        {
            m_Entries[(ushort)(0xb00 + i)] = new HardwiredZeroEntry((ushort)(0xb00 + i), $"mhpmcounter{i}",  "Performance");
            m_Entries[(ushort)(0xb80 + i)] = new HardwiredZeroEntry((ushort)(0xb80 + i), $"mhpmcounter{i}h", "Performance");
        }

        // mhpmevent3…31 (0x323–0x33f) — hardwired 0
        for (int i = 3; i <= 31; i++)
            m_Entries[(ushort)(0x320 + i)] = new HardwiredZeroEntry((ushort)(0x320 + i), $"mhpmevent{i}", "Performance");

        // tdata1…3 (0x7a1–0x7a3) — illegal instruction
        m_Entries[0x7a1] = new UnimplementedCsrEntry(0x7a1, "tdata1", "Trigger");
        m_Entries[0x7a2] = new UnimplementedCsrEntry(0x7a2, "tdata2", "Trigger");
        m_Entries[0x7a3] = new UnimplementedCsrEntry(0x7a3, "tdata3", "Trigger");

        // dscratch0/1 (0x7b2–0x7b3) — not implemented, illegal instruction
        m_Entries[0x7b2] = new UnimplementedCsrEntry(0x7b2, "dscratch0", "Debug");
        m_Entries[0x7b3] = new UnimplementedCsrEntry(0x7b3, "dscratch1", "Debug");

        // dmdata0 (0xbff) — debug module data register, illegal instruction outside debug mode
        m_Entries[0xbff] = new UnimplementedCsrEntry(0xbff, "dmdata0", "Debug");
    }

    public uint Get(ushort address)         => m_Entries[address].Read();
    public void Set(ushort address, uint v) => m_Entries[address].Write(v);
}
