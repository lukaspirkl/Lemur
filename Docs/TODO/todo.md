# Venture RP2350 Implementation Roadmap

This is the master todo list for implementing the full RP2350 feature set in the Venture emulator.
Items are ordered by dependency — each phase builds on the previous ones.
Check off items as they are completed.


## Dependency Summary

```
Phase 1 (CSR + LR/SC + GPIO_IN)
  └─> Phase 2 (IRQ Controller + GPIO interrupts + SIO FIFO)
        └─> Phase 3 (UART irqs + Timer irqs + Watchdog)
              ├─> Phase 4 (SPI, I2C, PWM, ADC)
              │     └─> Phase 5 (DMA — needs DREQ sources)
              │           └─> Phase 6 (PIO — needs DMA + GPIO)
              └─> Phase 7 (ROSC, TRNG, TICKS, SYSINFO — mostly independent)
Phase 9 (Boot/Reset) — can be done alongside Phase 2-3
Phase 10 (CPU completeness) — can be done alongside Phase 1-2
Phase 11 (HSTX stub) — independent
Phase 12 (UI) — after corresponding peripheral phases
```


## Quick Wins (Do Anytime)

These have no prerequisites and fix existing bugs or gaps:

- [ ] `SIO.cs`: Fix `GPIO_IN` to read from `UserBankIO` (Phase 1.3 — foundational enough to do first).
- [ ] `OTPData.cs`: Replace hardcoded reads with a real byte-array-backed storage.
- [ ] `BootRAM.cs`: Implement the `BOOTRAM_BASE` register at `0x400e0800`.
- [ ] `SYSINFO`: Return real `CHIP_ID = 0x01002927`.
- [ ] `PSM`: Return `DONE = 0x00FFFFFF` to prevent boot firmware hangs on PSM polling.
- [ ] `Resets.cs`: Return 0 from `RESET_DONE` initially; set bits as peripherals are released.
