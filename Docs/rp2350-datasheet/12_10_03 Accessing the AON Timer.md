# 12.10.3 Accessing the AON Timer

To start and stop the AON Timer, write to [TIMER](#page-478-1).RUN.

To read the current 64-bit AON Timer value, use the following 2 × 32-bit read-only registers:

- [READ\\_TIME\\_UPPER](#page-477-2)
- [READ\\_TIME\\_LOWER](#page-477-3)

Because the AON Timer can increment during a read, use the following procedure to protect against erroneous reads:

- 1. Read [READ\\_TIME\\_UPPER](#page-477-2)
- 2. Read [READ\\_TIME\\_LOWER](#page-477-3)
- 3. Read [READ\\_TIME\\_UPPER](#page-477-2)
- 4. If the [READ\\_TIME\\_UPPER](#page-477-2) value changes between steps 1 and 3, repeat the whole procedure

When used as a real time clock, the 64-bit time value is set using 4 × 16-bit registers. These registers can only be written when the AON Timer is stopped by writing a 0 to [TIMER.](#page-478-1)RUN:

- [SET\\_TIME\\_63TO48](#page-476-1)
- [SET\\_TIME\\_47TO32](#page-476-2)
- [SET\\_TIME\\_31TO16](#page-476-3)
- [SET\\_TIME\\_15TO0](#page-476-4)

These registers cannot be used to read the time value.

When used as an interval timer, write a 1 to [TIMER.](#page-478-1)CLEAR to clear the timer value. It is not necessary to stop the AON Timer to do this. The [TIMER](#page-478-1).CLEAR register is self-clearing: it returns to 0 when the operation completes. This allows easy implementation of an alarm that wakes the chip or generates an interrupt at regular intervals.

