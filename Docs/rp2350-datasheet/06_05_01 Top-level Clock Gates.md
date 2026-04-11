# 6.5.1 Top-level Clock Gates

Each clock domain (for example, the system clock) may drive a large number of distinct hardware blocks, not all of which may be required at once. To avoid unnecessary power dissipation, each individual endpoint of each clock (for example, the UART system clock input) may be disabled at any time.

Enabling and disabling a clock gate is glitch-free. If a peripheral clock is temporarily disabled, and subsequently reenabled, the peripheral will be in the same state as prior to the clock being disabled. No reset or reinitialisation should be required.

Clock gates are controlled by two sets of registers: the WAKE\_ENx registers (starting at [WAKE\\_EN0\)](#page-545-0) and SLEEP\_ENx registers (starting at [SLEEP\\_EN0\)](#page-547-0). These two sets of registers are identical at the bit level, each possessing a flag to control each clock endpoint. The WAKE\_EN registers specify which clocks are enabled whilst the system is awake, and the SLEEP\_ENx registers select which clocks are enabled while the processor is in the SLEEP state [\(Section 6.5.2\)](#page-486-1).

The two processors do not have externally-controllable clock gates. Instead, the processors gate the clocks of their subsystems autonomously, based on execution of WFI/WFE instructions, and external Event and IRQ signals.

