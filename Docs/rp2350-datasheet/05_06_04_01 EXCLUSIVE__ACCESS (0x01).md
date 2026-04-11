# 5.6.4.1 EXCLUSIVE\_ACCESS (0x01)

Claim or release exclusive access for writing to the RP2350 over USB (versus the Mass Storage Interface)

*Table 458. PICOBOOT EXCLUSIVE\_ACCESS command structure*

| Offset | Name            | Value / Description     |                                                                                                                                                     |
|--------|-----------------|-------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x08   | bCmdId          | 0x01 (EXCLUSIVE_ACCESS) |                                                                                                                                                     |
| 0x09   | bCmdSize        | 0x01                    |                                                                                                                                                     |
| 0x0c   | dTransferLength | 0x00000000              |                                                                                                                                                     |
| 0x10   | bExclusive      | NOT_EXCLUSIVE (0)       | No restriction on USB Mass Storage operation                                                                                                        |
|        |                 | EXCLUSIVE (1)           | Disable USB Mass Storage writes (the host should<br>see them as write protect failures, but in any case<br>any active UF2 download will be aborted) |
|        |                 | EXCLUSIVE_AND_EJECT (2) | Lock the USB Mass Storage Interface out by<br>marking the drive media as not present (ejecting<br>the drive)                                        |

#### **5.6.4.2. REBOOT (0x02)**

Not supported on RP2350.

Use [Section 5.6.4.10](#page-406-0) instead.

#### **5.6.4.3. FLASH\_ERASE (0x03)**

Erases a contiguous range of flash sectors.

*Table 459. PICOBOOT FLASH\_ERASE command structure*

| Offset | Name            | Value / Description                                                                             |
|--------|-----------------|-------------------------------------------------------------------------------------------------|
| 0x08   | bCmdId          | 0x03 (FLASH_ERASE)                                                                              |
| 0x09   | bCmdSize        | 0x08                                                                                            |
| 0x0c   | dTransferLength | 0x00000000                                                                                      |
| 0x10   | dAddr           | The address in flash to erase, starting at this location. This must be sector<br>(4 kB) aligned |

| Offset | Name  | Value / Description                                                                   |
|--------|-------|---------------------------------------------------------------------------------------|
| 0x14   | dSize | The number of bytes to erase. This must an exact multiple number of sectors<br>(4 kB) |

#### **5.6.4.4. READ (0x84)**

Read a contiguous memory (Flash or RAM or ROM) range from the RP2350

*Table 460. PICOBOOT Read memory command (Flash, RAM, ROM) structure*

| Offset | Name            | Value / Description                                     |
|--------|-----------------|---------------------------------------------------------|
| 0x08   | bCmdId          | 0x84 (READ)                                             |
| 0x09   | bCmdSize        | 0x08                                                    |
| 0x0c   | dTransferLength | Must be the same as dSize                               |
| 0x10   | dAddr           | The address to read from. May be in Flash or RAM or ROM |
| 0x14   | dSize           | The number of bytes to read                             |

#### **5.6.4.5. WRITE (0x05)**

Writes a contiguous memory range of memory (Flash or RAM) on the RP2350.

*Table 461. PICOBOOT Write memory command (Flash, RAM) structure*

| Offset | Name            | Value / Description                                                                                                                                                             |
|--------|-----------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x08   | bCmdId          | 0x05 (WRITE)                                                                                                                                                                    |
| 0x09   | bCmdSize        | 0x08                                                                                                                                                                            |
| 0x0c   | dTransferLength | Must be the same as dSize                                                                                                                                                       |
| 0x10   | dAddr           | The address to write from. May be in Flash or RAM, however must be page<br>(256 byte) aligned if in Flash. Note the flash must be erased first or the results<br>are undefined. |
| 0x14   | dSize           | The number of bytes to write. If writing to flash and the size is not an exact<br>multiple of pages (256 bytes) then the last page is zero-filled to the end.                   |

#### **5.6.4.6. EXIT\_XIP (0x06)**

A no-op provided for compatibility with RP2040. An XIP exit sequence ([flash\\_exit\\_xip\(\)](#page-385-1)) is issued once before entering the USB bootloader, which returns the external QSPI device from whatever XIP state it was in to a serial command state, and the external QSPI device then remains in this state until reboot.

*Table 462. PICOBOOT EXIT\_XIP command structure*

| Offset | Name            | Value / Description |
|--------|-----------------|---------------------|
| 0x08   | bCmdId          | 0x06 (EXIT_XIP)     |
| 0x09   | bCmdSize        | 0x00                |
| 0x0c   | dTransferLength | 0x00000000          |

#### **5.6.4.7. ENTER\_XIP (0x07)**

A no-op provided for compatibility with RP2040. Note that, unlike RP2040, the low-level bootrom flash operations do not leave the QSPI interface in a state where XIP is inaccessible, therefore there is no need to reinitialise the interface each time. XIP setup is performed once before entering the USB bootloader, using an 03h command with a fixed clock divisor

#### of 6.

*Table 463. PICOBOOT Enter Execute in place (XIP) command*

| Offset | Name            | Value / Description |
|--------|-----------------|---------------------|
| 0x08   | bCmdId          | 0x07 (ENTER_XIP)    |
| 0x09   | bCmdSize        | 0x00                |
| 0x0c   | dTransferLength | 0x00000000          |

#### **5.6.4.8. EXEC (0x08)**

Not supported on RP2350.

#### **5.6.4.9. VECTORIZE\_FLASH (0x09)**

Not supported on RP2350.

