# 12.10.2 Changes from RP2040

The RP2040 Real Time Clock (RTC) is not used in RP2350. Instead, RP2350 has a timer in the Always-On power domain which is used for scheduling power-up events and can also be used as a real-time counter. The AON Timer works differently from the RP2040 RTC. It counts milliseconds to 64 bits and this value can be used to calculate the date and time in software if required.

