using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Venture.Csr;

// ── Base ──────────────────────────────────────────────────────────────────────

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public abstract class CsrEntry
{
    public ushort Address { get; }
    public string Name    { get; }
    public string Group   { get; }

    protected CsrEntry(ushort address, string name, string group)
        => (Address, Name, Group) = (address, name, group);

    public abstract uint Read();
    public abstract void Write(uint value);
    public virtual uint Peek() => Read();

    public IEnumerable<EntryValue> GetValues()
    {
        var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var list = new List<EntryValue>();

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<EntryValueAttribute>();
            if (attr == null)
                continue;

            var rawValue = prop.GetValue(this);

            uint value;
            try
            {
                value = rawValue != null ? Convert.ToUInt32(rawValue) : 0;
            }
            catch (Exception)
            {
                throw new InvalidOperationException(
                    $"Cannot convert property {prop.Name} of type {prop.PropertyType} to uint");
            }

            list.Add(new EntryValue(
                prop.Name,
                attr.Bits,
                attr.Description,
                value
            ));
        }

        return list.OrderBy(e => e.Bits);
    }
}

public record EntryValue(string Name, string Bits, string Description, uint Value);

[AttributeUsage(AttributeTargets.Property)]
public class EntryValueAttribute : Attribute
{
    public string Bits { get; }
    public string Description { get; }

    public EntryValueAttribute(string bits, string description)
    {
        Bits = bits;
        Description = description;
    }
}

public interface ICsrWindowed
{
    int  WindowCount   { get; }
    int  BitsPerWindow { get; }
    int  BitsPerItem   { get; }  // 1 for meiea/meifa/meipa (one flag per IRQ), 4 for meipra (priority nibble per IRQ)
    uint PeekWindow(int index);
}

// ═════════════════════════════════════════════════════════════════════════════
// 3.1  Standard M-mode Identification CSRs
// ═════════════════════════════════════════════════════════════════════════════

// mvendorid — 0xf11 — read-only configurable constant
public class MvendoridEntry : CsrEntry
{
    private readonly uint m_Value;

    [EntryValue("31:7", "JEDEC JEP106 continuation code count (bank − 1)")]
    public uint Bank   => m_Value >> 7;

    [EntryValue("6:0",  "Vendor ID within bank (parity bit not stored)")]
    public uint Offset => m_Value & 0x7Fu;

    public MvendoridEntry(uint value = 0) : base(0xf11, "mvendorid", "Identification")
        => m_Value = value;

    public override uint Read()            => m_Value;
    public override void Write(uint value) { }
}

// marchid — 0xf12 — hardwired: bit31=0 (open-source), bits30:0=0x1b (Hazard3 ID 27)
public class MarchidEntry : CsrEntry
{
    [EntryValue("30:0", "Hazard3 architecture ID (27 decimal, registered with RISC-V International)")]
    public uint ArchId => 0x1bu;

    public MarchidEntry() : base(0xf12, "marchid", "Identification") { }

    public override uint Read()            => 0x1bu; // bit 31 = 0 (open-source)
    public override void Write(uint value) { }
}

// mimpid — 0xf13 — read-only configurable constant (git hash or 0)
public class MimpidEntry : CsrEntry
{
    private readonly uint m_Value;

    [EntryValue("31:0", "Implementation ID (git hash of synthesised Hazard3 revision, or 0)")]
    public uint ImpId => m_Value;

    public MimpidEntry(uint value = 0) : base(0xf13, "mimpid", "Identification")
        => m_Value = value;

    public override uint Read()            => m_Value;
    public override void Write(uint value) { }
}

// mhartid — 0xf14 — read-only configurable constant (per-core identifier)
public class MhartidEntry : CsrEntry
{
    private readonly uint m_Value;

    [EntryValue("31:0", "Hart identifier (unique per core, assigned consecutively from 0)")]
    public uint HartId => m_Value;

    public MhartidEntry(uint hartId = 0) : base(0xf14, "mhartid", "Identification")
        => m_Value = hartId;

    public override uint Read()            => m_Value;
    public override void Write(uint value) { }
}

// mconfigptr — 0xf15 — read-only, pointer to configuration data structure or 0
public class MconfigptrEntry : CsrEntry
{
    private readonly uint m_Value;

    [EntryValue("31:0", "Pointer to configuration data structure (4-byte aligned), or 0")]
    public uint Ptr => m_Value;

    public MconfigptrEntry(uint value = 0) : base(0xf15, "mconfigptr", "Identification")
        => m_Value = value;

    public override uint Read()            => m_Value;
    public override void Write(uint value) { }
}

// misa — 0x301 — read-only, ISA capability register
public class MisaEntry : CsrEntry
{
    [EntryValue("31:30", "MXL: machine XLEN (always 1 = 32-bit)")]
    public uint Mxl => 1;

    [EntryValue("23", "X: custom extension present")]
    public bool X { get; }

    [EntryValue("20", "U: user mode supported")]
    public bool U { get; }

    [EntryValue("12", "M: integer multiply/divide extension")]
    public bool M { get; }

    [EntryValue("2",  "C: compressed instruction extension")]
    public bool C { get; }

    [EntryValue("0",  "A: atomic instruction extension")]
    public bool A { get; }

    public MisaEntry(bool hasUMode = false, bool hasMExt = false, bool hasCExt = false,
                     bool hasAExt = false, bool hasCustom = false)
        : base(0x301, "misa", "Identification")
    {
        U = hasUMode; M = hasMExt; C = hasCExt; A = hasAExt; X = hasCustom;
    }

    public override uint Read()
    {
        uint v = 1u << 30; // MXL = 1
        if (X) v |= 1u << 23;
        if (U) v |= 1u << 20;
        if (M) v |= 1u << 12;
        if (C) v |= 1u <<  2;
        if (A) v |= 1u;
        return v;
    }

    public override void Write(uint value) { }
}

// ═════════════════════════════════════════════════════════════════════════════
// 3.2  Standard M-mode Trap Handling CSRs
// ═════════════════════════════════════════════════════════════════════════════

// mstatus — 0x300
public class MstatusEntry : CsrEntry
{
    [EntryValue("21", "timeout wait (U-mode only)")]
    public bool Tw   { get; set; }

    [EntryValue("17", "modify privilege (U-mode only)")]
    public bool Mprv { get; set; }

    [EntryValue("12:11", "previous privilege (0=U, 3=M)")]
    public uint Mpp  { get; set; }

    [EntryValue("7", "previous interrupt enable")]
    public bool Mpie { get; set; }

    [EntryValue("3", "interrupt enable")]
    public bool Mie  { get; set; }

    public MstatusEntry() : base(0x300, "mstatus", "Trap") { }

    public override uint Read()
    {
        uint v = 0;
        if (Tw)   v |= 1u << 21;
        if (Mprv) v |= 1u << 17;
        v |= (Mpp & 0x3u) << 11;
        if (Mpie) v |= 1u << 7;
        if (Mie)  v |= 1u << 3;
        return v;
    }

    public override void Write(uint value)
    {
        Tw   = (value >> 21 & 1u) != 0;
        Mprv = (value >> 17 & 1u) != 0;
        uint mpp = value >> 11 & 0x3u;
        Mpp  = mpp is 0 or 3 ? mpp : 3u; // round unsupported mode to M-mode
        Mpie = (value >>  7 & 1u) != 0;
        Mie  = (value >>  3 & 1u) != 0;
    }
}

// mstatush — 0x310 — hardwired to 0
public class MstatushEntry : CsrEntry
{
    public MstatushEntry() : base(0x310, "mstatush", "Trap") { }
    public override uint Read()            => 0;
    public override void Write(uint value) { }
}

// medeleg — 0x302 / mideleg — 0x303 — illegal instruction on access (no S-mode)
public class UnimplementedCsrEntry : CsrEntry
{
    public UnimplementedCsrEntry(ushort address, string name, string group = "Trap") : base(address, name, group) { }
    // Real emulator catches these and raises an illegal-instruction trap.
    public override uint Read()            => throw new InvalidOperationException($"{Name}: illegal instruction");
    public override void Write(uint value) => throw new InvalidOperationException($"{Name}: illegal instruction");
    public override uint Peek() => 0;
}

// Catch-all for registers hardwired to 0 with no side-effects.
public class HardwiredZeroEntry : CsrEntry
{
    public HardwiredZeroEntry(ushort address, string name, string group)
        : base(address, name, group) { }
    public override uint Read()            => 0;
    public override void Write(uint value) { }
}

// mie — 0x304 — per-source interrupt enables
public class MieEntry : CsrEntry
{
    [EntryValue("11", "external interrupt enable")]
    public bool Meie { get; set; }

    [EntryValue("7",  "timer interrupt enable")]
    public bool Mtie { get; set; }
    
    [EntryValue("3",  "software interrupt enable")]
    public bool Msie { get; set; }

    public MieEntry() : base(0x304, "mie", "Trap") { }

    public override uint Read()
    {
        uint v = 0;
        if (Meie) v |= 1u << 11;
        if (Mtie) v |= 1u <<  7;
        if (Msie) v |= 1u <<  3;
        return v;
    }

    public override void Write(uint value)
    {
        Meie = (value >> 11 & 1u) != 0;
        Mtie = (value >>  7 & 1u) != 0;
        Msie = (value >>  3 & 1u) != 0;
    }
}

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

    [EntryValue("7",  "timer interrupt pending")]
    public bool Mtip { get; set; }

    [EntryValue("3",  "software interrupt pending")]
    public bool Msip { get; set; }

    public MipEntry(MeicontextEntry meicontext, MeipaEntry meipa, MeieaEntry meiea, MeipraEntry meipra) : base(0x344, "mip", "Trap")
    {
        m_Meicontext = meicontext;
        m_Meipa = meipa;
        m_Meiea = meiea;
        m_Meipra = meipra;
    }

    public override uint Read()
    {
        uint v = 0;
        if (Meip) v |= 1u << 11;
        if (Mtip) v |= 1u <<  7;
        if (Msip) v |= 1u <<  3;
        return v;
    }

    public override void Write(uint value) { } // writable bits exist only for S/U modes
}

// mtvec — 0x305
public class MtvecEntry : CsrEntry
{
    [EntryValue("31:2", "trap vector base address")]
    public uint Base     { get; set; }

    [EntryValue("0",    "vectored mode (0=direct, 1=vectored)")]
    public bool Vectored { get; set; }

    public MtvecEntry() : base(0x305, "mtvec", "Trap") { }

    public override uint Read()            => (Base << 2) | (Vectored ? 1u : 0u);
    public override void Write(uint value) { Base = value >> 2; Vectored = (value & 1u) != 0; }
}

// mscratch — 0x340
public class MscratchEntry : CsrEntry
{
    private uint m_Value;
    public MscratchEntry() : base(0x340, "mscratch", "Trap") { }
    public override uint Read()            => m_Value;
    public override void Write(uint value) => m_Value = value;
}

// mepc — 0x341
public class MepcEntry : CsrEntry
{
    private uint m_Value;
    public MepcEntry() : base(0x341, "mepc", "Trap") { }
    public override uint Read()            => m_Value;
    public override void Write(uint value) => m_Value = value & ~0x3u; // bits 1:0 hardwired 0 (no C ext)
}

// mcause — 0x342
public class McauseEntry : CsrEntry
{
    private uint m_Value;

    [EntryValue("31", "vectored mode (0=direct, 1=vectored)")]
    public bool IsInterrupt => (m_Value & 0x8000_0000u) != 0;

    [EntryValue("30:0", "vectored mode (0=direct, 1=vectored)")]
    public uint CauseCode   =>  m_Value & 0x7FFF_FFFFu;

    public McauseEntry() : base(0x342, "mcause", "Trap") { }
    
    public override uint Read()            => m_Value;

    // Bit 31 + 5 LSBs cover all Hazard3 exception and interrupt causes.
    public override void Write(uint value) => m_Value = value & 0x8000_001Fu;
}

// mtval — 0x343 — hardwired to 0
public class MtvalEntry : CsrEntry
{
    public MtvalEntry() : base(0x343, "mtval", "Trap") { }
    public override uint Read()            => 0;
    public override void Write(uint value) { }
}

// mcounteren — 0x306 — U-mode counter access control
public class McounterenEntry : CsrEntry
{
    [EntryValue("2", "instret access (U-mode)")]
    public bool Ir { get; set; }

    [EntryValue("1", "time access (U-mode)")]
    public bool Tm { get; set; }

    [EntryValue("0", "cycle access (U-mode)")]
    public bool Cy { get; set; }

    public McounterenEntry() : base(0x306, "mcounteren", "Trap") { }
    public override uint Read()            => (Ir ? 4u : 0u) | (Tm ? 2u : 0u) | (Cy ? 1u : 0u);
    public override void Write(uint value) { Ir = (value & 4u) != 0; Tm = (value & 2u) != 0; Cy = (value & 1u) != 0; }
}

// ═════════════════════════════════════════════════════════════════════════════
// 3.3  Standard Memory Protection CSRs
// ═════════════════════════════════════════════════════════════════════════════

// pmpcfg0…3 — 0x3a0 through 0x3a3 — configuration for 4 PMP regions each
// Each byte covers one region: L(7), RES0(6:5), A(4:3), X(2), W(1), R(0).
public class PmpcfgEntry : CsrEntry
{
    private uint m_Value;

    public PmpcfgEntry(ushort address, int index)
        : base(address, $"pmpcfg{index}", "Memory Protection") { }

    public override uint Read() => m_Value;

    public override void Write(uint value)
    {
        uint result = 0;
        for (int i = 0; i < 4; i++)
        {
            // Locked bytes are read-only until reset.
            if ((m_Value >> (i * 8) & 0x80u) != 0) { result |= m_Value & (0xFFu << (i * 8)); continue; }
            byte src = (byte)(value >> (i * 8));
            // A field (bits 4:3): 0=OFF, 2=NA4, 3=NAPOT; value 1 is unsupported → OFF.
            byte a = (byte)((src >> 3) & 0x3);
            if (a == 1) a = 0;
            // Keep L(7), X(2), W(1), R(0); sanitize A; clear reserved bits 6:5.
            byte b = (byte)((src & 0x87u) | (uint)(a << 3));
            result |= (uint)b << (i * 8);
        }
        m_Value = result;
    }
}

// pmpaddr0…15 — 0x3b0 through 0x3bf — PMP region address registers (30-bit each)
public class PmpaddrEntry : CsrEntry
{
    private uint m_Value;

    [EntryValue("29:0", "PMP address in units of 4 bytes (30 significant bits)")]
    public uint Addr => m_Value;

    public PmpaddrEntry(ushort address, int index)
        : base(address, $"pmpaddr{index}", "Memory Protection") { }

    public override uint Read()            => m_Value;
    public override void Write(uint value) => m_Value = value & 0x3FFF_FFFFu;
}

// ═════════════════════════════════════════════════════════════════════════════
// 3.4  Standard M-mode Performance Counters
// ═════════════════════════════════════════════════════════════════════════════

// Shared 64-bit backing store for counter pairs (mcycle/mcycleh, minstret/minstreth).
public class Counter64 { public ulong Value { get; set; } }

// mcycle — 0xb00 — lower 32 bits of 64-bit cycle counter
public class McycleEntry : CsrEntry
{
    internal readonly Counter64 m_Counter = new();

    [EntryValue("31:0", "Lower 32 bits of 64-bit cycle counter")]
    public uint Low => (uint)m_Counter.Value;

    public McycleEntry() : base(0xb00, "mcycle", "Performance") { }

    public override uint Read()            => (uint)m_Counter.Value;
    public override void Write(uint value) => m_Counter.Value = (m_Counter.Value & 0xFFFF_FFFF_0000_0000UL) | value;
}

// mcycleh — 0xb80 — upper 32 bits of cycle counter (shares Counter64 with mcycle)
public class McyclehEntry : CsrEntry
{
    private readonly Counter64 m_Counter;

    [EntryValue("31:0", "Upper 32 bits of 64-bit cycle counter")]
    public uint High => (uint)(m_Counter.Value >> 32);

    public McyclehEntry(McycleEntry mcycle) : base(0xb80, "mcycleh", "Performance")
        => m_Counter = mcycle.m_Counter;

    public override uint Read()            => (uint)(m_Counter.Value >> 32);
    public override void Write(uint value) => m_Counter.Value = ((ulong)value << 32) | (uint)m_Counter.Value;
}

// minstret — 0xb02 — lower 32 bits of 64-bit instruction retire counter
public class MinstretEntry : CsrEntry
{
    internal readonly Counter64 m_Counter = new();

    [EntryValue("31:0", "Lower 32 bits of 64-bit instruction retire counter")]
    public uint Low => (uint)m_Counter.Value;

    public MinstretEntry() : base(0xb02, "minstret", "Performance") { }

    public override uint Read()            => (uint)m_Counter.Value;
    public override void Write(uint value) => m_Counter.Value = (m_Counter.Value & 0xFFFF_FFFF_0000_0000UL) | value;
}

// minstreth — 0xb82 — upper 32 bits of instruction retire counter
public class MinstrethEntry : CsrEntry
{
    private readonly Counter64 m_Counter;

    [EntryValue("31:0", "Upper 32 bits of 64-bit instruction retire counter")]
    public uint High => (uint)(m_Counter.Value >> 32);

    public MinstrethEntry(MinstretEntry minstret) : base(0xb82, "minstreth", "Performance")
        => m_Counter = minstret.m_Counter;

    public override uint Read()            => (uint)(m_Counter.Value >> 32);
    public override void Write(uint value) => m_Counter.Value = ((ulong)value << 32) | (uint)m_Counter.Value;
}

// mcountinhibit — 0x320 — counter inhibit (ir and cy reset to 1)
public class McountinhibitEntry : CsrEntry
{
    [EntryValue("2", "inhibit minstret/minstreth counting (resets to 1)")]
    public bool Ir { get; set; } = true;

    [EntryValue("0", "inhibit mcycle/mcycleh counting (resets to 1)")]
    public bool Cy { get; set; } = true;

    public McountinhibitEntry() : base(0x320, "mcountinhibit", "Performance") { }

    public override uint Read()            => (Ir ? 4u : 0u) | (Cy ? 1u : 0u);
    public override void Write(uint value) { Ir = (value & 4u) != 0; Cy = (value & 1u) != 0; }
}

// ═════════════════════════════════════════════════════════════════════════════
// 3.5  Standard Trigger CSRs
// ═════════════════════════════════════════════════════════════════════════════

// tselect — 0x7a0 — unimplemented: reads as 0, write causes illegal instruction
public class TselectEntry : CsrEntry
{
    public TselectEntry() : base(0x7a0, "tselect", "Trigger") { }
    public override uint Read()            => 0;
    public override uint Peek()            => 0;
    public override void Write(uint value) => throw new InvalidOperationException("tselect: illegal instruction");
}

// ═════════════════════════════════════════════════════════════════════════════
// 3.6  Standard Debug Mode CSRs
// ═════════════════════════════════════════════════════════════════════════════

// dcsr — 0x7b0 — debug control and status
// xdebugver hardwired 4; stepie hardwired 0; stopcount/stoptime hardwired 1.
public class DcsrEntry : CsrEntry
{
    [EntryValue("31:28", "xdebugver: external debug spec version (hardwired to 4)")]
    public uint Xdebugver => 4;

    [EntryValue("15", "ebreakm: ebreak in M-mode enters debug mode instead of trapping")]
    public bool Ebreakm { get; set; }

    [EntryValue("12", "ebreaku: ebreak in U-mode enters debug mode (hardwired 0 if no U-mode)")]
    public bool Ebreaku { get; set; }

    [EntryValue("8:6", "cause: reason for debug mode entry (read-only, set by hardware)")]
    public uint Cause { get; private set; }

    [EntryValue("2", "step: re-enter debug mode after each M-mode instruction")]
    public bool Step { get; set; }

    [EntryValue("1:0", "prv: privilege level at debug entry / exit (3=M, 0=U)")]
    public uint Prv { get; set; } = 3;

    public DcsrEntry() : base(0x7b0, "dcsr", "Debug") { }

    public override uint Read()
    {
        uint v = 4u << 28;           // xdebugver
        if (Ebreakm) v |= 1u << 15;
        if (Ebreaku) v |= 1u << 12;
        // stepie = 0 (hardwired)
        v |= 1u << 10;               // stopcount hardwired 1
        v |= 1u <<  9;               // stoptime hardwired 1
        v |= (Cause & 0x7u) << 6;
        if (Step) v |= 1u << 2;
        v |= Prv & 0x3u;
        return v;
    }

    public override void Write(uint value)
    {
        Ebreakm = (value >> 15 & 1u) != 0;
        Ebreaku = (value >> 12 & 1u) != 0;
        Step    = (value >>  2 & 1u) != 0;
        uint prv = value & 0x3u;
        Prv = prv is 0 or 3 ? prv : 3u;
    }

    internal void SetCause(uint cause) => Cause = cause & 0x7u;
}

// dpc — 0x7b1 — debug program counter (R/W in debug mode)
public class DpcEntry : CsrEntry
{
    private uint m_Value;

    [EntryValue("31:0", "Debug program counter")]
    public uint Pc => m_Value;

    public DpcEntry() : base(0x7b1, "dpc", "Debug") { }
    public override uint Read()            => m_Value;
    public override void Write(uint value) => m_Value = value & ~0x1u; // bit 0 hardwired 0
}

// ═════════════════════════════════════════════════════════════════════════════
// 3.8  Custom Interrupt Handling CSRs  (Xh3irq)
// ═════════════════════════════════════════════════════════════════════════════
//
// Windowed registers (meiea, meipa, meifa, meipra) use the "zero-lower-bits" trick:
//
//   Read() returns the window data in bits[31:16] with the lower bits = 0.
//
// When csrrs executes as  Set(Get() | rs1),  the window index for the write
// comes from rs1[4:0] alone because Get()[4:0] == 0. No GetForWrite override
// is needed and the emulator's existing Get/Set split works without change.

// meiea — 0xBE0 — external interrupt enable array (1 bit per IRQ)
public class MeieaEntry : CsrEntry, ICsrWindowed
{
    private readonly uint[] m_Windows = new uint[32]; // 32 × 16 bits = 512 IRQ enables
    private int m_Index;

    public MeieaEntry() : base(0xBE0, "meiea", "Custom IRQ") { }

    public int  WindowCount   => 32;
    public int  BitsPerWindow => 16;
    public int  BitsPerItem   => 1;
    public uint PeekWindow(int index) => GetWindow(index);

    public override uint Read()            => m_Windows[m_Index] << 16;
    public override void Write(uint value) { m_Index = (int)(value & 0x1Fu); m_Windows[m_Index] = value >> 16 & 0xFFFF; }

    public bool IsEnabled(int irq)   => (m_Windows[irq >> 4] &  (1u << (irq & 0xF))) != 0;
    public uint GetWindow(int index) =>  m_Windows[index];
}

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

// meipa — 0xBE1 — external interrupt pending array (computed, software read-only)
// Effective pending = hardware_asserted | meifa (force). Software cannot clear pending bits
// directly; the IRQ source must deassert or meifa must be cleared.
public class MeipaEntry : CsrEntry, ICsrWindowed
{
    private readonly uint[]     m_Hardware = new uint[32]; // driven by hardware peripherals
    private readonly MeifaEntry m_Meifa;
    private int m_Index;

    public MeipaEntry(MeifaEntry meifa) : base(0xBE1, "meipa", "Custom IRQ")
        => m_Meifa = meifa;

    public int  WindowCount   => 32;
    public int  BitsPerWindow => 16;
    public int  BitsPerItem   => 1;
    public uint PeekWindow(int index) => GetWindow(index);

    private uint ComputeWindow(int i) => m_Hardware[i] | m_Meifa.GetWindow(i);

    public override uint Read()            => ComputeWindow(m_Index) << 16;
    public override void Write(uint value) => m_Index = (int)(value & 0x1Fu); // index-only; window is read-only

    public bool IsPending(int irq)   => (ComputeWindow(irq >> 4) & (1u << (irq & 0xF))) != 0;
    public uint GetWindow(int index) =>  ComputeWindow(index);

    /// <summary>Called by hardware peripherals to assert or deassert an IRQ line.</summary>
    public void SetHardwarePending(int irq, bool pending)
    {
        if (pending) m_Hardware[irq >> 4] |=  (1u << (irq & 0xF));
        else         m_Hardware[irq >> 4] &= ~(1u << (irq & 0xF));
    }
}

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

    public override uint Read()            => m_Windows[m_Index] << 16;
    public override void Write(uint value) { m_Index = (int)(value & 0x7Fu); m_Windows[m_Index] = value >> 16 & 0xFFFF; }

    public byte GetPriority(int irq) => (byte)(m_Windows[irq >> 2] >> ((irq & 3) << 2) & 0xF);
}

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

    public override uint Read()
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

    public override void Write(uint value)
    {
        if ((value & 1u) == 0) return; // only the update bit has effect
        var (noIrq, irq) = FindNext();
        if (!noIrq && m_Meifa.IsForced(irq))
            m_Meifa.ClearForced(irq);
        m_Meicontext.ApplyUpdate(noIrq, irq, m_Meipra);
    }
}

// ═════════════════════════════════════════════════════════════════════════════
// 3.9  Custom Memory Protection CSRs
// ═════════════════════════════════════════════════════════════════════════════

// pmpcfgm0 — 0xbd0 — PMP M-mode configuration (one bit per region, no locking)
public class Pmpcfgm0Entry : CsrEntry
{
    [EntryValue("15:0", "M-mode apply bit per PMP region (OR'd with pmpcfg.L; does not lock)")]
    public ushort M { get; set; }

    public Pmpcfgm0Entry() : base(0xbd0, "pmpcfgm0", "Custom Memory") { }

    public override uint Read()            => M;
    public override void Write(uint value) => M = (ushort)(value & 0xFFFFu);
}

// ═════════════════════════════════════════════════════════════════════════════
// 3.10 Custom Power Control CSRs
// ═════════════════════════════════════════════════════════════════════════════

// msleep — 0xbf0 — M-mode sleep control (resets to 0)
public class MsleepEntry : CsrEntry
{
    [EntryValue("2", "sleeponblock: enter deep sleep on h3.block as well as wfi")]
    public bool Sleeponblock { get; set; }

    [EntryValue("1", "powerdown: release external power request when sleeping")]
    public bool Powerdown { get; set; }

    [EntryValue("0", "deepsleep: deassert clock enable when entering sleep state")]
    public bool Deepsleep { get; set; }

    public MsleepEntry() : base(0xbf0, "msleep", "Custom Power") { }

    public override uint Read()
    {
        uint v = 0;
        if (Sleeponblock) v |= 1u << 2;
        if (Powerdown)    v |= 1u << 1;
        if (Deepsleep)    v |= 1u;
        return v;
    }

    public override void Write(uint value)
    {
        Sleeponblock = (value >> 2 & 1u) != 0;
        Powerdown    = (value >> 1 & 1u) != 0;
        Deepsleep    =  (value     & 1u) != 0;
    }
}

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
