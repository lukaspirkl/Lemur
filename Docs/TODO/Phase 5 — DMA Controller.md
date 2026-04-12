## Phase 5 — DMA Controller

DMA is a major complexity jump — it moves data autonomously between peripherals and memory.
Do this after all DREQ-producing peripherals are at least partially stubbed.

File: `Venture/Peripherals/DMA.cs` (new, currently `.Unimplemented()`)

- [ ] Create `DMA.cs` at `0x50000000`.

### 5.1 Channel Registers (16 channels)

Each channel has registers at base `+ n*0x40`:
- [ ] **`CHn_READ_ADDR`** (0x00): source address, auto-increments per transfer if `INCR_READ=1`.
- [ ] **`CHn_WRITE_ADDR`** (0x04): destination address, auto-increments if `INCR_WRITE=1`.
- [ ] **`CHn_TRANS_COUNT`** (0x08): number of transfers. Write sets the reload value.
  Reading gives remaining count. A write with mode bits (bits 31:28) sets the trigger mode.
- [ ] **`CHn_CTRL_TRIG`** (0x0C): configuration + trigger. Key fields:
  - `EN` (bit 0): channel enable
  - `HIGH_PRIORITY` (bit 1): priority over other channels
  - `DATA_SIZE` (bits 3:2): 0=byte, 1=halfword, 2=word
  - `INCR_READ` (bit 4), `INCR_WRITE` (bit 5)
  - `RING_SIZE` (bits 9:6): wrap mask for addresses
  - `RING_SEL` (bit 10): which address wraps
  - `CHAIN_TO` (bits 15:11): chain to another channel on completion
  - `TREQ_SEL` (bits 21:15): DREQ source (0-54, 63 = always ready)
  - `IRQ_QUIET` (bit 22): suppress completion IRQ
  - `BSWAP` (bit 23): byte-swap
  - `SNIFF_EN` (bit 24): enable sniff (CRC)
  - `BUSY` (bit 25): read-only, transfer in progress
  - `WRITE_ERROR`/`READ_ERROR` (bits 30:29): error flags
  - `AHB_ERROR` (bit 31): read-only

### 5.2 System Registers

- [ ] **`INTR`** (0x400): raw interrupt status (one bit per channel).
- [ ] **`INTE0`** (0x404), `INTF0` (0x408), `INTS0` (0x40C): interrupt enable/force/status, set 0.
- [ ] **`INTE1/INTF1/INTS1`** (0x414/0x418/0x41C): second interrupt line.
- [ ] **`TIMER0-3`** (0x420-0x42C): fractional pacing timers (alternative to DREQ).
- [ ] **`MULTI_CHAN_TRIGGER`** (0x430): trigger multiple channels simultaneously.
- [ ] **`SNIFF_CTRL`** (0x434), `SNIFF_DATA` (0x438): CRC calculation on DMA data.
- [ ] **`FIFO_LEVELS`** (0x440): read-only FIFO fill levels for debugging.
- [ ] **`CHAN_ABORT`** (0x444): abort in-progress transfers.
- [ ] **`N_CHANNELS`** (0x448): read-only, returns 16.

### 5.3 DMA Transfer Engine

- [ ] **DREQ subscriptions**: `DMA` subscribes to each DREQ source (SPI, UART, I2C, ADC, PWM, PIO,
  SHA256). When a DREQ fires, find all channels with matching `TREQ_SEL` and advance their
  credit counter. `TREQ_SEL=63` (permanent) channels run as fast as possible.
- [ ] **Transfer execution**: On each emulator step (or as a separate tick), if a channel
  has credits and is enabled and not waiting, execute one transfer unit:
  read `DATA_SIZE` bytes from `READ_ADDR`, write to `WRITE_ADDR`, decrement `TRANS_COUNT`.
  Update addresses if `INCR_READ`/`INCR_WRITE`. Apply ring buffer wrap if `RING_SIZE != 0`.
- [ ] **Completion**: When `TRANS_COUNT` reaches 0, set `INTR` bit for that channel.
  If `CHAIN_TO` points to another channel (not self), trigger that channel.
  If `IRQ_QUIET=0` and `INTE` bit set, raise `DMA_IRQ_0` (IRQ 10) or `DMA_IRQ_1` (IRQ 11).
- [ ] **Wire interrupts**: IRQ 10 (`DMA_IRQ_0`), IRQ 11 (`DMA_IRQ_1`).