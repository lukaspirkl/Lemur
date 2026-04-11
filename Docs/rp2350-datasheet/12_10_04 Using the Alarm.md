# 12.10.4 Using the Alarm

To set the alarm time, use the following 4 × 16-bit registers:

- [ALARM\\_TIME\\_63TO48](#page-477-1)
- [ALARM\\_TIME\\_47TO32](#page-477-4)

- [ALARM\\_TIME\\_31TO16](#page-477-5)
- [ALARM\\_TIME\\_15TO0](#page-477-0)

To avoid false alarms, disable the alarm before setting the alarm time.

To enable the alarm, use [TIMER](#page-478-1).ALARM\_ENAB.

When the alarm fires, the AON Timer sets the alarm status flag [TIMER.](#page-478-1)ALARM.

To clear the alarm status flag, write a 1 to the alarm status flag.

To configure the alarm to trigger a power-up, set [TIMER](#page-478-1).PWRUP\_ON\_ALARM. This feature is not available to Non-secure code.

The alarm can be configured to trigger an interrupt. The interrupt is handled in the standard way using the following register fields:

- [INTR.](#page-484-3)TIMER raw interrupt
- [INTE](#page-484-4).TIMER interrupt enable
- [INTF](#page-485-1).TIMER force interrupt
- [INTS](#page-485-2).TIMER interrupt status

