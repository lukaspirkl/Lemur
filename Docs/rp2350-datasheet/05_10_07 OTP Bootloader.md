# 5.10.7 OTP Bootloader

This is similar to the custom bootloader scenario, but it will be stored in the OTP and will run in SRAM.

One possible use case could place decryption code into OTP which decrypts an executable image from a flash partition into RAM.

The entire bootloader will need to fit in the OTP rows from 0x0C0 to 0xF48 to avoid interfering with other reserved OTP functionality, giving a maximum size of 7440 bytes (2 bytes per ECC row). If some boot keys and OTP keys are unused, this region can extend slightly on either end.

The OTP bootloader itself should be stored in ECC format, starting from the row set in [OTPBOOT\\_SRC](#page-1314-0) with size set in [OTPBOOT\\_LEN](#page-1314-2). When booting, it will be loaded into the address specified in [OTPBOOT\\_DST0](#page-1314-1) and [OTPBOOT\\_DST1,](#page-1315-2) which must be in the main SRAM. The bootloader must fulfil the same criteria as a standard image: it must include an IMAGE\_DEF, which must be signed if secure boot is enabled.

Once the OTP bootloader has been written to OTP, and the [OTPBOOT\\_SRC](#page-1314-0), [OTPBOOT\\_LEN,](#page-1314-2) [OTPBOOT\\_DST0](#page-1314-1) and [OTPBOOT\\_DST1](#page-1315-2) set, OTP booting can be enabled by setting [BOOT\\_FLAGS0](#page-1304-1).ENABLE\_OTP\_BOOT. If the OTP image fails the bootrom's launch checks, then, by default, boot continues along the normal flash boot path. You can prevent this by setting [BOOT\\_FLAGS0](#page-1304-1).DISABLE\_FLASH\_BOOT.

# **WARNING**

Take extreme care when writing an OTP bootloader. Once the ECC rows are written, they *cannot* be modified.

