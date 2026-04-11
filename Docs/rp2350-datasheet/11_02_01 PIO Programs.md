# 11.2.1 PIO Programs

PIO state machines execute short binary programs.

Programs for common interfaces, such as UART, SPI, or I2C, are available in the PIO library. In many cases, it is not necessary to write PIO programs. However, the PIO is much more flexible when programmed directly, supporting a wide variety of interfaces which may not have been foreseen by its designers.

The PIO has a total of nine instructions: JMP, WAIT, IN, OUT, PUSH, PULL, MOV, IRQ, and SET. For more information about these instructions, see [Section 11.4](#page-886-1).

Though the PIO only has a total of nine instructions, it would be difficult to edit PIO program binaries by hand. PIO assembly is a textual format, describing a PIO program, where each command corresponds to one instruction in the output binary. The following code snippet contains an example program written in in PIO assembly:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.pio](https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.pio#L8-L13) Lines 8 - 13*

```
 8 .program squarewave
 9 set pindirs, 1 ; Set pin to output
10 again:
11 set pins, 1 [1] ; Drive pin high and then delay for one cycle
12 set pins, 0 ; Drive pin low
13 jmp again ; Set PC to label `again`
```

The PIO assembler is included with the SDK, and is called pioasm. This program processes a PIO assembly input text file, which may contain multiple programs, and writes out the assembled programs ready for use. For the SDK, these assembled programs are emitted as C headers, containing constant arrays.

For more information, see [Section 11.3.](#page-882-1)

#### <span id="page-876-1"></span>**11.2.2. Control Flow**

On every system clock cycle, each state machine fetches, decodes and executes one instruction. Each instruction takes precisely one cycle, unless it explicitly stalls (such as the WAIT instruction). Instructions may insert a delay of up to 31 cycles before the next instruction execute, to help write cycle-exact programs.

The program counter, or PC, points to the location in the instruction memory being executed on this cycle. Generally, PC increments by one each cycle, wrapping at the end of the instruction memory. Jump instructions are an exception and explicitly provide the next value that PC will take.

Our example assembly program (listed as .program squarewave above) shows both of these concepts in practice. It drives a 50/50 duty cycle square wave with a period of four cycles onto a GPIO. Using some other features (e.g. side-set) this can be made as low as two cycles.

# **NOTE**

Side-set is where a state machine drives a small number of GPIOs *in addition to* the main side effects of the instruction it executes. It's described fully in [Section 11.5.1.](#page-899-1)

The system has write-only access to the instruction memory, which is used to load programs. The clock divider slows the state machine's execution by a constant factor, represented as a 16.8 fixed-point fractional number. In the following example, if a clock division of 2.5 were programmed, the square wave would have a period of cycles. This is useful for setting a precise baud rate for a serial interface, such as a UART.

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.c](https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.c#L34-L38) Lines 34 - 38*

```
34 // Load the assembled program directly into the PIO's instruction memory.
35 // Each PIO instance has a 32-slot instruction memory, which all 4 state
36 // machines can see. The system has write-only access.
37 for (uint i = 0; i < count_of(squarewave_program_instructions); ++i)
38 pio->instr_mem[i] = squarewave_program_instructions[i];
```

The following code fragments are part of a complete code example which drives a 12.5 MHz square wave out of GPIO 0 (or any other pins we might choose to map). We can also use pins WAIT PIN instruction to stall a state machine's execution for some amount of time, or a JMP PIN instruction to branch on the state of a pin, so control flow can vary based on pin state.

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.c](https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.c#L42-L47) Lines 42 - 47*

```
42 // Configure state machine 0 to run at sysclk/2.5. The state machines can
43 // run as fast as one instruction per clock cycle, but we can scale their
44 // speed down uniformly to meet some precise frequency target, e.g. for a
45 // UART baud rate. This register has 16 integer divisor bits and 8
46 // fractional divisor bits.
47 pio->sm[0].clkdiv = (uint32_t) (2.5f * (1 << 16));
```

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.c](https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.c#L51-L59) Lines 51 - 59*

```
51 // There are five pin mapping groups (out, in, set, side-set, jmp pin)
52 // which are used by different instructions or in different circumstances.
53 // Here we're just using SET instructions. Configure state machine 0 SETs
54 // to affect GPIO 0 only; then configure GPIO0 to be controlled by PIO0,
55 // as opposed to e.g. the processors.
56 pio->sm[0].pinctrl =
57 (1 << PIO_SM0_PINCTRL_SET_COUNT_LSB) |
58 (0 << PIO_SM0_PINCTRL_SET_BASE_LSB);
59 gpio_set_function(0, pio_get_funcsel(pio));
```

The system can start and stop each state machine at any time, via the CTRL register. Multiple state machines can be started simultaneously, and the deterministic nature of PIO means they can stay perfectly synchronised.

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.c](https://github.com/raspberrypi/pico-examples/blob/master/pio/squarewave/squarewave.c#L63-L67) Lines 63 - 67*

```
63 // Set the state machine running. The PIO CTRL register is global within a
64 // PIO instance, so you can start/stop multiple state machines
65 // simultaneously. We're using the register's hardware atomic set alias to
66 // make one bit high without doing a read-modify-write on the register.
67 hw_set_bits(&pio->ctrl, 1 << (PIO_CTRL_SM_ENABLE_LSB + 0));
```

Most instructions are executed from instruction memory, but there are other sources which can be freely mixed:

- Instructions written to a special configuration register (SMx INSTR) are immediately executed, momentarily interrupting other execution. For example, a JMP instruction written to SMx INSTR causes the state machine to start executing from a different location.
- Instructions can be executed from a register, using the MOV EXEC instruction.
- Instructions can be executed from the output shifter, using the OUT EXEC instruction

The last of these is particularly versatile: instructions can be embedded in the stream of data passing through the FIFO. The I2C example uses this to embed e.g. STOP and RESTART line conditions alongside normal data. In the case of MOV and OUT EXEC, the MOV/OUT itself executes in one cycle, and the executee on the next.

