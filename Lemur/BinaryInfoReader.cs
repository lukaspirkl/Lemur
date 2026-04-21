using Lemur;
using System.Collections.Generic;
using System.Text;

public class BinaryInfoReader
{
    const uint BINARY_INFO_MARKER_START = 0x7188ebf2;
    const uint BINARY_INFO_MARKER_END = 0xe71aa390;
    const uint FLASH_BASE = 0x10000000;

    // Types
    const int BINARY_INFO_TYPE_ID_AND_INT = 5;
    const int BINARY_INFO_TYPE_ID_AND_STRING = 6;
    const int BINARY_INFO_TYPE_PINS_WITH_FUNCTION = 8;
    const int BINARY_INFO_TYPE_PINS_WITH_NAME = 9;
    const int BINARY_INFO_TYPE_NAMED_GROUP = 10;

    // Well-known IDs
    const uint ID_PROGRAM_NAME         = 0x02031C86;
    const uint ID_PROGRAM_NAME_LEGACY  = 0x933010b4;
    const uint ID_PROGRAM_VERSION      = 0x5360B3AB;
    const uint ID_PROGRAM_VERSION_LEGACY = 0xd9866810;
    const uint ID_PROGRAM_URL          = 0x4a95632d;
    const uint ID_BUILD_DATE           = 0x9DA22254;
    const uint ID_BUILD_DATE_LEGACY    = 0x6442656d;
    const uint ID_BUILD_TYPE           = 0x4275F0D3;
    const uint ID_TARGET_BOARD         = 0xB63CFFBB;
    const uint ID_BOOT2_NAME           = 0x7F8882E1;

    /// <summary>
    /// Parses binary info metadata from flash memory.
    /// Returns null when no valid metadata header is found.
    /// </summary>
    public static BinaryInfo? ProcessBinary(IDebuggable system)
    {
        var headerOffset = FindHeader(system);
        if (!headerOffset.HasValue)
            return null;

        try
        {
            uint entriesStart = system.MemoryRead32(headerOffset.Value + 4);
            uint entriesEnd   = system.MemoryRead32(headerOffset.Value + 8);
            uint markerEnd    = system.MemoryRead32(headerOffset.Value + 16);

            if (markerEnd != BINARY_INFO_MARKER_END)
                return null;

            var builder = new Builder();

            for (uint i = entriesStart; i < entriesEnd; i += 4)
            {
                uint entryPtr = system.MemoryRead32(i);
                if (entryPtr == 0) continue;
                ReadEntry(system, entryPtr, builder);
            }

            return builder.Build();
        }
        catch
        {
            return null;
        }
    }

    static void ReadEntry(IDebuggable system, uint offset, Builder builder)
    {
        ushort type = system.MemoryRead16(offset);

        switch (type)
        {
            case BINARY_INFO_TYPE_PINS_WITH_NAME:
                ReadPinsWithName(system, offset, builder);
                break;
            case BINARY_INFO_TYPE_ID_AND_STRING:
                ReadIdAndString(system, offset, builder);
                break;
            case BINARY_INFO_TYPE_ID_AND_INT:
                // No int fields surfaced in BinaryInfo yet
                break;
        }
    }

    static void ReadPinsWithName(IDebuggable system, uint offset, Builder builder)
    {
        uint mask     = system.MemoryRead32(offset + 4);
        uint labelPtr = system.MemoryRead32(offset + 8);
        string label  = ReadString(system, labelPtr);
        var pins      = DecodeMask(mask);
        builder.Pins.Add(new PinInfo(pins, label));
    }

    static void ReadIdAndString(IDebuggable system, uint offset, Builder builder)
    {
        uint id     = system.MemoryRead32(offset + 4);
        uint valPtr = system.MemoryRead32(offset + 8);
        string val  = ReadString(system, valPtr);

        switch (id)
        {
            case ID_PROGRAM_NAME:         builder.ProgramName    ??= val; break;
            case ID_PROGRAM_NAME_LEGACY:  builder.ProgramName    ??= val; break;
            case ID_PROGRAM_VERSION:      builder.ProgramVersion ??= val; break;
            case ID_PROGRAM_VERSION_LEGACY: builder.ProgramVersion ??= val; break;
            case ID_PROGRAM_URL:          builder.ProgramUrl     ??= val; break;
            case ID_BUILD_DATE:           builder.BuildDate      ??= val; break;
            case ID_BUILD_DATE_LEGACY:    builder.BuildDate      ??= val; break;
            case ID_BUILD_TYPE:           builder.BuildType      ??= val; break;
            case ID_TARGET_BOARD:         builder.TargetBoard    ??= val; break;
            case ID_BOOT2_NAME:           builder.Boot2Name      ??= val; break;
        }
    }

    static uint? FindHeader(IDebuggable system)
    {
        uint limit = 4096;
        for (uint i = 0; i < limit; i += 4)
        {
            if (system.MemoryRead32(FLASH_BASE + i) == BINARY_INFO_MARKER_START)
            {
                uint markerEnd = system.MemoryRead32(FLASH_BASE + i + 16);
                if (markerEnd == BINARY_INFO_MARKER_END)
                    return FLASH_BASE + i;
            }
        }
        return null;
    }

    static List<int> DecodeMask(uint mask)
    {
        var pins = new List<int>();
        for (int i = 0; i < 32; i++)
            if ((mask & (1u << i)) != 0)
                pins.Add(i);
        return pins;
    }

    static string ReadString(IDebuggable system, uint offset)
    {
        uint end = offset;
        while (system.MemoryRead(end, 1)[0] != 0)
            end++;
        return Encoding.ASCII.GetString(system.MemoryRead(offset, (int)(end - offset)));
    }

    class Builder
    {
        public string? ProgramName;
        public string? ProgramVersion;
        public string? ProgramUrl;
        public string? BuildDate;
        public string? BuildType;
        public string? TargetBoard;
        public string? Boot2Name;
        public List<PinInfo> Pins = new();

        public BinaryInfo Build() => new(
            ProgramName, ProgramVersion, ProgramUrl,
            BuildDate, BuildType, TargetBoard, Boot2Name,
            Pins.AsReadOnly());
    }
}
