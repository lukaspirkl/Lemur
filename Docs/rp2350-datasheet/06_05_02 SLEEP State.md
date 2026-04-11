# 6.5.2 SLEEP State

RP2350 enters the SLEEP state when all of the following are true:

- Both processors are asleep (e.g. in a WFE or WFI instruction)
- The system DMA has no outstanding transfers on any channel

RP2350 exits the SLEEP state when either processor is awoken by an interrupt.

When in the SLEEP state, the top-level clock gates are masked by the SLEEP\_ENx registers (starting at [SLEEP\\_EN0\)](#page-547-0), rather than the WAKE\_ENx registers (starting at [WAKE\\_EN0](#page-545-0)). This permits more aggressive pruning of the clock tree when the processors are asleep.

![](_page_486_Figure_12.jpeg)

Though it is possible for a clock to be enabled during SLEEP and disabled outside of SLEEP, this is generally not useful.

For example, if the system is sleeping until a character interrupt from a UART, the entire system except for the UART can be clock-gated (SLEEP\_ENx = all-zeroes except for CLK\_SYS\_UART0 and CLK\_PERI\_UART0). This includes system infrastructure such as the bus fabric.

When the UART asserts its interrupt and wakes a processor, RP2350 leaves SLEEP mode and switches back to the WAKE\_ENx clock mask. At the minimum, this should include the bus fabric and the memory devices containing the processor's stack and interrupt vectors.

A system-level clock request handshake holds the processors off the bus until the clocks are re-enabled.

## <span id="page-486-2"></span>**6.5.3. DORMANT State**

The DORMANT state is a true zero-dynamic-power sleep state, where all clocks (and all oscillators) are disabled. The system can awake from the DORMANT state upon a GPIO event (high/low level or rising/falling edge), or an AON Timer alarm: this restarts one of the oscillators (either ring oscillator or crystal oscillator) and ungates the oscillator output once it is stable. System state is retained, so code execution resumes immediately upon leaving the DORMANT state.

If relying on the AON Timer ([Section 12.10\)](#page-1194-0) to wake from the DORMANT state, the AON Timer must run from the LPOSC or an external clock source. The AON Timer accepts clock frequencies as low as 1Hz.

DORMANT does not halt PLLs. To avoid unnecessary power dissipation, software should power down PLLs before entering the DORMANT state, and power up and reconfigure the PLLs again after exiting.

If you halt the crystal oscillator (XOSC), you must also halt the PLLs to prevent them losing lock when their input reference clock stops. The PLL VCO may behave erratically when the frequency reference is lost, such as increasing to a very high frequency. Reconfigure and re-enable the PLLs after the XOSC starts again. Do not attempt to run clocks from the PLLs while the XOSC is stopped.

The DORMANT state is entered by writing a keyword to the DORMANT register in whichever oscillator is active: ring oscillator ([Section 8.3](#page-558-0)) or crystal oscillator ([Section 8.2](#page-552-0)). If both are active, the one providing the processor clock must be stopped last because it will stop software from executing.

#### <span id="page-487-2"></span>**6.5.3.1. Waking from the DORMANT State**

The system exits the DORMANT state on any of the following events:

- an alarm from the AON Timer which causes [TIMER.](#page-478-1)ALARM to assert
- the assertion of an interrupt from GPIO Bank 0 to the DORMANT\_WAKE interrupt destination
- the assertion of an interrupt from GPIO Bank 1 to the DORMANT\_WAKE interrupt destination

When waking from the AON Timer you do not have to enable the IRQ output from POWMAN. It is sufficient for the timer to fire, without being mapped to an interrupt output. Any AON Timer alarm comparison event which causes [TIMER](#page-478-1).ALARM to assert causes the system to exit the DORMANT state. It is the actual alarm event which causes the exit, not the [TIMER.](#page-478-1)ALARM status; if you enter the DORMANT state with the [TIMER.](#page-478-1)ALARM status set to 1, but the timer alarm comparison logic *disabled* by [TIMER.](#page-478-1)ALARM\_ENAB, you will not exit the DORMANT state.

The GPIO Bank registers have interrupt enable registers for interrupts targeting the DORMANT mode wake logic, such as [DORMANT\\_WAKE\\_INTE0.](#page-738-0) These are identical to the interrupt enable registers for interrupts targeting the processors, such as [PROC0\\_INTE0.](#page-702-0)

Waking from the DORMANT state restarts the oscillator which was disabled by entry to the DORMANT state. It does not restart any other oscillators, or change any system-level clock configuration.

