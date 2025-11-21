using Venture.Processor;

namespace Venture;

public class Peripheral : ReadWriteMemoryBase
{
    byte[] memory = new byte[0x1000]; // = 4096 = 4kB

    public Peripheral(uint startAddress)
        : base(startAddress, 0x4000)
    {
    }

    public override ArraySegment<byte> Read(uint address, int count)
    {
        var adr = (address - StartAddress) % 0x1000;
        return new ArraySegment<byte>(memory, (int)adr, count);
    }

    public override void Write(uint address, byte[] data)
    {
        var adr = (address - StartAddress) % 0x1000;
        var type = (address - StartAddress) / 0x1000;
        switch (type)
        {
            case 0:
                // normal write
                data.CopyTo(memory, (int)adr);
                break;

            case 1:
                // Atomic XOR on write
                ApplyAtomic(adr, data, (uint dst, uint mask) => dst ^= mask);
                break;

            case 2:
                // Atomic bitmask SET (dst |= mask)
                ApplyAtomic(adr, data, (uint dst, uint mask) => dst |= mask);
                break;

            case 3:
                // Atomic bitmask CLEAR (dst &= ~mask)
                ApplyAtomic(adr, data, (uint dst, uint mask) => dst &= ~mask);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range.");
        }
    }

    private void ApplyAtomic(uint adr, byte[] data, Action<uint, uint> op)
    {
        // Each write is applied per 32-bit word.
        // A write of N bytes is interpreted as N/4 mask words.
        int baseOffset = (int)(adr & ~3u);

        for (int i = 0; i < data.Length; i += 4)
        {
            uint mask = BitConverter.ToUInt32(data, i);
            int dstOffset = baseOffset + i;

            uint current = BitConverter.ToUInt32(memory, dstOffset);
            op(current, mask);
            Array.Copy(BitConverter.GetBytes(current), 0, memory, dstOffset, 4);
        }
    }
}
