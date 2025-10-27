using ELFSharp.ELF;
using ELFSharp.ELF.Segments;

namespace Venture;

public class Memory : IMemory
{
    private readonly uint flashStart;
    private byte[] flashMemory;

    public Memory(string binPath, uint flashStart, uint flashSize)
    {
        this.flashStart = flashStart;
        this.flashMemory = new byte[flashSize];

        if (!BitConverter.IsLittleEndian)
        {
            throw new InvalidOperationException("This platform is not little endian.");
        }

        var elf = ELFReader.Load(binPath);
        var loadableSegments = elf.Segments.OfType<Segment<UInt32>>().Where(x => x.Type == SegmentType.Load);
        foreach (var segment in loadableSegments)
        {
            Console.WriteLine($"Processing segment at 0x{segment.Address:X}...");
            long arrayOffset = (long)(segment.Address - flashStart);
            if (arrayOffset < 0 || (arrayOffset + (long)segment.Size) > flashMemory.Length)
            {
                Console.WriteLine($"Warning: Segment at 0x{segment.Address:X} (size 0x{segment.Size:X}) " +
                                  $"is outside the defined flash memory range. Skipping.");
                continue;
            }

            byte[] segmentData = segment.GetMemoryContents();
            Array.Copy(segmentData, 0, flashMemory, arrayOffset, segmentData.Length);
        }
    }

    public uint InitialPC => flashStart;

    public void Write(uint address, byte[] data)
    {
        if (flashStart <= address && address < flashStart + flashMemory.Length)
        {
            data.CopyTo(flashMemory, (int)(address - flashStart));
            return;
        }

        throw new IndexOutOfRangeException($"Writing to invalid memory: {address.ToHex()}");
    }

    public ArraySegment<byte> Read(uint address, int count)
    {
        if (flashStart <= address && address < flashStart + flashMemory.Length)
        {
            return new ArraySegment<byte>(flashMemory, (int)(address - flashStart), count);
        }

        throw new IndexOutOfRangeException($"Reading invalid memory: {address.ToHex()}");
    }
}
