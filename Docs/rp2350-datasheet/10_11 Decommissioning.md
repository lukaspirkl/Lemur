# 10.11 Decommissioning

Devices returned to Raspberry Pi Ltd for fault analysis must be **decommissioned** before return, to restore factory test functionality. A device is decommissioned by programming the OTP [PAGE63\\_LOCK0](#page-1320-0).RMA flag to 1. Return may be requested by Raspberry Pi Ltd when diagnosing systematic issues across a population of devices.

Setting the RMA flag has two effects:

- The factory test JTAG interface is re-enabled, irrespective of the values of any [CRIT1](#page-1304-0) flags.
- Pages 3 through 61 become permanently inaccessible: this is all pages which do not have predefined contents listed in [Section 13.9.](#page-1288-0)

10.10. Factory Test JTAG **871**

The effect on OTP contents is as though all had been promoted to the inaccessible lock level:

- write attempts will fail
- read attempts will return all-ones, when read via an unguarded read alias, or bus faults, when read via a guarded read alias

The logic which disables OTP access and the logic which re-enables the test interface are driven from the same signal internally, so this bit does not provide external access to user OTP contents, provided no sensitive material is stored in pages 0, 1, 2, 62 or 63. Setting the RMA flag is irreversible, and may render the device permanently unusable, if it is configured to boot from OTP contents stored in pages 3 through 61.

After setting the RMA flag, test the OTP access (e.g. via the SWD interface) and verify for yourself that any sensitive data stored in OTP has been made inaccessible.

The page 63 lock word has no other function besides RMA, since pages 62/63 contain the lock words themselves, each of which is protected by its own permissions. This means the RMA flag can be write-protected by setting either a hard or soft lock on page 63.

10.11. Decommissioning **872**

# <span id="page-873-0"></span>**Chapter 11. PIO**

