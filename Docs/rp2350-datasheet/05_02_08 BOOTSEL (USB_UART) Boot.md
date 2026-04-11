# 5.2.8 BOOTSEL (USB/UART) Boot

The bootrom samples the state of QSPI CSn shortly after reset. Based on the result, the bootrom decides whether to enter BOOTSEL mode, which refers collectively to the USB and UART bootloaders.

The bootrom initialises the chip select to the following state:

- Output disabled
- Pulled high (note CSn is an active-low signal, so this deselects the external QSPI device if there is one)

If the chip select remains high, the bootrom continues with its normal, non-BOOTSEL sequence. By default on a blank device, this means driving the chip select low and attempting to boot from an external flash or PSRAM device.

If chip select is driven low externally, the bootrom enters BOOTSEL mode. You must drive the chip select low with a sufficiently low impedance to overcome the internal pull-up. A 4.7 kΩ resistance to ground is a good intermediate value which reliably creates a low input logic level, but will not affect the output levels when RP2350 drives the chip select.

The QSPI SD1 line, which RP2350 initially pulls low, selects which bootloader to enter:

- SD1 remains pulled low: enter USB bootloader
- SD1 driven high: enter UART bootloader

USB boot is a low-friction method for programming an RP2350 from a sophisticated host like a Linux PC. It also directly exposes more advanced options like OTP programming. See [Section 5.5](#page-398-0) for the drag-and-drop mass storage interface, or [Section 5.6](#page-402-0) for the PICOBOOT vendor interface.

UART boot is a minimal interface for bootstrapping a flashless RP2350 from another microcontroller. UART boot uses QSPI SD2 for UART TX, and QSPI SD3 for UART RX, at a fixed baud rate of 1 Mbaud. For more details about UART boot, see [Section 5.8](#page-414-0).

#### <span id="page-374-1"></span>**5.2.8.1. BOOTSEL Clock Requirements**

BOOTSEL mode requires either a crystal attached across the XIN and XOUT pins, or a clock signal from an external oscillator driven into the XIN pin. See [Table 1438](#page-1337-0) for the electrical specifications of these two XOSC pins.

The bootrom assumes a default XOSC frequency of 12 MHz. It configures the USB PLL to derive a fixed 48 MHz frequency from the XOSC reference. For USB, this must be a precise frequency. If you use a non-12 MHz crystal, and intend to use USB boot, program [BOOTSEL\\_PLL\\_CFG](#page-1310-1) and [BOOTSEL\\_XOSC\\_CFG](#page-1311-0) in OTP, and then set [BOOT\\_FLAGS0.](#page-1304-1)ENABLE\_BOOTSEL\_NON\_DEFAULT\_PLL\_XOSC\_CFG. For details about calculating the correct PLL parameters for your crystal, see [Section 8.6.3](#page-573-1).

UART boot uses the same PLL configuration as USB boot. However, the permissible range of crystal frequencies under the default PLL configuration is wider. See [Section 5.8.1.](#page-414-1)

