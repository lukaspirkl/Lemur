using ELFSharp.ELF;
using ELFSharp.ELF.Segments;

namespace Venture;

public class Emulator
{
    private const uint FLASH_BASE_ADDRESS = 0x80000000;
    private const int FLASH_SIZE = 1024 * 128; // 128 KiB

    private byte[] flashMemory = new byte[FLASH_SIZE];

    public void MemoryWrite(uint address, uint value)
    {
        BitConverter.GetBytes(value).CopyTo(flashMemory, (int)(address - FLASH_BASE_ADDRESS));
    }
    
    public uint MemoryRead(uint address)
    {
        return BitConverter.ToUInt32(new ArraySegment<byte>(flashMemory, (int)(address - FLASH_BASE_ADDRESS), 4));
    }

    public Emulator(string binPath)
    {
        if (!BitConverter.IsLittleEndian)
        {
            throw new InvalidOperationException("This platform is not little endian.");
        }

        var elf = ELFReader.Load(binPath);
        var loadableSegments = elf.Segments.OfType<Segment<UInt32>>().Where(x => x.Type == SegmentType.Load);
        foreach (var segment in loadableSegments)
        {
            Console.WriteLine($"Processing segment at 0x{segment.Address:X}...");
            long arrayOffset = (long)(segment.Address - FLASH_BASE_ADDRESS);
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

    public uint PC { get; set; } = FLASH_BASE_ADDRESS;

    uint[] registers = new uint[32];

    public void ExecuteInstruction()
    {
        Console.WriteLine($"PC: {PC.ToHex()}");
        var instruction = MemoryRead(PC);

        var opcode = ExtractBits(instruction, 0, 7);
        if (opcode == 0b1101111)
        {
            Console.WriteLine("jal");
            uint rd = (instruction >> 7) & 0x1F; // 0x1F is a mask for 5 bits (11111)
            uint imm_20 = (instruction >> 31) & 0x01; // Bit 31 -> imm[20]
            uint imm_10_1 = (instruction >> 21) & 0x3FF; // Bits 30-21 -> imm[10:1]
            uint imm_11 = (instruction >> 20) & 0x01; // Bit 20 -> imm[11]
            uint imm_19_12 = (instruction >> 12) & 0xFF;  // Bits 19-12 -> imm[19:12]
            uint imm_j = (imm_20 << 20)    // imm[20]
                       | (imm_19_12 << 12) // imm[19:12]
                       | (imm_11 << 11)    // imm[11]
                       | (imm_10_1 << 1);  // imm[10:1] (shifted left by 1 for imm[0]=0)

            int signed_imm_j = (int)imm_j;
            if (imm_20 == 1)
            {
                // Extend the sign bit from bit 20 upwards
                signed_imm_j |= unchecked((int)0xFFE00000); // Mask for bits 31 down to 21
            }

            uint returnAddress = PC + 4;

            if (rd != 0)
            {
                registers[rd] = returnAddress;
            }

            uint targetAddress = (uint)(PC + signed_imm_j);

            PC = targetAddress;
        }
        // else if (opcode == 0b0010011)
        // {

        // }
        else
        {
            Console.WriteLine(instruction.ToBin());
        }
    }

    public static uint ExtractBits(uint opcode, int startBit, int length)
    {
        // 1. Calculate the mask: (1 << length) - 1
        //    Example (length = 3): (1 << 3) - 1 = 8 - 1 = 7 (Binary 00...0111)
        uint mask = (1U << length) - 1;

        // 2. Right-shift to move the desired bits to the least significant position.
        //    Example (startBit = 12): Moves bits 14-12 to positions 2-0.
        uint shifted = opcode >> startBit;

        // 3. Bitwise AND with the mask to clear any higher bits that were shifted in.
        //    This isolates the required value.
        uint extractedValue = shifted & mask;

        return extractedValue;
    }

}
