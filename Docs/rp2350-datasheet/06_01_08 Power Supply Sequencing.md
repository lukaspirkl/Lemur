# 6.1.8 Power Supply Sequencing

With the exception of the two voltage regulator supplies (VREG\_VIN and VREG\_AVDD), which should be powered up together, RP2350's power supplies may be powered up or down in any order. However, small transient currents may flow in the ADC supply (ADC\_AVDD) if it is powered up before, or powered down after, the digital core supply (DVDD). This will not damage the chip, but can be avoided by powering up DVDD before or at the same time as ADC\_AVDD, and powering down DVDD after or at the same time as ADC\_AVDD. In the most common power supply scheme, where the chip is powered from a single 3.3 V supply, DVDD will be powered up shortly after ADC\_AVDD due to the startup time of the on-chip voltage regulator. This is acceptable behaviour.

