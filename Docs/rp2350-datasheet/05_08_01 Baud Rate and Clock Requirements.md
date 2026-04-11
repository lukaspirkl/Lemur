# 5.8.1 Baud Rate and Clock Requirements

The nominal baud rate for UART boot is 1 Mbaud, divided from a nominal 48 MHz system clock frequency. UART boot uses the USB PLL to derive the system clock and UART baud clock, so you must either provide a crystal or drive a stable clock into the crystal oscillator XIN pad. The host baud rate must match the RP2350 baud rate within 3%.

By default the crystal is assumed to be 12 MHz, but the [BOOTSEL\\_PLL\\_CFG](#page-1310-1) and [BOOTSEL\\_XOSC\\_CFG](#page-1311-0) OTP locations override this to achieve a nominal 48 MHz system clock from any supported crystal. The same OTP configuration is used for both USB and UART boot.

![](_page_414_Figure_8.jpeg)

You may drive a somewhat faster or slower clock into XIN without any OTP configuration, if you scale your UART baud rate appropriately. The permissible range is 7.5 to 16 MHz on XIN, limited by the PLL VCO frequency range.

