using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;
using Microsoft.Extensions.Logging;

namespace Venture;

public record MemoryWriteArgs(uint Address, byte[] Data);

public class MemoryFactory
{
    private readonly ILogger<Memory> logger;

    public MemoryFactory(ILogger<Memory> logger)
    {
        this.logger = logger;
    }

    public Memory Create(string name, uint startAddress, uint size, bool isReadonly = false)
    {
        return new Memory(name, startAddress, size, isReadonly, logger);
    }
}

public class Memory : IAddressableResource
{
    private readonly string name;
    private readonly bool isReadonly;
    private readonly ILogger<Memory> logger;
    private readonly byte[] memory;

    public uint BaseAddress { get; }
    public uint Size { get; }

    public event EventHandler<MemoryWriteArgs>? OnWrite;

    public Dictionary<string, uint> Symbols { get; set; } = new Dictionary<string, uint>();

    public Memory(string name, uint startAddress, uint size, bool isReadonly, ILogger<Memory> logger)
    {
        this.name = name;
        memory = new byte[size];
        Array.Fill<byte>(memory, 0xFF);

        BaseAddress = startAddress;
        Size = size;
        this.isReadonly = isReadonly;
        this.logger = logger;
    }

    public void Write(uint address, byte[] data)
    {
        if (isReadonly)
        {
            logger.LogError("Writing to read only memory {name} address {address} value {value}", name, address.ToHex(), data.ToHex());
            return;
        }

        logger.LogInformation("mem[{address}] <- {data}", address.ToHex(), data.ToHex());

        OnWrite?.Invoke(this, new MemoryWriteArgs(address, data));

        data.CopyTo(memory, (int)(address - BaseAddress));
    }

    public byte[] Read(uint address, int count)
    {
        return memory.Skip((int)(address - BaseAddress)).Take(count).ToArray();
    }

    public void LoadElf(string path)
    {
        logger.LogInformation("Loading {path}", path);

        var elf = ELFReader.Load(path);

        Symbols = ((ISymbolTable)elf.GetSection(".symtab")).Entries.OfType<SymbolEntry<uint>>().GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First().Value);

        var loadableSegments = elf.Segments.OfType<Segment<UInt32>>().Where(x => x.Type == SegmentType.Load);
        foreach (var segment in loadableSegments)
        {
            logger.LogInformation("Processing segment at {address}...", segment.Address.ToHex());

            long flashOffset = segment.Address - BaseAddress;
            if (flashOffset >= 0 && (flashOffset + segment.Size) <= memory.Length)
            {
                logger.LogInformation("Segment is written to {name}", name);
                byte[] segmentData = segment.GetMemoryContents();
                Array.Copy(segmentData, 0, memory, flashOffset, segmentData.Length);
                continue;
            }

            logger.LogWarning("Segment at {addres} (size {size}) is outside the defined flash memory range. Skipping.", segment.Address.ToHex(), segment.Size.ToHex());
        }
    }

    public void LoadBin(string path)
    {
        var loaded = File.ReadAllBytes(path);
        loaded.CopyTo(memory, 0);
    }
}
