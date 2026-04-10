using Microsoft.Extensions.Logging;

namespace Venture;

public class XIP : IAddressableResource
{
    private readonly Memory m_Memory;
    private readonly ILogger<XIP> m_Logger;

    public uint BaseAddress => 0x10000000;

    public uint Size => 0x10000000;

    /// <summary>Raised on the calling thread whenever data is written to the flash region.</summary>
    public event Action? Written;

    public XIP(Memory memory, ILogger<XIP> logger)
    {
        m_Memory = memory;
        m_Logger = logger;
    }

    public byte[] Read(uint address, int count)
    {
        var offset = (address - BaseAddress) % 0x0400_0000;

        var type = (address - BaseAddress) / 0x0400_0000;
        if (type != 0)
        {
            m_Logger.LogWarning("Reading from XIP address: {address} offset: {offset} type: {type}", address, offset, type);
        }

        return m_Memory.Read(BaseAddress + offset, count);
    }

    public void Write(uint address, byte[] data)
    {
        var offset = (address - BaseAddress) % 0x0400_0000;

        var type = (address - BaseAddress) / 0x0400_0000;
        if (type != 0)
        {
            m_Logger.LogWarning("Writing to XIP address: {address} offset: {offset} type: {type}", address, offset, type);
            return;
        }

        m_Memory.Write(BaseAddress + offset, data);
        Written?.Invoke();
    }
}
