using System;
using System.Collections.Generic;
using Venture.Processor;

namespace VentureUI;

/// <summary>
/// Static metadata describing every Hazard3 CSR — address, name, chapter group, and bit fields.
/// No runtime state lives here; this is pure data used to build the CSR viewer.
/// </summary>
public static class CsrDefinitions
{
    public record FieldDef(
        string Name,
        int Lsb,
        int Width,
        Func<uint, string>? Interpret = null)
    {
        public int Msb => Lsb + Width - 1;
        public string Bits => Width == 1 ? $"[{Lsb}]" : $"[{Msb}:{Lsb}]";
    }

    public record CsrDef(
        ushort Address,
        string Name,
        IReadOnlyList<FieldDef> Fields,
        bool IsWindowed = false);

    public record GroupDef(string Name, IReadOnlyList<CsrDef> Registers);

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static string EnabledDisabled(uint v) => v != 0 ? "Enabled" : "Disabled";
    private static string YesNo(uint v) => v != 0 ? "Yes" : "No";

    private static string MppDecode(uint v) => v switch
    {
        0 => "User",
        3 => "Machine",
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

    private static string MtvecMode(uint v) => v switch
    {
        0 => "Direct",
        1 => "Vectored",
        _ => $"Reserved ({v})"
    };

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

    // ------------------------------------------------------------------
    // PMP field helpers — pmpcfg registers pack 4 × 8-bit region configs
    // ------------------------------------------------------------------

    private static IReadOnlyList<FieldDef> PmpcfgFields(int regIndex)
    {
        var fields = new List<FieldDef>();
        for (int r = 0; r < 4; r++)
        {
            int regionIndex = regIndex * 4 + r;
            int lsb = r * 8;
            string prefix = $"R{regionIndex}";
            fields.Add(new FieldDef($"{prefix}.L",   lsb + 7, 1, YesNo));
            fields.Add(new FieldDef($"{prefix}.A",   lsb + 3, 2, PmpA));
            fields.Add(new FieldDef($"{prefix}.X",   lsb + 2, 1, YesNo));
            fields.Add(new FieldDef($"{prefix}.W",   lsb + 1, 1, YesNo));
            fields.Add(new FieldDef($"{prefix}.R",   lsb + 0, 1, YesNo));
        }
        return fields;
    }

    // ------------------------------------------------------------------
    // Group definitions
    // ------------------------------------------------------------------

    public static IReadOnlyList<GroupDef> Groups { get; } = new List<GroupDef>
    {
        // ── 3.1 Identification ──────────────────────────────────────────
        new("Identification", new List<CsrDef>
        {
            new(0xF11, "mvendorid", new List<FieldDef>
            {
                new("bank",   7, 25),
                new("offset", 0, 7),
            }),
            new(0xF12, "marchid", new List<FieldDef>
            {
                new("open_source", 31, 1, YesNo),
                new("arch_id",      0, 31),
            }),
            new(0xF13, "mimpid",    new List<FieldDef>()),
            new(0xF14, "mhartid",   new List<FieldDef>()),
            new(0xF15, "mconfigptr",new List<FieldDef>()),
            new(0x301, "misa", new List<FieldDef>
            {
                new("mxl",        30, 2),
                new("extensions",  0, 26, MisaExtensions),
            }),
        }),

        // ── 3.2 Trap Handling ───────────────────────────────────────────
        new("Trap Handling", new List<CsrDef>
        {
            new(CSR.MSTATUS, "mstatus", new List<FieldDef>
            {
                new("tw",   21, 1, EnabledDisabled),
                new("mprv", 17, 1, EnabledDisabled),
                new("mpp",  11, 2, MppDecode),
                new("mpie",  7, 1, EnabledDisabled),
                new("mie",   3, 1, EnabledDisabled),
            }),
            new(0x310, "mstatush", new List<FieldDef>()),
            new(CSR.MIE, "mie", new List<FieldDef>
            {
                new("meie", 11, 1, EnabledDisabled),
                new("mtie",  7, 1, EnabledDisabled),
                new("msie",  3, 1, EnabledDisabled),
            }),
            new(CSR.MIP, "mip", new List<FieldDef>
            {
                new("meip", 11, 1, YesNo),
                new("mtip",  7, 1, YesNo),
                new("msip",  3, 1, YesNo),
            }),
            new(CSR.MTVEC, "mtvec", new List<FieldDef>
            {
                new("base", 2, 30),
                new("mode", 0,  1, MtvecMode),
            }),
            new(CSR.MSCRATCH, "mscratch", new List<FieldDef>()),
            new(CSR.MEPC, "mepc", new List<FieldDef>()),
            new(CSR.MCAUSE, "mcause", new List<FieldDef>
            {
                new("interrupt", 31, 1, YesNo),
                new("code",       0, 31, v =>
                {
                    // Interpret raw mcause to give a combined description
                    return string.Empty; // shown via full-register interpretation below
                }),
            }),
            new(CSR.MTVAL, "mtval", new List<FieldDef>()),
            new(0x306, "mcounteren", new List<FieldDef>
            {
                new("ir", 2, 1, EnabledDisabled),
                new("tm", 1, 1, EnabledDisabled),
                new("cy", 0, 1, EnabledDisabled),
            }),
        }),

        // ── 3.3 Memory Protection ───────────────────────────────────────
        new("Memory Protection", new List<CsrDef>
        {
            new(0x3A0, "pmpcfg0", PmpcfgFields(0)),
            new(0x3A1, "pmpcfg1", PmpcfgFields(1)),
            new(0x3A2, "pmpcfg2", PmpcfgFields(2)),
            new(0x3A3, "pmpcfg3", PmpcfgFields(3)),
            new(0x3B0, "pmpaddr0",  new List<FieldDef>()),
            new(0x3B1, "pmpaddr1",  new List<FieldDef>()),
            new(0x3B2, "pmpaddr2",  new List<FieldDef>()),
            new(0x3B3, "pmpaddr3",  new List<FieldDef>()),
            new(0x3B4, "pmpaddr4",  new List<FieldDef>()),
            new(0x3B5, "pmpaddr5",  new List<FieldDef>()),
            new(0x3B6, "pmpaddr6",  new List<FieldDef>()),
            new(0x3B7, "pmpaddr7",  new List<FieldDef>()),
            new(0x3B8, "pmpaddr8",  new List<FieldDef>()),
            new(0x3B9, "pmpaddr9",  new List<FieldDef>()),
            new(0x3BA, "pmpaddr10", new List<FieldDef>()),
            new(0x3BB, "pmpaddr11", new List<FieldDef>()),
            new(0x3BC, "pmpaddr12", new List<FieldDef>()),
            new(0x3BD, "pmpaddr13", new List<FieldDef>()),
            new(0x3BE, "pmpaddr14", new List<FieldDef>()),
            new(0x3BF, "pmpaddr15", new List<FieldDef>()),
        }),

        // ── 3.5 Triggers ────────────────────────────────────────────────
        new("Triggers", new List<CsrDef>
        {
            new(0x7A0, "tselect", new List<FieldDef>()),
            new(0x7A1, "tdata1",  new List<FieldDef>()),
            new(0x7A2, "tdata2",  new List<FieldDef>()),
            new(0x7A3, "tdata3",  new List<FieldDef>()),
        }),

        // ── 3.6 Debug Mode ──────────────────────────────────────────────
        new("Debug Mode", new List<CsrDef>
        {
            new(0x7B0, "dcsr", new List<FieldDef>
            {
                new("xdebugver", 28, 4),
                new("ebreakm",   15, 1, EnabledDisabled),
                new("ebreaku",   12, 1, EnabledDisabled),
                new("stepie",    11, 1, EnabledDisabled),
                new("stopcount", 10, 1, YesNo),
                new("stoptime",   9, 1, YesNo),
                new("cause",      6, 3, DcsrCause),
                new("step",       2, 1, EnabledDisabled),
                new("prv",        0, 2, MppDecode),
            }),
            new(0x7B1, "dpc",       new List<FieldDef>()),
            new(0x7B2, "dscratch0", new List<FieldDef>()),
            new(0x7B3, "dscratch1", new List<FieldDef>()),
        }),

        // ── 3.7 Custom Debug ────────────────────────────────────────────
        new("Custom Debug", new List<CsrDef>
        {
            new(0xBFF, "dmdata0", new List<FieldDef>()),
        }),

        // ── 3.8 Custom IRQ (Xh3irq) ─────────────────────────────────────
        new("Custom IRQ", new List<CsrDef>
        {
            // Windowed registers — show current window + index; "Fetch All" button iterates
            new(0xBE0, "meiea", new List<FieldDef>
            {
                new("window", 16, 16),
                new("index",   0,  5),
            }, IsWindowed: true),
            new(0xBE1, "meipa", new List<FieldDef>
            {
                new("window", 16, 16),
                new("index",   0,  5),
            }, IsWindowed: true),
            new(0xBE2, "meifa", new List<FieldDef>
            {
                new("window", 16, 16),
                new("index",   0,  5),
            }, IsWindowed: true),
            new(0xBE3, "meipra", new List<FieldDef>
            {
                new("window", 16, 16),
                new("index",   0,  7),
            }, IsWindowed: true),
            new(0xBE4, "meinext", new List<FieldDef>
            {
                new("noirq", 31, 1, YesNo),
                new("irq",    2, 9),
                new("update", 0, 1, YesNo),
            }),
            new(CSR.MEICONTEXT, "meicontext", new List<FieldDef>
            {
                new("pppreempt", 28, 4),
                new("ppreempt",  24, 4),
                new("preempt",   16, 5),
                new("noirq",     15, 1, YesNo),
                new("irq",        4, 9),
                new("mtiesave",   3, 1, YesNo),
                new("msiesave",   2, 1, YesNo),
                new("clearts",    1, 1),
                new("mreteirq",   0, 1, EnabledDisabled),
            }),
        }),

        // ── 3.9 Custom PMP ──────────────────────────────────────────────
        new("Custom PMP", new List<CsrDef>
        {
            new(0xBD0, "pmpcfgm0", new List<FieldDef>
            {
                new("m", 0, 16),
            }),
        }),

        // ── 3.10 Power Control ──────────────────────────────────────────
        new("Power Control", new List<CsrDef>
        {
            new(0xBF0, "msleep", new List<FieldDef>
            {
                new("sleeponblock", 2, 1, EnabledDisabled),
                new("powerdown",    1, 1, EnabledDisabled),
                new("deepsleep",    0, 1, EnabledDisabled),
            }),
        }),

        // ── Performance counters ─────────────────────────────────────────
        new("Performance", new List<CsrDef>
        {
            new(CSR.MCYCLE,    "mcycle",    new List<FieldDef>()),
            new(CSR.MCYCLEH,   "mcycleh",   new List<FieldDef>()),
            new(CSR.MINSTRET,  "minstret",  new List<FieldDef>()),
            new(CSR.MINSTRETH, "minstreth", new List<FieldDef>()),
        }),
    };
}
