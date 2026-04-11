# 10.6.3 List of Registers

<span id="page-823-1"></span>The ACCESSCTRL registers start at a base address of 0x40060000 (defined as [ACCESSCTRL\\_BASE](#page-31-1) in the SDK).

*Table 910. List of ACCESSCTRL registers*

| Offset | Name          | Info                                                                                                                                                                                                                                                       |
|--------|---------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x00   | LOCK          | Once a LOCK bit is written to 1, ACCESSCTRL silently ignores<br>writes from that master. LOCK is writable only by a Secure,<br>Privileged processor or debugger.                                                                                           |
|        |               | LOCK bits are only writable when their value is zero. Once set,<br>they can never be cleared, except by a full reset of ACCESSCTRL                                                                                                                         |
|        |               | Setting the LOCK bit does not affect whether an access raises a<br>bus error. Unprivileged writes, or writes from the DMA, will<br>continue to raise bus errors. All other accesses will continue not<br>to.                                               |
| 0x04   | FORCE_CORE_NS | Force core 1's bus accesses to always be Non-secure, no matter<br>the core's internal state.                                                                                                                                                               |
|        |               | Useful for schemes where one core is designated as the Non<br>secure core, since some peripherals may filter individual<br>registers internally based on security state but not on master ID.                                                              |
| 0x08   | CFGRESET      | Write 1 to reset all ACCESSCTRL configuration, except for the<br>LOCK and FORCE_CORE_NS registers.                                                                                                                                                         |
|        |               | This bit is used in the RP2350 bootrom to quickly restore<br>ACCESSCTRL to a known state during the boot path.                                                                                                                                             |
|        |               | Note that, like all registers in ACCESSCTRL, this register is not<br>writable when the writer's corresponding LOCK bit is set,<br>therefore a master which has been locked out of ACCESSCTRL<br>can not use the CFGRESET register to disturb its contents. |
| 0x0c   | GPIO_NSMASK0  | Control whether GPIO0…31 are accessible to Non-secure code.<br>Writable only by a Secure, Privileged processor or debugger.                                                                                                                                |
|        |               | 0 → Secure access only                                                                                                                                                                                                                                     |
|        |               | 1 → Secure + Non-secure access                                                                                                                                                                                                                             |
| 0x10   | GPIO_NSMASK1  | Control whether GPIO3247 are accessible to Non-secure code,<br>and whether QSPI and USB bitbang are accessible through the<br>Non-secure SIO. Writable only by a Secure, Privileged processor<br>or debugger.                                              |
| 0x14   | ROM           | Control access to ROM. Defaults to fully open access.                                                                                                                                                                                                      |
| 0x18   | XIP_MAIN      | Control access to XIP_MAIN. Defaults to fully open access.                                                                                                                                                                                                 |
| 0x1c   | SRAM0         | Control access to SRAM0. Defaults to fully open access.                                                                                                                                                                                                    |
| 0x20   | SRAM1         | Control access to SRAM1. Defaults to fully open access.                                                                                                                                                                                                    |
| 0x24   | SRAM2         | Control access to SRAM2. Defaults to fully open access.                                                                                                                                                                                                    |
| 0x28   | SRAM3         | Control access to SRAM3. Defaults to fully open access.                                                                                                                                                                                                    |
| 0x2c   | SRAM4         | Control access to SRAM4. Defaults to fully open access.                                                                                                                                                                                                    |
| 0x30   | SRAM5         | Control access to SRAM5. Defaults to fully open access.                                                                                                                                                                                                    |
| 0x34   | SRAM6         | Control access to SRAM6. Defaults to fully open access.                                                                                                                                                                                                    |
| 0x38   | SRAM7         | Control access to SRAM7. Defaults to fully open access.                                                                                                                                                                                                    |

| Offset | Name             | Info                                                                                                  |
|--------|------------------|-------------------------------------------------------------------------------------------------------|
| 0x3c   | SRAM8            | Control access to SRAM8. Defaults to fully open access.                                               |
| 0x40   | SRAM9            | Control access to SRAM9. Defaults to fully open access.                                               |
| 0x44   | DMA              | Control access to DMA. Defaults to Secure access from any<br>master.                                  |
| 0x48   | USBCTRL          | Control access to USBCTRL. Defaults to Secure access from any<br>master.                              |
| 0x4c   | PIO0             | Control access to PIO0. Defaults to Secure access from any<br>master.                                 |
| 0x50   | PIO1             | Control access to PIO1. Defaults to Secure access from any<br>master.                                 |
| 0x54   | PIO2             | Control access to PIO2. Defaults to Secure access from any<br>master.                                 |
| 0x58   | CORESIGHT_TRACE  | Control access to CORESIGHT_TRACE. Defaults to Secure,<br>Privileged processor or debug access only.  |
| 0x5c   | CORESIGHT_PERIPH | Control access to CORESIGHT_PERIPH. Defaults to Secure,<br>Privileged processor or debug access only. |
| 0x60   | SYSINFO          | Control access to SYSINFO. Defaults to fully open access.                                             |
| 0x64   | RESETS           | Control access to RESETS. Defaults to Secure access from any<br>master.                               |
| 0x68   | IO_BANK0         | Control access to IO_BANK0. Defaults to Secure access from<br>any master.                             |
| 0x6c   | IO_BANK1         | Control access to IO_BANK1. Defaults to Secure access from<br>any master.                             |
| 0x70   | PADS_BANK0       | Control access to PADS_BANK0. Defaults to Secure access from<br>any master.                           |
| 0x74   | PADS_QSPI        | Control access to PADS_QSPI. Defaults to Secure access from<br>any master.                            |
| 0x78   | BUSCTRL          | Control access to BUSCTRL. Defaults to Secure access from any<br>master.                              |
| 0x7c   | ADC              | Control access to ADC. Defaults to Secure access from any<br>master.                                  |
| 0x80   | HSTX             | Control access to HSTX. Defaults to Secure access from any<br>master.                                 |
| 0x84   | I2C0             | Control access to I2C0. Defaults to Secure access from any<br>master.                                 |
| 0x88   | I2C1             | Control access to I2C1. Defaults to Secure access from any<br>master.                                 |
| 0x8c   | PWM              | Control access to PWM. Defaults to Secure access from any<br>master.                                  |
| 0x90   | SPI0             | Control access to SPI0. Defaults to Secure access from any<br>master.                                 |
| 0x94   | SPI1             | Control access to SPI1. Defaults to Secure access from any<br>master.                                 |

<span id="page-826-0"></span>

| Offset | Name     | Info                                                                                          |
|--------|----------|-----------------------------------------------------------------------------------------------|
| 0x98   | TIMER0   | Control access to TIMER0. Defaults to Secure access from any<br>master.                       |
| 0x9c   | TIMER1   | Control access to TIMER1. Defaults to Secure access from any<br>master.                       |
| 0xa0   | UART0    | Control access to UART0. Defaults to Secure access from any<br>master.                        |
| 0xa4   | UART1    | Control access to UART1. Defaults to Secure access from any<br>master.                        |
| 0xa8   | OTP      | Control access to OTP. Defaults to Secure access from any<br>master.                          |
| 0xac   | TBMAN    | Control access to TBMAN. Defaults to Secure access from any<br>master.                        |
| 0xb0   | POWMAN   | Control access to POWMAN. Defaults to Secure, Privileged<br>processor or debug access only.   |
| 0xb4   | TRNG     | Control access to TRNG. Defaults to Secure, Privileged processor<br>or debug access only.     |
| 0xb8   | SHA256   | Control access to SHA256. Defaults to Secure, Privileged access<br>only.                      |
| 0xbc   | SYSCFG   | Control access to SYSCFG. Defaults to Secure, Privileged<br>processor or debug access only.   |
| 0xc0   | CLOCKS   | Control access to CLOCKS. Defaults to Secure, Privileged<br>processor or debug access only.   |
| 0xc4   | XOSC     | Control access to XOSC. Defaults to Secure, Privileged processor<br>or debug access only.     |
| 0xc8   | ROSC     | Control access to ROSC. Defaults to Secure, Privileged processor<br>or debug access only.     |
| 0xcc   | PLL_SYS  | Control access to PLL_SYS. Defaults to Secure, Privileged<br>processor or debug access only.  |
| 0xd0   | PLL_USB  | Control access to PLL_USB. Defaults to Secure, Privileged<br>processor or debug access only.  |
| 0xd4   | TICKS    | Control access to TICKS. Defaults to Secure, Privileged<br>processor or debug access only.    |
| 0xd8   | WATCHDOG | Control access to WATCHDOG. Defaults to Secure, Privileged<br>processor or debug access only. |
| 0xdc   | PSM      | Control access to PSM. Defaults to Secure, Privileged processor<br>or debug access only.      |
| 0xe0   | XIP_CTRL | Control access to XIP_CTRL. Defaults to Secure, Privileged<br>processor or debug access only. |
| 0xe4   | XIP_QMI  | Control access to XIP_QMI. Defaults to Secure, Privileged<br>processor or debug access only.  |
| 0xe8   | XIP_AUX  | Control access to XIP_AUX. Defaults to Secure, Privileged access<br>only.                     |

# **[ACCESSCTRL:](#page-823-1) LOCK Register**

**Offset**: 0x00

#### **Description**

Once a LOCK bit is written to 1, ACCESSCTRL silently ignores writes from that master. LOCK is writable only by a Secure, Privileged processor or debugger.

LOCK bits are only writable when their value is zero. Once set, they can never be cleared, except by a full reset of ACCESSCTRL

Setting the LOCK bit does not affect whether an access raises a bus error. Unprivileged writes, or writes from the DMA, will continue to raise bus errors. All other accesses will continue not to.

*Table 911. LOCK Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31:4 | Reserved.   | -    | -     |
| 3    | DEBUG       | RW   | 0x0   |
| 2    | DMA         | RO   | 0x1   |
| 1    | CORE1       | RW   | 0x0   |
| 0    | CORE0       | RW   | 0x0   |

# <span id="page-827-0"></span>**[ACCESSCTRL:](#page-823-1) FORCE\_CORE\_NS Register**

**Offset**: 0x04

#### **Description**

Force core 1's bus accesses to always be Non-secure, no matter the core's internal state.

Useful for schemes where one core is designated as the Non-secure core, since some peripherals may filter individual registers internally based on security state but not on master ID.

*Table 912. FORCE\_CORE\_NS Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31:2 | Reserved.   | -    | -     |
| 1    | CORE1       | RW   | 0x0   |
| 0    | Reserved.   | -    | -     |

# <span id="page-827-1"></span>**[ACCESSCTRL:](#page-823-1) CFGRESET Register**

**Offset**: 0x08

*Table 913. CFGRESET Register*

| Bits | Description                                                                                                                                                                                                                                                | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:1 | Reserved.                                                                                                                                                                                                                                                  | -    | -     |
| 0    | Write 1 to reset all ACCESSCTRL configuration, except for the LOCK and<br>FORCE_CORE_NS registers.                                                                                                                                                         | SC   | 0x0   |
|      | This bit is used in the RP2350 bootrom to quickly restore ACCESSCTRL to a<br>known state during the boot path.                                                                                                                                             |      |       |
|      | Note that, like all registers in ACCESSCTRL, this register is not writable when<br>the writer's corresponding LOCK bit is set, therefore a master which has been<br>locked out of ACCESSCTRL can not use the CFGRESET register to disturb its<br>contents. |      |       |

# <span id="page-828-0"></span>**[ACCESSCTRL:](#page-823-1) GPIO\_NSMASK0 Register**

**Offset**: 0x0c

*Table 914. GPIO\_NSMASK0 Register*

| Bits | Description                                                                                                                                           | Type | Reset      |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Control whether GPIO0…31 are accessible to Non-secure code. Writable only<br>by a Secure, Privileged processor or debugger.<br>0 → Secure access only | RW   | 0x00000000 |
|      | 1 → Secure + Non-secure access                                                                                                                        |      |            |

# <span id="page-828-1"></span>**[ACCESSCTRL:](#page-823-1) GPIO\_NSMASK1 Register**

**Offset**: 0x10

#### **Description**

Control whether GPIO32..47 are accessible to Non-secure code, and whether QSPI and USB bitbang are accessible through the Non-secure SIO. Writable only by a Secure, Privileged processor or debugger.

*Table 915. GPIO\_NSMASK1 Register*

| Bits  | Description | Type | Reset  |
|-------|-------------|------|--------|
| 31:28 | QSPI_SD     | RW   | 0x0    |
| 27    | QSPI_CSN    | RW   | 0x0    |
| 26    | QSPI_SCK    | RW   | 0x0    |
| 25    | USB_DM      | RW   | 0x0    |
| 24    | USB_DP      | RW   | 0x0    |
| 23:16 | Reserved.   | -    | -      |
| 15:0  | GPIO        | RW   | 0x0000 |

#### <span id="page-828-2"></span>**[ACCESSCTRL:](#page-823-1) ROM Register**

**Offset**: 0x14

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access ROM, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which

becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 916. ROM Register*

| Bits | Description                                                                                                                  | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                    | -    | -     |
| 7    | DBG: If 1, ROM can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, ROM can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, ROM can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, ROM can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, ROM can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, ROM can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, ROM can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |
| 0    | NSU: If 1, and NSP is also set, ROM can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                            |      |       |

# <span id="page-829-0"></span>**[ACCESSCTRL:](#page-823-1) XIP\_MAIN Register**

#### **Offset**: 0x18

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access XIP\_MAIN, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 917. XIP\_MAIN Register*

| Bits | Description                                                                                                                       | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                         | -    | -     |
| 7    | DBG: If 1, XIP_MAIN can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, XIP_MAIN can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, XIP_MAIN can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, XIP_MAIN can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, XIP_MAIN can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, XIP_MAIN can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, XIP_MAIN can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |

| Bits | Description                                                                                          | Type | Reset |
|------|------------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, XIP_MAIN can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.    |      |       |

# <span id="page-830-0"></span>**[ACCESSCTRL:](#page-823-1) SRAM0 Register**

#### **Offset**: 0x1c

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM0, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 918. SRAM0 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM0 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM0 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM0 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM0 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM0 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM0 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM0 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |
| 0    | NSU: If 1, and NSP is also set, SRAM0 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

## <span id="page-830-1"></span>**[ACCESSCTRL:](#page-823-1) SRAM1 Register**

#### **Offset**: 0x20

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM1, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 919. SRAM1 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM1 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM1 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM1 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM1 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM1 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM1 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM1 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |
| 0    | NSU: If 1, and NSP is also set, SRAM1 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

# <span id="page-831-0"></span>**[ACCESSCTRL:](#page-823-1) SRAM2 Register**

## **Offset**: 0x24 **Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM2, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 920. SRAM2 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM2 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM2 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM2 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM2 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM2 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM2 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM2 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |

| Bits | Description                                                                                       | Type | Reset |
|------|---------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, SRAM2 can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set. |      |       |

# <span id="page-832-0"></span>**[ACCESSCTRL:](#page-823-1) SRAM3 Register**

#### **Offset**: 0x28

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM3, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 921. SRAM3 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM3 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM3 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM3 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM3 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM3 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM3 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM3 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |
| 0    | NSU: If 1, and NSP is also set, SRAM3 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

# <span id="page-832-1"></span>**[ACCESSCTRL:](#page-823-1) SRAM4 Register**

#### **Offset**: 0x2c

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM4, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 922. SRAM4 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM4 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM4 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM4 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM4 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM4 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM4 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM4 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |
| 0    | NSU: If 1, and NSP is also set, SRAM4 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

# <span id="page-833-0"></span>**[ACCESSCTRL:](#page-823-1) SRAM5 Register**

# **Offset**: 0x30

**Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM5, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 923. SRAM5 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM5 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM5 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM5 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM5 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM5 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM5 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM5 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |

| Bits | Description                                                                                       | Type | Reset |
|------|---------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, SRAM5 can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set. |      |       |

# <span id="page-834-0"></span>**[ACCESSCTRL:](#page-823-1) SRAM6 Register**

#### **Offset**: 0x34

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM6, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 924. SRAM6 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM6 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM6 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM6 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM6 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM6 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM6 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM6 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |
| 0    | NSU: If 1, and NSP is also set, SRAM6 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

## <span id="page-834-1"></span>**[ACCESSCTRL:](#page-823-1) SRAM7 Register**

#### **Offset**: 0x38

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM7, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 925. SRAM7 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM7 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM7 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM7 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM7 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM7 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM7 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM7 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |
| 0    | NSU: If 1, and NSP is also set, SRAM7 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

# <span id="page-835-0"></span>**[ACCESSCTRL:](#page-823-1) SRAM8 Register**

#### **Offset**: 0x3c

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM8, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 926. SRAM8 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM8 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM8 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM8 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM8 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM8 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM8 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM8 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |

| Bits | Description                                                                                       | Type | Reset |
|------|---------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, SRAM8 can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set. |      |       |

# <span id="page-836-0"></span>**[ACCESSCTRL:](#page-823-1) SRAM9 Register**

#### **Offset**: 0x40

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SRAM9, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 927. SRAM9 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, SRAM9 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SRAM9 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SRAM9 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SRAM9 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SRAM9 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SRAM9 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SRAM9 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x1   |
| 0    | NSU: If 1, and NSP is also set, SRAM9 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

# <span id="page-836-1"></span>**[ACCESSCTRL:](#page-823-1) DMA Register**

#### **Offset**: 0x44

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access DMA, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 928. DMA Register*

| Bits | Description                                                                                                                  | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                    | -    | -     |
| 7    | DBG: If 1, DMA can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, DMA can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, DMA can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, DMA can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, DMA can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, DMA can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, DMA can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, DMA can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                            |      |       |

# <span id="page-837-0"></span>**[ACCESSCTRL:](#page-823-1) USBCTRL Register**

**Offset**: 0x48

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access USBCTRL, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 929. USBCTRL Register*

| Bits | Description                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                        | -    | -     |
| 7    | DBG: If 1, USBCTRL can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, USBCTRL can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, USBCTRL can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, USBCTRL can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, USBCTRL can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, USBCTRL can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, USBCTRL can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                         | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, USBCTRL can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.   |      |       |

# <span id="page-838-0"></span>**[ACCESSCTRL:](#page-823-1) PIO0 Register**

#### **Offset**: 0x4c

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access PIO0, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 930. PIO0 Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, PIO0 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, PIO0 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, PIO0 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, PIO0 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, PIO0 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, PIO0 can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, PIO0 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, PIO0 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                             |      |       |

# <span id="page-838-1"></span>**[ACCESSCTRL:](#page-823-1) PIO1 Register**

#### **Offset**: 0x50

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access PIO1, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 931. PIO1 Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, PIO1 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, PIO1 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, PIO1 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, PIO1 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, PIO1 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, PIO1 can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, PIO1 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, PIO1 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                             |      |       |

# <span id="page-839-0"></span>**[ACCESSCTRL:](#page-823-1) PIO2 Register**

**Offset**: 0x54

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access PIO2, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 932. PIO2 Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, PIO2 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, PIO2 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, PIO2 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, PIO2 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, PIO2 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, PIO2 can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, PIO2 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                       | Type | Reset |
|------|---------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, PIO2 can be accessed from a Non-secure,<br>Unprivileged context.  | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set. |      |       |

# <span id="page-840-0"></span>**[ACCESSCTRL:](#page-823-1) CORESIGHT\_TRACE Register**

#### **Offset**: 0x58

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access CORESIGHT\_TRACE, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 933. CORESIGHT\_TRACE Register*

| Bits | Description                                                                                                                              | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                                | -    | -     |
| 7    | DBG: If 1, CORESIGHT_TRACE can be accessed by the debugger, at<br>security/privilege levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, CORESIGHT_TRACE can be accessed by the DMA, at<br>security/privilege levels permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, CORESIGHT_TRACE can be accessed by core 1, at<br>security/privilege levels permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, CORESIGHT_TRACE can be accessed by core 0, at<br>security/privilege levels permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, CORESIGHT_TRACE can be accessed from a Secure, Privileged<br>context.                                                          | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, CORESIGHT_TRACE can be accessed from a<br>Secure, Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, CORESIGHT_TRACE can be accessed from a Non-secure, Privileged<br>context.                                                     | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, CORESIGHT_TRACE can be accessed from a<br>Non-secure, Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                                        |      |       |

#### <span id="page-840-1"></span>**[ACCESSCTRL:](#page-823-1) CORESIGHT\_PERIPH Register**

## **Offset**: 0x5c **Description**

Control whether debugger, DMA, core 0 and core 1 can access CORESIGHT\_PERIPH, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 934. CORESIGHT\_PERIPH Register*

| Bits | Description                                                                                                                               | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                                 | -    | -     |
| 7    | DBG: If 1, CORESIGHT_PERIPH can be accessed by the debugger, at<br>security/privilege levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, CORESIGHT_PERIPH can be accessed by the DMA, at<br>security/privilege levels permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, CORESIGHT_PERIPH can be accessed by core 1, at<br>security/privilege levels permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, CORESIGHT_PERIPH can be accessed by core 0, at<br>security/privilege levels permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, CORESIGHT_PERIPH can be accessed from a Secure, Privileged<br>context.                                                          | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, CORESIGHT_PERIPH can be accessed from a<br>Secure, Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, CORESIGHT_PERIPH can be accessed from a Non-secure, Privileged<br>context.                                                     | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, CORESIGHT_PERIPH can be accessed from a<br>Non-secure, Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                                         |      |       |

# <span id="page-841-0"></span>**[ACCESSCTRL:](#page-823-1) SYSINFO Register**

#### **Offset**: 0x60

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SYSINFO, and at what security/privilege levels they can do so.

Defaults to fully open access.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 935. SYSINFO Register*

| Bits | Description                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                        | -    | -     |
| 7    | DBG: If 1, SYSINFO can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SYSINFO can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SYSINFO can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SYSINFO can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SYSINFO can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SYSINFO can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |

| Bits | Description                                                                                         | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------|------|-------|
| 1    | NSP: If 1, SYSINFO can be accessed from a Non-secure, Privileged context.                           | RW   | 0x1   |
| 0    | NSU: If 1, and NSP is also set, SYSINFO can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x1   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.   |      |       |

# <span id="page-842-0"></span>**[ACCESSCTRL:](#page-823-1) RESETS Register**

#### **Offset**: 0x64

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access RESETS, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 936. RESETS Register*

| Bits | Description                                                                                                                     | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                       | -    | -     |
| 7    | DBG: If 1, RESETS can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, RESETS can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, RESETS can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, RESETS can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, RESETS can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, RESETS can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, RESETS can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, RESETS can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                               |      |       |

# <span id="page-842-1"></span>**[ACCESSCTRL:](#page-823-1) IO\_BANK0 Register**

#### **Offset**: 0x68

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access IO\_BANK0, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 937. IO\_BANK0 Register*

| Bits | Description                                                                                                                       | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                         | -    | -     |
| 7    | DBG: If 1, IO_BANK0 can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, IO_BANK0 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, IO_BANK0 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, IO_BANK0 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, IO_BANK0 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, IO_BANK0 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, IO_BANK0 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, IO_BANK0 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                                 |      |       |

# <span id="page-843-0"></span>**[ACCESSCTRL:](#page-823-1) IO\_BANK1 Register**

#### **Offset**: 0x6c

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access IO\_BANK1, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 938. IO\_BANK1 Register*

| Bits | Description                                                                                                                       | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                         | -    | -     |
| 7    | DBG: If 1, IO_BANK1 can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, IO_BANK1 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, IO_BANK1 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, IO_BANK1 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, IO_BANK1 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, IO_BANK1 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, IO_BANK1 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                          | Type | Reset |
|------|------------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, IO_BANK1 can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.    |      |       |

# <span id="page-844-0"></span>**[ACCESSCTRL:](#page-823-1) PADS\_BANK0 Register**

**Offset**: 0x70

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access PADS\_BANK0, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 939. PADS\_BANK0 Register*

| Bits | Description                                                                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                           | -    | -     |
| 7    | DBG: If 1, PADS_BANK0 can be accessed by the debugger, at<br>security/privilege levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, PADS_BANK0 can be accessed by the DMA, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, PADS_BANK0 can be accessed by core 1, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, PADS_BANK0 can be accessed by core 0, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, PADS_BANK0 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, PADS_BANK0 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, PADS_BANK0 can be accessed from a Non-secure, Privileged<br>context.                                                     | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, PADS_BANK0 can be accessed from a Non<br>secure, Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                                   |      |       |

#### <span id="page-844-1"></span>**[ACCESSCTRL:](#page-823-1) PADS\_QSPI Register**

**Offset**: 0x74 **Description**

> Control whether debugger, DMA, core 0 and core 1 can access PADS\_QSPI, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 940. PADS\_QSPI Register*

| Bits | Description                                                                                                                        | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                          | -    | -     |
| 7    | DBG: If 1, PADS_QSPI can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, PADS_QSPI can be accessed by the DMA, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, PADS_QSPI can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, PADS_QSPI can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, PADS_QSPI can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, PADS_QSPI can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, PADS_QSPI can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, PADS_QSPI can be accessed from a Non<br>secure, Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                                  |      |       |

# <span id="page-845-0"></span>**[ACCESSCTRL:](#page-823-1) BUSCTRL Register**

**Offset**: 0x78

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access BUSCTRL, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 941. BUSCTRL Register*

| Bits | Description                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                        | -    | -     |
| 7    | DBG: If 1, BUSCTRL can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, BUSCTRL can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, BUSCTRL can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, BUSCTRL can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, BUSCTRL can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, BUSCTRL can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, BUSCTRL can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                         | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, BUSCTRL can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.   |      |       |

# <span id="page-846-0"></span>**[ACCESSCTRL:](#page-823-1) ADC Register**

#### **Offset**: 0x7c

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access ADC, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 942. ADC Register*

| Bits | Description                                                                                                                  | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                    | -    | -     |
| 7    | DBG: If 1, ADC can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, ADC can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, ADC can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, ADC can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, ADC can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, ADC can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, ADC can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, ADC can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                            |      |       |

# <span id="page-846-1"></span>**[ACCESSCTRL:](#page-823-1) HSTX Register**

#### **Offset**: 0x80

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access HSTX, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 943. HSTX Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, HSTX can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, HSTX can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, HSTX can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, HSTX can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, HSTX can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, HSTX can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, HSTX can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, HSTX can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                             |      |       |

# <span id="page-847-0"></span>**[ACCESSCTRL:](#page-823-1) I2C0 Register**

**Offset**: 0x84

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access I2C0, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 944. I2C0 Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, I2C0 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, I2C0 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, I2C0 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, I2C0 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, I2C0 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, I2C0 can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, I2C0 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                       | Type | Reset |
|------|---------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, I2C0 can be accessed from a Non-secure,<br>Unprivileged context.  | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set. |      |       |

# <span id="page-848-0"></span>**[ACCESSCTRL:](#page-823-1) I2C1 Register**

#### **Offset**: 0x88

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access I2C1, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 945. I2C1 Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, I2C1 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, I2C1 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, I2C1 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, I2C1 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, I2C1 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, I2C1 can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, I2C1 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, I2C1 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                             |      |       |

# <span id="page-848-1"></span>**[ACCESSCTRL:](#page-823-1) PWM Register**

#### **Offset**: 0x8c

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access PWM, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 946. PWM Register*

| Bits | Description                                                                                                                  | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                    | -    | -     |
| 7    | DBG: If 1, PWM can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, PWM can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, PWM can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, PWM can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, PWM can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, PWM can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, PWM can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, PWM can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                            |      |       |

# <span id="page-849-0"></span>**[ACCESSCTRL:](#page-823-1) SPI0 Register**

**Offset**: 0x90

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SPI0, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 947. SPI0 Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, SPI0 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SPI0 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SPI0 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SPI0 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SPI0 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SPI0 can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SPI0 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                       | Type | Reset |
|------|---------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, SPI0 can be accessed from a Non-secure,<br>Unprivileged context.  | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set. |      |       |

# <span id="page-850-0"></span>**[ACCESSCTRL:](#page-823-1) SPI1 Register**

#### **Offset**: 0x94

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SPI1, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 948. SPI1 Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, SPI1 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SPI1 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SPI1 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SPI1 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SPI1 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SPI1 can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, SPI1 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, SPI1 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                             |      |       |

# <span id="page-850-1"></span>**[ACCESSCTRL:](#page-823-1) TIMER0 Register**

#### **Offset**: 0x98

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access TIMER0, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 949. TIMER0 Register*

| Bits | Description                                                                                                                     | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                       | -    | -     |
| 7    | DBG: If 1, TIMER0 can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, TIMER0 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, TIMER0 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, TIMER0 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, TIMER0 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, TIMER0 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, TIMER0 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, TIMER0 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                               |      |       |

# <span id="page-851-0"></span>**[ACCESSCTRL:](#page-823-1) TIMER1 Register**

**Offset**: 0x9c

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access TIMER1, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 950. TIMER1 Register*

| Bits | Description                                                                                                                     | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                       | -    | -     |
| 7    | DBG: If 1, TIMER1 can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, TIMER1 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, TIMER1 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, TIMER1 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, TIMER1 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, TIMER1 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, TIMER1 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                        | Type | Reset |
|------|----------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, TIMER1 can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.  |      |       |

# <span id="page-852-0"></span>**[ACCESSCTRL:](#page-823-1) UART0 Register**

#### **Offset**: 0xa0

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access UART0, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 951. UART0 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, UART0 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, UART0 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, UART0 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, UART0 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, UART0 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, UART0 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, UART0 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, UART0 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

# <span id="page-852-1"></span>**[ACCESSCTRL:](#page-823-1) UART1 Register**

#### **Offset**: 0xa4

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access UART1, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 952. UART1 Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, UART1 can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, UART1 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, UART1 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, UART1 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, UART1 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, UART1 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, UART1 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, UART1 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

# <span id="page-853-0"></span>**[ACCESSCTRL:](#page-823-1) OTP Register**

#### **Offset**: 0xa8

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access OTP, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 953. OTP Register*

| Bits | Description                                                                                                                  | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                    | -    | -     |
| 7    | DBG: If 1, OTP can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, OTP can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, OTP can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, OTP can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, OTP can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, OTP can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, OTP can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                       | Type | Reset |
|------|---------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, OTP can be accessed from a Non-secure,<br>Unprivileged context.   | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set. |      |       |

# <span id="page-854-0"></span>**[ACCESSCTRL:](#page-823-1) TBMAN Register**

#### **Offset**: 0xac

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access TBMAN, and at what security/privilege levels they can do so.

Defaults to Secure access from any master.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 954. TBMAN Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, TBMAN can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, TBMAN can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, TBMAN can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, TBMAN can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, TBMAN can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, TBMAN can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x1   |
| 1    | NSP: If 1, TBMAN can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, TBMAN can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

## <span id="page-854-1"></span>**[ACCESSCTRL:](#page-823-1) POWMAN Register**

#### **Offset**: 0xb0

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access POWMAN, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 955. POWMAN Register*

| Bits | Description                                                                                                                     | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                       | -    | -     |
| 7    | DBG: If 1, POWMAN can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, POWMAN can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, POWMAN can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, POWMAN can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, POWMAN can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, POWMAN can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, POWMAN can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, POWMAN can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                               |      |       |

# <span id="page-855-0"></span>**[ACCESSCTRL:](#page-823-1) TRNG Register**

# **Offset**: 0xb4

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access TRNG, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 956. TRNG Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, TRNG can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, TRNG can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, TRNG can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, TRNG can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, TRNG can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, TRNG can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, TRNG can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                       | Type | Reset |
|------|---------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, TRNG can be accessed from a Non-secure,<br>Unprivileged context.  | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set. |      |       |

# <span id="page-856-0"></span>**[ACCESSCTRL:](#page-823-1) SHA256 Register**

#### **Offset**: 0xb8

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SHA256, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 957. SHA256 Register*

| Bits | Description                                                                                                                     | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                       | -    | -     |
| 7    | DBG: If 1, SHA256 can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SHA256 can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, SHA256 can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SHA256 can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SHA256 can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SHA256 can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, SHA256 can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, SHA256 can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                               |      |       |

## <span id="page-856-1"></span>**[ACCESSCTRL:](#page-823-1) SYSCFG Register**

#### **Offset**: 0xbc

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access SYSCFG, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 958. SYSCFG Register*

| Bits | Description                                                                                                                     | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                       | -    | -     |
| 7    | DBG: If 1, SYSCFG can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, SYSCFG can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, SYSCFG can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, SYSCFG can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, SYSCFG can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, SYSCFG can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, SYSCFG can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, SYSCFG can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                               |      |       |

# <span id="page-857-0"></span>**[ACCESSCTRL:](#page-823-1) CLOCKS Register**

#### **Offset**: 0xc0

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access CLOCKS, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 959. CLOCKS Register*

| Bits | Description                                                                                                                     | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                       | -    | -     |
| 7    | DBG: If 1, CLOCKS can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, CLOCKS can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, CLOCKS can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, CLOCKS can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, CLOCKS can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, CLOCKS can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, CLOCKS can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                        | Type | Reset |
|------|----------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, CLOCKS can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.  |      |       |

# <span id="page-858-0"></span>**[ACCESSCTRL:](#page-823-1) XOSC Register**

#### **Offset**: 0xc4

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access XOSC, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 960. XOSC Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, XOSC can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, XOSC can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, XOSC can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, XOSC can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, XOSC can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, XOSC can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, XOSC can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, XOSC can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                             |      |       |

## <span id="page-858-1"></span>**[ACCESSCTRL:](#page-823-1) ROSC Register**

#### **Offset**: 0xc8

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access ROSC, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 961. ROSC Register*

| Bits | Description                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                     | -    | -     |
| 7    | DBG: If 1, ROSC can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, ROSC can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, ROSC can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, ROSC can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, ROSC can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, ROSC can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, ROSC can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, ROSC can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                             |      |       |

# <span id="page-859-0"></span>**[ACCESSCTRL:](#page-823-1) PLL\_SYS Register**

#### **Offset**: 0xcc

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access PLL\_SYS, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 962. PLL\_SYS Register*

| Bits | Description                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                        | -    | -     |
| 7    | DBG: If 1, PLL_SYS can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, PLL_SYS can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, PLL_SYS can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, PLL_SYS can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, PLL_SYS can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, PLL_SYS can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, PLL_SYS can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                         | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, PLL_SYS can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.   |      |       |

# <span id="page-860-0"></span>**[ACCESSCTRL:](#page-823-1) PLL\_USB Register**

#### **Offset**: 0xd0

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access PLL\_USB, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 963. PLL\_USB Register*

| Bits | Description                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                        | -    | -     |
| 7    | DBG: If 1, PLL_USB can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, PLL_USB can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, PLL_USB can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, PLL_USB can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, PLL_USB can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, PLL_USB can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, PLL_USB can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, PLL_USB can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                                |      |       |

# <span id="page-860-1"></span>**[ACCESSCTRL:](#page-823-1) TICKS Register**

#### **Offset**: 0xd4

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access TICKS, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 964. TICKS Register*

| Bits | Description                                                                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                      | -    | -     |
| 7    | DBG: If 1, TICKS can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, TICKS can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, TICKS can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, TICKS can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, TICKS can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, TICKS can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, TICKS can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, TICKS can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                              |      |       |

# <span id="page-861-0"></span>**[ACCESSCTRL:](#page-823-1) WATCHDOG Register**

**Offset**: 0xd8

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access WATCHDOG, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 965. WATCHDOG Register*

| Bits | Description                                                                                                                       | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                         | -    | -     |
| 7    | DBG: If 1, WATCHDOG can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, WATCHDOG can be accessed by the DMA, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, WATCHDOG can be accessed by core 1, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, WATCHDOG can be accessed by core 0, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, WATCHDOG can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, WATCHDOG can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, WATCHDOG can be accessed from a Non-secure, Privileged context. RW                                                     |      | 0x0   |

| Bits | Description                                                                                          | Type | Reset |
|------|------------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, WATCHDOG can be accessed from a Non<br>secure, Unprivileged context. | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.    |      |       |

# <span id="page-862-0"></span>**[ACCESSCTRL:](#page-823-1) PSM Register**

#### **Offset**: 0xdc

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access PSM, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 966. PSM Register*

| Bits | Description                                                                                                                  | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                    | -    | -     |
| 7    | DBG: If 1, PSM can be accessed by the debugger, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, PSM can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, PSM can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, PSM can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, PSM can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, PSM can be accessed from a Secure, Unprivileged<br>context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, PSM can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, PSM can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                            |      |       |

# <span id="page-862-1"></span>**[ACCESSCTRL:](#page-823-1) XIP\_CTRL Register**

#### **Offset**: 0xe0

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access XIP\_CTRL, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 967. XIP\_CTRL Register*

| Bits | Description                                                                                                                       | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                         | -    | -     |
| 7    | DBG: If 1, XIP_CTRL can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, XIP_CTRL can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, XIP_CTRL can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, XIP_CTRL can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, XIP_CTRL can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, XIP_CTRL can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, XIP_CTRL can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, XIP_CTRL can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                                 |      |       |

# <span id="page-863-0"></span>**[ACCESSCTRL:](#page-823-1) XIP\_QMI Register**

#### **Offset**: 0xe4

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access XIP\_QMI, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged processor or debug access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 968. XIP\_QMI Register*

| Bits | Description                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                        | -    | -     |
| 7    | DBG: If 1, XIP_QMI can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, XIP_QMI can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x0   |
| 5    | CORE1: If 1, XIP_QMI can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, XIP_QMI can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, XIP_QMI can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, XIP_QMI can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, XIP_QMI can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |

| Bits | Description                                                                                         | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------|------|-------|
| 0    | NSU: If 1, and NSP is also set, XIP_QMI can be accessed from a Non-secure,<br>Unprivileged context. | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.   |      |       |

#### <span id="page-864-1"></span>**[ACCESSCTRL:](#page-823-1) XIP\_AUX Register**

#### **Offset**: 0xe8

#### **Description**

Control whether debugger, DMA, core 0 and core 1 can access XIP\_AUX, and at what security/privilege levels they can do so.

Defaults to Secure, Privileged access only.

This register is writable only from a Secure, Privileged processor or debugger, with the exception of the NSU bit, which becomes Non-secure-Privileged-writable when the NSP bit is set.

*Table 969. XIP\_AUX Register*

| Bits | Description                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                        | -    | -     |
| 7    | DBG: If 1, XIP_AUX can be accessed by the debugger, at security/privilege<br>levels permitted by SP/NSP/SU/NSU in this register. | RW   | 0x1   |
| 6    | DMA: If 1, XIP_AUX can be accessed by the DMA, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.      | RW   | 0x1   |
| 5    | CORE1: If 1, XIP_AUX can be accessed by core 1, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 4    | CORE0: If 1, XIP_AUX can be accessed by core 0, at security/privilege levels<br>permitted by SP/NSP/SU/NSU in this register.     | RW   | 0x1   |
| 3    | SP: If 1, XIP_AUX can be accessed from a Secure, Privileged context.                                                             | RW   | 0x1   |
| 2    | SU: If 1, and SP is also set, XIP_AUX can be accessed from a Secure,<br>Unprivileged context.                                    | RW   | 0x0   |
| 1    | NSP: If 1, XIP_AUX can be accessed from a Non-secure, Privileged context.                                                        | RW   | 0x0   |
| 0    | NSU: If 1, and NSP is also set, XIP_AUX can be accessed from a Non-secure,<br>Unprivileged context.                              | RW   | 0x0   |
|      | This bit is writable from a Non-secure, Privileged context, if and only if the NSP<br>bit is set.                                |      |       |

