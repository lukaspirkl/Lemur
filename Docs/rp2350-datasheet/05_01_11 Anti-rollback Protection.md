# 5.1.11 Anti-rollback Protection

**Anti-rollback** on a secured RP2350 prevents booting an older binary which may have known vulnerabilities. It prevents this even if the binary is correctly signed and meets all other requirements for bootability.

Full IMAGE\_DEF version information is of the form (rollback).major.minor, where the rollback part is optional. If a **rollback version** is present, it is accompanied by a list of OTP rows whose ordered values are used to form a **thermometer** of bits indicating the minimum rollback version that may run on the device.

A thermometer code is a base-1 (unary) number where the integer value is one plus the index of the most-significant set bit. For example, the bit string 00001111 encodes a value of four, and the all-zeroes bit pattern encodes a value of zero. The bootrom uses this encoding because:

- it allows OTP rows containing counters to be incremented, and
- it does not allow them to be decremented

On a secured RP2350, the bootrom compares the rollback version of the IMAGE\_DEF against the thermometer-coded minimum rollback version stored in OTP. If the IMAGE\_DEF value is lower, the bootrom refuses to boot the image.

The IMAGE\_DEF rollback version is covered by the image's signature, thus cannot be modified by an adversary who does not know the signing key. The list of OTP rows which define the chip's minimum rollback version is also stored in the program image, and also covered by the image signature.

The list of OTP rows in the IMAGE\_DEF must always have at least one bit spare beyond the IMAGE\_DEF's rollback version (enforced by picotool). As a result, older binaries always contain enough information for the bootrom to detect that the chip's minimum rollback version has been incremented past the rollback version in the IMAGE\_DEF. You can append more rows to the list on newer binaries to accommodate higher rollback versions without ambiguity.

When an executable image with a non-zero rollback version is successfully booted, its rollback version is written to the OTP thermometer. The [BOOT\\_FLAGS0.](#page-1304-1)ROLLBACK\_REQUIRED flag may be used to *require* an IMAGE\_DEF have a rollback version on a secured RP2350. This flag is set automatically when updating the rollback version in OTP.

![](_page_359_Figure_14.jpeg)

An IMAGE\_DEF with a rollback version of 0 will not automatically set the [BOOT\\_FLAGS0.](#page-1304-1)ROLLBACK\_REQUIRED flag, so it is recommended that the minimum rollback version used is 1, unless the [BOOT\\_FLAGS0](#page-1304-1).ROLLBACK\_REQUIRED flag is manually set during provisioning.

## <span id="page-359-1"></span>**5.1.12. Flash Image Boot**

RP2350 is designed primarily to run code from a QSPI flash device, either in-package or soldered separately to the circuit board. Code runs either in-place in flash, or in SRAM after being loaded from flash. **Flash boot** is the process of discovering that code and preparing to run it. **Flash image boot** uses a program binary stored directly in flash rather than in a [flash partition.](#page-354-2) Flash image boot requires the bootrom to discover a block loop starting within the first 4 kB of flash which contains a valid IMAGE\_DEF (and no PARTITION\_TABLE).

Flash image boot has no partition table, so it cannot be used with A/B version checking, which requires separate A/B partitions. The IMAGE\_DEF will boot if it is valid (which includes requiring a signature on a secured RP2350).

For the non-signed case, the IMAGE\_DEF can be as small as a 20-bytes; see [Section 5.9.5](#page-426-0).

![](_page_360_Picture_3.jpeg)

A more complicated version of this scenario stores multiple IMAGE\_DEFs in the block loop. In this case, the last IMAGE\_DEF for the current architecture is booted, if valid. You can use this to implement universal binaries for various supported architectures, or to include multiple signatures for targeting devices with different keys.

