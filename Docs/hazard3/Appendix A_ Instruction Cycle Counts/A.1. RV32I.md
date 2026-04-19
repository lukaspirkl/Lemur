## <span id="page-52-1"></span> A.1. RV32I

<span id="page-52-2"></span>

| Instruction                | Cycles   | Note                                  |  |  |  |
|----------------------------|----------|---------------------------------------|--|--|--|
| Integer Register-register  |          |                                       |  |  |  |
| add rd, rs1, rs2           | 1        |                                       |  |  |  |
| sub rd, rs1, rs2           | 1        |                                       |  |  |  |
| slt rd, rs1, rs2           | 1        |                                       |  |  |  |
| sltu rd, rs1, rs2          | 1        |                                       |  |  |  |
| and rd, rs1, rs2           | 1        |                                       |  |  |  |
| or rd, rs1, rs2            | 1        |                                       |  |  |  |
| xor rd, rs1, rs2           | 1        |                                       |  |  |  |
| sll rd, rs1, rs2           | 1        |                                       |  |  |  |
| srl rd, rs1, rs2           | 1        |                                       |  |  |  |
| sra rd, rs1, rs2           | 1        |                                       |  |  |  |
| Integer Register-immediate |          |                                       |  |  |  |
| addi rd, rs1, imm          | 1        | nop is a pseudo-op for addi x0, x0, 0 |  |  |  |
| slti rd, rs1, imm          | 1        |                                       |  |  |  |
| sltiu rd, rs1, imm         | 1        |                                       |  |  |  |
| andi rd, rs1, imm          | 1        |                                       |  |  |  |
| ori rd, rs1, imm           | 1        |                                       |  |  |  |
| xori rd, rs1, imm          | 1        |                                       |  |  |  |
| slli rd, rs1, imm          | 1        |                                       |  |  |  |
| srli rd, rs1, imm          | 1        |                                       |  |  |  |
| srai rd, rs1, imm          | 1        |                                       |  |  |  |
| Large Immediate            |          |                                       |  |  |  |
| lui rd, imm                | 1        |                                       |  |  |  |
| auipc rd, imm              | 1        |                                       |  |  |  |
| Control Transfer           |          |                                       |  |  |  |
| jal rd, label              | [1]<br>2 |                                       |  |  |  |
| jalr rd, rs1, imm          | [1]<br>2 |                                       |  |  |  |

| Instruction          | Cycles    | Note                                                     |  |
|----------------------|-----------|----------------------------------------------------------|--|
| beq rs1, rs2, label  | 1 or 2[1] | 1 if correctly predicted, 2 if mispredicted.             |  |
| bne rs1, rs2, label  | 1 or 2[1] | 1 if correctly predicted, 2 if mispredicted.             |  |
| blt rs1, rs2, label  | 1 or 2[1] | 1 if correctly predicted, 2 if mispredicted.             |  |
| bge rs1, rs2, label  | 1 or 2[1] | 1 if correctly predicted, 2 if mispredicted.             |  |
| bltu rs1, rs2, label | 1 or 2[1] | 1 if correctly predicted, 2 if mispredicted.             |  |
| bgeu rs1, rs2, label | 1 or 2[1] | 1 if correctly predicted, 2 if mispredicted.             |  |
| Load and Store       |           |                                                          |  |
| lw rd, imm(rs1)      | 1 or 2    | 1 if next instruction is independent, 2 if dependent.[2] |  |
| lh rd, imm(rs1)      | 1 or 2    | 1 if next instruction is independent, 2 if dependent.[2] |  |
| lhu rd, imm(rs1)     | 1 or 2    | 1 if next instruction is independent, 2 if dependent.[2] |  |
| lb rd, imm(rs1)      | 1 or 2    | 1 if next instruction is independent, 2 if dependent.[2] |  |
| lbu rd, imm(rs1)     | 1 or 2    | 1 if next instruction is independent, 2 if dependent.[2] |  |
| sw rs2, imm(rs1)     | 1         |                                                          |  |
| sh rs2, imm(rs1)     | 1         |                                                          |  |
| sb rs2, imm(rs1)     | 1         |                                                          |  |

