# 10.5 Secure Boot Enable Procedure

To enable secure boot:

- 1. Program at least one public key fingerprint into OTP, starting at [BOOTKEY0\\_0](#page-1315-1).
- 2. Mark programmed keys as valid by programming [BOOT\\_FLAGS1.](#page-1306-0)KEY\_VALID.
- 3. Optionally, mark unused keys as invalid by programming [BOOT\\_FLAGS1](#page-1306-0).KEY\_INVALID this is recommended to prevent a malicious actor installing their own boot keys at a later date.
  - KEY\_INVALID takes precedence over KEY\_VALID, which prevents more keys from being added later.
  - Program KEY\_INVALID with additional bits to revoke keys at a later time.
- 4. Disable debugging by programming [CRIT1.](#page-1304-0)DEBUG\_DISABLE, [CRIT1.](#page-1304-0)SECURE\_DEBUG\_DISABLE, or installing a debug key ([Section 3.5.9.2\)](#page-92-1).
- 5. Optionally, enable the glitch detector ([Section 10.9\)](#page-866-1) by programming [CRIT1.](#page-1304-0)GLITCH\_DETECTOR\_ENABLE and setting the desired sensitivity in [CRIT1](#page-1304-0).GLITCH\_DETECTOR\_SENS.
- 6. Disable unused boot options such as USB and UART boot in [BOOT\\_FLAGS0](#page-1304-1).
- 7. Enable secure boot, by programming [CRIT1](#page-1304-0).SECURE\_BOOT\_ENABLE.

#### **WARNING**

*This procedure is irreversible.* Before programming, ensure that you are using the correct public key, correctly hashed. picotool supports programming keys into OTP from standard PEM files, performing the fingerprint hashing automatically. Programming the wrong key will make it impossible to run code on your device.

