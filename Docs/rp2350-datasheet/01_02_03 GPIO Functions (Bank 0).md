# 1.2.3 GPIO Functions (Bank 0)

Each individual GPIO pin can be connected to an internal peripheral via the GPIO functions defined below. Some internal peripheral connections appear in multiple places to allow some system level flexibility. SIO, PIO0, PIO1 and PIO2 can connect to all GPIO pins and are controlled by software (or software controlled state machines) so can be used to implement many functions.

*Table 3. General Purpose Input/Output (GPIO) Bank 0 Functions*

| GPIO | F0   | F1       | F2        | F3       | F4     | F5  | F6   | F7   | F8   | F9           | F10           | F11      |
|------|------|----------|-----------|----------|--------|-----|------|------|------|--------------|---------------|----------|
| 0    |      | SPI0 RX  | UART0 TX  | I2C0 SDA | PWM0 A | SIO | PIO0 | PIO1 | PIO2 | QMI CS1n     | USB OVCUR DET |          |
| 1    |      | SPI0 CSn | UART0 RX  | I2C0 SCL | PWM0 B | SIO | PIO0 | PIO1 | PIO2 | TRACECLK     | USB VBUS DET  |          |
| 2    |      | SPI0 SCK | UART0 CTS | I2C1 SDA | PWM1 A | SIO | PIO0 | PIO1 | PIO2 | TRACEDATA0   | USB VBUS EN   | UART0 TX |
| 3    |      | SPI0 TX  | UART0 RTS | I2C1 SCL | PWM1 B | SIO | PIO0 | PIO1 | PIO2 | TRACEDATA1   | USB OVCUR DET | UART0 RX |
| 4    |      | SPI0 RX  | UART1 TX  | I2C0 SDA | PWM2 A | SIO | PIO0 | PIO1 | PIO2 | TRACEDATA2   | USB VBUS DET  |          |
| 5    |      | SPI0 CSn | UART1 RX  | I2C0 SCL | PWM2 B | SIO | PIO0 | PIO1 | PIO2 | TRACEDATA3   | USB VBUS EN   |          |
| 6    |      | SPI0 SCK | UART1 CTS | I2C1 SDA | PWM3 A | SIO | PIO0 | PIO1 | PIO2 |              | USB OVCUR DET | UART1 TX |
| 7    |      | SPI0 TX  | UART1 RTS | I2C1 SCL | PWM3 B | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS DET  | UART1 RX |
| 8    |      | SPI1 RX  | UART1 TX  | I2C0 SDA | PWM4 A | SIO | PIO0 | PIO1 | PIO2 | QMI CS1n     | USB VBUS EN   |          |
| 9    |      | SPI1 CSn | UART1 RX  | I2C0 SCL | PWM4 B | SIO | PIO0 | PIO1 | PIO2 |              | USB OVCUR DET |          |
| 10   |      | SPI1 SCK | UART1 CTS | I2C1 SDA | PWM5 A | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS DET  | UART1 TX |
| 11   |      | SPI1 TX  | UART1 RTS | I2C1 SCL | PWM5 B | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS EN   | UART1 RX |
| 12   | HSTX | SPI1 RX  | UART0 TX  | I2C0 SDA | PWM6 A | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPIN0  | USB OVCUR DET |          |
| 13   | HSTX | SPI1 CSn | UART0 RX  | I2C0 SCL | PWM6 B | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPOUT0 | USB VBUS DET  |          |
| 14   | HSTX | SPI1 SCK | UART0 CTS | I2C1 SDA | PWM7 A | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPIN1  | USB VBUS EN   | UART0 TX |
| 15   | HSTX | SPI1 TX  | UART0 RTS | I2C1 SCL | PWM7 B | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPOUT1 | USB OVCUR DET | UART0 RX |
| 16   | HSTX | SPI0 RX  | UART0 TX  | I2C0 SDA | PWM0 A | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS DET  |          |
| 17   | HSTX | SPI0 CSn | UART0 RX  | I2C0 SCL | PWM0 B | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS EN   |          |
| 18   | HSTX | SPI0 SCK | UART0 CTS | I2C1 SDA | PWM1 A | SIO | PIO0 | PIO1 | PIO2 |              | USB OVCUR DET | UART0 TX |
| 19   | HSTX | SPI0 TX  | UART0 RTS | I2C1 SCL | PWM1 B | SIO | PIO0 | PIO1 | PIO2 | QMI CS1n     | USB VBUS DET  | UART0 RX |
| 20   |      | SPI0 RX  | UART1 TX  | I2C0 SDA | PWM2 A | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPIN0  | USB VBUS EN   |          |
| 21   |      | SPI0 CSn | UART1 RX  | I2C0 SCL | PWM2 B | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPOUT0 | USB OVCUR DET |          |
| 22   |      | SPI0 SCK | UART1 CTS | I2C1 SDA | PWM3 A | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPIN1  | USB VBUS DET  | UART1 TX |

| 23 | SPI0 TX                              | UART1 RTS | I2C1 SCL | PWM3 B  | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPOUT1 | USB VBUS EN   | UART1 RX |
|----|--------------------------------------|-----------|----------|---------|-----|------|------|------|--------------|---------------|----------|
| 24 | SPI1 RX                              | UART1 TX  | I2C0 SDA | PWM4 A  | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPOUT2 | USB OVCUR DET |          |
| 25 | SPI1 CSn                             | UART1 RX  | I2C0 SCL | PWM4 B  | SIO | PIO0 | PIO1 | PIO2 | CLOCK GPOUT3 | USB VBUS DET  |          |
| 26 | SPI1 SCK                             | UART1 CTS | I2C1 SDA | PWM5 A  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS EN   | UART1 TX |
| 27 | SPI1 TX                              | UART1 RTS | I2C1 SCL | PWM5 B  | SIO | PIO0 | PIO1 | PIO2 |              | USB OVCUR DET | UART1 RX |
| 28 | SPI1 RX                              | UART0 TX  | I2C0 SDA | PWM6 A  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS DET  |          |
| 29 | SPI1 CSn                             | UART0 RX  | I2C0 SCL | PWM6 B  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS EN   |          |
|    | GPIOs 30 through 47 are QFN-80 only: |           |          |         |     |      |      |      |              |               |          |
| 30 | SPI1 SCK                             | UART0 CTS | I2C1 SDA | PWM7 A  | SIO | PIO0 | PIO1 | PIO2 |              | USB OVCUR DET | UART0 TX |
| 31 | SPI1 TX                              | UART0 RTS | I2C1 SCL | PWM7 B  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS DET  | UART0 RX |
| 32 | SPI0 RX                              | UART0 TX  | I2C0 SDA | PWM8 A  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS EN   |          |
| 33 | SPI0 CSn                             | UART0 RX  | I2C0 SCL | PWM8 B  | SIO | PIO0 | PIO1 | PIO2 |              | USB OVCUR DET |          |
| 34 | SPI0 SCK                             | UART0 CTS | I2C1 SDA | PWM9 A  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS DET  | UART0 TX |
| 35 | SPI0 TX                              | UART0 RTS | I2C1 SCL | PWM9 B  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS EN   | UART0 RX |
| 36 | SPI0 RX                              | UART1 TX  | I2C0 SDA | PWM10 A | SIO | PIO0 | PIO1 | PIO2 |              | USB OVCUR DET |          |
| 37 | SPI0 CSn                             | UART1 RX  | I2C0 SCL | PWM10 B | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS DET  |          |
| 38 | SPI0 SCK                             | UART1 CTS | I2C1 SDA | PWM11 A | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS EN   | UART1 TX |
| 39 | SPI0 TX                              | UART1 RTS | I2C1 SCL | PWM11 B | SIO | PIO0 | PIO1 | PIO2 |              | USB OVCUR DET | UART1 RX |
| 40 | SPI1 RX                              | UART1 TX  | I2C0 SDA | PWM8 A  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS DET  |          |
| 41 | SPI1 CSn                             | UART1 RX  | I2C0 SCL | PWM8 B  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS EN   |          |
| 42 | SPI1 SCK                             | UART1 CTS | I2C1 SDA | PWM9 A  | SIO | PIO0 | PIO1 | PIO2 |              | USB OVCUR DET | UART1 TX |
| 43 | SPI1 TX                              | UART1 RTS | I2C1 SCL | PWM9 B  | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS DET  | UART1 RX |
| 44 | SPI1 RX                              | UART0 TX  | I2C0 SDA | PWM10 A | SIO | PIO0 | PIO1 | PIO2 |              | USB VBUS EN   |          |

**GPIO F0 F1 F2 F3 F4 F5 F6 F7 F8 F9 F10 F11**

| GPIO | F0 | F1       | F2        | F3       | F4      | F5  | F6   | F7   | F8   | F9       | F10           | F11      |
|------|----|----------|-----------|----------|---------|-----|------|------|------|----------|---------------|----------|
| 45   |    | SPI1 CSn | UART0 RX  | I2C0 SCL | PWM10 B | SIO | PIO0 | PIO1 | PIO2 |          | USB OVCUR DET |          |
| 46   |    | SPI1 SCK | UART0 CTS | I2C1 SDA | PWM11 A | SIO | PIO0 | PIO1 | PIO2 |          | USB VBUS DET  | UART0 TX |
| 47   |    | SPI1 TX  | UART0 RTS | I2C1 SCL | PWM11 B | SIO | PIO0 | PIO1 | PIO2 | QMI CS1n | USB VBUS EN   | UART0 RX |
|      |    |          |           |          |         |     |      |      |      |          |               |          |

*Table 4. GPIO bank 0 function descriptions*

| Function Name                     | Description                                                                                                                                                                                                                                                                                                                                                                                         |
|-----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| SPIx                              | Connect one of the internal PL022 SPI peripherals to GPIO                                                                                                                                                                                                                                                                                                                                           |
| UARTx                             | Connect one of the internal PL011 UART peripherals to GPIO                                                                                                                                                                                                                                                                                                                                          |
| I2Cx                              | Connect one of the internal DW I2C peripherals to GPIO                                                                                                                                                                                                                                                                                                                                              |
| PWMx A/B                          | Connect a PWM slice to GPIO. There are twelve PWM slices, each with two output<br>channels (A/B). The B pin can also be used as an input, for frequency and duty cycle<br>measurement.                                                                                                                                                                                                              |
| SIO                               | Software control of GPIO, from the single-cycle IO (SIO) block. The SIO function (F5)<br>must be selected for the processors to drive a GPIO, but the input is always connected,<br>so software can check the state of GPIOs at any time.                                                                                                                                                           |
| PIOx                              | Connect one of the programmable IO blocks (PIO) to GPIO. PIO can implement a wide<br>variety of interfaces, and has its own internal pin mapping hardware, allowing flexible<br>placement of digital interfaces on bank 0 GPIOs. The PIO function (F6, F7, F8) must be<br>selected for PIO to drive a GPIO, but the input is always connected, so the PIOs can<br>always see the state of all pins. |
| HSTX                              | Connect the high-speed transmit peripheral (HSTX) to GPIO                                                                                                                                                                                                                                                                                                                                           |
| CLOCK GPINx                       | General purpose clock inputs. Can be routed to a number of internal clock domains on<br>RP2350, e.g. to provide a 1Hz clock for the AON Timer, or can be connected to an<br>internal frequency counter.                                                                                                                                                                                             |
| CLOCK GPOUTx                      | General purpose clock outputs. Can drive a number of internal clocks (including PLL<br>outputs) onto GPIOs, with optional integer divide.                                                                                                                                                                                                                                                           |
| TRACECLK, TRACEDATAx              | CoreSight TPIU execution trace output from Cortex-M33 processors (Arm-only)                                                                                                                                                                                                                                                                                                                         |
| USB OVCUR DET/VBUS<br>DET/VBUS EN | USB power control signals to/from the internal USB controller                                                                                                                                                                                                                                                                                                                                       |
| QMI CS1n                          | Auxiliary chip select for QSPI bus, to allow execute-in-place from an additional flash or<br>PSRAM device                                                                                                                                                                                                                                                                                           |

# **NOTE**

GPIOs 0 through 29 are available in all package variants. GPIOs 30 through 47 are available only in QFN-80 (RP2350B) package.

# **NOTE**

Analogue input is available on GPIOs 26 through 29 in the QFN-60 package (RP2350A), for a total of four inputs, and on GPIOs 40 through 47 in the QFN-80 package (RP2350B), for a total of eight inputs.

