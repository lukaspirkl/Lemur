# <span id="page-13-0"></span>**Chapter 1. Introduction**

RP2350 is a new family of microcontrollers from Raspberry Pi that offers significant enhancements over RP2040. Key features include:

- Dual Cortex-M33 or Hazard3 processors at 150 MHz
- 520 kB on-chip SRAM, in 10 independent banks
- 8 kB of one-time-programmable storage (OTP)
- Up to 16 MB of external QSPI flash/PSRAM via dedicated QSPI bus
  - Additional 16 MB flash/PSRAM accessible via optional second chip-select
- On-chip switched-mode power supply to generate core voltage
  - Low-quiescent-current LDO mode can be enabled for sleep states
- 2× on-chip PLLs for internal or external clock generation
- Security features:
  - Optional boot signing, enforced by on-chip mask ROM, with key fingerprint in OTP
  - Protected OTP storage for optional boot decryption key
  - Global bus filtering based on Arm or RISC-V security/privilege levels
  - Peripherals, GPIOs and DMA channels individually assignable to security domains
  - Hardware mitigations for fault injection attacks
  - Hardware SHA-256 accelerator
- Peripherals:
  - 2× UARTs
  - 2× SPI controllers
  - 2× I2C controllers
  - 24× PWM channels
  - USB 1.1 controller and PHY, with host and device support
  - 12× PIO state machines
  - 1× HSTX peripheral

The RP2350 family of devices is shown in table [Table 1](#page-13-1), showing options for QFN-80 (10 × 10 mm) and QFN-60 (7 × 7 mm) packages, with and without flash-in-package.

*Table 1. RP2350 device family*

<span id="page-13-1"></span>

| Product | Package | Internal Flash | GPIO | Analogue Inputs |
|---------|---------|----------------|------|-----------------|
| RP2350A | QFN-60  | None           | 30   | 4               |
| RP2350B | QFN-80  | None           | 48   | 8               |
| RP2354A | QFN-60  | 2 MB           | 30   | 4               |
| RP2354B | QFN-80  | 2 MB           | 48   | 8               |

Chapter 1. Introduction **13**

