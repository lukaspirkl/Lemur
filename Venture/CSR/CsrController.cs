using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Venture.Csr.Identification;
using Venture.Csr.TrapHandling;
using Venture.Csr.MemoryProtection;
using Venture.Csr.PerformanceCounters;
using Venture.Csr.Triggers;
using Venture.Csr.Debug;
using Venture.Csr.CustomInterrupts;
using Venture.Csr.CustomMemoryProtection;
using Venture.Csr.CustomPower;

namespace Venture.Csr;

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
    public MtvalEntry            Mtval      { get; }
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

    public CsrController()
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
        Mtval      = new();
        Mcounteren = new();

        // 3.3 — Memory Protection (arrays registered separately below)
        for (int i = 0; i < 4;  i++) Pmpcfg[i]  = new((ushort)(0x3a0 + i), i);
        for (int i = 0; i < 16; i++) Pmpaddr[i] = new((ushort)(0x3b0 + i), i);

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



    /// <summary>IRQ 0 — Timer 0 alarm 0.</summary>
    public const int TIMER0_IRQ_0 = 0;
    /// <summary>IRQ 1 — Timer 0 alarm 1.</summary>
    public const int TIMER0_IRQ_1 = 1;
    /// <summary>IRQ 2 — Timer 0 alarm 2.</summary>
    public const int TIMER0_IRQ_2 = 2;
    /// <summary>IRQ 3 — Timer 0 alarm 3.</summary>
    public const int TIMER0_IRQ_3 = 3;
    /// <summary>IRQ 4 — Timer 1 alarm 0.</summary>
    public const int TIMER1_IRQ_0 = 4;
    /// <summary>IRQ 5 — Timer 1 alarm 1.</summary>
    public const int TIMER1_IRQ_1 = 5;
    /// <summary>IRQ 6 — Timer 1 alarm 2.</summary>
    public const int TIMER1_IRQ_2 = 6;
    /// <summary>IRQ 7 — Timer 1 alarm 3.</summary>
    public const int TIMER1_IRQ_3 = 7;
    /// <summary>IRQ 8 — PWM slice 0 wrap.</summary>
    public const int PWM_IRQ_WRAP_0 = 8;
    /// <summary>IRQ 9 — PWM slice 1 wrap.</summary>
    public const int PWM_IRQ_WRAP_1 = 9;
    /// <summary>IRQ 10 — DMA channel 0.</summary>
    public const int DMA_IRQ_0 = 10;
    /// <summary>IRQ 11 — DMA channel 1.</summary>
    public const int DMA_IRQ_1 = 11;
    /// <summary>IRQ 12 — DMA channel 2.</summary>
    public const int DMA_IRQ_2 = 12;
    /// <summary>IRQ 13 — DMA channel 3.</summary>
    public const int DMA_IRQ_3 = 13;
    /// <summary>IRQ 14 — USB controller.</summary>
    public const int USBCTRL_IRQ = 14;
    /// <summary>IRQ 15 — PIO0 SM 0/1 interrupt 0.</summary>
    public const int PIO0_IRQ_0 = 15;
    /// <summary>IRQ 16 — PIO0 SM 2/3 interrupt 1.</summary>
    public const int PIO0_IRQ_1 = 16;
    /// <summary>IRQ 17 — PIO1 SM 0/1 interrupt 0.</summary>
    public const int PIO1_IRQ_0 = 17;
    /// <summary>IRQ 18 — PIO1 SM 2/3 interrupt 1.</summary>
    public const int PIO1_IRQ_1 = 18;
    /// <summary>IRQ 19 — PIO2 SM 0/1 interrupt 0.</summary>
    public const int PIO2_IRQ_0 = 19;
    /// <summary>IRQ 20 — PIO2 SM 2/3 interrupt 1.</summary>
    public const int PIO2_IRQ_1 = 20;
    /// <summary>IRQ 21 — GPIO Bank 0 (Secure). Core-local.</summary>
    public const int IO_IRQ_BANK0 = 21;
    /// <summary>IRQ 22 — GPIO Bank 0 (Non-secure). Core-local.</summary>
    public const int IO_IRQ_BANK0_NS = 22;
    /// <summary>IRQ 23 — QSPI GPIO (Secure).</summary>
    public const int IO_IRQ_QSPI = 23;
    /// <summary>IRQ 24 — QSPI GPIO (Non-secure).</summary>
    public const int IO_IRQ_QSPI_NS = 24;
    /// <summary>IRQ 25 — SIO inter-processor FIFO (Secure). Core-local.</summary>
    public const int SIO_IRQ_FIFO = 25;
    /// <summary>IRQ 26 — SIO doorbell (Secure). Core-local.</summary>
    public const int SIO_IRQ_BELL = 26;
    /// <summary>IRQ 27 — SIO FIFO (Non-secure). Core-local.</summary>
    public const int SIO_IRQ_FIFO_NS = 27;
    /// <summary>IRQ 28 — SIO doorbell (Non-secure). Core-local.</summary>
    public const int SIO_IRQ_BELL_NS = 28;
    /// <summary>IRQ 29 — RISC-V platform timer compare. Core-local.</summary>
    public const int SIO_IRQ_MTIMECMP = 29;
    /// <summary>IRQ 30 — Clocks subsystem.</summary>
    public const int CLOCKS_IRQ = 30;
    /// <summary>IRQ 31 — SPI0.</summary>
    public const int SPI0_IRQ = 31;
    /// <summary>IRQ 32 — SPI1.</summary>
    public const int SPI1_IRQ = 32;
    /// <summary>IRQ 33 — UART0.</summary>
    public const int UART0_IRQ = 33;
    /// <summary>IRQ 34 — UART1.</summary>
    public const int UART1_IRQ = 34;
    /// <summary>IRQ 35 — ADC FIFO.</summary>
    public const int ADC_IRQ_FIFO = 35;
    /// <summary>IRQ 36 — I2C0.</summary>
    public const int I2C0_IRQ = 36;
    /// <summary>IRQ 37 — I2C1.</summary>
    public const int I2C1_IRQ = 37;
    /// <summary>IRQ 38 — OTP programming done / error.</summary>
    public const int OTP_IRQ = 38;
    /// <summary>IRQ 39 — True Random Number Generator.</summary>
    public const int TRNG_IRQ = 39;
    /// <summary>IRQ 40 — Cortex-M33 cross-trigger (core 0).</summary>
    public const int PROC0_IRQ_CTI = 40;
    /// <summary>IRQ 41 — Cortex-M33 cross-trigger (core 1).</summary>
    public const int PROC1_IRQ_CTI = 41;
    /// <summary>IRQ 42 — System PLL lock / unlock.</summary>
    public const int PLL_SYS_IRQ = 42;
    /// <summary>IRQ 43 — USB PLL lock / unlock.</summary>
    public const int PLL_USB_IRQ = 43;
    /// <summary>IRQ 44 — Power manager power-state change.</summary>
    public const int POWMAN_IRQ_POW = 44;
    /// <summary>IRQ 45 — Power manager always-on timer.</summary>
    public const int POWMAN_IRQ_TIMER = 45;
    // IRQs 46–51: SPAREIRQ_IRQ_0..5 — software-triggered only, not modelled here.
}
