# 8.2.8 List of Registers

The XOSC registers start at a base address of 0x40048000 (defined as [XOSC\\_BASE](#page-31-1) in SDK).

*Table 597. List of XOSC registers*

<span id="page-556-2"></span>

| Offset | Name    | Info                                                                            |
|--------|---------|---------------------------------------------------------------------------------|
| 0x00   | CTRL    | Crystal Oscillator Control                                                      |
| 0x04   | STATUS  | Crystal Oscillator Status                                                       |
| 0x08   | DORMANT | Crystal Oscillator pause control                                                |
| 0x0c   | STARTUP | Controls the startup delay                                                      |
| 0x10   | COUNT   | A down counter running at the XOSC frequency which counts to<br>zero and stops. |

#### <span id="page-556-1"></span>**[XOSC](#page-556-2): CTRL Register**

**Offset**: 0x00

**Description**

Crystal Oscillator Control

*Table 598. CTRL Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:24 | Reserved.   | -    | -     |

| Bits  | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Type | Reset |
|-------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 23:12 | ENABLE: On power-up this field is initialised to DISABLE and the chip runs<br>from the ROSC.<br>If the chip has subsequently been programmed to run from the XOSC then<br>setting this field to DISABLE may lock-up the chip. If this is a concern then run<br>the clk_ref from the ROSC and enable the clk_sys RESUS feature.<br>The 12-bit code is intended to give some protection against accidental writes.<br>An invalid setting will retain the previous value. The actual value being used<br>can be read from STATUS_ENABLED | RW   | -     |
|       | Enumerated values:                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |       |
|       | 0xd1e → DISABLE                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |      |       |
|       | 0xfab → ENABLE                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |      |       |
| 11:0  | FREQ_RANGE: The 12-bit code is intended to give some protection against<br>accidental writes. An invalid setting will retain the previous value. The actual<br>value being used can be read from STATUS_FREQ_RANGE                                                                                                                                                                                                                                                                                                                    | RW   | -     |
|       | Enumerated values:                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |       |
|       | 0xaa0 → 1_15MHZ                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |      |       |
|       | 0xaa1 → 10_30MHZ                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |      |       |
|       | 0xaa2 → 25_60MHZ                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |      |       |
|       | 0xaa3 → 40_100MHZ                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |      |       |

# <span id="page-557-0"></span>**[XOSC](#page-556-2): STATUS Register**

**Offset**: 0x04

#### **Description**

Crystal Oscillator Status

*Table 599. STATUS Register*

| Bits  | Description                                                                                 | Type | Reset |
|-------|---------------------------------------------------------------------------------------------|------|-------|
| 31    | STABLE: Oscillator is running and stable                                                    | RO   | 0x0   |
| 30:25 | Reserved.                                                                                   | -    | -     |
| 24    | BADWRITE: An invalid value has been written to CTRL_ENABLE or<br>CTRL_FREQ_RANGE or DORMANT | WC   | 0x0   |
| 23:13 | Reserved.                                                                                   | -    | -     |
| 12    | ENABLED: Oscillator is enabled but not necessarily running and stable, resets<br>to 0       | RO   | -     |
| 11:2  | Reserved.                                                                                   | -    | -     |
| 1:0   | FREQ_RANGE: The current frequency range setting                                             | RO   | -     |
|       | Enumerated values:                                                                          |      |       |
|       | 0x0 → 1_15MHZ                                                                               |      |       |
|       | 0x1 → 10_30MHZ                                                                              |      |       |
|       | 0x2 → 25_60MHZ                                                                              |      |       |
|       | 0x3 → 40_100MHZ                                                                             |      |       |

# <span id="page-557-1"></span>**[XOSC](#page-556-2): DORMANT Register**

**Offset**: 0x08

#### **Description**

Crystal Oscillator pause control

*Table 600. DORMANT Register*

| Bits | Description                                                                                                                                                                                                                                               | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:0 | This is used to save power by pausing the XOSC<br>On power-up this field is initialised to WAKE<br>An invalid write will also select WAKE<br>WARNING: stop the PLLs before selecting dormant mode<br>WARNING: setup the irq before selecting dormant mode | RW   | -     |
|      | Enumerated values:                                                                                                                                                                                                                                        |      |       |
|      | 0x636f6d61 → DORMANT                                                                                                                                                                                                                                      |      |       |
|      | 0x77616b65 → WAKE                                                                                                                                                                                                                                         |      |       |

# <span id="page-558-1"></span>**[XOSC](#page-556-2): STARTUP Register**

**Offset**: 0x0c

#### **Description**

Controls the startup delay

*Table 601. STARTUP Register*

| Bits  | Description                                                                                                                                                                                                            | Type | Reset  |
|-------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:21 | Reserved.                                                                                                                                                                                                              | -    | -      |
| 20    | X4: Multiplies the startup_delay by 4, just in case. The reset value is controlled<br>by a mask-programmable tiecell and is provided in case we are booting from<br>XOSC and the default startup delay is insufficient | RW   | 0x0    |
| 19:14 | Reserved.                                                                                                                                                                                                              | -    | -      |
| 13:0  | DELAY: in multiples of 256*xtal_period. The reset value of 0xc4 corresponds<br>to approx 50 000 cycles.                                                                                                                | RW   | 0x00c4 |

#### <span id="page-558-2"></span>**[XOSC](#page-556-2): COUNT Register**

**Offset**: 0x10

*Table 602. COUNT Register*

| Bits  | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Type | Reset  |
|-------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | -    | -      |
| 15:0  | A down counter running at the xosc frequency which counts to zero and stops.<br>Can be used for short software pauses when setting up time sensitive<br>hardware.<br>To start the counter, write a non-zero value. Reads will return 1 while the count<br>is running and 0 when it has finished.<br>Minimum count value is 4. Count values <4 will be treated as count value =4.<br>Note that synchronisation to the register clock domain costs 2 register clock<br>cycles and the counter cannot compensate for that. | RW   | 0x0000 |

