## <span id="page-53-1"></span><span id="page-53-0"></span>**A.2. M Extension**

Timings assume the core is configured with MULDIV_UNROLL = 2 and MUL_FAST = 1. I.e. the sequential multiply/divide circuit processes two bits per cycle, and a separate dedicated multiplier is present for the mul instruction.

| Instruction                       | Cycles   | Note                         |  |  |
|-----------------------------------|----------|------------------------------|--|--|
| 32 × 32 → 32 Multiply             |          |                              |  |  |
| mul rd, rs1, rs2                  | 1        |                              |  |  |
| 32 × 32 → 64 Multiply, Upper Half |          |                              |  |  |
| mulh rd, rs1, rs2                 | 1        |                              |  |  |
| mulhsu rd, rs1, rs2               | 1        |                              |  |  |
| mulhu rd, rs1, rs2                | 1        |                              |  |  |
| Divide and Remainder              |          |                              |  |  |
| div rd, rs1, rs2                  | 18 or 19 | Depending on sign correction |  |  |
| divu rd, rs1, rs2                 | 18       |                              |  |  |
| rem rd, rs1, rs2                  | 18 or 19 | Depending on sign correction |  |  |
| remu rd, rs1, rs2                 | 18       |                              |  |  |

