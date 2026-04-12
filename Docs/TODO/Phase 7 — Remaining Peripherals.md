## Phase 7 — Remaining Peripherals

These can be done in any order after their dependencies are in place.

### 7.1 ROSC (Ring Oscillator)

File: `Venture/Peripherals/ROSC.cs` (new, currently `.Unimplemented()`)

- [ ] Create at `0x400e8000`. Implement `CTRL` (enable/frequency range), `STATUS` (STABLE bit set),
  `FREQA/FREQB` (drive strength config), `COUNT` (oscillation counter), `DORMANT` (power mode).
- [ ] `STATUS.STABLE` should return 1 immediately (no physical oscillator to wait for).

### 7.2 TRNG (True Random Number Generator)

File: `Venture/Peripherals/TRNG.cs` (new, currently `.Unimplemented()`)

- [ ] Create at `0x400f0000`.
- [ ] **`RNG_IMR`** (0x100): interrupt mask.
- [ ] **`RNG_ISR`** (0x104): interrupt status (EHR_VALID flag).
- [ ] **`RNG_ICR`** (0x108): interrupt clear.
- [ ] **`TRNG_CONFIG`** (0x10C): ring oscillator configuration.
- [ ] **`TRNG_VALID`** (0x110): EHR_VALID flag (data ready).
- [ ] **`EHR_DATA0-5`** (0x114-0x128): 192-bit entropy harvest register.
- [ ] **`RND_SOURCE_ENABLE`** (0x12C): enable entropy source.
- [ ] **`SAMPLE_CNT1`** (0x130): sample count.
- [ ] **`TRNG_DEBUG_CONTROL`** (0x138): bypass and debug flags.
- [ ] **`TRNG_SW_RESET`** (0x140): software reset bit.
- [ ] **Behavior**: Return `System.Security.Cryptography.RandomNumberGenerator` data in `EHR_DATA0-5`,
  set `TRNG_VALID=1` immediately after `RND_SOURCE_ENABLE=1`.
- [ ] **Wire interrupt**: IRQ 39.

### 7.3 TICKS (Tick Generators)

File: `Venture/Peripherals/Ticks.cs` (new, currently `.Unimplemented()`)

- [ ] Create at `0x40108000`.
- [ ] 7 tick generators: PROC0, PROC1, TIMER0, TIMER1, WATCHDOG, RISCV, POW.
- [ ] Each has `CTRL` (`ENABLE` bit, `CYCLES` field 0-8 = number of clk_ref cycles per tick),
  `RESULT` (read-only, running status + tick count).
- [ ] These are used by hardware timers and the RISC-V platform timer to generate periodic ticks.
  For emulation, treat `CYCLES` as a divisor and tick every `CYCLES` clk_ref periods.

### 7.4 SYSINFO / SYSCFG

- [ ] **SYSINFO** (`0x40000000`): Implement real register values.
  - `CHIP_ID` (0x00): `0x01002927` (RP2350 JEDEC ID + part number).
  - `PLATFORM` (0x04): bits for FPGA/ASIC/QFN flags. Return `0x00000000` for ASIC.
  - `GITREF_RP2350` (0x40): a specific Git reference hash. Return `0x00000000`.
- [ ] **SYSCFG** (`0x40008000`): Key registers:
  - `PROC_CONFIG` (0x00): `PROC0_HALTED`/`PROC1_HALTED` read bits. `PROC1_NMI_MASK` and `PROC0_NMI_MASK`.
  - `PROC_IN_SYNC_BYPASS` (0x04), `PROC_IN_SYNC_BYPASS_HI` (0x08): GPIO synchronizer bypass.
  - `DBGFORCE` (0x0C): force debug signals.
  - `MEMPOWERDOWN` (0x10): power down memory banks.

### 7.5 PSM (Power-on State Machine)

File: currently `.Unimplemented()` at `0x40018000`

- [ ] **`FRCE_ON`** (0x00): force power-on for specific subsystems.
- [ ] **`FRCE_OFF`** (0x04): force power-off.
- [ ] **`WDSEL`** (0x08): watchdog reset enable per subsystem.
- [ ] **`DONE`** (0x0C): read-only, which subsystems are powered and ready. Return `0x00FFFFFF`
  (all subsystems ready) to prevent firmware from hanging.

### 7.6 ACCESSCTRL

File: currently `.Unimplemented()` at `0x40060000`

- [ ] Implement as register storage with no enforcement. Read back whatever was written.
  The RP2350 uses this for TrustZone-like access control; the emulator can treat all
  access as secure/privileged.

### 7.7 BUSCTRL

File: currently `.Unimplemented()` at `0x40068000`

- [ ] **`BUS_PRIORITY`** (0x00): priority setting for Cortex-M33 / DMA / RISC-V. Read/write storage.
- [ ] **`BUS_PRIORITY_ACK`** (0x04): read-only, returns same value as `BUS_PRIORITY`.
- [ ] **`PERFCTR0-3`** / **`PERFSEL0-3`**: bus performance counters. Stub as zero.

### 7.8 XIP_CTRL / XIP_AUX

File: currently `.Unimplemented()` at `0x400c8000`

- [ ] **`CTRL`** (0x00): XIP enable, power-down cache. Key bit: `EN` (bit 0). Return `0x00000003`.
- [ ] **`FLUSH`** (0x04): write to flush cache. Reads back 0 when flush complete.
- [ ] **`STAT`** (0x08): `FIFO_EMPTY` (bit 2), `FLUSH_READY` (bit 1). Return `0x00000002`.
- [ ] **`CTR_HIT`** / `CTR_ACC`** (0x0C/0x10): cache hit/access counters. Stub as zero.
- [ ] **`STREAM_ADDR`** / `STREAM_CTR` / `STREAM_FIFO`** (0x14/0x18/0x1C): streaming DMA through XIP.

### 7.9 USB

File: currently `.Unimplemented()` at `0x50110000` (partial stub exists)

USB is very complex. A minimal stub that makes boot software happy:
- [ ] Implement key `SIE_CTRL` (offset 0x4C) — already returns `0x00048000`, keep this.
- [ ] Add `USB_PWR` (0xE0): `VBUS_DETECT` bit, `VBUS_DETECT_OVERRIDE_EN`. Return `0x00000000`.
- [ ] Add `USBPHY_DIRECT` (0xCC), `USBPHY_DIRECT_OVERRIDE` (0xD0): USB PHY controls.
- [ ] Add `MAIN_CTRL` (0x40): `SIM_TIMING`, `HOST_NDEVICE`, `CONTROLLER_EN`. Return `0x00000001`.
- [ ] Full USB implementation is a separate major effort — skip until other peripherals are done.