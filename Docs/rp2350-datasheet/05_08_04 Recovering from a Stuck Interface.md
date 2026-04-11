# 5.8.4 Recovering from a Stuck Interface

Noise on the GPIOs may cause the UART boot shell to stop replying to commands, for example because it thinks the host is part way through a write payload, and the host thinks that it is not. To resynchronise to the start of the next command:

- 1. Wait 1 ms for the link to quiesce
- 2. Send 33 'n' NOP commands (size of longest command)
- 3. Wait 1 ms and flush your receive data
- 4. Send 1 'n' NOP command and confirm the device responds with an echoed NOP

If the interface fails to recover, reboot the device and try again. Failure may be caused by:

- Noise on GPIOs (particularly over long traces or wires)
- Incorrect baud rate matching
- An unstable frequency reference on XOSC XIN
- Mismatch of voltage levels (for example a QSPI\_IOVDD of 1.8 V on RP2350, and a 3.3 V IO voltage on the host)

5.8. UART Boot **415**

### <span id="page-416-0"></span>**5.8.5. Requirements for UART Boot Binaries**

A UART boot binary is a normal RAM binary. It must have a valid IMAGE\_DEF in order for the boot path to recognise it as a bootable binary. The search window for the IMAGE\_DEF is the whole of SRAM, but it's recommended to place it close to the beginning, because the bootrom searches linearly forward for the beginning of the IMAGE\_DEF.

The maximum size for a UART boot binary is the entirety of main SRAM: 520 kB, or 532 480 bytes.

UART boot only supports loading to the start of SRAM, so your binary must be linked to run at address 0x20000000. Sparse loading is not supported: your program must load as a single flat binary image.

All security requirements relating to RAM image boot apply to UART boot too. If secure boot is enabled, your binary must be signed. Likewise, if OTP anti-rollback versioning is in effect, your binary's rollback version must be no lower than the version number stored in OTP.

