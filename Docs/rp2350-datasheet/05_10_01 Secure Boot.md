# 5.10.1 Secure Boot

To enable secure boot on RP2350, you must:

- 1. Set the SHA-256 hashes of the boot keys you will be using in [BOOTKEY0\\_0](#page-1315-1) onwards
- 2. Set bits in [BOOT\\_FLAGS1.](#page-1306-0)KEY\_VALID for the keys you will be using
- 3. Optionally set bits in [BOOT\\_FLAGS1.](#page-1306-0)KEY\_INVALID for all unused keys this is recommended to prevent a malicious actor installing their own boot keys at a later date
- 4. Set [CRIT1](#page-1304-0).SECURE\_BOOT\_ENABLE to turn on secure boot.

#### **NOTE**

These steps are the minimum for enabling secure boot support *in the bootrom*. See [Section 10.5](#page-819-0) for additional steps you must take to fully secure your device, such as disabling hardware debug.

All of the above can be achieved with picotool. For example, when signing using picotool seal you can add an OTP JSON output file, to which it will add the relevant OTP field values to enable secure boot [\(BOOTKEY0\\_0,](#page-1315-1) [BOOT\\_FLAGS1.](#page-1306-0)KEY\_VALID and [CRIT1.](#page-1304-0)SECURE\_BOOT\_ENABLE):

```
$ picotool seal --sign unsigned.elf signed.elf private.pem /path/to/otp.json
```

To configure the SDK to output this OTP JSON file when signing, add the following command to your CMakeLists.txt:

```
pico_set_otp_key_output_file(target_name /path/to/otp.json)
```

You can then issue the following command to write this OTP JSON file to the device, thus enabling secure boot:

```
$ picotool otp load /path/to/otp.json
```

Once secure boot is enabled, the bootrom verifies signatures of images from all supported media: flash, OTP, and images preloaded into SRAM via the UART and USB bootloaders. At this point you lose the ability to run unsigned images; during development you may find it more convenient to leave secure boot disabled. The next section describes the generation of **signed images** to run on a secure-boot-enabled device.

