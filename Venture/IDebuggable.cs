using Venture.Processor;

namespace Venture;

public interface IDebuggable : IDisposable
{
    IRegisters Registers { get; }

    HashSet<uint> Brakpoints { get; }

    event EventHandler? Stopped;

    byte[] MemoryRead(uint address, int count);
    void MemoryWrite(uint address, byte[] data);
    void Run();
    void Step();
    void Stop();
    void Reset();

    uint GetCSR(ushort index);
    void SetCSR(ushort index, uint value);
}

public static class DebuggableExtensions
{
    public static uint MemoryRead32(this IDebuggable debuggable, uint address)
    {
        return BitConverter.ToUInt32(debuggable.MemoryRead(address, 4));
    }

    public static void MemoryWrite32(this IDebuggable debuggable, uint address, uint value)
    {
        debuggable.MemoryWrite(address, BitConverter.GetBytes(value));
    }
}
