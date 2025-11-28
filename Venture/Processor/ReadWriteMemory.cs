using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;


namespace Venture.Processor;

public class ReadWriteMemory : ReadWriteMemoryBase
{
    private readonly string name;
    private readonly byte[] memory;

    public event EventHandler<MemoryWriteArgs>? OnWrite;

    public Dictionary<string, uint> Symbols { get; set; } = new Dictionary<string, uint>();

    public ReadWriteMemory(string name, uint startAddress, uint size)
        : base(startAddress, size)
    {
        this.name = name;
        memory = new byte[size];
    }

    public override void Write(uint address, byte[] data)
    {
        //Console.WriteLine($"mem[{address.ToHex()}] <- {data.ToHex()}");

        OnWrite?.Invoke(this, new MemoryWriteArgs(address, data));

        data.CopyTo(memory, (int)(address - StartAddress));
    }

    public override ArraySegment<byte> Read(uint address, int count)
    {
        return new ArraySegment<byte>(memory, (int)(address - StartAddress), count);
    }

    public void LoadElf(string path)
    {
        Console.WriteLine($"Loading {path}");

        var elf = ELFReader.Load(path);

        Symbols = ((ISymbolTable)elf.GetSection(".symtab")).Entries.OfType<SymbolEntry<uint>>().GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First().Value);

        var loadableSegments = elf.Segments.OfType<Segment<UInt32>>().Where(x => x.Type == SegmentType.Load);
        foreach (var segment in loadableSegments)
        {
            Console.WriteLine($"Processing segment at 0x{segment.Address:X}...");

            long flashOffset = segment.Address - StartAddress;
            if (flashOffset >= 0 && (flashOffset + segment.Size) <= memory.Length)
            {
                Console.WriteLine($"Segment is written to {name}");
                byte[] segmentData = segment.GetMemoryContents();
                Array.Copy(segmentData, 0, memory, flashOffset, segmentData.Length);
                continue;
            }

            Console.WriteLine($"Warning: Segment at 0x{segment.Address:X} (size 0x{segment.Size:X}) " +
                                $"is outside the defined flash memory range. Skipping.");
        }
    }

    public void LoadBin(string path)
    {
        var loaded = File.ReadAllBytes(path);
        loaded.CopyTo(memory, 0);
    }
}
