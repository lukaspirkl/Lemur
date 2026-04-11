# 12.7.2 Changes from RP2040

All changes from RP2040 are a superset of the RP2040 features. Existing software for the RP2040 USB controller will continue to work with one exception: you must clear the [MAIN\\_CTRL](#page-1159-0).PHY\_ISO bit at startup and after power down events. We recommend leaving the [LINESTATE\\_TUNING](#page-1171-0) register at its reset value. Software should not clear this register.

#### **12.7.2.1. Errata Fixes**

RP2350 fixes all RP2040 USB errata. This includes fixes for the following RP2040B0 and B1 errata which are also fixed by RP2040B2:

- RP2040-E2: USB device endpoint abort is not cleared
- RP2040-E5: USB device fails to exit RESET state on busy USB bus

For more information about RP2040B2, see the RP2040 datasheet.

RP2350 fixes the following RP2040B2 errata, which require software workarounds on RP2040B2:

- RP2040-E3: USB host: interrupt endpoint buffer done flag can be set with incorrect buffer select
- RP2040-E4: USB host writes to upper half of buffer status in single buffered mode
- RP2040-E15: USB Device controller will hang if certain bus errors occur during an IN transfer (see [Section](#page-1141-1) [12.7.2.2.4](#page-1141-1))

