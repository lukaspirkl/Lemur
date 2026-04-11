# 8.2.4 Startup Delay

The STARTUP\_DELAY register specifies how many clock cycles must be seen from the crystal before it can be used. This is specified in multiples of 256. The SDK xosc\_init function sets this value. The 1 ms default is sufficient for the RP2350 reference design (see [Hardware design with RP2350,](https://datasheets.raspberrypi.com/rp2350/hardware-design-with-rp2350.pdf) [Minimal Design Example](https://datasheets.raspberrypi.com/rp2350/hardware-design-with-rp2350.pdf#minimal-design-example)) which runs the XOSC at 12 MHz. When the timer expires, the STATUS\_STABLE flag will be set to indicate the XOSC output can be used.

Before starting the XOSC the programmer must ensure the STARTUP\_DELAY register is correctly configured. The required value can be calculated by:

So with a 12 MHz crystal and a 1 ms wait time, the calculation is:

![](_page_554_Figure_11.jpeg)

The value is rounded up to the nearest integer, so the wait time will be just over 1 ms.

#### <span id="page-554-2"></span>**8.2.5. XOSC Counter**

The COUNT register provides a method of managing short software delays. To use this method:

- 1. Write a value to the COUNT register. The register automatically begins to count down to zero at the XOSC frequency.
- 2. Poll the register until it reaches zero.

This is preferable to using NOPs in software loops because it is independent of the core clock frequency, the compiler, and the execution time of the compiled code.

#### <span id="page-554-3"></span>**8.2.6. DORMANT mode**

In DORMANT mode (see [Section 6.5.3, "DORMANT State"](#page-486-2)), all of the on-chip clocks can be paused to save power. This is particularly useful in battery-powered applications. RP2350 wakes from DORMANT mode by interrupt: either from an external event, such as an edge on a GPIO pin, or from the AON Timer. This must be configured before entering DORMANT mode. To use the AON Timer to trigger a wake from DORMANT mode, it must be clocked from the LPOSC or from an external source.

To enter DORMANT mode:

1. Switch all internal clocks to be driven from XOSC or ROSC and stop the PLLs.

2. Choose an oscillator (XOSC or ROSC). Write a specific 32-bit value to the DORMANT register of the chosen oscillator to stop it.

When exiting DORMANT mode, the chosen oscillator will restart. If you chose XOSC, the frequency will be more precise, but the restart will take more time due to startup delay (>1 ms on the RP2350 reference design (see [Hardware design](https://datasheets.raspberrypi.com/rp2350/hardware-design-with-rp2350.pdf) [with RP2350](https://datasheets.raspberrypi.com/rp2350/hardware-design-with-rp2350.pdf), [Minimal Design Example\)](https://datasheets.raspberrypi.com/rp2350/hardware-design-with-rp2350.pdf#minimal-design-example)). If you chose ROSC, the frequency will be less precise, but the start-up time is very short (approximately 1μs). See [Section 6.5.3.1, "Waking from the DORMANT State"](#page-487-2) for the events which cause the system to exit DORMANT mode.

# **NOTE**

You must stop the PLLs before entering DORMANT mode.

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_xosc/xosc.c](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_xosc/xosc.c#L56-L63) Lines 56 - 63*

```
56 void xosc_dormant(void) {
57 // WARNING: This stops the xosc until woken up by an irq
58 xosc_hw->dormant = XOSC_DORMANT_VALUE_DORMANT;
59 // Wait for it to become stable once woken up
60 while(!(xosc_hw->status & XOSC_STATUS_STABLE_BITS)) {
61 tight_loop_contents();
62 }
63 }
```

## **WARNING**

If you do not configure IRQ before entering DORMANT mode, neither oscillator will restart.

See [Section 6.5.6.2, "DORMANT"](#page-489-0) for a complete example of DORMANT mode using the XOSC.

