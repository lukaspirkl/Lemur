# 7.6.4 List of Registers

The chip-level reset subsystem shares a register address space with other power management subsystems in the always-on domain. The address space is referred to as POWMAN elsewhere in this document. A complete list of POWMAN registers is provided in [Section 6.4, "Power Management \(POWMAN\) Registers"](#page-454-1), but information on registers associated with the brownout detector are repeated here.

The POWMAN registers start at a base address of 0x40100000 (defined as [POWMAN\\_BASE](#page-31-1) in SDK).

- [BOD\\_CTRL](#page-464-0)
- [BOD](#page-465-0)
- [BOD\\_LP\\_ENTRY](#page-465-1)
- [BOD\\_LP\\_EXIT](#page-466-0)

# <span id="page-510-0"></span>**Chapter 8. Clocks**

