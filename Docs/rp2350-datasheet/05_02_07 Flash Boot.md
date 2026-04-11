# 5.2.7 Flash Boot

The bootrom scans flash up to 16 times until it finds a valid IMAGE\_DEF or PARTITION\_TABLE. At this point, the flash settings are considered valid, and the flash boot proceeds if a valid bootable IMAGE\_DEF is found with these settings. It uses the following combinations of flash read instruction and SCK divisor for the 16 attempts:

*Table 451. QSPI read modes supported by the bootrom, in the order it attempts them.*

<span id="page-372-3"></span>

| Mode       | Clock Divisor |
|------------|---------------|
| EBh quad   | 3             |
| BBh dual   | 3             |
| 0Bh serial | 3             |
| 03h serial | 3             |
| EBh quad   | 6             |
| BBh dual   | 6             |
| 0Bh serial | 6             |

| Mode       | Clock Divisor |
|------------|---------------|
| 03h serial | 6             |
| EBh quad   | 12            |
| BBh dual   | 12            |
| 0Bh serial | 12            |
| 03h serial | 12            |
| EBh quad   | 24            |
| BBh dual   | 24            |
| 0Bh serial | 24            |
| 03h serial | 24            |

QSPI does not provide a reliable method to detect whether a device is attached. However, this is not much of an issue for boot purposes: either there is a device with valid and bootable contents, or there are no such contents (either due to lack of a connected device, invalid device contents, or failure to communicate in the current QSPI mode).

When there is no device (or no recognisable contents), the bootrom tries all 16 modes in [Table 451](#page-372-3) before finally giving up. The size of the initial search region is limited to 4 kB to minimise the time spent scanning flash before falling through to USB or UART boot. This same 4 kB limit also applies to search within a [flash partition](#page-354-2), which allows the bootrom to reliably sever the contained image's block loop with a single 4 kB sector erase at the start of a partition, such as on a [version downgrade](#page-361-0).

There are three main ways that the bootrom locates flash images:

#### **[Flash image boot](#page-359-1)**

A flash image can be written directly to flash storage address 0x0, and the bootrom will find it from there. This is the most similar to flash boot on RP2040 (the main differences being the removal of a boot2 in the first 256 bytes of the image, and the new requirement for a valid [image definition](#page-355-1) anywhere within the first 4 kB of the image).

#### **[Flash partition boot](#page-360-0)**

A flash image can be written into a partition of a [partition table](#page-354-2). The partition table is described by a PARTITION\_TABLE block stored at the start of flash. The bootrom finds the partition table and scans its partitions to look for bootable images.

#### **[Partition-table-in-image boot](#page-360-1)**

A flash image containing an IMAGE\_DEF and PARTITION\_TABLE block in a single [block loop](#page-356-1) is written to the start of flash. The bootrom loads the embedded partition table, and enters the image in the same way as the [flash image boot](#page-359-1) case.

Revisit the linked bootrom concepts sections to get the fullest understanding of each of these three forms of flash boot. For the purposes of this section, all that matters is whether the bootrom can discover a valid, bootable image or not. In all three cases, the image must have a valid IMAGE\_DEF, and meet all relevant security requirements such as being correctly [signed,](#page-357-2) and having a [rollback version](#page-359-0) greater than or equal to the one stored in OTP.

The bootrom enters the flash image in whatever QSPI mode it discovered to work during flash programming. Any further setup (such as prefixless continuous read modes) is performed by the flash image itself. This setup code, referred to as an **XIP setup function**, is usually copied into RAM before execution to avoid running from flash whilst the XIP interface is being reconfigured.

#### **TIP**

The PICO\_EMBED\_XIP\_SETUP=1 flag in the SDK enables inclusion and execution of an XIP setup function on RP2350 builds. In this case the function executes on the core 0 stack during early startup, so no additional static memory need be allocated. This is not the case for subsequent calls, because the stack is often not executable post-startup.

You should save your XIP setup function in the first 256 bytes of boot RAM to make it easily locatable when the XIP mode is re-initialised following a serial flash programming operation which had to drop out of XIP mode. The bootrom writes a default XIP setup function to this address before entering the flash image, which restores the mode the bootrom discovered during flash programming.

# **NOTE**

You cannot execute an XIP setup function directly from boot RAM, because boot RAM is never executable. You must copy it into SRAM before execution.

XIP setup functions should be fully position-independent, and no more than 256 bytes in size. If you are unable to meet these requirements, you should install a stub function which calls your XIP setup function elsewhere in RAM.

