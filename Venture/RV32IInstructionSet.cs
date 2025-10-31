using Venture.InstructionFormats;

namespace Venture;

public static class RV32IInstructionSet
{
    public static void AddRV32I(this IEmulator e, Action<ECallParams>? OnECall = null)
    {
        var x = e.Registers;

        e.AddInstructionSet(builder =>
        {
            builder.Opcode(0b0110111, UTypeInstructionFormat.Parse, "lui", f => x[f.rd] = f.imm_u);
            builder.Opcode(0b0010111, UTypeInstructionFormat.Parse, "auipc", f => x[f.rd] = e.PC + f.imm_u);

            builder.Opcode(0b1101111, JTypeInstructionFormat.Parse, "jal", f =>
            {
                x[f.rd] = e.PC + 4;
                e.PC = (uint)(e.PC + f.imm_j);
            });

            builder.Opcode(0b1100111, ITypeInstructionFormat.Parse)
                .Funct3(0b000, "jalr", f =>
                {
                    uint targetAddress = (uint)((int)x[f.rs1] + f.imm_i_signed);
                    x[f.rd] = e.PC + 4;
                    e.PC = targetAddress & 0b11111111_11111111_11111111_11111110;
                });

            builder.Opcode(0b1100011, BTypeInstructionFormat.Parse)
                .Funct3(0b000, "beq", f =>
                {
                    if(x[f.rs1] == x[f.rs2])
                    {
                        e.PC = (uint)((int)e.PC + f.imm_b);
                    }
                })
                .Funct3(0b001, "bne", f =>
                {
                    if (x[f.rs1] != x[f.rs2])
                    {
                        e.PC = (uint)((int)e.PC + f.imm_b);
                    }
                })
                .Funct3(0b100, "blt", f =>
                {
                    if ((int)x[f.rs1] < (int)x[f.rs2])
                    {
                        e.PC = (uint)((int)e.PC + f.imm_b);
                    }
                })
                .Funct3(0b101, "bge", f =>
                {
                    if ((int)x[f.rs1] >= (int)x[f.rs2])
                    {
                        e.PC = (uint)((int)e.PC + f.imm_b);
                    }
                })
                .Funct3(0b110, "bltu", f =>
                {
                    if (x[f.rs1] < x[f.rs2])
                    {
                        e.PC = (uint)((int)e.PC + f.imm_b);
                    }
                })
                .Funct3(0b111, "bgeu", f =>
                {
                    if (x[f.rs1] >= x[f.rs2])
                    {
                        e.PC = (uint)((int)e.PC + f.imm_b);
                    }
                });

            builder.Opcode(0b0000011, ITypeInstructionFormat.Parse)
                .Funct3(0b000, "lb", f => x[f.rd] = (uint)(sbyte)e.Memory.ReadByte((uint)(x[f.rs1] + f.imm_i_signed)))
                .Funct3(0b001, "lh", f => x[f.rd] = (uint)(short)e.Memory.ReadHalfWord((uint)(x[f.rs1] + f.imm_i_signed)))
                .Funct3(0b010, "lw", f => x[f.rd] = (uint)(int)e.Memory.ReadWord((uint)(x[f.rs1] + f.imm_i_signed)))
                .Funct3(0b100, "lbu", f => x[f.rd] = e.Memory.ReadByte((uint)(x[f.rs1] + f.imm_i_signed)))
                .Funct3(0b101, "lhu", f => x[f.rd] = e.Memory.ReadHalfWord((uint)(x[f.rs1] + f.imm_i_signed)));

            builder.Opcode(0b0100011, STypeInstructionFormat.Parse)
                .Funct3(0b000, "sb", f => e.Memory.WriteByte((uint)(x[f.rs1] + f.imm_s), BitConverter.GetBytes(x[f.rs2])[0]))
                .Funct3(0b001, "sh", f => e.Memory.Write((uint)(x[f.rs1] + f.imm_s), BitConverter.GetBytes(x[f.rs2]).Take(2).ToArray()))
                .Funct3(0b010, "sw", f => e.Memory.WriteWord((uint)(x[f.rs1] + f.imm_s), x[f.rs2]));

            builder.Opcode(0b0010011, ITypeInstructionFormat.Parse)
                .Funct3(0b000, "addi", f => x[f.rd] = (uint)((int)x[f.rs1] + f.imm_i_signed))
                .Funct3(0b010, "slti", f => x[f.rd] = (int)x[f.rs1] < f.imm_i_signed ? (uint)1 : 0)
                .Funct3(0b011, "sltiu", f => x[f.rd] = x[f.rs1] < (uint)f.imm_i_signed ? (uint)1 : 0)
                .Funct3(0b100, "xori", f => x[f.rd] = x[f.rs1] ^ (uint)f.imm_i_signed)
                .Funct3(0b110, "ori", f => x[f.rd] = x[f.rs1] | (uint)f.imm_i_signed)
                .Funct3(0b111, "andi", f => x[f.rd] = x[f.rs1] & (uint)f.imm_i_signed)
                .Funct3(0b001, b => b.Funct7(0b0000000, "slli", f => x[f.rd] = x[f.rs1] << (int)f.shamt_i))
                .Funct3(0b101, b => b.Funct7(0b0000000, "srli", f => x[f.rd] = x[f.rs1] >> (int)f.shamt_i))
                .Funct3(0b101, b => b.Funct7(0b0100000, "srai", f => x[f.rd] = (uint)((int)x[f.rs1] >> (int)f.shamt_i)));

            builder.Opcode(0b0110011, RTypeInstructionFormat.Parse)
                .Funct3(0b000, b => b.Funct7(0b0000000, "add", f => x[f.rd] = x[f.rs1] + x[f.rs2]))
                .Funct3(0b000, b => b.Funct7(0b0100000, "add/sub", f => x[f.rd] = x[f.rs1] - x[f.rs2]))
                .Funct3(0b001, b => b.Funct7(0b0000000, "sll", f => x[f.rd] = x[f.rs1] << (int)x[f.rs2].ExtractBits(0, 5)))
                .Funct3(0b010, b => b.Funct7(0b0000000, "slt", f => x[f.rd] = ((int)x[f.rs1] < (int)x[f.rs2]) ? (uint)1 : 0))
                .Funct3(0b011, b => b.Funct7(0b0000000, "sltu", f => x[f.rd] = (x[f.rs1] < x[f.rs2]) ? (uint)1 : 0))
                .Funct3(0b100, b => b.Funct7(0b0000000, "xor", f => x[f.rd] = x[f.rs1] ^ x[f.rs2]))
                .Funct3(0b101, b => b.Funct7(0b0000000, "srl", f => x[f.rd] = x[f.rs1] >> (int)x[f.rs2].ExtractBits(0, 5)))
                .Funct3(0b101, b => b.Funct7(0b0100000, "sra", f => x[f.rd] = (uint)((int)x[f.rs1] >> (int)x[f.rs2].ExtractBits(0, 5))))
                .Funct3(0b110, b => b.Funct7(0b0000000, "or", f => x[f.rd] = x[f.rs1] | x[f.rs2]))
                .Funct3(0b111, b => b.Funct7(0b0000000, "and", f => x[f.rd] = x[f.rs1] & x[f.rs2]));

            builder.Opcode(0b1110011, ITypeInstructionFormat.Parse)
                .Funct3(0b001, "csrrw", f =>
                {
                    x[f.rd] = e.CSR.GetValueOrDefault(f.csr, (uint)0);
                    e.CSR[f.csr] = x[f.rs1];
                })
                .Funct3(0b010, "csrrs", f =>
                {
                    uint original_csr_value = e.CSR.GetValueOrDefault(f.csr, (uint)0);

                    if (f.rs1 != 0)
                    {
                        uint rs1_mask = x[f.rs1];
                        uint new_csr_value = original_csr_value | rs1_mask;
                        e.CSR[f.csr] = new_csr_value;
                    }

                    x[f.rd] = original_csr_value;
                })
                .Funct3(0b101, "csrrwi", f =>
                {
                    x[f.rd] = e.CSR.GetValueOrDefault(f.csr, (uint)0);
                    e.CSR[f.csr] = (uint)f.imm_i_unsigned;
                })
                .Funct3(0b000, "e", f =>
                {
                    if (f.instruction == 0x30200073)
                    {
                        // mret
                        e.PC = e.CSR[0x341];
                        return;
                    }
                    ;

                    if (f.instruction == 0x00000073)
                    {
                        // ecall
                        Console.WriteLine($"ecall - service number: {x[17]} argument: {x[10]}");
                        OnECall?.Invoke(new ECallParams(x[17], x[10]));
                        return;
                    }
                });

            builder.Opcode(0b0001111, ITypeInstructionFormat.Parse, "fence", f =>
            {
                if (f.instruction == 0x0ff0000f)
                {
                    //Console.WriteLine("fence");
                    // For my single hart processor this is no-operation.
                    return;
                }

                if (f.instruction == 0x0000100f)
                {
                    //Console.WriteLine("fence.i");
                    // For my single hart processor this is no-operation.
                    return;
                }

                throw new NotImplementedException($"Unknown instruction {f.instruction.ToHex()} in opcode 0b0001111");
            });
        });
    }

    public record ECallParams(uint ServiceNumber, uint Argument);
}
