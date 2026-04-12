## Phase 3 — Complete Existing Peripherals

### 3.1 UART Interrupt Delivery

The UART register set is fully defined. What's missing is routing to the CPU.

- [ ] **Wire `UARTINTR` to `IrqController`**: In UART's `OnWrite` for `UARTICR` and on any
  state change that affects `UARTMIS`, call `IrqController.RaiseIrq(33)` (UART0) or
  `RaiseIrq(34)` (UART1) when `UARTMIS != 0`, and `ClearIrq` when it becomes 0.
  This is the TODO at `UART.cs:363`.
- [ ] **Receive timeout interrupt (`RTIM`)**: Set `UARTRIS.RTIM` when the RX FIFO contains
  data but no new data has arrived for 32 bit periods (approximate with a `System.Threading.Timer`
  or poll in the emulator loop). This is noted in `UART.cs:349` and `UART.cs:488`.
- [ ] **FIFO level triggers**: `UARTRIS.TXRIS` fires when TX FIFO depth ≤ trigger level
  (set by `UARTIFLS.TXIFLSEL`). `UARTRIS.RXRIS` fires when RX FIFO depth ≥ trigger level.
  Currently these are set/cleared but need to be re-evaluated after every FIFO push/pop.

### 3.2 UART GPIO Wiring

- [ ] **TX pin output**: When UART transmits a character, drive the TX GPIO line (pin depends on
  `FUNCSEL`; e.g. GPIO 0 = UART0 TX for function 2). The actual GPIO line should transition
  High/Low to reflect the serial data stream, or at minimum hold High (idle) when not
  transmitting. Required for external device simulation.
- [ ] **RX pin input**: Read from the GPIO line mapped to UART RX. When the GPIO line transitions
  (driven by an external device), push received bytes into the UART RX FIFO.
- [ ] **CTS/RTS hardware flow control** (`UART.cs:138-144`): When `UARTCR.RTSEN=1`, drive the
  RTS GPIO output low when the RX FIFO has space. Gate TX when `UARTCR.CTSEN=1` and CTS
  GPIO is high (not asserted).

### 3.3 Timer Interrupt Delivery

- [ ] **Wire timer alarms to `IrqController`**: `Timer0.cs` and `Timer1.cs` have `INTR/INTE/INTS`
  registers. When an alarm fires and the corresponding `INTE` bit is set, raise the
  appropriate IRQ (0-3 for Timer0, 4-7 for Timer1).
- [ ] **Clear on INTR write**: Writing 1 to a bit in `INTR` clears that alarm interrupt.
  Confirm this also lowers the IRQ line when all armed alarms are cleared.
- [ ] **`MTIME`/`MTIMECMP` in SIO**: When `MTIME >= MTIMECMP`, set `MIP.MTIP` to trigger
  a machine timer interrupt. This is the RISC-V standard timer mechanism.

### 3.4 Watchdog

File: `Venture/Peripherals/Watchdog.cs` (new, currently `.Unimplemented()`)

- [ ] Create `Watchdog.cs` at `0x400d8000`.
- [ ] **`CTRL` register** (offset 0x00): bits include `ENABLE` (bit 30), `PAUSE_JTAG` (bit 26),
  `PAUSE_DBG0` (bit 25), `PAUSE_DBG1` (bit 24), `TRIGGER` read-only (bit 31 = timeout occurred).
  `TIME` field (bits 23:0) = current countdown value.
- [ ] **`LOAD` register** (offset 0x04): writing a 24-bit value reloads the countdown timer.
  This is the "pet the watchdog" register.
- [ ] **`REASON` register** (offset 0x08): read-only, bits 1:0 indicate TIMER reset (bit 0)
  or FORCE reset (bit 1).
- [ ] **`SCRATCH0`-`SCRATCH7`** (offsets 0x0C-0x28): 8 general-purpose scratch registers
  preserved across watchdog reset.
- [ ] **`TICK` register** (offset 0x2C): controls the tick generator — `ENABLE` bit, `CYCLES`
  field (9-bit count of clk_ref cycles per tick), `RUNNING` and `COUNT` read-only fields.
- [ ] **Timeout action**: When the countdown reaches zero, simulate a system reset by
  resetting the CPU PC to the boot vector and raising a reset event. Set `REASON.TIMER=1`.