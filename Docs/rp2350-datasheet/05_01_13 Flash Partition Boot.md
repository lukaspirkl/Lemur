# 5.1.13 Flash Partition Boot

If a PARTITION\_TABLE, but no IMAGE\_DEF, is found in the valid block loop that starts within the first 4 kB of flash, and it is valid (including signature if necessary on a secured RP2350), the bootrom searches that partition table's partitions for an executable image to boot. This process, when successful, is referred to as **flash partition boot**.

The partitions are searched in order, skipping those marked as ignored for the current architecture. The bootrom ignores partitions as an optimisation, or to prevent [automatic architecture switching.](#page-364-0)

If the partition is not part of an A/B pair, the first 4 kB is searched for the start of a valid block loop. If a valid block loop is found, and it contains an executable image with a valid (including signature on a secured RP2350) IMAGE\_DEF, then that executable image is chosen for boot.

If the partition is the A partition of an A/B pair, the bootrom searches both partitions as described above. If both partitions result in a bootable IMAGE\_DEF, the IMAGE\_DEF with the higher version number is chosen. Otherwise, the valid IMAGE\_DEF is chosen. There are some exceptions to this rule in advanced scenarios; see [Section 5.1.16](#page-361-0) and [Section](#page-362-0) [5.1.17](#page-362-0) for details.

