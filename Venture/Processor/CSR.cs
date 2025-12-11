using Microsoft.Extensions.Logging;

namespace Venture.Processor;

public class CSR
{
    private readonly Dictionary<ushort, uint> csr = new Dictionary<ushort, uint>();
    private readonly ILogger<CSR> logger;

    public CSR(ILogger<CSR> logger)
    {
        this.logger = logger;
    }

    public void Set(ushort key, uint value)
    {
        logger.LogInformation("csr[{reg}] <- {value} (set)", ((uint)key).ToHex(4), value.ToHex());
        csr[key] = value;
    }

    public uint Get(ushort key)
    {
        var value = csr.GetValueOrDefault(key, (uint)0);
        logger.LogInformation("csr[{reg}] -> {value} (get)", ((uint)key).ToHex(4), value.ToHex());
        return value;
    }
}
