## Phase 12 — UI Extensions

These depend on Phase 3–6 peripherals being implemented.

- [ ] **SPI ViewModel**: Show TX/RX FIFO state, last transferred bytes, baud rate, mode (CPOL/CPHA).
- [ ] **I2C ViewModel**: Show master/slave mode, last transaction address and data, ACK/NACK status.
- [ ] **PWM ViewModel**: Show per-slice frequency, duty cycle, counter value (update on wrap event).
- [ ] **ADC ViewModel**: Show per-channel voltage (mV), raw 12-bit value, FIFO level.
- [ ] **PIO ViewModel**: Show per-state-machine PC, registers (X, Y, OSR, ISR), FIFO contents,
  executing instruction decoded as pioasm mnemonic.
- [ ] **DMA ViewModel**: Show per-channel state (active/idle), read/write addresses, remaining transfer count.
- [ ] **GPIO interrupt ViewModel**: Show which pins have pending interrupt flags per bank.
- [ ] **Interrupt monitor**: Show which IRQs are currently pending and enabled, which is being serviced.