# 10.10 Factory Test JTAG

RP2350 contains JTAG hardware that is used to test devices after manufacturing. It is not a public interface, but its capabilities are documented here for user risk assessment.

Much like the user-facing SWD debug, the JTAG interface is disabled at power-on, and enabled only once the OTP power-on state machine has completed. If the [CRIT1.](#page-1304-0)SECURE\_BOOT\_ENABLE, [CRIT1.](#page-1304-0)SECURE\_DEBUG\_DISABLE or [CRIT1.](#page-1304-0)DEBUG\_DISABLE flag is set, then the JTAG interface remains held in reset indefinitely, so it cannot be communicated with and cannot control internal hardware. The only way to re-enable the JTAG interface after setting one of these critical flags is to set the RMA OTP flag [\(Section 10.11\)](#page-871-1), which also permanently disables read and write access to user OTP pages. The RMA flag itself is write-protected using the page 63 protection flags, so you can prevent untrusted software from programming the RMA flag.

To take the JTAG interface out of reset, write to bit 0 of the RP-AP control register, accessed via SWD. To connect the JTAG interface to GPIOs (TCK, TMS, TDI, TDO on GPIO0 → 3), set bit 1 of the RP-AP control register. The RP-AP is always accessible, even when external debug is disabled, because it is also used to enter the debug keys [\(Section 3.5.9.2](#page-92-1)). However, attempts to remove the JTAG reset are ignored when any of the aforementioned critical OTP flags are set.

The JTAG interface provides:

- Standard test capabilities such as IDCODE and EXTEST (boundary scan); these are not guaranteed to be IEEEcompliant, as this is an internal factory test interface, not a user-facing debug port
- Full AHB bus access, with Secure and Privileged attributes and an HMASTER of 3 (debugger)
- Asynchronous access to a small subset of register controls, generally limited to clocks, oscillators and reset controls

The JTAG interface's AHB bus access is muxed in place of the DMA read port, when the JTAG interface is enabled.

Any and all details of the factory test JTAG interface, with the exception of which OTP flags disable and re-enable it, are subject to change with revisions of the RP2350 silicon.

