## <span id="page-56-1"></span> A.8. Zcmp Extension

<span id="page-56-2"></span>

| Instruction             | Cycles                               | Note                              |
|-------------------------|--------------------------------------|-----------------------------------|
| cm.push {rlist}, -imm   | 1 + n                                | n is number of registers in rlist |
| cm.pop {rlist}, imm     | 1 + n                                | n is number of registers in rlist |
| cm.popret {rlist}, imm  | 4 (n = 1)[5]<br>or 2 + n (n >= 2)[1] | n is number of registers in rlist |
| cm.popretz {rlist}, imm | 5 (n = 1)[5]<br>or 3 + n (n >= 2)[1] | n is number of registers in rlist |
| cm.mva01s r1s', r2s'    | 2                                    |                                   |
| cm.mvsa01 r1s', r2s'    | 2                                    |                                   |

