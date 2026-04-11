# 5.8.2 UART Boot Shell Protocol

After the bootrom samples QSPI CSn and SD1, there will be a delay of several milliseconds as the bootrom goes through some necessary steps such as switching from the ring oscillator to the PLL, and erasing SRAM before releasing it to the Non-secure UART bootloader.

The UART bootloader signals it is ready to begin by printing the ASCII splash string RP2350. In bytes, this is 0x52, 0x50, 0x32, 0x33, 0x35, 0x30.

Before sending any commands, you must send a special knock sequence to unlock the interface. This is a measure to avoid transient effects due to noise on GPIOs and ensure the host and device are initially well-synchronised. The sequence is: 0x56, 0xff, 0x8b, 0xe4. This is the RP2040 UF2 family ID, chosen as a well-known magic number. Any sequence of bytes ending with this four-byte sequence is detected.

A UART boot shell command is always in the host-to-device direction (RP2350 receives), and consists of a single command byte, optionally followed by a 32-byte write payload. RP2350 responds with an optional 32-byte read payload followed by an echo of the command byte. You should wait for the command echo before sending the next command.

The supported commands are:

| Command<br>(ASCII) | Command<br>(hex) | Description                                                                                                                                                                                    |
|--------------------|------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| n                  | 0x6e             | No-op. Do nothing, and report back when you've done it. Used to ping the interface when<br>recovering lost synchronisation. Echoes the command byte, 'n'.                                      |
| w                  | 0x77             | Write a 32-byte payload to the current value of the read/write pointer. Increment the address<br>pointer by 32. Echoes the command byte, 'w', once all 32 bytes are written to memory.         |
| r                  | 0x72             | Read a 32-byte payload from the current value of the read/write pointer. Increment the<br>address pointer by 32. Echoes the command byte, 'r', after transmitting the 32-byte read<br>payload. |

5.8. UART Boot **414**

| Command<br>(ASCII) | Command<br>(hex) | Description                                                                                                                                                                                                                                                               |
|--------------------|------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| c                  | 0x63             | Clear the read/write pointer. The pointer resets to the first location in SRAM 0x20000000, and<br>you can begin a new read or write sequence from there. Echoes the command byte, 'c'.                                                                                    |
| x                  | 0x78             | Execute the payload that has been written to memory. Echoes the command byte, 'x', and<br>then reboots, passing a RAM boot search window spanning all of main SRAM. If a valid binary<br>was successfully written into SRAM before sending this command, it will execute. |

Unrecognised commands are echoed with no other effect. More commands may be added in future versions.

