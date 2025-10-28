using Venture.InstructionFormats;

namespace Venture;

public static class RV32IInstructionSet
{
    public static void AddRV32I(this IEmulator e, Action<ECallParams>? OnECall = null)
    {
        e.AddInstructionSet(builder =>
        {
            builder.Opcode(0b0110111, UTypeInstructionFormat.Parse, "lui", format => e.Registers[format.rd] = format.imm_u);
            builder.Opcode(0b0010111, UTypeInstructionFormat.Parse, "auipc", format => e.Registers[format.rd] = e.PC + format.imm_u);

            builder.Opcode(0b1101111, JTypeInstructionFormat.Parse, "jal", f =>
            {
                e.Registers[f.rd] = e.PC + 4;
                e.PC = (uint)(e.PC + f.imm_j);
            });

            builder.Opcode(0b1100111, ITypeInstructionFormat.Parse)
                .Funct3(0b000, "jalr", f =>
                {
                    uint targetAddress = (uint)((int)e.Registers[f.rs1] + f.imm_i_signed);
                    e.Registers[f.rd] = e.PC + 4;
                    e.PC = targetAddress & 0b11111111_11111111_11111111_11111110;
                });

            builder.Opcode(0b1100011, BTypeInstructionFormat.Parse)
                .Funct3(0b000, "beq", format =>
                {
                    if(e.Registers[format.rs1] == e.Registers[format.rs2])
                    {
                        e.PC = (uint)((int)e.PC + format.imm_b);
                    }
                })
                .Funct3(0b001, "bne", format =>
                {
                    if (e.Registers[format.rs1] != e.Registers[format.rs2])
                    {
                        e.PC = (uint)((int)e.PC + format.imm_b);
                    }
                })
                .Funct3(0b100, "blt", format =>
                {
                    if ((int)e.Registers[format.rs1] < (int)e.Registers[format.rs2])
                    {
                        e.PC = (uint)((int)e.PC + format.imm_b);
                    }
                })
                .Funct3(0b101, "bge", format =>
                {
                    if ((int)e.Registers[format.rs1] >= (int)e.Registers[format.rs2])
                    {
                        e.PC = (uint)((int)e.PC + format.imm_b);
                    }
                })
                .Funct3(0b110, "bltu", format =>
                {
                    if (e.Registers[format.rs1] < e.Registers[format.rs2])
                    {
                        e.PC = (uint)((int)e.PC + format.imm_b);
                    }
                })
                .Funct3(0b111, "bgeu", format =>
                {
                    if (e.Registers[format.rs1] >= e.Registers[format.rs2])
                    {
                        e.PC = (uint)((int)e.PC + format.imm_b);
                    }
                });

            builder.Opcode(0b0000011, ITypeInstructionFormat.Parse)
                .Funct3(0b000, "lb", f => e.Registers[f.rd] = (uint)(sbyte)e.Memory.ReadByte((uint)(e.Registers[f.rs1] + f.imm_i_signed)))
                .Funct3(0b001, "lh", f => e.Registers[f.rd] = (uint)(short)e.Memory.ReadHalfWord((uint)(e.Registers[f.rs1] + f.imm_i_signed)))
                .Funct3(0b010, "lw", f => e.Registers[f.rd] = (uint)(int)e.Memory.ReadWord((uint)(e.Registers[f.rs1] + f.imm_i_signed)))
                .Funct3(0b100, "lbu", f => e.Registers[f.rd] = e.Memory.ReadByte((uint)(e.Registers[f.rs1] + f.imm_i_signed)))
                .Funct3(0b101, "lhu", f => e.Registers[f.rd] = e.Memory.ReadHalfWord((uint)(e.Registers[f.rs1] + f.imm_i_signed)));

            builder.Opcode(0b0100011, STypeInstructionFormat.Parse)
                .Funct3(0b000, "sb", f => e.Memory.WriteByte((uint)(e.Registers[f.rs1] + f.imm_s), BitConverter.GetBytes(e.Registers[f.rs2])[0]))
                .Funct3(0b001, "sh", f => e.Memory.Write((uint)(e.Registers[f.rs1] + f.imm_s), BitConverter.GetBytes(e.Registers[f.rs2]).Take(2).ToArray()))
                .Funct3(0b010, "sw", f => e.Memory.WriteWord((uint)(e.Registers[f.rs1] + f.imm_s), e.Registers[f.rs2]));

            builder.Opcode(0b0010011, ITypeInstructionFormat.Parse)
                .Funct3(0b000, "addi", format => e.Registers[format.rd] = (uint)((int)e.Registers[format.rs1] + format.imm_i_signed))
                .Funct3(0b010, "slti", format => e.Registers[format.rd] = (int)e.Registers[format.rs1] < format.imm_i_signed ? (uint)1 : 0)
                .Funct3(0b011, "sltiu", format => e.Registers[format.rd] = e.Registers[format.rs1] < (uint)format.imm_i_signed ? (uint)1 : 0)
                .Funct3(0b100, "xori", format => e.Registers[format.rd] = e.Registers[format.rs1] ^ (uint)format.imm_i_signed)
                .Funct3(0b110, "ori", format => e.Registers[format.rd] = e.Registers[format.rs1] | (uint)format.imm_i_signed)
                .Funct3(0b111, "andi", format => e.Registers[format.rd] = e.Registers[format.rs1] & (uint)format.imm_i_signed)
                .Funct3(0b001, "slli", format => e.Registers[format.rd] = e.Registers[format.rs1] << (int)format.shamt_i) // format.funct7 == 0b0000000
                .Funct3(0b101, "srli/srai", format =>
                {
                    // TODO: The funct7 should be something like funct3 (dictionary)
                    if (format.funct7 == 0b0000000)
                    {
                        e.Registers[format.rd] = e.Registers[format.rs1] >> (int)format.shamt_i;
                        return;
                    }
                    
                    if (format.funct7 == 0b0100000)
                    {
                        e.Registers[format.rd] = (uint)((int)e.Registers[format.rs1] >> (int)format.shamt_i);
                        return;
                    }

                    throw new NotImplementedException();
                });


            builder.Opcode(0b0110011, RTypeInstructionFormat.Parse)
                .Funct3(0b000, "add/sub", format =>
                {
                    // TODO: The funct7 should be something like funct3 (dictionary)
                    if (format.funct7 == 0b0000000)
                    {
                        e.Registers[format.rd] = e.Registers[format.rs1] + e.Registers[format.rs2];
                        return;
                    }
                    
                    if (format.funct7 == 0b0100000)
                    {
                        e.Registers[format.rd] = e.Registers[format.rs1] - e.Registers[format.rs2];
                        return;
                    }

                    throw new NotImplementedException();
                })
                .Funct3(0b001, "sll", format => e.Registers[format.rd] = e.Registers[format.rs1] << (int)e.Registers[format.rs2].ExtractBits(0, 5)) // format.funct7 == 0b0000000
                .Funct3(0b010, "slt", format => e.Registers[format.rd] = ((int)e.Registers[format.rs1] < (int)e.Registers[format.rs2]) ? (uint)1 : 0) // format.funct7 == 0b0000000
                .Funct3(0b011, "sltu", format => e.Registers[format.rd] = (e.Registers[format.rs1] < e.Registers[format.rs2]) ? (uint)1 : 0) // format.funct7 == 0b0000000
                .Funct3(0b100, "xor", format => e.Registers[format.rd] = e.Registers[format.rs1] ^ e.Registers[format.rs2]) // format.funct7 == 0b0000000
                .Funct3(0b101, "srl/sra", format =>
                {
                    // TODO: The funct7 should be something like funct3 (dictionary)
                    if (format.funct7 == 0b0000000)
                    {
                        e.Registers[format.rd] = e.Registers[format.rs1] >> (int)e.Registers[format.rs2].ExtractBits(0, 5);
                        return;
                    }
                    else if (format.funct7 == 0b0100000)
                    {
                        e.Registers[format.rd] = (uint)((int)e.Registers[format.rs1] >> (int)e.Registers[format.rs2].ExtractBits(0, 5));
                        return;
                    }

                    throw new NotImplementedException();
                })
                .Funct3(0b110, "or", format => e.Registers[format.rd] = e.Registers[format.rs1] | e.Registers[format.rs2]) // format.funct7 == 0b0000000
                .Funct3(0b111, "and", format => e.Registers[format.rd] = e.Registers[format.rs1] & e.Registers[format.rs2]) // format.funct7 == 0b0000000
                ;

                


            builder.Opcode(0b1110011, ITypeInstructionFormat.Parse)
                .Funct3(0b001, "csrrw", format =>
                {
                    e.Registers[format.rd] = e.CSR.GetValueOrDefault(format.csr, (uint)0);
                    e.CSR[format.csr] = e.Registers[format.rs1];
                })
                .Funct3(0b010, "csrrs", format =>
                {
                    uint original_csr_value = e.CSR.GetValueOrDefault(format.csr, (uint)0);

                    if (format.rs1 != 0)
                    {
                        uint rs1_mask = e.Registers[format.rs1];
                        uint new_csr_value = original_csr_value | rs1_mask;
                        e.CSR[format.csr] = new_csr_value;
                    }

                    e.Registers[format.rd] = original_csr_value;
                })
                .Funct3(0b101, "csrrwi", format =>
                {
                    e.Registers[format.rd] = e.CSR.GetValueOrDefault(format.csr, (uint)0);
                    e.CSR[format.csr] = (uint)format.imm_i_unsigned;
                })
                .Funct3(0b000, "e", format =>
                {
                    if (format.instruction == 0x30200073)
                    {
                        // mret
                        e.PC = e.CSR[0x341];
                        return;
                    }
                    ;

                    if (format.instruction == 0x00000073)
                    {
                        // ecall
                        Console.WriteLine($"ecall - service number: {e.Registers[17]} argument: {e.Registers[10]}");
                        OnECall?.Invoke(new ECallParams(e.Registers[17], e.Registers[10]));
                        return;
                    }
                });

            builder.Opcode(0b0001111, ITypeInstructionFormat.Parse, "fence", format =>
            {
                if (format.instruction == 0x0ff0000f)
                {
                    //Console.WriteLine("fence");
                    // For my single hart processor this is no-operation.
                    return;
                }

                if (format.instruction == 0x0000100f)
                {
                    //Console.WriteLine("fence.i");
                    // For my single hart processor this is no-operation.
                    return;
                }

                throw new NotImplementedException($"Unknown instruction {format.instruction.ToHex()} in opcode 0b0001111");
            });
        });
    }

    public record ECallParams(uint ServiceNumber, uint Argument);
}
