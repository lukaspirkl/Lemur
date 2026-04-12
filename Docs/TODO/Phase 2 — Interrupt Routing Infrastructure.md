## Phase 2 — Interrupt Routing Infrastructure

### 2.1 IRQ Controller / Interrupt Aggregator

The RP2350 presents 52 IRQ lines per core to the RISC-V interrupt controller.
External interrupts go through `MEIP` (MIP bit 11) using Platform-Level Interrupt Controller
(PLIC) semantics, but on RP2350 the Hazard3 core uses a simple priority scheme: the highest
pending-and-enabled IRQ fires.

- [ ] **Create `IrqController` class**: Holds a 64-bit mask of pending IRQ lines and a
  64-bit mask of enabled IRQ lines (one set per core, but for now single-core is fine).
  Expose a method `RaiseIrq(int irqNumber)` / `ClearIrq(int irqNumber)`.
- [ ] **Connect to CPU**: `IrqController` should notify `Hazard3Processor` when the
  pending AND enabled mask is non-zero by setting `MIP.MEIP`. On IRQ acknowledgement
  (MRET + handler writing to appropriate register), clear the bit.
- [ ] **IRQ number table**: Map the 52 IRQ lines to their sources. Key entries:
  - IRQ 0: TIMER0_IRQ_0 … IRQ 3: TIMER0_IRQ_3
  - IRQ 4: TIMER1_IRQ_0 … IRQ 7: TIMER1_IRQ_3
  - IRQ 8: PWM_IRQ_WRAP_0, IRQ 9: PWM_IRQ_WRAP_1
  - IRQ 10: DMA_IRQ_0, IRQ 11: DMA_IRQ_1, IRQ 12: DMA_IRQ_2, IRQ 13: DMA_IRQ_3
  - IRQ 14: USBCTRL_IRQ
  - IRQ 15: PIO0_IRQ_0, IRQ 16: PIO0_IRQ_1
  - IRQ 17: PIO1_IRQ_0, IRQ 18: PIO1_IRQ_1
  - IRQ 19: PIO2_IRQ_0, IRQ 20: PIO2_IRQ_1
  - IRQ 21: IO_IRQ_BANK0 (GPIO bank 0, all pins), IRQ 22: IO_IRQ_BANK0_NS
  - IRQ 23: IO_IRQ_QSPI, IRQ 24: IO_IRQ_QSPI_NS
  - IRQ 25: SIO_IRQ_FIFO (core 0), IRQ 26: SIO_IRQ_FIFO (core 1)
  - IRQ 27: SIO_IRQ_BELL
  - IRQ 28: SIO_IRQ_FIFO_NS, IRQ 29: SIO_IRQ_BELL_NS
  - IRQ 30: SIO_IRQ_MTIMECMP
  - IRQ 31: SPI0_IRQ, IRQ 32: SPI1_IRQ
  - IRQ 33: UART0_IRQ, IRQ 34: UART1_IRQ
  - IRQ 35: ADC_IRQ_FIFO
  - IRQ 36: I2C0_IRQ, IRQ 37: I2C1_IRQ
  - IRQ 38: OTP_IRQ
  - IRQ 39: TRNG_IRQ
  - IRQ 40: PROC0_IRQ_CTI, IRQ 41: PROC1_IRQ_CTI
  - IRQ 42: PLL_SYS_IRQ, IRQ 43: PLL_USB_IRQ
  - IRQ 44: POWMAN_IRQ_POW, IRQ 45: POWMAN_IRQ_TIMER
  - IRQ 46-51: SPARE (hardwired 0)

### 2.2 GPIO Interrupt Delivery

- [ ] **Edge/level detection in `UserBankIO`**: For each pin, compare the current input
  value against the previous value. On a rising edge, set the EDGE_HIGH bit in the
  pin's `INTR` register. On a falling edge, set EDGE_LOW. LEVEL_HIGH and LEVEL_LOW
  are combinational (not latched).
- [ ] **PROC0/1_INTE/INTF/INTS registers**: `INTE` = software enable mask, `INTF` = force bits
  (software can force an interrupt for testing), `INTS` = final status = `(INTR | INTF) & INTE`.
- [ ] **Raise `IO_IRQ_BANK0`**: If any bit in any `PROC0_INTS` register is set, call
  `IrqController.RaiseIrq(21)`. Clear when all INTS bits are zero.
- [ ] **Edge interrupt clear**: EDGE_HIGH/EDGE_LOW bits in `INTR` are cleared by writing 1
  to them (write-1-to-clear). Level bits are not clearable (they follow pin state).

### 2.3 SIO FIFO / Doorbell Interrupts

- [ ] **Inter-processor FIFO**: Implement `FIFO_WR` / `FIFO_RD` as actual 8-entry FIFOs.
  `FIFO_ST` flags: `VLD` (RX FIFO non-empty), `RDY` (TX FIFO not full), `ROE` (RX overrun),
  `WOF` (TX overflow).
- [ ] **FIFO interrupt**: When core 0's RX FIFO becomes non-empty, raise `SIO_IRQ_FIFO`
  (IRQ 25) on core 0.
- [ ] **Doorbell registers**: `DOORBELL_OUT_SET/CLR` writes raise the doorbell interrupt
  on the other core.