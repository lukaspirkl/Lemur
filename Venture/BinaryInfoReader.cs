using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Text;
using Venture;

public class BinaryInfoReader
{
    const uint BINARY_INFO_MARKER_START = 0x7188ebf2;
    const uint BINARY_INFO_MARKER_END = 0xe71aa390;
    const uint FLASH_BASE = 0x10000000;

    // Types
    const int BINARY_INFO_TYPE_RAW_DATA = 1;
    const int BINARY_INFO_TYPE_SIZED_DATA = 2;
    const int BINARY_INFO_TYPE_BINARY_INFO_LIST_ZERO_TERMINATED = 3;
    const int BINARY_INFO_TYPE_BSON = 4;
    const int BINARY_INFO_TYPE_ID_AND_INT = 5;
    const int BINARY_INFO_TYPE_ID_AND_STRING = 6;
    const int BINARY_INFO_TYPE_PINS_WITH_FUNCTION = 8;
    const int BINARY_INFO_TYPE_PINS_WITH_NAME = 9;
    const int BINARY_INFO_TYPE_NAMED_GROUP = 10;
    const int BINARY_INFO_TYPE_BLOCK_DEVICE = 11;

    //static void Main(string[] args)
    //{
    //    if (args.Length == 0)
    //    {
    //        Console.WriteLine("Usage: dotnet run <bin_file>");
    //        return;
    //    }

    //    string filePath = args[0];
    //    if (!File.Exists(filePath))
    //    {
    //        Console.WriteLine($"Error: File '{filePath}' not found.");
    //        return;
    //    }

    //    byte[] binary = File.ReadAllBytes(filePath);
    //    ProcessBinary(binary);
    //}

    public static void ProcessBinary(IDebuggable system)
    {
        // 1. Find Header
        var headerOffset = FindHeader(system);
        if (!headerOffset.HasValue)
        {
            Console.WriteLine("Error: Could not find BINARY_INFO_MARKER_START.");
            return;
        }

        Console.WriteLine($"Found Binary Info Header at offset: 0x{headerOffset:X}");

        // 2. Read Header
        // Layout: marker_start, entries_start, entries_end, mapping_table, marker_end
        try
        {
            uint markerStart = system.MemoryRead32(headerOffset.Value);
            uint entriesStart = system.MemoryRead32(headerOffset.Value + 4);
            uint entriesEnd = system.MemoryRead32(headerOffset.Value + 8);
            uint mappingTable = system.MemoryRead32(headerOffset.Value + 12);
            uint markerEnd = system.MemoryRead32(headerOffset.Value + 16);

            if (markerEnd != BINARY_INFO_MARKER_END)
            {
                Console.WriteLine($"Warning: Invalid marker_end: 0x{markerEnd:X}. Expected: 0x{BINARY_INFO_MARKER_END:X}");
            }

            Console.WriteLine($"Entries Start (VMA): 0x{entriesStart:X}");
            Console.WriteLine($"Entries End (VMA):   0x{entriesEnd:X}");

            // 3. Process Entries
            //int entriesOffset = (int)(entriesStart - FLASH_BASE);
            //int endOffset = (int)(entriesEnd - FLASH_BASE);

            //if (entriesOffset < 0 || entriesOffset >= binary.Length || endOffset < 0 || endOffset > binary.Length)
            //{
            //    Console.WriteLine("Error: Entries table out of file bounds.");
            //    return;
            //}

            for (uint i = entriesStart; i < entriesEnd; i += 4)
            {
                uint entryPtr = system.MemoryRead32(i);
                if (entryPtr == 0) continue; // Null pointer?

                //int entryOffset = (int)(entryPtr - FLASH_BASE);
                //if (entryOffset < 0 || entryOffset >= binary.Length)
                //{
                //    Console.WriteLine($"Warning: Entry pointer 0x{entryPtr:X} out of bounds.");
                //    continue;
                //}

                ReadEntry(system, entryPtr);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing binary: {ex.Message}");
        }
    }

    static void ReadEntry(IDebuggable system, uint offset)
    {
        // Header: Type (2 bytes), Tag (2 bytes)
        ushort type = system.MemoryRead16(offset);
        ushort tag = system.MemoryRead16(offset + 2);

        // Console.WriteLine($"Entry at 0x{offset:X}: Type={type}, Tag=0x{tag:X}");

        switch (type)
        {
            case BINARY_INFO_TYPE_PINS_WITH_NAME:
                ReadPinsWithName(system, offset);
                break;
            case BINARY_INFO_TYPE_PINS_WITH_FUNCTION:
                ReadPinsWithFunction(system, offset);
                break;
            case BINARY_INFO_TYPE_ID_AND_STRING:
                ReadIdAndString(system, offset);
                break;
            case BINARY_INFO_TYPE_ID_AND_INT:
                ReadIdAndInt(system, offset);
                break;
            case BINARY_INFO_TYPE_NAMED_GROUP:
                ReadNamedGroup(system, offset);
                break;
            default:
                // Console.WriteLine($"  Unknown Type: {type}");
                break;
        }
    }

    static void ReadPinsWithName(IDebuggable system, uint offset)
    {
        // struct: core, mask (4), label (4 ptr)
        uint mask = system.MemoryRead32(offset + 4);
        uint labelPtr = system.MemoryRead32(offset + 8);
        string label = ReadString(system, labelPtr);

        List<int> pins = DecodeMask(mask);
        string pinStr = string.Join(", ", pins);
        if (pins.Count > 1) pinStr = "[" + pinStr + "]";

        Console.WriteLine($"  [PinsWithName] Pin(s) {pinStr}: \"{label}\"");
    }

    static void ReadPinsWithFunction(IDebuggable system, uint offset)
    {
        // struct: core, mask (4), func (4)
        uint mask = system.MemoryRead32(offset + 4);
        uint func = system.MemoryRead32(offset + 8);

        List<int> pins = DecodeMask(mask);
        string pinStr = string.Join(", ", pins);
        if (pins.Count > 1) pinStr = "[" + pinStr + "]";

        Console.WriteLine($"  [PinsWithFunction] Pin(s) {pinStr}: Function {func}");
    }

    static void ReadIdAndString(IDebuggable system, uint offset)
    {
        // struct: core, id (4), value (4 ptr)
        uint id = system.MemoryRead32(offset + 4);
        uint valPtr = system.MemoryRead32(offset + 8);
        string val = ReadString(system, valPtr);

        // ID decoding
        string idStr = DecodeId(id);

        Console.WriteLine($"  [IdAndString] {idStr} (0x{id:X}): \"{val}\"");
    }

    static void ReadIdAndInt(IDebuggable system, uint offset)
    {
        // struct: core, id (4), value (4)
        uint id = system.MemoryRead32(offset + 4);
        int val = ReadInt32(system, offset + 8);

        string idStr = DecodeId(id);
        Console.WriteLine($"  [IdAndInt] {idStr} (0x{id:X}): {val}");
    }

    static void ReadNamedGroup(IDebuggable system, uint offset)
    {
        // struct: core, parent_id(4), flags(2), group_tag(2), label(4)
        uint parentId = system.MemoryRead32(offset + 4);
        ushort flags = system.MemoryRead16(offset + 8);
        ushort groupTag = system.MemoryRead16(offset + 10);
        uint labelPtr = system.MemoryRead32(offset + 12);
        string label = ReadString(system, labelPtr);

        Console.WriteLine($"  [NamedGroup] Parent: 0x{parentId:X}, Tag: 0x{groupTag:X}, Label: \"{label}\"");
    }

    static string DecodeId(uint id)
    {
        switch (id)
        {
            case 0x2031C86: return "Program Name";
            case 0x9DA22254: return "Build Date";
            case 0x5360B3AB: return "Program Version";
            case 0xB63CFFBB: return "Target Board";
            case 0x7F8882E1: return "Boot2 Name";
            case 0x4275F0D3: return "Build Type"; // "Debug"
            case 0x933010b4: return "Program Name (Legacy)";
            case 0xd9866810: return "Program Version (Legacy)";
            case 0x6442656d: return "Build Date (Legacy)";
            case 0x4a95632d: return "Program URL";
            default: return "ID";
        }
    }

    static List<int> DecodeMask(uint mask)
    {
        var pins = new List<int>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1u << i)) != 0)
            {
                pins.Add(i);
            }
        }
        return pins;
    }

    // ---

    static uint? FindHeader(IDebuggable system)
    {
        // Search first 4KB usually enough.
        uint limit = 4096;
        for (uint i = 0; i < limit; i += 4)
        {
            if (system.MemoryRead32(FLASH_BASE + i) == BINARY_INFO_MARKER_START)
            {
                // Verify end marker
                uint markerEnd = system.MemoryRead32(FLASH_BASE + i + 16);
                if (markerEnd == BINARY_INFO_MARKER_END)
                {
                    return FLASH_BASE + i;
                }
            }
        }
        return null;
    }

    static int ReadInt32(IDebuggable system, uint offset)
    {
        return BitConverter.ToInt32(system.MemoryRead(offset, 4));
    }

    static string ReadString(IDebuggable system, uint offset)
    {
        //if (offset < 0 || offset >= binary.Length) return "<invalid ptr>";
        uint end = offset;
        
        while (/*end < binary.Length &&*/ system.MemoryRead(end, 1)[0] != 0)
        {
            end++;
        }
        return Encoding.ASCII.GetString(system.MemoryRead(offset, (int)(end - offset)));
    }
}
