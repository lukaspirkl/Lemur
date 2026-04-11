# 5.1.14 Partition-Table-in-Image Boot

If both a PARTITION\_TABLE and an IMAGE\_DEF block are found in the valid block loop that starts within the first 4 kB of flash, a third type of flash boot takes place. The IMAGE\_DEF and PARTITION\_TABLE must only be recognised, not necessarily valid or correctly signed. This stipulation prevents a causality loop.

This is known as **partition-table-in-image** boot, since the application contains the partition table (instead of vice versa). This partition table is referred to as an **embedded partition table**.

The PARTITION\_TABLE is loaded as the current partition table, and the IMAGE\_DEF is launched directly. The table defined by the PARTITION\_TABLE is *not* searched for IMAGE\_DEFs to boot.

The following common cases might use this scenario:

- You are only using the PARTITION\_TABLE for flash permissions. You want to load that partition table, then boot as normal.
- The IMAGE\_DEF contains a small bootloader stored alongside the partition table. In this case, the partition table will once again be loaded, and the associated image entered. The entered image will then likely pick a partition from the partition table, and launch an image from there itself.

#### <span id="page-360-2"></span>**5.1.15. Flash Boot Slots**

The previous sections within this chapter discuss block loops starting within the first 4 kB of flash. Such a block loop contained either an IMAGE\_DEF, a partition table (searched for IMAGE\_DEFs), or an IMAGE\_DEF *and* a PARTITION\_TABLE (not searched).

All the previously mentioned cases discovered their block loop in **slot 0**. Under certain circumstances, the neighbouring **slot 1** is also searched.

Slot 0 starts at the beginning of flash, and has a size of n × 4 kB sectors. Slot 1 has the same size and follows immediately after slot 0. The value of n defaults to 1. Both slots are 4 kB in size, but you can override this value by specifying a value in [FLASH\\_PARTITION\\_SLOT\\_SIZE](#page-1310-0) and then setting [BOOT\\_FLAGS0.](#page-1304-1)OVERRIDE\_FLASH\_PARTITION\_SLOT\_SIZE.

Similarly to how a choice can be made between IMAGE\_DEFs in A/B partitions, a choice can be made between A/B PARTITION\_TABLEs via the two **boot slots**. This allows for versioning partition tables, targeted drag and drop of UF2s ([Section 5.1.18\)](#page-362-1) containing partition tables, etc. similar to the process used for images.

Slot 1 is only of use when potentially using partition tables. In the simple case of an IMAGE\_DEF and no PARTITION\_TABLE found in a block loop starting in slot 0, that image likely actually overlays the space where slot 1 would be, but in any case, slot 1 is ignored since there is no PARTITION\_TABLE.

If slot 0 contains a PARTITION\_TABLE or does not contain an IMAGE\_DEF (including nothing/garbage in slot 0), slot 1 can be considered. As an optimisation, in the former case, the scanning of slot 1 can be prevented by setting the singleton flag in the PARTITION\_TABLE.

# **NOTE**

When IMAGE\_DEFs are also present in the slots, the PARTITION\_TABLE's VERSION item determines which of slot 0 and slot 1 to use. The IMAGE\_DEF metadata is ignored for the purpose of version comparison.

