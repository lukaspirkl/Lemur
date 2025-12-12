using Venture;
using Venture.Processor;

namespace Tests;

public class MockEmulator : IDebuggable
{
    public class MockRegisters : IIndexable<uint>
    {
        private readonly uint[] data = new uint[4];

        public uint this[uint index] { get => data[index]; set => data[index] = value; }

        public int Length => data.Length;
    }

    private readonly MockRegisters registers = new MockRegisters();

    public IIndexable<uint> Registers => registers;

    public byte[] Memory = new byte[32];

    public byte[] MemoryRead(uint address, int count)
    {
        return Memory.Skip((int)address).Take(count).ToArray();
    }

    public void MemoryWrite(uint address, byte[] data)
    {
        data.CopyTo(Memory, address);
    }

    public void Run()
    {
        
    }

    public void Step()
    {
        
    }

    public void Stop()
    {
        
    }
}