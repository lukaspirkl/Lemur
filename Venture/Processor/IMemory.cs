namespace Venture.Processor;

public interface IMemory
{
    uint InitialPC { get; }
    void Write(uint address, byte[] data);
    ArraySegment<byte> Read(uint address, int count);
}

public record MemoryWriteArgs(uint Address, byte[] Data);

public static class MemoryExtensions
{
    public static void WriteByte(this IMemory memory, uint address, byte data)
    {
        memory.Write(address, [data]);
    }

    public static void WriteHalfWord(this IMemory memory, uint address, ushort data)
    {
        memory.Write(address, BitConverter.GetBytes(data));
    }

    public static void WriteWord(this IMemory memory, uint address, uint data)
    {
        memory.Write(address, BitConverter.GetBytes(data));
    }

    public static byte ReadByte(this IMemory memory, uint address)
    {
        return memory.Read(address, 1)[0];
    }

    public static ushort ReadHalfWord(this IMemory memory, uint address)
    {
        return BitConverter.ToUInt16(memory.Read(address, 2));
    }

    public static uint ReadWord(this IMemory memory, uint address)
    {
        return BitConverter.ToUInt32(memory.Read(address, 4));
    }
}
