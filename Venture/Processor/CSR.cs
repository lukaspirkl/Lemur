namespace Venture.Processor;

public class CSR
{
    private readonly Dictionary<ushort, uint> csr = new Dictionary<ushort, uint>();

    public void Set(ushort key, uint value)
    {
        //Console.WriteLine($"csr[{((uint)key).ToHex(4)}] <- {value.ToHex()}");
        csr[key] = value;
    }

    public uint Get(ushort key)
    {
        return csr.GetValueOrDefault(key, (uint)0);
    }
}
