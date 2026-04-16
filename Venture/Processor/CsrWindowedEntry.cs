namespace Venture.Processor;

/// <summary>
/// A CSR that exposes a large array of state through a windowed access protocol.
///
/// Protocol: writing selects a window (lower <c>indexBits</c> bits) and simultaneously
/// updates the window data (bits [31:16]). Reading returns the current window data in
/// bits [31:16] and the current index in the lower bits.
///
/// The full backing array is also directly accessible for efficient bit-level use by
/// the interrupt controller — no need to go through the window protocol.
///
/// Spec: RP2350 Datasheet §3.8 — "Xh3irq external interrupt controller".
/// </summary>
public class CsrWindowedEntry : CsrEntry
{
    private readonly uint[] m_Windows;       // one uint per 16-bit window slot
    private readonly int    m_IndexBits;
    private readonly bool   m_WriteEnabled;
    private          int    m_CurrentIndex;

    /// <summary>
    /// Optional computed reader override — used for read-only derived registers
    /// (e.g. MEIPA = pending | force) where the data is computed at read time.
    /// Set after construction via <see cref="SetComputedReader"/>.
    /// </summary>
    private Func<int, uint>? m_ComputedReader;

    public int WindowCount => m_Windows.Length;

    internal CsrWindowedEntry(ushort address, string name, string group,
                               int windowCount, int indexBits, bool writeEnabled = true)
        : base(address, name, group)
    {
        m_Windows      = new uint[windowCount];
        m_IndexBits    = indexBits;
        m_WriteEnabled = writeEnabled;
    }

    /// <summary>
    /// Override window reads with a computed function.
    /// Used by IrqController to wire MEIPA as a derived view over pending | force.
    /// </summary>
    public void SetComputedReader(Func<int, uint> reader) => m_ComputedReader = reader;

    private uint ReadWindow(int index) =>
        m_ComputedReader != null ? m_ComputedReader(index) : m_Windows[index];

    // ── CsrEntry overrides ────────────────────────────────────────────────────

    // Spec: bits[4:0] (index field) are write-only self-clearing — "no value is stored".
    // Returning m_CurrentIndex here would corrupt csrrs: the SDK ORs the old CSR value
    // with the new (en_bit << 16 | window) mask. If old[4:0] = current_index != new_window,
    // the OR shifts the write to the wrong window and the intended window is never updated.
    public override uint Read() => ReadWindow(m_CurrentIndex) << 16;

    public override void Write(uint value)
    {
        int mask = (1 << m_IndexBits) - 1;
        m_CurrentIndex = (int)(value & (uint)mask);
        if (m_WriteEnabled)
            m_Windows[m_CurrentIndex] = (value >> 16) & 0xFFFF;
        RaiseChanged();
    }

    // ── UI access — no window-select side-effect ──────────────────────────────

    /// <summary>Returns all window values for UI display without modifying the current index.</summary>
    public IReadOnlyList<uint> ReadAllWindows()
    {
        var result = new uint[m_Windows.Length];
        for (int i = 0; i < m_Windows.Length; i++)
            result[i] = ReadWindow(i);
        return result;
    }

    // ── Direct data access for IrqController ─────────────────────────────────

    /// <summary>
    /// Reads the first four 16-bit windows packed into a 64-bit value.
    /// Covers IRQs 0–63 (windows 0–3). Used by IrqController for efficient
    /// mask arithmetic (enable, force, pending).
    /// </summary>
    public ulong GetUlong()
    {
        ulong result = 0;
        int   count  = Math.Min(4, m_Windows.Length);
        for (int i = 0; i < count; i++)
            result |= (ulong)m_Windows[i] << (i * 16);
        return result;
    }

    /// <summary>Read a single IRQ bit (for 1-bit-per-IRQ arrays: MEIEA, MEIFA).</summary>
    public bool GetBit(int irqNumber)
    {
        int window = irqNumber >> 4;   // / 16
        int bit    = irqNumber & 0xF;  // % 16
        return (m_Windows[window] & (1u << bit)) != 0;
    }

    /// <summary>Write a single IRQ bit.</summary>
    public void SetBit(int irqNumber, bool value)
    {
        int window = irqNumber >> 4;
        int bit    = irqNumber & 0xF;
        if (value) m_Windows[window] |=  (1u << bit);
        else       m_Windows[window] &= ~(1u << bit);
        RaiseChanged();
    }

    /// <summary>Read a 4-bit priority nibble for one IRQ (for MEIPRA).</summary>
    public byte GetNibble(int irqNumber)
    {
        int window = irqNumber >> 2;       // / 4
        int shift  = (irqNumber & 3) * 4;
        return (byte)((m_Windows[window] >> shift) & 0xF);
    }

    /// <summary>Write a 4-bit priority nibble for one IRQ.</summary>
    public void SetNibble(int irqNumber, byte priority)
    {
        int window = irqNumber >> 2;
        int shift  = (irqNumber & 3) * 4;
        m_Windows[window] = (m_Windows[window] & ~(0xFu << shift))
                          | ((uint)(priority & 0xF) << shift);
        RaiseChanged();
    }
}
