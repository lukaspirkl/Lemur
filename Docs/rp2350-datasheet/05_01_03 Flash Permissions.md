# 5.1.3 Flash Permissions

One of the roles of the partition tables introduced in [Section 5.1.2](#page-354-2) is to define **flash permissions**, or simply permissions. The partition table stores one set of permission flags for each partition: all bytes covered by a single partition have the same permissions. The partition table separately defines permissions for **unpartitioned space**: flash addresses which do not match any of partitions defined in the partition table.

Separate read/write permissions are specified for each of Secure (S), Non-secure (NS) and bootloader (BL) access. Bootloader permissions control where UF2s can be written to, and what can be accessed via picotool when the device is in BOOTSEL mode.

Because flash permissions may be changed dynamically at runtime, part of the partition table is resident in RAM at runtime. You can modify this table to add permissions for other areas of flash at runtime, without changing the partition table stored in flash itself. There is no bootrom API for this, however the in-memory partition table format is documented, and a pointer is available in the ROM table. The SDK provides APIs to wrap this functionality.

