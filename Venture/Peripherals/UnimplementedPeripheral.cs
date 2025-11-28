namespace Venture.Peripherals;

public class UnimplementedPeripheral : IAddressableResource
{
    private readonly string name;

    public uint StartAddress { get; }

    public uint Size => 0x4000;

    public UnimplementedPeripheral(uint startAddress, string name)
    {
        StartAddress = startAddress;
        this.name = name;
    }

    public byte[] Read(uint address, int count)
    {
        throw new NotImplementedException($"Read from {address.ToHex()} - unimplemented peripheral: {name}");
    }

    public void Write(uint address, byte[] data)
    {
        throw new NotImplementedException($"Read to {address.ToHex()} - unimplemented peripheral: {name}");
    }
}
