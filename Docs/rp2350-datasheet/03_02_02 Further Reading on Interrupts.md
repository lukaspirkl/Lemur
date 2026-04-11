# 3.2.2 Further Reading on Interrupts

This section describes the routing of system-level interrupt requests to the processor subsystem. It omits important details such as the processor's response to receiving an interrupt, and how processors choose which system-level interrupt requests to subscribe to. The following is a selection of relevant information for these topics:

- [Section 3.7.2.5](#page-127-0) describes the Cortex-M33's internal interrupt controller, the NVIC
- Register listings starting from [NVIC\\_ISER0](#page-179-1) describe controls for NVIC operation
- [Section 3.7.4.6](#page-135-0) is an overview of Cortex-M33 exception handling

3.2. Interrupts **83**

- The [Armv8-M Architecture Reference Manual](https://developer.arm.com/documentation/ddi0553/latest/) describes detailed architecture rules for exception handling
- [Section 3.8.4](#page-282-0) describes standard RISC-V trap handling
- [Section 3.8.4.2](#page-284-0) describes the standard RISC-V external, timer and software interrupt requests, and how they are connected on RP2350
- [Section 3.8.6.1](#page-287-0) describes the Xh3irq interrupt controller, which provides priority-controlled interrupt support for the system-level interrupts on Hazard3
- Each peripheral has its own interrupt registers which control the assertion of its system-level interrupts listed in [Table 94](#page-82-3) — see peripheral documentation for more information

