# 7.5.3 List of Registers

<span id="page-502-1"></span>The reset controller registers start at a base address of 0x40020000 (defined as [RESETS\\_BASE](#page-31-1) in SDK).

*Table 533. List of RESETS registers*

| Offset | Name       | Info |
|--------|------------|------|
| 0x0    | RESET      |      |
| 0x4    | WDSEL      |      |
| 0x8    | RESET_DONE |      |

# <span id="page-503-0"></span>**[RESETS](#page-502-1): RESET Register**

**Offset**: 0x0

*Table 534. RESET Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:29 | Reserved.   | -    | -     |
| 28    | USBCTRL     | RW   | 0x1   |
| 27    | UART1       | RW   | 0x1   |
| 26    | UART0       | RW   | 0x1   |
| 25    | TRNG        | RW   | 0x1   |
| 24    | TIMER1      | RW   | 0x1   |
| 23    | TIMER0      | RW   | 0x1   |
| 22    | TBMAN       | RW   | 0x1   |
| 21    | SYSINFO     | RW   | 0x1   |
| 20    | SYSCFG      | RW   | 0x1   |
| 19    | SPI1        | RW   | 0x1   |
| 18    | SPI0        | RW   | 0x1   |
| 17    | SHA256      | RW   | 0x1   |
| 16    | PWM         | RW   | 0x1   |
| 15    | PLL_USB     | RW   | 0x1   |
| 14    | PLL_SYS     | RW   | 0x1   |
| 13    | PIO2        | RW   | 0x1   |
| 12    | PIO1        | RW   | 0x1   |
| 11    | PIO0        | RW   | 0x1   |
| 10    | PADS_QSPI   | RW   | 0x1   |
| 9     | PADS_BANK0  | RW   | 0x1   |
| 8     | JTAG        | RW   | 0x1   |
| 7     | IO_QSPI     | RW   | 0x1   |
| 6     | IO_BANK0    | RW   | 0x1   |
| 5     | I2C1        | RW   | 0x1   |
| 4     | I2C0        | RW   | 0x1   |
| 3     | HSTX        | RW   | 0x1   |
| 2     | DMA         | RW   | 0x1   |
| 1     | BUSCTRL     | RW   | 0x1   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 0    | ADC         | RW   | 0x1   |

# <span id="page-504-0"></span>**[RESETS](#page-502-1): WDSEL Register**

**Offset**: 0x4

*Table 535. WDSEL Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:29 | Reserved.   | -    | -     |
| 28    | USBCTRL     | RW   | 0x0   |
| 27    | UART1       | RW   | 0x0   |
| 26    | UART0       | RW   | 0x0   |
| 25    | TRNG        | RW   | 0x0   |
| 24    | TIMER1      | RW   | 0x0   |
| 23    | TIMER0      | RW   | 0x0   |
| 22    | TBMAN       | RW   | 0x0   |
| 21    | SYSINFO     | RW   | 0x0   |
| 20    | SYSCFG      | RW   | 0x0   |
| 19    | SPI1        | RW   | 0x0   |
| 18    | SPI0        | RW   | 0x0   |
| 17    | SHA256      | RW   | 0x0   |
| 16    | PWM         | RW   | 0x0   |
| 15    | PLL_USB     | RW   | 0x0   |
| 14    | PLL_SYS     | RW   | 0x0   |
| 13    | PIO2        | RW   | 0x0   |
| 12    | PIO1        | RW   | 0x0   |
| 11    | PIO0        | RW   | 0x0   |
| 10    | PADS_QSPI   | RW   | 0x0   |
| 9     | PADS_BANK0  | RW   | 0x0   |
| 8     | JTAG        | RW   | 0x0   |
| 7     | IO_QSPI     | RW   | 0x0   |
| 6     | IO_BANK0    | RW   | 0x0   |
| 5     | I2C1        | RW   | 0x0   |
| 4     | I2C0        | RW   | 0x0   |
| 3     | HSTX        | RW   | 0x0   |
| 2     | DMA         | RW   | 0x0   |
| 1     | BUSCTRL     | RW   | 0x0   |
| 0     | ADC         | RW   | 0x0   |

<span id="page-504-1"></span>**[RESETS](#page-502-1): RESET\_DONE Register**

**Offset**: 0x8

*Table 536. RESET\_DONE Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:29 | Reserved.   | -    | -     |
| 28    | USBCTRL     | RO   | 0x0   |
| 27    | UART1       | RO   | 0x0   |
| 26    | UART0       | RO   | 0x0   |
| 25    | TRNG        | RO   | 0x0   |
| 24    | TIMER1      | RO   | 0x0   |
| 23    | TIMER0      | RO   | 0x0   |
| 22    | TBMAN       | RO   | 0x0   |
| 21    | SYSINFO     | RO   | 0x0   |
| 20    | SYSCFG      | RO   | 0x0   |
| 19    | SPI1        | RO   | 0x0   |
| 18    | SPI0        | RO   | 0x0   |
| 17    | SHA256      | RO   | 0x0   |
| 16    | PWM         | RO   | 0x0   |
| 15    | PLL_USB     | RO   | 0x0   |
| 14    | PLL_SYS     | RO   | 0x0   |
| 13    | PIO2        | RO   | 0x0   |
| 12    | PIO1        | RO   | 0x0   |
| 11    | PIO0        | RO   | 0x0   |
| 10    | PADS_QSPI   | RO   | 0x0   |
| 9     | PADS_BANK0  | RO   | 0x0   |
| 8     | JTAG        | RO   | 0x0   |
| 7     | IO_QSPI     | RO   | 0x0   |
| 6     | IO_BANK0    | RO   | 0x0   |
| 5     | I2C1        | RO   | 0x0   |
| 4     | I2C0        | RO   | 0x0   |
| 3     | HSTX        | RO   | 0x0   |
| 2     | DMA         | RO   | 0x0   |
| 1     | BUSCTRL     | RO   | 0x0   |
| 0     | ADC         | RO   | 0x0   |

