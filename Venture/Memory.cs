using Microsoft.Extensions.Logging;

namespace Venture;

public class MemoryFactory
{
    private readonly ILogger<Memory> m_Logger;

    public MemoryFactory(ILogger<Memory> logger)
    {
        m_Logger = logger;
    }

    public Memory Create(string name, uint startAddress, uint size, bool isReadonly = false)
    {
        return new Memory(name, startAddress, size, isReadonly, m_Logger);
    }
}

public class Memory : IAddressableResource
{
    private readonly string m_Name;
    private readonly bool m_IsReadonly;
    private readonly ILogger<Memory> m_Logger;
    private readonly byte[] m_Memory;

    public uint BaseAddress { get; }
    public uint Size { get; }

    public Memory(string name, uint startAddress, uint size, bool isReadonly, ILogger<Memory> logger)
    {
        m_Name = name;
        m_Memory = new byte[size];
        Array.Fill<byte>(m_Memory, 0xFF);

        BaseAddress = startAddress;
        Size = size;
        m_IsReadonly = isReadonly;
        m_Logger = logger;
    }

    public void Write(uint address, byte[] data)
    {
        if (m_IsReadonly)
        {
            m_Logger.LogError("Writing to read only memory {name} address {address} value {value}", m_Name, address.ToHex(), data.ToHex());
            return;
        }

        data.CopyTo(m_Memory, (int)(address - BaseAddress));
    }

    public byte[] Read(uint address, int count)
    {
        return m_Memory.Skip((int)(address - BaseAddress)).Take(count).ToArray();
    }

    public void LoadBin(string path)
    {
        var loaded = File.ReadAllBytes(path);
        loaded.CopyTo(m_Memory, 0);
    }

    public void Load(Stream stream)
    {
        using var ms = new MemoryStream(m_Memory);
        stream.CopyTo(ms);
    }
}
