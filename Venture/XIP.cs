using Microsoft.Extensions.Logging;

namespace Venture;

public class XIP : IAddressableResource
{
    private readonly Memory memory;
    private readonly ILogger<XIP> logger;

    public uint BaseAddress => 0x10000000;

    public uint Size => 0x10000000;

    public XIP(Memory memory, ILogger<XIP> logger)
    {
        this.memory = memory;
        this.logger = logger;
    }

    public byte[] Read(uint address, int count)
    {
        var offset = (address - BaseAddress) % 0x0400_0000;
        
        var type = (address - BaseAddress) / 0x0400_0000;
        if (type != 0)
        {
            logger.LogWarning("Reading from XIP address: {address} offset: {offset} type: {type}", address, offset, type);
        }

        return memory.Read(BaseAddress + offset, count);
    }

    public void Write(uint address, byte[] data)
    {
        var offset = (address - BaseAddress) % 0x0400_0000;

        var type = (address - BaseAddress) / 0x0400_0000;
        if (type != 0)
        {
            logger.LogWarning("Writing to XIP address: {address} offset: {offset} type: {type}", address, offset, type);
            return;
        }

        memory.Write(BaseAddress + offset, data);
    }
}
