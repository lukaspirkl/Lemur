# 11.1.1 Changes from RP2040

RP2350 adds the following new registers and controls:

- [DBG\\_CFGINFO](#page-946-0).VERSION indicates the PIO version, to allow PIO feature detection at runtime.
  - This 4-bit field was reserved-0 on RP2040 (indicating version 0), and reads as 1 on RP2350.
- [GPIOBASE](#page-953-0) adds support for more than 32 GPIOs per PIO block.
  - Each PIO block is still limited to 32 GPIOs at a time, but [GPIOBASE](#page-953-0) selects *which* 32.
- [CTRL](#page-941-0).NEXT\_PIO\_MASK and [CTRL.](#page-941-0)PREV\_PIO\_MASK apply some [CTRL](#page-941-0) register operations to state machines in neighbouring PIO blocks simultaneously.
  - [CTRL](#page-941-0).NEXTPREV\_SM\_DISABLE stops PIO state machines in multiple PIO blocks simultaneously.
  - [CTRL](#page-941-0).NEXTPREV\_SM\_ENABLE starts PIO state machines in multiple PIO blocks simultaneously.
  - [CTRL](#page-941-0).NEXTPREV\_CLKDIV\_RESTART synchronises the clock dividers of PIO state machines in multiple PIO blocks
- [SM0\\_SHIFTCTRL.](#page-948-0)IN\_COUNT masks unneeded IN-mapped pins to zero.
  - This is useful for MOV x, PINS instructions, which previously always returned a full rotated 32-bit value.
- [IRQ0\\_INTE](#page-954-0) and [IRQ1\\_INTE](#page-956-0) now expose all eight SM IRQ flags to system-level interrupts (not just the lower four).
- Registers starting from [RXF0\\_PUTGET0](#page-950-0) expose each RX FIFO's internal storage registers for random read *or* write access from the system,
  - The new FJOIN\_RX\_PUT FIFO join mode enables random writes from the state machine, and random reads from the system (for implementing status registers).
  - The new FJOIN\_RX\_GET FIFO join mode enables random reads from the state machine, and random writes from the system (for implementing control registers).
  - Setting both FJOIN\_RX\_PUT and FJOIN\_RX\_GET enables random read *and* write access from the state machine, but disables system access.

RP2350 adds the following new instruction features:

- Adds PINCTRL\_JMP\_PIN as a source for the WAIT instruction, plus an offset in the range 0-3.
  - This gives WAIT pin arguments a per-SM mapping that is independent of the IN-mapped pins.
- Adds PINDIRS as a destination for MOV.
  - This allows changing the direction of all OUT-mapped pins with a single instruction: MOV PINDIRS, NULL or MOV PINDIRS, ~NULL
- Adds SM IRQ flags as a source for MOV x, STATUS
  - This allows branching (as well as blocking) on the assertion of SM IRQ flags.
- Extends IRQ instruction encoding to allow state machines to set, clear and observe IRQ flags from different PIO blocks.
  - There is no delay penalty for cross-PIO IRQ flags: an IRQ on one state machine is observable to all state machines on the next cycle.
- Adds the FJOIN\_RX\_GET FIFO mode.
  - A new MOV encoding reads any of the four RX FIFO storage registers into OSR.
  - This instruction permits random reads of the four FIFO entries, indexed either by instruction bits or the Y scratch register.
- Adds the FJOIN\_RX\_PUT FIFO mode.
  - A new MOV encoding writes the ISR into any of the four RX FIFO storage registers.
  - The registers are indexed either by instruction bits or the Y scratch register.

RP2350 adds the following security features:

- Limits Non-secure PIOs (set to via ACCESSCTRL) to observation of only Non-secure GPIOs. Attempting to read a Secure GPIO returns a 0.
- Disables cross-PIO functionality (IRQs, CTRL\_NEXTPREV operations) between Non-secure PIO blocks (those which permit Non-secure access according to ACCESSCTRL) and Secure-only blocks (those which do not).

RP2350 includes the following general improvements:

- Increased the number of PIO blocks from two to three (8 → 12 state machines).
- Improved GPIO input/output delay and skew.
- Reduced DMA request (DREQ) latency by one cycle vs RP2040.

