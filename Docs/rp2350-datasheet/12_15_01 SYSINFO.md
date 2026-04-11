# 12.15.1 SYSINFO

#### **12.15.1.1. Overview**

The sysinfo block contains system information. The first register contains the Chip ID, which allows the programmer to know which version of the chip software is running on. The second register indicates which package configuration is used (QFN-60 or QFN-80). The third register will always read as 1.

#### **12.15.1.2. List of Registers**

The sysinfo registers start at a base address of 0x40000000 (defined as [SYSINFO\\_BASE](#page-31-1) in SDK).

*Table 1305. List of SYSINFO registers*

<span id="page-1247-3"></span>

| Offset | Name          | Info                                                                                                                                                                         |
|--------|---------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x00   | CHIP_ID       | JEDEC JEP-106 compliant chip identifier.                                                                                                                                     |
| 0x04   | PACKAGE_SEL   | Package selection indicator, 0 = QFN80, 1 = QFN60                                                                                                                            |
| 0x08   | PLATFORM      | Platform register. Allows software to know what environment it<br>is running in during pre-production development. Post<br>production, the PLATFORM is always ASIC, non-SIM. |
| 0x14   | GITREF_RP2350 | Git hash of the chip source. Used to identify chip version.                                                                                                                  |

# <span id="page-1247-0"></span>**[SYSINFO:](#page-1247-3) CHIP\_ID Register**

**Offset**: 0x00 **Description**

JEDEC JEP-106 compliant chip identifier.

*Table 1306. CHIP\_ID Register*

| Bits  | Description  | Type | Reset |
|-------|--------------|------|-------|
| 31:28 | REVISION     | RO   | -     |
| 27:12 | PART         | RO   | -     |
| 11:1  | MANUFACTURER | RO   | -     |
| 0     | STOP_BIT     | RO   | 0x1   |

# <span id="page-1247-1"></span>**[SYSINFO:](#page-1247-3) PACKAGE\_SEL Register**

**Offset**: 0x04

*Table 1307. PACKAGE\_SEL Register*

| Bits | Description                                       | Type | Reset |
|------|---------------------------------------------------|------|-------|
| 31:1 | Reserved.                                         | -    | -     |
| 0    | Package selection indicator, 0 = QFN80, 1 = QFN60 | RO   | 0x0   |

#### <span id="page-1247-2"></span>**[SYSINFO:](#page-1247-3) PLATFORM Register**

**Offset**: 0x08 **Description**

> Platform register. Allows software to know what environment it is running in during pre-production development. Post-production, the PLATFORM is always ASIC, non-SIM.

*Table 1308. PLATFORM Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31:5 | Reserved.   | -    | -     |
| 4    | GATESIM     | RO   | -     |
| 3    | BATCHSIM    | RO   | -     |
| 2    | HDLSIM      | RO   | -     |
| 1    | ASIC        | RO   | -     |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 0    | FPGA        | RO   | -     |

# <span id="page-1248-1"></span>**[SYSINFO:](#page-1247-3) GITREF\_RP2350 Register**

#### **Offset**: 0x14

*Table 1309. GITREF\_RP2350 Register*

| Bits | Description                                                 | Type | Reset |
|------|-------------------------------------------------------------|------|-------|
| 31:0 | Git hash of the chip source. Used to identify chip version. | RO   | -     |

