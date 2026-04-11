# 3.8.1 Instruction Set Reference

This section is a programmer's reference guide for the instructions supported by Hazard3. It covers basic assembly syntax, instruction behaviour, ranges for immediate values, and conditions for instruction compression. [The index](#page-237-0) lists instructions alphabetically, including pseudo-instructions.

The pseudocode in this guide is informative only, and is no replacement for the official RISC-V specifications in [Section](#page-233-2) [3.8.1.1](#page-233-2). However, it should prove a useful mnemonic aid once you have read the specifications.

#### <span id="page-233-2"></span>**3.8.1.1. Links to RISC-V Specifications**

This table links ratified versions of the base instruction set and extensions implemented by Hazard3. These are the authoritative reference for the instructions documented in this reference guide.

| Extension         | Specification                                  |
|-------------------|------------------------------------------------|
| RV32I v2.1        | Unprivileged ISA 20191213                      |
| M v2.0            | Unprivileged ISA 20191213                      |
| A v2.1            | Unprivileged ISA 20191213                      |
| C v2.0            | Unprivileged ISA 20191213                      |
| Zicsr v2.0        | Unprivileged ISA 20191213                      |
| Zifencei v2.0     | Unprivileged ISA 20191213                      |
| Zba v1.0.0        | Bit Manipulation ISA extensions 20210628       |
| Zbb v1.0.0        | Bit Manipulation ISA extensions 20210628       |
| Zbs v1.0.0        | Bit Manipulation ISA extensions 20210628       |
| Zbkb v1.0.1       | Scalar Cryptography ISA extensions 20220218    |
| Zcb v1.0.3-1      | Code Size Reduction extensions frozen v1.0.3-1 |
| Zcmp v1.0.3-1     | Code Size Reduction extensions frozen v1.0.3-1 |
| Machine ISA v1.12 | Privileged Architecture 20211203               |
| Debug v0.13.2     | RISC-V External Debug Support 20190322         |

You may also refer to the [RISC-V Assembly Programmer's Manual](https://github.com/riscv-non-isa/riscv-asm-manual/blob/main/riscv-asm.md) for information on assembly syntax.

Consult the [source code](https://github.com/wren6991/hazard3) for detailed questions about implementation-defined behaviour, which is not covered by the RISC-V specifications. RP2350 uses version [86fc4e3,](https://github.com/Wren6991/Hazard3/releases/tag/v1.0-rc1) with metal ECOs for commits [2f6e983](https://github.com/Wren6991/Hazard3/commit/2f6e98335fd4fafe9c6faf9285374316c0da8ee4) and [af08c0b](https://github.com/Wren6991/Hazard3/commit/af08c0becd13ef6462f0656d247231cbdeb56f70).

#### **3.8.1.2. Architecture Strings**

-march strings completely specify the set of available RISC-V instructions, so that a compiler can generate correct and optimal code for your device. Use the following in descending order of preference:

- 1. Use rv32ima\_zicsr\_zifencei\_zba\_zbb\_zbs\_zbkb\_zca\_zcb\_zcmp for compilers which support the Zcb and Zcmp extensions, such as GCC 14.
- 2. Use rv32ima\_zicsr\_zifencei\_zba\_zbb\_zbs\_zbkb\_zca\_zcb for GCC 14 packaged with an older assembler which does not support Zcmp.
- 3. Use rv32imac\_zicsr\_zifencei\_zba\_zbb\_zbs\_zbkb for older compilers, such as GCC 13 and below.

#### **3.8.1.3. RISC-V Architectural State**

The mutable state visible to the programmer consists of:

- The 31 × 32-bit integer general-purpose registers (GPRs), named x1 through x31
- The program counter pc, which points to the beginning of the current instruction in memory
- The control and status registers (CSRs), which configure processor behaviour and are used in trap handling
- The local monitor bit, which helps maintain correctness of atomic read-modify-write sequences
- The current privilege level, which determines which memory locations the core can access, which CSRs it can access, and which instructions it can execute

Hazard3 supports two privilege levels: Machine and User. These are interchangeably referred to as **modes**, and are commonly abbreviated as M-mode and U-mode. Debug mode behaves as an additional privilege level above M-mode.

The 0th general-purpose register, x0, is hardwired to zero and ignores writes. There is no flags register; branch instructions perform GPR-to-GPR comparisons directly.

This state is duplicated per hardware thread, or **hart**. RP2350 implements two Hazard3 cores, each with one hart.

#### **3.8.1.3.1. Register Conventions**

The following ABI names are synonymous with x0 through x31:

| Register  | ABI Name | Description                          |
|-----------|----------|--------------------------------------|
| x0        | zero     | Hardwired to zero; ignores writes    |
| x1        | ra       | Return address (link register)       |
| x2        | sp       | Stack pointer                        |
| x3        | gp       | Global pointer                       |
| x4        | tp       | Thread pointer                       |
| x5 - x7   | t0 - t2  | Temporaries                          |
| x8        | s0 or fp | Saved register or frame pointer      |
| x9        | s1       | Saved register                       |
| x10 - x11 | a0 - a1  | Function arguments and return values |
| x12 - x17 | a2 - a7  | Function arguments                   |
| x18 - x27 | s2 - s11 | Saved registers                      |
| x28 - x31 | t3 - t6  | Temporaries                          |

Registers x1 through x31 are identical, and any 32-bit opcode can use any combination of these registers. However, compressed instructions give preferential treatment to commonly-used registers sp, ra, s0, s1 and a0 through a5 to improve code density. All compressed instructions implemented by Hazard3 are 16-bit aliases for existing 32-bit instructions, so you can still perform any operation on any register.

See the [RISC-V PSABI Specification](https://github.com/riscv-non-isa/riscv-elf-psabi-doc/releases/download/v1.0/riscv-abi.pdf) for more information on the ABI register assignment as well as the RISC-V procedure calling convention.

#### **3.8.1.4. Compressed Instructions**

The RISC-V extensions which Hazard3 implements use a mixture of 32-bit and 16-bit opcodes, the latter being referred to as **compressed instructions**. With the exception of Zcmp, each compressed instruction maps to a subset of an existing 32-bit instruction. For example, c.add is a 16-bit alias of the [add](#page-238-0) instruction, with restrictions on register allocation.

The assembler automatically uses compressed instructions when possible. For example, add a0, a0, a1 is a compressible form of add. This assembles to the 16-bit opcode c.add a0, a1 when compressed instructions are enabled in the assembler.

The following extensions use 16-bit opcodes:

- [C: compressed instructions](#page-261-0) (the non-floating-point subset is equivalently spelled as Zca)
- [Zcb: additional basic compressed instructions](#page-272-0)
- [Zcmp: compressed push, pop and double-move](#page-273-0)

Disabling the above extensions for compilation (and assembly) aligns all instructions to 32-bit boundaries. This may have a minor performance advantage for branch-dense code sequences (see [Section 3.8.7](#page-296-0)), at the cost of poorer code density.

When an instruction has an optional 16-bit compressed form, the limitations of the compressed form are documented in the listing for the 32-bit form. It is useful to be aware of these restrictions when optimising for code size. If no such limitations are mentioned, it means the instruction is always a 32-bit opcode.

Zcmp is an outlier in that its instructions each expand to a *sequence* of 32-bit instructions from the RV32I base instruction set. They therefore have no direct 32-bit counterparts.

#### **3.8.1.5. Conventions for Pseudocode**

Pseudocode in this section is in Verilog 2005 syntax (IEEE 1364-2005). These Verilog operators are used throughout:

- Infix operators +, -, \*, /, &, ^, |, <<, ==, !=, < and >= can be considered the same as the corresponding C operator.
- \$signed() bit-casts to a signed value; comparisons between two signed values are signed comparisons.
- >> is always a logical (zero-extending) right shift.
- >>> on a signed value is an arithmetic (sign-extending) right shift.
- {a, b} is the bit-concatenation of a and b, with a in the more-significant position of the result.
- a[n] on an array is a subscript array access. For example mem[0] is the first byte of memory.
- x[m:l] on a packed array (a bit vector) is a bit slice of x, where m is the (inclusive) MSB and l is the (inclusive) LSB. For example rs1[7:0] is the 8 least-significant bits of rs1.
- {n{x}}, where n is a constant and x is a packed array, replicates <sup>x</sup> <sup>n</sup> times. n copies of x are concatenated together. For example {32{1'b1}} is a 32-bit all-ones value.

The pseudocode uses <= non-blocking assignments to assign to outputs: all such assignments are applied in a batch after the block of pseudocode has executed. Local variables may be assigned with = blocking assignments, which update the assignee immediately, similar to = procedural assignments in e.g. C programs. This distinction is important in some cases where e.g. rd and rs1 may alias the same register, but it's generally sufficient just to be aware that a <= b and a = b are both assignments into a.

#### **3.8.1.5.1. Variables Used in Pseudocode**

Pseudocode in this guide uses the following conventions for variables:

- rs1, rs2 and rd are 32-bit unsigned packed arrays (bit vectors), representing the values of the two register operands and the destination register.
- regnum\_rs1, regnum\_rs2, and regnum\_rd are the 5-bit register numbers which select a GPR for rs1, rs2 and rd
- imm is a 32-bit unsigned packed array referring to the instruction's immediate value.
- pc is a 32-bit unsigned packed array referring to the program counter, which is exactly the address of the current instruction.
- mem is an array of 8-bit unsigned packed arrays, each corresponding to a byte address in memory.
- csr is an array of 32-bit unsigned packed arrays, each corresponding to a CSR listed in [Section 3.8.9.](#page-304-0)
- priv is a 2-bit unsigned packed array which contains the value 0x3 when the core is in Debug or M-mode, and 0x0 when the core is in U-mode.
- <sup>i</sup> and j are pseudocode temporary variables of type integer which may be used for loop variables.

The following tasks are used throughout:

- raise\_exception(n) raises an exception with a cause of n (see [Section 3.8.4.1\)](#page-283-0).
- bus\_error(addr) returns 1 when the address addr returns a bus error, and 0 otherwise.

#### <span id="page-237-0"></span>3.8.1.6. Alphabetical List of Instructions

This instruction reference covers all instructions from all extensions which Hazard3 implements on RP2350. The table below also includes common pseudo-instructions such as not and ret, which you may see in disassembly and be surprised not to see in the ISA manual. The links for pseudo-instructions go to the entry for the underlying hardware instruction aliased by that pseudo-instruction.

![](_page_237_Picture_3.jpeg)

#### TIP

The instruction names at the left-hand margin of the instruction listings are links back to this index. Use them to quickly return here and look up another instruction.

<span id="page-237-1"></span>

| Alphabetical order: lo | Alphabetical order: left-to-right, then top-to-bottom. |           |            |          |           |
|------------------------|--------------------------------------------------------|-----------|------------|----------|-----------|
| add                    | addi                                                   | amoadd.w  | amoand.w   | amomax.w | amomaxu.w |
| amomin.w               | amominu.w                                              | amoor.w   | amoswap.w  | amoxor.w | and       |
| andi                   | andn                                                   | auipc     | bclr       | bclri    | beq       |
| beqz                   | bext                                                   | bexti     | bge        | bgeu     | bgez      |
| bgt                    | bgtu                                                   | bgtz      | binv       | binvi    | ble       |
| bleu                   | blez                                                   | blt       | bltu       | bltz     | bne       |
| bnez                   | brev8                                                  | bset      | bseti      | clz      | cm.mva01s |
| cm.mvsa01              | cm.pop                                                 | cm.popret | cm.popretz | cm.push  | срор      |
| csrc                   | csrci                                                  | csrr      | esrre      | esrrei   | csrrs     |
| csrrsi                 | csrrw                                                  | csrrwi    | csrs       | csrsi    | csrw      |
| csrwi                  | ctz                                                    | div       | divu       | ebreak   | ecall     |
| fence                  | fence.i                                                | j         | jal        | jalr     | jr        |
| 1b                     | lbu                                                    | lh        | lhu        | lr.w     | lui       |
| lw                     | max                                                    | maxu      | min        | minu     | mret      |
| mul                    | mulh                                                   | mulhsu    | mulhu      | mv       | neg       |
| пор                    | not                                                    | or        | orc.b      | ori      | orn       |
| pack                   | packh                                                  | rem       | remu       | ret      | rev8      |
| rol                    | гог                                                    | rori      | sb         | SC.W     | seqz      |
| sext.b                 | sext.h                                                 | sgtz      | sh1add     | sh2add   | sh3add    |
| sh                     | sll                                                    | slli      | slt        | slti     | sltiu     |
| sltu                   | sltz                                                   | snez      | sra        | srai     | srl       |
| srli                   | sub                                                    | SW        | unzip      | wfi      | xnor      |
| хог                    | xori                                                   | zext.b    | zext.h     | zip      |           |

The remainder of this reference guide groups instructions by extension:

- RV32I: base ISA (register-register)
- RV32I: base ISA (register-immediate)
- RV32I: base ISA (large immediate)
- RV32I: base ISA (control transfer)
- RV32I: base ISA (load/store)

- [M: multiply and divide](#page-252-2)
- [A: atomics](#page-254-2)
- [C: compressed instructions](#page-261-0)
- [Zba: bit manipulation for address generation](#page-262-3)
- [Zbb: basic bit manipulation](#page-262-4)
- [Zbs: single bit manipulation](#page-269-4)
- [Zbkb: basic bit manipulation for scalar cryptography](#page-271-3)
- [Zcb: additional basic compressed instructions](#page-272-0)
- [Zcmp: compressed push, pop and double-move](#page-273-0)
- [RV32I and Zifencei: memory ordering](#page-273-2)
- [Zicsr: control and status register access](#page-274-1)
- [Privileged instructions](#page-277-3)

#### <span id="page-238-2"></span>**3.8.1.7. RV32I: Base ISA (Register-register)**

These instructions calculate a function of two register operands, rs1 and rs2. They write the 32-bit result to a destination register, rd.

<span id="page-238-0"></span>**[add](#page-237-1)**

Add register to register.

Usage:

```
add rd, rs1, rs2
```

Operation:

```
rd <= rs1 + rs2;
```

Compressible if either:

- rd matches rs1, no operands are zero (aka c.add)
- rs2 is zero and neither rd nor rs1 is zero (aka c.mv)

<span id="page-238-1"></span>**[and](#page-237-1)**

Bitwise AND register with register.

Usage:

```
and rd, rs1, rs2
```

Operation:

```
rd <= rs1 & rs2;
```

Compressible if: rd matches rs1, registers are in x8 - x15.

<span id="page-239-0"></span>**[or](#page-237-1)**

Bitwise OR register with register.

Usage:

```
or rd, rs1, rs2
```

Operation:

```
rd <= rs1 | rs2;
```

Compressible if: rd matches rs1, registers are in x8 - x15.

<span id="page-239-2"></span>**[sll](#page-237-1)**

Shift left, logical. Shift amount is modulo 32.

Usage:

```
sll rd, rs1, rs2
```

Operation:

```
rd <= rs1 << rs2[4:0];
```

<span id="page-239-1"></span>**[slt](#page-237-1)**

Set if less than (signed). Result is 0 for false, 1 for true.

Usage:

```
slt rd, rs1, rs2
sltz rd, rs1 // pseudo: rs2 is zero
sgtz rd, rs2 // pseudo: rs1 is zero
```

Operation:

```
rd <= $signed(rs1) < $signed(rs2);
```

<span id="page-239-3"></span>**[sltu](#page-237-1)**

Set if less than (unsigned). Result is 0 for false, 1 for true.

Usage:

```
sltu rd, rs1, rs
snez rd, rs2 // pseudo: rs1 is zero
```

```
rd <= rs1 < rs2;
```

<span id="page-240-1"></span>**[sra](#page-237-1)**

Shift right, arithmetic. Shift amount is modulo 32.

Usage:

```
sra rd, rs1, rs2
```

Operation:

```
rd <= $signed(rs1) >>> rs2[4:0];
```

<span id="page-240-2"></span>**[srl](#page-237-1)**

Shift right, logical. Shift amount is modulo 32.

Usage:

```
srl rd, rs1, rs2
```

Operation:

```
rd <= rs1 >> rs2[4:0];
```

<span id="page-240-0"></span>**[sub](#page-237-1)**

Two's complement subtract register from register.

Usage:

```
sub rd, rs1, rs2
neg rd, rs2 // pseudo: rs1 is zero
```

Operation:

```
rd <= rs1 - rs2;
```

<span id="page-240-3"></span>Compressible if: rd matches rs1, registers are in x8 - x15.

**[xor](#page-237-1)**

Bitwise XOR register with register

Usage:

```
xor rd, rs1, rs2
```

Operation:

```
rd <= rs1 ^ rs2;
```

Compressible if: rd matches rs1, registers are in x8 - x15.

#### <span id="page-241-2"></span>**3.8.1.8. RV32I: Base ISA (Register-immediate)**

These instructions calculate a function of one register rs1 and one immediate operand imm. They write the 32-bit result to a destination register rd.

Immediate operands are constants encoded directly in the instruction, which avoids the cost of first materialising the constant value into a register.

<span id="page-241-0"></span>**[addi](#page-237-1)**

Add register to immediate.

Usage:

```
addi rd, rs1, imm
mv rd, rs1 // pseudo: imm is 0
nop // pseudo: rd, rs1 are zero, imm is 0
```

Operation:

```
rd <= rs1 + imm
```

Immediate range: -0x800 through 0x7ff for 32-bit, smaller for 16-bit.

Compressible if:

- rd matches rs1, and immediate is in the range -0x20 through 0x1f (aka c.addi)
- rd is not zero, rs1 is zero, and immediate is in the range -0x20 through 0x1f (aka c.li)
- rd is in x8 x15, rs1 is sp, and immediate is a nonzero multiple of four in the range 0x000 through 0x3fc (aka c.addi4spn)
- rd is sp, rs1 is sp, and immediate is a nonzero multiple of 16 in the range -0x200 through 0x1f0 (aka c.addi16sp)

Note compressed c.mv canonically expands to [add,](#page-238-0) not addi.

<span id="page-241-1"></span>**[andi](#page-237-1)**

Bitwise AND register with immediate.

Usage:

```
andi rd, rs1, imm
zext.b rd, rs1 // pseudo: imm is 0xff
```

```
rd <= rs1 & imm;
```

Immediate range: -0x800 through 0x7ff for 32-bit, -0x20 through 0x1f for 16-bit.

Compressible if: rd matches rs1, registers are in x8 - x15, and immediate is in the range -0x20 through 0x1f.

<span id="page-242-0"></span>**[ori](#page-237-1)**

Bitwise OR register with immediate.

Usage:

```
ori rd, rs1, imm
```

Operation:

```
rd <= rs1 | imm;
```

Immediate range: -0x800 through 0x7ff

<span id="page-242-1"></span>**[slli](#page-237-1)**

Shift left, logical, immediate.

Usage:

```
slli rd, rs1, imm
```

Operation:

```
rd <= rs1 << imm;
```

Immediate range: 0 through 31.

Compressible if: rd matches rs1, registers are not zero.

<span id="page-242-2"></span>**[slti](#page-237-1)**

Set if less than immediate (signed). Result is 0 for false, 1 for true.

Usage:

```
slti rd, rs1, imm
```

Operation:

```
rd <= $signed(rs1) < $signed(imm);
```

Immediate range: -0x800 through 0x7ff

#### <span id="page-243-0"></span>**[sltiu](#page-237-1)**

Set if less than immediate (unsigned). Result is 0 for false, 1 for true.

Usage:

```
sltiu rd, rs1, imm
seqz rd, rs1 // pseudo: imm is 1
```

Operation:

```
rd <= rs1 < imm;
```

Immediate range: -0x800 through 0x7ff

Note the negative values indicated for the immediate range are two's complement: this instruction uses them in an unsigned context, so -0x800 through -0x001 can be thought of as +0xfffff800 through +0xffffffff for the comparison.

#### <span id="page-243-1"></span>**[srai](#page-237-1)**

Shift right, arithmetic, immediate.

Usage:

```
srai rd, rs1, imm
```

Operation:

```
rd <= $signed(rs1) >>> imm;
```

Immediate range: 0 through 31.

Compressible if: rd matches rs1, registers are in x8 through x15.

#### <span id="page-243-2"></span>**[srli](#page-237-1)**

Shift right, logical, immediate.

Usage:

```
srli rd, rs1, imm
```

Operation:

```
rd <= rs1 >> imm;
```

Immediate range: 0 through 31.

Compressible if: rd matches rs1, registers are in x8 through x15.

<span id="page-244-2"></span>**[xori](#page-237-1)**

Bitwise XOR register with immediate.

Usage:

```
xori rd, rs1, imm
not rd, rs1 // pseudo: imm is -1
```

Operation:

```
rd <= rs1 ^ imm;
```

Immediate range: -0x800 through 0x7ff

Compressible if: rd matches rs1, registers are in x8 - x15, and immediate is -1 (aka c.not)

#### <span id="page-244-3"></span>**3.8.1.9. RV32I: Base ISA (Large Immediate)**

These instructions are the first in a two-instruction sequence to materialise a 32-bit constant, or a 32-bit offset from pc.

<span id="page-244-0"></span>**[auipc](#page-237-1)**

Add upper immediate to program counter.

Usage:

```
auipc rd, imm
```

Operation:

```
rd <= pc + (imm << 12);
```

Immediate range: -0x80000 through 0x7ffff.

Note -0x80000 through -0x00001 are equivalent to 0x80000 through 0xfffff after the left shift (**on RV32 only**) and the assembler may also accept these positive values.

<span id="page-244-1"></span>**[lui](#page-237-1)**

Load upper immediate.

Usage:

```
lui rd, imm
```

Operation:

```
rd <= imm << 12;
```

Immediate range: -0x80000 through 0x7ffff if 32-bit, or -0x20 through 0x1f if 16-bit.

Compressible if: rd is neither zero nor sp, and imm is nonzero in the range -0x20 through 0x1f.

Note -0x80000 through -0x00001 are equivalent to 0x80000 through 0xfffff after the left shift (**on RV32 only**) and the assembler may also accept these positive values.

#### <span id="page-245-3"></span>**3.8.1.10. RV32I: Base ISA (Control Transfer)**

These instructions modify the value of pc. When unmodified, pc increments by the size of the current instruction in bytes.

Conditional branches either modify or do not modify pc, based on a comparison between two registers. There is no flags register, however you can pass boolean conditions into branches by comparing a register with the zero register.

#### <span id="page-245-0"></span>**[beq](#page-237-1)**

Branch if equal.

Usage:

```
beq rs1, rs2, label
beqz rs1, label // pseudo: rs2 is zero
```

Operation:

```
if (rs1 == rs2)
  pc <= label;
```

Immediate range: even values in the range -0x1000 through 0x0ffe (±4 kB) if 32-bit, or -0x100 through 0x0fe (±256 B) if 16-bit.

Compressible if: rs2 is zero, and immediate is in the range -0x100 through 0x0fe (aka c.beqz).

#### <span id="page-245-1"></span>**[bge](#page-237-1)**

Branch if greater than or equal (signed).

Usage:

```
bge rs1, rs2, label
bgez rs1, label // pseudo: rs2 is zero
ble rs2, rs1, label // pseudo: operands swapped by assembler
blez rs2, label // pseudo: rs1 is zero
```

Operation:

```
if ($signed(rs1) >= $signed(rs2))
  pc <= label;
```

<span id="page-245-2"></span>Immediate range: even values in the range -0x1000 through 0x0ffe (±4 kB)

#### **[bgeu](#page-237-1)**

Branch if less than or equal (unsigned).

Usage:

```
bgeu rs1, rs2, label
bleu rs2, rs1, label // pseudo: operands swapped by assembler
```

Operation:

```
if (rs1 >= rs2)
  pc <= label;
```

Immediate range: even values in the range -0x1000 through 0x0ffe (±4 kB)

<span id="page-246-0"></span>**[blt](#page-237-1)**

Branch if less than (signed).

Usage:

```
blt rs1, rs2, label
bltz rs1, label // pseudo: rs2 is zero
bgt rs2, rs1, label // pseudo: operands swapped by assembler
bgtz rs2, label // pseudo: rs1 is zero
```

Operation:

```
if ($signed(rs1) < $signed(rs2))
  pc <= label;
```

Immediate range: even values in the range -0x1000 through 0x0ffe (±4 kB)

<span id="page-246-1"></span>**[bltu](#page-237-1)**

Branch if less than (unsigned).

Usage:

```
bltu rs1, rs2, label
bgtu rs2, rs1, label // pseudo: operands swapped by assembler
```

Operation:

```
if (rs1 < rs2)
  pc <= label;
```

Immediate range: even values in the range -0x1000 through 0x0ffe (±4 kB)

<span id="page-246-2"></span>**[bne](#page-237-1)**

Branch if not equal.

Usage:

```
bne rs1, rs2, label
bnez rs1, label // pseudo: rs2 is zero
```

Operation:

```
if (rs1 != rs2)
  pc <= label;
```

Immediate range: even values in the range -0x1000 through 0x0ffe (±4 kB) if 32-bit, or -0x100 through 0x0fe (±256 B) if 16-bit.

Compressible if: rs2 is zero, and immediate is in the range -0x100 through 0x0fe (aka c.bnez).

<span id="page-247-0"></span>**[jal](#page-237-1)**

Jump and link, pc-relative.

Usage:

```
jal rd, label
jal label // pseudo: rd is ra
j label // pseudo: rd is zero
```

Operation:

```
rd <= pc + 4; // or +2 if opcode is 16-bit
pc <= label;
```

Immediate range: even values in the range -0x100000 through 0x0ffffe (±1 MB) if 32-bit, or -0x800 through 0x7fe (±2 kB) if 16-bit.

Compressible if: rd is zero or ra, and immediate is in the range -0x800 through 0x7fe.

<span id="page-247-1"></span>**[jalr](#page-237-1)**

Jump and link, register-offset.

Usage:

```
jalr rd, rs1, imm // (imm is implicitly 0 if omitted.)
jalr rd, imm(rs1) // alternate syntax. (imm is implicitly 0 if omitted.)
jalr rs1, imm // pseudo: rd is ra. (imm is implicitly 0 if omitted.)
jalr imm(rs1) // pseudo: rd is ra. (imm is implicitly 0 if omitted.)
jr rs1, imm // pseudo: rd is zero. (imm is implicitly 0 if omitted.)
jr imm(rs1) // pseudo: rd is zero. (imm is implicitly 0 if omitted.)
ret // pseudo for jr ra
```

Operation:

```
rd <= pc + 4; // or +2 if opcode is 16-bit
pc <= rs1 + imm;
```

Immediate range: -0x800 through 0x7ff.

Compressible if: rd is zero or ra, immediate is zero, and rs1 is not zero.

#### <span id="page-248-2"></span>**3.8.1.11. RV32I: Base ISA (Load and Store)**

These instructions transfer data between memory and core registers. The register operand rs1 and immediate imm are added to form the address. Stores write register operand rs2 into memory, and loads read from memory into the destination register rd.

All load and store instructions to naturally aligned addresses on RISC-V are **single-copy atomic**. This means a naturallyaligned load does not observe byte tearing between the values that a memory location held before and after any naturally-aligned store to that location. Equivalently, all bytes covered by a single naturally-aligned load or store instruction transfer in a single transaction with the memory subsystem.

Hazard3 raises an exception on a load or store to a non-naturally-aligned address. See [Section 3.8.4.1](#page-283-0) for an exhaustive list of exception causes.

<span id="page-248-0"></span>**[lb](#page-237-1)**

Load signed byte from memory.

Usage:

```
lb rd, imm(rs1)
lb rd, (rs1) // imm is implicitly 0 if omitted.
```

Operation:

```
reg [31:0] addr;
addr = rs1 + imm;
if (bus_fault(addr)) begin
  raise_exception(4'h5); // Cause = load fault
end else begin
  rd <= {
  {24{mem[addr][7]}}, // Sign-extend
  mem[addr]
  };
end
```

Immediate range: -0x800 through 0x7ff for 32-bit, or 0x0 through 0x3 for 16-bit.

<span id="page-248-1"></span>**[lbu](#page-237-1)**

Load unsigned byte from memory.

Usage:

```
lbu rd, imm(rs1)
lbu rd, (rs1) // imm is implicitly 0 if omitted.
```

Operation:

```
reg [31:0] addr;
addr = rs1 + imm;
if (bus_fault(addr)) begin
  raise_exception(4'h5); // Cause = load fault
end else begin
  rd <= {
  24'h000000, // Zero-extend
  mem[addr]
  };
end
```

Immediate range: -0x800 through 0x7ff for 32-bit, or 0x0 through 0x3 for 16-bit.

Compressible if: rd and rs1 are in x8 through x15, and immediate is in the range 0x0 through 0x3.

<span id="page-249-0"></span>**[lh](#page-237-1)**

Load signed halfword from memory.

Usage:

```
lh rd, imm(rs1)
lh rd, (rs1) // imm is implicitly 0 if omitted.
```

Operation:

```
reg [31:0] addr;
addr = rs1 + imm;
if (addr[0]) begin
  raise_exception(4'h4); // Cause = unaligned load
end else if (bus_fault(addr)) begin
  raise_exception(4'h5); // Cause = load fault
end else begin
  rd <= {
  {16{mem[addr + 1][7]}}, // Sign-extend
  mem[addr + 1],
  mem[addr]
  };
end
```

Immediate range: -0x800 through 0x7ff for 32-bit, or even values in the range 0x0 through 0x2 for 16-bit.

Compressible if: rd and rs1 are in x8 through x15, and immediate is 0x0 or 0x2.

<span id="page-249-1"></span>**[lhu](#page-237-1)**

Load unsigned halfword from memory.

Usage:

```
lhu rd, imm(rs1)
lhu rd, (rs1) // imm is implicitly 0 if omitted.
```

Operation:

```
reg [31:0] addr;
addr = rs1 + imm;
if (addr[0]) begin
  raise_exception(4'h4); // Cause = unaligned load
end else if (bus_fault(addr)) begin
  raise_exception(4'h5); // Cause = load fault
end else begin
  rd <= {
  16'h0000, // Zero-extend
  mem[addr + 1],
  mem[addr]
  };
end
```

Immediate range: -0x800 through 0x7ff for 32-bit, or even values in the range 0x0 through 0x2 for 16-bit.

Compressible if: rd and rs1 are in x8 through x15, and immediate is 0x0 or 0x2.

<span id="page-250-0"></span>**[lw](#page-237-1)**

Load word from memory.

Usage:

```
lw rd, imm(rs1)
lw rd, (rs1) // imm is implicitly 0 if omitted.
```

Operation:

```
reg [31:0] addr;
addr = rs1 + imm;
if (addr[1:0]) begin
  raise_exception(4'h4); // Cause = unaligned load
end else if (bus_fault(addr)) begin
  raise_exception(4'h5); // Cause = load fault
end else begin
  rd <= {
  mem[addr + 3], // Note little-endian;
  mem[addr + 2], // MSBs are highest address
  mem[addr + 1],
  mem[addr]
  };
end
```

Immediate range: -0x800 through 0x7ff for 32-bit, smaller for 16-bit.

Compressible if:

- rd and rs1 are in x8 x15, and immediate is a multiple of four in the range -0x40 through 0x3c (aka c.lw)
- rd is not zero, rs1 is sp, and immediate is a multiple of four in the range 0x00 through 0xfc (aka c.lwsp)

<span id="page-250-1"></span>**[sb](#page-237-1)**

Store byte to memory.

Usage:

```
sb rs2, imm(rs1)
sb rs2, (rs1) // imm is implicitly 0 if omitted.
```

```
reg [31:0] addr;
addr = rs1 + imm;
if (bus_fault(addr)) begin
  raise_exception(4'h7); // Cause = store/AMO fault
end else begin
  mem[addr] <= rs2[7:0];
end
```

Immediate range: -0x800 through 0x7ff for 32-bit, or 0x0 through 0x3 for 16-bit.

Compressible if: rd and rs1 are in x8 through x15, and immediate is in the range 0x0 through 0x3.

<span id="page-251-0"></span>**[sh](#page-237-1)**

Store halfword to memory.

Usage:

```
sh rs2, imm(rs1)
sh rs2, (rs1) // imm is implicitly 0 if omitted.
```

Operation:

```
reg [31:0] addr;
addr = rs1 + imm;
if (addr[0]) begin
  raise_exception(4'h6); // Cause = unaligned store/AMO
end else if (bus_fault(addr)) begin
  raise_exception(4'h7); // Cause = store/AMO fault
end else begin
  mem[addr] <= rs2[7:0];
  mem[addr + 1] <= rs2[15:8];
end
```

Immediate range: -0x800 through 0x7ff for 32-bit, or even values in the range 0x0 through 0x2 for 16-bit.

Compressible if: rd and rs1 are in x8 through x15, and immediate is 0x0 or 0x2.

<span id="page-251-1"></span>**[sw](#page-237-1)**

Store word to memory.

Usage:

```
sw rs2, imm(rs1)
sw rs2, (rs1) // imm is implicitly 0 if omitted.
```

Operation:

```
reg [31:0] addr;
addr = rs1 + imm;
if (addr[1:0]) begin
  raise_exception(4'h6); // Cause = unaligned store/AMO
end else if (bus_fault(addr)) begin
  raise_exception(4'h7); // Cause = store/AMO fault
end else begin
  mem[addr] <= rs2[7:0];
  mem[addr + 1] <= rs2[15:8];
  mem[addr + 2] <= rs2[23:16];
  mem[addr + 3] <= rs2[31:24];
end
```

Immediate range: -0x800 through 0x7ff for 32-bit, smaller for 16-bit.

Compressible if:

- rs1 and rs2 are in x8 x15, and immediate is a multiple of four in the range -0x40 through 0x3c (aka c.sw)
- rs2 is not zero, rs1 is sp, and immediate is a multiple of four in the range 0x00 through 0xfc (aka c.swsp)

#### <span id="page-252-2"></span>**3.8.1.12. M: Multiply and Divide**

These instructions implement integer multiply, divide and modulo.

<span id="page-252-0"></span>**[div](#page-237-1)**

Divide (signed).

Usage:

```
div rd, rs1, rs2
```

Operation:

```
if (rs2 == 32'h0)
  rd <= 32'hffffffff; // Defined for division by zero
else if (rs1 == 32'h80000000 && rs2 == 32'hffffffff)
  rd <= 32'h80000000; // Defined for signed overflow
else
  rd <= $signed(rs1) / $signed(rs2); // Sign of rd is XOR of signs
```

<span id="page-252-1"></span>**[divu](#page-237-1)**

Divide (unsigned).

Usage:

```
divu rd, rs1, rs2
```

Operation:

```
if (rs2 == 32'h0)
  rd <= 32'hffffffff; // Defined for division by zero
else
  rd <= rs1 / rs2;
```

#### <span id="page-253-0"></span>**[mul](#page-237-1)**

Multiply 32 × 32 → 32.

Usage:

```
mul rd, rs1, rs2
```

Operation:

```
rd <= rs1 * rs2;
```

Compressible if: rd matches rs1, registers are in x8 through x15.

#### <span id="page-253-1"></span>**[mulh](#page-237-1)**

Multiply signed (32) by signed (32), return upper 32 bits of the 64-bit result.

Usage:

```
mulh rd, rs1, rs2
```

Operation:

```
// Both operands are sign-extended to 64 bits:
reg [63:0] result_full;
result_full = {{32{rs1[31]}}, rs1} * {{32{rs2[31]}}, rs2};
rd <= result_full[63:32];
```

#### <span id="page-253-2"></span>**[mulhsu](#page-237-1)**

Multiply signed (32) by unsigned (32), return upper 32 bits of the 64-bit result.

Usage:

```
mulhsu rd, rs1, rs2
```

Operation:

```
// rs1 is sign-extended, rs2 is zero-extended:
reg [63:0] result_full;
result_full = {{32{rs1[31}}, rs1} * {32'h00000000, rs2};
rd <= result_full[63:32];
```

#### **[mulhu](#page-237-1)**

Multiply unsigned (32) by unsigned (32), return upper 32 bits of the 64-bit result.

Usage:

```
mulhu rd, rs1, rs2
```

Operation:

```
// Both operands are zero-extended to 64 bits:
reg [63:0] result_full;
result_full = {32'h00000000, rs1} * {32'h00000000, rs2};
rd <= result_full[63:32];
```

<span id="page-254-0"></span>**[rem](#page-237-1)**

Remainder (signed).

Usage:

```
rem rd, rs1, rs2
```

Operation:

```
if (rs2 == 32'h0)
  rd <= rs1; // Defined for division by zero
else
  rd <= $signed(rs1) % $signed(rs2); // Sign of rd is sign of rs1
```

<span id="page-254-1"></span>**[remu](#page-237-1)**

Remainder (unsigned).

Usage:

```
remu rd, rs1, rs2
```

Operation:

```
if (rs2 == 32'h0)
  rd <= rs1;
else
  rd <= rs1 % rs2;
```

#### <span id="page-254-2"></span>**3.8.1.13. A: Atomics**

These instructions help software to safely and concurrently modify shared variables. They fall into two groups:

- lr.w and sc.w, load-reserved and store-conditional instructions, which allow software to safely perform read-modifywrite operations on shared variables by looping until success
- amo\*.w instructions (atomic memory operations or AMOs), which atomically modify a memory location and return the value it held immediately prior to modification

The pseudocode in this section references the 1-bit global variable local\_monitor\_valid. It is true when the hart has:

- previously completed a successful AHB5 exclusive read
- not attempted an exclusive write since the read
- not been interrupted or taken an exception since the read (*implementation-defined behaviour*)

The pseudocode maintains this invariant over the local\_monitor\_valid flag. This flag helps the hart maintain atomicity of its read-modify-write sequences with respect to its own interrupts. Hardware refuses to perform exclusive writes when the local monitor flag is not set.

AMOs clear the local monitor state even when bailing out during the read phase, since even in this case you have attempted to execute an instruction which performs an exclusive write. In an lr.w, sc.w sequence with an AMO executed in between, the sc.w always fails.

Hazard3 builds its atomic shared memory implementation on top of AHB5 exclusive accesses. The following tasks, used throughout this section, represent AHB5 32-bit exclusive reads and writes:

```
// Read 32 bits from memory and return reservation success/fail according to
// global monitor. Set local monitor bit if the reservation succeeded.
task exclusive_read_32;
  input [31:0] addr;
  output [31:0] data;
  output exclusive_ok;
begin
  data = {
  mem[addr + 3],
  mem[addr + 2],
  mem[addr + 1],
  mem[addr]
  };
  local_monitor_valid = global_monitor_read(addr);
  exclusive_ok = local_monitor_valid;
end
endtask
// Attempt to write 32 bits to memory, and return write success/fail according
// to global monitor. Always clear the local monitor flag.
task exclusive_write_32;
  input [31:0] addr;
  input [31:0] data;
  output exclusive_ok;
begin
  if (!local_monitor_valid) begin
  exclusive_ok = 0; // Write refused by local monitor
  end else if (global_monitor_write(addr)) begin
  exclusive_ok = 1; // Write succeeds
  mem[addr + 3] <= data[31:24];
  mem[addr + 2] <= data[23:16];
  mem[addr + 1] <= data[15: 8];
  mem[addr + 0] <= data[ 7: 0];
  end else begin
  exclusive_ok = 0; // Write refused by global monitor
  end
  local_monitor_valid = 0; // Always clear local monitor
end
```

```
endtask
```

The functions global\_monitor\_read(addr); and global\_monitor\_write(addr); in the above code return the global monitor response for an exclusive read or write to this address, following the rules laid out in [Section 2.1.6](#page-28-0). The global monitor enforces atomicity of this hart's read-modify-write sequences with respect to other harts sharing the same memory.

Because Hazard3 implements an AMO as a hardware-sequenced read-modify-write retry loop using AHB5 exclusives, the hardware promotes a read reservation failure during an AMO to a store/AMO fault exception (mcause = 7). This behaviour avoids an infinite loop when accessing locations which do not support exclusive access.

The following local variables are common to all AMO pseudocode:

```
reg done = 0;
reg exclusive_success;
reg [31:0] tmp;
```

#### <span id="page-256-0"></span>**[amoadd.w](#page-237-1)**

Atomically add register to memory and return original memory value.

Usage:

```
amoadd.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
  done = 1;
end
while (!done) begin
  exclusive_read_32(rs1, tmp, exclusive_success);
  if (!exclusive_success || bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
  done = 1;
  end else begin
  tmp = tmp + rs2;
  exclusive_write_32(rs1, tmp, done);
  end
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-256-1"></span>**[amoand.w](#page-237-1)**

Atomically bitwise AND register into memory. Return original memory value.

Usage:

```
amoand.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
  done = 1;
end
while (!done) begin
  exclusive_read_32(rs1, tmp, exclusive_success);
  if (!exclusive_success || bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
  done = 1;
  end else begin
  tmp = tmp & rs2;
  exclusive_write_32(rs1, tmp, done);
  end
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-257-0"></span>**[amomax.w](#page-237-1)**

Atomically: check if register is signed-greater-than memory value, and write to memory if true. Return original memory value.

Usage:

```
amomax.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
  done = 1;
end
while (!done) begin
  exclusive_read_32(rs1, tmp, exclusive_success);
  if (!exclusive_success || bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
  done = 1;
  end else begin
  tmp = $signed(tmp) < $signed(rs2) ? rs2 : tmp;
  exclusive_write_32(rs1, tmp, done);
  end
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-257-1"></span>**[amomaxu.w](#page-237-1)**

Atomically: check if register is unsigned-greater-than memory value, and write to memory if so. Return original memory value.

Usage:

```
amomaxu.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
  done = 1;
end
while (!done) begin
  exclusive_read_32(rs1, tmp, exclusive_success);
  if (!exclusive_success || bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
  done = 1;
  end else begin
  tmp = tmp < rs2 ? rs2 : tmp;
  exclusive_write_32(rs1, tmp, done);
  end
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-258-0"></span>**[amomin.w](#page-237-1)**

Atomically: check if register is signed-less-than memory value, and write to memory if so. Return original memory value.

Usage:

```
amomin.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
  done = 1;
end
while (!done) begin
  exclusive_read_32(rs1, tmp, exclusive_success);
  if (!exclusive_success || bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
  done = 1;
  end else begin
  tmp = $signed(tmp) < $signed(rs2) ? tmp : rs2;
  exclusive_write_32(rs1, tmp, done);
  end
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-258-1"></span>**[amominu.w](#page-237-1)**

Atomically: check if register is unsigned-less-than memory value, and write to memory if so. Return original memory value.

Usage:

```
amominu.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
  done = 1;
end
while (!done) begin
  exclusive_read_32(rs1, tmp, exclusive_success);
  if (!exclusive_success || bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
  done = 1;
  end else begin
  tmp = tmp < rs2 ? tmp : rs2;
  exclusive_write_32(rs1, tmp, done);
  end
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-259-0"></span>**[amoor.w](#page-237-1)**

Atomically bitwise OR register into memory. Return original memory value.

Usage:

```
amoor.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
  done = 1;
end
while (!done) begin
  exclusive_read_32(rs1, tmp, exclusive_success);
  if (!exclusive_success || bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
  done = 1;
  end else begin
  tmp = tmp | rs2;
  exclusive_write_32(rs1, tmp, done);
  end
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-259-1"></span>**[amoswap.w](#page-237-1)**

Atomically: write a value to memory, and return the value the memory location held immediately prior to the write.

Usage:

```
amoswap.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
  done = 1;
end
while (!done) begin
  exclusive_read_32(rs1, tmp, exclusive_success);
  if (!exclusive_success || bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
  done = 1;
  end else begin
  exclusive_write_32(rs1, rs2, done);
  end
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-260-0"></span>**[amoxor.w](#page-237-1)**

Atomically bitwise OR register into memory. Return original memory value.

Usage:

```
amoxor.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
  done = 1;
end
while (!done) begin
  exclusive_read_32(rs1, tmp, exclusive_success);
  if (!exclusive_success || bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
  done = 1;
  end else begin
  exclusive_write_32(rs1, rs2, done);
  end
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-260-1"></span>**[lr.w](#page-237-1)**

Load a value from memory and make a reservation with the global monitor. Set local monitor bit according to reservation success.

Usage:

```
lr.w rd, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h4); // Cause: load align
end else if (bus_fault(rs1)) begin
  raise_exception(4'h5); // Cause: load fault
end else begin
  read_exclusive_32(rs1, tmp, local_monitor_valid);
  rd <= tmp;
end
```

#### <span id="page-261-1"></span>**[sc.w](#page-237-1)**

Conditionally store a value to memory. Succeed if reservation is valid at both local and global monitor. Return 1 for failure, 0 for success.

Usage:

```
sc.w rd, rs2, (rs1)
```

Operation:

```
if (rs1[1:0]) begin
  raise_exception(4'h6); // Cause: store/AMO align
end else if (bus_fault(addr)) begin
  raise_exception(4'h7); // Cause: store/AMO fault
end else if (!local_monitor_valid) begin
  rd <= 1; // Refused by local monitor
end else begin
  write_exclusive_32(rs1, rs2, exclusive_success);
  rd <= !exclusive_success;
end
local_monitor_valid = 0; // Always clear local monitor
```

#### <span id="page-261-0"></span>**3.8.1.14. C: Compressed Instructions**

All instructions in the C extension are 16-bit aliases of 32-bit instructions from other extensions. In the case of Hazard3, which lacks the F extension, these are all aliases of base I instructions. They behave identically to their 32-bit counterparts.

C adds compressed aliases for the following instructions from RV32I:

| Alphabetical order: left-to-right, then top-to-bottom. |      |      |      |     |     |
|--------------------------------------------------------|------|------|------|-----|-----|
| add                                                    | addi | and  | andi | beq | bne |
| ebreak                                                 | jal  | jalr | lui  | lw  | or  |
| slli                                                   | srai | srli | sub  | sw  | xor |

See the per-instruction documentation for the compression limitations of each instruction. The assembler automatically uses compressed variants when the limitations are met, and when the relevant compressed instruction extension is enabled for the assembler, for example by passing c in the -march ISA string.

The above also applies to Zca and Zcb: the former is an alias for the non-floating-point subset of C, and the latter adds 16 bit aliases for additional common instructions from the I, M and Zbb extensions. Each Zcmp instruction expands to a sequence of multiple instructions from the I extension.

#### [\(Return to index\)](#page-237-0)

#### <span id="page-262-3"></span>**3.8.1.15. Zba: Bit manipulation (address generation)**

These instructions accelerate address generation for arrays of 2, 4 and 8-byte elements. They can also multiply by constant values 3, 5 and 9 if that is more your style.

#### <span id="page-262-0"></span>**[sh1add](#page-237-1)**

Add, with the first addend shifted left by 1.

Usage:

```
sh1add rd, rs1, rs2
```

Operation:

```
rd <= (rs1 << 1) + rs2;
```

#### <span id="page-262-1"></span>**[sh2add](#page-237-1)**

Add, with the first addend shifted left by 2.

Usage:

```
sh2add rd, rs1, rs2
```

Operation:

```
rd <= (rs1 << 2) + rs2;
```

#### <span id="page-262-2"></span>**[sh3add](#page-237-1)**

Add, with the first addend shifted left by 3.

Usage:

```
sh3add rd, rs1, rs2
```

Operation:

```
rd <= (rs1 << 3) + rs2;
```

#### <span id="page-262-4"></span>**3.8.1.16. Zbb: Bit manipulation (basic)**

These instructions are useful for bitfield manipulation, and complex integer arithmetic, such as in soft floating point routines. Many of them substitute directly for common pairs of RV32I instructions, like zext.h → sll, srl.

#### <span id="page-263-0"></span>**[andn](#page-237-1)**

Bitwise AND with inverted second operand.

Usage:

```
andn rd, rs1, rs2
```

Operation:

```
rd <= rs1 & ~rs2;
```

<span id="page-263-1"></span>**[clz](#page-237-1)**

Count leading zeroes (starting from MSB, searching LSB-ward).

Usage:

```
clz rd, rs1
```

Operation:

```
rd <= 32; // Default = 32 if no set bits
reg found = 1'b0; // Local variable
for (i = 0; i < 32; i = i + 1) begin
  if (rs1[31 - i] && !found) begin
  found = 1'b1;
  rd <= i;
  end
end
```

#### <span id="page-263-2"></span>**[cpop](#page-237-1)**

Population count.

Usage:

```
cpop rd, rs1
```

Operation:

```
reg [5:0] sum = 6'd0; // Local variable
for (i = 0; i < 32; i = i + 1)
  sum = sum + rs1[i];
rd <= sum;
```

<span id="page-263-3"></span>**[ctz](#page-237-1)**

Count trailing zeroes (starting from LSB, searching MSB-ward).

Usage:

```
ctz rd, rs1
```

Operation:

```
rd <= 32; // Default = 32 if no set bits
reg found = 1'b0; // Local variable
for (i = 0; i < 32; i = i + 1) begin
  if (rs1[i] && !found) begin
  found = 1'b1;
  rd <= i;
  end
end
```

<span id="page-264-0"></span>**[max](#page-237-1)**

Maximum of two values (signed).

Usage:

```
max rd, rs1, rs2
```

Operation:

```
if ($signed(rs1) < $signed(rs2))
  rd <= rs2;
else
  rd <= rs1;
```

<span id="page-264-1"></span>**[maxu](#page-237-1)**

Maximum of two values (unsigned).

Usage:

```
maxu rd, rs1, rs2
```

Operation:

```
if (rs1 < rs2)
  rd <= rs2;
else
  rd <= rs1;
```

<span id="page-264-2"></span>**[min](#page-237-1)**

Minimum of two values (signed).

Usage:

```
min rd, rs1, rs2
```

Operation:

```
if ($signed(rs1) < $signed(rs2))
  rd <= rs1;
else
  rd <= rs2;
```

<span id="page-265-0"></span>**[minu](#page-237-1)**

Minimum of two values (unsigned).

Usage:

```
minu rd, rs1, rs2
```

Operation:

```
if (rs1 < rs2)
  rd <= rs1;
else
  rd <= rs2;
```

<span id="page-265-1"></span>**[orc.b](#page-237-1)**

OR-combine of bits within each byte. Generates a mask of nonzero bytes.

Usage:

```
orc.b rd, rs1
```

Operation:

```
rd <= {
  {8{|rs1[31:24]}},
  {8{|rs1[23:16]}},
  {8{|rs1[15:8]}},
  {8{|rs1[7:0]}}
};
```

<span id="page-265-2"></span>**[orn](#page-237-1)**

Bitwise OR with inverted second operand.

Usage:

```
orn rd, rs1, rs2
```

```
rd <= rs1 | ~rs2;
```

<span id="page-266-0"></span>**[rev8](#page-237-1)**

Reverse bytes within word.

Usage:

```
rev8 rd, rs1
```

Operation:

```
rd <= {
  rs1[7:0],
  rs1[15:8],
  rs1[23:16],
  rs1[31:24]
};
```

<span id="page-266-1"></span>**[rol](#page-237-1)**

Rotate left by register, modulo 32.

Usage:

```
rol rd, rs1, rs2
```

Operation:

```
rd <= ({rs1, rs1} << rs2[4:0]) >> 32;
```

<span id="page-266-2"></span>**[ror](#page-237-1)**

Rotate right by register, modulo 32.

Usage:

```
ror rd, rs1, rs2
```

Operation:

```
rd <= {rs1, rs1} >> rs2[4:0];
```

<span id="page-267-0"></span>**[rori](#page-237-1)**

Rotate right by immediate.

Usage:

```
rori rd, rs1, imm
```

Operation:

```
rd <= {rs1, rs1} >> imm;
```

Immediate range: 0 through 31.

#### <span id="page-267-1"></span>**[sext.b](#page-237-1)**

Sign-extend from byte.

Usage:

```
sext.b rd, rs1
```

Operation:

```
rd <= {
  {24{rs1[7]}},
  rs1[7:0]
};
```

Compressible if: rd matches rs1, and registers are in x8 - x15.

#### <span id="page-267-2"></span>**[sext.h](#page-237-1)**

Sign-extend from halfword.

Usage:

```
sext.h rd, rs1
```

Operation:

```
rd <= {
  {16{rs1[15]}},
  rs1[15:0]
};
```

<span id="page-267-3"></span>Compressible if: rd matches rs1, and registers are in x8 - x15.

**[xnor](#page-237-1)**

Bitwise XOR with inverted operand. Equivalently, bitwise NOT of bitwise XOR.

Usage:

```
xnor rd, rs1, rs2
```

Operation:

```
rd <= rs1 ^ ~rs2;
```

#### <span id="page-268-0"></span>**[zext.b](#page-237-1)**

Zero-extend from byte.

Usage:

```
zext.b rd, rs1
```

Operation:

```
rd <= {
  24'h000000,
  rs1[7:0]
};
```

Compressible if: rd matches rs1, and registers are in x8 - x15.

The 32-bit opcode for zext.b is a pseudo-instruction for [andi.](#page-241-1) However, the compressed variant is a dedicated instruction from Zcb. It is not actually a part of Zbb, but is documented here for grouping with the other sext./zext instructions.

#### <span id="page-268-1"></span>**[zext.h](#page-237-1)**

Zero-extend from halfword.

Usage:

```
zext.h rd, rs1
```

Operation:

```
rd <= {
  16'h0000,
  rs1[15:0]
};
```

Compressible if: rd matches rs1, and registers are in x8 - x15.

#### <span id="page-269-4"></span>**3.8.1.17. Zbs: Bit manipulation (single-bit)**

These instructions invert, set, clear and extract single bits in a register.

#### <span id="page-269-0"></span>**[bclr](#page-237-1)**

Clear single bit.

Usage:

```
bclr rd, rs1, rs2
```

Operation:

```
rd <= rs1 & ~(32'h1 << rs2[4:0]);
```

#### <span id="page-269-1"></span>**[bclri](#page-237-1)**

Clear single bit (immediate).

Usage:

```
bclri rd, rs1, imm
```

Operation:

```
rd <= rs1 & ~(32'h1 << imm);
```

Immediate range: 0 through 31.

#### <span id="page-269-2"></span>**[bext](#page-237-1)**

Extract single bit.

Usage:

```
bext rd, rs1, rs2
```

Operation:

```
rd <= (rs1 >> rs2[4:0]) & 32'h1;
```

#### <span id="page-269-3"></span>**[bexti](#page-237-1)**

Extract single bit (immediate).

Usage:

```
bexti rd, rs1, imm
```

```
rd <= (rs1 >> imm) & 32'h1;
```

Immediate range: 0 through 31.

#### <span id="page-270-0"></span>**[binv](#page-237-1)**

Invert single bit.

Usage:

```
binv rd, rs1, rs2
```

Operation:

```
rd <= rs1 ^ (32'h1 << rs2[4:0]);
```

#### <span id="page-270-1"></span>**[binvi](#page-237-1)**

Invert single bit (immediate).

Usage:

```
binvi rd, rs1, imm
```

Operation:

```
rd <= rs1 ^ (32'h1 << imm);
```

Immediate range: 0 through 31.

#### <span id="page-270-2"></span>**[bset](#page-237-1)**

Set single bit.

Usage:

```
bset rd, rs1, rs2
```

Operation:

```
rd <= rs1 | (32'h1 << rs2[4:0])
```

#### <span id="page-270-3"></span>**[bseti](#page-237-1)**

Set single bit (immediate).

Usage:

```
bseti rd, rs1, imm
```

```
rd <= rs1 | (32'h1 << imm);
```

Immediate range: 0 through 31.

#### <span id="page-271-3"></span>**3.8.1.18. Zbkb: Basic bit manipulation for cryptography**

Zbkb has a large overlap with Zbb (basic bit manipulation). This section covers instructions in Zbkb but not in Zbb.

#### <span id="page-271-0"></span>**[brev8](#page-237-1)**

Bit-reverse within each byte.

Usage:

```
brev8 rd, rs1
```

Operation:

```
for (i = 0; i < 32; i = i + 8) begin
  for (j = 0; j < 8; j = j + 1) begin
  rd[i + j] <= rs1[i + (7 - j)];
  end
end
```

#### <span id="page-271-1"></span>**[pack](#page-237-1)**

Pack two halfwords into one word.

Usage:

```
pack rd, rs1, rs2
```

Operation:

```
rd <= {
  rs2[15:0],
  rs1[15:0]
};
```

#### <span id="page-271-2"></span>**[packh](#page-237-1)**

Pack two bytes into one halfword.

Usage:

```
packh rd, rs1, rs2
```

```
rd <= {
  16'h0000,
  rs2[7:0],
  rs1[7:0]
};
```

#### <span id="page-272-1"></span>**[unzip](#page-237-1)**

Deinterleave odd/even bits of register into upper/lower half of result.

Usage:

```
unzip rd, rs1
```

Operation:

```
for (i = 0; i < 32; i = i + 2) begin
  rd[i / 2] <= rs1[i];
  rd[i / 2 + 16] <= rs1[i + 1];
end
```

#### <span id="page-272-2"></span>**[zip](#page-237-1)**

Interleave upper/lower half of register into odd/even bits of result.

Usage:

```
zip rd, rs1
```

Operation:

```
for (i = 0; i < 32; i = i + 2) begin
  rd[i] <= rs1[i / 2];
  rd[i + 1] <= rs1[i / 2 + 16];
end
```

#### <span id="page-272-0"></span>**3.8.1.19. Zcb: Additional Basic Compressed Instructions**

Zcb adds 16-bit compressed aliases for the following instructions from the I, M and Zbb extensions:

| Alphabetical order: left-to-right, then top-to-bottom. |    |     |     |     |    |  |
|--------------------------------------------------------|----|-----|-----|-----|----|--|
| lbu                                                    | lh | lhu | mul | not | sb |  |

| Alphabetical order: left-to-right, then top-to-bottom. |        |    |        |        |  |
|--------------------------------------------------------|--------|----|--------|--------|--|
| sext.b                                                 | sext.h | sh | zext.b | zext.h |  |

See per-instruction documentation for the compressibility limitations for each instruction.

[\(Return to index\)](#page-237-0)

#### <span id="page-273-0"></span>**3.8.1.20. Zcmp: Compressed Push, Pop and Double Move**

Zcmp adds 16-bit instructions which expand to common sequences of 32-bit RV32I instructions used in function prologues and epilogues. The following is a rough description of the available instructions:

- cm.push: allocates a stack frame and saves registers.
  - Push ra onto the stack.
  - Optionally push a number of the s0 through s11 saved registers, consecutively up from s0.
  - Round the total stack decrement to a multiple of 16 bytes, to maintain stack alignment if already aligned.
  - Decrement the stack pointer by up to 48 additional bytes, in multiples of 16 bytes, to allocate additional frame space.
  - There are twelve s\* registers, and you can push any number of them *except for eleven.* If you need to push more than ten s\* registers, push twelve.
- cm.pop: reverse of cm.push. Deallocates a stack frame and restores ra, optionally s0 through s11.
- cm.popret: equivalent to cm.pop followed by ret. Deallocates a stack frame, restores saved registers, and returns.
- cm.popretz: equivalent to cm.pop; li a0, 0; ret. It is common for functions to return a constant 0.
- cm.mvsa01: move a0 and a1 into any two registers in the range s0 through s7. Used to save arguments over embedded calls.
- cm.mva01s: move into a0 and a1, from any two registers in s0 through s7. Used to restore saved arguments.

See [Section 3.8.1.1](#page-233-2) for a link to the Zcmp specification which covers key details such as stack layout and atomicity with respect to interrupts. See [Section 3.8.7](#page-296-0) for cycle counts for these instructions on Hazard3.

[\(Return to index\)](#page-237-0)

#### <span id="page-273-2"></span>**3.8.1.21. RV32I and Zifencei: Memory Ordering Instructions**

These instructions control observed memory ordering of loads and stores in multi-hart systems. They also enforce when a hart's instruction fetch observes its own stores.

#### <span id="page-273-1"></span>**[fence](#page-237-1)**

Constrain the position of this hart's accesses in the total memory order, according to this hart's program order.

Usage:

```
  // <set> is a nonempty string which matches the regex i?o?r?w?
fence <set>, <set> // predecessor, successor
fence // pseudo: fence iorw, iorw
fence.tso // variant of fence rw, rw; see below
```

Operation: Hazard3 has no store buffer, and assumes the memory subsystem is sequentially consistent. Therefore no additional book-keeping is required to enforce ordering on shared memory, and this instruction executes as a noop. (The SDK still uses fence instructions, and the ordered variants of amo\*.w, for portability across platforms which

take advantage of relaxed memory ordering.)

Nominally a fence enforces that the **predecessor** set appears before the **successor** set in the total memory order. These sets respectively contain the hart's memory accesses before and after the fence instruction in program order, and are further filtered by a 4-bit mask each:

- Device input (I)
- Device output (O)
- Read (R)
- Write (W)

The fence.tso (total store order) variant is equivalent to fence rw, rw except that it does not enforce write-beforeread ordering.

#### <span id="page-274-0"></span>**[fence.i](#page-237-1)**

Instruction fence. Ensure subsequent instruction fetches on this hart observe this hart's previous stores.

Usage:

```
fence.i
```

#### Operation:

- 1. Clear the branch target buffer [\(Section 3.8.7.10\)](#page-301-0)
- 2. Jump to the instruction at the sequentially-next address (pc + 4), to clear the prefetch buffer.

The prefetch buffer can reorder instruction fetch against stores which are earlier in program order. For example:

```
  la a0, label // get address for store instruction
  li a1, 0x9002 // get immediate value of c.ebreak
  div t1, t1, t1 // long-running instruction, fills prefetch buffer
  sh a1, (a0) // write to next address. (16-bit opcode)
label:
  nop // (16-bit opcode)
```

If you execute the above code on Hazard3, you may or may not get a breakpoint exception at label. The outcome depends on how many cycles the bus accesses take. This is permitted by the RISC-V memory model.

This case is generally only reachable on fall-through, because Hazard3 does not prefetch through control flow instructions except for the taken backward conditional branch currently allocated in the branch target buffer. In particular it does not prefetch through indirect branches like ret. You are unlikely to hit this issue in practice; however, be aware fence.i is the standard mechanism for solving this class of problem.

Hazard3 behaves unpredictably if you write to the address of a conditional branch instruction that is currently tagged in the branch target buffer, and then execute that conditional branch instruction without first executing a fence.i. Avoid this by always executing a fence.i between writing to memory and executing that same memory.

#### <span id="page-274-1"></span>**3.8.1.22. Zicsr: Control and Status Register Access**

These instructions access the control and status registers (CSRs) listed in [Section 3.8.9.](#page-304-0) A CSR instruction may read a CSR, modify a CSR, or simultaneously read and modify the same CSR. A modification consists of a normal write, an atomic bit-clear, or an atomic bit-set.

CSR addresses are in the range 0x000 through 0xfff (12 bits, 4096 possible CSRs). The CSR address is an immediate constant in the instruction, so you cannot index CSRs with runtime values. The assembler accepts numeric constants or

CSR names such as mstatus as CSR addresses.

#### <span id="page-275-0"></span>**[csrrc](#page-237-1)**

Simultaneously read and clear bits in a CSR.

Usage:

```
csrrc rd, <addr>, rs1
csrc <addr>, rs1 // pseudo: rd is zero
```

Operation:

```
rd <= csr[addr];
if (regnum_rs1 != 5'h00)
  csr[addr] <= csr[addr] & ~rs1;
```

#### <span id="page-275-1"></span>**[csrrci](#page-237-1)**

Simultaneously read and clear bits in a CSR, with an immediate value for the clear.

Usage:

```
csrrci rd, <addr>, imm
csrci <addr>, imm // pseudo: rd is zero
```

Operation:

```
rd <= csr[addr];
if (imm != 32'h0)
  csr[addr] <= csr[addr] & ~imm;
```

Immediate range: 0 through 31.

#### <span id="page-275-2"></span>**[csrrs](#page-237-1)**

Simultaneously read and set bits in a CSR.

Usage:

```
csrrs rd, <addr>, rs1
csrs <addr>, rs1 // pseudo: rd is zero
csrr rd, <addr> // pseudo: rs1 is zero
```

Operation:

```
rd <= csr[addr];
if (regnum_rs1 != 5'h00)
  csr[addr] <= csr[addr] | rs1;
```

#### **[csrrsi](#page-237-1)**

Simultaneously read and set bits in a CSR, with an immediate value for the set.

Usage:

```
csrrsi rd, <addr>, imm
csrsi <addr>, imm // pseudo: rd is zero
```

Operation:

```
rd <= csr[addr];
if (imm != 32'h0)
  csr[addr] <= csr[addr] | imm;
```

Immediate range: 0 through 31.

#### <span id="page-276-0"></span>**[csrrw](#page-237-1)**

Simultaneously read and write a CSR.

Usage:

```
csrrw rd, <addr>, rs1
csrw <addr>, rs1 // pseudo: rd is zero
```

Operation:

```
if (regnum_rd != 5'h00)
  rd <= csr[addr];
csr[addr] <= rs1;
```

#### <span id="page-276-1"></span>**[csrrwi](#page-237-1)**

Simultaneously read and write a CSR, with an immediate value for the write.

Usage:

```
csrrwi rd, <addr>, imm
csrwi <addr>, imm // pseudo: rd is zero
```

Operation:

```
if (regnum_rd != 5'h00)
  rd <= csr[addr];
csr[addr] <= imm;
```

Immediate range: 0 through 31.

#### <span id="page-277-3"></span>**3.8.1.23. Privileged Instructions**

These instructions are part of the trap and interrupt control support defined in the privileged ISA manual. The other part of this support is the CSRs ([Section 3.8.9\)](#page-304-0).

#### <span id="page-277-0"></span>**[ebreak](#page-237-1)**

Raise a breakpoint exception.

Usage:

```
ebreak
```

Operation:

```
raise_exception(4'h3); // Cause = ebreak
```

Compressible if: always.

Privilege requirements: any privilege level.

See [Section 3.8.4](#page-282-0) for details of the RISC-V trap entry sequence. All exceptions trap into M-mode on Hazard3. The exception program counter mepc points to the start of the ebreak instruction.

An external debug host can catch the execution of breakpoint instructions. If the core is in M-mode, and [DCSR](#page-326-0).EBREAKM is set, the core enters Debug mode instead of taking the exception. In U-mode, [DCSR.](#page-326-0)EBREAKU enables the same behaviour.

#### <span id="page-277-1"></span>**[ecall](#page-237-1)**

Environment call. Raise an exception to access a handler at a higher privilege level.

Usage:

```
ecall
```

Operation:

```
if (priv == 2'h3)
  raise_exception(4'hb); // Cause: Environment call from M-mode
else
  raise_exception(4'h8); // Cause: Environment call from U-mode
```

Privilege requirements: any privilege level.

See [Section 3.8.4](#page-282-0) for details of the RISC-V trap entry sequence. All exceptions trap into M-mode on Hazard3. The exception program counter mepc points to the start of the ecall instruction.

<span id="page-277-2"></span>**[mret](#page-237-1)**

Return from M-mode trap.

Usage:

```
mret
```

Operation: execute the trap return sequence described in [Section 3.8.4.](#page-282-0)

Privilege requirements: M-mode only.

<span id="page-278-1"></span>**[wfi](#page-237-1)**

Wait for interrupt.

Usage:

```
wfi
```

Operation: pause execution until the processor is interrupted, or enters Debug mode.

Privilege requirements: M-mode is always permitted. U-mode is permitted if [MSTATUS.](#page-309-0)TW is clear.

wfi ignores the global interrupt enable, [MSTATUS](#page-309-0).MIE. It respects all other interrupt controls. For example:

- If [MIP](#page-314-0).MEIP is 1, [MIE.](#page-311-0)MEIE is 1, and [MSTATUS.](#page-309-0)MIE is 0, a wfi instruction falls through immediately without pausing.
- In this example, setting [MSTATUS](#page-309-0).MIE to 1 would cause the core to immediately take the interrupt.
- If no bit is set in both [MIP](#page-314-0) and [MIE](#page-311-0), the wfi stalls until there is at least one such bit.

When a wfi is interrupted, the exception return address [MEPC](#page-313-0) points to the instruction following the wfi.

When the debugger halts the core during a wfi, [DPC](#page-327-0) points to the instruction immediately following the wfi instruction. wfi executes as a no-op under instruction single-stepping (it does not stall), and under Debug-mode execution in the Program Buffer.

Hazard3's [MSLEEP](#page-332-0) CSR controls additional power-saving measures the core can implement during a wfi sleep state.

### <span id="page-278-0"></span>**3.8.2. Memory Access**

Hazard3 accesses memory within a 4 GB (2<sup>32</sup> bytes) physical address space. There is no address translation. Each possible value of an integer register uniquely identifies a single byte in the physical address space. Multi-byte values occupy consecutive byte addresses.

#### **3.8.2.1. Endianness**

Hazard3 is always little-endian for all load and store accesses. RISC-V instruction fetch is always little-endian.

This means in a multi-byte access such as a sw instruction (four bytes are transferred), data stored at higher byte addresses has greater numerical significance. For example:

```
li a0, 0x0d0c0b0a // materialise constant in register
la a4, some_global_variable // materialise address (assume addr % 4 == 0)
sw a0, (a4) // 4-byte write to memory
lbu a0, 0(a4) // load byte from addr + 0: 0x0a
lbu a1, 1(a4) // load byte from addr + 1: 0x0b
lbu a2, 2(a4) // load byte from addr + 2: 0x0c
lbu a3, 3(a4) // load byte from addr + 3: 0x0d
```

#### **3.8.2.2. Physical Memory Attributes**

The RP2350 address space has the following physical memory attributes:

*Table 365. List of physical memory attributes for the RP2350 address space. Main SRAM supports all atomics, other addresses support none. Peripherals are nonidempotent, all other addresses are idempotent.*

<span id="page-279-1"></span>

| Start      | End        | Description                     | Access                           | Atomicity                         | Idempotency    |
|------------|------------|---------------------------------|----------------------------------|-----------------------------------|----------------|
| 0x00000000 | 0x00007fff | Boot ROM                        | No AMOs                          | RsrvNone,<br>AMONone              | Idempotent     |
| 0x10000000 | 0x13ffffff | XIP, Cached                     | No AMOs                          | RsrvNone,<br>AMONone              | Idempotent     |
| 0x14000000 | 0x17ffffff | XIP, Uncached                   | No AMOs                          | RsrvNone,<br>AMONone              | Idempotent     |
| 0x18000000 | 0x1bffffff | XIP, Cache<br>Maintenance       | Write-only                       | RsrvNone,<br>AMONone              | Idempotent     |
| 0x1c000000 | 0x1fffffff | XIP, Uncached +<br>Untranslated | No AMOs                          | RsrvNone,<br>AMONone              | Idempotent     |
| 0x20000000 | 0x20081fff | Main SRAM                       | Any                              | RsrvNonEventual,<br>AMOArithmetic | Idempotent     |
| 0x40000000 | 0x4fffffff | APB Peripherals                 | No AMOs, no<br>instruction fetch | RsrvNone,<br>AMONone              | Non-idempotent |
| 0x50000000 | 0x5fffffff | AHB Peripherals                 | No AMOs, no<br>instruction fetch | RsrvNone,<br>AMONone              | Non-idempotent |
| 0xd0000000 | 0xdfffffff | SIO Peripherals                 | No AMOs, no<br>instruction fetch | RsrvNone,<br>AMONone              | Non-idempotent |

All addresses have Strong ordering. Any address not listed in [Table 365](#page-279-1) is a Vacant address. Accessing these addresses has no effect other than returning a bus fault.

Note Hazard3's PMP implementation requires that non-read-idempotent PMAs are also non-executable, because it enforces execute permissions at the point an instruction is executed, rather than the point an instruction is fetched. Therefore all non-idempotent locations in [Table 365](#page-279-1) are also non-executable. This is enforced at a lower level than the PMP, and executing these addresses at any privilege level will always fault.

Note that cached XIP regions are not cacheable from a PMA point of view, because the cache is private to the memory controller. Each system address is served by either a single cache controller or none, so coherence between harts is irrelevant. You may have to perform manual cache maintenance following some operations like flash programming, but this is a detail of the XIP subsystem, not the system-level memory model.

See section 3.6 of the RISC-V privileged ISA manual linked in [Section 3.8.1.1](#page-233-2) for definitions of these attributes.

#### <span id="page-279-0"></span>**3.8.3. Memory Protection**

Hazard3 implements Physical Memory Protection (PMP). It does not implement the Sv32 virtual memory extension or its associated protections.

The PMP defines permissions for physical addresses. It mostly protects M-mode memory from S-mode and U-mode access. Hazard3 only implements M-mode and U-mode.

A PMP **region** applies read, write and execute permissions to a span of byte addresses. For each region there is one address register, [PMPADDR0](#page-321-0) through [PMPADDR15](#page-324-0), and an 8-bit configuration field packed into [PMPCFG0](#page-315-0) through [PMPCFG3.](#page-319-0) The read, write and execute permissions are always enforced for U-mode. They may also be enforced for Mmode, depending on the PMPCFG L bit for that region, and the [PMPCFGM0](#page-328-0) register.

RP2350 configures Hazard3's PMP hardware with the following features:

- 8× dynamically configurable regions, 0 through <sup>7</sup>
- 3× statically configured (hardwired) regions, 8 through <sup>10</sup>
- (Remaining regions 11 through 15 are hardwired to OFF)
- A granule of 32 bytes
- Support for naturally aligned power of two (NAPOT) region shapes only
- The custom [PMPCFGM0](#page-328-0) CSR can apply M-mode permissions to individual regions without locking them

[Section 3.8.8.1](#page-303-0) defines the configuration of the hardwired regions 8 through 10. These regions apply default U-mode permissions to RP2350 ROM and peripherals, to avoid having to spend dynamic regions to cover these addresses. The system-level ACCESSCTRL registers ([Section 10.6](#page-819-1)) can assign each peripheral individually to M-mode or U-mode.

When multiple PMP regions match the same byte address, the lowest-numbered of these regions takes effect. The other regions are ignored.

#### **3.8.3.1. PMP Address Registers**

Addresses in PMP address registers [PMPADDR0](#page-321-0) through [PMPADDR15](#page-324-0) are stored with a right-shift of two, so that they can cover a 16 GB physical address space when Sv32 address translation is in effect. Hazard3 does not implement address translation, so the physical address space is 4 GB (32-bit byte-addressed) and the two MSBs of each address register are hardwired to zero.

The RP2350 configuration of Hazard3 supports only the OFF and NAPOT values for the PMPCFG A fields (e.g. [PMPCFG0.](#page-315-0)R0\_A). Setting A to OFF means the region matches no bytes, and is effectively disabled. Setting A to NAPOT means the region matches on a naturally aligned span of bytes (the base address modulo the size is zero) whose size is a power of two.

The number of trailing 1s in the PMP address value encodes the size of an NAPOT region. This is the number of consecutive 1s counted from the LSB without reaching a 0. A PMP address value with *no* trailing ones (ending in a 0) matches a region eight bytes in size, and the region size is doubled with each additional 1 bit.

The PMP region matches on the address bits to the left of the least-significant 0 bit. Because the PMP address registers are right-shifted by two, you must apply the same shift to the addresses being compared. The following examples demonstrate how to match addresses based on PMPADDRx values:

- The 30-bit all-ones bit pattern 0x3fffffff has the maximum possible size, and matches all addresses.
- The all-zeroes bit pattern 0x00000000 has the minimum possible size.
  - Since there are no trailing 1s, this matches starting from bit 1 of the PMP address register.
  - Due to addresses being right-shifted by two, this is a region of eight bytes starting from address 0x0.
- The bit pattern 0x???????7 (where ? is any digit) matches any 64-byte region.
  - Shift the base address of this 64-byte region by two to get bits 29:4 of the PMPADDRx value.
- The bit pattern 0x0800000f matches byte addresses between 0x20000000 and 0x2000007f, the first 128 bytes of SRAM.
  - Right-shift the base address (0x20000000) by two to get 0x08000000.
  - Add trailing ones to increase the region size and get the final value of 0x0800000f.
  - The size of the region is eight bytes times two to the power of the number of trailing 1 bits, which in this case (four 1s) works out to 8 × 2 4 = 128 bytes.

For more examples of PMP address match patterns, see the hardwired PMP region values in [Section 3.8.8.1](#page-303-0).

RP2350 configures Hazard3 with a **granule** of 32 bytes. This means the two least-significant bits of each PMP address register are hardwired to all-ones when the region is enabled. The hardware does not decode address regions smaller than 32 bytes.

#### **3.8.3.2. PMP Permissions**

Each 8-bit PMP configuration field contains three permission flags:

- <sup>R</sup> permits non-instruction-fetch reads:
  - load instructions
  - the read phase of AMOs
- <sup>W</sup> permits writes:
  - store instructions
  - the write phase of AMOs
- <sup>X</sup> permits instruction execution

A 1 value for each permission means it is granted, and a 0 means it is revoked. These permissions apply to U-mode access to the region. They also apply to M-mode accesses when any of the following is true:

- The L (lock) configuration bit is <sup>1</sup>
- The Hazard3 custom [PMPCFGM0](#page-328-0) register bit for this region is <sup>1</sup>

The L (lock) bit also locks the associated PMP address register and 8-bit PMP configuration field, so that it ignores future writes. You should always lock PMP regions consecutively from region 0, so that locked regions cannot be bypassed by unlocked regions.

U-mode accesses which match no PMP regions have no permissions: all memory accesses fail. M-mode accesses which match no PMP regions have *all* permissions. The hardwired PMP regions in [Section 3.8.8.1](#page-303-0) define additional Umode permissions for the ROM and peripheral address ranges: these can be overridden by enabling any of the dynamically configured regions.

![](_page_281_Figure_15.jpeg)

Due to [RP2350-E6](#page-1360-1) the field order in the PMP configuration fields is R, W, X (MSB-first) rather than the standard X, W, R. The SDK register headers match the as-implemented order.

#### **3.8.3.3. Accesses Spanning Multiple PMP Regions**

Hazard3 does not support non-naturally-aligned loads or stores, other than to generate standard exceptions when they are attempted. Since NAPOT PMP regions are always naturally aligned, it is impossible for a load or store to span two PMP regions. Therefore all bytes covered by a load or store instruction are determined by at most a single active PMP region which matches the lowest byte address accessed by that instruction.

Instructions are up to 32 bits in size with as little as 16-bit alignment. Therefore it is possible for an instruction to match multiple PMP regions. When this happens, the instruction generates an instruction fault exception, (mcause = 0x1), *unless* there is a lower-numbered PMP region which fully covers the instruction. Lower-numbered PMP regions take precedence.

The exact quote from the privileged ISA specification is: *"The lowest-numbered PMP entry that matches any byte of an access determines whether that access succeeds or fails. The matching PMP entry must match all bytes of an access, or the access fails, irrespective of the L, R, W, and X bits."* (page 60 of RISC-V privileged ISA manual version 20211203).

The RISC-V specification is flexible in what is considered a single access for the purposes of memory protection checking. Hazard3 considers the fetch of one instruction to be a single access. It therefore forbids instruction fetches which straddle two PMP regions, even if both regions grant execute permission. Due to this architecture rule, portable RISC-V software *must not* assume it can execute instructions which span multiple PMP regions.

Avoid this issue by using *hole-punching* region configurations in preference to *glueing* configurations. Suppose you want to cover the first 12 kB of SRAM (0x20000000 → 0x20002fff), this can be achieved in two ways:

- One region adding permissions to 0x20000000 <sup>→</sup> 0x200001fff, and another region adding permissions to 0x20002000 <sup>→</sup> 0x20002fff
- One region adding permissions to 0x20000000 <sup>→</sup> 0x20003fff, and a lower-numbered region *subtracting* permissions from 0x20003000 → 0x20003fff

The former option has a crack between the two regions, which has potentially unwanted effects on some platforms. The latter avoids this issue entirely.

