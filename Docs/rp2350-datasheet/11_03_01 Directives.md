# 11.3.1 Directives

The following directives control the assembly of PIO programs:

#### **.define (PUBLIC) <symbol> <value>**

Define an integer symbol named <symbol> with the value <value> (see [Section 11.3.2\)](#page-884-0). If this .define appears before the first program in the input file, then this define is global to all programs, otherwise it is local to the program in which it occurs. If PUBLIC is specified, the symbol will be emitted into the assembled output for use by user code. For the SDK this takes the following forms:

- #define <program\_name> <symbol> value: for program symbols
- #define <symbol> value: for global symbols

#### **.clock\_div <divider>**

If this directive is present, <divider> is the state machine clock divider for the program. Note, that divider is a floating point value, but *may not* currently use arithmetic expressions or defined values. This directive affects the default state machine configuration for a program. This directive is only valid within a program before the first instruction.

#### **.fifo <fifo\_config>**

If this directive is present, it is used to specify the FIFO configuration for the program. It affects the default state machine configuration for a program, but also restricts what instructions may be used (for example PUSH makes no sense if there is no IN FIFO configured).

This directive supports the following configuration values:

- txrx: 4 FIFO entries for each of TX and RX; this is the default.
- tx: All 8 FIFO entries for TX.
- rx: All 8 FIFO entries for RX.
- txput: 4 FIFO entries for TX, and 4 FIFO entries for mov rxfifo[index], isr aka put. This value is not supported on PIO version 0.
- txget: 4 FIFO entries for TX, and 4 FIFO entries for mov osr, rxfifo[index] aka get. This value is not supported on PIO version 0.
- putget: 4 FIFO entries for mov rxfifo[index], isr aka put, and 4 FIFO entries for mov osr, rxfifo[index] aka get. This value is not supported on PIO version 0.

This directive is only valid within a program before the first instruction.

```
.mov_status rxfifo < <n>
.mov_status txfifo < <n>
.mov_status irq <(next|prev)> set <n>
```

This directive configures the source for the mov , STATUS. One of the three syntaxes can be used to set the status based on the RXFIFO level being below a value N, the TXFIFO level being below a value N, or an IRQ flag N being set on this PIO instance (or the next higher numbered, or lowered numbered PIO instance if next or prev or specified). Note, that the IRQ option requires PIO version 1.

This directive affects the default state machine configuration for a program. This directive is only valid within a program before the first instruction.

```
.in <count> (left|right) (auto) (<threshold>)
```

If this directive is present, <count> indicates the number of IN bits to be used. 'left' or 'right' if specified, control the ISR shift direction; 'auto', if present, enables "auto-push"; <threshold>, if present, specifies the "auto-push" threshold. This directive affects the default state machine configuration for a program.

This directive is only valid within a program before the first instruction. When assembling for PIO version 0, <count> must be 32.

#### **.program <name>**

Start a new program with the name <name>. Note that that name is used in code so should be alphanumeric/underscore not starting with a digit. The program lasts until another .program directive or the end of the source file. PIO instructions are only allowed within a program.

#### **.origin <offset>**

Optional directive to specify the PIO instruction memory offset at which the program *must* load. Most commonly this is used for programs that must load at offset 0, because they use data based JMPs with the (absolute) jmp target being stored in only a few bits. This directive is invalid outside a program.

#### **.out <count> (left|right) (auto) (<threshold>)**

If this directive is present, <count> indicates the number of OUT bits to be used. 'left' or 'right' if specified control the OSR shift direction; 'auto', if present, enables "auto-pull"; <threshold>, if present, specifies the "auto-pull" threshold. This directive affects the default state machine configuration for a program. This directive is only valid within a program before the first instruction.

#### **.pio\_version <version>**

This directive sets the target PIO hardware version. The version for RP2350 is 1 or RP2350, and is also the default version number. For backwards compatibility with RP2040, 0 or RP2040 may be used.

If this directive appears before the first program in the input file, then this define is the default for all programs, otherwise it specifies the version for the program in which it occurs. If specified for a program, it must occur before the first instruction.

#### **.set <count>**

If this directive is present, <count> indicates the number of SET bits to be used. This directive affects the default state machine configuration for a program. This directive is only valid within a program before the first instruction.

#### **.side\_set <count> (opt) (pindirs)**

If this directive is present, <count> indicates the number of side-set bits to be used. Additionally, opt may be specified to indicate that a side <value> is optional for instructions (note this requires stealing an extra bit — in addition to the <count> bits — from those available for the instruction delay). Finally, pindirs may be specified to indicate that the side set values should be applied to the PINDIRs and not the PINs. This directive is only valid within a program before the first instruction.

#### **.wrap\_target**

Place prior to an instruction, this directive specifies the instruction where execution continues due to program wrapping. This directive is invalid outside of a program, may only be used once within a program, and if not specified defaults to the start of the program.

#### **.wrap**

Placed after an instruction, this directive specifies the instruction after which, in normal control flow (i.e. jmp with false condition, or no jmp), the program wraps (to .wrap\_target instruction). This directive is invalid outside of a program, may only be used once within a program, and if not specified defaults to after the last program instruction.

#### **.lang\_opt <lang> <name> <option>**

Specifies an option for the program related to a particular language generator. (See Language Generators in [Raspberry Pi Pico-series C/C++ SDK\)](https://datasheets.raspberrypi.com/pico/raspberry-pi-pico-c-sdk.pdf). This directive is invalid outside of a program.

#### **.word <value>**

Stores a raw 16-bit value as an instruction in the program. This directive is invalid outside of a program.

