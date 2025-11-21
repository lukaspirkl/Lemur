namespace Venture.Processor;

public class Memory : IMemory
{
    private readonly ReadWriteMemoryBase[] memory;

    public Memory(ReadWriteMemoryBase[] memory)
    {
        if (!BitConverter.IsLittleEndian)
        {
            throw new InvalidOperationException("This platform is not little endian.");
        }

        this.memory = memory;
    }

    public uint InitialPC => memory.FirstOrDefault()?.StartAddress ?? 0;

    public void Write(uint address, byte[] data)
    {
        var segment = memory.FirstOrDefault(x => x.CanHandle(address));
        if (segment == null)
        {
            throw new IndexOutOfRangeException($"Writing to invalid memory: {address.ToHex()}");
        }

        segment.Write(address, data);
    }

    public ArraySegment<byte> Read(uint address, int count)
    {
        var segment = memory.FirstOrDefault(x => x.CanHandle(address));
        if (segment == null)
        {
            throw new IndexOutOfRangeException($"Reading invalid memory: {address.ToHex()}");
        }

        return segment.Read(address, count);
    }
}
