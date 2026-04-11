# 5.10.8 Rollback Versions And Bootloaders

#### **WARNING**

Ignoring the advice in this section could render your device unable to boot.

For bootloaders that need to chain into executable images with rollback versions on a secured RP2350, you *must* use separate OTP rows for:

- the bootloader rollback version
- the chained executable image's rollback version

Otherwise, bumping the version of the chained executable image renders the OTP bootloader and your device unable to boot.

You must also make sure that *both* the bootloader and the executable image have non-zero rollback versions, as the OTP flags relating to requiring rollback versions are global. Failure to do so will render your device unable to boot.

We recommend using the [DEFAULT\\_BOOT\\_VERSION0](#page-1307-0) and [DEFAULT\\_BOOT\\_VERSION1](#page-1308-1) rows for the binary's rollback version, and selecting some other unused rows in the OTP for the bootloader's rollback version.

# <span id="page-439-0"></span>**Chapter 6. Power**

