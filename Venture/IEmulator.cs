using Venture.Processor;

namespace Venture
{
    public interface IEmulator
    {
        IIndexable<uint> Registers { get; }

        ArraySegment<byte> MemoryRead(uint address, int count);
        void MemoryWrite(uint address, byte[] data);
        void Run();
        void Step();
        void Stop();
    }
}