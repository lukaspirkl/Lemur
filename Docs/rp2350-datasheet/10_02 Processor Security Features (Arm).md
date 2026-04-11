# 10.2 Processor Security Features (Arm)

The Cortex-M33 processors on RP2350 are configured with the following standard Arm security features:

- Support for the Armv8-M Security extension
- 8× security attribution unit (SAU) regions
- 8× Secure and 8× Non-secure memory protection unit (MPU) regions

These features are covered exhaustively in the [Armv8-M Architecture Reference Manual](https://developer.arm.com/documentation/ddi0553/latest/), the Cortex-M33 Technical Reference Manual, and the Cortex-M33 section of this datasheet ([Section 3.7\)](#page-123-1). This section gives a high-level overview of these features, as well as a description of the implementation-defined attribution unit included in RP2350.

