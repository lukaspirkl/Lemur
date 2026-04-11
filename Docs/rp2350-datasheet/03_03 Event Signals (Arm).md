# 3.3 Event Signals (Arm)

Using the WFE instruction, the Cortex-M33 can enter a sleep state until an "event" (or interrupt) takes place. It can also generate events using the SEV instruction. RP2350 cross-wires event signals between the two processors: an event sent by one processor will be received on the other.

# **NOTE**

The event flag is "sticky": if both processors send an event (SEV) simultaneously, then enter the sleep state (WFE), they will both wake immediately. This prevents the processors from getting stuck in a sleep state in this scenario.

Processors also receive an event signal from the global monitor if their reservation is lost due to a write by a different master, in accordance with Armv8-M architecture requirements.

While in a WFE (or WFI) sleep state, the processor shuts off its internal clock gates to reduce power consumption. When **both** processors are in a sleep state and the DMA is inactive, all of RP2350 can enter a sleep state, disabling clocks on unused infrastructure such as the bus fabric. The rest of RP2350 wakes automatically when either of the processors wakes. See [Section 6.5.2](#page-486-1).

