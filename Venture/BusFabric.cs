namespace Venture;

public class BusFabric : IBusFabric
{
    private readonly IEnumerable<IAddressableResource> resources;
    private readonly IEmuLogger<BusFabric> logger;

    public BusFabric(IEnumerable<IAddressableResource> resources, IEmuLogger<BusFabric> logger)
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
        logger.LogMemoryWrite(address, data);

        if (data.Length != 1 && data.Length != 2 && data.Length != 4)
        {
            throw new RiscVException(ExceptionCause.StoreAddressMisaligned, address, $"It is possible to write only word, halfword, or byte. (address: {address.ToHex()} data: {data.ToHex()})");
        }

        if (address % data.Length != 0)
        {
            throw new RiscVException(ExceptionCause.StoreAddressMisaligned, address, $"Misaligned address {address.ToHex()} when writing {data.Length} bytes.");
        }

        var segment = resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new IndexOutOfRangeException($"Writing to invalid memory: {address.ToHex()}");
        }

        segment.Write(address, data);
    }

    public uint ReadInstruction(uint address)
    {
        if (address % 2 != 0)
        {
            throw new RiscVException(ExceptionCause.InstructionAddressMisaligned, address, $"Misaligned address {address.ToHex()} when reading instruction.");
        }

        var segment = resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new IndexOutOfRangeException($"Reading invalid memory: {address.ToHex()}");
        }

        return BitConverter.ToUInt32(segment.Read(address, 4));;
    }

    public byte[] Read(uint address, int count)
    {
        logger.LogMemoryRead(address, count);

        if (count != 1 && count != 2 && count != 4)
        {
            throw new RiscVException(ExceptionCause.LoadAddressMisaligned, address, $"It is possible to read only word, halfword, or byte. (address: {address.ToHex()} count: {count})");
        }

        if (address % count != 0)
        {
            throw new RiscVException(ExceptionCause.LoadAddressMisaligned, address, $"Misaligned address {address.ToHex()} when reading {count} bytes.");
        }

        var segment = resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new IndexOutOfRangeException($"Reading invalid memory: {address.ToHex()}");
        }

        return segment.Read(address, count);
    }
}
