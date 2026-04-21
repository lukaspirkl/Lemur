using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;
using Microsoft.Extensions.Logging;
using Tests;
using Lemur;

namespace Tests;

public record MemoryWriteArgs(uint Address, byte[] Data);

public class ElfMemory : IAddressableResource
{
    private readonly string m_Name;
    private readonly bool m_IsReadonly;
    private readonly ILogger<Memory> m_Logger;
    private readonly byte[] m_Memory;

    public uint BaseAddress { get; }
    public uint Size { get; }

    public event EventHandler<MemoryWriteArgs>? OnWrite;

    public Dictionary<string, uint> Symbols { get; set; } = new Dictionary<string, uint>();

    public ElfMemory(string name, uint startAddress, uint size, bool isReadonly, ILogger<Memory> logger)
    {
        m_Name = name;
        m_Memory = new byte[size];
        Array.Fill<byte>(m_Memory, 0xFF);

        BaseAddress = startAddress;
        Size = size;
        m_IsReadonly = isReadonly;
        m_Logger = logger;
    }

    public void Write(uint address, byte[] data)
    {
        if (m_IsReadonly)
        {
            m_Logger.LogError("Writing to read only memory {name} address {address} value {value}", m_Name, address.ToHex(), data.ToHex());
            return;
        }

        m_Logger.LogInformation("mem[{address}] <- {data}", address.ToHex(), data.ToHex());

        OnWrite?.Invoke(this, new MemoryWriteArgs(address, data));

        data.CopyTo(m_Memory, (int)(address - BaseAddress));
    }

    public byte[] Read(uint address, int count)
    {
        return m_Memory.Skip((int)(address - BaseAddress)).Take(count).ToArray();
    }

    public void LoadElf(string path)
    {
        m_Logger.LogInformation("Loading {path}", path);

        var elf = ELFReader.Load(path);

        Symbols = ((ISymbolTable)elf.GetSection(".symtab")).Entries.OfType<SymbolEntry<uint>>().GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First().Value);

        var loadableSegments = elf.Segments.OfType<Segment<UInt32>>().Where(x => x.Type == SegmentType.Load);
        foreach (var segment in loadableSegments)
        {
            m_Logger.LogInformation("Processing segment at {address}...", segment.Address.ToHex());

            long flashOffset = segment.Address - BaseAddress;
            if (flashOffset >= 0 && (flashOffset + segment.Size) <= m_Memory.Length)
            {
                m_Logger.LogInformation("Segment is written to {name}", m_Name);
                byte[] segmentData = segment.GetMemoryContents();
                Array.Copy(segmentData, 0, m_Memory, flashOffset, segmentData.Length);
                continue;
            }

            m_Logger.LogWarning("Segment at {addres} (size {size}) is outside the defined flash memory range. Skipping.", segment.Address.ToHex(), segment.Size.ToHex());
        }
    }
}
