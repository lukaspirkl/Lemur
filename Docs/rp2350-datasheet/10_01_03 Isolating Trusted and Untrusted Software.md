# 10.1.3 Isolating Trusted and Untrusted Software

In security- or safety-critical applications, access must be limited to those who need it. For example, a JPEG decode library should not be able to access the core voltage regulator and increase DVDD to 3.3 V (unless you are decompressing a *very* large JPEG). The Cortex-M33 processors contain hardware that separates two execution contexts, known as Secure and Non-secure, and enforces a number of invariants between them, such as:

- Non-secure code cannot access Secure memory
- The Secure context cannot execute Non-secure memory
- Non-secure code cannot directly access peripherals managed by Secure code
- Non-secure code cannot prevent Secure interrupts from being serviced

By making less of your code able to access your most critical hardware and data, you reduce the chance of accidentally exposing this critical hardware and data to the outside world. For a high-level explanation of how the Cortex-M33 implements this, see [Section 10.2](#page-816-0). For full details, see [Armv8-M Architecture Reference Manual](https://developer.arm.com/documentation/ddi0553/latest/).

10.1. Overview (Arm) **815**

To make the programming model of Secure and Non-secure software consistent, and to avoid overhead in Non-secure code, RP2350 extends Secure/Non-secure separation throughout the system. For example, DMA channels can be assigned for Secure or Non-secure use. Using this extended separation, Non-secure code can use DMA transfers to accelerate peripheral accesses without endangering security model invariants (such as Non-secure code using the DMA to read Secure memory).

The key hardware features that enable Secure/Non-secure separation throughout the system are:

- The Cortex-M33's implementation of the Secure and Non-secure states ([Section 10.2](#page-816-0))
- The DMA's implementation of matching per-channel security states [\(Section 10.7](#page-864-0))
- The system-level bus access filtering implemented by ACCESSCTRL [\(Section 10.6](#page-819-1))
- Peripheral-level filtering, such as the per-GPIO access filtering of the SIO GPIO registers ([Section 3.1.1\)](#page-37-0)

