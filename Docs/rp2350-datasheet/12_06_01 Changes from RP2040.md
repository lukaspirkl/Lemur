# 12.6.1 Changes from RP2040

The following new features have been added:

- Increased the number of DMA channels from 12 to 16.
- Increased the number of shared IRQ outputs from 2 to 4.
- Channels can be assigned to security domains using [SECCFG\\_CH0](#page-1134-0) through [SECCFG\\_CH15](#page-1134-0).
- The DMA now filters bus accesses using the built-in memory protection unit ([Section 12.6.6.3\)](#page-1101-0).
- Interrupts can be assigned to security domains using [SECCFG\\_IRQ0](#page-1135-0) through [SECCFG\\_IRQ3](#page-1135-0).
- Pacing timers and the CRC sniffer can be assigned to security domains using the [SECCFG\\_MISC](#page-1136-0) register.
- The four most-significant bits of TRANS\_COUNT ([CH0\\_TRANS\\_COUNT](#page-1123-0)) are redefined as the MODE field, which defines what happens when TRANS\_COUNT reaches zero:

- This backward-incompatible change reduces the maximum transfers in one sequence from 2<sup>32</sup>-1 to 2<sup>28</sup>-1.
- Mode 0x0 has the same behaviour as RP2040, so there is no need to modify software that performs less than 256 million transfers at a time.
- Mode 0x1, "trigger self", allows a channel to automatically restart itself after finishing a transfer sequence, in addition to the usual end-of-sequence actions like raising an interrupt or triggering other channels. This can be used for example to get periodic interrupts from streaming ring buffer transfers.
- Mode 0xf, "endless", allows a channel to run forever: TRANS\_COUNT does not decrement.
- New [CH0\\_CTRL\\_TRIG](#page-1124-0).INCR\_READ\_REV and [CH0\\_CTRL\\_TRIG](#page-1124-0).INCR\_WRITE\_REV fields allow addresses to decrement rather than increment, or to increment by two.
  - Some existing fields in the CTRL registers, such as [CH0\\_CTRL\\_TRIG](#page-1124-0).BUSY, have moved to accommodate the new fields.

Some existing behaviour has been refined:

- The logic which adjusts values read from WRITE\_ADDR and READ\_ADDR according to the number of in-flight transfers is disabled for address-wrapping and non-incrementing transfers (erratum RP2040-E12).
- You can now poll the ABORT register to wait for completion of an aborted channel (erratum RP2040-E13).
- DMA completion actions such as CHAIN\_TO are now strictly ordered against the last write completion, so a CHAIN\_TO on a channel whose registers you write to is a well-defined operation.
  - This enables the use of control blocks which do not include one of the four trigger register aliases.
  - Previously, a channel was considered to complete on the *first* cycle of its last write's data phase. Now, a channel is considered to complete on the *last* cycle of its last write's data phase. This is usually the same cycle, but it can be later when the DMA encounters a write data-phase bus stall.
- Previously, the DMA's internal arbitration logic inserted an idle cycle after completing a round of active high-priority channels ([CH0\\_CTRL\\_TRIG.](#page-1124-0)HIGH\_PRIORITY), even if there were no active low-priority requests. This reduced DMA throughput when lightly loaded. This idle cycle has been removed, eliminating lost throughput.
- IRQ assertion latency has been reduced by one cycle.

