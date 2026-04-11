# 8.2.2 Changes from RP2040

• Maximum crystal frequency increased from 15 MHz to 50 MHz, when appropriate range is selected in [CTRL](#page-556-1).FREQ\_RANGE

![](_page_553_Figure_12.jpeg)

The above change applies when using the XOSC as a crystal oscillator, with a crystal connected between the XIN and XOUT pins. When using the XOSC XIN pin as a CMOS clock input from an external oscillator, the maximum is always 50 MHz. You do not have to configure [CTRL.](#page-556-1)FREQ\_RANGE for the CMOS input case. The CMOS input behaviour is the same as RP2040.

# **NOTE**

The maximum clk\_ref frequency is 25 MHz. If you use a >25 MHz crystal as the source of clk\_ref, you must divide the XOSC output using the clk\_ref divider.

