## Phase 9 — Boot Sequence & Reset Improvements

### 9.1 Reset Controller

File: `Resets.cs` (already implemented as stub)

- [ ] Implement `RESET` register (offset 0x00): bitfield of peripheral reset states.
  Writing a 1 holds that peripheral in reset; writing 0 releases it.
  Peripherals should check their reset state and return zeros (or default values) when held in reset.
- [ ] `RESET_DONE` (offset 0x08): bits go from 0 to 1 as peripherals come out of reset.
  Currently hardcoded to `0x1FFFFFFF`; change to track actual reset states.
- [ ] `WDSEL` (offset 0x04): which peripherals are reset by the watchdog.

### 9.2 Boot Flow

- [ ] **Halt on breakpoint EBREAK**: Currently `EBreak` is an event. Ensure the GDB handler
  uses this correctly to pause execution.
- [ ] **Bootrom correctness**: Verify the embedded `bootrom-combined.bin` correctly boots
  into RISC-V mode. If the bootrom expects peripherals like PSM, POWMAN, ROSC, etc. to
  respond correctly, stub those first.
- [ ] **Dual-core support**: Core 1 launch via the SIO FIFO mailbox protocol (FIFO handshake).
  When core 0 writes the magic sequence to the FIFO, start a second execution context
  (or at least acknowledge the boot sequence so firmware doesn't hang).