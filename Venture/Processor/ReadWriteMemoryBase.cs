namespace Venture.Processor;

public abstract class ReadWriteMemoryBase
{
    public uint StartAddress { get; }
    public uint Size { get; }

    protected ReadWriteMemoryBase(uint startAddress, uint size)
    {
        StartAddress = startAddress;
        Size = size;
    }

    public virtual bool CanHandle(uint address)
    {
        return address - StartAddress >= 0 && address - StartAddress < Size;
    }

    public abstract ArraySegment<byte> Read(uint address, int count);
    public abstract void Write(uint address, byte[] data);



    public void WriteByte(uint address, byte data)
    {
        Write(address, [data]);
    }

    public void WriteHalfWord(uint address, ushort data)
    {
        Write(address, BitConverter.GetBytes(data));
    }

    public void WriteWord(uint address, uint data)
    {
        Write(address, BitConverter.GetBytes(data));
    }

    public byte ReadByte(uint address)
    {
        return Read(address, 1)[0];
    }

    public ushort ReadHalfWord(uint address)
    {
        return BitConverter.ToUInt16(Read(address, 2));
    }

    public uint ReadWord(uint address)
    {
        return BitConverter.ToUInt32(Read(address, 4));
    }
}
