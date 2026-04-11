# 3.5.9 Security

By default, the SWD debug access port allows an external debugger to access all system memory and peripherals, and to observe and change the execution of software running on the processors. If boot signature enforcement is enabled ([Section 10.1.1\)](#page-813-2), debug access becomes a security concern, as it is able to sidestep this protection. To account for this, RP2350 supports progressively locking down the debug port using configuration in on-chip OTP storage.

Conceptually there are two control bits: debug disable, and secure debug disable. Debug disable is intended to completely cut off debug access to the processors and the system bus, whilst the secure debug disable forbids Secure bus accesses, and halting of processors in the Secure state, but still allows Non-secure software to be debugged as normal. There are two ways to set these control bits:

- Setting the relevant OTP critical flag: [CRIT1.](#page-1304-0)DEBUG\_DISABLE or [CRIT1.](#page-1304-0)SECURE\_DEBUG\_DISABLE to set the debug disable or secure debug disable, respectively
- Installing a 128-bit fixed debug key as OTP key 5 or 6 [\(Section 3.5.9.2](#page-92-1))

OTP configuration changes take effect at the next reset of the OTP block.

Once debug has been disabled, software can re-enable debug using the OTP [DEBUGEN](#page-1284-0) register, which allows the secure and overall debug enable to be cleared individually for each processor. For example, Secure software may implement a shell where users can authenticate using a cryptographic challenge to enable debug on systems where it is disabled by default. The DEBUGEN register belongs to the processor cold reset domain, so it is preserved over a PSM reset starting from as early as OTP (the second PSM stage). This allows almost a full system reset without losing debug access.

To avoid accidental writes of the [DEBUGEN](#page-1284-0) register, its bits can be individually locked using the matching bits in [DEBUGEN\\_LOCK](#page-1285-0).

This offers increasing levels of debug protection:

- 1. Fully open: no keys installed and no OTP debug disable flags are set. This is the most convenient configuration for product development.
- 2. Access with key only: at least one key is installed, but no OTP debug disable flags are set.
- 3. No access even with key (an OTP debug disable flag is set), but Secure code can enable debug access by writing to [DEBUGEN.](#page-1284-0)
- 4. No access even with key (an OTP debug disable flag is set), and [DEBUGEN](#page-1284-0) is locked by [DEBUGEN\\_LOCK](#page-1285-0).

#### <span id="page-91-1"></span>**3.5.9.1. Effects of Debug Disables**

The secure debug disable flag [\(CRIT1.](#page-1304-0)SECURE\_DEBUG\_DISABLE) has the following effects:

- Set Secure AP enable signals for Arm core 0 and core 1 AHB-APs to 0.
  - This prevents the APs from performing Secure bus accesses (including to the PPB).
  - Status is reported in the SDeviceEn flag of the AHB-AP CSW register.
- Set the Cortex-M33 SPIDEN and SPNIDEN signals for both cores to 0.
  - This prevents the cores from being halted or traced whilst in the Secure state.
- Disable the factory test JTAG interface ([Section 10.10\)](#page-871-0).

The debug disable flag [\(CRIT1.](#page-1304-0)DEBUG\_DISABLE) has all of the effects of the secure debug disable flag. It also has the following additional effects:

- Set AP enable signals for Arm core 0 and core 1 AHB-APs to 0.
  - This prevents the APs from performing any bus accesses at all (including to the PPB).
  - Status is reported in the DeviceEn flag of the AHB-AP CSW register.
- Set AP enable signal for RISC-V DM APB-AP to 0.

- This prevents the AP from accessing the RISC-V Debug Module.
- Status is reported in the DeviceEn flag of the APB-AP CSW register.
- Set DBGEN and NIDEN signals for the CTI to 0.

On RISC-V [CRIT1.](#page-1304-0)SECURE\_DEBUG\_DISABLE has no useful effect. Debug-mode accesses from the cores always have Secure and Privileged bus attributes, except when reduced by [FORCE\\_CORE\\_NS.](#page-827-0) Likewise, System Bus Access via the Debug Module is always Secure and Privileged, unless [FORCE\\_CORE\\_NS.](#page-827-0)CORE1 is set, in which case it is Non-secure and Privileged. Use the [CRIT1](#page-1304-0).DEBUG\_DISABLE flag on RISC-V.

#### <span id="page-92-1"></span>**3.5.9.2. Debug Keys**

[Section 13.5.2](#page-1272-0) describes the OTP hardware access keys. Hardware reads OTP access keys into hidden registers as part of the OTP power-up sequence which takes place after an OTP reset, and the corresponding OTP locations then become inaccessible. OTP keys 5 and 6 are special in that they control access to the SWD debug hardware in addition to functioning as normal OTP page keys.

A debug key is a 128-bit fixed challenge. Installing a debug key in OTP locks down debug access, and it remains locked until the debug host writes a matching key value through the RP-AP [DBGKEY](#page-95-0) register. This is a write-only interface.

To install a debug key, first program the OTP locations starting from [KEY5\\_0](#page-1315-0) or [KEY6\\_0](#page-1315-0). These locations are ECCprotected. Once you have programmed the 128-bit key value and read it back to confirm the correct value is programmed, write the raw bit pattern 0x010101 to [KEY5\\_VALID](#page-1318-0) or [KEY6\\_VALID](#page-1318-1) to mark the key as valid. The validity takes effect at the next reset of the OTP block.

Once a key is valid, the OTP storage locations for that key become inaccessible for both reads and writes. Only the OTP power-up state machine [\(Section 13.3.4\)](#page-1269-1) can read the key.

The effect of installing debug keys depends on which of key 5 and 6 are installed:

- If key 5 or key 6 is valid, and no matching key (either) has been entered through the RP-AP, all debug is disabled. This has the same effect as setting [CRIT1](#page-1304-0).DEBUG\_DISABLE.
- If key 5 is valid, and no matching key (key 5 specifically) has been entered through the RP-AP, Secure debug is disabled. This has the same effect as writing [CRIT1.](#page-1304-0)SECURE\_DEBUG\_DISABLE.

When both keys are installed, key 5 provides both Secure and Non-secure debug access, and key 6 provides Non-secure debug access only. When only a single key is installed, that key provides both Secure and Non-secure debug access.

To enter a key over SWD, first write a 1 to [DBGKEY.](#page-95-0)RESET. Then sequentially write 128 bits to [DBGKEY.](#page-95-0)DATA, each accompanied by a 1 written to [DBGKEY.](#page-95-0)PUSH. Write the data LSB-first, starting with the lowest-numbered OTP row.

Assuming you wrote a value that matched one of the installed debug keys, debug unlocks after the 128th push. The SDeviceEn and DeviceEn flags in the Mem-AP CSW registers indicate success or failure.

Failure to supply a matching key through the RP-AP disables debug if it would otherwise be enabled. However, supplying a key does not enable if it is already disabled for other reasons. For example, if [CRIT1.](#page-1304-0)DEBUG\_DISABLE is set, and [DEBUGEN](#page-1284-0) is clear, debug is be disabled no matter the state of the debug keys and the RP-AP.

#### <span id="page-92-0"></span>**3.5.10. RP-AP**

The RP-AP is a small register block which is always accessible over SWD. RP-AP access does not require the switched core domain to be powered up, or any internal system clock generators to be running.

#### **3.5.10.1. List of Registers**

<span id="page-92-2"></span>The RP-AP registers start at offset 0x80000 in the debug address space, which is accessed via address 0x80000 in the SW-DP's SELECT register. Unlike the other APs, it can not be accessed directly from the system bus.

*Table 98. List of RP\_AP registers*

| Offset | Name                 | Info                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
|--------|----------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x000  | CTRL                 | This register is primarily used for DFT but can also be used to<br>overcome some power up problems. However, it should not be<br>used to force power up of domains. Use DBG_POW_OVRD for<br>that.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| 0x004  | DBGKEY               | Serial key load interface (write-only)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| 0x008  | DBG_POW_STATE_SWCORE | This register indicates the state of the power sequencer for the<br>switched-core domain.<br>The sequencer timing is managed by the POWMAN_SEQ_*<br>registers. See the header file for those registers for more<br>information on the timing.<br>Power up of the domain commences by clearing bit 0 (IS_PD)<br>then bits 1-8 are set in sequence. Bit 8 (IS_PU) indicates the<br>sequence is complete.<br>Power down of the domain commences by clearing bit 8 (IS_PU)<br>then bits 7-1 are cleared in sequence. Bit 0 (IS_PU) is then set to<br>indicate the sequence is complete.<br>Bits 9-11 describe the states of the power manager clocks which<br>change as clock generators in the switched-core become<br>available following switched-core power up.<br>This bus can be sent to GPIO for debug. See<br>DBG_POW_OUTPUT_TO_GPIO in the DBG_POW_OVRD register. |
| 0x00c  | DBG_POW_STATE_XIP    | This register indicates the state of the power sequencer for the<br>XIP domain.<br>The sequencer timing is managed by the POWMAN_SEQ_*<br>registers. See the header file for those registers for more<br>information on the timing.<br>Power up of the domain commences by clearing bit 0 (IS_PD)<br>then bits 1-8 are set in sequence. Bit 8 (IS_PU) indicates the<br>sequence is complete.<br>Power down of the domain commences by clearing bit 8 (IS_PU)<br>then bits 7-1 are cleared in sequence. Bit 0 (IS_PU) is then set to<br>indicate the sequence is complete.                                                                                                                                                                                                                                                                                              |
| 0x010  | DBG_POW_STATE_SRAM0  | This register indicates the state of the power sequencer for the<br>SRAM0 domain.<br>The sequencer timing is managed by the POWMAN_SEQ_*<br>registers. See the header file for those registers for more<br>information on the timing.<br>Power up of the domain commences by clearing bit 0 (IS_PD)<br>then bits 1-8 are set in sequence. Bit 8 (IS_PU) indicates the<br>sequence is complete.<br>Power down of the domain commences by clearing bit 8 (IS_PU)<br>then bits 7-1 are cleared in sequence. Bit 0 (IS_PU) is then set to<br>indicate the sequence is complete.                                                                                                                                                                                                                                                                                            |

| Offset | Name                   | Info                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
|--------|------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x014  | DBG_POW_STATE_SRAM1    | This register indicates the state of the power sequencer for the<br>SRAM1 domain.<br>The sequencer timing is managed by the POWMAN_SEQ_*<br>registers. See the header file for those registers for more<br>information on the timing.<br>Power up of the domain commences by clearing bit 0 (IS_PD)<br>then bits 1-8 are set in sequence. Bit 8 (IS_PU) indicates the<br>sequence is complete.<br>Power down of the domain commences by clearing bit 8 (IS_PU)<br>then bits 7-1 are cleared in sequence. Bit 0 (IS_PU) is then set to<br>indicate the sequence is complete.                     |
| 0x018  | DBG_POW_OVRD           | This register allows external control of the power sequencer<br>outputs for all the switched power domains. If any of the power<br>sequencers stall at any stage then force power up operation of<br>all domains by running this sequence:<br>- set DBG_POW_OVRD = 0x3b to force small power switches on,<br>large power switches off, resets on and isolation on<br>- allow time for the domain power supplies to reach full rail<br>- set DBG_POW_OVRD = 0x3b to force large power switches on<br>- set DBG_POW_OVRD = 0x37 to remove isolation<br>- set DBG_POW_OVRD = 0x17 to remove resets |
| 0x01c  | DBG_POW_OUTPUT_TO_GPIO | Send some, or all, bits of DBG_POW_STATE_SWCORE to gpios.<br>Bit 0 sends bit 0 of DBG_POW_STATE_SWCORE to GPIO 34<br>Bit 1 sends bit 1 of DBG_POW_STATE_SWCORE to GPIO 35<br>Bit 2 sends bit 2 of DBG_POW_STATE_SWCORE to GPIO 36<br>Bit 11 sends bit 11 of DBG_POW_STATE_SWCORE to GPIO 45                                                                                                                                                                                                                                                                                                     |
| 0xdfc  | IDR                    | Standard Coresight ID Register                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |

#### <span id="page-94-0"></span>**[RP\\_AP:](#page-92-2) CTRL Register**

## **Offset**: 0x000

#### **Description**

This register is primarily used for DFT but can also be used to overcome some power up problems. However, it should not be used to force power up of domains. Use DBG\_POW\_OVRD for that.

*Table 99. CTRL Register*

| Bits | Description                                                                                                                                                                                                                                                                                                                                                                                                                                      | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31   | RESCUE_RESTART: Allows debug of boot problems by restarting the chip with<br>minimal boot code execution. Write to 1 to put the chip in reset then write to 0<br>to restart the chip with the rescue flag set. The rescue flag is in the<br>POWMAN_CHIP_RESET register and is read by boot code. The rescue flag is<br>cleared by writing 0 to POWMAN_CHIP_RESET_RESCUE_FLAG or by resetting<br>the chip by any means other than RESCUE_RESTART. | RW   | 0x0   |
| 30   | SPARE: Unused                                                                                                                                                                                                                                                                                                                                                                                                                                    | RW   | 0x0   |
| 29:7 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                        | -    | -     |

| Bits | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 6    | DBG_FRCE_GPIO_LPCK: Allows chip start-up when the Low Power Oscillator<br>(LPOSC) is inoperative or malfunctioning and also allows the initial power<br>sequencing rate to be adjusted. Write to 1 to force the LPOSC output to be<br>driven from a GPIO (gpio20 on 80-pin package, gpio34 on the 60-pin package).<br>If the LPOSC is inoperative or malfunctioning it may also be necessary to set<br>the LPOSC_STABLE_FRCE bit in this register. The user must provide a clock on<br>the GPIO. For normal operation use a clock running at around 32kHz.<br>Adjusting the frequency will speed up or slow down the initial power-up<br>sequence. | RW   | 0x0   |
| 5    | LPOSC_STABLE_FRCE: Allows the chip to start-up even though the Low Power<br>Oscillator (LPOSC) is failing to set its stable flag. Initial power sequencing is<br>clocked by LPOSC at around 32kHz but does not start until the LPOSC<br>declares itself to be stable. If the LPOSC is otherwise working correctly the<br>chip will boot when this bit is set. If the LPOSC is not working then<br>DBG_FRCE_GPIO_LPCK must be set and an external clock provided.                                                                                                                                                                                   | RW   | 0x0   |
| 4    | POWMAN_DFT_ISO_OFF: Holds the isolation gates between power domains in<br>the open state. This is intended to hold the gates open for DFT and power<br>manager debug. It is not intended to force the isolation gates open. Use the<br>overrides in DBG_POW_OVRD to force the isolation gates open or closed.                                                                                                                                                                                                                                                                                                                                      | RW   | 0x0   |
| 3    | POWMAN_DFT_PWRON: Holds the power switches on for all domains. This is<br>intended to keep the power on for DFT and debug, rather than for switching<br>the power on. The power switches are not sequenced and the sudden demand<br>for current could cause the always-on power domain to brown out. This<br>register is in the always-on domain therefore chaos could ensue. It is<br>recommended to use the DBG_POW_OVRD controls instead.                                                                                                                                                                                                       | RW   | 0x0   |
| 2    | POWMAN_DBGMODE: This prevents the power manager from powering down<br>and resetting the switched-core power domain. It is intended for DFT and for<br>debugging the power manager after the chip has booted. It cannot be used to<br>force initial power on because it simultaneously deasserts the reset.                                                                                                                                                                                                                                                                                                                                         | RW   | 0x0   |
| 1    | JTAG_FUNCSEL: Multiplexes the JTAG ports onto GPIO0-3                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | RW   | 0x0   |
| 0    | JTAG_TRSTN: Resets the JTAG module. Active low.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | RW   | 0x0   |

# <span id="page-95-0"></span>**[RP\\_AP:](#page-92-2) DBGKEY Register**

**Offset**: 0x004

#### **Description**

Serial key load interface (write-only)

*Table 100. DBGKEY Register*

| Bits | Description                             | Type | Reset |
|------|-----------------------------------------|------|-------|
| 31:3 | Reserved.                               | -    | -     |
| 2    | RESET: Reset (before sending a new key) | RW   | 0x0   |
| 1    | PUSH                                    | RW   | 0x0   |
| 0    | DATA                                    | RW   | 0x0   |

#### <span id="page-95-1"></span>**[RP\\_AP:](#page-92-2) DBG\_POW\_STATE\_SWCORE Register**

**Offset**: 0x008

This register indicates the state of the power sequencer for the switched-core domain.

The sequencer timing is managed by the POWMAN\_SEQ\_\* registers. See the header file for those registers for more information on the timing.

Power up of the domain commences by clearing bit 0 (IS\_PD) then bits 1-8 are set in sequence. Bit 8 (IS\_PU) indicates the sequence is complete.

Power down of the domain commences by clearing bit 8 (IS\_PU) then bits 7-1 are cleared in sequence. Bit 0 (IS\_PU) is then set to indicate the sequence is complete.

Bits 9-11 describe the states of the power manager clocks which change as clock generators in the switched-core become available following switched-core power up.

This bus can be sent to GPIO for debug. See DBG\_POW\_OUTPUT\_TO\_GPIO in the DBG\_POW\_OVRD register.

*Table 101. DBG\_POW\_STATE\_SW CORE Register*

| Bits  | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Type | Reset |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:12 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | -    | -     |
| 11    | USING_FAST_POWCK: Indicates the source of the power manager clock. On<br>switched-core power up the clock switches from the LPOSC to clk_ref and this<br>flag will be set. clk_ref will be running from the ROSC initially but will switch to<br>XOSC when it comes available. On switched-core power down the clock<br>switches to LPOSC and this flag will be cleared.                                                                                                                                                                                                                                                                                                                                                                                                                                                               | RO   | 0x0   |
| 10    | WAITING_POWCK: Indicates the switched-core power sequencer is waiting for<br>the power manager clock to update. On switched-core power up the clock<br>switches from the LPOSC to clk_ref. clk_ref will be running from the ROSC<br>initially but will switch to XOSC when it comes available. On switched-core<br>power down the clock switches to LPOSC.<br>If the switched-core power up sequence stalls with this flag active then it<br>means clk_ref is not running which indicates a problem with the ROSC. If that<br>happens then set DBG_POW_RESTART_FROM_XOSC in the DBG_POW_OVRD<br>register to avoid using the ROSC.<br>If the switched-core power down sequence stalls with this flag active then it<br>means LPOSC is not running. The solution is to not stop LPOSC when the<br>switched-core power domain is powered. | RO   | 0x0   |
| 9     | WAITING_TIMCK: Indicates that the switched-core power sequencer is waiting<br>for the AON-Timer to update. On switched-core power-up there is nothing to<br>be done. The AON-Timer continues to run from the LPOSC so this flag will not<br>be set. Software decides whether to switch the AON-Timer clock to XOSC (via<br>clk_ref). On switched-core power-down the sequencer will switch the AON<br>Timer back to LPOSC if software switched it to XOSC. During the switchover<br>the WAITING_TIMCK flag will be set. If the switched-core power down<br>sequence stalls with this flag active then the only recourse is to reset the chip<br>and change software to not select XOSC as the AON-Timer source.                                                                                                                        | RO   | 0x0   |
| 8     | IS_PU: Indicates the power somain is fully powered up.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | RO   | 0x0   |
| 7     | RESET_FROM_SEQ: Indicates the state of the reset to the power domain.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | RO   | 0x0   |
| 6     | ENAB_ACK: Indicates the state of the enable to the power domain.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | RO   | 0x0   |
| 5     | ISOLATE_FROM_SEQ: Indicates the state of the isolation control to the power<br>domain.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | RO   | 0x0   |
| 4     | LARGE_ACK: Indicates the state of the large power switches for the power<br>domain.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | RO   | 0x0   |

| Bits | Description                                                                                                                                                                                                                                                                                                                                     | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 3    | SMALL_ACK2: The small switches are split into 3 chains. In the power up<br>sequence they are switched on separately to allow management of the VDD<br>rise time. In the power down sequence they switch off simultaneously with the<br>large power switches.<br>This bit indicates the state of the last element in small power switch chain 2. | RO   | 0x0   |
| 2    | SMALL_ACK1: This bit indicates the state of the last element in small power<br>switch chain 1.                                                                                                                                                                                                                                                  | RO   | 0x0   |
| 1    | SMALL_ACK0: This bit indicates the state of the last element in small power<br>switch chain 0.                                                                                                                                                                                                                                                  | RO   | 0x0   |
| 0    | IS_PD: Indicates the power somain is fully powered down.                                                                                                                                                                                                                                                                                        | RO   | 0x0   |

# <span id="page-97-0"></span>**[RP\\_AP:](#page-92-2) DBG\_POW\_STATE\_XIP Register**

# **Offset**: 0x00c

#### **Description**

This register indicates the state of the power sequencer for the XIP domain.

The sequencer timing is managed by the POWMAN\_SEQ\_\* registers. See the header file for those registers for more information on the timing.

Power up of the domain commences by clearing bit 0 (IS\_PD) then bits 1-8 are set in sequence. Bit 8 (IS\_PU) indicates the sequence is complete.

Power down of the domain commences by clearing bit 8 (IS\_PU) then bits 7-1 are cleared in sequence. Bit 0 (IS\_PU) is then set to indicate the sequence is complete.

*Table 102. DBG\_POW\_STATE\_XIP Register*

| Bits | Description                                                                                                                                                                                                                                                                                                                                     | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:9 | Reserved.                                                                                                                                                                                                                                                                                                                                       | -    | -     |
| 8    | IS_PU: Indicates the power somain is fully powered up.                                                                                                                                                                                                                                                                                          | RO   | 0x0   |
| 7    | RESET_FROM_SEQ: Indicates the state of the reset to the power domain.                                                                                                                                                                                                                                                                           | RO   | 0x0   |
| 6    | ENAB_ACK: Indicates the state of the enable to the power domain.                                                                                                                                                                                                                                                                                | RO   | 0x0   |
| 5    | ISOLATE_FROM_SEQ: Indicates the state of the isolation control to the power<br>domain.                                                                                                                                                                                                                                                          | RO   | 0x0   |
| 4    | LARGE_ACK: Indicates the state of the large power switches for the power<br>domain.                                                                                                                                                                                                                                                             | RO   | 0x0   |
| 3    | SMALL_ACK2: The small switches are split into 3 chains. In the power up<br>sequence they are switched on separately to allow management of the VDD<br>rise time. In the power down sequence they switch off simultaneously with the<br>large power switches.<br>This bit indicates the state of the last element in small power switch chain 2. | RO   | 0x0   |
| 2    | SMALL_ACK1: This bit indicates the state of the last element in small power<br>switch chain 1.                                                                                                                                                                                                                                                  | RO   | 0x0   |
| 1    | SMALL_ACK0: This bit indicates the state of the last element in small power<br>switch chain 0.                                                                                                                                                                                                                                                  | RO   | 0x0   |
| 0    | IS_PD: Indicates the power somain is fully powered down.                                                                                                                                                                                                                                                                                        | RO   | 0x0   |

## <span id="page-97-1"></span>**[RP\\_AP:](#page-92-2) DBG\_POW\_STATE\_SRAM0 Register**

**Offset**: 0x010

This register indicates the state of the power sequencer for the SRAM0 domain.

The sequencer timing is managed by the POWMAN\_SEQ\_\* registers. See the header file for those registers for more information on the timing.

Power up of the domain commences by clearing bit 0 (IS\_PD) then bits 1-8 are set in sequence. Bit 8 (IS\_PU) indicates the sequence is complete.

Power down of the domain commences by clearing bit 8 (IS\_PU) then bits 7-1 are cleared in sequence. Bit 0 (IS\_PU) is then set to indicate the sequence is complete.

*Table 103. DBG\_POW\_STATE\_SR AM0 Register*

| Bits | Description                                                                                                                                                                                                                                                                                                                                     | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:9 | Reserved.                                                                                                                                                                                                                                                                                                                                       | -    | -     |
| 8    | IS_PU: Indicates the power somain is fully powered up.                                                                                                                                                                                                                                                                                          | RO   | 0x0   |
| 7    | RESET_FROM_SEQ: Indicates the state of the reset to the power domain.                                                                                                                                                                                                                                                                           | RO   | 0x0   |
| 6    | ENAB_ACK: Indicates the state of the enable to the power domain.                                                                                                                                                                                                                                                                                | RO   | 0x0   |
| 5    | ISOLATE_FROM_SEQ: Indicates the state of the isolation control to the power<br>domain.                                                                                                                                                                                                                                                          | RO   | 0x0   |
| 4    | LARGE_ACK: Indicates the state of the large power switches for the power<br>domain.                                                                                                                                                                                                                                                             | RO   | 0x0   |
| 3    | SMALL_ACK2: The small switches are split into 3 chains. In the power up<br>sequence they are switched on separately to allow management of the VDD<br>rise time. In the power down sequence they switch off simultaneously with the<br>large power switches.<br>This bit indicates the state of the last element in small power switch chain 2. | RO   | 0x0   |
| 2    | SMALL_ACK1: This bit indicates the state of the last element in small power<br>switch chain 1.                                                                                                                                                                                                                                                  | RO   | 0x0   |
| 1    | SMALL_ACK0: This bit indicates the state of the last element in small power<br>switch chain 0.                                                                                                                                                                                                                                                  | RO   | 0x0   |
| 0    | IS_PD: Indicates the power somain is fully powered down.                                                                                                                                                                                                                                                                                        | RO   | 0x0   |

#### <span id="page-98-0"></span>**[RP\\_AP:](#page-92-2) DBG\_POW\_STATE\_SRAM1 Register**

#### **Offset**: 0x014

#### **Description**

This register indicates the state of the power sequencer for the SRAM1 domain.

The sequencer timing is managed by the POWMAN\_SEQ\_\* registers. See the header file for those registers for more information on the timing.

Power up of the domain commences by clearing bit 0 (IS\_PD) then bits 1-8 are set in sequence. Bit 8 (IS\_PU) indicates the sequence is complete.

Power down of the domain commences by clearing bit 8 (IS\_PU) then bits 7-1 are cleared in sequence. Bit 0 (IS\_PU) is then set to indicate the sequence is complete.

*Table 104. DBG\_POW\_STATE\_SR AM1 Register*

| Bits | Description                                                                            | Type | Reset |
|------|----------------------------------------------------------------------------------------|------|-------|
| 31:9 | Reserved.                                                                              | -    | -     |
| 8    | IS_PU: Indicates the power somain is fully powered up.                                 | RO   | 0x0   |
| 7    | RESET_FROM_SEQ: Indicates the state of the reset to the power domain.                  | RO   | 0x0   |
| 6    | ENAB_ACK: Indicates the state of the enable to the power domain.                       | RO   | 0x0   |
| 5    | ISOLATE_FROM_SEQ: Indicates the state of the isolation control to the power<br>domain. | RO   | 0x0   |

| Bits | Description                                                                                                                                                                                                                                                                                                                                     | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 4    | LARGE_ACK: Indicates the state of the large power switches for the power<br>domain.                                                                                                                                                                                                                                                             | RO   | 0x0   |
| 3    | SMALL_ACK2: The small switches are split into 3 chains. In the power up<br>sequence they are switched on separately to allow management of the VDD<br>rise time. In the power down sequence they switch off simultaneously with the<br>large power switches.<br>This bit indicates the state of the last element in small power switch chain 2. | RO   | 0x0   |
| 2    | SMALL_ACK1: This bit indicates the state of the last element in small power<br>switch chain 1.                                                                                                                                                                                                                                                  | RO   | 0x0   |
| 1    | SMALL_ACK0: This bit indicates the state of the last element in small power<br>switch chain 0.                                                                                                                                                                                                                                                  | RO   | 0x0   |
| 0    | IS_PD: Indicates the power somain is fully powered down.                                                                                                                                                                                                                                                                                        | RO   | 0x0   |

# <span id="page-99-0"></span>**[RP\\_AP:](#page-92-2) DBG\_POW\_OVRD Register**

#### **Offset**: 0x018

#### **Description**

This register allows external control of the power sequencer outputs for all the switched power domains. If any of the power sequencers stall at any stage then force power up operation of all domains by running this sequence:

- set DBG\_POW\_OVRD = 0x3b to force small power switches on, large power switches off, resets on and isolation on
- allow time for the domain power supplies to reach full rail
- set DBG\_POW\_OVRD = 0x3b to force large power switches on
- set DBG\_POW\_OVRD = 0x37 to remove isolation
- set DBG\_POW\_OVRD = 0x17 to remove resets

*Table 105. DBG\_POW\_OVRD Register*

| Bits | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:7 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | -    | -     |
| 6    | DBG_POW_RESTART_FROM_XOSC: By default the system begins boot as<br>soon as a clock is available from the ROSC, then it switches to the XOSC when<br>it is available. This is done because the XOSC takes several ms to start up. If<br>there is a problem with the ROSC then the default behaviour can be changed<br>to not use the ROSC and wait for XOSC. However, this requires a mask change<br>to modify the reset value of the Power Manager START_FROM_XOSC register.<br>To allow experimentation the default can be temporarily changed by setting<br>this register bit to 1. After setting this bit the core must be reset by a Coresight<br>dprst or a rescue reset (see RESCUE_RESTART in the RP_AP_CTRL register<br>above). A power-on reset, brown-out reset or RUN pin reset will reset this<br>control and revert to the default behaviour. | RW   | 0x0   |
| 5    | DBG_POW_RESET: When DBG_POW_OVRD_RESET=1 this register bit controls<br>the resets for all domains. 1 = reset. 0 = not reset.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | RW   | 0x0   |
| 4    | DBG_POW_OVRD_RESET: Enables DBG_POW_RESET to control the resets for<br>the power manager and the switched-core. Essentially that is everythjing<br>except the Coresight 2-wire interface and the RP_AP registers.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | RW   | 0x0   |
| 3    | DBG_POW_ISO: When DBG_POW_OVRD_ISO=1 this register bit controls the<br>isolation gates for all domains. 1 = isolated. 0 = not isolated.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | RW   | 0x0   |

| Bits | Description                                                                                                                                                                                                                                                                                                                 | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 2    | DBG_POW_OVRD_ISO: Enables DBG_POW_ISO to control the isolation gates<br>between domains.                                                                                                                                                                                                                                    | RW   | 0x0   |
| 1    | DBG_POW_OVRD_LARGE_REQ: Turn on the large power switches for all<br>domains. This should not be done until sufficient time has been allowed for<br>the small switches to bring the supplies up. Switching the large switches on<br>too soon risks browning out the always-on domain and corrupting these very<br>registers. | RW   | 0x0   |
| 0    | DBG_POW_OVRD_SMALL_REQ: Turn on the small power switches for all<br>domains. This switches on chain 0 for each domain and switches off chains 2<br>& 3 and the large power switch chain. This will bring the power up for all<br>domains without browning out the always-on power domain.                                   | RW   | 0x0   |

# <span id="page-100-1"></span>**[RP\\_AP:](#page-92-2) DBG\_POW\_OUTPUT\_TO\_GPIO Register**

**Offset**: 0x01c

#### **Description**

Send some, or all, bits of DBG\_POW\_STATE\_SWCORE to gpios. Bit 0 sends bit 0 of DBG\_POW\_STATE\_SWCORE to GPIO 34 Bit 1 sends bit 1 of DBG\_POW\_STATE\_SWCORE to GPIO 35 Bit 2 sends bit 2 of DBG\_POW\_STATE\_SWCORE to GPIO 36

1. +

2. + Bit 11 sends bit 11 of DBG\_POW\_STATE\_SWCORE to GPIO 45

*Table 106. DBG\_POW\_OUTPUT\_T O\_GPIO Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:12 | Reserved.   | -    | -     |
| 11:0  | ENABLE      | RW   | 0x000 |

#### <span id="page-100-2"></span>**[RP\\_AP:](#page-92-2) IDR Register**

**Offset**: 0xdfc

*Table 107. IDR Register*

| Bits | Description                    | Type | Reset |
|------|--------------------------------|------|-------|
| 31:0 | Standard Coresight ID Register | RO   | -     |

