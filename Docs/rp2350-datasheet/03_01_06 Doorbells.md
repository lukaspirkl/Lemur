# 3.1.6 Doorbells

The doorbell registers raise an interrupt on the opposite core. There are 8 doorbell flags in each direction, combined into a single doorbell interrupt per core. This is a core-local interrupt: the same interrupt number on each core (SIO\_IRQ\_BELL, interrupt number 26) notifies that core of incoming doorbell interrupts.

Whereas the mailbox FIFOs are used for cross-core events whose count and order is important, doorbells are used for events which are accumulative (i.e. may post multiple times, but only answered once) and which can be responded to in any order.

Writing a non-zero value to the [DOORBELL\\_OUT\\_SET](#page-75-1) register raises the opposite core's doorbell interrupt. The interrupt remains raised until all bits are cleared. Generally, the opposite core enters its doorbell interrupt handler, reads its [DOORBELL\\_IN\\_CLR](#page-76-1) register to get the mask of active doorbell flags, and then writes back to acknowledge and clear the interrupt.

The [DOORBELL\\_IN\\_SET](#page-76-2) register allows a processor to ring its own doorbell. This is useful when the routine which rings a doorbell can be scheduled on either core. Likewise, for symmetry, a processor can clear the opposite core's doorbell flags using the [DOORBELL\\_OUT\\_CLR](#page-75-2) register: this is useful for setup code, but should be avoided in general because of the potential for race conditions when acknowledging interrupts meant for the opposite core.

At any time, a core can read back its [DOORBELL\\_OUT\\_SET](#page-75-1) or [DOORBELL\\_OUT\\_CLR](#page-75-2) register (they return the same result) to see the status of doorbell interrupts posted to the opposite core. Likewise, reading either [DOORBELL\\_IN\\_SET](#page-76-2) or [DOORBELL\\_IN\\_CLR](#page-76-1) returns the status of doorbell interrupts posted to this core.

# **NOTE**

RP2350 has separate per-core doorbell interrupt signals and doorbell registers for Secure and Non-secure SIO banks. Non-secure doorbells are posted on SIO\_IRQ\_BELL\_NS, interrupt number 28. See [Section 3.1.1](#page-37-0).

