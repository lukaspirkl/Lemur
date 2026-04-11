# 13.3.1 Lock Shim

The lock shim is inserted between the Synopsys AP block and the SHF block, and is used to enforce read/write page locks, based on:

- The OTP address presented on the AP → SHF bus
- The read/write strobe on the AP → SHF bus
- The security attribute of the upstream bus access which caused this SHF access (assumed to be Secure if SBPI is currently enabled via USR.DCTRL)

Because the Synopsys AP performs both reads and writes in the course of programming an OTP row, it is impossible to disable reads to an address without also disabling writes. Three lock states are supported:

- Read/Write
- Read-only
- Inaccessible

The full locking scheme is described in in [Section 13.5,](#page-1271-0) but to summarise:

- The lock state of each OTP page is read from OTP at boot time.
- There is a separate copy of the lock state for Secure/Non-secure accesses. The lock shim applies the Secure read permissions to Secure reads, and the Non-secure read permissions to Non-secure reads. There is no such rule for writes, because Non-secure code is not capable of accessing the programming hardware.
- The lock encoding in OTP storage is such that a page can always be locked down to a less permissive state (in the order RW → RO → Inaccessible) but can never return to a more permissive state.
- Software can advance the state of each individual lock at runtime without programming OTP, and this lasts until the OTP PSM is re-run.
- Software locks also obey the lock progression order (RW → RO → Inaccessible) and can not be regressed.

The full locking scheme is described in in [Section 13.5.](#page-1271-0)

