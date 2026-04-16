namespace Venture.Processor;

/// <summary>
/// Field metadata for one bit-range within a CSR.
/// Used only for display (CSR viewer); the emulator always reads/writes whole 32-bit values.
/// </summary>
public record CsrFieldDef(string Name, int Lsb, int Width, Func<uint, string>? Interpret = null)
{
    public int    Msb  => Lsb + Width - 1;
    public string Bits => Width == 1 ? $"[{Lsb}]" : $"[{Msb}:{Lsb}]";
}

/// <summary>
/// One CSR register — backing value, optional live getter/setter, write mask, and field metadata.
/// Registered via <see cref="CSR.Register"/> using the fluent builder methods.
/// </summary>
public class CsrEntry
{
    // ── Metadata ──────────────────────────────────────────────────────────────

    public ushort                   Address { get; }
    public string                   Name    { get; }
    public string                   Group   { get; }
    public IReadOnlyList<CsrFieldDef> Fields => m_Fields;

    // ── Runtime ───────────────────────────────────────────────────────────────

    private readonly List<CsrFieldDef> m_Fields   = new();
    private          uint              m_Value;
    private          Func<uint>?       m_Getter;    // live read (e.g. MCYCLE)
    private          Action<uint>?     m_Setter;    // live write
    private          uint              m_WriteMask = 0xFFFF_FFFFu;

    /// <summary>Wired by <see cref="CSR"/> at registration to propagate changes to subscribers.</summary>
    internal Action<ushort, uint>? OnChanged;

    internal CsrEntry(ushort address, string name, string group)
    {
        Address = address;
        Name    = name;
        Group   = group;
    }

    // ── Software access (CSRRW / CSRRS / CSRRC) ──────────────────────────────

    /// <summary>Software read — invokes getter hook if present, otherwise reads backing store.</summary>
    public virtual uint Read() => m_Getter?.Invoke() ?? m_Value;

    /// <summary>Software write — applies write mask, invokes setter hook or updates backing store.</summary>
    public virtual void Write(uint value)
    {
        uint masked = value & m_WriteMask;
        if (m_Setter != null)
            m_Setter(masked);
        else
            m_Value = masked;
        RaiseChanged();
    }

    // ── Hardware-internal access (trap entry, MRET, interrupt checks) ─────────

    /// <summary>
    /// Bypasses getter hook. Returns backing store directly.
    /// Use when the processor needs the stored value, not a live computed value.
    /// </summary>
    public uint ForceRead() => m_Value;

    /// <summary>
    /// Bypasses write mask and setter hook. Writes backing store directly.
    /// Used by trap entry / MRET to update MSTATUS, MEPC, MCAUSE atomically.
    /// </summary>
    public void ForceWrite(uint value)
    {
        m_Value = value;
        RaiseChanged();
    }

    // ── Fluent builder ────────────────────────────────────────────────────────

    public CsrEntry Field(string name, int lsb, int width, Func<uint, string>? interpret = null)
    {
        m_Fields.Add(new CsrFieldDef(name, lsb, width, interpret));
        return this;
    }

    /// <summary>Mark read-only (write mask = 0) and optionally set a reset value.</summary>
    public CsrEntry ReadOnly(uint resetValue = 0)
    {
        m_WriteMask = 0;
        m_Value     = resetValue;
        return this;
    }

    /// <summary>Set a write mask; only bits set in <paramref name="mask"/> can be written.</summary>
    public CsrEntry WithWriteMask(uint mask)
    {
        m_WriteMask = mask;
        return this;
    }

    /// <summary>
    /// Attach live getter/setter delegates (e.g. MCYCLE that reads from a processor counter).
    /// May be called post-construction to allow deferred wiring (e.g. from IrqController).
    /// </summary>
    public CsrEntry LiveValue(Func<uint> getter, Action<uint>? setter = null)
    {
        m_Getter = getter;
        m_Setter = setter;
        return this;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    protected void RaiseChanged() => OnChanged?.Invoke(Address, Read());
}
