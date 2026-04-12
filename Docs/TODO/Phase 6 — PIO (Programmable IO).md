## Phase 6 — PIO (Programmable IO)

PIO is the most complex peripheral. Each PIO block is essentially a small programmable processor.
Implement one block first, then replicate.

Files: `Venture/Peripherals/PIO0.cs`, `PIO1.cs`, `PIO2.cs` (new, currently `.Unimplemented()`)

### 6.1 PIO Block Structure

Each PIO block (base: PIO0=0x50200000, PIO1=0x50300000, PIO2=0x50400000):

- [ ] **Instruction memory**: 32 × 16-bit instruction words (`INSTR_MEM0`-`INSTR_MEM31`, offsets 0x48-0xC4).
- [ ] **4 state machines** per block with independent execution.

### 6.2 Per-State-Machine Registers (SM0 at 0xC8, SM1 at 0xE0, etc.)

- [ ] **`SMx_CLKDIV`**: 16.8 fractional clock divider (INT bits 31:16, FRAC bits 23:16, split weirdly — see datasheet).
- [ ] **`SMx_EXECCTRL`**: `SIDE_EN`, `SIDE_PINDIR`, `JMP_PIN`, `OUT_EN_SEL`, `INLINE_OUT_EN`,
  `OUT_STICKY`, `WRAP_TOP` (bits 16:12), `WRAP_BOTTOM` (bits 11:7), `STATUS_SEL`, `STATUS_N`.
- [ ] **`SMx_SHIFTCTRL`**: `FJOIN_RX`, `FJOIN_TX`, `PULL_THRESH`, `PUSH_THRESH`, `OUT_SHIFTDIR`,
  `IN_SHIFTDIR`, `AUTOPULL`, `AUTOPUSH`.
- [ ] **`SMx_ADDR`**: read-only current PC of state machine.
- [ ] **`SMx_INSTR`**: writing executes instruction immediately; reading gives current instruction.
- [ ] **`SMx_PINCTRL`**: `SIDESET_COUNT`, `SET_COUNT`, `OUT_COUNT`, `IN_BASE`, `SIDESET_BASE`,
  `SET_BASE`, `OUT_BASE`.
- [ ] **`FSTAT`** (0x04): TX/RX FIFO empty/full status for all 4 SMs.
- [ ] **`FDEBUG`** (0x08): TX stall/overflow, RX underflow/stall flags.
- [ ] **`FLEVEL`** (0x0C): FIFO fill levels.
- [ ] **`TXFn`** (0x10-0x1C), **`RXFn`** (0x20-0x2C): FIFO push/pop registers.
- [ ] **`IRQ`** (0x30): PIO-internal IRQ flags (8 bits, lower 4 are system-visible).
- [ ] **`IRQ_FORCE`** (0x34): force IRQ flags.
- [ ] **`INPUT_SYNC_BYPASS`** (0x38): bypass 2-stage synchronizer for specific pins.
- [ ] **`DBG_PADOUT`** / `DBG_PADOE` (0x3C/0x40): debug read of GPIO output.
- [ ] **`INTR`** (0x128), **`INTE0`** (0x12C), **`INTF0`** (0x130), **`INTS0`** (0x134),
  **`INTE1/INTF1/INTS1`** (0x138/0x13C/0x140): per-SM FIFO and IRQ interrupt routing.

### 6.3 PIO Instruction Set Execution

Each state machine has a 5-bit PC, OSR/ISR shift registers, X/Y scratch registers, 4-entry TX/RX FIFOs.

- [ ] **JMP** (opcode 0x0): conditional jump on X, Y, zero, non-zero, OSRE, pin level, scratch compare.
- [ ] **WAIT** (opcode 0x1): stall until GPIO pin, IRQ flag, or jmppin matches a level.
- [ ] **IN** (opcode 0x2): shift data into ISR from source (PINS, X, Y, NULL, ISR, OSR). Support
  autopush threshold: when ISR has enough bits, push to RX FIFO and clear ISR.
- [ ] **OUT** (opcode 0x3): shift data from OSR to destination (PINS, X, Y, NULL, PINDIRS, PC, ISR, EXEC).
  Support autopull threshold: when OSR is empty enough, pull from TX FIFO.
- [ ] **PUSH** (opcode 0x4 with bit 7=0): push ISR to RX FIFO (if IFFULL=1, only when ISR is full).
- [ ] **PULL** (opcode 0x4 with bit 7=1): pull TX FIFO to OSR.
- [ ] **MOV** (opcode 0x5): move between internal registers with optional NOT/reverse.
- [ ] **IRQ** (opcode 0x6): set or clear internal IRQ flags; WAIT bit stalls until flag clears.
- [ ] **SET** (opcode 0x7): set PINS/PINDIRS/X/Y to 5-bit immediate.
- [ ] **SIDESET**: any instruction can include side-set data (applied to side-set pins before main effect).
- [ ] **PC wrap**: when SM PC reaches `WRAP_TOP`, next PC = `WRAP_BOTTOM` (no instruction needed).
- [ ] **GPIO mapping**: map `OUT_BASE+n` etc. to physical GPIO lines using `PINCTRL` settings.
- [ ] **Clock divider**: only step the SM every N cycles (fractional via Bresenham accumulator).
- [ ] **Wire interrupts**: PIO0 → IRQ 15/16, PIO1 → IRQ 17/18, PIO2 → IRQ 19/20.
- [ ] **DREQ outputs**: PIO0 TX0-3 = DREQ 0-3, PIO0 RX0-3 = DREQ 4-7, etc.