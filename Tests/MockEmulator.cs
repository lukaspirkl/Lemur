using Venture;
using Venture.Processor;

namespace Tests;

public class MockEmulator : IDebuggable
{
    public class MockRegisters : IRegisters
    {
        private readonly uint[] data = new uint[4];

        public uint this[uint index] { get => data[index]; set => data[index] = value; }

        public uint Length => (uint)data.Length;
    }

    private readonly MockRegisters registers = new MockRegisters();

    private readonly Dictionary<ushort, uint> csr = new Dictionary<ushort, uint>();

    public IRegisters Registers => registers;

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

    public void Reset()
    {

    }

    public void Dispose()
    {
    }

    public uint GetCSR(ushort index)
    {
        return csr.GetValueOrDefault(index, (uint)0);
    }

    public void SetCSR(ushort index, uint value)
    {
        csr[index] = value;
    }
}