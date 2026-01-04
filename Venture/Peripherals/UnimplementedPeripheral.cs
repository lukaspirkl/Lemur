using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class UnimplementedPeripheralFactory
{
    private readonly ILogger<UnimplementedPeripheral> m_Logger;

    public UnimplementedPeripheralFactory(ILogger<UnimplementedPeripheral> logger)
    {
        m_Logger = logger;
    }

    public UnimplementedPeripheral Create(uint address, string name, uint? size)
    {
        return new UnimplementedPeripheral(address, size ?? 0x4000, name, m_Logger);
    }
}

public class UnimplementedPeripheral : IAddressableResource
{
    private readonly string m_Name;
    private readonly ILogger<UnimplementedPeripheral> m_Logger;

    public uint BaseAddress { get; }

    public uint Size { get; }

    public UnimplementedPeripheral(uint startAddress, uint size, string name, ILogger<UnimplementedPeripheral> logger)
    {
        BaseAddress = startAddress;
        Size = size;
        m_Name = name;
        m_Logger = logger;
    }

    public byte[] Read(uint address, int count)
    {
        //throw new NotImplementedException($"Read from {address.ToHex()} - unimplemented peripheral: {name}");
        m_Logger.LogError("Reading from {address} - unimplemented peripheral: {name}", address.ToHex(), m_Name);
        return new byte[count];
    }

    public void Write(uint address, byte[] data)
    {
        //throw new NotImplementedException($"Write to {address.ToHex()} - unimplemented peripheral: {name}");
        m_Logger.LogError("Write to {address} value {value} - unimplemented peripheral: {name}", address.ToHex(), data.ToHex(), m_Name);
    }
}
