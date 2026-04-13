namespace Venture.Processor;

/// <summary>
/// Machine-level Control and Status Registers.
/// Spec: RISC-V Privileged ISA, Volume II — Chapter 2 (CSR listing) and Chapter 3 (Machine-level ISA).
///
/// The plain dictionary stores backing values for all CSRs.
/// Optional getter/setter hooks allow the processor (or future hardware models) to intercept
/// accesses to specific CSRs — e.g., to return live counter values or enforce write-masks.
/// Software accesses (CSRRW/CSRRS/etc.) go through <see cref="Get"/>/<see cref="Set"/>.
/// Internal hardware transitions (trap entry, MRET) use <see cref="RawGet"/>/<see cref="RawSet"/>
/// to bypass hooks and avoid spurious log noise.
/// </summary>
public class CSR
{
    // -------------------------------------------------------------------------
    // CSR addresses — Table 2.1 / 2.2, Privileged ISA
    // -------------------------------------------------------------------------

    /// <summary>0x300 — Machine status register. Contains MIE, MPIE and other status bits.</summary>
    public const ushort MSTATUS   = 0x300;

    /// <summary>0x304 — Machine interrupt-enable register. Per-source enable gates.</summary>
    public const ushort MIE       = 0x304;

    /// <summary>0x305 — Machine trap-handler base address. Encodes mode in bits [1:0].</summary>
    public const ushort MTVEC     = 0x305;

    /// <summary>0x340 — Scratch register, no hardware semantics; free for M-mode software use.</summary>
    public const ushort MSCRATCH  = 0x340;

    /// <summary>0x341 — Machine exception program counter. Holds faulting/interrupted PC.</summary>
    public const ushort MEPC      = 0x341;

    /// <summary>0x342 — Machine cause register. Bit 31 = interrupt flag; bits 30:0 = cause code.</summary>
    public const ushort MCAUSE    = 0x342;

    /// <summary>
    /// 0x343 — Machine bad address or instruction.
    /// For load/store faults: the faulting virtual address.
    /// For illegal instruction: the instruction encoding.
    /// Note: Hazard3 hardwires MTVAL to 0 in hardware, but the emulator populates it correctly
    /// to allow firmware that reads MTVAL to behave as if running on a more capable core.
    /// </summary>
    public const ushort MTVAL     = 0x343;

    /// <summary>0x344 — Machine interrupt-pending register. Set by hardware/peripherals.</summary>
    public const ushort MIP       = 0x344;

    /// <summary>0xB00 — Lower 32 bits of the cycle counter (MCYCLE).</summary>
    public const ushort MCYCLE    = 0xB00;

    /// <summary>0xB02 — Lower 32 bits of the instructions-retired counter (MINSTRET).</summary>
    public const ushort MINSTRET  = 0xB02;

    /// <summary>0xB80 — Upper 32 bits of MCYCLE.</summary>
    public const ushort MCYCLEH   = 0xB80;

    /// <summary>0xB82 — Upper 32 bits of MINSTRET.</summary>
    public const ushort MINSTRETH = 0xB82;

    // -------------------------------------------------------------------------
    // MSTATUS bit positions — Section 3.1.6
    // -------------------------------------------------------------------------

    /// <summary>Bit 3 of MSTATUS — Global machine-level interrupt enable.</summary>
    public const int MSTATUS_MIE_BIT  = 3;

    /// <summary>
    /// Bit 7 of MSTATUS — Previous MIE value, saved automatically on trap entry
    /// and restored by MRET. Allows nested interrupt handlers to re-enable interrupts
    /// without losing the outer handler's enable state.
    /// </summary>
    public const int MSTATUS_MPIE_BIT = 7;

    // -------------------------------------------------------------------------
    // MIP / MIE bit positions — Section 3.1.9, Table 3.6
    // -------------------------------------------------------------------------

    /// <summary>
    /// Bit 3 of MIP/MIE — Machine software interrupt pending/enable.
    /// Raised by writing MSIP in the memory-mapped CLINT or by SIO RISCV_SOFTIRQ.
    /// </summary>
    public const int MxP_MSIP_BIT = 3;

    /// <summary>
    /// Bit 7 of MIP/MIE — Machine timer interrupt pending/enable.
    /// Set when MTIME >= MTIMECMP (SIO registers 0x1B0–0x1BF).
    /// </summary>
    public const int MxP_MTIP_BIT = 7;

    /// <summary>
    /// Bit 11 of MIP/MIE — Machine external interrupt pending/enable.
    /// Set by the IRQ controller when any enabled peripheral IRQ line is asserted.
    /// </summary>
    public const int MxP_MEIP_BIT = 11;

    // -------------------------------------------------------------------------
    // MTVEC mode encoding — Section 3.1.7
    // -------------------------------------------------------------------------

    /// <summary>
    /// MTVEC mode 0 — Direct: all traps jump to BASE (bits [31:2] of MTVEC).
    /// </summary>
    public const uint MTVEC_MODE_DIRECT   = 0;

    /// <summary>
    /// MTVEC mode 1 — Vectored: asynchronous interrupts jump to BASE + 4×cause.
    /// Synchronous exceptions still jump to BASE in this mode.
    /// </summary>
    public const uint MTVEC_MODE_VECTORED = 1;

    // -------------------------------------------------------------------------
    // Implementation
    // -------------------------------------------------------------------------

    private readonly Dictionary<ushort, uint>        m_Csr     = new();
    private readonly Dictionary<ushort, Func<uint>>  m_Getters = new();
    private readonly Dictionary<ushort, Action<uint>> m_Setters = new();
    private readonly IEmuLogger<CSR> m_Logger;

    public CSR(IEmuLogger<CSR> logger)
    {
        m_Logger = logger;
    }

    /// <summary>
    /// Registers a custom getter for the given CSR address.
    /// When registered, <see cref="Get"/> calls this function instead of reading the backing store.
    /// Useful for live counters (MCYCLE, MINSTRET) whose values are held in the processor, not here.
    /// </summary>
    public void AddGetter(ushort address, Func<uint> getter) => m_Getters[address] = getter;

    /// <summary>
    /// Registers a custom setter for the given CSR address.
    /// When registered, <see cref="Set"/> calls this instead of writing the backing store directly.
    /// The setter is responsible for persisting the value if needed (e.g., updating a counter field).
    /// </summary>
    public void AddSetter(ushort address, Action<uint> setter) => m_Setters[address] = setter;

    /// <summary>
    /// Software CSR write (used by CSRRW / CSRRS / CSRRC instructions).
    /// Invokes the registered setter hook if present, otherwise writes the backing store.
    /// </summary>
    public void Set(ushort key, uint value)
    {
        m_Logger.LogCSRSet(key, value);
        if (m_Setters.TryGetValue(key, out var setter))
            setter(value);
        else
            m_Csr[key] = value;
    }

    /// <summary>
    /// Software CSR read (used by CSRRW / CSRRS / CSRRC instructions).
    /// Invokes the registered getter hook if present, otherwise reads the backing store.
    /// </summary>
    public uint Get(ushort key)
    {
        uint value = m_Getters.TryGetValue(key, out var getter)
            ? getter()
            : m_Csr.GetValueOrDefault(key, 0u);
        m_Logger.LogCSRGet(key, value);
        return value;
    }

    /// <summary>
    /// Hardware-internal read: reads the backing store directly, bypassing getter hooks.
    /// Used by trap-entry/MRET logic and interrupt checks where hook overhead is undesirable
    /// and the raw stored value is what matters.
    /// </summary>
    internal uint RawGet(ushort key) => m_Csr.GetValueOrDefault(key, 0u);

    /// <summary>
    /// Hardware-internal write: writes the backing store directly, bypassing setter hooks.
    /// Used by trap-entry/MRET to update MSTATUS, MEPC, MCAUSE, MTVAL atomically.
    /// Also used by <see cref="Hazard3Processor.SetMip"/> so peripherals can drive interrupt lines
    /// without going through the software CSR path.
    /// </summary>
    internal void RawSet(ushort key, uint value) => m_Csr[key] = value;
}
