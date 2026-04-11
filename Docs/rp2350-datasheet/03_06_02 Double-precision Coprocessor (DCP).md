# 3.6.2 Double-precision Coprocessor (DCP)

Each Cortex-M33 CPU core is equipped with two instances of a double-precision coprocessor that provides acceleration of double-precision floating point operations including add, subtract, multiply, divide and square root. The design is implemented in just a few thousand gates and so occupies much less silicon die area than a full double-precision floating-point unit. Nevertheless, these coprocessors considerably speed up basic double-precision operations compared to pure software implementations. The coprocessors also offer support for some single-precision operations and conversions.

The two coprocessor instances are assigned to the Secure and Non-secure domains. Coprocessor number 4 always maps to the coprocessor used for the current processor security state. Coprocessor number 5 always maps to the Nonsecure coprocessor instance, but is accessible only from Secure code. This duplication avoids saving and restoring the coprocessor context during Secure/Non-secure state transitions.

#### **3.6.2.1. CPU Interface**

As with the other coprocessors, the accelerator connects to the CPU over a 64-bit bus. Two words of data can be transferred per cycle over that bus using the following instructions:

- MCRR: move two integer registers to coprocessor
- MRRC: move two integer registers from coprocessor

There are also single-register versions of these instructions, including ones that allow the CPU's flags to be loaded from the coprocessor. The CPU issues CDP instructions to trigger operations within the coprocessor without transferring any data.

#### **3.6.2.2. Internal architecture**

A block diagram of the accelerator is shown in [Figure 12](#page-105-0).

*Figure 12. Block diagram of doubleprecision accelerator*

<span id="page-105-0"></span>![](_page_105_Figure_2.jpeg)

At the heart of the design are:

- two sets of registers, each designed to hold an unpacked double-precision value
- a 9-bit status register

Unlike a conventional FPU, the accelerator does not contain a full register bank. Not only does this save die area, it also means that saving and restoring the coprocessor's state is very fast: in fact, the entire state fits within six 32-bit words and hence can be saved to, or restored from, the CPU in three instructions.

The accelerator contains a wide adder, capable of adding two mantissa values and three exponent values simultaneously. There is also a shifter that can either perform a logical right shift by a given amount, or normalise a denormalised mantissa and report the amount of shift required to do so. A considerable amount of hardware in the shifter is shared between these two operating modes.

Control logic, shown at the top of the diagram, decodes coprocessor instructions and configures the accelerator's functional units and datapath multiplexers in order to execute the desired operation. Each coprocessor instruction takes a single cycle, so coprocessor operations cannot stall the CPU.

A floating-point operation such as addition or subtraction is carried out by executing a fixed (or 'canned') sequence of instructions as follows:

- 1. One or two MCRR instructions to write the operands to the coprocessor.
- 2. A number of CDP (and possibly other) instructions that together perform the operation itself.
- 3. An MRRC or MRC instruction to read back the result.

The hardware handles special cases involving zeroes, NaNs, and infinities, as well as rounding, underflow and overflow.

The accelerator does not contain a multiplier array, as that would occupy a considerable amount of die area. Instead, the mantissas of the operands of a multiplication operation are brought back into the CPU to take advantage of the fast long multiply instructions available there. The coprocessor handles the processing of exponents.

Division and square root operations also involve data moving back and forth between coprocessor and CPU. To assist with these operations, the coprocessor contains two small lookup tables (implemented as random logic) that provide initial approximations used in the divide and square root algorithms. The coprocessor handles the processing of exponents.

The accelerator is only meant to be used with the canned instruction sequences that implement basic floating-point operations. The state of the accelerator is not guaranteed to be preserved from the end of one canned sequence to the beginning of the next: see the discussion of the 'engaged' flag in the status register below.

#### **3.6.2.3. Registers**

#### **X and Y mantissa registers**

The X and Y mantissa registers (xm and ym) are each 64 bits wide. They can be read and written directly by the CPU; the xm register can also store the lower part of the result from the adder. When a value is written to the coprocessor using a 'write unpacked' MCRR instruction, the top two bits of the mantissa register are set to 01 and the next most significant bits are filled from the mantissa field of the floating-point operand. The low-order bits of the mantissa register are cleared.

#### **X and Y exponent registers**

The X and Y exponent registers (xe and ye) are each 14 bits wide. They can be read and written directly by the CPU; the xe register can also store the higher part of the result from the adder. When a value is written to the coprocessor using a 'write unpacked' MCRR instruction, the exponent register is set from the exponent field of the floating-point operand.

#### **X and Y flag registers**

The X and Y flag registers (xf and yf) are each four bits wide. They can be read and written directly by the CPU. The flag register stores information about the type of floating-point number represented in the corresponding mantissa and exponent registers: its sign, whether it is a zero, whether it is an infinity, and whether it is a NaN. When a value is written to the coprocessor using a 'write unpacked' MCRR instruction, the bits of the flag register are updated according to the type of the floating-point operand.

#### **Status register**

The status register contains nine bits. It can be read and written directly by the CPU. The least significant six bits of the register store the shift required to align the two operands of an addition or subtraction; the next two bits indicate whether the value represented by (xe, xm) is greater than, equal to, or less than the value represented by (ye, ym) - in other words, whether the magnitude of the value stored in the X registers is greater than, equal to, or less than the magnitude of the value stored in the Y registers. These status bits are set in the first step of an addition, subtraction or comparison operation after the operands have been loaded.

The final bit of the status register indicates whether the coprocessor is 'engaged'. The engaged flag is set by all coprocessor instructions that occur at the beginning or in the middle of the canned instruction sequences. It is cleared by those instructions used at the end of a canned sequence to read back a final result.

#### **3.6.2.4. State save and restore**

An interrupt handler can test the engaged flag to determine whether it has pre-empted an in-progress operation on the same coprocessor. If the engaged flag is set, the handler can save (and restore) the coprocessor state before using the coprocessor. If the engaged flag is clear, the save (and restore) step can be skipped. If this approach is implemented, the state of the accelerator must be regarded as undefined when not within one of the canned instruction sequences.

Three MRRC instructions are provided to copy the six words of state in the coprocessor into integer registers in the CPU, from where they can, for example, be pushed onto the stack. The last of these instructions clears the engaged flag. Similarly, three MCRR instructions are provided to restore the state of the coprocessor from integer registers, including the state of the engaged flag.

#### **3.6.2.5. Instruction summary**

As mentioned above, it is intended that the coprocessor instructions are only used as part of canned sequences. Nevertheless, for completeness, a list of the available instructions is given here with an outline of their effects.

MCRR instructions are shown in [Table 108](#page-107-0).

*Table 108. MCRR instructions*

<span id="page-107-0"></span>

| Mnemonic | Effect                                                | Used by                               |
|----------|-------------------------------------------------------|---------------------------------------|
| WXMD     | write xm direct                                       | restore status                        |
| WYMD     | write ym direct                                       | restore status                        |
| WEFD     | write xe,xf,ye,yf,other status direct                 | restore status                        |
| WXUP     | write xm,xe,xf unpacked double-precision              | double-precision binary<br>operations |
| WYUP     | write ym,ye,yf unpacked double-precision              | double-precision binary<br>operations |
| WXYU     | write xm,xe,xf,ym,ye,yf two unpacked single-precision | single-precision binary<br>operations |
| WXMS     | write xm bit 0=0/1 if data zero/nonzero               | dmul                                  |
| WXMO     | write xm direct OR into b0, add exponents, XOR signs  | dmul                                  |
| WXDD     | write xm direct; subtract exponents, XOR signs        | ddiv                                  |
| WXDQ     | write xm direct, offset exponent                      | dsqrt                                 |
| WXUC     | write X unsigned int+252+232, Y=252+232               | conversions from<br>unsigned int      |
| WXIC     | write X signed int+252+232, Y=252+232                 | conversions from signed<br>int        |
| WXDC     | write X unpacked double-precision, Y=252+232          | conversions from double<br>precision  |
| WXFC     | write X unpacked single-precision, Y=252+232          | conversions from single<br>precision  |
| WXFM     | write xm direct, add exponents, XOR signs             | fmul                                  |
| WXFD     | write xm direct, subtract exponents, XOR signs        | fdiv                                  |
| WXFQ     | write xm direct, offset exponent                      | fsqrt                                 |

CDP instructions are shown in [Table 109.](#page-107-1)

*Table 109. CDP instructions*

<span id="page-107-1"></span>

| Mnemonic | Effect                       | Used by       |
|----------|------------------------------|---------------|
| INIT     | zero all registers           |               |
| ADD0     | compare X-Y, set status      | add, sub, cmp |
| ADD1     | xm:=±xm+±ym>>s or ±ym+±xm>>s | add           |
| SUB1     | xm:=±xm–±ym>>s or –±ym±xm>>s | sub           |
| SQR0     | xe=xe/2, xm=xm<<0:1          | sqrt          |

| Mnemonic | Effect                                                         | Used by                                                            |
|----------|----------------------------------------------------------------|--------------------------------------------------------------------|
| NORM     | normalise                                                      |                                                                    |
| NRDF     | normalise and round single-precision                           | single-precision<br>operations, conversions<br>to single-precision |
| NRDD     | normalise and round double-precision                           | double-precision<br>operations, conversions<br>to double-precision |
| NTDC     | normalise and truncate double-precision pre-integer conversion | truncating conversions to<br>int                                   |
| NRDC     | normalise and round double-precision pre-integer conversion    | rounding conversions to<br>int                                     |

MRRC and MRC instructions are shown in [Table 110.](#page-108-0)

*Table 110. MRRC and MRC instructions*

<span id="page-108-0"></span>

| Mnemonic | Effect                                         | Used by                                               |
|----------|------------------------------------------------|-------------------------------------------------------|
| RXVD     | read xf,VERSION direct                         | dclassify, check version                              |
| RCMP     | read processed status                          | dcmp                                                  |
| RDFA     | read FADD result packed from X                 | fadd                                                  |
| RDFS     | read FSUB result packed from X                 | fsub                                                  |
| RDFM     | read FMUL result packed from X                 | fmul                                                  |
| RDFD     | read FDIV result packed from X                 | fdiv                                                  |
| RDFQ     | read FSQRT result packed from X                | fsqrt                                                 |
| RDFG     | read general float result packed from X        | double-precision to<br>single-precision<br>conversion |
| RDUC     | read unsigned integer conversion result from X | conversions to unsigned<br>int                        |
| RDIC     | read signed integer conversion result from X   | conversions to signed int                             |
| RXMD     | read xm direct                                 | save status                                           |
| RYMD     | read ym direct, engaged=0                      | save status                                           |
| REFD     | read xe,xf,ye,yf,other status direct           | save status                                           |
| RXMS     | read xm Q62-s                                  | dmul, ddiv, dsqrt                                     |
| RYMS     | read ym Q62-s                                  | dmul, ddiv                                            |
| RXYH     | read ym hi, xm hi                              | fmul, fdiv                                            |
| RYMR     | read ym hi, recip approximation lo             | fdiv, ddiv                                            |
| RXMQ     | read xm hi, rsqrt approximation lo             | fsqrt, dsqrt                                          |
| RDDA     | read DADD result packed from X                 | dadd                                                  |
| RDDS     | read DSUB result packed from X                 | dsub                                                  |
| RDDM     | read DMUL result packed from X                 | dmul                                                  |
| RDDD     | read DDIV result packed from X                 | ddiv                                                  |

| Mnemonic | Effect                                   | Used by                                               |
|----------|------------------------------------------|-------------------------------------------------------|
| RDDQ     | read DSQRT result packed from X          | dsqrt                                                 |
| RDDG     | read general double result packed from X | single-precision to<br>double-precision<br>conversion |

Alongside each MRRC and MRC instruction is a variant starting P (for 'peek') instead of R that has the same function but preserves the engaged flag. RXMD is identical to PXMD; REFD is identical to PEFD.

The SDK includes macros to generate Arm assembler from the mnemonics above in the [file](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_dcp/include/hardware/dcp_instr.inc.S) [dcp\\_instr.inc.S](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_dcp/include/hardware/dcp_instr.inc.S) [in the SDK,](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_dcp/include/hardware/dcp_instr.inc.S) for example turning WXUP r0,r1 into mcrr p4,#1,r0,r1,c0.

#### **3.6.2.6. Example canned sequence**

The assembly code sequence to implement a callable double-precision addition operation is shown in [Table 111](#page-109-0).

*Table 111. Assembly code sequence to implement a callable double-precision addition operation*

<span id="page-109-0"></span>

| Arm assembler         | Coprocessor mnemonic | Action                                                                                            |
|-----------------------|----------------------|---------------------------------------------------------------------------------------------------|
| mcrr p4,#1,r0,r1,c0   | WXUP r0,r1           | write R0 and R1 unpacked double-precision into X                                                  |
| mcrr p4,#1,r2,r3,c1   | WYUP r2,r3           | write R2 and R3 unpacked double-precision into Y                                                  |
| cdp p4,#0,c0,c0,c1,#0 | ADD0                 | compare X and Y; set status and alignment shift                                                   |
| cdp p4,#1,c0,c0,c1,#0 | ADD1                 | add/subtract (depending on status and signs) xm and ym<br>aligned, write result to xm             |
| cdp p4,#8,c0,c0,c0,#1 | NRDD                 | normalise and round double-precision result                                                       |
| mrrc p4,#1,r0,r1,c0   | RDDA r0,r1           | read R0 and R1 packed double-precision from X, including<br>special-value processing for addition |
| bx r14                |                      | return from function                                                                              |

Logic in the coprocessor ensures, for example, that the ADD1 instruction shifts the smaller argument, that xm and ym are negated as required before being sent to the adder, and that the larger exponent is used as the basis for the subsequent normalisation.

#### **3.6.2.7. Using the coprocessor via the SDK library**

The SDK pico\_double library automatically uses the coprocessor for double-precision floating-point calculations. This is the simplest way to take advantage of the coprocessor, but it entails a few cycles of overhead for each operation. Not only is there the overhead involved in a function call and return, but for safety the general-purpose implementations in the SDK always test the engaged flag, saving and restoring the coprocessor state to and from the stack as needed. That ensures that the functions work correctly if used in interrupt handlers, without additional intervention.

#### **3.6.2.8. Using the coprocessor directly**

The SDK includes macros to generate canned sequences for standard operations in the file [dcp\\_canned.inc.S](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_dcp/include/hardware/dcp_canned.inc.S) [in the SDK.](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_dcp/include/hardware/dcp_canned.inc.S)

These allow the callable double-precision addition operation listed above, for example, to be written as:

```
dcp_dadd_m r0,r1, r0,r1,r2,r3 @ result in r0,r1; operands in r0,r1 and r2,r3
bx r14
```

dcp\_dadd\_m is a macro which expands into the sequence of coprocessor instructions given above. This macro allows you to specify the integer registers to be used for the operands and the result, which means that using these macros directly not only avoids function call and return overhead, it also avoids the extra overhead associated with argument marshalling.

The more complex macros also require you to specify 'scratch' registers that they can use for storing intermediate results. The following function, which calculates the dot product of two three-element vectors of doubles pointed to by R0 and R1, illustrates this:

```
push {r4-r9,r14}
ldrd r3,r4,[r0],#8 @ load x₀
ldrd r5,r6,[r1],#8 @ load y₀
dcp_dmul_m r7,r8, r3,r4,r5,r6, r3,r4,r5,r6,r12,r14,r9 @ compute x₀y₀ ①
ldrd r3,r4,[r0],#8 @ load x₁
ldrd r5,r6,[r1],#8 @ load y₁
dcp_dmul_m r3,r4, r3,r4,r5,r6, r3,r4,r5,r6,r12,r14,r9 @ compute x₁y₁ ①
dcp_dadd_m r7,r8, r3,r4,r7,r8 @ compute x₀y₀+x₁y₁
ldrd r3,r4,[r0],#8 @ load x₂
ldrd r5,r6,[r1],#8 @ load y₂
dcp_dmul_m r3,r4, r3,r4,r5,r6, r3,r4,r5,r6,r12,r14,r9 @ compute x₂y₂ ①
dcp_dadd_m r0,r1, r3,r4,r7,r8 @ compute x₀y₀+x₁y₁+x₂y₂ ②
pop {r4-r9,r15}
```

- 1. r3, r4, r5, r6, r12, r14, and r9 are scratch registers.
- 2. stores the result in r0, r1.

# **NOTE**

This example does not check the engaged flag. If used in interrupt handlers or in multi-threaded applications, a suitable test would have to be added. For example, see the [SDK implementation of](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/pico_double/double_aeabi_dcp.S) [\\_\\_aeabi\\_dadd](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/pico_double/double_aeabi_dcp.S) for an efficient way to do this. The test only needs to be performed once, at the beginning of the function, so the overhead in this case would be relatively small.

The following example demonstrates how to use the coprocessor:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/dcp/hello\\_dcp/hello\\_dcp.c](https://github.com/raspberrypi/pico-examples/blob/master/dcp/hello_dcp/hello_dcp.c#L18-L109) Lines 18 - 109*

```
 18 extern double dcp_dot (double*p,double*q,int n);
 19 extern double dcp_dotx (float*p,float*q,int n);
 20 extern float dcp_iirx (float x,float*temp,float*coeff,int order);
 21 extern void dcp_butterfly_radix2 (double*x,double*y);
 22 extern void dcp_butterfly_radix2_twiddle_dif (double*x,double*y,double*tf);
 23 extern void dcp_butterfly_radix2_twiddle_dit (double*x,double*y,double*tf);
 24 extern void dcp_butterfly_radix4 (double*w,double*x,double*y,double*z);
 25 
 26 static void dcp_test0() {
 27 double u[3]={1,2,3};
 28 double v[3]={4,5,6};
 29 double w;
 30 w=dcp_dot(u,v,3);
 31 printf("(1,2,3).(4,5,6)=%g\n",w);
 32 }
 33 
 34 static void dcp_test1() {
 35 float u[3]={1+pow(2,-20),2,3};
 36 float v[3]={1-pow(2,-20),5,6};
 37 double w;
 38 w=dcp_dotx(u,v,3);
```

```
 39 printf("(1+pow(2,-20),2,3).(1-pow(2,-20),5,6)=%.17g\n",w);
 40 }
 41 
 42 static void dcp_test2() {
 43 int t;
 44 float w;
 45 // filter coefficients calculated using Octave as follows:
 46 // octave> pkg load signal
 47 // octave> format long
 48 // octave> [b,a]=cheby1(2,1,.5)
 49 // b = 0.307043201259064 0.614086402518128 0.307043201259064
 50 // a = 1.000000000000000e+00 6.406405700380895e-02 3.139684953186774e-01
 51 // and tested as follows:
 52 // octave> filter(b,a,[1 zeros(1,19)])
 53 float coeff[5]={0.3070432,0.3139685,0.6140864,0.06406406,0.3070432};
 54 float temp[4]={0};
 55 printf("IIR filter impulse response:\n");
 56 for(t=0;t<20;t++) {
 57 w=dcp_iirx(t?0:1,temp,coeff,2);
 58 printf("y[%2d]=%g\n",t,w);
 59 }
 60 }
 61 
 62 static void dcp_test3() {
 63 double x[2]={2,3};
 64 double y[2]={5,7};
 65 dcp_butterfly_radix2(x,y);
 66 printf("Radix-2 butterfly of (2+3j,5+7j)=(%g%+gj,%g%+gj)\n",x[0],x[1],y[0],y[1]);
 67 }
 68 
 69 static void dcp_test4() {
 70 double x[2]={2,3};
 71 double y[2]={5,7};
 72 double t[2]={1.5,2.5};
 73 dcp_butterfly_radix2_twiddle_dif(x,y,t);
 74 printf("Radix-2 DIF butterfly of (2+3j,5+7j) with twiddle factor
  (1.5+2.5j)=(%g%+gj,%g%+gj)\n",x[0],x[1],y[0],y[1]);
 75 }
 76 
 77 static void dcp_test5() {
 78 double x[2]={2,3};
 79 double y[2]={5,7};
 80 double t[2]={1.5,2.5};
 81 dcp_butterfly_radix2_twiddle_dit(x,y,t);
 82 printf("Radix-2 DIT butterfly of (2+3j,5+7j) with twiddle factor
  (1.5+2.5j)=(%g%+gj,%g%+gj)\n",x[0],x[1],y[0],y[1]);
 83 }
 84 
 85 static void dcp_test6() {
 86 double w[2]={2,3};
 87 double x[2]={5,7};
 88 double y[2]={11,17};
 89 double z[2]={41,43};
 90 dcp_butterfly_radix4(w,x,y,z);
 91 printf("Radix-4 butterfly of (2+3j,5+7j,11+17j,41+43j)=(%g%+gj,%g%+gj,%g%+gj,%g%+gj)\n"
  ,w[0],w[1],x[0],x[1],y[0],y[1],z[0],z[1]);
 92 }
 93 
 94 int main() {
 95 stdio_init_all();
 96 
 97 printf("Hello, DCP!\n");
 98 
 99 dcp_test0();
```

```
100 dcp_test1();
101 dcp_test2();
102 dcp_test3();
103 dcp_test4();
104 dcp_test5();
105 dcp_test6();
106 
107 return 0;
108 }
```

There are also further examples in the dcp/ directory in the [Pico Examples](https://github.com/raspberrypi/pico-examples) repository.

#### **3.6.2.9. IEEE 754 compliance**

The canned instruction sequences provide IEEE-compliant operations with the exception that denormals are flushed to zero on input and output. Zeroes, NaNs and infinities are correctly handled. Rounding is to nearest, even on tie.

Faster versions of division and square root operations, named ddiv\_fast and dsqrt\_fast respectively, are available. These do not always give correctly rounded results but do have a guaranteed error before rounding of less than 0.5ulp ('units in last place'), which in particular means that if there is an exact representation of the result then that is what is returned.

### **3.6.2.10. Benchmarks**

[Table 112](#page-112-1) gives cycle counts for various floating-point operations using the accelerator with inlined code, compared to some typical ranges of benchmarks for (a) fully-fledged hardware double-precision FPUs; and (b) pure software implementations.

*Table 112. Cycle counts for floatingpoint operations using the accelerator*

<span id="page-112-1"></span>

| Operation              | Using<br>coprocessor | Full hardware (latency) | Software only |
|------------------------|----------------------|-------------------------|---------------|
| dadd                   | 6                    | 2-6                     | 70-90         |
| dsub                   | 6                    | 2-6                     | 70-90         |
| dmul                   | 17                   | 3-7                     | 75-90         |
| ddiv                   | 51                   | 13-60                   | 135-600       |
| ddiv_fast              | 32                   |                         |               |
| dsqrt                  | 49                   | 15-62                   | 130-650       |
| dsqrt_fast             | 38                   |                         |               |
| dcmp                   | 4                    |                         |               |
| dclassify              | 2                    |                         |               |
| integer to/from double | 5                    |                         |               |

