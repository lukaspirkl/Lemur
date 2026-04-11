# 5.2.4 Watchdog Boot Vector

Watchdog boot allows users to install their own boot handler, and divert control away from the main boot sequence on non-POR/BOR resets. It recognises the following values written to the watchdog's upper scratch registers:

- [SCRATCH4](#page-1194-2): magic number 0xb007c0d3
- [SCRATCH5](#page-1194-2): entry point XORed with magic -0xb007c0d3 (0x4ff83f2d)
- [SCRATCH6](#page-1194-2): stack pointer
- [SCRATCH7](#page-1194-2): entry point

If either of the magic numbers mismatch, watchdog boot does not take place. If the numbers match, the Bootrom zeroes [SCRATCH4](#page-1194-2) before transferring control, so that the behaviour does not persist over subsequent reboots.

Watchdog boot can also be used to select the bootrom's special one-shot boot modes, described in [Section 5.2.4.1.](#page-371-1) The term **one-shot** refers to the fact these only affect the next boot (and not subsequent ones) due to the bootrom clearing [SCRATCH4](#page-1194-2) each boot. These boot types are encoded by setting a special entry point (pc) value of 0xb007c0d3, which is otherwise not a valid entry address, and then setting the boot type in the stack pointer (sp) value. [Section 5.2.4.1](#page-371-1) lists the supported values.

The watchdog boot vector is permitted to return. The boot path continues as normal when it returns: use this to perform any additional setup required for the boot path, such as issuing additional commands to an external QSPI device. On RISC-V the vector is permitted to use its own global pointer (gp) value, as the bootrom only uses gp during USB boot, which installs its own value.

With the exception of the magic boot type entry point (0xb007c0d3), the vector entry point pc must have the LSB set on Arm (the Thumb bit) and clear on RISC-V. If this condition is not met, the bootrom assumes you have passed a RISC-V function pointer to an Arm processor (or vice versa) and hangs the core rather than continuing.

#### <span id="page-371-1"></span>**5.2.4.1. Special Watchdog Boot Types**

The magic entry point 0xb007c0d3 indicates a special one-shot boot type, identified by the stack pointer value:

#### **BOOTSEL**

Selected by sp = 2. Boot into BOOTSEL mode. This will be either UART or USB boot depending on whether QSPI SD1 is driven high (default pull-down selects USB boot). See [Section 5.2.8](#page-374-0) for more details.

#### **RAM\_IMAGE**

Selected by sp = 3. Boot into an image stored in SRAM or XIP SRAM. BOOTSEL mode uses this to request execution of an image it loaded into RAM before rebooting. See [Section 5.2.5](#page-372-0) for more details.

#### **FLASH\_UPDATE**

Selected by sp = 4. BOOTSEL selects this mode when rebooting following a flash download. Changes some flash boot behaviour, such as allowing older [versions](#page-357-0) to boot in preference to newer ones. See [Section 5.1.16](#page-361-0) for more details.

Parameters to the one-shot boot type are passed in:

- [SCRATCH2](#page-1194-2): Parameter 0
- [SCRATCH3](#page-1194-2): Parameter 1

These directly correspond to the p0 and p1 boot parameters passed into the [reboot\(\)](#page-395-0) API. For example, on a RAM\_IMAGE boot, this specifies the base and size of the RAM region to be searched for a valid IMAGE\_DEF. See the API listing in [Section 5.4.8.24](#page-395-0) for more details. When not performing one of the listed boot types, [SCRATCH2](#page-1194-2) and [SCRATCH3](#page-1194-2) remain free for arbitrary user values, and the bootrom does not modify or interpret their contents.

