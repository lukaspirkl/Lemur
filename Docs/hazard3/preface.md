Hazard3

Updated: 2024-Aug-07

# **Table of Contents**

| 1. Introduction                          |   |
|------------------------------------------|---|
| 1.1. Architectural Overview              |   |
| 1.1.1. Pipeline Stages                   |   |
| 1.1.2. Bus Interfaces.                   |   |
| 1.1.3. Multiply/Divide                   |   |
| 1.2. List of RISC-V Specifications       |   |
| 2. Configuration and Integration         |   |
| 2.1. Hazard3 Source Files                |   |
| 2.2. Top-level Modules                   |   |
| 2.3. FPGA Synthesis                      |   |
| 2.4. ASIC Synthesis                      |   |
| 2.5. Interfaces (Top-level Ports).       |   |
| 2.5.1. Interfaces Common to All Wrappers |   |
| 2.5.2. Interfaces for 1-port AHB5 CPU    |   |
| 2.5.3. Interfaces for 2-port AHB5 CPU    | 1 |
| 2.6. Configuration Parameters            | 1 |
| 2.6.1. Reset state configuration         | 1 |
| 2.6.2. Standard RISC-V ISA support       | 1 |
| 2.6.3. Custom Hazard3 Extensions.        | 1 |
| 2.6.4. CSR support                       | 1 |
| 2.6.5. External interrupt support        | 1 |
| 2.6.6. Identification Registers          | 1 |
| 2.6.7. Performance/size options          |   |
| 3. CSRs                                  | 2 |
| 3.1. Standard M-mode Identification CSRs | 2 |
| 3.1.1. mvendorid                         | 2 |
| 3.1.2. marchid                           | 2 |
| 3.1.3. mimpid                            | 2 |
| 3.1.4. mhartid                           | 2 |
| 3.1.5. mconfigptr                        | 2 |
| 3.1.6. misa                              | 2 |
| 3.2. Standard M-mode Trap Handling CSRs  |   |
| 3.2.1. mstatus                           |   |
| 3.2.2. mstatush                          | 2 |
| 3.2.3. medeleg                           |   |
| 3.2.4. mideleg                           |   |
| 3.2.5. mie                               |   |
| 3.2.6. mip                               |   |

| 3.2.7. mtvec                              | 25 |
|-------------------------------------------|----|
| 3.2.8. mscratch                           | 25 |
| 3.2.9. mepc.                              | 25 |
| 3.2.10. mcause.                           | 25 |
| 3.2.11. mtval                             | 26 |
| 3.2.12. mcounteren                        | 26 |
| 3.3. Standard Memory Protection CSRs      | 27 |
| 3.3.1. pmpcfg03                           | 27 |
| 3.3.2. pmpaddr015                         | 27 |
| 3.4. Standard M-mode Performance Counters | 28 |
| 3.4.1. mcycle                             | 28 |
| 3.4.2. mcycleh                            | 28 |
| 3.4.3. minstret                           | 28 |
| 3.4.4. minstreth                          | 29 |
| 3.4.5. mhpmcounter331                     | 29 |
| 3.4.6. mhpmcounter331h                    | 29 |
| 3.4.7. mcountinhibit                      | 29 |
| 3.4.8. mhpmevent331                       | 29 |
| 3.5. Standard Trigger CSRs                | 29 |
| 3.5.1. tselect                            | 29 |
| 3.5.2. tdata13                            | 29 |
| 3.6. Standard Debug Mode CSRs             | 30 |
| 3.6.1. dcsr                               | 30 |
| 3.6.2. dpc                                | 31 |
| 3.6.3. dscratch0                          | 31 |
| 3.6.4. dscratch1                          | 31 |
| 3.7. Custom Debug Mode CSRs               | 31 |
| 3.7.1. dmdata0                            | 31 |
| 3.8. Custom Interrupt Handling CSRs       |    |
| 3.8.1. meiea                              |    |
| 3.8.2. meipa                              | 32 |
| 3.8.3. meifa                              | 33 |
| 3.8.4. meipra                             | 34 |
| 3.8.5. meinext                            | 34 |
| 3.8.6. meicontext                         | 35 |
| 3.9. Custom Memory Protection CSRs        |    |
| 3.9.1. pmpcfgm0                           |    |
| 3.10. Custom Power Control CSRs.          |    |
| 3.10.1. msleep                            | 38 |
| 4. Custom Extensions                      |    |
| 4.1. Xh3irq: Hazard3 interrupt controller |    |

| 4.2. Xh3pmpm: M-mode PMP regions 40               |
|---------------------------------------------------|
| 4.3. Xh3power: Hazard3 power management 40        |
| 4.3.1. h3.block 41                                |
| 4.3.2. h3.unblock                                 |
| 4.4. Xh3bextm: Hazard3 bit extract multiple       |
| 4.4.1. h3.bextm                                   |
| 4.4.2. h3.bextmi                                  |
| 5. Debug                                          |
| 5.1. Debug Topologies                             |
| 5.2. Implementation-defined behaviour             |
| 5.3. Debug Module to Core Interface 48            |
| Appendix A: Instruction Cycle Counts              |
| A.1. RV32I                                        |
| A.2. M Extension 50                               |
| A.3. A Extension                                  |
| A.4. C Extension 51                               |
| A.5. Privileged Instructions (including Zicsr) 51 |
| A.6. Bit Manipulation                             |
| A.7. Zcb Extension                                |
| A.8. Zcmp Extension                               |
| A.9. Branch Predictor                             |

