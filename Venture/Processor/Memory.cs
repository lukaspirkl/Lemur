using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;

namespace Venture.Processor;

public class Memory : IMemory
{
    private readonly uint flashStart;
    private byte[] flashMemory;

    private readonly uint ramStart;
    private byte[] ramMemory;

    public event EventHandler<MemoryWriteArgs>? OnWrite;

    public uint ToHostAddress { get; }

    public Dictionary<string, uint> Symbols { get; }

    public Memory(string binPath, uint flashStart, uint flashSize, uint ramStart = 0, uint ramSize = 0)
    {
        this.flashStart = flashStart;
        this.flashMemory = new byte[flashSize];

        this.ramStart = ramStart;
        this.ramMemory = new byte[ramSize];

        if (!BitConverter.IsLittleEndian)
        {
            throw new InvalidOperationException("This platform is not little endian.");
        }

        var elf = ELFReader.Load(binPath);

        Symbols = ((ISymbolTable)elf.GetSection(".symtab")).Entries.OfType<SymbolEntry<uint>>().GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First().Value);

        var loadableSegments = elf.Segments.OfType<Segment<UInt32>>().Where(x => x.Type == SegmentType.Load);
        foreach (var segment in loadableSegments)
        {
            Console.WriteLine($"Processing segment at 0x{segment.Address:X}...");
            
            long flashOffset = segment.Address - flashStart;
            if (flashOffset >= 0 && (flashOffset + segment.Size) <= flashMemory.Length)
            {
                Console.Write("Segment is written to flash");
                byte[] segmentData = segment.GetMemoryContents();
                Array.Copy(segmentData, 0, flashMemory, flashOffset, segmentData.Length);
                continue;
            }

            long ramOffset = segment.Address - ramStart;
            if (ramOffset >= 0 && (ramOffset + segment.Size) <= ramMemory.Length)
            {
                Console.Write("Segment is written to ram");
                byte[] segmentData = segment.GetMemoryContents();
                Array.Copy(segmentData, 0, ramMemory, ramOffset, segmentData.Length);
                continue;
            }

            Console.WriteLine($"Warning: Segment at 0x{segment.Address:X} (size 0x{segment.Size:X}) " +
                                $"is outside the defined flash memory range. Skipping.");
        }
    }

    public uint InitialPC => flashStart;

    public void Write(uint address, byte[] data)
    {
        Console.WriteLine($"mem[{address.ToHex()}] <- {data.ToHex()}");

        OnWrite?.Invoke(this, new MemoryWriteArgs(address, data));

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
