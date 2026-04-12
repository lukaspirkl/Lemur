## Phase 8 — OTP Completion

### 8.1 OTP Register Interface

- [ ] **OTP** (`0x40120000`, `OTP.cs`): Currently returns hardcoded values.
  Implement the full register set:
  - `SW_LOCK0-63` (0x00-0xFC): per-row software lock registers.
  - `SBPI_INSTR` (0x100): SBPI instruction register.
  - `SBPI_WDATA_0/1` (0x104/0x108): write data.
  - `SBPI_RDATA_0/1` (0x10C/0x110): read data.
  - `SBPI_STATUS` (0x114): status bits.
  - `USR` (0x118): user data.
  - `DBG` (0x11C): debug access.
  - `BIST` (0x124): built-in self test.
  - `CRT_KEY_W0-3` (0x128-0x134): critical key write registers.
  - `CRITICAL` (0x15C): critical flags (already partially implemented).
  - `KEY_VALID` (0x160): key validity.
  - `BOOTLOCK_STAT` (0x164): boot lock status.
  - `BOOTLOCK0-7` (0x168-0x184): boot lock acquire registers.
- [ ] **OTPData** (`0x40130000`, `OTPData.cs`): Implement backed storage (byte array of 4KB).
  Reads return from storage; writes (if unlocked) update storage. For emulation, initialize
  with factory-default values (all 0s except CHIPID row).