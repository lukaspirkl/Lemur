# 8.4.3 List of Registers

The low power oscillator shares register address space with other power management subsystems in the always-on domain. The address space is referred to as POWMAN elsewhere in this document. A complete list of POWMAN registers is provided in [Section 6.4, "Power Management \(POWMAN\) Registers"](#page-454-1), but information on registers associated with the low power oscillator is repeated here.

The POWMAN registers start at a base address of 0x40100000 (defined as [POWMAN\\_BASE](#page-31-1) in SDK).

- [LPOSC](#page-467-1)
- [EXT\\_TIME\\_REF](#page-474-1)
- [LPOSC\\_FREQ\\_KHZ\\_INT](#page-475-0)
- <span id="page-567-3"></span>• [LPOSC\\_FREQ\\_KHZ\\_FRAC](#page-475-1)

