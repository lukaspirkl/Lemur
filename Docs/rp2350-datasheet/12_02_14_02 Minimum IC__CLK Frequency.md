# 12.2.14.2 Minimum IC\_CLK Frequency

This section describes the minimum ic\_clk frequencies that the DW\_apb\_i2c supports for each speed mode, and the associated high and low count values. In slave mode, [IC\\_SDA\\_HOLD](#page-1031-0) (Thd;dat) and [IC\\_SDA\\_SETUP](#page-1038-0) (Tsu:dat) need to be programmed to satisfy the I2C protocol timing requirements. The following examples are for the case where [IC\\_FS\\_SPKLEN](#page-1041-0) is programmed to two.

#### <span id="page-1002-0"></span>**12.2.14.2.1. Standard Mode (SM), Fast Mode (FM), and Fast Mode Plus (FM+)**

This section details how to derive a minimum ic\_clk value for standard and fast modes of the DW\_apb\_i2c. Although the following method shows how to do fast mode calculations, you can also use the same method in order to do calculations for standard mode and fast mode plus.

#### **NOTE**

The following computations do not consider the SCL\_Rise\_time and SCL\_Fall\_time.

Given conditions and calculations for the minimum DW\_apb\_i2c ic\_clk value in fast mode:

- Fast mode has data rate of 400 kb/s; implies SCL period of 1/400 kHz = 2.5μs
- Minimum hcnt value of 14 as a seed value; IC\_HCNT\_FS = 14
- Protocol minimum SCL high and low times:
  - MIN\_SCL\_LOWtime\_FS = 1300 ns
  - MIN\_SCL\_HIGHtime\_FS = 600 ns

Derived equations:

```
SCL_PERIOD_FS / (IC_HCNT_FS + IC_LCNT_FS) = IC_CLK_PERIOD
IC_LCNT_FS × IC_CLK_PERIOD = MIN_SCL_LOWtime_FS
```

Combined, the previous equations produce the following:

```
IC_LCNT_FS × (SCL_PERIOD_FS / (IC_LCNT_FS + IC_HCNT_FS) ) = MIN_SCL_LOWtime_FS
```

Solving for IC\_LCNT\_FS:

```
IC_LCNT_FS × (2.5μs / (IC_LCNT_FS + 14) ) = 1.3μs
```

The previous equation gives:

```
IC_LCNT_FS = roundup(15.166) = 16
```

These calculations produce IC\_LCNT\_FS = 16 and IC\_HCNT\_FS = 14, giving an ic\_clk value of:

```
2.5μs / (16 + 14) = 83.3ns = 12 MHz
```

Testing these results shows that protocol requirements are satisfied.

[Table 1052](#page-1003-0) lists the minimum ic\_clk values for all modes with high and low count values.

*Table 1052.* ic\_clk *in Relation to High and Low Counts*

<span id="page-1003-0"></span>

| Speed Mode | ic_clkfreq<br>(MHz) | Minimum<br>Value of<br>IC_*_SPKLEN | SCL Low Time<br>in `ic_clk`s | SCL Low<br>Program<br>Value | SCL Low Time SCL High | Time in<br>`ic_clk`s | SCL High<br>Program<br>Value | SCL High<br>Time |
|------------|---------------------|------------------------------------|------------------------------|-----------------------------|-----------------------|----------------------|------------------------------|------------------|
| SS         | 2.7                 | 1                                  | 13                           | 12                          | 4.7μs                 | 14                   | 6                            | 5.2μs            |
| FS         | 12.0                | 1                                  | 16                           | 15                          | 1.33μs                | 14                   | 6                            | 1.16μs           |
| FM+        | 32                  | 2                                  | 16                           | 15                          | 500 ns                | 16                   | 7                            | 500 ns           |

- The IC\_\*\_SCL\_LCNT and IC\_\*\_SCL\_HCNT registers are programmed using the SCL low and high program values in [Table](#page-1003-0) [1052,](#page-1003-0) which are calculated using SCL low count minus one, and SCL high counts minus eight, respectively. The values in [Table 1052](#page-1003-0) are based on IC\_SDA\_RX\_HOLD = 0. The maximum IC\_SDA\_RX\_HOLD value depends on the IC\_\*CNT registers in Master mode.
- In order to compute the HCNT and LCNT considering RC timings, use the following equations:

```
◦
   IC_HCNT_* = [(HCNT + IC_*_SPKLEN + 7) * ic_clk] + SCL_Fall_time
◦
   IC_LCNT_* = [(LCNT + 1) * ic_clk] - SCL_Fall_time + SCL_Rise_time
```

#### **12.2.14.3. Calculating High and Low Counts**

The calculations below show how to calculate SCL high and low counts for each speed mode in the DW\_apb\_i2c. For the calculations to work, the ic\_clk frequencies used must not be less than the minimum ic\_clk frequencies specified in [Table 1052.](#page-1003-0)

The default ic\_clk period value is set to 100 ns, so default SCL high and low count values are calculated for each speed

mode based on this clock. These values need updating according to the guidelines below.

The equation to calculate the proper number of ic\_clk signals required for setting the proper SCL clocks high and low times is as follows:

```
IC_xCNT = (ROUNDUP(MIN_SCL_xxxtime*OSCFREQ,0))
MIN_SCL_HIGHtime = Minimum High Period
MIN_SCL_HIGHtime = 4000ns for 100kb/s,
  600ns for 400kb/s,
  260ns for 1000kb/s,
MIN_SCL_LOWtime = Minimum Low Period
MIN_SCL_LOWtime = 4700ns for 100kb/s,
  1300ns for 400kb/s,
  500ns for 1000kb/s,
OSCFREQ = ic_clk Clock Frequency (Hz).
```

For example:

```
OSCFREQ = 100MHz
I2Cmode = fast, 400kb/s
MIN_SCL_HIGHtime = 600ns.
MIN_SCL_LOWtime = 1300ns.
IC_xCNT = (ROUNDUP(MIN_SCL_HIGH_LOWtime*OSCFREQ,0))
IC_HCNT = (ROUNDUP(600ns * 100MHz,0))
IC_HCNTSCL PERIOD = 60
IC_LCNT = (ROUNDUP(1300ns * 100MHz,0))
IC_LCNTSCL PERIOD = 130
Actual MIN_SCL_HIGHtime = 60*(1/100MHz) = 600ns
Actual MIN_SCL_LOWtime = 130*(1/100MHz) = 1300ns
```

