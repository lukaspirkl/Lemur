# 12.2.16 Operation of Interrupt Registers

[Table 1053](#page-1005-2) lists the operation of the DW\_apb\_i2c interrupt registers and how they are set and cleared. Some bits are set by hardware and cleared by software, whereas other bits are set and cleared by hardware.

*Table 1053. Clearing and Setting of Interrupt Registers*

<span id="page-1005-2"></span>

| Interrupt Bit Fields | Set by Hardware/Cleared by Software | Set and Cleared by Hardware |
|----------------------|-------------------------------------|-----------------------------|
| RESTART_DET          | Y                                   | N                           |
| GEN_CALL             | Y                                   | N                           |
| START_DET            | Y                                   | N                           |
| STOP_DET             | Y                                   | N                           |
| ACTIVITY             | Y                                   | N                           |
| RX_DONE              | Y                                   | N                           |
| TX_ABRT              | Y                                   | N                           |
| RD_REQ               | Y                                   | N                           |
| TX_EMPTY             | N                                   | Y                           |
| TX_OVER              | Y                                   | N                           |
| RX_FULL              | N                                   | Y                           |
| RX_OVER              | Y                                   | N                           |
| RX_UNDER             | Y                                   | N                           |
|                      |                                     |                             |

