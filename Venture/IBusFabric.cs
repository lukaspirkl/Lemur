namespace Venture;

public interface IBusFabric
{
    void Write(uint address, byte[] data);
    byte[] Read(uint address, int count);
}

public static class BusFabricExtensions
{
    public static void WriteByte(this IBusFabric memory, uint address, byte data)
    {
        memory.Write(address, new byte[] { data });
    }

    public static void WriteHalfWord(this IBusFabric memory, uint address, ushort data)
    {
        memory.Write(address, BitConverter.GetBytes(data));
    }

    public static void WriteWord(this IBusFabric memory, uint address, uint data)
    {
        memory.Write(address, BitConverter.GetBytes(data));
    }

    public static byte ReadByte(this IBusFabric memory, uint address)
    {
        return memory.Read(address, 1)[0];
    }

    public static ushort ReadHalfWord(this IBusFabric memory, uint address)
    {
        return BitConverter.ToUInt16(memory.Read(address, 2));
    }

    public static uint ReadWord(this IBusFabric memory, uint address)
    {
        return BitConverter.ToUInt32(memory.Read(address, 4));
    }
}
