# 13.1 OTP Address Map

The OTP hardware resides in a 128 kB region starting at 0x40120000 (OTP\_BASE in the SDK). Bit 16 of the address is used to select either the OTP control registers, in the lower 64 kB, or one of the OTP read data aliases, in the upper 64 kB of this space.

The OTP control registers [\(Section 13.8\)](#page-1276-0) are aliased at 4 kB intervals to implement the usual set, clear, and XOR atomic write aliases described in [Section 2.1.3.](#page-26-0)

The read data region starting at 0x40130000 divides further into four aliases:

• 0x40130000, OTP\_DATA\_BASE: ECC read alias. A 32-bit read returns the ECC-corrected data for two neighbouring rows, or all-ones on permission failure. Only the first 8 kB is populated.

13.1. OTP Address Map **1265**

- 0x40138000, OTP\_DATA\_GUARDED\_BASE: ECC guarded read alias. Successful reads return the same data as OTP\_DATA\_BASE. Only the first 8 kB is populated.
- 0x40134000, OTP\_DATA\_RAW\_BASE: raw read alias. A 32-bit read directly returns the 24-bit contents of a single row, with zeroes in the eight MSBs, or returns all-ones on permission failure.
- 0x4013c000, OTP\_DATA\_RAW\_GUARDED\_BASE: raw, guarded read alias. Successful reads return the same data as OTP\_DATA\_RAW\_BASE.

Bit 14 of the address selects ECC (0) vs raw (1). Bit 15 of the address selects unguarded (0) vs guarded (1) access. Guarded reads return the same data as unguarded reads, but perform additional hardware consistency checks and return bus faults on permission failure. For more information, see [Section 13.1.1](#page-1266-0).

#### **IMPORTANT**

The read data regions starting at 0x40130000 are accessible only when [USR.](#page-1281-0)DCTRL is set, otherwise all reads return a bus error response. This bit is clear when the OTP is being programmed via the SBPI bridge.

Writing to the read data aliases is not a valid operation, and will always return a bus fault. The OTP is programmed by the SBPI bridge, which is used internally by the bootrom otp\_access API, [Section 5.4.8.21.](#page-393-2)

