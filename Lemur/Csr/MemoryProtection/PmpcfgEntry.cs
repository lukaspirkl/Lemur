namespace Lemur.Csr.MemoryProtection;

// pmpcfg0…3 — 0x3a0 through 0x3a3 — configuration for 4 PMP regions each
// Each byte covers one region: L(7), RES0(6:5), A(4:3), X(2), W(1), R(0).
public class PmpcfgEntry : CsrEntry
{
    private uint m_Value;

    public PmpcfgEntry(ushort address, int index)
        : base(address, $"pmpcfg{index}", "Memory Protection") { }

    protected override uint ReadCore() => m_Value;

    protected override void WriteCore(uint value)
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
