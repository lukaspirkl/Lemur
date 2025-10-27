using Venture.InstructionFormats;

namespace Venture;

public class Emulator
{
    private readonly IMemory memory;

    public Emulator(IMemory memory)
    {
        this.memory = memory;
        PC = memory.InitialPC;
    }

    public uint PC { get; set; }

    private uint[] registers = new uint[32];

    private Dictionary<ushort, uint> csr = new Dictionary<ushort, uint>();

    public event EventHandler<ECallEventArgs>? ECall;

    protected virtual void OnECall(ECallEventArgs e) 
    { 
        ECall?.Invoke(this, e);
    }

    public void ExecuteInstruction()
    {
        var instruction = memory.ReadWord(PC);
        Console.WriteLine($"PC: {PC.ToHex()} Instruction: {instruction.ToHex()} {instruction.ToBin()}");
        //Console.WriteLine($"registers[5] {registers[5].ToHex()}");
        
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

        if (opcode == 0b1100111)
        {
            Console.WriteLine("I-Type: jalr");
            var format = new ITypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                uint targetAddress = (uint)((int)registers[format.rs1] + format.imm_i_signed);

                if (format.rd != 0)
                {
                    registers[format.rd] = PC + 4;
                }

                PC = targetAddress & 0b11111111_11111111_11111111_11111110;
                return;
            }
        }

        if (opcode == 0b0000011)
        {
            var format = new ITypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                Console.WriteLine("I-Type: lb");
                if (format.rd != 0)
                {
                    // Convert to sbyte and then to uint to have sign extension
                    registers[format.rd] = (uint)(sbyte)memory.ReadByte((uint)(registers[format.rs1] + format.imm_i_signed));
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b001)
            {
                Console.WriteLine("I-Type: lh");
                if (format.rd != 0)
                {
                    // Convert to short(signed) and then to uint to have sign extension
                    registers[format.rd] = (uint)(short)memory.ReadHalfWord((uint)(registers[format.rs1] + format.imm_i_signed));
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b010)
            {
                Console.WriteLine("I-Type: lw");
                if (format.rd != 0)
                {
                    // Convert to int and then to uint to have sign extension
                    registers[format.rd] = (uint)(int)memory.ReadWord((uint)(registers[format.rs1] + format.imm_i_signed));
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b100)
            {
                Console.WriteLine("I-Type: lbu");
                if (format.rd != 0)
                {
                    registers[format.rd] = memory.ReadByte((uint)(registers[format.rs1] + format.imm_i_signed));
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b101)
            {
                Console.WriteLine("I-Type: lhu");
                if (format.rd != 0)
                {
                    registers[format.rd] = memory.ReadHalfWord((uint)(registers[format.rs1] + format.imm_i_signed));
                }
                PC = PC + 4;
                return;
            }
        }

        if (opcode == 0b0100011)
        {
            var format = new STypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                Console.WriteLine("S-Type: sb");
                var bytes = BitConverter.GetBytes(registers[format.rs2]);
                memory.WriteByte((uint)(registers[format.rs1] + format.imm_s), bytes[0]);
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b001)
            {
                Console.WriteLine("S-Type: sh");
                var bytes = BitConverter.GetBytes(registers[format.rs2]).Take(2).ToArray();
                memory.Write((uint)(registers[format.rs1] + format.imm_s), bytes);
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b010)
            {
                Console.WriteLine("S-Type: sw");
                memory.WriteWord((uint)(registers[format.rs1] + format.imm_s), registers[format.rs2]);
                PC = PC + 4;
                return;
            }
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
            if (format.funct3 == 0b010)
            {
                Console.WriteLine("I-Type: slti");
                if (format.rd != 0)
                {
                    registers[format.rd] = (int)registers[format.rs1] < format.imm_i_signed ? (uint)1 : 0;
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b011)
            {
                Console.WriteLine("I-Type: sltiu");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] < (uint)format.imm_i_signed ? (uint)1 : 0;
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b001 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("I-Type: slli");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] << (int)format.shamt_i;
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b101 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("I-Type: srli");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] >> (int)format.shamt_i;
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b101 && format.funct7 == 0b0100000)
            {
                Console.WriteLine("I-Type: srai");
                if (format.rd != 0)
                {
                    registers[format.rd] = (uint)((int)registers[format.rs1] >> (int)format.shamt_i);
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b100)
            {
                Console.WriteLine("I-Type: xori");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] ^ (uint)format.imm_i_signed;
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b110)
            {
                Console.WriteLine("I-Type: ori");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] | (uint)format.imm_i_signed;
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b111)
            {
                Console.WriteLine("I-Type: andi");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] & (uint)format.imm_i_signed;
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
            if (format.funct3 == 0b101)
            {
                Console.WriteLine("B-Type: bge");
                if ((int)registers[format.rs1] >= (int)registers[format.rs2])
                {
                    PC = (uint)((int)PC + format.imm_b);
                }
                else
                {
                    PC = PC + 4;
                }
                return;
            }
            if (format.funct3 == 0b110)
            {
                Console.WriteLine("B-Type: bltu");
                if (registers[format.rs1] < registers[format.rs2])
                {
                    PC = (uint)((int)PC + format.imm_b);
                }
                else
                {
                    PC = PC + 4;
                }
                return;
            }
            if (format.funct3 == 0b111)
            {
                Console.WriteLine("B-Type: bgeu");
                if (registers[format.rs1] >= registers[format.rs2])
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
            if (format.funct3 == 0b000 && format.funct7 == 0b0100000)
            {
                Console.WriteLine("R-Type: sub");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] - registers[format.rs2];
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b001 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("R-Type: sll");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] << (int)registers[format.rs2].ExtractBits(0, 5);
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b010 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("R-Type: slt");
                if (format.rd != 0)
                {
                    registers[format.rd] = ((int)registers[format.rs1] < (int)registers[format.rs2]) ? (uint)1 : 0;
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b011 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("R-Type: sltu");
                if (format.rd != 0)
                {
                    registers[format.rd] = (registers[format.rs1] < registers[format.rs2]) ? (uint)1 : 0;
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b100 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("R-Type: xor");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] ^ registers[format.rs2];
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b101 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("R-Type: srl");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] >> (int)registers[format.rs2].ExtractBits(0, 5);
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b101 && format.funct7 == 0b0100000)
            {
                Console.WriteLine("R-Type: sra");
                if (format.rd != 0)
                {
                    registers[format.rd] = (uint)((int)registers[format.rs1] >> (int)registers[format.rs2].ExtractBits(0, 5));
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b110 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("R-Type: or");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] | registers[format.rs2];
                }
                PC = PC + 4;
                return;
            }
            if (format.funct3 == 0b111 && format.funct7 == 0b0000000)
            {
                Console.WriteLine("R-Type: and");
                if (format.rd != 0)
                {
                    registers[format.rd] = registers[format.rs1] & registers[format.rs2];
                }
                PC = PC + 4;
                return;
            }

            throw new NotImplementedException($"R-Type with funct3 {format.funct3.ToBin(3)} finct7 {format.funct7.ToBin(7)} not implemented. Instruction: {instruction.ToHex()}");
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
            // For my single hart processor this is no-operation.
            PC = PC + 4;
            return;
        }

        if (instruction == 0x0000100f)
        {
            Console.WriteLine("fence.i");
            // For my single hart processor this is no-operation.
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