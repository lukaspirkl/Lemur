namespace Venture.Processor;

/// <summary>
/// One named group of CSRs as shown in the CSR viewer (e.g. "Trap Handling").
/// </summary>
public record CsrGroupDef(string Name, IReadOnlyList<CsrEntry> Registers);

/// <summary>
/// Machine-level Control and Status Registers for Hazard3 / RP2350.
///
/// Implements <see cref="ICsrAccess"/> — the narrow interface used by the processor
/// and interrupt controller for ordinary Get/Set operations.
///
/// Also exposes:
///   <see cref="ForceRead"/> / <see cref="ForceWrite"/> for hardware-internal trap/MRET
///   operations that must bypass write masks and getter/setter hooks.
///
///   <see cref="GetEntry"/> so IrqController can retrieve windowed entries and wire up
///   live-value delegates (MEINEXT, MEICONTEXT) and computed readers (MEIPA).
///
///   <see cref="GetGroups"/> so the UI CSR viewer gets grouped, annotated metadata
///   without any separate CsrDefinitions file.
///
/// Spec: RISC-V Privileged ISA Vol. II — Chapter 2/3; RP2350 Datasheet §3.8.
/// </summary>
public class CSR : ICsrAccess
{
    // ── Address constants ──────────────────────────────────────────────────────

    /// <summary>0x300 — Machine status register.</summary>
    public const ushort MSTATUS    = 0x300;
    /// <summary>0x304 — Machine interrupt-enable register.</summary>
    public const ushort MIE        = 0x304;
    /// <summary>0x305 — Machine trap-handler base address.</summary>
    public const ushort MTVEC      = 0x305;
    /// <summary>0x340 — Scratch register.</summary>
    public const ushort MSCRATCH   = 0x340;
    /// <summary>0x341 — Machine exception program counter.</summary>
    public const ushort MEPC       = 0x341;
    /// <summary>0x342 — Machine cause register. Bit 31 = interrupt; bits 30:0 = code.</summary>
    public const ushort MCAUSE     = 0x342;
    /// <summary>0x343 — Machine bad address / instruction. Hardwired to 0 on Hazard3.</summary>
    public const ushort MTVAL      = 0x343;
    /// <summary>0x344 — Machine interrupt-pending register.</summary>
    public const ushort MIP        = 0x344;
    /// <summary>0xB00 — Lower 32 bits of the cycle counter.</summary>
    public const ushort MCYCLE     = 0xB00;
    /// <summary>0xB02 — Lower 32 bits of the instructions-retired counter.</summary>
    public const ushort MINSTRET   = 0xB02;
    /// <summary>0xB80 — Upper 32 bits of MCYCLE.</summary>
    public const ushort MCYCLEH    = 0xB80;
    /// <summary>0xB82 — Upper 32 bits of MINSTRET.</summary>
    public const ushort MINSTRETH  = 0xB82;
    /// <summary>0xBE5 — Xh3irq external interrupt context register.</summary>
    public const ushort MEICONTEXT = 0xBE5;

    // ── MSTATUS bit positions ──────────────────────────────────────────────────

    /// <summary>Bit 3 of MSTATUS — Global machine-level interrupt enable.</summary>
    public const int MSTATUS_MIE_BIT  = 3;
    /// <summary>Bit 7 of MSTATUS — Previous MIE value (saved/restored on trap/MRET).</summary>
    public const int MSTATUS_MPIE_BIT = 7;

    // ── MIP / MIE bit positions ────────────────────────────────────────────────

    /// <summary>Bit 3 — Machine software interrupt pending/enable.</summary>
    public const int MxP_MSIP_BIT = 3;
    /// <summary>Bit 7 — Machine timer interrupt pending/enable.</summary>
    public const int MxP_MTIP_BIT = 7;
    /// <summary>Bit 11 — Machine external interrupt pending/enable.</summary>
    public const int MxP_MEIP_BIT = 11;

    // ── MTVEC mode encoding ────────────────────────────────────────────────────

    public const uint MTVEC_MODE_DIRECT   = 0;
    public const uint MTVEC_MODE_VECTORED = 1;

    // ── Event ─────────────────────────────────────────────────────────────────

    /// <summary>Fired whenever a CSR value changes (software or hardware write).</summary>
    public event Action<ushort, uint>? Changed;

    // ── State ─────────────────────────────────────────────────────────────────

    private readonly Dictionary<ushort, CsrEntry> m_Entries   = new();
    private readonly List<string>                 m_GroupOrder = new();
    private readonly Dictionary<string, List<CsrEntry>> m_Groups = new();
    private readonly IEmuLogger<CSR>              m_Logger;

    public CSR(IEmuLogger<CSR> logger)
    {
        m_Logger = logger;
        SetupRegisters();
    }

    // ── ICsrAccess ─────────────────────────────────────────────────────────────

    public uint Get(ushort address)
    {
        uint value = m_Entries.TryGetValue(address, out var e) ? e.Read() : 0u;
        m_Logger.LogCSRGet(address, value);
        return value;
    }

    public void Set(ushort address, uint value)
    {
        m_Logger.LogCSRSet(address, value);
        if (m_Entries.TryGetValue(address, out var e))
            e.Write(value);
    }

    // ── Hardware-internal access ───────────────────────────────────────────────

    /// <summary>Bypass getter; reads backing store. Used by trap / MRET machinery.</summary>
    internal uint ForceRead(ushort address) =>
        m_Entries.TryGetValue(address, out var e) ? e.ForceRead() : 0u;

    /// <summary>Bypass write mask and setter; writes backing store. Used by trap / MRET machinery.</summary>
    internal void ForceWrite(ushort address, uint value)
    {
        if (m_Entries.TryGetValue(address, out var e))
            e.ForceWrite(value);
    }

    /// <summary>Alias kept for compatibility with existing trap/MRET code.</summary>
    internal uint RawGet(ushort address)  => ForceRead(address);
    /// <summary>Alias kept for compatibility with existing trap/MRET code.</summary>
    internal void RawSet(ushort address, uint value) => ForceWrite(address, value);

    // ── Entry access ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the registered entry for <paramref name="address"/>, or null.
    /// Used by IrqController to retrieve windowed entries and wire live-value delegates.
    /// </summary>
    public CsrEntry? GetEntry(ushort address) =>
        m_Entries.TryGetValue(address, out var e) ? e : null;

    // ── UI metadata ────────────────────────────────────────────────────────────

    public IReadOnlyList<CsrGroupDef> GetGroups() =>
        m_GroupOrder.Select(n => new CsrGroupDef(n, m_Groups[n])).ToList();

    // ── Registration helpers ───────────────────────────────────────────────────

    private CsrEntry Register(ushort address, string name, string group)
    {
        var entry = new CsrEntry(address, name, group);
        Wire(entry);
        return entry;
    }

    private CsrWindowedEntry RegisterWindowed(ushort address, string name, string group,
        int windowCount, int indexBits, bool writeEnabled = true)
    {
        var entry = new CsrWindowedEntry(address, name, group, windowCount, indexBits, writeEnabled);
        Wire(entry);
        return entry;
    }

    private void Wire(CsrEntry entry)
    {
        entry.OnChanged = (addr, val) => Changed?.Invoke(addr, val);
        m_Entries[entry.Address] = entry;
        if (!m_Groups.ContainsKey(entry.Group))
        {
            m_GroupOrder.Add(entry.Group);
            m_Groups[entry.Group] = new List<CsrEntry>();
        }
        m_Groups[entry.Group].Add(entry);
    }

    // ── Interpretation helpers (private — used only in SetupRegisters) ─────────

    private static string EnabledDisabled(uint v) => v != 0 ? "Enabled" : "Disabled";
    private static string YesNo(uint v)            => v != 0 ? "Yes" : "No";

    private static string MppDecode(uint v) => v switch
    {
        0 => "User",
        3 => "Machine",
        _ => $"Reserved ({v})"
    };

    private static string MtvecMode(uint v) => v switch
    {
        0 => "Direct",
        1 => "Vectored",
        _ => $"Reserved ({v})"
    };

    private static string McauseDecode(uint raw)
    {
        bool isInterrupt = (raw & 0x8000_0000u) != 0;
        uint code = raw & 0x7FFF_FFFFu;
        if (isInterrupt)
            return code switch
            {
                3  => "Machine software interrupt",
                7  => "Machine timer interrupt",
                11 => "Machine external interrupt",
                _  => $"Interrupt {code}"
            };
        return code switch
        {
            0  => "Instruction address misaligned",
            1  => "Instruction access fault",
            2  => "Illegal instruction",
            3  => "Breakpoint",
            4  => "Load address misaligned",
            5  => "Load access fault",
            6  => "Store/AMO address misaligned",
            7  => "Store/AMO access fault",
            11 => "Environment call",
            _  => $"Exception {code}"
        };
    }

    private static string PmpA(uint v) => v switch
    {
        0 => "OFF",
        1 => "TOR",
        2 => "NA4",
        3 => "NAPOT",
        _ => $"{v}"
    };

    private static string DcsrCause(uint v) => v switch
    {
        1 => "ebreak",
        3 => "halt/reset-halt",
        4 => "single-step",
        _ => $"{v}"
    };

    private static string MisaExtensions(uint v)
    {
        var parts = new List<string>();
        if ((v & (1 << 0))  != 0) parts.Add("A (atomics)");
        if ((v & (1 << 2))  != 0) parts.Add("C (compressed)");
        if ((v & (1 << 12)) != 0) parts.Add("M (mul/div)");
        if ((v & (1 << 20)) != 0) parts.Add("U (user mode)");
        if ((v & (1 << 23)) != 0) parts.Add("X (custom)");
        return parts.Count > 0 ? string.Join(", ", parts) : "None";
    }

    // pmpcfg registers pack 4 × 8-bit region configs
    private static void AddPmpcfgFields(CsrEntry e, int regIndex)
    {
        for (int r = 0; r < 4; r++)
        {
            int    regionIndex = regIndex * 4 + r;
            int    lsb         = r * 8;
            string prefix      = $"R{regionIndex}";
            e.Field($"{prefix}.L", lsb + 7, 1, YesNo);
            e.Field($"{prefix}.A", lsb + 3, 2, PmpA);
            e.Field($"{prefix}.X", lsb + 2, 1, YesNo);
            e.Field($"{prefix}.W", lsb + 1, 1, YesNo);
            e.Field($"{prefix}.R", lsb + 0, 1, YesNo);
        }
    }

    // ── CSR registrations ──────────────────────────────────────────────────────

    private void SetupRegisters()
    {
        const string Identification = "Identification";
        const string TrapHandling   = "Trap Handling";
        const string MemProtection  = "Memory Protection";
        const string Triggers       = "Triggers";
        const string DebugMode      = "Debug Mode";
        const string CustomDebug    = "Custom Debug";
        const string CustomIrq      = "Custom IRQ";
        const string CustomPmp      = "Custom PMP";
        const string PowerControl   = "Power Control";
        const string Performance    = "Performance";

        // ── 3.1 Identification ────────────────────────────────────────────────
        Register(0xF11, "mvendorid",  Identification).ReadOnly()
            .Field("bank",   7, 25)
            .Field("offset", 0,  7);
        Register(0xF12, "marchid",    Identification).ReadOnly()
            .Field("open_source", 31, 1, YesNo)
            .Field("arch_id",      0, 31);
        Register(0xF13, "mimpid",     Identification).ReadOnly();
        Register(0xF14, "mhartid",    Identification).ReadOnly();
        Register(0xF15, "mconfigptr", Identification).ReadOnly();
        Register(0x301, "misa",       Identification).ReadOnly()
            .Field("mxl",        30,  2)
            .Field("extensions",  0, 26, MisaExtensions);

        // ── 3.2 Trap Handling ─────────────────────────────────────────────────
        Register(MSTATUS, "mstatus", TrapHandling)
            .Field("tw",   21, 1, EnabledDisabled)
            .Field("mprv", 17, 1, EnabledDisabled)
            .Field("mpp",  11, 2, MppDecode)
            .Field("mpie",  7, 1, EnabledDisabled)
            .Field("mie",   3, 1, EnabledDisabled);
        Register(0x310, "mstatush", TrapHandling);
        Register(MIE, "mie", TrapHandling)
            .Field("meie", 11, 1, EnabledDisabled)
            .Field("mtie",  7, 1, EnabledDisabled)
            .Field("msie",  3, 1, EnabledDisabled);
        Register(MIP, "mip", TrapHandling)
            .Field("meip", 11, 1, YesNo)
            .Field("mtip",  7, 1, YesNo)
            .Field("msip",  3, 1, YesNo);
        Register(MTVEC, "mtvec", TrapHandling)
            .Field("base", 2, 30)
            .Field("mode", 0,  1, MtvecMode);
        Register(MSCRATCH,  "mscratch",  TrapHandling);
        Register(MEPC,      "mepc",      TrapHandling);
        Register(MCAUSE,    "mcause",    TrapHandling)
            .Field("interrupt", 31, 1, YesNo)
            .Field("code",       0, 31, McauseDecode);
        Register(MTVAL,     "mtval",     TrapHandling).ReadOnly(); // hardwired to 0 on Hazard3
        Register(0x306, "mcounteren",   TrapHandling)
            .Field("ir", 2, 1, EnabledDisabled)
            .Field("tm", 1, 1, EnabledDisabled)
            .Field("cy", 0, 1, EnabledDisabled);

        // ── 3.3 Memory Protection ─────────────────────────────────────────────
        AddPmpcfgFields(Register(0x3A0, "pmpcfg0", MemProtection), 0);
        AddPmpcfgFields(Register(0x3A1, "pmpcfg1", MemProtection), 1);
        AddPmpcfgFields(Register(0x3A2, "pmpcfg2", MemProtection), 2);
        AddPmpcfgFields(Register(0x3A3, "pmpcfg3", MemProtection), 3);
        for (int i = 0; i < 16; i++)
            Register((ushort)(0x3B0 + i), $"pmpaddr{i}", MemProtection);

        // ── 3.5 Triggers ──────────────────────────────────────────────────────
        Register(0x7A0, "tselect", Triggers);
        Register(0x7A1, "tdata1",  Triggers);
        Register(0x7A2, "tdata2",  Triggers);
        Register(0x7A3, "tdata3",  Triggers);

        // ── 3.6 Debug Mode ────────────────────────────────────────────────────
        Register(0x7B0, "dcsr", DebugMode)
            .Field("xdebugver", 28, 4)
            .Field("ebreakm",   15, 1, EnabledDisabled)
            .Field("ebreaku",   12, 1, EnabledDisabled)
            .Field("stepie",    11, 1, EnabledDisabled)
            .Field("stopcount", 10, 1, YesNo)
            .Field("stoptime",   9, 1, YesNo)
            .Field("cause",      6, 3, DcsrCause)
            .Field("step",       2, 1, EnabledDisabled)
            .Field("prv",        0, 2, MppDecode);
        Register(0x7B1, "dpc",       DebugMode);
        Register(0x7B2, "dscratch0", DebugMode);
        Register(0x7B3, "dscratch1", DebugMode);

        // ── 3.7 Custom Debug ──────────────────────────────────────────────────
        Register(0xBFF, "dmdata0", CustomDebug);

        // ── 3.8 Custom IRQ (Xh3irq) ───────────────────────────────────────────
        // Windowed registers: each window is 16 bits exposed in bits [31:16];
        // bits [indexBits-1:0] select the window. IrqController wires up the
        // live-value delegates for MEINEXT and MEICONTEXT, and the computed
        // reader for MEIPA, after retrieving entries via GetEntry().
        RegisterWindowed(0xBE0, "meiea",  CustomIrq, windowCount: 32,  indexBits: 5)
            .Field("window", 16, 16)
            .Field("index",   0,  5);
        RegisterWindowed(0xBE1, "meipa",  CustomIrq, windowCount: 32,  indexBits: 5, writeEnabled: false)
            .Field("window", 16, 16)
            .Field("index",   0,  5);
        RegisterWindowed(0xBE2, "meifa",  CustomIrq, windowCount: 32,  indexBits: 5)
            .Field("window", 16, 16)
            .Field("index",   0,  5);
        RegisterWindowed(0xBE3, "meipra", CustomIrq, windowCount: 128, indexBits: 7)
            .Field("window", 16, 16)
            .Field("index",   0,  7);
        // MEINEXT and MEICONTEXT have complex read/write logic; IrqController
        // attaches LiveValue delegates in its constructor.
        Register(0xBE4, "meinext", CustomIrq)
            .Field("noirq",  31, 1, YesNo)
            .Field("irq",     2, 9)
            .Field("update",  0, 1, YesNo);
        Register(MEICONTEXT, "meicontext", CustomIrq)
            .Field("pppreempt", 28, 4)
            .Field("ppreempt",  24, 4)
            .Field("preempt",   16, 5)
            .Field("noirq",     15, 1, YesNo)
            .Field("irq",        4, 9)
            .Field("mtiesave",   3, 1, YesNo)
            .Field("msiesave",   2, 1, YesNo)
            .Field("clearts",    1, 1)
            .Field("mreteirq",   0, 1, EnabledDisabled);

        // ── 3.9 Custom PMP ────────────────────────────────────────────────────
        Register(0xBD0, "pmpcfgm0", CustomPmp)
            .Field("m", 0, 16);

        // ── 3.10 Power Control ────────────────────────────────────────────────
        Register(0xBF0, "msleep", PowerControl)
            .Field("sleeponblock", 2, 1, EnabledDisabled)
            .Field("powerdown",    1, 1, EnabledDisabled)
            .Field("deepsleep",    0, 1, EnabledDisabled);

        // ── Performance counters ──────────────────────────────────────────────
        // LiveValue delegates for these are attached by Hazard3Processor.
        Register(MCYCLE,    "mcycle",    Performance);
        Register(MCYCLEH,   "mcycleh",   Performance);
        Register(MINSTRET,  "minstret",  Performance);
        Register(MINSTRETH, "minstreth", Performance);
    }
}
