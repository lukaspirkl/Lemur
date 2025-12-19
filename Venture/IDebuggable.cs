using Venture.Processor;

namespace Venture
{
    public interface IDebuggable : IDisposable
    {
        IRegisters Registers { get; }

        byte[] MemoryRead(uint address, int count);
        void MemoryWrite(uint address, byte[] data);
        void Run();
        void Step();
        void Stop();
        void Reset();

        uint GetCSR(ushort index);
        void SetCSR(ushort index, uint value);
    }
}