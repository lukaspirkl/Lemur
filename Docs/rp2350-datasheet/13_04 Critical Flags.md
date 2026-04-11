# 13.4 Critical Flags

Critical flags enable hardware security features which are fundamental to RP2350's secure boot implementation. The OTP power-up state machine reads critical flags very early in the system reset sequence, before any code runs on the processors.

Most critical flags are in the main Boot Configuration page, page 1. These are listed under [CRIT1](#page-1304-0) in the OTP data listing. The exceptions are the Arm/RISC-V disable flags, which are in the Chip Info page, page 0. This page is made read-only during factory programming, so users can not write to the [CRIT0](#page-1303-0) flags.

Critical flags define 0 as the unprogrammed value, and 1 as the programmed value. On a blank device, all of the [CRIT1](#page-1304-0) flags are 0. The *reset* value specified below is the value assigned to the internal logic net between the OTP reset being applied and the OTP PSM completing. For example, the reset value of 1 for the debug disable flags implies that debug is not accessible whilst the OTP PSM is running, but may be available afterward, depending on the value read from OTP storage.

- ARM\_DISABLE (*reset:* <sup>0</sup> ): Force the [ARCHSEL](#page-1285-1) register to RISC-V, at higher priority than RISC-V disable flag, secure boot enable flag, or default boot architecture flag.
- RISCV\_DISABLE (*reset:* <sup>0</sup>): Force the [ARCHSEL](#page-1285-1) register to Arm, at higher priority than the default boot architecture flag.
- SECURE\_BOOT\_ENABLE (*reset:* <sup>1</sup> ): Enable boot signature checking in bootrom, disable factory JTAG, and force the [ARCHSEL](#page-1285-1) register to Arm, at higher priority than the default boot architecture flag.
- SECURE\_DEBUG\_DISABLE (*reset:* <sup>1</sup> ): Disable factory JTAG, block Secure accesses from Mem-APs, and block halt requests to Secure processors.
  - Prevents secure AP accesses by masking their ap\_secure\_en signals.
  - Prevents secure processor halting by masking the Cortex-M33's SPIDEN and NSPIDEN signals.
  - Secure debug can be re-enabled by a Secure register in the OTP block.
  - Re-enable of Secure debug can be disabled by a Secure write-1-only lock register, also in the OTP block.

13.4. Critical Flags **1270**

- DEBUG\_DISABLE (*reset:* <sup>1</sup>): Completely disable the Mem-APs, in addition to disabling everything disabled by the secure debug disable flag.
- BOOT\_ARCH (*reset:* <sup>0</sup> ): set the reset value of the [ARCHSEL](#page-1285-1) register (0 → Arm, 1 → RISC-V) if it has not been forced by other critical flags.
  - Not critical, but hardware-read.
- GLITCH\_DETECTOR\_ENABLE (*reset:* <sup>0</sup>): pass an enable signal to the glitch detectors so that they can be armed before any software runs.
- GLITCH\_DETECTOR\_SENS(*reset*: 0): configure the initial sensitivity of the glitch detector circuits.

Critical flags are encoded with a three-of-eight vote across eight consecutive OTP rows. Each flag is redundantly programmed to the same bit position in eight consecutive rows. Hardware considers the flag to be set if the bit reads as 1 in at least three of these eight rows. The flag is considered clear if no more than two bits are observed to be set.

JTAG disable is ignored only if the customer RMA flag ([Section 13.7](#page-1275-1)) is set.

For further discussion of the effects of the critical flags, see:

- [Section 3.5.9.1](#page-91-1) for the effects of the debug disable flags
- [Section 3.9](#page-335-0) for the effects of the Arm/RISC-V architecture select flags
- [Section 10.9](#page-866-1) for the effects of the glitch detector configuration flags
- <span id="page-1271-0"></span>• [Section 10.1.1](#page-813-2) for discussion of the bootrom secure boot support enabled by the SECURE\_BOOT\_ENABLE flag

