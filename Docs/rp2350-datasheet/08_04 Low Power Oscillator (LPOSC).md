# 8.4 Low Power Oscillator (LPOSC)

The Low Power Oscillator (LPOSC) provides a clock signal to the always-on logic when the main crystal oscillator is powered down in a low power (P1.x) state. It operates at a nominal 32.768kHz and is an RC oscillator, requiring no external components. The oscillator's output clock is used to sequence initial chip start up and transition to and from low-power states. It can also be used by the AON Timer, see [Section 12.10, "Always-On Timer".](#page-1194-0)

The oscillator starts up as soon as the core power supply is available and power-on reset has been released. If brownout detection is enabled, the oscillator will be disabled when a core supply brownout is detected, but will restart as soon as the core supply has recovered and brownout reset has been released. The oscillator's frequency takes around 1ms to stabilise, and the chip will be held in reset during this period.

