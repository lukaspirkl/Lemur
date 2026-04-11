# 12.9.4 Control Watchdog Reset Levels

To control the level of reset triggered by a watchdog event, use the registers outside the watchdog register block:

- POWMAN\_WATCHDOG allows the watchdog to trigger chip level resets
- PSM\_WDSEL allows the watchdog to trigger system resets by running a full or partial PSM sequence (Power-on State Machine)
- RESETS\_WDSEL allows the watchdog to trigger subsystem resets

These are described in the Resets section, see [Chapter 7](#page-491-0).

