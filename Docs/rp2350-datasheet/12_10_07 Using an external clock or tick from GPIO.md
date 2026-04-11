# 12.10.7 Using an external clock or tick from GPIO

The following features use a GPIO as a clock or a tick:

- external 32kHz clock source
- external 1kHz tick
- external 1Hz tick

Only 4 GPIOs are available for these features. You can only select one, because they share the same GPIO selection logic. The set of 4 GPIOs differs between package types. The selection is controlled by a 2-bit register field.

The AON Timer uses the following GPIOs:

- [EXT\\_TIME\\_REF.](#page-474-1)SOURCE\_SEL = 0 → GPIO12
- [EXT\\_TIME\\_REF.](#page-474-1)SOURCE\_SEL = 1 → GPIO20
- [EXT\\_TIME\\_REF.](#page-474-1)SOURCE\_SEL = 2 → GPIO14
- [EXT\\_TIME\\_REF.](#page-474-1)SOURCE\_SEL = 3 → GPIO22

