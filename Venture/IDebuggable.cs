using Venture.Processor;

namespace Venture;

public interface IDebuggable : IDisposable
{
    IRegisters Registers { get; }

    HashSet<uint> Brakpoints { get; }

    event Action? Stopped;

    event Action? EBreak;

    byte[] MemoryRead(uint address, int count);
    void MemoryWrite(uint address, byte[] data);
    void Run();
    void Step();
    void RunTo(uint address);
    void Stop();
    void Reset();

    uint GetCSR(ushort index);
    void SetCSR(ushort index, uint value);
}

public static class DebuggableExtensions
{
    public static ushort MemoryRead16(this IDebuggable debuggable, uint address)
    {
        return BitConverter.ToUInt16(debuggable.MemoryRead(address, 2));
    }

    public static uint MemoryRead32(this IDebuggable debuggable, uint address)
    {
        return BitConverter.ToUInt32(debuggable.MemoryRead(address, 4));
    }

    public static void MemoryWrite32(this IDebuggable debuggable, uint address, uint value)
    {
        debuggable.MemoryWrite(address, BitConverter.GetBytes(value));
    }
}

public class NullDebuggable : IDebuggable
{
    public IRegisters Registers { get; } = new Registers(new NullEmuLogger<Registers>());

    public HashSet<uint> Brakpoints { get; } = new HashSet<uint>();

    public event Action? Stopped { add { } remove { } }

    public event Action? EBreak { add { } remove { } }

    public void Dispose()
    {
    }

    public uint GetCSR(ushort index)
    {
        return 0;
    }

    public byte[] MemoryRead(uint address, int count)
    {
        return new byte[count];
    }

    public void MemoryWrite(uint address, byte[] data)
    {
    }

    public void Reset()
    {
    }

    public void Run()
    {
    }

    public void RunTo(uint address)
    {
    }

    public void SetCSR(ushort index, uint value)
    {
    }

    public void Step()
    {
    }

    public void Stop()
    {
    }
}