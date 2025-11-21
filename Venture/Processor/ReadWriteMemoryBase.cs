using System;

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
}
