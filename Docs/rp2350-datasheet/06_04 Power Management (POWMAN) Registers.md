# 6.4 Power Management (POWMAN) Registers

Password-protected POWMAN registers require a password (0x5AFE) to be written to the top 16 bits to enable the write operation. This protects against accidental writes which could crash the chip untraceably. Writes to protected registers that do not include the password are ignored, setting a flag in the [BADPASSWD](#page-459-0) register. Reads from protected registers do not return the password, to protect against erroneous read-modify-write operations.

Protected registers obviously do not have writeable fields in the top 16 bits, however they may have read-only fields in that range.

All registers with address offsets up to and including 0x000000ac are password protected. Therefore, the following writeable registers are unprotected and have 32-bit write access:

- POWMAN\_SCRATCH0 <sup>→</sup> POWMAN\_SCRATCH7
- POWMAN\_BOOT0 <sup>→</sup> POWMAN\_BOOT3
- POWMAN\_INTR
- POWMAN\_INTE
- POWMAN\_INTF

*Table 476. List of POWMAN registers*

<span id="page-455-0"></span>

| Offset | Name                                                         | Info                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
|--------|--------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x00   | BADPASSWD                                                    | Indicates a bad password has been used                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| 0x04   | VREG_CTRL                                                    | Voltage Regulator Control                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x08   | VREG_STS                                                     | Voltage Regulator Status                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| 0x0c   | VREG                                                         | Voltage Regulator Settings                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| 0x10   | VREG_LP_ENTRY                                                | Voltage Regulator Low Power Entry Settings                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| 0x14   | VREG_LP_EXIT                                                 | Voltage Regulator Low Power Exit Settings                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x18   | BOD_CTRL                                                     | Brown-out Detection Control                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| 0x1c   | BOD                                                          | Brown-out Detection Settings                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| 0x20   | BOD_LP_ENTRY<br>Brown-out Detection Low Power Entry Settings |                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 0x24   | BOD_LP_EXIT<br>Brown-out Detection Low Power Exit Settings   |                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 0x28   | LPOSC<br>Low power oscillator control register.              |                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 0x2c   | CHIP_RESET                                                   | Chip reset control and status                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| 0x30   | WDSEL                                                        | Allows a watchdog reset to reset the internal state of powman in<br>addition to the power-on state machine (PSM).<br>Note that powman ignores watchdog resets that do not select at<br>least the CLOCKS stage or earlier stages in the PSM. If using<br>these bits, it's recommended to set PSM_WDSEL to all-ones in<br>addition to the desired bits in this register. Failing to select<br>CLOCKS or earlier will result in the POWMAN_WDSEL register<br>having no effect. |
| 0x34   | SEQ_CFG                                                      | For configuration of the power sequencer<br>Writes are ignored while POWMAN_STATE_CHANGING=1                                                                                                                                                                                                                                                                                                                                                                                |

| Offset | Name                | Info                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |  |
|--------|---------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--|
| 0x38   | STATE               | This register controls the power state of the 4 power domains.<br>The current power state is indicated in<br>POWMAN_STATE_CURRENT which is read-only.<br>To change the state, write to POWMAN_STATE_REQ.<br>The coding of POWMAN_STATE_CURRENT &<br>POWMAN_STATE_REQ corresponds to the power states<br>defined in the datasheet:<br>bit 3 = SWCORE<br>bit 2 = XIP cache<br>bit 1 = SRAM0<br>bit 0 = SRAM1<br>0 = powered up<br>1 = powered down<br>When POWMAN_STATE_REQ is written, the<br>POWMAN_STATE_WAITING flag is set while the Power Manager<br>determines what is required. If an invalid transition is requested<br>the Power Manager will still register the request in<br>POWMAN_STATE_REQ but will also set the POWMAN_BAD_REQ<br>flag. It will then implement the power-up requests and ignore the<br>power down requests. To do nothing would risk entering an<br>unrecoverable lock-up state. Invalid requests are: any<br>combination of power up and power down requests any request<br>that results in swcore being powered and xip unpowered If the<br>request is to power down the switched-core domain then<br>POWMAN_STATE_WAITING stays active until the processors<br>halt. During this time the POWMAN_STATE_REQ field can be re<br>written to change or cancel the request. When the power state<br>transition begins the POWMAN_STATE_WAITING_flag is cleared,<br>the POWMAN_STATE_CHANGING flag is set and POWMAN<br>register writes are ignored until the transition completes. |  |
| 0x3c   | POW_FASTDIV         |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |  |
| 0x40   | POW_DELAY           | power state machine delays                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |  |
| 0x44   | EXT_CTRL0           | Configures a gpio as a power mode aware control output                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |  |
| 0x48   | EXT_CTRL1           | Configures a gpio as a power mode aware control output                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |  |
| 0x4c   | EXT_TIME_REF        | Select a GPIO to use as a time reference, the source can be used<br>to drive the low power clock at 32kHz, or to provide a 1ms tick to<br>the timer, or provide a 1Hz tick to the timer. The tick selection is<br>controlled by the POWMAN_TIMER register.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |  |
| 0x50   | LPOSC_FREQ_KHZ_INT  | Informs the AON Timer of the integer component of the clock<br>frequency when running off the LPOSC.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |  |
| 0x54   | LPOSC_FREQ_KHZ_FRAC | Informs the AON Timer of the fractional component of the clock<br>frequency when running off the LPOSC.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |  |
| 0x58   | XOSC_FREQ_KHZ_INT   | Informs the AON Timer of the integer component of the clock<br>frequency when running off the XOSC.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |  |
| 0x5c   | XOSC_FREQ_KHZ_FRAC  | Informs the AON Timer of the fractional component of the clock<br>frequency when running off the XOSC.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |  |
| 0x60   | SET_TIME_63TO48     |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |  |
| 0x64   | SET_TIME_47TO32     |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |  |

| Offset | Name              | Info                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
|--------|-------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x68   | SET_TIME_31TO16   |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x6c   | SET_TIME_15TO0    |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x70   | READ_TIME_UPPER   |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x74   | READ_TIME_LOWER   |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x78   | ALARM_TIME_63TO48 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x7c   | ALARM_TIME_47TO32 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x80   | ALARM_TIME_31TO16 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x84   | ALARM_TIME_15TO0  |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x88   | TIMER             |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 0x8c   | PWRUP0            | 4 GPIO powerup events can be configured to wake the chip up<br>from a low power state.<br>The pwrups are level/edge sensitive and can be set to trigger on<br>a high/rising or low/falling event<br>The number of gpios available depends on the package option.<br>An invalid selection will be ignored<br>source = 0 selects gpio0<br>source = 47 selects gpio47<br>source = 48 selects qspi_ss<br>source = 49 selects qspi_sd0<br>source = 50 selects qspi_sd1<br>source = 51 selects qspi_sd2<br>source = 52 selects qspi_sd3<br>source = 53 selects qspi_sclk<br>level = 0 triggers the pwrup when the source is low<br>level = 1 triggers the pwrup when the source is high |
| 0x90   | PWRUP1            | 4 GPIO powerup events can be configured to wake the chip up<br>from a low power state.<br>The pwrups are level/edge sensitive and can be set to trigger on<br>a high/rising or low/falling event<br>The number of gpios available depends on the package option.<br>An invalid selection will be ignored<br>source = 0 selects gpio0<br>source = 47 selects gpio47<br>source = 48 selects qspi_ss<br>source = 49 selects qspi_sd0<br>source = 50 selects qspi_sd1<br>source = 51 selects qspi_sd2<br>source = 52 selects qspi_sd3<br>source = 53 selects qspi_sclk<br>level = 0 triggers the pwrup when the source is low<br>level = 1 triggers the pwrup when the source is high |

| Offset | Name              | Info                                                                                                                                                                                                                                                                                                                                                                                              |
|--------|-------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x94   | PWRUP2            | 4 GPIO powerup events can be configured to wake the chip up<br>from a low power state.<br>The pwrups are level/edge sensitive and can be set to trigger on<br>a high/rising or low/falling event<br>The number of gpios available depends on the package option.<br>An invalid selection will be ignored<br>source = 0 selects gpio0<br>source = 47 selects gpio47                                |
|        |                   | source = 48 selects qspi_ss<br>source = 49 selects qspi_sd0<br>source = 50 selects qspi_sd1<br>source = 51 selects qspi_sd2<br>source = 52 selects qspi_sd3<br>source = 53 selects qspi_sclk<br>level = 0 triggers the pwrup when the source is low<br>level = 1 triggers the pwrup when the source is high                                                                                       |
| 0x98   | PWRUP3            | 4 GPIO powerup events can be configured to wake the chip up<br>from a low power state.<br>The pwrups are level/edge sensitive and can be set to trigger on<br>a high/rising or low/falling event<br>The number of gpios available depends on the package option.<br>An invalid selection will be ignored<br>source = 0 selects gpio0<br>source = 47 selects gpio47<br>source = 48 selects qspi_ss |
|        |                   | source = 49 selects qspi_sd0<br>source = 50 selects qspi_sd1<br>source = 51 selects qspi_sd2<br>source = 52 selects qspi_sd3<br>source = 53 selects qspi_sclk<br>level = 0 triggers the pwrup when the source is low<br>level = 1 triggers the pwrup when the source is high                                                                                                                      |
| 0x9c   | CURRENT_PWRUP_REQ | Indicates current powerup request state<br>pwrup events can be cleared by removing the enable from the<br>pwrup register. The alarm pwrup req can be cleared by clearing<br>timer.alarm_enab<br>0 = chip reset, for the source of the last reset see<br>POWMAN_CHIP_RESET<br>1 = pwrup0<br>2 = pwrup1<br>3 = pwrup2<br>4 = pwrup3<br>5 = coresight_pwrup<br>6 = alarm_pwrup                       |

| Offset | Name              | Info                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |  |
|--------|-------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--|
| 0xa0   | LAST_SWCORE_PWRUP | Indicates which pwrup source triggered the last switched-core<br>power up<br>0 = chip reset, for the source of the last reset see<br>POWMAN_CHIP_RESET<br>1 = pwrup0<br>2 = pwrup1<br>3 = pwrup2<br>4 = pwrup3<br>5 = coresight_pwrup<br>6 = alarm_pwrup                                                                                                                                                                                                                                                                                             |  |
| 0xa4   | DBG_PWRCFG        |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |  |
| 0xa8   | BOOTDIS           | Tell the bootrom to ignore the BOOT03 registers following the<br>next RSM reset (e.g. the next core power down/up).<br>If an early boot stage has soft-locked some OTP pages in order<br>to protect their contents from later stages, there is a risk that<br>Secure code running at a later stage can unlock the pages by<br>powering the core up and down.<br>This register can be used to ensure that the bootloader runs as<br>normal on the next power up, preventing Secure code at a later<br>stage from accessing OTP in its unlocked state. |  |
|        |                   | Should be used in conjunction with the OTP BOOTDIS register.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |  |
| 0xac   | DBGCONFIG         |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |  |
| 0xb0   | SCRATCH0          | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xb4   | SCRATCH1          | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xb8   | SCRATCH2          | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xbc   | SCRATCH3          | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xc0   | SCRATCH4          | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xc4   | SCRATCH5          | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xc8   | SCRATCH6          | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xcc   | SCRATCH7          | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xd0   | BOOT0             | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xd4   | BOOT1             | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xd8   | BOOT2             | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xdc   | BOOT3             | Scratch register. Information persists in low power mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |  |
| 0xe0   | INTR              | Raw Interrupts                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |  |
| 0xe4   | INTE              | Interrupt Enable                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |  |
| 0xe8   | INTF              | Interrupt Force                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |  |
|        |                   |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |  |

## <span id="page-459-0"></span>**[POWMAN:](#page-455-0) BADPASSWD Register**

**Offset**: 0x00

*Table 477. BADPASSWD Register*

| Bits | Description                            | Type | Reset |
|------|----------------------------------------|------|-------|
| 31:1 | Reserved.                              | -    | -     |
| 0    | Indicates a bad password has been used | WC   | 0x0   |

# <span id="page-460-0"></span>**[POWMAN:](#page-455-0) VREG\_CTRL Register**

**Offset**: 0x04 **Description**

Voltage Regulator Control

*Table 478. VREG\_CTRL Register*

| Bits  | Description                                                                | Type | Reset |
|-------|----------------------------------------------------------------------------|------|-------|
| 31:16 | Reserved.                                                                  | -    | -     |
| 15    | RST_N: returns the regulator to its startup settings                       | RW   | 0x1   |
|       | 0 - reset                                                                  |      |       |
|       | 1 - not reset (default)                                                    |      |       |
| 14    | Reserved.                                                                  | -    | -     |
| 13    | UNLOCK: unlocks the VREG control interface after power up                  | RW   | 0x0   |
|       | 0 - Locked (default)                                                       |      |       |
|       | 1 - Unlocked                                                               |      |       |
|       | It cannot be relocked when it is unlocked.                                 |      |       |
| 12    | ISOLATE: isolates the VREG control interface                               | RW   | 0x0   |
|       | 0 - not isolated (default)                                                 |      |       |
|       | 1 - isolated                                                               |      |       |
| 11:9  | Reserved.                                                                  | -    | -     |
| 8     | DISABLE_VOLTAGE_LIMIT: 0=not disabled, 1=enabled                           | RW   | 0x0   |
| 7     | Reserved.                                                                  | -    | -     |
| 6:4   | HT_TH: high temperature protection threshold                               | RW   | 0x5   |
|       | regulator power transistors are disabled when junction temperature exceeds |      |       |
|       | threshold                                                                  |      |       |
|       | 000 - 100C                                                                 |      |       |
|       | 001 - 105C                                                                 |      |       |
|       | 010 - 110C                                                                 |      |       |
|       | 011 - 115C                                                                 |      |       |
|       | 100 - 120C                                                                 |      |       |
|       | 101 - 125C                                                                 |      |       |
|       | 110 - 135C                                                                 |      |       |
|       | 111 - 150C                                                                 |      |       |
| 3:2   | Reserved.                                                                  | -    | -     |
|       |                                                                            |      |       |

# <span id="page-460-1"></span>**[POWMAN:](#page-455-0) VREG\_STS Register**

**Offset**: 0x08 **Description**

Voltage Regulator Status

*Table 479. VREG\_STS Register*

| Bits | Description                                                               | Type | Reset |
|------|---------------------------------------------------------------------------|------|-------|
| 31:5 | Reserved.                                                                 | -    | -     |
| 4    | VOUT_OK: output regulation status<br>0=not in regulation, 1=in regulation | RO   | 0x0   |
| 3:1  | Reserved.                                                                 | -    | -     |
| 0    | STARTUP: startup status<br>0=startup complete, 1=starting up              | RO   | 0x0   |

#### <span id="page-461-0"></span>**[POWMAN:](#page-455-0) VREG Register**

**Offset**: 0x0c

#### **Description**

Voltage Regulator Settings

*Table 480. VREG Register*

| Bits  | Description                                                                                                                | Type | Reset |
|-------|----------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:16 | Reserved.                                                                                                                  | -    | -     |
| 15    | UPDATE_IN_PROGRESS: regulator state is being updated<br>writes to the vreg register will be ignored when this field is set | RO   | 0x0   |
| 14:9  | Reserved.                                                                                                                  | -    | -     |

| Bits | Description                                                                 | Type | Reset |
|------|-----------------------------------------------------------------------------|------|-------|
| 8:4  | VSEL: output voltage select                                                 | RW   | 0x0b  |
|      | the regulator output voltage is limited to 1.3V unless the voltage limit    |      |       |
|      | is disabled using the disable_voltage_limit field in the vreg_ctrl register |      |       |
|      | 00000 - 0.55V                                                               |      |       |
|      | 00001 - 0.60V                                                               |      |       |
|      | 00010 - 0.65V                                                               |      |       |
|      | 00011 - 0.70V                                                               |      |       |
|      | 00100 - 0.75V                                                               |      |       |
|      | 00101 - 0.80V                                                               |      |       |
|      | 00110 - 0.85V                                                               |      |       |
|      | 00111 - 0.90V                                                               |      |       |
|      | 01000 - 0.95V                                                               |      |       |
|      | 01001 - 1.00V                                                               |      |       |
|      | 01010 - 1.05V                                                               |      |       |
|      | 01011 - 1.10V (default)                                                     |      |       |
|      | 01100 - 1.15V                                                               |      |       |
|      | 01101 - 1.20V                                                               |      |       |
|      | 01110 - 1.25V                                                               |      |       |
|      | 01111 - 1.30V                                                               |      |       |
|      | 10000 - 1.35V                                                               |      |       |
|      | 10001 - 1.40V                                                               |      |       |
|      | 10010 - 1.50V                                                               |      |       |
|      | 10011 - 1.60V                                                               |      |       |
|      | 10100 - 1.65V                                                               |      |       |
|      | 10101 - 1.70V                                                               |      |       |
|      | 10110 - 1.80V                                                               |      |       |
|      | 10111 - 1.90V                                                               |      |       |
|      | 11000 - 2.00V                                                               |      |       |
|      | 11001 - 2.35V                                                               |      |       |
|      | 11010 - 2.50V                                                               |      |       |
|      | 11011 - 2.65V                                                               |      |       |
|      | 11100 - 2.80V                                                               |      |       |
|      | 11101 - 3.00V                                                               |      |       |
|      | 11110 - 3.15V                                                               |      |       |
|      | 11111 - 3.30V                                                               |      |       |
| 3    | Reserved.                                                                   | -    | -     |
| 2    | RESERVED: write 0 to this field                                             | RW   | 0x0   |
| 1    | HIZ: high impedance mode select                                             | RW   | 0x0   |
|      | 0=not in high impedance mode, 1=in high impedance mode                      |      |       |
| 0    | Reserved.                                                                   | -    | -     |

#### <span id="page-462-0"></span>**[POWMAN:](#page-455-0) VREG\_LP\_ENTRY Register**

**Offset**: 0x10 **Description**

Voltage Regulator Low Power Entry Settings

*Table 481. VREG\_LP\_ENTRY Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31:9 | Reserved.   | -    | -     |

| Bits | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 8:4  | VSEL: output voltage select<br>the regulator output voltage is limited to 1.3V unless the voltage limit<br>is disabled using the disable_voltage_limit field in the vreg_ctrl register<br>00000 - 0.55V<br>00001 - 0.60V<br>00010 - 0.65V<br>00011 - 0.70V<br>00100 - 0.75V<br>00101 - 0.80V<br>00110 - 0.85V<br>00111 - 0.90V<br>01000 - 0.95V<br>01001 - 1.00V<br>01010 - 1.05V<br>01011 - 1.10V (default)<br>01100 - 1.15V<br>01101 - 1.20V<br>01110 - 1.25V<br>01111 - 1.30V<br>10000 - 1.35V<br>10001 - 1.40V<br>10010 - 1.50V<br>10011 - 1.60V<br>10100 - 1.65V<br>10101 - 1.70V<br>10110 - 1.80V<br>10111 - 1.90V<br>11000 - 2.00V<br>11001 - 2.35V<br>11010 - 2.50V<br>11011 - 2.65V<br>11100 - 2.80V<br>11101 - 3.00V<br>11110 - 3.15V<br>11111 - 3.30V | RW   | 0x0b  |
| 3    | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | -    | -     |
| 2    | MODE: selects either normal (switching) mode or low power (linear) mode<br>low power mode can only be selected for output voltages up to 1.3V<br>0 = normal mode (switching)<br>1 = low power mode (linear)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | RW   | 0x1   |
| 1    | HIZ: high impedance mode select<br>0=not in high impedance mode, 1=in high impedance mode                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | RW   | 0x0   |
| 0    | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | -    | -     |

## <span id="page-463-0"></span>**[POWMAN:](#page-455-0) VREG\_LP\_EXIT Register**

**Offset**: 0x14

#### **Description**

Voltage Regulator Low Power Exit Settings

*Table 482. VREG\_LP\_EXIT Register*

| Bits | Description                                                                 | Type | Reset |
|------|-----------------------------------------------------------------------------|------|-------|
| 31:9 | Reserved.                                                                   | -    | -     |
| 8:4  | VSEL: output voltage select                                                 | RW   | 0x0b  |
|      | the regulator output voltage is limited to 1.3V unless the voltage limit    |      |       |
|      | is disabled using the disable_voltage_limit field in the vreg_ctrl register |      |       |
|      | 00000 - 0.55V                                                               |      |       |
|      | 00001 - 0.60V                                                               |      |       |
|      | 00010 - 0.65V                                                               |      |       |
|      | 00011 - 0.70V                                                               |      |       |
|      | 00100 - 0.75V                                                               |      |       |
|      | 00101 - 0.80V                                                               |      |       |
|      | 00110 - 0.85V                                                               |      |       |
|      | 00111 - 0.90V                                                               |      |       |
|      | 01000 - 0.95V                                                               |      |       |
|      | 01001 - 1.00V                                                               |      |       |
|      | 01010 - 1.05V                                                               |      |       |
|      | 01011 - 1.10V (default)                                                     |      |       |
|      | 01100 - 1.15V                                                               |      |       |
|      | 01101 - 1.20V                                                               |      |       |
|      | 01110 - 1.25V                                                               |      |       |
|      | 01111 - 1.30V                                                               |      |       |
|      | 10000 - 1.35V                                                               |      |       |
|      | 10001 - 1.40V                                                               |      |       |
|      | 10010 - 1.50V                                                               |      |       |
|      | 10011 - 1.60V                                                               |      |       |
|      | 10100 - 1.65V                                                               |      |       |
|      | 10101 - 1.70V                                                               |      |       |
|      | 10110 - 1.80V                                                               |      |       |
|      | 10111 - 1.90V                                                               |      |       |
|      | 11000 - 2.00V                                                               |      |       |
|      | 11001 - 2.35V                                                               |      |       |
|      | 11010 - 2.50V                                                               |      |       |
|      | 11011 - 2.65V                                                               |      |       |
|      | 11100 - 2.80V                                                               |      |       |
|      | 11101 - 3.00V                                                               |      |       |
|      | 11110 - 3.15V                                                               |      |       |
|      | 11111 - 3.30V                                                               |      |       |
| 3    | Reserved.                                                                   | -    | -     |
| 2    | MODE: selects either normal (switching) mode or low power (linear) mode     | RW   | 0x0   |
|      | low power mode can only be selected for output voltages up to 1.3V          |      |       |
|      | 0 = normal mode (switching)                                                 |      |       |
|      | 1 = low power mode (linear)                                                 |      |       |
| 1    | HIZ: high impedance mode select                                             | RW   | 0x0   |
|      | 0=not in high impedance mode, 1=in high impedance mode                      |      |       |
| 0    | Reserved.                                                                   | -    | -     |

#### <span id="page-464-0"></span>**[POWMAN:](#page-455-0) BOD\_CTRL Register**

**Offset**: 0x18

#### **Description**

Brown-out Detection Control

*Table 483. BOD\_CTRL Register*

| Bits  | Description                                                                                               | Type | Reset |
|-------|-----------------------------------------------------------------------------------------------------------|------|-------|
| 31:13 | Reserved.                                                                                                 | -    | -     |
| 12    | ISOLATE: isolates the brown-out detection control interface<br>0 - not isolated (default)<br>1 - isolated | RW   | 0x0   |
| 11:0  | Reserved.                                                                                                 | -    | -     |

# <span id="page-465-0"></span>**[POWMAN:](#page-455-0) BOD Register**

**Offset**: 0x1c

#### **Description**

Brown-out Detection Settings

*Table 484. BOD Register*

| Bits | Description                    | Type | Reset |
|------|--------------------------------|------|-------|
| 31:9 | Reserved.                      | -    | -     |
| 8:4  | VSEL: threshold select         | RW   | 0x0b  |
|      | 00000 - 0.473V                 |      |       |
|      | 00001 - 0.516V                 |      |       |
|      | 00010 - 0.559V                 |      |       |
|      | 00011 - 0.602V                 |      |       |
|      | 00100 - 0.645VS                |      |       |
|      | 00101 - 0.688V                 |      |       |
|      | 00110 - 0.731V                 |      |       |
|      | 00111 - 0.774V                 |      |       |
|      | 01000 - 0.817V                 |      |       |
|      | 01001 - 0.860V (default)       |      |       |
|      | 01010 - 0.903V                 |      |       |
|      | 01011 - 0.946V                 |      |       |
|      | 01100 - 0.989V                 |      |       |
|      | 01101 - 1.032V                 |      |       |
|      | 01110 - 1.075V                 |      |       |
|      | 01111 - 1.118V                 |      |       |
|      | 10000 - 1.161                  |      |       |
|      | 10001 - 1.204V                 |      |       |
| 3:1  | Reserved.                      | -    | -     |
| 0    | EN: enable brown-out detection | RW   | 0x1   |
|      | 0=not enabled, 1=enabled       |      |       |

#### <span id="page-465-1"></span>**[POWMAN:](#page-455-0) BOD\_LP\_ENTRY Register**

**Offset**: 0x20

**Description**

Brown-out Detection Low Power Entry Settings

*Table 485. BOD\_LP\_ENTRY Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31:9 | Reserved.   | -    | -     |

| Bits | Description                    | Type | Reset |
|------|--------------------------------|------|-------|
| 8:4  | VSEL: threshold select         | RW   | 0x0b  |
|      | 00000 - 0.473V                 |      |       |
|      | 00001 - 0.516V                 |      |       |
|      | 00010 - 0.559V                 |      |       |
|      | 00011 - 0.602V                 |      |       |
|      | 00100 - 0.645VS                |      |       |
|      | 00101 - 0.688V                 |      |       |
|      | 00110 - 0.731V                 |      |       |
|      | 00111 - 0.774V                 |      |       |
|      | 01000 - 0.817V                 |      |       |
|      | 01001 - 0.860V (default)       |      |       |
|      | 01010 - 0.903V                 |      |       |
|      | 01011 - 0.946V                 |      |       |
|      | 01100 - 0.989V                 |      |       |
|      | 01101 - 1.032V                 |      |       |
|      | 01110 - 1.075V                 |      |       |
|      | 01111 - 1.118V                 |      |       |
|      | 10000 - 1.161                  |      |       |
|      | 10001 - 1.204V                 |      |       |
| 3:1  | Reserved.                      | -    | -     |
| 0    | EN: enable brown-out detection | RW   | 0x0   |
|      | 0=not enabled, 1=enabled       |      |       |

# <span id="page-466-0"></span>**[POWMAN:](#page-455-0) BOD\_LP\_EXIT Register**

**Offset**: 0x24 **Description**

Brown-out Detection Low Power Exit Settings

*Table 486. BOD\_LP\_EXIT Register*

| Bits | Description              | Type | Reset |
|------|--------------------------|------|-------|
| 31:9 | Reserved.                | -    | -     |
| 8:4  | VSEL: threshold select   | RW   | 0x0b  |
|      | 00000 - 0.473V           |      |       |
|      | 00001 - 0.516V           |      |       |
|      | 00010 - 0.559V           |      |       |
|      | 00011 - 0.602V           |      |       |
|      | 00100 - 0.645VS          |      |       |
|      | 00101 - 0.688V           |      |       |
|      | 00110 - 0.731V           |      |       |
|      | 00111 - 0.774V           |      |       |
|      | 01000 - 0.817V           |      |       |
|      | 01001 - 0.860V (default) |      |       |
|      | 01010 - 0.903V           |      |       |
|      | 01011 - 0.946V           |      |       |
|      | 01100 - 0.989V           |      |       |
|      | 01101 - 1.032V           |      |       |
|      | 01110 - 1.075V           |      |       |
|      | 01111 - 1.118V           |      |       |
|      | 10000 - 1.161            |      |       |
|      | 10001 - 1.204V           |      |       |
| 3:1  | Reserved.                | -    | -     |

| Bits | Description                    | Type | Reset |
|------|--------------------------------|------|-------|
| 0    | EN: enable brown-out detection | RW   | 0x1   |
|      | 0=not enabled, 1=enabled       |      |       |

#### <span id="page-467-1"></span>**[POWMAN:](#page-455-0) LPOSC Register**

**Offset**: 0x28

#### **Description**

Low power oscillator control register.

*Table 487. LPOSC Register*

| Bits  | Description                                                                                         | Type | Reset |
|-------|-----------------------------------------------------------------------------------------------------|------|-------|
| 31:10 | Reserved.                                                                                           | -    | -     |
| 9:4   | TRIM: Frequency trim - the trim step is typically 1% of the reset frequency, but<br>can be up to 3% | RW   | 0x20  |
| 3:2   | Reserved.                                                                                           | -    | -     |
| 1:0   | MODE: This feature has been removed                                                                 | RW   | 0x3   |

# <span id="page-467-0"></span>**[POWMAN:](#page-455-0) CHIP\_RESET Register**

**Offset**: 0x2c

#### **Description**

Chip reset control and status

*Table 488. CHIP\_RESET Register*

| Bits  | Description                                                          | Type | Reset |
|-------|----------------------------------------------------------------------|------|-------|
| 31:29 | Reserved.                                                            | -    | -     |
| 28    | HAD_WATCHDOG_RESET_PSM: Last reset was a watchdog timeout which      | RO   | 0x0   |
|       | was configured to reset the power-on state machine                   |      |       |
|       | This resets:                                                         |      |       |
|       | double_tap flag no                                                   |      |       |
|       | DP no                                                                |      |       |
|       | RPAP no                                                              |      |       |
|       | rescue_flag no                                                       |      |       |
|       | timer no                                                             |      |       |
|       | powman no                                                            |      |       |
|       | swcore no                                                            |      |       |
|       | psm yes                                                              |      |       |
|       | and does not change the power state                                  |      |       |
| 27    | HAD_HZD_SYS_RESET_REQ: Last reset was a system reset from the hazard | RO   | 0x0   |
|       | debugger                                                             |      |       |
|       | This resets:                                                         |      |       |
|       | double_tap flag no                                                   |      |       |
|       | DP no                                                                |      |       |
|       | RPAP no                                                              |      |       |
|       | rescue_flag no                                                       |      |       |
|       | timer no                                                             |      |       |
|       | powman no                                                            |      |       |
|       | swcore no                                                            |      |       |
|       | psm yes                                                              |      |       |
|       | and does not change the power state                                  |      |       |

| Bits | Description                                                                                                                                                                                                                                                                        | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 26   | HAD_GLITCH_DETECT: Last reset was due to a power supply glitch<br>This resets:<br>double_tap flag no<br>DP no<br>RPAP no<br>rescue_flag no<br>timer no<br>powman no<br>swcore no<br>psm yes<br>and does not change the power state                                                 | RO   | 0x0   |
| 25   | HAD_SWCORE_PD: Last reset was a switched core powerdown<br>This resets:<br>double_tap flag no<br>DP no<br>RPAP no<br>rescue_flag no<br>timer no<br>powman no<br>swcore yes<br>psm yes<br>then starts the power sequencer                                                           | RO   | 0x0   |
| 24   | HAD_WATCHDOG_RESET_SWCORE: Last reset was a watchdog timeout<br>which was configured to reset the switched-core<br>This resets:<br>double_tap flag no<br>DP no<br>RPAP no<br>rescue_flag no<br>timer no<br>powman no<br>swcore yes<br>psm yes<br>then starts the power sequencer   | RO   | 0x0   |
| 23   | HAD_WATCHDOG_RESET_POWMAN: Last reset was a watchdog timeout<br>which was configured to reset the power manager<br>This resets:<br>double_tap flag no<br>DP no<br>RPAP no<br>rescue_flag no<br>timer yes<br>powman yes<br>swcore yes<br>psm yes<br>then starts the power sequencer | RO   | 0x0   |

| Bits | Description                                                                                                                                                                                                                                                                                             | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 22   | HAD_WATCHDOG_RESET_POWMAN_ASYNC: Last reset was a watchdog<br>timeout which was configured to reset the power manager asynchronously<br>This resets:<br>double_tap flag no<br>DP no<br>RPAP no<br>rescue_flag no<br>timer yes<br>powman yes<br>swcore yes<br>psm yes<br>then starts the power sequencer | RO   | 0x0   |
| 21   | HAD_RESCUE: Last reset was a rescue reset from the debugger<br>This resets:<br>double_tap flag no<br>DP no<br>RPAP no<br>rescue_flag no, it sets this flag<br>timer yes<br>powman yes<br>swcore yes<br>psm yes<br>then starts the power sequencer                                                       | RO   | 0x0   |
| 20   | Reserved.                                                                                                                                                                                                                                                                                               | -    | -     |
| 19   | HAD_DP_RESET_REQ: Last reset was an reset request from the arm debugger<br>This resets:<br>double_tap flag no<br>DP no<br>RPAP no<br>rescue_flag yes<br>timer yes<br>powman yes<br>swcore yes<br>psm yes<br>then starts the power sequencer                                                             | RO   | 0x0   |
| 18   | HAD_RUN_LOW: Last reset was from the RUN pin<br>This resets:<br>double_tap flag no<br>DP yes<br>RPAP yes<br>rescue_flag yes<br>timer yes<br>powman yes<br>swcore yes<br>psm yes<br>then starts the power sequencer                                                                                      | RO   | 0x0   |

| Bits | Description                                                                     | Type | Reset |
|------|---------------------------------------------------------------------------------|------|-------|
| 17   | HAD_BOR: Last reset was from the brown-out detection block                      | RO   | 0x0   |
|      | This resets:                                                                    |      |       |
|      | double_tap flag yes                                                             |      |       |
|      | DP yes                                                                          |      |       |
|      | RPAP yes                                                                        |      |       |
|      | rescue_flag yes                                                                 |      |       |
|      | timer yes                                                                       |      |       |
|      | powman yes                                                                      |      |       |
|      | swcore yes                                                                      |      |       |
|      | psm yes                                                                         |      |       |
|      | then starts the power sequencer                                                 |      |       |
| 16   | HAD_POR: Last reset was from the power-on reset                                 | RO   | 0x0   |
|      | This resets:                                                                    |      |       |
|      | double_tap flag yes                                                             |      |       |
|      | DP yes                                                                          |      |       |
|      | RPAP yes                                                                        |      |       |
|      | rescue_flag yes                                                                 |      |       |
|      | timer yes                                                                       |      |       |
|      | powman yes                                                                      |      |       |
|      | swcore yes                                                                      |      |       |
|      | psm yes                                                                         |      |       |
|      | then starts the power sequencer                                                 |      |       |
| 15:5 | Reserved.                                                                       | -    | -     |
| 4    | RESCUE_FLAG: This is set by a rescue reset from the RP-AP.                      | WC   | 0x0   |
|      | Its purpose is to halt before the bootrom before booting from flash in order to |      |       |
|      | recover from a boot lock-up.                                                    |      |       |
|      | The debugger can then attach once the bootrom has been halted and flash         |      |       |
|      | some working code that does not lock up.                                        |      |       |
| 3:1  | Reserved.                                                                       | -    | -     |
| 0    | DOUBLE_TAP: This flag is set by double-tapping RUN. It tells bootcode to go     | RW   | 0x0   |
|      | into the bootloader.                                                            |      |       |

# <span id="page-470-0"></span>**[POWMAN:](#page-455-0) WDSEL Register**

#### **Offset**: 0x30

#### **Description**

Allows a watchdog reset to reset the internal state of powman in addition to the power-on state machine (PSM). Note that powman ignores watchdog resets that do not select at least the CLOCKS stage or earlier stages in the PSM. If using these bits, it's recommended to set PSM\_WDSEL to all-ones in addition to the desired bits in this register. Failing to select CLOCKS or earlier will result in the POWMAN\_WDSEL register having no effect.

*Table 489. WDSEL Register*

| Bits  | Description                                                                                                                                                                                                                                                                   | Type | Reset |
|-------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:13 | Reserved.                                                                                                                                                                                                                                                                     | -    | -     |
| 12    | RESET_PSM: If set to 1, a watchdog reset will run the full power-on state<br>machine (PSM) sequence<br>From a user perspective it is the same as setting RSM_WDSEL_PROC_COLD<br>From a hardware debug perspective it has the same effect as a reset from a<br>glitch detector | RW   | 0x0   |
| 11:9  | Reserved.                                                                                                                                                                                                                                                                     | -    | -     |

| Bits | Description                                                                                                                                                                                                                                                                                                                                    | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 8    | RESET_SWCORE: If set to 1, a watchdog reset will reset the switched core<br>power domain and run the full power-on state machine (PSM) sequence<br>From a user perspective it is the same as setting RSM_WDSEL_PROC_COLD<br>From a hardware debug perspective it has the same effect as a power-on<br>reset for the switched core power domain | RW   | 0x0   |
| 7:5  | Reserved.                                                                                                                                                                                                                                                                                                                                      | -    | -     |
| 4    | RESET_POWMAN: If set to 1, a watchdog reset will restore powman defaults,<br>reset the timer, reset the switched core power domain<br>and run the full power-on state machine (PSM) sequence<br>This relies on clk_ref running. Use reset_powman_async if that may not be true                                                                 | RW   | 0x0   |
| 3:1  | Reserved.                                                                                                                                                                                                                                                                                                                                      | -    | -     |
| 0    | RESET_POWMAN_ASYNC: If set to 1, a watchdog reset will restore powman<br>defaults, reset the timer,<br>reset the switched core domain and run the full power-on state machine<br>(PSM) sequence<br>This does not rely on clk_ref running                                                                                                       | RW   | 0x0   |

# <span id="page-471-0"></span>**[POWMAN:](#page-455-0) SEQ\_CFG Register**

## **Offset**: 0x34 **Description**

For configuration of the power sequencer

Writes are ignored while POWMAN\_STATE\_CHANGING=1

*Table 490. SEQ\_CFG Register*

| Bits  | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Type | Reset |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:21 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                       | -    | -     |
| 20    | USING_FAST_POWCK: 0 indicates the POWMAN clock is running from the low<br>power oscillator (32kHz)<br>1 indicates the POWMAN clock is running from the reference clock (2-50MHz)                                                                                                                                                                                                                                                                                | RO   | 0x1   |
| 19:18 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                       | -    | -     |
| 17    | USING_BOD_LP: Indicates the brown-out detector (BOD) mode<br>0 = BOD high power mode which is the default<br>1 = BOD low power mode                                                                                                                                                                                                                                                                                                                             | RO   | 0x0   |
| 16    | USING_VREG_LP: Indicates the voltage regulator (VREG) mode<br>0 = VREG high power mode which is the default<br>1 = VREG low power mode                                                                                                                                                                                                                                                                                                                          | RO   | 0x0   |
| 15:13 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                       | -    | -     |
| 12    | USE_FAST_POWCK: selects the reference clock (clk_ref) as the source of the<br>POWMAN clock when switched-core is powered. The POWMAN clock always<br>switches to the slow clock (lposc) when switched-core is powered down<br>because the fast clock stops running.<br>0 always run the POWMAN clock from the slow clock (lposc)<br>1 run the POWMAN clock from the fast clock when available<br>This setting takes effect when a power up sequence is next run | RW   | 0x1   |
| 11:9  | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                       | -    | -     |

| Bits | Description                                                                                                                                                                                                     | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 8    | RUN_LPOSC_IN_LP: Set to 0 to stop the low power osc when the switched<br>core is powered down, which is unwise if using it to clock the timer<br>This setting takes effect when the swcore is next powered down | RW   | 0x1   |
| 7    | USE_BOD_HP: Set to 0 to prevent automatic switching to bod high power<br>mode when switched-core is powered up<br>This setting takes effect when the swcore is next powered up                                  | RW   | 0x1   |
| 6    | USE_BOD_LP: Set to 0 to prevent automatic switching to bod low power mode<br>when switched-core is powered down<br>This setting takes effect when the swcore is next powered down                               | RW   | 0x1   |
| 5    | USE_VREG_HP: Set to 0 to prevent automatic switching to vreg high power<br>mode when switched-core is powered up<br>This setting takes effect when the swcore is next powered up                                | RW   | 0x1   |
| 4    | USE_VREG_LP: Set to 0 to prevent automatic switching to vreg low power<br>mode when switched-core is powered down<br>This setting takes effect when the swcore is next powered down                             | RW   | 0x1   |
| 3:2  | Reserved.                                                                                                                                                                                                       | -    | -     |
| 1    | HW_PWRUP_SRAM0: Specifies the power state of SRAM0 when powering up<br>swcore from a low power state (P1.xxx) to a high power state (P0.0xx).<br>0=power-up<br>1=no change                                      | RW   | 0x0   |
| 0    | HW_PWRUP_SRAM1: Specifies the power state of SRAM1 when powering up<br>swcore from a low power state (P1.xxx) to a high power state (P0.0xx).<br>0=power-up<br>1=no change                                      | RW   | 0x0   |

# <span id="page-472-0"></span>**[POWMAN:](#page-455-0) STATE Register**

## **Offset**: 0x38 **Description**

This register controls the power state of the 4 power domains.

The current power state is indicated in POWMAN\_STATE\_CURRENT which is read-only.

To change the state, write to POWMAN\_STATE\_REQ.

The coding of POWMAN\_STATE\_CURRENT & POWMAN\_STATE\_REQ corresponds to the power states defined in the datasheet:

bit 3 = SWCORE

bit 2 = XIP cache

bit 1 = SRAM0

bit 0 = SRAM1

0 = powered up

1 = powered down

When POWMAN\_STATE\_REQ is written, the POWMAN\_STATE\_WAITING flag is set while the Power Manager determines what is required. If an invalid transition is requested the Power Manager will still register the request in POWMAN\_STATE\_REQ but will also set the POWMAN\_BAD\_REQ flag. It will then implement the power-up requests and ignore the power down requests. To do nothing would risk entering an unrecoverable lock-up state. Invalid requests are: any combination of power up and power down requests any request that results in swcore being powered and xip unpowered If the request is to power down the switched-core domain then POWMAN\_STATE\_WAITING stays active until the processors halt. During this time the POWMAN\_STATE\_REQ field can be re-written to change or cancel the request. When the power state transition begins the POWMAN\_STATE\_WAITING\_flag is cleared, the POWMAN\_STATE\_CHANGING flag is set and POWMAN register writes are ignored until the transition completes.

*Table 491. STATE Register*

| Bits  | Description                                                                                                                                  | Type | Reset |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:14 | Reserved.                                                                                                                                    | -    | -     |
| 13    | CHANGING: Indicates a power state change is in progress                                                                                      | RO   | 0x0   |
| 12    | WAITING: Indicates the power manager has received a state change request<br>and is waiting for other actions to complete before executing it | RO   | 0x0   |
| 11    | BAD_HW_REQ: Invalid hardware initiated state request, power up requests<br>actioned, power down requests ignored                             | RO   | 0x0   |
| 10    | BAD_SW_REQ: Invalid software initiated state request ignored                                                                                 | RO   | 0x0   |
| 9     | PWRUP_WHILE_WAITING: Indicates that a power state change request was<br>ignored because of a pending power state change request              | WC   | 0x0   |
| 8     | REQ_IGNORED: Indicates that a software state change request was ignored<br>because it clashed with an ongoing hardware or debugger request   | WC   | 0x0   |
| 7:4   | REQ: This is written by software or hardware to request a new power state                                                                    | RW   | 0x0   |
| 3:0   | CURRENT: Indicates the current power state                                                                                                   | RO   | 0xf   |

# <span id="page-473-1"></span>**[POWMAN:](#page-455-0) POW\_FASTDIV Register**

**Offset**: 0x3c

*Table 492. POW\_FASTDIV Register*

| Bits  | Description                                                                                                                                                                                                                     | Type | Reset |
|-------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:11 | Reserved.                                                                                                                                                                                                                       | -    | -     |
| 10:0  | divides the POWMAN clock to provide a tick for the delay module and state<br>machines<br>when clk_pow is running from the slow clock it is not divided<br>when clk_pow is running from the fast clock it is divided by tick_div | RW   | 0x040 |

# <span id="page-473-2"></span>**[POWMAN:](#page-455-0) POW\_DELAY Register**

**Offset**: 0x40 **Description**

power state machine delays

*Table 493. POW\_DELAY Register*

| Bits  | Description                                                                                                                                                  | Type | Reset |
|-------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:16 | Reserved.                                                                                                                                                    | -    | -     |
| 15:8  | SRAM_STEP: timing between the sram0 and sram1 power state machine<br>steps<br>measured in units of the powman tick period (>=1us), 0 gives a delay of 1 unit | RW   | 0x20  |
| 7:4   | XIP_STEP: timing between the xip power state machine steps<br>measured in units of the lposc period, 0 gives a delay of 1 unit                               | RW   | 0x1   |
| 3:0   | SWCORE_STEP: timing between the swcore power state machine steps<br>measured in units of the lposc period, 0 gives a delay of 1 unit                         | RW   | 0x1   |

#### <span id="page-473-0"></span>**[POWMAN:](#page-455-0) EXT\_CTRL0 Register**

**Offset**: 0x44 **Description**

Configures a gpio as a power mode aware control output

*Table 494. EXT\_CTRL0 Register*

| Bits  | Description                                                              | Type | Reset |
|-------|--------------------------------------------------------------------------|------|-------|
| 31:15 | Reserved.                                                                | -    | -     |
| 14    | LP_EXIT_STATE: output level when exiting the low power state             | RW   | 0x0   |
| 13    | LP_ENTRY_STATE: output level when entering the low power state           | RW   | 0x0   |
| 12    | INIT_STATE                                                               | RW   | 0x0   |
| 11:9  | Reserved.                                                                | -    | -     |
| 8     | INIT                                                                     | RW   | 0x0   |
| 7:6   | Reserved.                                                                | -    | -     |
| 5:0   | GPIO_SELECT: selects from gpio 0→30<br>set to 31 to disable this feature | RW   | 0x3f  |

# <span id="page-474-0"></span>**[POWMAN:](#page-455-0) EXT\_CTRL1 Register**

**Offset**: 0x48

#### **Description**

Configures a gpio as a power mode aware control output

*Table 495. EXT\_CTRL1 Register*

| Bits  | Description                                                              | Type | Reset |
|-------|--------------------------------------------------------------------------|------|-------|
| 31:15 | Reserved.                                                                | -    | -     |
| 14    | LP_EXIT_STATE: output level when exiting the low power state             | RW   | 0x0   |
| 13    | LP_ENTRY_STATE: output level when entering the low power state           | RW   | 0x0   |
| 12    | INIT_STATE                                                               | RW   | 0x0   |
| 11:9  | Reserved.                                                                | -    | -     |
| 8     | INIT                                                                     | RW   | 0x0   |
| 7:6   | Reserved.                                                                | -    | -     |
| 5:0   | GPIO_SELECT: selects from gpio 0→30<br>set to 31 to disable this feature | RW   | 0x3f  |

### <span id="page-474-1"></span>**[POWMAN:](#page-455-0) EXT\_TIME\_REF Register**

#### **Offset**: 0x4c

#### **Description**

Select a GPIO to use as a time reference, the source can be used to drive the low power clock at 32kHz, or to provide a 1ms tick to the timer, or provide a 1Hz tick to the timer. The tick selection is controlled by the POWMAN\_TIMER register.

*Table 496. EXT\_TIME\_REF Register*

| Bits | Description                                                                                                                                            | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:5 | Reserved.                                                                                                                                              | -    | -     |
| 4    | DRIVE_LPCK: Use the selected GPIO to drive the 32kHz low power clock, in<br>place of LPOSC. This field must only be written when<br>POWMAN_TIMER_RUN=0 | RW   | 0x0   |
| 3:2  | Reserved.                                                                                                                                              | -    | -     |

| Bits | Description            | Type | Reset |
|------|------------------------|------|-------|
| 1:0  | SOURCE_SEL: 0 → gpio12 | RW   | 0x0   |
|      | 1 → gpio20             |      |       |
|      | 2 → gpio14             |      |       |
|      | 3 → gpio22             |      |       |
|      | Enumerated values:     |      |       |
|      | 0x0 → GPIO12           |      |       |
|      | 0x1 → GPIO20           |      |       |
|      | 0x2 → GPIO14           |      |       |
|      | 0x3 → GPIO22           |      |       |

# <span id="page-475-0"></span>**[POWMAN:](#page-455-0) LPOSC\_FREQ\_KHZ\_INT Register**

**Offset**: 0x50 **Description**

Informs the AON Timer of the integer component of the clock frequency when running off the LPOSC.

*Table 497. LPOSC\_FREQ\_KHZ\_IN T Register*

| Bits | Description                                                                                                                                                                   | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:6 | Reserved.                                                                                                                                                                     | -    | -     |
| 5:0  | Integer component of the LPOSC or GPIO clock source frequency in kHz.<br>Default = 32 This field must only be written when POWMAN_TIMER_RUN=0 or<br>POWMAN_TIMER_USING_XOSC=1 | RW   | 0x20  |

# <span id="page-475-1"></span>**[POWMAN:](#page-455-0) LPOSC\_FREQ\_KHZ\_FRAC Register**

**Offset**: 0x54

#### **Description**

Informs the AON Timer of the fractional component of the clock frequency when running off the LPOSC.

*Table 498. LPOSC\_FREQ\_KHZ\_FR AC Register*

| Bits  | Description                                                                                                                                                                         | Type | Reset  |
|-------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                           | -    | -      |
| 15:0  | Fractional component of the LPOSC or GPIO clock source frequency in kHz.<br>Default = 0.768 This field must only be written when POWMAN_TIMER_RUN=0<br>or POWMAN_TIMER_USING_XOSC=1 | RW   | 0xc49c |

### <span id="page-475-2"></span>**[POWMAN:](#page-455-0) XOSC\_FREQ\_KHZ\_INT Register**

**Offset**: 0x58

#### **Description**

Informs the AON Timer of the integer component of the clock frequency when running off the XOSC.

*Table 499. XOSC\_FREQ\_KHZ\_INT Register*

| Bits  | Description                                                                                                                                                           | Type | Reset  |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                             | -    | -      |
| 15:0  | Integer component of the XOSC frequency in kHz. Default = 12000 Must be >1<br>This field must only be written when POWMAN_TIMER_RUN=0 or<br>POWMAN_TIMER_USING_XOSC=0 | RW   | 0x2ee0 |

# <span id="page-476-0"></span>**[POWMAN:](#page-455-0) XOSC\_FREQ\_KHZ\_FRAC Register**

**Offset**: 0x5c

#### **Description**

Informs the AON Timer of the fractional component of the clock frequency when running off the XOSC.

*Table 500. XOSC\_FREQ\_KHZ\_FRA C Register*

| Bits  | Description                                                                                                                                | Type | Reset  |
|-------|--------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                  | -    | -      |
| 15:0  | Fractional component of the XOSC frequency in kHz. This field must only be<br>written when POWMAN_TIMER_RUN=0 or POWMAN_TIMER_USING_XOSC=0 | RW   | 0x0000 |

# <span id="page-476-1"></span>**[POWMAN:](#page-455-0) SET\_TIME\_63TO48 Register**

**Offset**: 0x60

*Table 501. SET\_TIME\_63TO48 Register*

| Bits  | Description                                                                                                                                                                 | Type | Reset  |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                   | -    | -      |
| 15:0  | For setting the time, do not use for reading the time, use<br>POWMAN_READ_TIME_UPPER and POWMAN_READ_TIME_LOWER. This field<br>must only be written when POWMAN_TIMER_RUN=0 | RW   | 0x0000 |

#### <span id="page-476-2"></span>**[POWMAN:](#page-455-0) SET\_TIME\_47TO32 Register**

**Offset**: 0x64

*Table 502. SET\_TIME\_47TO32 Register*

| Bits  | Description                                                                                                                                                                 | Type | Reset  |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                   | -    | -      |
| 15:0  | For setting the time, do not use for reading the time, use<br>POWMAN_READ_TIME_UPPER and POWMAN_READ_TIME_LOWER. This field<br>must only be written when POWMAN_TIMER_RUN=0 | RW   | 0x0000 |

#### <span id="page-476-3"></span>**[POWMAN:](#page-455-0) SET\_TIME\_31TO16 Register**

**Offset**: 0x68

*Table 503. SET\_TIME\_31TO16 Register*

| Bits  | Description                                                                                                                                                                 | Type | Reset  |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                   | -    | -      |
| 15:0  | For setting the time, do not use for reading the time, use<br>POWMAN_READ_TIME_UPPER and POWMAN_READ_TIME_LOWER. This field<br>must only be written when POWMAN_TIMER_RUN=0 | RW   | 0x0000 |

<span id="page-476-4"></span>**[POWMAN:](#page-455-0) SET\_TIME\_15TO0 Register**

**Offset**: 0x6c

*Table 504. SET\_TIME\_15TO0 Register*

| Bits  | Description                                                                                                                                                                 | Type | Reset  |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                   | -    | -      |
| 15:0  | For setting the time, do not use for reading the time, use<br>POWMAN_READ_TIME_UPPER and POWMAN_READ_TIME_LOWER. This field<br>must only be written when POWMAN_TIMER_RUN=0 | RW   | 0x0000 |

# <span id="page-477-2"></span>**[POWMAN:](#page-455-0) READ\_TIME\_UPPER Register**

**Offset**: 0x70

*Table 505. READ\_TIME\_UPPER Register*

| Bits | Description                                                                                                                                                                                                                                   | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | For reading bits 63:32 of the timer. When reading all 64 bits it is possible for<br>the LOWER count to rollover during the read. It is recommended to read<br>UPPER, then LOWER, then re-read UPPER and, if it has changed, re-read<br>LOWER. | RO   | 0x00000000 |

# <span id="page-477-3"></span>**[POWMAN:](#page-455-0) READ\_TIME\_LOWER Register**

**Offset**: 0x74

*Table 506. READ\_TIME\_LOWER Register*

| Bits | Description                         | Type | Reset      |
|------|-------------------------------------|------|------------|
| 31:0 | For reading bits 31:0 of the timer. | RO   | 0x00000000 |

# <span id="page-477-1"></span>**[POWMAN:](#page-455-0) ALARM\_TIME\_63TO48 Register**

**Offset**: 0x78

*Table 507. ALARM\_TIME\_63TO48 Register*

| Bits  | Description                                              | Type | Reset  |
|-------|----------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                | -    | -      |
| 15:0  | This field must only be written when POWMAN_ALARM_ENAB=0 | RW   | 0x0000 |

# <span id="page-477-4"></span>**[POWMAN:](#page-455-0) ALARM\_TIME\_47TO32 Register**

**Offset**: 0x7c

*Table 508. ALARM\_TIME\_47TO32 Register*

| Bits  | Description                                              | Type | Reset  |
|-------|----------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                | -    | -      |
| 15:0  | This field must only be written when POWMAN_ALARM_ENAB=0 | RW   | 0x0000 |

# <span id="page-477-5"></span>**[POWMAN:](#page-455-0) ALARM\_TIME\_31TO16 Register**

**Offset**: 0x80

*Table 509. ALARM\_TIME\_31TO16 Register*

| Bits  | Description                                              | Type | Reset  |
|-------|----------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                | -    | -      |
| 15:0  | This field must only be written when POWMAN_ALARM_ENAB=0 | RW   | 0x0000 |

### <span id="page-477-0"></span>**[POWMAN:](#page-455-0) ALARM\_TIME\_15TO0 Register**

**Offset**: 0x84

*Table 510. ALARM\_TIME\_15TO0 Register*

| Bits  | Description                                              | Type | Reset  |
|-------|----------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                | -    | -      |
| 15:0  | This field must only be written when POWMAN_ALARM_ENAB=0 | RW   | 0x0000 |

# <span id="page-478-1"></span>**[POWMAN:](#page-455-0) TIMER Register**

#### **Offset**: 0x88

*Table 511. TIMER Register*

| Bits  | Description                                                                                                                                                                                                                                                                                 | Type | Reset |
|-------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:20 | Reserved.                                                                                                                                                                                                                                                                                   | -    | -     |
| 19    | USING_GPIO_1HZ: Timer is synchronised to a 1hz gpio source                                                                                                                                                                                                                                  | RO   | 0x0   |
| 18    | USING_GPIO_1KHZ: Timer is running from a 1khz gpio source                                                                                                                                                                                                                                   | RO   | 0x0   |
| 17    | USING_LPOSC: Timer is running from lposc                                                                                                                                                                                                                                                    | RO   | 0x0   |
| 16    | USING_XOSC: Timer is running from xosc                                                                                                                                                                                                                                                      | RO   | 0x0   |
| 15:14 | Reserved.                                                                                                                                                                                                                                                                                   | -    | -     |
| 13    | USE_GPIO_1HZ: Selects the gpio source as the reference for the sec counter.<br>The msec counter will continue to use the lposc or xosc reference.                                                                                                                                           | RW   | 0x0   |
| 12:11 | Reserved.                                                                                                                                                                                                                                                                                   | -    | -     |
| 10    | USE_GPIO_1KHZ: switch to gpio as the source of the 1kHz timer tick                                                                                                                                                                                                                          | SC   | 0x0   |
| 9     | USE_XOSC: switch to xosc as the source of the 1kHz timer tick                                                                                                                                                                                                                               | SC   | 0x0   |
| 8     | USE_LPOSC: Switch to lposc as the source of the 1kHz timer tick                                                                                                                                                                                                                             | SC   | 0x0   |
| 7     | Reserved.                                                                                                                                                                                                                                                                                   | -    | -     |
| 6     | ALARM: Alarm has fired. Write to 1 to clear the alarm.                                                                                                                                                                                                                                      | WC   | 0x0   |
| 5     | PWRUP_ON_ALARM: Alarm wakes the chip from low power mode                                                                                                                                                                                                                                    | RW   | 0x0   |
| 4     | ALARM_ENAB: Enables the alarm. The alarm must be disabled while writing<br>the alarm time.                                                                                                                                                                                                  | RW   | 0x0   |
| 3     | Reserved.                                                                                                                                                                                                                                                                                   | -    | -     |
| 2     | CLEAR: Clears the timer, does not disable the timer and does not affect the<br>alarm. This control can be written at any time.                                                                                                                                                              | SC   | 0x0   |
| 1     | RUN: Timer enable. Setting this bit causes the timer to begin counting up from<br>its current value. Clearing this bit stops the timer from counting.                                                                                                                                       | RW   | 0x0   |
|       | Before enabling the timer, set the POWMAN_LPOSC_FREQ* and<br>POWMAN_XOSC_FREQ* registers to configure the count rate, and initialise the<br>current time by writing to SET_TIME_63TO48 through SET_TIME_15TO0. You<br>must not write to the SET_TIME_x registers when the timer is running. |      |       |
|       | Once configured, start the timer by setting POWMAN_TIMER_RUN=1. This will<br>start the timer running from the LPOSC. When the XOSC is available switch the<br>reference clock to XOSC then select it as the timer clock by setting<br>POWMAN_TIMER_USE_XOSC=1                               |      |       |
| 0     | NONSEC_WRITE: Control whether Non-secure software can write to the timer<br>registers. All other registers are hardwired to be inaccessible to Non-secure.                                                                                                                                  | RW   | 0x0   |

# <span id="page-478-0"></span>**[POWMAN:](#page-455-0) PWRUP0 Register**

#### **Offset**: 0x8c

#### **Description**

4 GPIO powerup events can be configured to wake the chip up from a low power state.

The pwrups are level/edge sensitive and can be set to trigger on a high/rising or low/falling event The number of gpios available depends on the package option. An invalid selection will be ignored source = 0 selects gpio0

1. +

2. + source = 47 selects gpio47 source = 48 selects qspi\_ss source = 49 selects qspi\_sd0 source = 50 selects qspi\_sd1 source = 51 selects qspi\_sd2 source = 52 selects qspi\_sd3 source = 53 selects qspi\_sclk level = 0 triggers the pwrup when the source is low

level = 1 triggers the pwrup when the source is high

*Table 512. PWRUP0 Register*

| Bits  | Description                                                                                                                                                                                                                      | Type | Reset |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:11 | Reserved.                                                                                                                                                                                                                        | -    | -     |
| 10    | RAW_STATUS: Value of selected gpio pin (only if enable == 1)                                                                                                                                                                     | RO   | 0x0   |
| 9     | STATUS: Status of gpio wakeup. Write to 1 to clear a latched edge detect.                                                                                                                                                        | WC   | 0x0   |
| 8     | MODE: Edge or level detect. Edge will detect a 0 to 1 transition (or 1 to 0<br>transition). Level will detect a 1 or 0. Both types of event get latched into the<br>current_pwrup_req register.                                  | RW   | 0x0   |
|       | Enumerated values:                                                                                                                                                                                                               |      |       |
|       | 0x0 → LEVEL                                                                                                                                                                                                                      |      |       |
|       | 0x1 → EDGE                                                                                                                                                                                                                       |      |       |
| 7     | DIRECTION                                                                                                                                                                                                                        | RW   | 0x0   |
|       | Enumerated values:                                                                                                                                                                                                               |      |       |
|       | 0x0 → LOW_FALLING                                                                                                                                                                                                                |      |       |
|       | 0x1 → HIGH_RISING                                                                                                                                                                                                                |      |       |
| 6     | ENABLE: Set to 1 to enable the wakeup source. Set to 0 to disable the wakeup<br>source and clear a pending wakeup event.<br>If using edge detect a latched edge needs to be cleared by writing 1 to the<br>status register also. | RW   | 0x0   |
| 5:0   | SOURCE                                                                                                                                                                                                                           | RW   | 0x3f  |

#### <span id="page-479-0"></span>**[POWMAN:](#page-455-0) PWRUP1 Register**

## **Offset**: 0x90 **Description**

4 GPIO powerup events can be configured to wake the chip up from a low power state.

The pwrups are level/edge sensitive and can be set to trigger on a high/rising or low/falling event The number of gpios available depends on the package option. An invalid selection will be ignored source = 0 selects gpio0

1. +

```
2. + source = 47 selects gpio47
  source = 48 selects qspi_ss
  source = 49 selects qspi_sd0
  source = 50 selects qspi_sd1
  source = 51 selects qspi_sd2
  source = 52 selects qspi_sd3
  source = 53 selects qspi_sclk
  level = 0 triggers the pwrup when the source is low
  level = 1 triggers the pwrup when the source is high
```

*Table 513. PWRUP1 Register*

| Bits  | Description                                                                                                                                                                                                                      | Type | Reset |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:11 | Reserved.                                                                                                                                                                                                                        | -    | -     |
| 10    | RAW_STATUS: Value of selected gpio pin (only if enable == 1)                                                                                                                                                                     | RO   | 0x0   |
| 9     | STATUS: Status of gpio wakeup. Write to 1 to clear a latched edge detect.                                                                                                                                                        | WC   | 0x0   |
| 8     | MODE: Edge or level detect. Edge will detect a 0 to 1 transition (or 1 to 0<br>transition). Level will detect a 1 or 0. Both types of event get latched into the<br>current_pwrup_req register.                                  | RW   | 0x0   |
|       | Enumerated values:                                                                                                                                                                                                               |      |       |
|       | 0x0 → LEVEL                                                                                                                                                                                                                      |      |       |
|       | 0x1 → EDGE                                                                                                                                                                                                                       |      |       |
| 7     | DIRECTION                                                                                                                                                                                                                        | RW   | 0x0   |
|       | Enumerated values:                                                                                                                                                                                                               |      |       |
|       | 0x0 → LOW_FALLING                                                                                                                                                                                                                |      |       |
|       | 0x1 → HIGH_RISING                                                                                                                                                                                                                |      |       |
| 6     | ENABLE: Set to 1 to enable the wakeup source. Set to 0 to disable the wakeup<br>source and clear a pending wakeup event.<br>If using edge detect a latched edge needs to be cleared by writing 1 to the<br>status register also. | RW   | 0x0   |
| 5:0   | SOURCE                                                                                                                                                                                                                           | RW   | 0x3f  |

# <span id="page-480-0"></span>**[POWMAN:](#page-455-0) PWRUP2 Register**

#### **Offset**: 0x94

#### **Description**

4 GPIO powerup events can be configured to wake the chip up from a low power state.

The pwrups are level/edge sensitive and can be set to trigger on a high/rising or low/falling event The number of gpios available depends on the package option. An invalid selection will be ignored source = 0 selects gpio0

1. +

```
2. + source = 47 selects gpio47
  source = 48 selects qspi_ss
  source = 49 selects qspi_sd0
  source = 50 selects qspi_sd1
  source = 51 selects qspi_sd2
  source = 52 selects qspi_sd3
  source = 53 selects qspi_sclk
  level = 0 triggers the pwrup when the source is low
```

level = 1 triggers the pwrup when the source is high

*Table 514. PWRUP2 Register*

| Bits  | Description                                                                                                                                                                                                                      | Type | Reset |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:11 | Reserved.                                                                                                                                                                                                                        | -    | -     |
| 10    | RAW_STATUS: Value of selected gpio pin (only if enable == 1)                                                                                                                                                                     | RO   | 0x0   |
| 9     | STATUS: Status of gpio wakeup. Write to 1 to clear a latched edge detect.                                                                                                                                                        | WC   | 0x0   |
| 8     | MODE: Edge or level detect. Edge will detect a 0 to 1 transition (or 1 to 0<br>transition). Level will detect a 1 or 0. Both types of event get latched into the<br>current_pwrup_req register.                                  | RW   | 0x0   |
|       | Enumerated values:                                                                                                                                                                                                               |      |       |
|       | 0x0 → LEVEL                                                                                                                                                                                                                      |      |       |
|       | 0x1 → EDGE                                                                                                                                                                                                                       |      |       |
| 7     | DIRECTION                                                                                                                                                                                                                        | RW   | 0x0   |
|       | Enumerated values:                                                                                                                                                                                                               |      |       |
|       | 0x0 → LOW_FALLING                                                                                                                                                                                                                |      |       |
|       | 0x1 → HIGH_RISING                                                                                                                                                                                                                |      |       |
| 6     | ENABLE: Set to 1 to enable the wakeup source. Set to 0 to disable the wakeup<br>source and clear a pending wakeup event.<br>If using edge detect a latched edge needs to be cleared by writing 1 to the<br>status register also. | RW   | 0x0   |
| 5:0   | SOURCE                                                                                                                                                                                                                           | RW   | 0x3f  |

## <span id="page-481-0"></span>**[POWMAN:](#page-455-0) PWRUP3 Register**

#### **Offset**: 0x98

#### **Description**

4 GPIO powerup events can be configured to wake the chip up from a low power state. The pwrups are level/edge sensitive and can be set to trigger on a high/rising or low/falling event The number of gpios available depends on the package option. An invalid selection will be ignored

source = 0 selects gpio0

1. +

2. + source = 47 selects gpio47

source = 48 selects qspi\_ss

source = 49 selects qspi\_sd0

source = 50 selects qspi\_sd1

source = 51 selects qspi\_sd2

source = 52 selects qspi\_sd3

source = 53 selects qspi\_sclk

level = 0 triggers the pwrup when the source is low

level = 1 triggers the pwrup when the source is high

*Table 515. PWRUP3 Register*

| Bits  | Description                                                               | Type | Reset |
|-------|---------------------------------------------------------------------------|------|-------|
| 31:11 | Reserved.                                                                 | -    | -     |
| 10    | RAW_STATUS: Value of selected gpio pin (only if enable == 1)              | RO   | 0x0   |
| 9     | STATUS: Status of gpio wakeup. Write to 1 to clear a latched edge detect. | WC   | 0x0   |

| Bits | Description                                                                                                                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 8    | MODE: Edge or level detect. Edge will detect a 0 to 1 transition (or 1 to 0<br>transition). Level will detect a 1 or 0. Both types of event get latched into the<br>current_pwrup_req register.                                  | RW   | 0x0   |
|      | Enumerated values:                                                                                                                                                                                                               |      |       |
|      | 0x0 → LEVEL                                                                                                                                                                                                                      |      |       |
|      | 0x1 → EDGE                                                                                                                                                                                                                       |      |       |
| 7    | DIRECTION                                                                                                                                                                                                                        | RW   | 0x0   |
|      | Enumerated values:                                                                                                                                                                                                               |      |       |
|      | 0x0 → LOW_FALLING                                                                                                                                                                                                                |      |       |
|      | 0x1 → HIGH_RISING                                                                                                                                                                                                                |      |       |
| 6    | ENABLE: Set to 1 to enable the wakeup source. Set to 0 to disable the wakeup<br>source and clear a pending wakeup event.<br>If using edge detect a latched edge needs to be cleared by writing 1 to the<br>status register also. | RW   | 0x0   |
| 5:0  | SOURCE                                                                                                                                                                                                                           | RW   | 0x3f  |

# <span id="page-482-1"></span>**[POWMAN:](#page-455-0) CURRENT\_PWRUP\_REQ Register**

**Offset**: 0x9c

*Table 516. CURRENT\_PWRUP\_RE Q Register*

| Bits | Description                                                                                                                                                                                                                                                                                                                                                           | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:7 | Reserved.                                                                                                                                                                                                                                                                                                                                                             | -    | -     |
| 6:0  | Indicates current powerup request state<br>pwrup events can be cleared by removing the enable from the pwrup register.<br>The alarm pwrup req can be cleared by clearing timer.alarm_enab<br>0 = chip reset, for the source of the last reset see POWMAN_CHIP_RESET<br>1 = pwrup0<br>2 = pwrup1<br>3 = pwrup2<br>4 = pwrup3<br>5 = coresight_pwrup<br>6 = alarm_pwrup | RO   | 0x00  |

#### <span id="page-482-0"></span>**[POWMAN:](#page-455-0) LAST\_SWCORE\_PWRUP Register**

**Offset**: 0xa0

*Table 517. LAST\_SWCORE\_PWRU P Register*

| Bits | Description                                                                                                                                                                                                                                        | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:7 | Reserved.                                                                                                                                                                                                                                          | -    | -     |
| 6:0  | Indicates which pwrup source triggered the last switched-core power up<br>0 = chip reset, for the source of the last reset see POWMAN_CHIP_RESET<br>1 = pwrup0<br>2 = pwrup1<br>3 = pwrup2<br>4 = pwrup3<br>5 = coresight_pwrup<br>6 = alarm_pwrup | RO   | 0x00  |

# <span id="page-483-1"></span>**[POWMAN:](#page-455-0) DBG\_PWRCFG Register**

**Offset**: 0xa4

*Table 518. DBG\_PWRCFG Register*

| Bits | Description                                                                                                                                                                          | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:1 | Reserved.                                                                                                                                                                            | -    | -     |
| 0    | IGNORE: Ignore pwrup req from debugger. If pwrup req is asserted then this<br>will prevent power down and set powerdown blocked. Set ignore to stop<br>paying attention to pwrup_req | RW   | 0x0   |

# <span id="page-483-0"></span>**[POWMAN:](#page-455-0) BOOTDIS Register**

**Offset**: 0xa8

#### **Description**

Tell the bootrom to ignore the BOOT0..3 registers following the next RSM reset (e.g. the next core power down/up).

If an early boot stage has soft-locked some OTP pages in order to protect their contents from later stages, there is a risk that Secure code running at a later stage can unlock the pages by powering the core up and down.

This register can be used to ensure that the bootloader runs as normal on the next power up, preventing Secure code at a later stage from accessing OTP in its unlocked state.

Should be used in conjunction with the OTP BOOTDIS register.

*Table 519. BOOTDIS Register*

| Bits | Description                                                                                                                                                                                                                                                    | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:2 | Reserved.                                                                                                                                                                                                                                                      | -    | -     |
| 1    | NEXT: This flag always ORs writes into its current contents. It can be set but<br>not cleared by software.                                                                                                                                                     | RW   | 0x0   |
|      | The BOOTDIS_NEXT bit is OR'd into the BOOTDIS_NOW bit when the core is<br>powered down. Simultaneously, the BOOTDIS_NEXT bit is cleared. Setting this<br>bit means that the BOOT03 registers will be ignored following the next reset<br>of the RSM by powman. |      |       |
|      | This flag should be set by an early boot stage that has soft-locked OTP pages,<br>to prevent later stages from unlocking it by power cycling.                                                                                                                  |      |       |

| Bits | Description                                                                                                                                                                                                                                                                                         | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 0    | NOW: When powman resets the RSM, the current value of BOOTDIS_NEXT is<br>OR'd into BOOTDIS_NOW, and BOOTDIS_NEXT is cleared.                                                                                                                                                                        | WC   | 0x0   |
|      | The bootrom checks this flag before reading the BOOT03 registers. If it is set,<br>the bootrom clears it, and ignores the BOOT registers. This prevents Secure<br>software from diverting the boot path before a bootloader has had the chance<br>to soft lock OTP pages containing sensitive data. |      |       |

# <span id="page-484-1"></span>**[POWMAN:](#page-455-0) DBGCONFIG Register**

**Offset**: 0xac

*Table 520. DBGCONFIG Register*

| Bits | Description                                                                                                                                                        | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                                                                                                                          | -    | -     |
| 3:0  | DP_INSTID: Configure DP instance ID for SWD multidrop selection.<br>Recommend that this is NOT changed until you require debug access in multi<br>chip environment | RW   | 0x0   |

# <span id="page-484-2"></span>**[POWMAN:](#page-455-0) SCRATCH0, SCRATCH1, …, SCRATCH6, SCRATCH7 Registers**

**Offsets**: 0xb0, 0xb4, …, 0xc8, 0xcc

*Table 521. SCRATCH0, SCRATCH1, …, SCRATCH6, SCRATCH7 Registers*

| Bits | Description                                              | Type | Reset      |
|------|----------------------------------------------------------|------|------------|
| 31:0 | Scratch register. Information persists in low power mode | RW   | 0x00000000 |

# <span id="page-484-0"></span>**[POWMAN:](#page-455-0) BOOT0, BOOT1, BOOT2, BOOT3 Registers**

**Offsets**: 0xd0, 0xd4, 0xd8, 0xdc

*Table 522. BOOT0, BOOT1, BOOT2, BOOT3 Registers*

| Bits | Description                                              | Type | Reset      |
|------|----------------------------------------------------------|------|------------|
| 31:0 | Scratch register. Information persists in low power mode | RW   | 0x00000000 |

#### <span id="page-484-3"></span>**[POWMAN:](#page-455-0) INTR Register**

**Offset**: 0xe0

**Description**

Raw Interrupts

*Table 523. INTR Register*

| Bits | Description                                              | Type | Reset |
|------|----------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                | -    | -     |
| 3    | PWRUP_WHILE_WAITING: Source is state.pwrup_while_waiting | RO   | 0x0   |
| 2    | STATE_REQ_IGNORED: Source is state.req_ignored           | RO   | 0x0   |
| 1    | TIMER                                                    | RO   | 0x0   |
| 0    | VREG_OUTPUT_LOW                                          | WC   | 0x0   |

## <span id="page-484-4"></span>**[POWMAN:](#page-455-0) INTE Register**

**Offset**: 0xe4

Interrupt Enable

*Table 524. INTE Register*

| Bits | Description                                              | Type | Reset |
|------|----------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                | -    | -     |
| 3    | PWRUP_WHILE_WAITING: Source is state.pwrup_while_waiting | RW   | 0x0   |
| 2    | STATE_REQ_IGNORED: Source is state.req_ignored           | RW   | 0x0   |
| 1    | TIMER                                                    | RW   | 0x0   |
| 0    | VREG_OUTPUT_LOW                                          | RW   | 0x0   |

# <span id="page-485-1"></span>**[POWMAN:](#page-455-0) INTF Register**

**Offset**: 0xe8 **Description**

Interrupt Force

*Table 525. INTF Register*

| Bits | Description                                              | Type | Reset |
|------|----------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                | -    | -     |
| 3    | PWRUP_WHILE_WAITING: Source is state.pwrup_while_waiting | RW   | 0x0   |
| 2    | STATE_REQ_IGNORED: Source is state.req_ignored           | RW   | 0x0   |
| 1    | TIMER                                                    | RW   | 0x0   |
| 0    | VREG_OUTPUT_LOW                                          | RW   | 0x0   |

#### <span id="page-485-2"></span>**[POWMAN:](#page-455-0) INTS Register**

**Offset**: 0xec

#### **Description**

Interrupt status after masking & forcing

*Table 526. INTS Register*

| Bits | Description                                              | Type | Reset |
|------|----------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                | -    | -     |
| 3    | PWRUP_WHILE_WAITING: Source is state.pwrup_while_waiting | RO   | 0x0   |
| 2    | STATE_REQ_IGNORED: Source is state.req_ignored           | RO   | 0x0   |
| 1    | TIMER                                                    | RO   | 0x0   |
| 0    | VREG_OUTPUT_LOW                                          | RO   | 0x0   |

