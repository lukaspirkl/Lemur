## <span id="page-55-0"></span>**A.6. Bit Manipulation**

| Cycles | Note                                         |
|--------|----------------------------------------------|
|        |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
|        |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      |                                              |
| 1      | zext.b is a pseudo-op for andi rd, rs1, 0xff |
|        |                                              |
| 1      |                                              |
| 1      |                                              |
|        |                                              |

| Instruction                                    | Cycles | Note |
|------------------------------------------------|--------|------|
| clmulr rd, rs1, rs2                            | 1      |      |
| Zbs (single-bit manipulation)                  |        |      |
| bclr rd, rs1, rs2                              | 1      |      |
| bclri rd, rs1, imm                             | 1      |      |
| bext rd, rs1, rs2                              | 1      |      |
| bexti rd, rs1, imm                             | 1      |      |
| binv rd, rs1, rs2                              | 1      |      |
| binvi rd, rs1, imm                             | 1      |      |
| bset rd, rs1, rs2                              | 1      |      |
| bseti rd, rs1, imm                             | 1      |      |
| Zbkb (basic bit manipulation for cryptography) |        |      |
| pack rd, rs1, rs2                              | 1      |      |
| packh rd, rs1, rs2                             | 1      |      |
| brev8 rd, rs1                                  | 1      |      |
| zip rd, rs1                                    | 1      |      |
| unzip rd, rs1                                  | 1      |      |

