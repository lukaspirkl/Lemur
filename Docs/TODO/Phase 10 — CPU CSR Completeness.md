## Phase 10 — CPU/CSR Completeness

### 10.1 Privilege Levels

Currently no privilege enforcement. For running secure-boot firmware correctly:
- [ ] **Machine mode only** is sufficient for RISC-V emulation (RP2350 doesn't use U-mode).
- [ ] **`MSTATUS.MPP`** (bits 12:11): previous privilege level saved on trap. Always `11` (M-mode).
- [ ] **Illegal instruction trap**: If an unknown opcode is executed, raise exception with
  MCAUSE=2 (Illegal instruction) and MTVAL = the instruction encoding.
- [ ] **Load/store address misalignment**: Raise exception MCAUSE=4/6 (load/store misaligned)
  with MTVAL = faulting address.
- [ ] **Instruction address misalignment**: MCAUSE=0 if PC is not 2-byte aligned (since C ext.).

### 10.2 Hazard3 Custom CSRs

See `03_08_09 Control and Status Registers.md` for the full list.

- [ ] **`meiea` (0xBE0)**: External interrupt enable array (8 × 32-bit window into 256 IRQ enables).
- [ ] **`meipa` (0xBE1)**: External interrupt pending array.
- [ ] **`meifa` (0xBE2)**: External interrupt force array.
- [ ] **`meipra` (0xBE3)**: External interrupt priority array.
- [ ] **`meinext` (0xBE4)**: Next interrupt to service (highest priority pending+enabled IRQ).
  Read auto-gates the CPU through interrupt dispatch.
- [ ] **`meicontext` (0xBE5)**: Interrupt context (preemption level, IRQ number, etc.).
- [ ] **`msleep` (0xBE7)**: Hazard3 sleep control (WFI behavior, clock-gating hints).
- [ ] **`meiea`/`meipa` integration**: Wire `IrqController` to these CSRs — raising an IRQ
  writes to `meipa`, and the CPU checks `meinext` to determine if an interrupt should fire.
  This replaces the simpler `MIP.MEIP` mechanism for external interrupts on Hazard3.

### 10.3 Compressed Instruction Completeness

- [ ] **Audit Zcb**: Ensure `C.LBU`, `C.LH`, `C.LHU`, `C.SB`, `C.SH`, `C.ZEXT.B`,
  `C.SEXT.B`, `C.ZEXT.H`, `C.SEXT.H`, `C.ZEXT.W`, `C.NOT`, `C.MUL` are all implemented.
- [ ] **Audit Zcmp**: `CM.PUSH`, `CM.POP`, `CM.POPRET`, `CM.POPRETZ`, `CM.MVA01S`, `CM.MVSA01`.
- [ ] **Fault on unknown compressed opcodes**: Raise illegal instruction exception instead of silently ignoring.