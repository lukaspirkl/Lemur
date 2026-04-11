# 5.1.6 Block Versioning

Any block may contain a **version**. Version information consists of a tuple of either two or three 16-bit values: (rollback).major.minor, where the rollback part is optional. An item of type VERSION contains the binary data structure which defines the version of a block.

The rollback version may only be specified for IMAGE\_DEFs and defaults to zero if not present. You cannot specify this version for partition tables. The rollback version can be used on a [secured RP2350](#page-354-1), where it, along with a current rollback verson number stored in OTP, can prevent installation of older, vulnerable code once a newer version is installed ([Section 5.1.11\)](#page-359-0).

The full version number can be used to pick the latest version between two IMAGE\_DEFs or two PARTITION\_TABLEs (see [Section 5.1.7](#page-357-1)). Versions compare in lexicographic order:

- 1. If version *x* has a different rollback version than version *y*, then the greater rollback version determines which version is greater overall
- 2. Else if version *x* has a different major version than version *y*, then the greater major version determines which version is greater overall
- 3. Else the minor version determines which of *x* and *y* is greater

See [Section 5.9.2.1](#page-417-1) for full details on the VERSION item in a block.

#### <span id="page-357-1"></span>**5.1.7. A/B Versions**

A pair of partitions may be grouped into an **A/B** pair. By logically grouping A and B partitions, you can keep the current executable image (or data) in one partition, and write a newer version into the other partition. When you finish writing a new version, you can safely switch to it, reverting to the older version if problems arise. This avoids partially written states that could render RP2350 un-bootable.

- When booting an A/B partition pair, the bootrom typically uses the partition with the higher version. For scenarios where this is not the case, see [Section 5.1.16](#page-361-0).
- When dragging a UF2 onto the BOOTSEL USB drive, the UF2 targets the *opposite* A/B partition to the one preferred at boot. See [Section 5.1.18](#page-362-1) for more details.

#### **NOTE**

It is also possible to have A/B versions of the partition table. For more information about this advanced topic, see [Section 5.1.15.](#page-360-2)

