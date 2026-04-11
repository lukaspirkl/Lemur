# 12.10.5 Selecting the AON Timer Tick Source

The AON Timer indicates the current configuration with read-only flags. [Table 1251](#page-1196-1) provides a list of sources supported by the 1kHz AON Timer tick.

*Table 1251. AON Timer tick generators*

<span id="page-1196-1"></span>

| Tick source          | Read-only flag        |
|----------------------|-----------------------|
| LPOSC clock division | TIMER.USING_LPOSC     |
| XOSC clock division  | TIMER.USING_XOSC      |
| external 1kHz tick   | TIMER.USING_GPIO_1KHZ |

## **NOTE**

The LPOSC clock can be substituted by an external 32kHz clock.

#### <span id="page-1196-2"></span>**12.10.5.1. Using LPOSC as the AON Timer Tick Source**

LPOSC is the default source and can be used in all power modes. It nominally runs at 32.768kHz and can only be tuned to 1% accuracy. The AON Timer derives the 1ms tick from the LPOSC using a 6.16 bit fractional divider whose divisor is initialised to 32.768. The divisor can be modified to achieve greater accuracy. Because the LPOSC frequency varies with supply voltage and temperature, accuracy is limited unless supply voltage and temperature are stable. To modify the divisor, write to the following registers:

- [LPOSC\\_FREQ\\_KHZ\\_INT](#page-475-0) (default value: 32)
- [LPOSC\\_FREQ\\_KHZ\\_FRAC](#page-475-1) (default value: 0.768)

These registers should only be written when [TIMER](#page-478-1).RUN = 0 or [TIMER](#page-478-1).USING\_LPOSC = 0.

If the tick source is not LPOSC, you can switch it back to LPOSC by writing a 1 to [TIMER](#page-478-1).USE\_LPOSC. It is not necessary to stop the AON Timer to do this. The newly selected tick will be synchronised to the current tick, so the operation may take up to 1 tick cycle (1ms in normal operation). When the operation is complete, [TIMER](#page-478-1).USE\_LPOSC will self-clear and [TIMER](#page-478-1).USING\_LPOSC will be set. Due to sampling, a small error of up to 2 periods of the newly selected clock will be subtracted from the time. When switching to LPOSC at 32kHz, an error of up to 62μs will be subtracted.

#### <span id="page-1197-0"></span>**12.10.5.2. Using an External Clock in Place of LPOSC**

If LPOSC is not sufficiently accurate, an external 32.768kHz clock can be used. This will be multiplexed onto the internal low-power clock and will therefore drive all components that are driven by that clock, including the power sequencer components. The external clock can be used in all power modes. When an external clock is in use, you can stop the LPOSC (see [Section 8.4\)](#page-566-0).

To select an external 32kHz clock:

- 1. Configure the GPIO source as described in [Section 12.10.7](#page-1198-1).
- 2. Switch to the external LPOSC by setting [EXT\\_TIME\\_REF.](#page-474-1)DRIVE\_LPCK. This register should only be written when [TIMER](#page-478-1).RUN = 0 and the power sequencer is inactive. You can only write to this register from Secure code.

The external 32kHz clock replaces the clock from LPOSC. Therefore the same registers are used for AON Timer configuration (see [Section 12.10.5.1](#page-1196-2)):

- [TIMER](#page-478-1).USE\_LPOSC
- [TIMER](#page-478-1).USING\_LPOSC
- [LPOSC\\_FREQ\\_KHZ\\_INT](#page-475-0)
- [LPOSC\\_FREQ\\_KHZ\\_FRAC](#page-475-1)

#### **12.10.5.3. Using the XOSC as the AON Timer Tick Source**

The XOSC clock is provided via the reference clock (clk\_ref). The user must ensure the reference clock is being driven from the XOSC before selecting it as the source of the AON Timer tick. This is the normal configuration following boot. To check, look for [CLK\\_REF\\_SELECTED](#page-535-2) = 0x4. The reference clock may be a divided version of the XOSC. The divisor defaults to 1 and can be read from [CLK\\_REF\\_DIV.](#page-535-1)INT. If the chip is operated with a faster XOSC, the clock sent to the AON Timer must not exceed 29MHz.

The AON Timer derives the 1ms tick from the XOSC using a 16.16 bit fractional divider whose divisor is initialised to 12000.0. This assumes a 12MHz crystal is used and the reference clock divisor is 1. If that is not the case, the divisor in the AON Timer can be modified by writing to the following registers:

- [XOSC\\_FREQ\\_KHZ\\_INT](#page-475-2) (default value: 12000)
- [XOSC\\_FREQ\\_KHZ\\_FRAC](#page-476-0) (default value: 0)

These registers should only be written when [TIMER](#page-478-1).RUN = 0 or [TIMER](#page-478-1).USING\_XOSC = 0.

To select the XOSC as the AON Timer tick source, write a 1 to [TIMER](#page-478-1).USE\_XOSC. It is not necessary to stop the AON Timer to do this. The newly selected tick will be synchronised to the current tick, so the operation may take up to 1 tick cycle (1ms in normal operation). When the operation is complete [TIMER.](#page-478-1)USE\_XOSC will self-clear and [TIMER](#page-478-1).USING\_XOSC will be set. Due to sampling, a small error of up to 2 periods of the newly selected clock will be subtracted from the time. When switching to XOSC at 12MHz an error of up to 167ns will be subtracted.

When the chip core is powered down the XOSC will stop. If [TIMER.](#page-478-1)USING\_XOSC is set, the power-down sequencer automatically reverts to [TIMER.](#page-478-1)USING\_LPOSC before the XOSC stops.

#### **12.10.5.4. Using an External 1ms Tick Source**

To select an external 1ms tick source, configure the GPIO source as described in [Section 12.10.7](#page-1198-1). Then, write a 1 to [TIMER](#page-478-1).USE\_GPIO\_1KHZ. It is not necessary to stop the AON Timer to do this, however the newly selected tick will not be synchronised to the current tick, so the operation so the operation will advance the time by up to 1ms. If using an external 1ms tick it is recommended to set the time after selecting the source. When the operation is complete [TIMER](#page-478-1).USE\_GPIO\_1KHZ will self-clear and [TIMER](#page-478-1).USING\_GPIO\_1KHZ will be set.

The tick is triggered from the falling edge of the selected GPIO. For correct sampling, the GPIO pulse width and interval must both be greater than the period of LPOSC (>31us). This limits the maximum frequency of the external tick to

16kHz.

The external 1ms tick can be used in all power modes.

