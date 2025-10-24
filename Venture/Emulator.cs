using ELFSharp.ELF;
using ELFSharp.ELF.Segments;
using System.Runtime.CompilerServices;
using Venture.InstructionFormats;

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

    private uint[] registers = new uint[32];

    private Dictionary<ushort, uint> csr = new Dictionary<ushort, uint>();

    public event EventHandler<ECallEventArgs> ECall;

    protected virtual void OnECall(ECallEventArgs e) 
    { 
        ECall?.Invoke(this, e);
    }

    public void ExecuteInstruction()
    {
        var instruction = MemoryRead(PC);
        Console.WriteLine($"PC: {PC.ToHex()} Instruction: {instruction.ToHex()} {instruction.ToBin()}");
        Console.WriteLine($"registers[10] {registers[10]}");
        
        var opcode = instruction.ExtractBits(0, 7);

        if (opcode == 0b1101111)
        {
            Console.WriteLine("J-Type: jal");
            var format = new JTypeInstructionFormat(instruction);

            uint returnAddress = PC + 4;

            if (format.rd != 0)
            {
                registers[format.rd] = returnAddress;
            }

            uint targetAddress = (uint)(PC + format.imm_j);

            PC = targetAddress;
            return;
        }

        if (opcode == 0b0010011)
        {
            var format = new ITypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                Console.WriteLine("I-Type: addi");
                if (format.rd != 0)
                {
                    registers[format.rd] = (uint)((int)registers[format.rs1] + format.imm_i_signed);
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b001)
            {
                Console.WriteLine("I-Type: slli");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] << (int)format.shamt_i;
                }
                PC = PC + 4;
                return;
            }

            throw new NotImplementedException($"I-Type with funct3 {format.funct3.ToBin(3)} not implemented. Instruction: {instruction.ToHex()}");
        }

        if (opcode == 0b1110011)
        {
            var format = new ITypeInstructionFormat(instruction);
            if (format.funct3 == 0b001)
            {
                Console.WriteLine("I-Type: csrrw");

                if (format.rd != 0)
                {
                    registers[format.rd] = csr[format.csr];
                }

                csr[format.csr] = registers[format.rs1];

                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b010)
            {
                Console.WriteLine("I-Type: csrrs");

                // TODO: Verify correctness
                uint original_csr_value = csr.GetValueOrDefault(format.csr, (uint)0);

                if (format.rs1 != 0)
                {
                    uint rs1_mask = registers[format.rs1];
                    uint new_csr_value = original_csr_value | rs1_mask;
                    csr[format.csr] = new_csr_value;
                }

                if (format.rd != 0)
                {
                    registers[format.rd] = original_csr_value;
                }

                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b101)
            {
                Console.WriteLine("I-Type: csrrwi");

                if (format.rd != 0)
                {
                    registers[format.rd] = csr[format.csr];
                }

                csr[format.csr] = (uint)format.imm_i_unsigned;

                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b000)
            {
                if (instruction == 0x30200073)
                {
                    Console.WriteLine("mret");
                    PC = csr[0x341];
                    return;
                };

                if (instruction == 0x00000073)
                {
                    Console.WriteLine($"ecall - service number: {registers[17]} argument: {registers[10]}");
                    OnECall(new ECallEventArgs(registers[17], registers[10]));
                    return;
                }

                throw new NotImplementedException($"I-Type with funct3 {format.funct3.ToBin(3)} not implemented. Instruction: {instruction.ToHex()}");
            }

            throw new NotImplementedException($"I-Type with funct3 {format.funct3.ToBin(3)} not implemented. Instruction: {instruction.ToHex()}");
        }

        if (opcode == 0b1100011)
        {
            var format = new BTypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                Console.WriteLine("B-Type: beq");
                if (registers[format.rs1] == registers[format.rs2])
                {
                    PC = (uint)((int)PC + format.imm_b);
                }
                else
                {
                    PC = PC + 4;
                }
                return;
            }
            if (format.funct3 == 0b001)
            {
                Console.WriteLine("B-Type: bne");
                if (registers[format.rs1] != registers[format.rs2])
                {
                    PC = (uint)((int)PC + format.imm_b);
                }
                else
                {
                    PC = PC + 4;
                }
                return;
            }
            if (format.funct3 == 0b100)
            {
                Console.WriteLine("B-Type: blt");
                if ((int)registers[format.rs1] < (int)registers[format.rs2])
                {
                    PC = (uint)((int)PC + format.imm_b);
                }
                else
                {
                    PC = PC + 4;
                }
                return;
            }

            throw new NotImplementedException($"B-Type with funct3 {format.funct3.ToBin(3)} not implemented. Instruction: {instruction.ToHex()}");
        }
        if (opcode == 0b0110011)
        {
            var format = new RTypeInstructionFormat(instruction);
            if (format.funct3 == 0b000 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("R-Type: add");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] + registers[format.rs2];
                }
                PC = PC + 4;
                return;
            }
        }
        
        if (opcode == 0b0010111)
        {
            Console.WriteLine("U-Type: auipc");
            var format = new UTypeInstructionFormat(instruction);
            if (format.rd != 0)
            {
                registers[format.rd] = PC + format.imm_u;
            }
            PC = PC + 4;
            return;
        }

        if (opcode == 0b0110111)
        {
            Console.WriteLine("U-Type: lui");
            var format = new UTypeInstructionFormat(instruction);
            if (format.rd != 0)
            {
                registers[format.rd] = format.imm_u;
            }
            PC = PC + 4;
            return;
        }
        
        if (instruction == 0x0ff0000f)
        {
            Console.WriteLine("fence");
            PC = PC + 4;
            return;
        }

        throw new NotImplementedException($"Opcode {opcode.ToBin(7)} not implemented. Instruction: {instruction.ToHex()}");
    }

   

}

public class ECallEventArgs : EventArgs
{
    public uint ServiceNumber { get; }
    public uint Argument { get; }

    public ECallEventArgs(uint serviceNumber, uint argument)
    {
        ServiceNumber = serviceNumber;
        Argument = argument;
    }
}