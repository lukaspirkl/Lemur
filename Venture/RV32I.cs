using Venture.InstructionFormats;

namespace Venture;

public static class RV32I
{
    public static IEmulator AddRW32I(this IEmulator e, Action<ECallParams>? OnECall = null)
    {
        e.Instructions.Add(0b1101111, instruction =>
        {
            //Console.WriteLine("J-Type: jal");
            var format = new JTypeInstructionFormat(instruction);
            e.Registers[format.rd] = e.PC + 4;
            e.PC = (uint)(e.PC + format.imm_j);
            return false;
        });

        e.Instructions.Add(0b1100111, instruction =>
        {
            //Console.WriteLine("I-Type: jalr");
            var format = new ITypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                uint targetAddress = (uint)((int)e.Registers[format.rs1] + format.imm_i_signed);
                e.Registers[format.rd] = e.PC + 4;
                e.PC = targetAddress & 0b11111111_11111111_11111111_11111110;
                return false;
            }

            throw new NotImplementedException($"Unknown funct3 {format.funct3.ToBin(3)} in opcode {format.opcode.ToBin(7)} (instruction: {instruction.ToHex()})");
        });

        e.Instructions.Add(0b0000011, instruction =>
        {
            var format = new ITypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                //Console.WriteLine("I-Type: lb");
                // Convert to sbyte and then to uint to have sign extension
                e.Registers[format.rd] = (uint)(sbyte)e.Memory.ReadByte((uint)(e.Registers[format.rs1] + format.imm_i_signed));
                return true;
            }
            if (format.funct3 == 0b001)
            {
                //Console.WriteLine("I-Type: lh");
                // Convert to short(signed) and then to uint to have sign extension
                e.Registers[format.rd] = (uint)(short)e.Memory.ReadHalfWord((uint)(e.Registers[format.rs1] + format.imm_i_signed));
                return true;
            }
            if (format.funct3 == 0b010)
            {
                //Console.WriteLine("I-Type: lw");
                // Convert to int and then to uint to have sign extension
                e.Registers[format.rd] = (uint)(int)e.Memory.ReadWord((uint)(e.Registers[format.rs1] + format.imm_i_signed));
                return true;
            }
            if (format.funct3 == 0b100)
            {
                //Console.WriteLine("I-Type: lbu");
                e.Registers[format.rd] = e.Memory.ReadByte((uint)(e.Registers[format.rs1] + format.imm_i_signed));
                return true;
            }
            if (format.funct3 == 0b101)
            {
                Console.WriteLine("I-Type: lhu");
                e.Registers[format.rd] = e.Memory.ReadHalfWord((uint)(e.Registers[format.rs1] + format.imm_i_signed));
                return true;
            }

            throw new NotImplementedException($"Unknown funct3 {format.funct3.ToBin(3)} in opcode {format.opcode.ToBin(7)} (instruction: {instruction.ToHex()})");
        });

        e.Instructions.Add(0b0100011, instruction =>
        {
            var format = new STypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                //Console.WriteLine("S-Type: sb");
                var bytes = BitConverter.GetBytes(e.Registers[format.rs2]);
                e.Memory.WriteByte((uint)(e.Registers[format.rs1] + format.imm_s), bytes[0]);
                return true;
            }
            if (format.funct3 == 0b001)
            {
                //Console.WriteLine("S-Type: sh");
                var bytes = BitConverter.GetBytes(e.Registers[format.rs2]).Take(2).ToArray();
                e.Memory.Write((uint)(e.Registers[format.rs1] + format.imm_s), bytes);
                return true;
            }
            if (format.funct3 == 0b010)
            {
                //Console.WriteLine("S-Type: sw");
                e.Memory.WriteWord((uint)(e.Registers[format.rs1] + format.imm_s), e.Registers[format.rs2]);
                return true;
            }

            throw new NotImplementedException($"Unknown funct3 {format.funct3.ToBin(3)} in opcode {format.opcode.ToBin(7)} (instruction: {instruction.ToHex()})");
        });

        e.Instructions.Add(0b0010011, instruction =>
        {
            var format = new ITypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                //Console.WriteLine("I-Type: addi");
                e.Registers[format.rd] = (uint)((int)e.Registers[format.rs1] + format.imm_i_signed);
                return true;
            }
            if (format.funct3 == 0b010)
            {
                //Console.WriteLine("I-Type: slti");
                e.Registers[format.rd] = (int)e.Registers[format.rs1] < format.imm_i_signed ? (uint)1 : 0;
                return true;
            }
            if (format.funct3 == 0b011)
            {
                //Console.WriteLine("I-Type: sltiu");
                e.Registers[format.rd] = e.Registers[format.rs1] < (uint)format.imm_i_signed ? (uint)1 : 0;
                return true;
            }
            if (format.funct3 == 0b001 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("I-Type: slli");
                e.Registers[format.rd] = e.Registers[format.rs1] << (int)format.shamt_i;
                return true;
            }
            if (format.funct3 == 0b101 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("I-Type: srli");
                e.Registers[format.rd] = e.Registers[format.rs1] >> (int)format.shamt_i;
                return true;
            }
            if (format.funct3 == 0b101 && format.funct7 == 0b0100000)
            {
                //Console.WriteLine("I-Type: srai");
                e.Registers[format.rd] = (uint)((int)e.Registers[format.rs1] >> (int)format.shamt_i);
                return true;
            }
            if (format.funct3 == 0b100)
            {
                //Console.WriteLine("I-Type: xori");
                e.Registers[format.rd] = e.Registers[format.rs1] ^ (uint)format.imm_i_signed;
                return true;
            }
            if (format.funct3 == 0b110)
            {
                //Console.WriteLine("I-Type: ori");
                e.Registers[format.rd] = e.Registers[format.rs1] | (uint)format.imm_i_signed;
                return true;
            }
            if (format.funct3 == 0b111)
            {
                //Console.WriteLine("I-Type: andi");
                e.Registers[format.rd] = e.Registers[format.rs1] & (uint)format.imm_i_signed;
                return true;
            }

            throw new NotImplementedException($"Unknown funct3 {format.funct3.ToBin(3)} in opcode {format.opcode.ToBin(7)} (instruction: {instruction.ToHex()})");
        });

        e.Instructions.Add(0b1110011, instruction =>
        {
            var format = new ITypeInstructionFormat(instruction);
            if (format.funct3 == 0b001)
            {
                //Console.WriteLine("I-Type: csrrw");
                e.Registers[format.rd] = e.CSR.GetValueOrDefault(format.csr, (uint)0);
                e.CSR[format.csr] = e.Registers[format.rs1];
                return true;
            }
            if (format.funct3 == 0b010)
            {
                //Console.WriteLine("I-Type: csrrs");

                uint original_csr_value = e.CSR.GetValueOrDefault(format.csr, (uint)0);

                if (format.rs1 != 0)
                {
                    uint rs1_mask = e.Registers[format.rs1];
                    uint new_csr_value = original_csr_value | rs1_mask;
                    e.CSR[format.csr] = new_csr_value;
                }

                e.Registers[format.rd] = original_csr_value;
                return true;
            }
            if (format.funct3 == 0b101)
            {
                //Console.WriteLine("I-Type: csrrwi");
                e.Registers[format.rd] = e.CSR.GetValueOrDefault(format.csr, (uint)0);
                e.CSR[format.csr] = (uint)format.imm_i_unsigned;
                return true;
            }
            if (format.funct3 == 0b000)
            {
                if (instruction == 0x30200073)
                {
                    //Console.WriteLine("mret");
                    e.PC = e.CSR[0x341];
                    return false;
                }
                ;

                if (instruction == 0x00000073)
                {
                    Console.WriteLine($"ecall - service number: {e.Registers[17]} argument: {e.Registers[10]}");
                    OnECall?.Invoke(new ECallParams(e.Registers[17], e.Registers[10]));
                    return true;
                }

                throw new NotImplementedException($"I-Type with funct3 {format.funct3.ToBin(3)} not implemented. Instruction: {instruction.ToHex()}");
            }

            throw new NotImplementedException($"Unknown funct3 {format.funct3.ToBin(3)} in opcode {format.opcode.ToBin(7)} (instruction: {instruction.ToHex()})");
        });

        e.Instructions.Add(0b1100011, instruction =>
        {
            var format = new BTypeInstructionFormat(instruction);
            if (format.funct3 == 0b000)
            {
                //Console.WriteLine("B-Type: beq");
                if (e.Registers[format.rs1] == e.Registers[format.rs2])
                {
                    e.PC = (uint)((int)e.PC + format.imm_b);
                    return false;
                }
                return true;
            }
            if (format.funct3 == 0b001)
            {
                //Console.WriteLine("B-Type: bne");
                if (e.Registers[format.rs1] != e.Registers[format.rs2])
                {
                    e.PC = (uint)((int)e.PC + format.imm_b);
                    return false;
                }
                return true;
            }
            if (format.funct3 == 0b100)
            {
                //Console.WriteLine("B-Type: blt");
                if ((int)e.Registers[format.rs1] < (int)e.Registers[format.rs2])
                {
                    e.PC = (uint)((int)e.PC + format.imm_b);
                    return false;
                }
                return true;
            }
            if (format.funct3 == 0b101)
            {
                //Console.WriteLine("B-Type: bge");
                if ((int)e.Registers[format.rs1] >= (int)e.Registers[format.rs2])
                {
                    e.PC = (uint)((int)e.PC + format.imm_b);
                    return false;
                }
                return true;
            }
            if (format.funct3 == 0b110)
            {
                //Console.WriteLine("B-Type: bltu");
                if (e.Registers[format.rs1] < e.Registers[format.rs2])
                {
                    e.PC = (uint)((int)e.PC + format.imm_b);
                    return false;
                }
                return true;
            }
            if (format.funct3 == 0b111)
            {
                //Console.WriteLine("B-Type: bgeu");
                if (e.Registers[format.rs1] >= e.Registers[format.rs2])
                {
                    e.PC = (uint)((int)e.PC + format.imm_b);
                    return false;
                }
                return true;
            }

            throw new NotImplementedException($"Unknown funct3 {format.funct3.ToBin(3)} in opcode {format.opcode.ToBin(7)} (instruction: {instruction.ToHex()})");
        });

        e.Instructions.Add(0b0110011, instruction =>
        {
            var format = new RTypeInstructionFormat(instruction);
            if (format.funct3 == 0b000 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("R-Type: add");
                e.Registers[format.rd] = e.Registers[format.rs1] + e.Registers[format.rs2];
                return true;
            }
            if (format.funct3 == 0b000 && format.funct7 == 0b0100000)
            {
                //Console.WriteLine("R-Type: sub");
                e.Registers[format.rd] = e.Registers[format.rs1] - e.Registers[format.rs2];
                return true;
            }
            if (format.funct3 == 0b001 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("R-Type: sll");
                e.Registers[format.rd] = e.Registers[format.rs1] << (int)e.Registers[format.rs2].ExtractBits(0, 5);
                return true;
            }
            if (format.funct3 == 0b010 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("R-Type: slt");
                e.Registers[format.rd] = ((int)e.Registers[format.rs1] < (int)e.Registers[format.rs2]) ? (uint)1 : 0;
                return true;
            }
            if (format.funct3 == 0b011 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("R-Type: sltu");
                e.Registers[format.rd] = (e.Registers[format.rs1] < e.Registers[format.rs2]) ? (uint)1 : 0;
                return true;
            }
            if (format.funct3 == 0b100 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("R-Type: xor");
                e.Registers[format.rd] = e.Registers[format.rs1] ^ e.Registers[format.rs2];
                return true;
            }
            if (format.funct3 == 0b101 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("R-Type: srl");
                e.Registers[format.rd] = e.Registers[format.rs1] >> (int)e.Registers[format.rs2].ExtractBits(0, 5);
                return true;
            }
            if (format.funct3 == 0b101 && format.funct7 == 0b0100000)
            {
                //Console.WriteLine("R-Type: sra");
                e.Registers[format.rd] = (uint)((int)e.Registers[format.rs1] >> (int)e.Registers[format.rs2].ExtractBits(0, 5));
                return true;
            }
            if (format.funct3 == 0b110 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("R-Type: or");
                e.Registers[format.rd] = e.Registers[format.rs1] | e.Registers[format.rs2];
                return true;
            }
            if (format.funct3 == 0b111 && format.funct7 == 0b0000000)
            {
                //Console.WriteLine("R-Type: and");
                e.Registers[format.rd] = e.Registers[format.rs1] & e.Registers[format.rs2];
                return true;
            }

            throw new NotImplementedException($"Unknown funct3 {format.funct3.ToBin(3)} funct7 {format.funct7.ToBin(7)} in opcode {format.opcode.ToBin(7)} (instruction: {instruction.ToHex()})");
        });

        e.Instructions.Add(0b0010111, instruction =>
        {
            //Console.WriteLine("U-Type: auipc");
            var format = new UTypeInstructionFormat(instruction);
            e.Registers[format.rd] = e.PC + format.imm_u;
            return true;
        });

        e.Instructions.Add(0b0110111, instruction =>
        {
            //Console.WriteLine("U-Type: lui");
            var format = new UTypeInstructionFormat(instruction);
            e.Registers[format.rd] = format.imm_u;
            return true;
        });

        e.Instructions.Add(0b0001111, instruction =>
        {
            if (instruction == 0x0ff0000f)
            {
                //Console.WriteLine("fence");
                // For my single hart processor this is no-operation.
                return true;
            }

            if (instruction == 0x0000100f)
            {
                //Console.WriteLine("fence.i");
                // For my single hart processor this is no-operation.
                return true;
            }

            throw new NotImplementedException($"Unknown instruction {instruction.ToHex()} in opcode 0b0001111");
        });

        return e;
    }

    public record ECallParams(uint ServiceNumber, uint Argument);
}
