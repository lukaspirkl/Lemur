# 11.7 List of Registers

The PIO0 and PIO1 registers start at base addresses of 0x50200000 and 0x50300000 respectively (defined as [PIO0\\_BASE](#page-33-0) and [PIO1\\_BASE](#page-33-0) in SDK).

*Table 980. List of PIO registers*

<span id="page-936-2"></span>

| Offset | Name   | Info                                                                                                                                                                                                                                                    |
|--------|--------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x000  | CTRL   | PIO control register                                                                                                                                                                                                                                    |
| 0x004  | FSTAT  | FIFO status register                                                                                                                                                                                                                                    |
| 0x008  | FDEBUG | FIFO debug register                                                                                                                                                                                                                                     |
| 0x00c  | FLEVEL | FIFO levels                                                                                                                                                                                                                                             |
| 0x010  | TXF0   | Direct write access to the TX FIFO for this state machine. Each<br>write pushes one word to the FIFO. Attempting to write to a full<br>FIFO has no effect on the FIFO state or contents, and sets the<br>sticky FDEBUG_TXOVER error flag for this FIFO. |
| 0x014  | TXF1   | Direct write access to the TX FIFO for this state machine. Each<br>write pushes one word to the FIFO. Attempting to write to a full<br>FIFO has no effect on the FIFO state or contents, and sets the<br>sticky FDEBUG_TXOVER error flag for this FIFO. |
| 0x018  | TXF2   | Direct write access to the TX FIFO for this state machine. Each<br>write pushes one word to the FIFO. Attempting to write to a full<br>FIFO has no effect on the FIFO state or contents, and sets the<br>sticky FDEBUG_TXOVER error flag for this FIFO. |
| 0x01c  | TXF3   | Direct write access to the TX FIFO for this state machine. Each<br>write pushes one word to the FIFO. Attempting to write to a full<br>FIFO has no effect on the FIFO state or contents, and sets the<br>sticky FDEBUG_TXOVER error flag for this FIFO. |

| Offset | Name              | Info                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
|--------|-------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x020  | RXF0              | Direct read access to the RX FIFO for this state machine. Each<br>read pops one word from the FIFO. Attempting to read from an<br>empty FIFO has no effect on the FIFO state, and sets the sticky<br>FDEBUG_RXUNDER error flag for this FIFO. The data returned to<br>the system on a read from an empty FIFO is undefined.                                                                                                                                                                                                                     |
| 0x024  | RXF1              | Direct read access to the RX FIFO for this state machine. Each<br>read pops one word from the FIFO. Attempting to read from an<br>empty FIFO has no effect on the FIFO state, and sets the sticky<br>FDEBUG_RXUNDER error flag for this FIFO. The data returned to<br>the system on a read from an empty FIFO is undefined.                                                                                                                                                                                                                     |
| 0x028  | RXF2              | Direct read access to the RX FIFO for this state machine. Each<br>read pops one word from the FIFO. Attempting to read from an<br>empty FIFO has no effect on the FIFO state, and sets the sticky<br>FDEBUG_RXUNDER error flag for this FIFO. The data returned to<br>the system on a read from an empty FIFO is undefined.                                                                                                                                                                                                                     |
| 0x02c  | RXF3              | Direct read access to the RX FIFO for this state machine. Each<br>read pops one word from the FIFO. Attempting to read from an<br>empty FIFO has no effect on the FIFO state, and sets the sticky<br>FDEBUG_RXUNDER error flag for this FIFO. The data returned to<br>the system on a read from an empty FIFO is undefined.                                                                                                                                                                                                                     |
| 0x030  | IRQ               | State machine IRQ flags register. Write 1 to clear. There are eight<br>state machine IRQ flags, which can be set, cleared, and waited on<br>by the state machines. There's no fixed association between<br>flags and state machines — any state machine can use any flag.<br>Any of the eight flags can be used for timing synchronisation<br>between state machines, using IRQ and WAIT instructions. Any<br>combination of the eight flags can also routed out to either of the<br>two system-level interrupt requests, alongside FIFO status |
| 0x034  | IRQ_FORCE         | interrupts — see e.g. IRQ0_INTE.<br>Writing a 1 to each of these bits will forcibly assert the<br>corresponding IRQ. Note this is different to the INTF register:<br>writing here affects PIO internal state. INTF just asserts the<br>processor-facing IRQ signal for testing ISRs, and is not visible to<br>the state machines.                                                                                                                                                                                                               |
| 0x038  | INPUT_SYNC_BYPASS | There is a 2-flipflop synchronizer on each GPIO input, which<br>protects PIO logic from metastabilities. This increases input<br>delay, and for fast synchronous IO (e.g. SPI) these synchronizers<br>may need to be bypassed. Each bit in this register corresponds<br>to one GPIO.<br>0 → input is synchronized (default)<br>1 → synchronizer is bypassed<br>If in doubt, leave this register as all zeroes.                                                                                                                                  |
| 0x03c  | DBG_PADOUT        | Read to sample the pad output values PIO is currently driving to<br>the GPIOs. On RP2040 there are 30 GPIOs, so the two most<br>significant bits are hardwired to 0.                                                                                                                                                                                                                                                                                                                                                                            |
| 0x040  | DBG_PADOE         | Read to sample the pad output enables (direction) PIO is<br>currently driving to the GPIOs. On RP2040 there are 30 GPIOs, so<br>the two most significant bits are hardwired to 0.                                                                                                                                                                                                                                                                                                                                                               |

| Offset | Name        | Info                                                                                                                                                               |
|--------|-------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x044  | DBG_CFGINFO | The PIO hardware has some free parameters that may vary<br>between chip products.<br>These should be provided in the chip datasheet, but are also<br>exposed here. |
| 0x048  | INSTR_MEM0  | Write-only access to instruction memory location 0                                                                                                                 |
| 0x04c  | INSTR_MEM1  | Write-only access to instruction memory location 1                                                                                                                 |
| 0x050  | INSTR_MEM2  | Write-only access to instruction memory location 2                                                                                                                 |
| 0x054  | INSTR_MEM3  | Write-only access to instruction memory location 3                                                                                                                 |
| 0x058  | INSTR_MEM4  | Write-only access to instruction memory location 4                                                                                                                 |
| 0x05c  | INSTR_MEM5  | Write-only access to instruction memory location 5                                                                                                                 |
| 0x060  | INSTR_MEM6  | Write-only access to instruction memory location 6                                                                                                                 |
| 0x064  | INSTR_MEM7  | Write-only access to instruction memory location 7                                                                                                                 |
| 0x068  | INSTR_MEM8  | Write-only access to instruction memory location 8                                                                                                                 |
| 0x06c  | INSTR_MEM9  | Write-only access to instruction memory location 9                                                                                                                 |
| 0x070  | INSTR_MEM10 | Write-only access to instruction memory location 10                                                                                                                |
| 0x074  | INSTR_MEM11 | Write-only access to instruction memory location 11                                                                                                                |
| 0x078  | INSTR_MEM12 | Write-only access to instruction memory location 12                                                                                                                |
| 0x07c  | INSTR_MEM13 | Write-only access to instruction memory location 13                                                                                                                |
| 0x080  | INSTR_MEM14 | Write-only access to instruction memory location 14                                                                                                                |
| 0x084  | INSTR_MEM15 | Write-only access to instruction memory location 15                                                                                                                |
| 0x088  | INSTR_MEM16 | Write-only access to instruction memory location 16                                                                                                                |
| 0x08c  | INSTR_MEM17 | Write-only access to instruction memory location 17                                                                                                                |
| 0x090  | INSTR_MEM18 | Write-only access to instruction memory location 18                                                                                                                |
| 0x094  | INSTR_MEM19 | Write-only access to instruction memory location 19                                                                                                                |
| 0x098  | INSTR_MEM20 | Write-only access to instruction memory location 20                                                                                                                |
| 0x09c  | INSTR_MEM21 | Write-only access to instruction memory location 21                                                                                                                |
| 0x0a0  | INSTR_MEM22 | Write-only access to instruction memory location 22                                                                                                                |
| 0x0a4  | INSTR_MEM23 | Write-only access to instruction memory location 23                                                                                                                |
| 0x0a8  | INSTR_MEM24 | Write-only access to instruction memory location 24                                                                                                                |
| 0x0ac  | INSTR_MEM25 | Write-only access to instruction memory location 25                                                                                                                |
| 0x0b0  | INSTR_MEM26 | Write-only access to instruction memory location 26                                                                                                                |
| 0x0b4  | INSTR_MEM27 | Write-only access to instruction memory location 27                                                                                                                |
| 0x0b8  | INSTR_MEM28 | Write-only access to instruction memory location 28                                                                                                                |
| 0x0bc  | INSTR_MEM29 | Write-only access to instruction memory location 29                                                                                                                |
| 0x0c0  | INSTR_MEM30 | Write-only access to instruction memory location 30                                                                                                                |
| 0x0c4  | INSTR_MEM31 | Write-only access to instruction memory location 31                                                                                                                |

| Offset | Name          | Info                                                                                                                                                                                   |
|--------|---------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x0c8  | SM0_CLKDIV    | Clock divisor register for state machine 0<br>Frequency = clock freq / (CLKDIV_INT + CLKDIV_FRAC / 256)                                                                                |
| 0x0cc  | SM0_EXECCTRL  | Execution/behavioural settings for state machine 0                                                                                                                                     |
| 0x0d0  | SM0_SHIFTCTRL | Control behaviour of the input/output shift registers for state<br>machine 0                                                                                                           |
| 0x0d4  | SM0_ADDR      | Current instruction address of state machine 0                                                                                                                                         |
| 0x0d8  | SM0_INSTR     | Read to see the instruction currently addressed by state machine<br>0's program counter<br>Write to execute an instruction immediately (including jumps)<br>and then resume execution. |
| 0x0dc  | SM0_PINCTRL   | State machine pin control                                                                                                                                                              |
| 0x0e0  | SM1_CLKDIV    | Clock divisor register for state machine 1<br>Frequency = clock freq / (CLKDIV_INT + CLKDIV_FRAC / 256)                                                                                |
| 0x0e4  | SM1_EXECCTRL  | Execution/behavioural settings for state machine 1                                                                                                                                     |
| 0x0e8  | SM1_SHIFTCTRL | Control behaviour of the input/output shift registers for state<br>machine 1                                                                                                           |
| 0x0ec  | SM1_ADDR      | Current instruction address of state machine 1                                                                                                                                         |
| 0x0f0  | SM1_INSTR     | Read to see the instruction currently addressed by state machine<br>1's program counter<br>Write to execute an instruction immediately (including jumps)<br>and then resume execution. |
| 0x0f4  | SM1_PINCTRL   | State machine pin control                                                                                                                                                              |
| 0x0f8  | SM2_CLKDIV    | Clock divisor register for state machine 2<br>Frequency = clock freq / (CLKDIV_INT + CLKDIV_FRAC / 256)                                                                                |
| 0x0fc  | SM2_EXECCTRL  | Execution/behavioural settings for state machine 2                                                                                                                                     |
| 0x100  | SM2_SHIFTCTRL | Control behaviour of the input/output shift registers for state<br>machine 2                                                                                                           |
| 0x104  | SM2_ADDR      | Current instruction address of state machine 2                                                                                                                                         |
| 0x108  | SM2_INSTR     | Read to see the instruction currently addressed by state machine<br>2's program counter<br>Write to execute an instruction immediately (including jumps)<br>and then resume execution. |
| 0x10c  | SM2_PINCTRL   | State machine pin control                                                                                                                                                              |
| 0x110  | SM3_CLKDIV    | Clock divisor register for state machine 3<br>Frequency = clock freq / (CLKDIV_INT + CLKDIV_FRAC / 256)                                                                                |
| 0x114  | SM3_EXECCTRL  | Execution/behavioural settings for state machine 3                                                                                                                                     |
| 0x118  | SM3_SHIFTCTRL | Control behaviour of the input/output shift registers for state<br>machine 3                                                                                                           |
| 0x11c  | SM3_ADDR      | Current instruction address of state machine 3                                                                                                                                         |
| 0x120  | SM3_INSTR     | Read to see the instruction currently addressed by state machine<br>3's program counter<br>Write to execute an instruction immediately (including jumps)<br>and then resume execution. |

| Offset | Name         | Info                                                                                                                     |
|--------|--------------|--------------------------------------------------------------------------------------------------------------------------|
| 0x124  | SM3_PINCTRL  | State machine pin control                                                                                                |
| 0x128  | RXF0_PUTGET0 | Direct read/write access to entry 0 of SM0's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x12c  | RXF0_PUTGET1 | Direct read/write access to entry 1 of SM0's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x130  | RXF0_PUTGET2 | Direct read/write access to entry 2 of SM0's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x134  | RXF0_PUTGET3 | Direct read/write access to entry 3 of SM0's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x138  | RXF1_PUTGET0 | Direct read/write access to entry 0 of SM1's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x13c  | RXF1_PUTGET1 | Direct read/write access to entry 1 of SM1's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x140  | RXF1_PUTGET2 | Direct read/write access to entry 2 of SM1's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x144  | RXF1_PUTGET3 | Direct read/write access to entry 3 of SM1's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x148  | RXF2_PUTGET0 | Direct read/write access to entry 0 of SM2's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x14c  | RXF2_PUTGET1 | Direct read/write access to entry 1 of SM2's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x150  | RXF2_PUTGET2 | Direct read/write access to entry 2 of SM2's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x154  | RXF2_PUTGET3 | Direct read/write access to entry 3 of SM2's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x158  | RXF3_PUTGET0 | Direct read/write access to entry 0 of SM3's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x15c  | RXF3_PUTGET1 | Direct read/write access to entry 1 of SM3's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x160  | RXF3_PUTGET2 | Direct read/write access to entry 2 of SM3's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |

| Offset | Name         | Info                                                                                                                     |
|--------|--------------|--------------------------------------------------------------------------------------------------------------------------|
| 0x164  | RXF3_PUTGET3 | Direct read/write access to entry 3 of SM3's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is<br>set. |
| 0x168  | GPIOBASE     | Relocate GPIO 0 (from PIO's point of view) in the system GPIO<br>numbering, to access more than 32 GPIOs from PIO.       |
|        |              | Only the values 0 and 16 are supported (only bit 4 is writable).                                                         |
| 0x16c  | INTR         | Raw Interrupts                                                                                                           |
| 0x170  | IRQ0_INTE    | Interrupt Enable for irq0                                                                                                |
| 0x174  | IRQ0_INTF    | Interrupt Force for irq0                                                                                                 |
| 0x178  | IRQ0_INTS    | Interrupt status after masking & forcing for irq0                                                                        |
| 0x17c  | IRQ1_INTE    | Interrupt Enable for irq1                                                                                                |
| 0x180  | IRQ1_INTF    | Interrupt Force for irq1                                                                                                 |
| 0x184  | IRQ1_INTS    | Interrupt status after masking & forcing for irq1                                                                        |

# <span id="page-941-0"></span>**[PIO:](#page-936-2) CTRL Register**

**Offset**: 0x000 **Description**

PIO control register

*Table 981. CTRL Register*

| Bits  | Description                                                                                                                                                                           | Type | Reset |
|-------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                                                                                                                             | -    | -     |
| 26    | NEXTPREV_CLKDIV_RESTART: Write 1 to restart the clock dividers of state<br>machines in neighbouring PIO blocks, as specified by NEXT_PIO_MASK and<br>PREV_PIO_MASK in the same write. | SC   | 0x0   |
|       | This is equivalent to writing 1 to the corresponding CLKDIV_RESTART bits in<br>those PIOs' CTRL registers.                                                                            |      |       |
| 25    | NEXTPREV_SM_DISABLE: Write 1 to disable state machines in neighbouring<br>PIO blocks, as specified by NEXT_PIO_MASK and PREV_PIO_MASK in the<br>same write.                           | SC   | 0x0   |
|       | This is equivalent to clearing the corresponding SM_ENABLE bits in those<br>PIOs' CTRL registers.                                                                                     |      |       |
| 24    | NEXTPREV_SM_ENABLE: Write 1 to enable state machines in neighbouring<br>PIO blocks, as specified by NEXT_PIO_MASK and PREV_PIO_MASK in the<br>same write.                             | SC   | 0x0   |
|       | This is equivalent to setting the corresponding SM_ENABLE bits in those PIOs'<br>CTRL registers.                                                                                      |      |       |
|       | If both OTHERS_SM_ENABLE and OTHERS_SM_DISABLE are set, the disable<br>takes precedence.                                                                                              |      |       |

| Bits  | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Type | Reset |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 23:20 | NEXT_PIO_MASK: A mask of state machines in the neighbouring higher<br>numbered PIO block in the system (or PIO block 0 if this is the highest<br>numbered PIO block) to which to apply the operations specified by<br>NEXTPREV_CLKDIV_RESTART, NEXTPREV_SM_ENABLE, and<br>NEXTPREV_SM_DISABLE in the same write.<br>This allows state machines in a neighbouring PIO block to be<br>started/stopped/clock-synced exactly simultaneously with a write to this PIO<br>block's CTRL register.<br>Note that in a system with two PIOs, NEXT_PIO_MASK and PREV_PIO_MASK<br>actually indicate the same PIO block. In this case the effects are applied<br>cumulatively (as though the masks were OR'd together).<br>Neighbouring PIO blocks are disconnected (status signals tied to 0 and<br>control signals ignored) if one block is accessible to NonSecure code, and one<br>is not. | SC   | 0x0   |
| 19:16 | PREV_PIO_MASK: A mask of state machines in the neighbouring lower<br>numbered PIO block in the system (or the highest-numbered PIO block if this<br>is PIO block 0) to which to apply the operations specified by<br>OP_CLKDIV_RESTART, OP_ENABLE, OP_DISABLE in the same write.<br>This allows state machines in a neighbouring PIO block to be<br>started/stopped/clock-synced exactly simultaneously with a write to this PIO<br>block's CTRL register.<br>Neighbouring PIO blocks are disconnected (status signals tied to 0 and<br>control signals ignored) if one block is accessible to NonSecure code, and one<br>is not.                                                                                                                                                                                                                                                 | SC   | 0x0   |
| 15:12 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | -    | -     |
| 11:8  | CLKDIV_RESTART: Restart a state machine's clock divider from an initial<br>phase of 0. Clock dividers are free-running, so once started, their output<br>(including fractional jitter) is completely determined by the integer/fractional<br>divisor configured in SMx_CLKDIV. This means that, if multiple clock dividers<br>with the same divisor are restarted simultaneously, by writing multiple 1 bits to<br>this field, the execution clocks of those state machines will run in precise<br>lockstep.<br>Note that setting/clearing SM_ENABLE does not stop the clock divider from<br>running, so once multiple state machines' clocks are synchronised, it is safe to                                                                                                                                                                                                     | SC   | 0x0   |
|       | disable/reenable a state machine, whilst keeping the clock dividers in sync.<br>Note also that CLKDIV_RESTART can be written to whilst the state machine is<br>running, and this is useful to resynchronise clock dividers after the divisors                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |      |       |
|       | (SMx_CLKDIV) have been changed on-the-fly.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |      |       |

| Bits | Description                                                                                                                                                                                                                                                                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 7:4  | SM_RESTART: Write 1 to instantly clear internal SM state which may be<br>otherwise difficult to access and will affect future execution.                                                                                                                                                                                                                                         | SC   | 0x0   |
|      | Specifically, the following are cleared: input and output shift counters; the<br>contents of the input shift register; the delay counter; the waiting-on-IRQ state;<br>any stalled instruction written to SMx_INSTR or run by OUT/MOV EXEC; any<br>pin write left asserted due to OUT_STICKY.<br>The contents of the output shift register and the X/Y scratch registers are not |      |       |
|      | affected.                                                                                                                                                                                                                                                                                                                                                                        |      |       |
| 3:0  | SM_ENABLE: Enable/disable each of the four state machines by writing 1/0 to<br>each of these four bits. When disabled, a state machine will cease executing<br>instructions, except those written directly to SMx_INSTR by the system.<br>Multiple bits can be set/cleared at once to run/halt multiple state machines<br>simultaneously.                                        | RW   | 0x0   |

# <span id="page-943-0"></span>**[PIO:](#page-936-2) FSTAT Register**

**Offset**: 0x004

#### **Description**

FIFO status register

*Table 982. FSTAT Register*

| Bits  | Description                             | Type | Reset |
|-------|-----------------------------------------|------|-------|
| 31:28 | Reserved.                               | -    | -     |
| 27:24 | TXEMPTY: State machine TX FIFO is empty | RO   | 0xf   |
| 23:20 | Reserved.                               | -    | -     |
| 19:16 | TXFULL: State machine TX FIFO is full   | RO   | 0x0   |
| 15:12 | Reserved.                               | -    | -     |
| 11:8  | RXEMPTY: State machine RX FIFO is empty | RO   | 0xf   |
| 7:4   | Reserved.                               | -    | -     |
| 3:0   | RXFULL: State machine RX FIFO is full   | RO   | 0x0   |

## <span id="page-943-1"></span>**[PIO:](#page-936-2) FDEBUG Register**

**Offset**: 0x008

#### **Description**

FIFO debug register

*Table 983. FDEBUG Register*

| Bits  | Description                                                                                                                       | Type | Reset |
|-------|-----------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:28 | Reserved.                                                                                                                         | -    | -     |
| 27:24 | TXSTALL: State machine has stalled on empty TX FIFO during a blocking<br>PULL, or an OUT with autopull enabled. Write 1 to clear. | WC   | 0x0   |
| 23:20 | Reserved.                                                                                                                         | -    | -     |

| Bits  | Description                                                                                                                                                                                                                                                                                                                                     | Type | Reset |
|-------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 19:16 | TXOVER: TX FIFO overflow (i.e. write-on-full by the system) has occurred.<br>Write 1 to clear. Note that write-on-full does not alter the state or contents of<br>the FIFO in any way, but the data that the system attempted to write is<br>dropped, so if this flag is set, your software has quite likely dropped some data<br>on the floor. | WC   | 0x0   |
| 15:12 | Reserved.                                                                                                                                                                                                                                                                                                                                       | -    | -     |
| 11:8  | RXUNDER: RX FIFO underflow (i.e. read-on-empty by the system) has<br>occurred. Write 1 to clear. Note that read-on-empty does not perturb the state<br>of the FIFO in any way, but the data returned by reading from an empty FIFO is<br>undefined, so this flag generally only becomes set due to some kind of<br>software error.              | WC   | 0x0   |
| 7:4   | Reserved.                                                                                                                                                                                                                                                                                                                                       | -    | -     |
| 3:0   | RXSTALL: State machine has stalled on full RX FIFO during a blocking PUSH,<br>or an IN with autopush enabled. This flag is also set when a nonblocking<br>PUSH to a full FIFO took place, in which case the state machine has dropped<br>data. Write 1 to clear.                                                                                | WC   | 0x0   |

# <span id="page-944-0"></span>**[PIO:](#page-936-2) FLEVEL Register**

**Offset**: 0x00c

#### **Description**

FIFO levels

*Table 984. FLEVEL Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:28 | RX3         | RO   | 0x0   |
| 27:24 | TX3         | RO   | 0x0   |
| 23:20 | RX2         | RO   | 0x0   |
| 19:16 | TX2         | RO   | 0x0   |
| 15:12 | RX1         | RO   | 0x0   |
| 11:8  | TX1         | RO   | 0x0   |
| 7:4   | RX0         | RO   | 0x0   |
| 3:0   | TX0         | RO   | 0x0   |

#### <span id="page-944-1"></span>**[PIO:](#page-936-2) TXF0, TXF1, TXF2, TXF3 Registers**

**Offsets**: 0x010, 0x014, 0x018, 0x01c

*Table 985. TXF0, TXF1, TXF2, TXF3 Registers*

| Bits | Description                                                                                                                                                                                                                                             | Type | Reset      |
|------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct write access to the TX FIFO for this state machine. Each write pushes<br>one word to the FIFO. Attempting to write to a full FIFO has no effect on the<br>FIFO state or contents, and sets the sticky FDEBUG_TXOVER error flag for this<br>FIFO. | WF   | 0x00000000 |

#### <span id="page-944-2"></span>**[PIO:](#page-936-2) RXF0, RXF1, RXF2, RXF3 Registers**

**Offsets**: 0x020, 0x024, 0x028, 0x02c

*Table 986. RXF0, RXF1, RXF2, RXF3 Registers*

| Bits | Description                                                                                                                                                                                                                                                                                                              | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:0 | Direct read access to the RX FIFO for this state machine. Each read pops one<br>word from the FIFO. Attempting to read from an empty FIFO has no effect on<br>the FIFO state, and sets the sticky FDEBUG_RXUNDER error flag for this FIFO.<br>The data returned to the system on a read from an empty FIFO is undefined. | RF   | -     |

# <span id="page-945-0"></span>**[PIO:](#page-936-2) IRQ Register**

**Offset**: 0x030

*Table 987. IRQ Register*

| Bits | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | -    | -     |
| 7:0  | State machine IRQ flags register. Write 1 to clear. There are eight state<br>machine IRQ flags, which can be set, cleared, and waited on by the state<br>machines. There's no fixed association between flags and state<br>machines — any state machine can use any flag.<br>Any of the eight flags can be used for timing synchronisation between state<br>machines, using IRQ and WAIT instructions. Any combination of the eight<br>flags can also routed out to either of the two system-level interrupt requests,<br>alongside FIFO status interrupts — see e.g. IRQ0_INTE. | WC   | 0x00  |

# <span id="page-945-1"></span>**[PIO:](#page-936-2) IRQ\_FORCE Register**

**Offset**: 0x034

*Table 988. IRQ\_FORCE Register*

| Bits | Description                                                                                                                                                                                                                                                                                | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                                                                                                                                                                                  | -    | -     |
| 7:0  | Writing a 1 to each of these bits will forcibly assert the corresponding IRQ.<br>Note this is different to the INTF register: writing here affects PIO internal<br>state. INTF just asserts the processor-facing IRQ signal for testing ISRs, and is<br>not visible to the state machines. | WF   | 0x00  |

#### <span id="page-945-2"></span>**[PIO:](#page-936-2) INPUT\_SYNC\_BYPASS Register**

**Offset**: 0x038

*Table 989. INPUT\_SYNC\_BYPASS Register*

| Bits | Description                                                                                                                                                                                                                                                                                                                                                                                                 | Type | Reset      |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | There is a 2-flipflop synchronizer on each GPIO input, which protects PIO logic<br>from metastabilities. This increases input delay, and for fast synchronous IO<br>(e.g. SPI) these synchronizers may need to be bypassed. Each bit in this<br>register corresponds to one GPIO.<br>0 → input is synchronized (default)<br>1 → synchronizer is bypassed<br>If in doubt, leave this register as all zeroes. | RW   | 0x00000000 |

#### <span id="page-945-3"></span>**[PIO:](#page-936-2) DBG\_PADOUT Register**

**Offset**: 0x03c

*Table 990. DBG\_PADOUT Register*

| Bits | Description                                                                                                                                                          | Type | Reset      |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Read to sample the pad output values PIO is currently driving to the GPIOs. On<br>RP2040 there are 30 GPIOs, so the two most significant bits are hardwired to<br>0. | RO   | 0x00000000 |

# <span id="page-946-1"></span>**[PIO:](#page-936-2) DBG\_PADOE Register**

**Offset**: 0x040

*Table 991. DBG\_PADOE Register*

| Bits | Description                                                                                                                                                                       | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Read to sample the pad output enables (direction) PIO is currently driving to<br>the GPIOs. On RP2040 there are 30 GPIOs, so the two most significant bits are<br>hardwired to 0. | RO   | 0x00000000 |

# <span id="page-946-0"></span>**[PIO:](#page-936-2) DBG\_CFGINFO Register**

**Offset**: 0x044

**Description**

The PIO hardware has some free parameters that may vary between chip products. These should be provided in the chip datasheet, but are also exposed here.

*Table 992. DBG\_CFGINFO Register*

| Bits  | Description                                                                                                                                                  | Type | Reset |
|-------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:28 | VERSION: Version of the core PIO hardware.                                                                                                                   | RO   | 0x1   |
|       | Enumerated values:                                                                                                                                           |      |       |
|       | 0x0 → V0: Version 0 (RP2040)                                                                                                                                 |      |       |
|       | 0x1 → V1: Version 1 (RP2350)                                                                                                                                 |      |       |
| 27:22 | Reserved.                                                                                                                                                    | -    | -     |
| 21:16 | IMEM_SIZE: The size of the instruction memory, measured in units of one<br>instruction                                                                       | RO   | -     |
| 15:12 | Reserved.                                                                                                                                                    | -    | -     |
| 11:8  | SM_COUNT: The number of state machines this PIO instance is equipped<br>with.                                                                                | RO   | -     |
| 7:6   | Reserved.                                                                                                                                                    | -    | -     |
| 5:0   | FIFO_DEPTH: The depth of the state machine TX/RX FIFOs, measured in<br>words.<br>Joining fifos via SHIFTCTRL_FJOIN gives one FIFO with double<br>this depth. | RO   | -     |

## <span id="page-946-2"></span>**[PIO:](#page-936-2) INSTR\_MEM0, INSTR\_MEM1, …, INSTR\_MEM30, INSTR\_MEM31 Registers**

**Offsets**: 0x048, 0x04c, …, 0x0c0, 0x0c4

*Table 993. INSTR\_MEM0, INSTR\_MEM1, …, INSTR\_MEM30, INSTR\_MEM31 Registers*

| Bits  | Description                                        | Type | Reset  |
|-------|----------------------------------------------------|------|--------|
| 31:16 | Reserved.                                          | -    | -      |
| 15:0  | Write-only access to instruction memory location N | WO   | 0x0000 |

# <span id="page-947-0"></span>**[PIO:](#page-936-2) SM0\_CLKDIV, SM1\_CLKDIV, SM2\_CLKDIV, SM3\_CLKDIV Registers**

**Offsets**: 0x0c8, 0x0e0, 0x0f8, 0x110

#### **Description**

Clock divisor register for state machine *N*

Frequency = clock freq / (CLKDIV\_INT + CLKDIV\_FRAC / 256)

*Table 994. SM0\_CLKDIV, SM1\_CLKDIV, SM2\_CLKDIV, SM3\_CLKDIV Registers*

| Bits  | Description                                                                                                                   | Type | Reset  |
|-------|-------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | INT: Effective frequency is sysclk/(int + frac/256).<br>Value of 0 is interpreted as 65536. If INT is 0, FRAC must also be 0. | RW   | 0x0001 |
| 15:8  | FRAC: Fractional part of clock divisor                                                                                        | RW   | 0x00   |
| 7:0   | Reserved.                                                                                                                     | -    | -      |

# <span id="page-947-1"></span>**[PIO:](#page-936-2) SM0\_EXECCTRL, SM1\_EXECCTRL, SM2\_EXECCTRL, SM3\_EXECCTRL Registers**

**Offsets**: 0x0cc, 0x0e4, 0x0fc, 0x114

#### **Description**

Execution/behavioural settings for state machine *N*

*Table 995. SM0\_EXECCTRL, SM1\_EXECCTRL, SM2\_EXECCTRL, SM3\_EXECCTRL Registers*

| Bits  | Description                                                                                                                                                                                                                                                                                                                                                                      | Type | Reset |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31    | EXEC_STALLED: If 1, an instruction written to SMx_INSTR is stalled, and<br>latched by the state machine. Will clear to 0 once this instruction completes.                                                                                                                                                                                                                        | RO   | 0x0   |
| 30    | SIDE_EN: If 1, the MSB of the Delay/Side-set instruction field is used as side<br>set enable, rather than a side-set data bit. This allows instructions to perform<br>side-set optionally, rather than on every instruction, but the maximum possible<br>side-set width is reduced from 5 to 4. Note that the value of<br>PINCTRL_SIDESET_COUNT is inclusive of this enable bit. | RW   | 0x0   |
| 29    | SIDE_PINDIR: If 1, side-set data is asserted to pin directions, instead of pin<br>values                                                                                                                                                                                                                                                                                         | RW   | 0x0   |
| 28:24 | JMP_PIN: The GPIO number to use as condition for JMP PIN. Unaffected by<br>input mapping.                                                                                                                                                                                                                                                                                        | RW   | 0x00  |
| 23:19 | OUT_EN_SEL: Which data bit to use for inline OUT enable                                                                                                                                                                                                                                                                                                                          | RW   | 0x00  |
| 18    | INLINE_OUT_EN: If 1, use a bit of OUT data as an auxiliary write enable<br>When used in conjunction with OUT_STICKY, writes with an enable of 0 will<br>deassert the latest pin write. This can create useful masking/override<br>behaviour<br>due to the priority ordering of state machine pin writes (SM0 < SM1 < …)                                                          | RW   | 0x0   |
| 17    | OUT_STICKY: Continuously assert the most recent OUT/SET to the pins                                                                                                                                                                                                                                                                                                              | RW   | 0x0   |
| 16:12 | WRAP_TOP: After reaching this address, execution is wrapped to<br>wrap_bottom.<br>If the instruction is a jump, and the jump condition is true, the jump takes<br>priority.                                                                                                                                                                                                      | RW   | 0x1f  |

| Bits | Description                                                                                                                                     | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 11:7 | WRAP_BOTTOM: After reaching wrap_top, execution is wrapped to this<br>address.                                                                  | RW   | 0x00  |
| 6:5  | STATUS_SEL: Comparison used for the MOV x, STATUS instruction.                                                                                  | RW   | 0x0   |
|      | Enumerated values:                                                                                                                              |      |       |
|      | 0x0 → TXLEVEL: All-ones if TX FIFO level < N, otherwise all-zeroes                                                                              |      |       |
|      | 0x1 → RXLEVEL: All-ones if RX FIFO level < N, otherwise all-zeroes                                                                              |      |       |
|      | 0x2 → IRQ: All-ones if the indexed IRQ flag is raised, otherwise all-zeroes                                                                     |      |       |
| 4:0  | STATUS_N: Comparison level or IRQ index for the MOV x, STATUS instruction.                                                                      | RW   | 0x00  |
|      | If STATUS_SEL is TXLEVEL or RXLEVEL, then values of STATUS_N greater<br>than the current FIFO depth are reserved, and have undefined behaviour. |      |       |
|      | Enumerated values:                                                                                                                              |      |       |
|      | 0x00 → IRQ: Index 0-7 of an IRQ flag in this PIO block                                                                                          |      |       |
|      | 0x08 → IRQ_PREVPIO: Index 0-7 of an IRQ flag in the next lower-numbered PIO<br>block                                                            |      |       |
|      | 0x10 → IRQ_NEXTPIO: Index 0-7 of an IRQ flag in the next higher-numbered<br>PIO block                                                           |      |       |

# <span id="page-948-0"></span>**[PIO:](#page-936-2) SM0\_SHIFTCTRL, SM1\_SHIFTCTRL, SM2\_SHIFTCTRL, SM3\_SHIFTCTRL Registers**

**Offsets**: 0x0d0, 0x0e8, 0x100, 0x118

#### **Description**

Control behaviour of the input/output shift registers for state machine *N*

*Table 996. SM0\_SHIFTCTRL, SM1\_SHIFTCTRL, SM2\_SHIFTCTRL, SM3\_SHIFTCTRL Registers*

| Bits  | Description                                                                                                                                                                                                    | Type | Reset |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31    | FJOIN_RX: When 1, RX FIFO steals the TX FIFO's storage, and becomes twice<br>as deep.<br>TX FIFO is disabled as a result (always reads as both full and empty).<br>FIFOs are flushed when this bit is changed. | RW   | 0x0   |
| 30    | FJOIN_TX: When 1, TX FIFO steals the RX FIFO's storage, and becomes twice<br>as deep.<br>RX FIFO is disabled as a result (always reads as both full and empty).<br>FIFOs are flushed when this bit is changed. | RW   | 0x0   |
| 29:25 | PULL_THRESH: Number of bits shifted out of OSR before autopull, or<br>conditional pull (PULL IFEMPTY), will take place.<br>Write 0 for value of 32.                                                            | RW   | 0x00  |
| 24:20 | PUSH_THRESH: Number of bits shifted into ISR before autopush, or<br>conditional push (PUSH IFFULL), will take place.<br>Write 0 for value of 32.                                                               | RW   | 0x00  |
| 19    | OUT_SHIFTDIR: 1 = shift out of output shift register to right. 0 = to left.                                                                                                                                    | RW   | 0x1   |
| 18    | IN_SHIFTDIR: 1 = shift input shift register to right (data enters from left). 0 = to<br>left.                                                                                                                  | RW   | 0x1   |

| Bits | Description                                                                                                                                                                                                                                                                                                                                                                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 17   | AUTOPULL: Pull automatically when the output shift register is emptied, i.e. on<br>or following an OUT instruction which causes the output shift counter to reach<br>or exceed PULL_THRESH.                                                                                                                                                                                                                                         | RW   | 0x0   |
| 16   | AUTOPUSH: Push automatically when the input shift register is filled, i.e. on an<br>IN instruction which causes the input shift counter to reach or exceed<br>PUSH_THRESH.                                                                                                                                                                                                                                                          | RW   | 0x0   |
| 15   | FJOIN_RX_PUT: If 1, disable this state machine's RX FIFO, make its storage<br>available for random write access by the state machine (using the put<br>instruction) and, unless FJOIN_RX_GET is also set, random read access by the<br>processor (through the RXFx_PUTGETy registers).<br>If FJOIN_RX_PUT and FJOIN_RX_GET are both set, then the RX FIFO's<br>registers can be randomly read/written by the state machine, but are | RW   | 0x0   |
|      | completely inaccessible to the processor.<br>Setting this bit will clear the FJOIN_TX and FJOIN_RX bits.                                                                                                                                                                                                                                                                                                                            |      |       |
| 14   | FJOIN_RX_GET: If 1, disable this state machine's RX FIFO, make its storage<br>available for random read access by the state machine (using the get<br>instruction) and, unless FJOIN_RX_PUT is also set, random write access by<br>the processor (through the RXFx_PUTGETy registers).                                                                                                                                              | RW   | 0x0   |
|      | If FJOIN_RX_PUT and FJOIN_RX_GET are both set, then the RX FIFO's<br>registers can be randomly read/written by the state machine, but are<br>completely inaccessible to the processor.                                                                                                                                                                                                                                              |      |       |
|      | Setting this bit will clear the FJOIN_TX and FJOIN_RX bits.                                                                                                                                                                                                                                                                                                                                                                         |      |       |
| 13:5 | Reserved.                                                                                                                                                                                                                                                                                                                                                                                                                           | -    | -     |
| 4:0  | IN_COUNT: Set the number of pins which are not masked to 0 when read by an<br>IN PINS, WAIT PIN or MOV x, PINS instruction.                                                                                                                                                                                                                                                                                                         | RW   | 0x00  |
|      | For example, an IN_COUNT of 5 means that the 5 LSBs of the IN pin group are<br>visible (bits 4:0), but the remaining 27 MSBs are masked to 0. A count of 32 is<br>encoded with a field value of 0, so the default behaviour is to not perform any<br>masking.                                                                                                                                                                       |      |       |
|      | Note this masking is applied in addition to the masking usually performed by<br>the IN instruction. This is mainly useful for the MOV x, PINS instruction, which<br>otherwise has no way of masking pins.                                                                                                                                                                                                                           |      |       |

# <span id="page-949-0"></span>**[PIO:](#page-936-2) SM0\_ADDR, SM1\_ADDR, SM2\_ADDR, SM3\_ADDR Registers**

**Offsets**: 0x0d4, 0x0ec, 0x104, 0x11c

*Table 997. SM0\_ADDR, SM1\_ADDR, SM2\_ADDR, SM3\_ADDR Registers*

| Bits | Description                                    | Type | Reset |
|------|------------------------------------------------|------|-------|
| 31:5 | Reserved.                                      | -    | -     |
| 4:0  | Current instruction address of state machine N | RO   | 0x00  |

#### <span id="page-949-1"></span>**[PIO:](#page-936-2) SM0\_INSTR, SM1\_INSTR, SM2\_INSTR, SM3\_INSTR Registers**

**Offsets**: 0x0d8, 0x0f0, 0x108, 0x120

*Table 998. SM0\_INSTR, SM1\_INSTR, SM2\_INSTR, SM3\_INSTR Registers*

| Bits  | Description                                                                                                                                                                             | Type | Reset |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:16 | Reserved.                                                                                                                                                                               | -    | -     |
| 15:0  | Read to see the instruction currently addressed by state machine N's program<br>counter.<br>Write to execute an instruction immediately (including jumps) and then<br>resume execution. | RW   | -     |

# <span id="page-950-1"></span>**[PIO:](#page-936-2) SM0\_PINCTRL, SM1\_PINCTRL, SM2\_PINCTRL, SM3\_PINCTRL Registers**

**Offsets**: 0x0dc, 0x0f4, 0x10c, 0x124

#### **Description**

State machine pin control

*Table 999. SM0\_PINCTRL, SM1\_PINCTRL, SM2\_PINCTRL, SM3\_PINCTRL Registers*

| Bits  | Description                                                                                                                                                                                                                                                                                                                                                                                                 | Type | Reset |
|-------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:29 | SIDESET_COUNT: The number of MSBs of the Delay/Side-set instruction field<br>which are used for side-set. Inclusive of the enable bit, if present. Minimum of<br>0 (all delay bits, no side-set) and maximum of 5 (all side-set, no delay).                                                                                                                                                                 | RW   | 0x0   |
| 28:26 | SET_COUNT: The number of pins asserted by a SET. In the range 0 to 5<br>inclusive.                                                                                                                                                                                                                                                                                                                          | RW   | 0x5   |
| 25:20 | OUT_COUNT: The number of pins asserted by an OUT PINS, OUT PINDIRS or<br>MOV PINS instruction. In the range 0 to 32 inclusive.                                                                                                                                                                                                                                                                              | RW   | 0x00  |
| 19:15 | IN_BASE: The pin which is mapped to the least-significant bit of a state<br>machine's IN data bus. Higher-numbered pins are mapped to consecutively<br>more-significant data bits, with a modulo of 32 applied to pin number.                                                                                                                                                                               | RW   | 0x00  |
| 14:10 | SIDESET_BASE: The lowest-numbered pin that will be affected by a side-set<br>operation. The MSBs of an instruction's side-set/delay field (up to 5,<br>determined by SIDESET_COUNT) are used for side-set data, with the remaining<br>LSBs used for delay. The least-significant bit of the side-set portion is the bit<br>written to this pin, with more-significant bits written to higher-numbered pins. | RW   | 0x00  |
| 9:5   | SET_BASE: The lowest-numbered pin that will be affected by a SET PINS or<br>SET PINDIRS instruction. The data written to this pin is the least-significant bit<br>of the SET data.                                                                                                                                                                                                                          | RW   | 0x00  |
| 4:0   | OUT_BASE: The lowest-numbered pin that will be affected by an OUT PINS,<br>OUT PINDIRS or MOV PINS instruction. The data written to this pin will always<br>be the least-significant bit of the OUT or MOV data.                                                                                                                                                                                            | RW   | 0x00  |

# <span id="page-950-0"></span>**[PIO:](#page-936-2) RXF0\_PUTGET0 Register**

**Offset**: 0x128

*Table 1000. RXF0\_PUTGET0 Register*

| Type | Reset      |
|------|------------|
| RW   | 0x00000000 |
|      |            |

#### <span id="page-950-2"></span>**[PIO:](#page-936-2) RXF0\_PUTGET1 Register**

**Offset**: 0x12c

*Table 1001. RXF0\_PUTGET1 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 1 of SM0's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

# <span id="page-951-1"></span>**[PIO:](#page-936-2) RXF0\_PUTGET2 Register**

**Offset**: 0x130

*Table 1002. RXF0\_PUTGET2 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 2 of SM0's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

# <span id="page-951-0"></span>**[PIO:](#page-936-2) RXF0\_PUTGET3 Register**

**Offset**: 0x134

*Table 1003. RXF0\_PUTGET3 Register*

| Bits | Description                                               | Type | Reset      |
|------|-----------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 3 of SM0's RX FIFO, if  | RW   | 0x00000000 |
|      | SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. |      |            |

# <span id="page-951-2"></span>**[PIO:](#page-936-2) RXF1\_PUTGET0 Register**

**Offset**: 0x138

*Table 1004. RXF1\_PUTGET0 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 0 of SM1's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

## <span id="page-951-3"></span>**[PIO:](#page-936-2) RXF1\_PUTGET1 Register**

**Offset**: 0x13c

*Table 1005. RXF1\_PUTGET1 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 1 of SM1's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

# <span id="page-951-4"></span>**[PIO:](#page-936-2) RXF1\_PUTGET2 Register**

**Offset**: 0x140

*Table 1006. RXF1\_PUTGET2 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 2 of SM1's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

#### <span id="page-951-5"></span>**[PIO:](#page-936-2) RXF1\_PUTGET3 Register**

**Offset**: 0x144

*Table 1007. RXF1\_PUTGET3 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 3 of SM1's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

# <span id="page-952-0"></span>**[PIO:](#page-936-2) RXF2\_PUTGET0 Register**

**Offset**: 0x148

*Table 1008. RXF2\_PUTGET0 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 0 of SM2's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

# <span id="page-952-1"></span>**[PIO:](#page-936-2) RXF2\_PUTGET1 Register**

**Offset**: 0x14c

*Table 1009. RXF2\_PUTGET1 Register*

| Bits | Description                                               | Type | Reset      |
|------|-----------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 1 of SM2's RX FIFO, if  | RW   | 0x00000000 |
|      | SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. |      |            |

# <span id="page-952-2"></span>**[PIO:](#page-936-2) RXF2\_PUTGET2 Register**

**Offset**: 0x150

*Table 1010. RXF2\_PUTGET2 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 2 of SM2's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

# <span id="page-952-3"></span>**[PIO:](#page-936-2) RXF2\_PUTGET3 Register**

**Offset**: 0x154

*Table 1011. RXF2\_PUTGET3 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 3 of SM2's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

# <span id="page-952-4"></span>**[PIO:](#page-936-2) RXF3\_PUTGET0 Register**

**Offset**: 0x158

*Table 1012. RXF3\_PUTGET0 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 0 of SM3's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

#### <span id="page-952-5"></span>**[PIO:](#page-936-2) RXF3\_PUTGET1 Register**

**Offset**: 0x15c

*Table 1013. RXF3\_PUTGET1 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 1 of SM3's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

# <span id="page-953-1"></span>**[PIO:](#page-936-2) RXF3\_PUTGET2 Register**

**Offset**: 0x160

*Table 1014. RXF3\_PUTGET2 Register*

| Bits | Description                                                                                                           | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 2 of SM3's RX FIFO, if<br>SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. | RW   | 0x00000000 |

# <span id="page-953-2"></span>**[PIO:](#page-936-2) RXF3\_PUTGET3 Register**

**Offset**: 0x164

*Table 1015. RXF3\_PUTGET3 Register*

| Bits | Description                                               | Type | Reset      |
|------|-----------------------------------------------------------|------|------------|
| 31:0 | Direct read/write access to entry 3 of SM3's RX FIFO, if  | RW   | 0x00000000 |
|      | SHIFTCTRL_FJOIN_RX_PUT xor SHIFTCTRL_FJOIN_RX_GET is set. |      |            |

# <span id="page-953-0"></span>**[PIO:](#page-936-2) GPIOBASE Register**

**Offset**: 0x168

*Table 1016. GPIOBASE Register*

| Bits | Description                                                                                                                                                                            | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:5 | Reserved.                                                                                                                                                                              | -    | -     |
| 4    | Relocate GPIO 0 (from PIO's point of view) in the system GPIO numbering, to<br>access more than 32 GPIOs from PIO.<br>Only the values 0 and 16 are supported (only bit 4 is writable). | RW   | 0x0   |
| 3:0  | Reserved.                                                                                                                                                                              | -    | -     |

#### <span id="page-953-3"></span>**[PIO:](#page-936-2) INTR Register**

**Offset**: 0x16c

#### **Description**

Raw Interrupts

*Table 1017. INTR Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:16 | Reserved.   | -    | -     |
| 15    | SM7         | RO   | 0x0   |
| 14    | SM6         | RO   | 0x0   |
| 13    | SM5         | RO   | 0x0   |
| 12    | SM4         | RO   | 0x0   |
| 11    | SM3         | RO   | 0x0   |
| 10    | SM2         | RO   | 0x0   |
| 9     | SM1         | RO   | 0x0   |
| 8     | SM0         | RO   | 0x0   |

| Bits | Description  | Type | Reset |
|------|--------------|------|-------|
| 7    | SM3_TXNFULL  | RO   | 0x0   |
| 6    | SM2_TXNFULL  | RO   | 0x0   |
| 5    | SM1_TXNFULL  | RO   | 0x0   |
| 4    | SM0_TXNFULL  | RO   | 0x0   |
| 3    | SM3_RXNEMPTY | RO   | 0x0   |
| 2    | SM2_RXNEMPTY | RO   | 0x0   |
| 1    | SM1_RXNEMPTY | RO   | 0x0   |
| 0    | SM0_RXNEMPTY | RO   | 0x0   |

# <span id="page-954-0"></span>**[PIO:](#page-936-2) IRQ0\_INTE Register**

**Offset**: 0x170 **Description**

Interrupt Enable for irq0

*Table 1018. IRQ0\_INTE Register*

| Bits  | Description  | Type | Reset |
|-------|--------------|------|-------|
| 31:16 | Reserved.    | -    | -     |
| 15    | SM7          | RW   | 0x0   |
| 14    | SM6          | RW   | 0x0   |
| 13    | SM5          | RW   | 0x0   |
| 12    | SM4          | RW   | 0x0   |
| 11    | SM3          | RW   | 0x0   |
| 10    | SM2          | RW   | 0x0   |
| 9     | SM1          | RW   | 0x0   |
| 8     | SM0          | RW   | 0x0   |
| 7     | SM3_TXNFULL  | RW   | 0x0   |
| 6     | SM2_TXNFULL  | RW   | 0x0   |
| 5     | SM1_TXNFULL  | RW   | 0x0   |
| 4     | SM0_TXNFULL  | RW   | 0x0   |
| 3     | SM3_RXNEMPTY | RW   | 0x0   |
| 2     | SM2_RXNEMPTY | RW   | 0x0   |
| 1     | SM1_RXNEMPTY | RW   | 0x0   |
| 0     | SM0_RXNEMPTY | RW   | 0x0   |

#### <span id="page-954-1"></span>**[PIO:](#page-936-2) IRQ0\_INTF Register**

**Offset**: 0x174

**Description**

Interrupt Force for irq0

*Table 1019. IRQ0\_INTF Register*

| Bits  | Description  | Type | Reset |
|-------|--------------|------|-------|
| 31:16 | Reserved.    | -    | -     |
| 15    | SM7          | RW   | 0x0   |
| 14    | SM6          | RW   | 0x0   |
| 13    | SM5          | RW   | 0x0   |
| 12    | SM4          | RW   | 0x0   |
| 11    | SM3          | RW   | 0x0   |
| 10    | SM2          | RW   | 0x0   |
| 9     | SM1          | RW   | 0x0   |
| 8     | SM0          | RW   | 0x0   |
| 7     | SM3_TXNFULL  | RW   | 0x0   |
| 6     | SM2_TXNFULL  | RW   | 0x0   |
| 5     | SM1_TXNFULL  | RW   | 0x0   |
| 4     | SM0_TXNFULL  | RW   | 0x0   |
| 3     | SM3_RXNEMPTY | RW   | 0x0   |
| 2     | SM2_RXNEMPTY | RW   | 0x0   |
| 1     | SM1_RXNEMPTY | RW   | 0x0   |
| 0     | SM0_RXNEMPTY | RW   | 0x0   |

# <span id="page-955-0"></span>**[PIO:](#page-936-2) IRQ0\_INTS Register**

**Offset**: 0x178

#### **Description**

Interrupt status after masking & forcing for irq0

*Table 1020. IRQ0\_INTS Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:16 | Reserved.   | -    | -     |
| 15    | SM7         | RO   | 0x0   |
| 14    | SM6         | RO   | 0x0   |
| 13    | SM5         | RO   | 0x0   |
| 12    | SM4         | RO   | 0x0   |
| 11    | SM3         | RO   | 0x0   |
| 10    | SM2         | RO   | 0x0   |
| 9     | SM1         | RO   | 0x0   |
| 8     | SM0         | RO   | 0x0   |
| 7     | SM3_TXNFULL | RO   | 0x0   |
| 6     | SM2_TXNFULL | RO   | 0x0   |
| 5     | SM1_TXNFULL | RO   | 0x0   |
| 4     | SM0_TXNFULL | RO   | 0x0   |

| Bits | Description  | Type | Reset |
|------|--------------|------|-------|
| 3    | SM3_RXNEMPTY | RO   | 0x0   |
| 2    | SM2_RXNEMPTY | RO   | 0x0   |
| 1    | SM1_RXNEMPTY | RO   | 0x0   |
| 0    | SM0_RXNEMPTY | RO   | 0x0   |

# <span id="page-956-0"></span>**[PIO:](#page-936-2) IRQ1\_INTE Register**

**Offset**: 0x17c **Description**

Interrupt Enable for irq1

*Table 1021. IRQ1\_INTE Register*

| Bits  | Description  | Type | Reset |  |
|-------|--------------|------|-------|--|
| 31:16 | Reserved.    | -    | -     |  |
| 15    | SM7          | RW   | 0x0   |  |
| 14    | SM6          | RW   | 0x0   |  |
| 13    | SM5          | RW   | 0x0   |  |
| 12    | SM4          | RW   | 0x0   |  |
| 11    | SM3          | RW   | 0x0   |  |
| 10    | SM2          | RW   | 0x0   |  |
| 9     | SM1          | RW   | 0x0   |  |
| 8     | SM0          | RW   | 0x0   |  |
| 7     | SM3_TXNFULL  | RW   | 0x0   |  |
| 6     | SM2_TXNFULL  | RW   | 0x0   |  |
| 5     | SM1_TXNFULL  | RW   | 0x0   |  |
| 4     | SM0_TXNFULL  | RW   | 0x0   |  |
| 3     | SM3_RXNEMPTY | RW   | 0x0   |  |
| 2     | SM2_RXNEMPTY | RW   | 0x0   |  |
| 1     | SM1_RXNEMPTY | RW   | 0x0   |  |
| 0     | SM0_RXNEMPTY | RW   | 0x0   |  |

#### <span id="page-956-1"></span>**[PIO:](#page-936-2) IRQ1\_INTF Register**

**Offset**: 0x180 **Description**

Interrupt Force for irq1

*Table 1022. IRQ1\_INTF Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:16 | Reserved.   | -    | -     |
| 15    | SM7         | RW   | 0x0   |
| 14    | SM6         | RW   | 0x0   |
| 13    | SM5         | RW   | 0x0   |

| Bits | Description  | Type | Reset |
|------|--------------|------|-------|
| 12   | SM4          | RW   | 0x0   |
| 11   | SM3          | RW   | 0x0   |
| 10   | SM2          | RW   | 0x0   |
| 9    | SM1          | RW   | 0x0   |
| 8    | SM0          | RW   | 0x0   |
| 7    | SM3_TXNFULL  | RW   | 0x0   |
| 6    | SM2_TXNFULL  | RW   | 0x0   |
| 5    | SM1_TXNFULL  | RW   | 0x0   |
| 4    | SM0_TXNFULL  | RW   | 0x0   |
| 3    | SM3_RXNEMPTY | RW   | 0x0   |
| 2    | SM2_RXNEMPTY | RW   | 0x0   |
| 1    | SM1_RXNEMPTY | RW   | 0x0   |
| 0    | SM0_RXNEMPTY | RW   | 0x0   |

# <span id="page-957-0"></span>**[PIO:](#page-936-2) IRQ1\_INTS Register**

**Offset**: 0x184

#### **Description**

Interrupt status after masking & forcing for irq1

*Table 1023. IRQ1\_INTS Register*

| Bits  | Description  | Type |     | Reset |
|-------|--------------|------|-----|-------|
| 31:16 | Reserved.    | -    | -   |       |
| 15    | SM7          | RO   | 0x0 |       |
| 14    | SM6          | RO   | 0x0 |       |
| 13    | SM5          | RO   | 0x0 |       |
| 12    | SM4          | RO   | 0x0 |       |
| 11    | SM3          | RO   | 0x0 |       |
| 10    | SM2          | RO   | 0x0 |       |
| 9     | SM1          | RO   | 0x0 |       |
| 8     | SM0          | RO   | 0x0 |       |
| 7     | SM3_TXNFULL  | RO   | 0x0 |       |
| 6     | SM2_TXNFULL  | RO   | 0x0 |       |
| 5     | SM1_TXNFULL  | RO   | 0x0 |       |
| 4     | SM0_TXNFULL  | RO   | 0x0 |       |
| 3     | SM3_RXNEMPTY | RO   | 0x0 |       |
| 2     | SM2_RXNEMPTY | RO   | 0x0 |       |
| 1     | SM1_RXNEMPTY | RO   | 0x0 |       |
| 0     | SM0_RXNEMPTY | RO   | 0x0 |       |

# <span id="page-958-0"></span>**Chapter 12. Peripherals**

