using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class UnimplementedPeripheralFactory
{
    private readonly ILogger<UnimplementedPeripheral> _logger;

    public UnimplementedPeripheralFactory(ILogger<UnimplementedPeripheral> logger)
    {
        _logger = logger;
    }

    public UnimplementedPeripheral Create(uint address, string name, uint size = 0x4000)
    {
        return new UnimplementedPeripheral(address, size, name, _logger);
    }
}

public class UnimplementedPeripheral : IAddressableResource
{
    private readonly string name;
    private readonly ILogger<UnimplementedPeripheral> logger;

    public uint StartAddress { get; }

    public uint Size { get; }

    public UnimplementedPeripheral(uint startAddress, uint size, string name, ILogger<UnimplementedPeripheral> logger)
    {
        StartAddress = startAddress;
        Size = size;
        this.name = name;
        this.logger = logger;
    }

    public byte[] Read(uint address, int count)
    {
        //throw new NotImplementedException($"Read from {address.ToHex()} - unimplemented peripheral: {name}");
        logger.LogError("Reading from {address} - unimplemented peripheral: {name}", address.ToHex(), name);
        return new byte[count];
    }

    public void Write(uint address, byte[] data)
    {
        //throw new NotImplementedException($"Write to {address.ToHex()} - unimplemented peripheral: {name}");
        logger.LogError("Write to {address} value {value} - unimplemented peripheral: {name}", address.ToHex(), data.ToHex(), name);
    }
}
