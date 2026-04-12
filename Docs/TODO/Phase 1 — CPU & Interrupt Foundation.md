## Phase 1 — CPU & Interrupt Foundation

These are prerequisites for almost everything else. Nothing can deliver interrupts until the
CPU's interrupt machinery is correct.

### 1.1 CSR Semantics

The CSR system is currently a plain dictionary with no enforced behavior.
These registers have hardware-specified behavior that firmware relies on.

- [ ] **MSTATUS**: Enforce `MIE` (bit 3) as the global interrupt enable/disable gate.
  When `MIE=0`, no external interrupts should be taken.
  When an interrupt fires, hardware clears `MIE` and saves it to `MPIE` (bit 7).
  `MRET` restores `MIE` from `MPIE`.
- [ ] **MIP**: Make bit 11 (`MEIP`) writable by peripherals to signal a pending external interrupt.
  Make bit 7 (`MTIP`) writable by the timer system to signal a timer interrupt.
  Make bit 3 (`MSIP`) writable for software interrupts.
- [ ] **MIE**: Enforce bit 11 (`MEIE`) as the external interrupt enable gate, bit 7 (`MTIE`)
  for timer, bit 3 (`MSIE`) for software. Only allow interrupt delivery when the
  corresponding `MIE` bit is set AND `MSTATUS.MIE` is set.
- [ ] **MTVEC**: Support both direct mode (bits 1:0 = 0) and vectored mode (bits 1:0 = 1).
  In vectored mode, jump to `MTVEC + 4 * cause` for interrupts (not exceptions).
- [ ] **MEPC / MCAUSE / MTVAL**: These exist but `MTVAL` is hardcoded to 0
  (see `Hazard3Processor.cs:74`). Populate `MTVAL` properly for load/store faults
  (faulting address) and illegal instruction faults (instruction encoding).
- [ ] **MCYCLE / MINSTRET**: Implement as real 64-bit counters (two 32-bit CSRs each).
  Increment `MINSTRET` on each retired instruction, `MCYCLE` each step.
- [ ] **MSCRATCH**: No-op storage register — confirm it works as plain read/write.
- [ ] **Interrupt check in Step()**: After each instruction, check if an interrupt should
  be taken: `MSTATUS.MIE && (MIP & MIE) != 0`. If so, save PC to MEPC, write MCAUSE
  (bit 31 set = interrupt, bits 30:0 = interrupt number), clear `MSTATUS.MIE`, jump to
  MTVEC (or MTVEC+4*cause in vectored mode).

### 1.2 Atomic Instructions

- [ ] **LR.W / SC.W**: Implement in `AExtensionFormat.cs` (see TODO at line 102).
  `LR.W` loads from memory and sets a reservation on that address.
  `SC.W` conditionally stores only if the reservation is still valid; writes 0 to `rd`
  on success, 1 on failure. Reservation must be cleared by any SC.W or exception.
  This is required for spinlocks (the RP2350 bootrom uses these heavily).

### 1.3 GPIO Input Path (SIO reads from pads)

Currently `SIO.cs:21` has a TODO: `GPIO_IN` always returns a fixed value.

- [ ] **Wire `GPIO_IN` to `UserBankIO`**: When firmware reads `SIO.GPIO_IN` or `SIO.GPIO_HI_IN`,
  the SIO should query the current input value of each `MuxedGpioLine` in `UserBankIO`
  rather than returning zeros. Requires `UserBankIO` to expose a method to read current
  input values per pin.
- [ ] **Pad input override (`INOVER`)**: Apply the `INOVER` field from each pin's `GpioControl`
  before returning the value to SIO or the function peripheral. `INOVER=0` = normal,
  `INOVER=1` = inverted, `INOVER=2` = force 0, `INOVER=3` = force 1.
