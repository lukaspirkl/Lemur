using Microsoft.Extensions.Logging;

namespace Venture;

public class BusFabric : IBusFabric
{
    private readonly IEnumerable<IAddressableResource> resources;
    private readonly ILogger<BusFabric> logger;

    public BusFabric(IEnumerable<IAddressableResource> resources, ILogger<BusFabric> logger)
    {
        if (!BitConverter.IsLittleEndian)
        {
            throw new InvalidOperationException("This platform is not little endian.");
        }

        this.resources = resources;
        this.logger = logger;
    }

    private bool CanHandle(IAddressableResource resource, uint address)
    {
        return address - resource.StartAddress >= 0 && address - resource.StartAddress < resource.Size; ;
    }

    public void Write(uint address, byte[] data)
    {
        logger.LogDebug("Write to memory {address} value {data}", address.ToHex(), data.ToHex());

        var segment = resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new IndexOutOfRangeException($"Writing to invalid memory: {address.ToHex()}");
        }

        segment.Write(address, data);
    }

    public byte[] Read(uint address, int count)
    {
        logger.LogDebug("Reading from memory {address} (count: {count})", address.ToHex(), count);

        var segment = resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new IndexOutOfRangeException($"Reading invalid memory: {address.ToHex()}");
        }

        return segment.Read(address, count);
    }
}
