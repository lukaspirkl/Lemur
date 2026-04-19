## <span id="page-54-0"></span> A.3. A Extension

<span id="page-54-4"></span><span id="page-54-3"></span>

| Instruction                     | Cycles | Note                                                                          |  |
|---------------------------------|--------|-------------------------------------------------------------------------------|--|
| Load-Reserved/Store-Conditional |        |                                                                               |  |
| lr.w rd, (rs1)                  | 1 or 2 | 2 if next instruction is dependent[2]<br>, an lr.w, sc.w or<br>[3]<br>amo*.w. |  |
| sc.w rd, rs2, (rs1)             | 1 or 2 | 2 if next instruction is dependent[2]<br>, an lr.w, sc.w or<br>[3]<br>amo*.w. |  |
| Atomic Memory Operations        |        |                                                                               |  |
| amoswap.w rd, rs2, (rs1)        | 4+     | 4 per attempt. Multiple attempts if reservation is lost.[4]                   |  |
| amoadd.w rd, rs2, (rs1)         | 4+     | 4 per attempt. Multiple attempts if reservation is lost.[4]                   |  |
| amoxor.w rd, rs2, (rs1)         | 4+     | 4 per attempt. Multiple attempts if reservation is lost.[4]                   |  |
| amoand.w rd, rs2, (rs1)         | 4+     | 4 per attempt. Multiple attempts if reservation is lost.[4]                   |  |
| amoor.w rd, rs2, (rs1)          | 4+     | 4 per attempt. Multiple attempts if reservation is lost.[4]                   |  |
| amomin.w rd, rs2, (rs1)         | 4+     | 4 per attempt. Multiple attempts if reservation is lost.[4]                   |  |
| amomax.w rd, rs2, (rs1)         | 4+     | 4 per attempt. Multiple attempts if reservation is lost.[4]                   |  |
| amominu.w rd, rs2, (rs1)        | 4+     | 4 per attempt. Multiple attempts if reservation is lost.[4]                   |  |
| amomaxu.w rd, rs2, (rs1)        | 4+     | 4 per attempt. Multiple attempts if reservation is lost.[4]                   |  |

