# 8.3.3 ROSC/XOSC trade-offs

The ROSC has several advantages:

- flexibility due to programmable frequency
- low power requirements
- no need for internal or external components
- optional frequency randomisation improves security

Because the ROSC has programmable frequency, it can provide a fast core clock without starting the PLLs and can generate slower peripheral clocks by dividing by clock generators ([Section 8.1, "Overview"](#page-510-1)). The ROSC starts immediately and responds immediately to frequency controls. It retains the frequency setting when entering and exiting the DORMANT state (see [Section 6.5.3, "DORMANT State"\)](#page-486-2). However, the user must be aware that the frequency may have drifted when exiting the DORMANT state due to changes in the supply voltage and the chip temperature.

The disadvantage of the ROSC is its frequency variation with PVT (Process, Voltage, and Temperature) which makes it unsuitable for generating precise clocks or for applications where software execution timing is important. However, the PVT frequency variation can be exploited to provide automatic frequency scaling to maximise performance. This is discussed in [Section 8.1, "Overview".](#page-510-1)

The only advantage of the XOSC is its accurate frequency, but this is an overriding requirement in many applications.

The XOSC has the following disadvantages:

• the requirement for external components (a crystal, etc.)

- higher power consumption
- slow startup time (>1ms)
- fixed, low frequency

PLLs are required to produce higher-frequency clocks. They consume more power and take significant time to start up or change frequency. Exiting DORMANT mode is much slower than for ROSC because the XOSC must restart and the PLLs must be reconfigured.

