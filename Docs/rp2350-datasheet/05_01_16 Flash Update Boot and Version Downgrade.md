# 5.1.16 Flash Update Boot and Version Downgrade

Normally the choice of slot 0 versus slot 1, and partition A versus partition B, is made based on the [version](#page-357-0) of the valid PARTITION\_TABLE or IMAGE\_DEF in those slots or partitions respectively. The greater of the two versions wins.

It is however perfectly valid to downgrade to a lower-versioned IMAGE\_DEF when using A/B partitions, provided this does not violate [anti-rollback](#page-359-0) rules on a secured RP2350.

Downloading the new image (and its IMAGE\_DEF) into the non-currently-booting partition and doing a normal reboot will not work in this case, as the newly downloaded image has a lower version.

For this purpose, you can enable a **flash update boot** boot by passing the FLASH\_UPDATE boot type constant flag through the watchdog scratch registers and a pointer to the start of the region of flash that has just been updated.

The bootrom automatically performs a flash update boot after programming a flash UF2 written to the USB Mass Storage drive. You can also invoke a flash update boot programmatically via the reboot() API (see [Section 5.4.8.24\)](#page-395-0).

The flash address range passed through the reboot parameters is treated specially during a flash update boot. A PARTITION\_TABLE in a slot, or IMAGE\_DEF in a partition, will be chosen for boot irrespective of version, if the start of the region is the start of the respective slot or partition.

In order for the downgrade to persist, the first sector of the previously booting slot or partition must be erased so that the newly installed PARTITION\_TABLE or IMAGE\_DEF will continue to be chosen on subsequent boots. This erase is performed as follows during a FLASH\_UPDATE boot.

- 1. When a PARTITION\_TABLE is valid (and correctly signed if necessary) and its slot is chosen for boot, the first sector of the other slot is erased.
- 2. When a valid (and correctly signed if necessary) IMAGE\_DEF is launched, the first sector of the other image is erased.
- 3. On explicit request by the image, after it is launched, the first sector of the other image is erased. This is an alternative to the standard behaviour in the previous bullet, and is selected by a special "Try Before You Buy" flag in the IMAGE\_DEF. For more information about this feature, see [Section 5.1.17.](#page-362-0)

# **NOTE**

Flash update and version downgrade have no effect when using a single slot, or standalone (non A/B) partitions.

