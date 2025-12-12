using Venture.Processor;

namespace Venture
{
    public interface IDebuggable
    {
        IIndexable<uint> Registers { get; }

        byte[] MemoryRead(uint address, int count);
        void MemoryWrite(uint address, byte[] data);
        void Run();
        void Step();
        void Stop();
    }
}