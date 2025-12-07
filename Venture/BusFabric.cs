namespace Venture;

public class BusFabric : IBusFabric
{
    private readonly IAddressableResource[] resources;

    public BusFabric(IAddressableResource[] resources)
    {
        if (!BitConverter.IsLittleEndian)
        {
            throw new InvalidOperationException("This platform is not little endian.");
        }

        this.resources = resources;
    }

    private bool CanHandle(IAddressableResource resource, uint address)
    {
        return address - resource.StartAddress >= 0 && address - resource.StartAddress < resource.Size; ;
    }

    public void Write(uint address, byte[] data)
    {
        Console.WriteLine($"mem {address.ToHex()} {data.ToHex()}");

        var segment = resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new IndexOutOfRangeException($"Writing to invalid memory: {address.ToHex()}");
        }

        segment.Write(address, data);
    }

    public byte[] Read(uint address, int count)
    {
        var segment = resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new IndexOutOfRangeException($"Reading invalid memory: {address.ToHex()}");
        }

        return segment.Read(address, count);
    }
}
