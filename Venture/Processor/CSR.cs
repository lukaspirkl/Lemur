namespace Venture.Processor;

public class CSR
{
    private readonly Dictionary<ushort, uint> m_Csr = new();
    private readonly IEmuLogger<CSR> m_Logger;

    public CSR(IEmuLogger<CSR> logger)
    {
        m_Logger = logger;
    }

    public void Set(ushort key, uint value)
    {
        m_Logger.LogCSRSet(key, value);
        m_Csr[key] = value;
    }

    public uint Get(ushort key)
    {
        var value = m_Csr.GetValueOrDefault(key, (uint)0);
        m_Logger.LogCSRGet(key, value);
        return value;
    }
}
