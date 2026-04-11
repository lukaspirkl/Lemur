# 7.4.1 Reset Sequence

Following a chip-level reset, the Power-on State Machine (PSM):

- 1. Removes cold reset to processors.
- 2. Takes OTP out of reset. OTP reads any content required to boot and asserts rst\_done.
- 3. Starts the Ring Oscillator. Asserts rst\_done once the oscillator output is stable.
- 4. Removes Crystal Oscillator (XOSC) controller reset. The XOSC does not start yet, so rst\_done is asserted immediately.
- 5. Deasserts the master subsystem reset, but does not remove individual subsystem resets.
- 6. Starts the clk\_ref and clk\_sys clock generators. In the initial configuration, clk\_ref runs from the ring oscillator with no divider and clk\_sys runs from clk\_ref.
- 7. The PSM confirms the clocks are active.
- 8. Removes Bus Fabric reset and initialises logic.
- 9. Removes various memory controllers' resets and initialises logic.
- 10. Removes Single-cycle IO subsystem (SIO) reset and initialises logic.
- 11. Removes Access Controller reset and initialises logic.
- 12. Deasserts Processor Complex reset. Both core 0 and core 1 start executing the boot code from ROM. The boot code reads the core id and core 1 sleeps, leaving core 0 to continue bootrom execution.

Following a watchdog reset trigger, the PSM restarts from a point selected by the PSM [WDSEL](#page-498-0) register.

## <span id="page-496-0"></span>**7.4.2. Register Control**

The PSM is a fully automated piece of hardware: it requires no input from the user to work. The debugger can trigger a full or partial sequence by writing to the FRCE\_OFF register. The FRCE\_ON register is a development feature that does nothing in production devices.

