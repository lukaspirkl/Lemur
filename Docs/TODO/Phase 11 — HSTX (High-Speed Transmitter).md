## Phase 11 — HSTX (High-Speed Transmitter)

File: `Venture/Peripherals/HSTX.cs` (new, currently `.Unimplemented()`)

This peripheral outputs high-speed differential signals for DVI/HDMI-like applications.
Minimum implementation to avoid boot hangs:

- [ ] Create at `0x400c0000` (HSTX_CTRL) and `0x50600000` (HSTX_FIFO).
- [ ] Implement register storage with no behavior. Return 0 for all reads.
- [ ] Full HSTX implementation (with video output) is a long-term stretch goal.