using Venture;
using Venture.Processor;

namespace Tests;

public class MockEmulator : IDebuggable
{
    public class MockRegisters : IRegisters
    {
        private readonly uint[] m_Data = new uint[4];

        public uint this[uint index] { get => m_Data[index]; set => m_Data[index] = value; }

        public uint Length => (uint)m_Data.Length;
    }

    private readonly MockRegisters m_Registers = new MockRegisters();

    private readonly Dictionary<ushort, uint> m_Csr = new Dictionary<ushort, uint>();

    public IRegisters Registers => m_Registers;

    public HashSet<uint> Brakpoints { get; } = new HashSet<uint>(); 

    public byte[] Memory = new byte[32];

    public event Action? Stopped { add { } remove { } }

    public event Action? EBreak { add { } remove { } }

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

    public void RunTo(uint address)
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
        return m_Csr.GetValueOrDefault(index, (uint)0);
    }

    public void SetCSR(ushort index, uint value)
    {
        m_Csr[index] = value;
    }
}