## <span id="page-54-2"></span>**A.5. Privileged Instructions (including Zicsr)**

| Instruction         | Cycles | Note |  |  |
|---------------------|--------|------|--|--|
| CSR Access          |        |      |  |  |
| csrrw rd, csr, rs1  | 1      |      |  |  |
| csrrc rd, csr, rs1  | 1      |      |  |  |
| csrrs rd, csr, rs1  | 1      |      |  |  |
| csrrwi rd, csr, imm | 1      |      |  |  |
| csrrci rd, csr, imm | 1      |      |  |  |
| csrrsi rd, csr, imm | 1      |      |  |  |

| Instruction  | Cycles | Note                               |
|--------------|--------|------------------------------------|
| Trap Request |        |                                    |
| ecall        | 3      | Time given is for jumping to mtvec |
| ebreak       | 3      | Time given is for jumping to mtvec |

