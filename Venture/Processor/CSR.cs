namespace Venture.Processor;

public class CSR
{
    private readonly Dictionary<ushort, uint> csr = new Dictionary<ushort, uint>();
    private readonly IEmuLogger<CSR> logger;

    public CSR(IEmuLogger<CSR> logger)
    {
        this.logger = logger;
    }

    public void Set(ushort key, uint value)
    {
        logger.LogCSRSet(key, value);
        csr[key] = value;
    }

    public uint Get(ushort key)
    {
        var value = csr.GetValueOrDefault(key, (uint)0);
        logger.LogCSRGet(key, value);
        return value;
    }
}
