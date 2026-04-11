# 7.5.1 Overview

The reset controller allows software to reset non-critical components in RP2350. The reset controller can reset the following components:

- USB Controller
- PIO
- Peripherals, including UART, I2C, SPI, PWM, Timer, ADC
- PLLs
- IO and Pad registers

For a full list of components that can be reset using the reset controller, see the register descriptions ([Section 7.5.3,](#page-502-0) ["List of Registers"](#page-502-0)).

When reset, components are held in reset at power-up. To use the component, software must deassert the reset.

![](_page_500_Figure_11.jpeg)

The SDK automatically deasserts some components after a reset.

