## Phase 4 — New Simple Peripherals

### 4.1 SPI0 / SPI1

Files: `Venture/Peripherals/SPI0.cs`, `SPI1.cs` (new, currently `.Unimplemented()`)

- [ ] Create `SPI0.cs` at `0x40080000` and `SPI1.cs` at `0x40088000`.
- [ ] **`SSPCR0`** (offset 0x00): `DSS` (data size 4-16 bit), `FRF` (frame format: Motorola SPI,
  TI SS, National Microwire), `SPO`/`SPH` (clock polarity/phase), `SCR` (serial clock rate).
- [ ] **`SSPCR1`** (offset 0x04): `MS` (master/slave), `SSE` (enable), `LBM` (loopback).
- [ ] **`SSPDR`** (offset 0x08): Write = TX FIFO push, Read = RX FIFO pop.
- [ ] **`SSPSR`** (offset 0x0C): `TFE` (TX FIFO empty), `TNF` (TX not full), `RNE` (RX not empty),
  `RFF` (RX full), `BSY` (busy).
- [ ] **`SSPCPSR`** (offset 0x10): clock prescale divisor (must be even, 2-254).
- [ ] **`SSPIMSC`** (offset 0x14): interrupt mask.
- [ ] **`SSPRIS`** (offset 0x18): raw interrupt status.
- [ ] **`SSPMIS`** (offset 0x1C): masked interrupt status.
- [ ] **`SSPICR`** (offset 0x20): interrupt clear (RORIC, RTIC only).
- [ ] **`SSPDMACR`** (offset 0x24): DMA enable (TXDMAE, RXDMAE).
- [ ] **Peripheral ID registers** (0xFE0-0xFEC): return PrimeCell ID values.
- [ ] **Loopback mode**: When `LBM=1`, route TX FIFO directly to RX FIFO.
- [ ] **Expose `ReceivedData` event** (like UART): for UI/device observation.
- [ ] **Wire interrupts to `IrqController`**: IRQ 31 (SPI0), IRQ 32 (SPI1).
- [ ] **DREQ outputs**: Signal `DREQ 24/25` (SPI0 TX/RX), `DREQ 26/27` (SPI1 TX/RX) for DMA.

### 4.2 I2C0 / I2C1

Files: `Venture/Peripherals/I2C0.cs`, `I2C1.cs` (new, currently `.Unimplemented()`)

I2C is more complex than SPI due to the start/stop/restart protocol.
Implement the register interface first, then behavior.

- [ ] Create `I2C0.cs` at `0x40090000` and `I2C1.cs` at `0x40098000`.
- [ ] **`IC_CON`** (0x00): `MASTER_MODE`, `SPEED` (standard/fast/fast+), `IC_10BITADDR_MASTER`,
  `IC_RESTART_EN`, `IC_SLAVE_DISABLE`, `STOP_DET_IF_MASTER_ACTIVE`, `TX_EMPTY_CTRL`.
- [ ] **`IC_TAR`** (0x04): target address (10-bit or 7-bit), general call bit.
- [ ] **`IC_SAR`** (0x08): own slave address.
- [ ] **`IC_DATA_CMD`** (0x10): combined TX/RX FIFO. Write: `CMD` bit (0=write, 1=read),
  `STOP` bit, `RESTART` bit, `DAT` (data byte). Read: `DAT`, `FIRST_DATA_BYTE`.
- [ ] **`IC_SS_SCL_HCNT/LCNT`** (0x14/0x18): standard mode clock timing.
- [ ] **`IC_FS_SCL_HCNT/LCNT`** (0x1C/0x20): fast mode clock timing.
- [ ] **`IC_INTR_STAT`** (0x2C), `IC_INTR_MASK` (0x30), `IC_RAW_INTR_STAT` (0x34),
  `IC_RX_TL` (0x38), `IC_TX_TL` (0x3C), `IC_CLR_*` registers (0x40-0x58).
- [ ] **`IC_ENABLE`** (0x6C): enable/disable bit.
- [ ] **`IC_STATUS`** (0x70): `ACTIVITY`, `TFNF` (TX not full), `TFE` (TX empty),
  `RFNE` (RX not empty), `RFF` (RX full), `MST_ACTIVITY`, `SLV_ACTIVITY`.
- [ ] **`IC_TXFLR`** (0x74), `IC_RXFLR` (0x78): FIFO level registers.
- [ ] **`IC_TX_ABRT_SOURCE`** (0x80): abort cause register.
- [ ] **`IC_SDA_HOLD`** (0x7C), `IC_SDA_SETUP` (0x94): timing config.
- [ ] **Transaction simulation**: In master mode, when `IC_DATA_CMD` is written with a byte
  and `STOP` is not set, accumulate the transaction. When `STOP` is set (or when RX read
  is requested in master-recv), raise `IC_INTR_STAT.STOP_DET`. For now, auto-complete
  transfers with no external slave by setting TX empty + STOP_DET.
- [ ] **Wire interrupts**: IRQ 36 (I2C0), IRQ 37 (I2C1).
- [ ] **DREQ outputs**: `DREQ 44/45` (I2C0 TX/RX), `DREQ 46/47` (I2C1 TX/RX).

### 4.3 PWM

File: `Venture/Peripherals/PWM.cs` (new, currently `.Unimplemented()`)

- [ ] Create `PWM.cs` at `0x400a8000`.
- [ ] **12 PWM slices**, each with:
  - `CHn_CSR` (offset `n*0x14 + 0x00`): `EN`, `PH_CORRECT`, `A_INV`, `B_INV`, `DIVMODE`
    (free run / B-pin gated / B-pin rising / B-pin falling), `PH_RET` (phase retard), `PH_ADV`.
  - `CHn_DIV` (offset `n*0x14 + 0x04`): 8.4 fractional clock divider (`INT`/`FRAC` fields).
  - `CHn_CTR` (offset `n*0x14 + 0x08`): current counter value (read/write).
  - `CHn_CC` (offset `n*0x14 + 0x0C`): compare values for A (bits 15:0) and B (bits 31:16).
  - `CHn_TOP` (offset `n*0x14 + 0x10`): wrap value (counter resets when it reaches TOP).
- [ ] **`EN` register** (0xF8): global enable bitmask for all 12 slices.
- [ ] **`INTR`** (0xF4), **`INTE`** (0x108 = slice 0...), **`INTF`**, **`INTS`**: per-slice wrap interrupts.
- [ ] **Counter simulation**: Increment counters using a background tick or on-demand evaluation
  (lazy evaluation based on elapsed time since last step is acceptable for emulation).
- [ ] **GPIO output**: Set the GPIO line for the A/B pins when FUNCSEL maps them to PWM.
  Output is HIGH when counter < CC value (for trailing-edge mode). Minimum: expose a
  `DutyCycleChanged` event with the slice/channel and duty cycle percentage.
- [ ] **Wire interrupts**: IRQ 8 (`PWM_IRQ_WRAP_0`), IRQ 9 (`PWM_IRQ_WRAP_1`) — each handles
  6 slices (INTE0 = slices 0-5, INTE1 = slices 6-11 per datasheet's IRQ1_INTE arrangement).
- [ ] **DREQ outputs**: `DREQ 32-43` (PWM_WRAP 0-11) — fires on each counter wrap for DMA pacing.

### 4.4 ADC

File: `Venture/Peripherals/ADC.cs` (new, currently `.Unimplemented()`)

- [ ] Create `ADC.cs` at `0x400a0000`.
- [ ] **`CS` register** (0x00): `EN` (enable), `TS_EN` (temperature sensor enable), `START_ONCE`
  (trigger single conversion), `START_MANY` (free-running), `READY` (read-only, conversion done),
  `ERR` / `ERR_STICKY`, `AINSEL` (channel select 0-4), `RROBIN` (round-robin channel mask).
- [ ] **`RESULT` register** (0x04): 12-bit conversion result (read-only).
- [ ] **`FCS` register** (0x08): FIFO control — `EN`, `SHIFT` (8-bit shift right), `ERR`, `DREQ_EN`,
  `EMPTY`, `FULL`, `LEVEL` (read-only), `THRESH` (DMA/interrupt threshold), `OVER`/`UNDER`.
- [ ] **`FIFO` register** (0x0C): pop from conversion FIFO.
- [ ] **`DIV` register** (0x10): 16.8 fractional pacing timer for free-running mode.
- [ ] **`INTR`** (0x14), **`INTE`** (0x18), **`INTF`** (0x1C), **`INTS`** (0x20): FIFO-level interrupt.
- [ ] **Channel voltage sources**: Expose a `Dictionary<int, Func<float>>` mapping channel
  numbers to voltage getters (0.0–3.3 V). Default all to 0V. Channel 4 = temperature sensor
  (return a fixed ~27°C equivalent: `0.706 / 3.3 * 4096 ≈ 876`).
- [ ] **Conversion simulation**: On `START_ONCE` write, call the getter for `AINSEL`, convert
  to 12-bit, push to FIFO, set `READY`.
- [ ] **Wire interrupt**: IRQ 35 (`ADC_IRQ_FIFO`).
- [ ] **DREQ output**: `DREQ 48` fires when FIFO level ≥ `FCS.THRESH` and `DREQ_EN` is set.