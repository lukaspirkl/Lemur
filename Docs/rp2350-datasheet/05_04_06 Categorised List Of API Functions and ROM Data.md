# 5.4.6 Categorised List Of API Functions and ROM Data

The terms in parentheses after each function name (Arm-S, Arm-NS, RISC-V) indicate the architecture and security state combinations where that API is available:

- Arm-S: Arm processors running in the Secure state
- Arm-NS: Arm processors running in the Non-secure state
- RISC-V: RISC-V processors

See [Section 5.4.2](#page-377-0) for the full definitions of these terms.

List entries ending with parentheses, such as [flash\\_op\(\)](#page-386-0), are callable functions. List entries without parentheses, such as [git\\_revision,](#page-393-0) are pointers to ROM data locations.

#### **5.4.6.1. Low-Level Flash Access**

These low-level (Secure-only) flash access functions are similar to the ones on RP2040:

- [connect\\_internal\\_flash\(\)](#page-384-1) (Arm-S, RISC-V)
- [flash\\_enter\\_cmd\\_xip\(\)](#page-385-0) (Arm-S, RISC-V)
- [flash\\_exit\\_xip\(\)](#page-385-1) (Arm-S, RISC-V)
- [flash\\_flush\\_cache\(\)](#page-385-2) (Arm-S, RISC-V)
- [flash\\_range\\_erase\(\)](#page-387-0) (Arm-S, RISC-V)
- [flash\\_range\\_program\(\)](#page-387-1) (Arm-S, RISC-V)

These are new with RP2350:

- [flash\\_reset\\_address\\_trans\(\)](#page-388-0) (Arm-S, RISC-V)
- [flash\\_select\\_xip\\_read\\_mode\(\)](#page-388-1) (Arm-S, RISC-V)

#### **5.4.6.2. High-Level Flash Access**

The higher level access functions, provide functionality that is safe to expose (with permissions) to Non-secure code as well.

- [flash\\_op\(\)](#page-386-0) (Arm-S, Arm-NS, RISC-V)
- [flash\\_runtime\\_to\\_storage\\_addr\(\)](#page-388-2) (Arm-S, Arm-NS, RISC-V)

#### **5.4.6.3. System Information**

- [flash\\_devinfo16\\_ptr](#page-384-2) (Arm-S, RISC-V)
- [get\\_partition\\_table\\_info\(\)](#page-389-0) (Arm-S, Arm-NS RISC-V)
- [get\\_sys\\_info\(\)](#page-390-0) (Arm-S, Arm-NS, RISC-V)
- [git\\_revision](#page-393-0) (Arm-S, Arm-NS, RISC-V)

#### **5.4.6.4. Partition Tables**

- [get\\_b\\_partition\(\)](#page-389-1) (Arm-S, RISC-V)
- [get\\_uf2\\_target\\_partition\(\)](#page-392-0) (Arm-S, RISC-V)

```
• pick_ab_partition() (Arm-S, RISC-V)
```

- [partition\\_table\\_ptr](#page-394-1) (Arm-S`, RISC-V)
- [load\\_partition\\_table\(\)](#page-393-1) (Arm-S, RISC-V)

#### **5.4.6.5. Bootrom Memory and State**

```
• set_bootrom_stack() (RISC-V)
```

- [xip\\_setup\\_func\\_ptr](#page-398-2) (Arm-S, RISC-V)
- [bootrom\\_state\\_reset\(\)](#page-383-1) (Arm-S, RISC-V)

#### **5.4.6.6. Executable Image management**

```
• chain_image() (Arm-S, RISC-V)
```

```
• (explicit_buy() (Arm-S, RISC-V)
```

#### **5.4.6.7. Security**

These Secure-only functions control access for Non-secure code:

```
• set_ns_api_permission() (Arm-S)
```

- [set\\_rom\\_callback\(\)](#page-397-2) (Arm-S, RISC-V)
- [validate\\_ns\\_buffer\(\)](#page-398-3) (Arm-S, RISC-V)

#### **5.4.6.8. Miscellaneous**

These functions are provided to all platforms and security levels, but perform additional checks when called from Nonsecure Arm code:

```
• reboot() (Arm-S, Arm-NS, RISC-V)
```

• [otp\\_access\(\)](#page-393-2) (Arm-S, Arm-NS, RISC-V)

#### **5.4.6.9. Non-secure Only**

• [secure\\_call\(\)](#page-396-0) (Arm-NS)

#### **5.4.6.10. Bit Manipulation**

Unlike RP2040 the bootrom does not contain bit manipulation functions. Processors on RP2350 implement hardware instructions for these operations which are far faster than the software implementations in the RP2040 bootrom.

#### **5.4.6.11. Memcpy and Memset**

Unlike RP2040, the bootrom does not provide memory copy or clearing functions, as your language runtime is expected to already provide well-performing implementations of these on Cortex-M33 or Hazard3.

The bootrom does contain private implementations of standard C memcpy() and memset(), for both Arm and RISC-V, but these are optimised for size rather than performance. They are not exported in the ROM table.

#### **5.4.6.12. Floating Point**

Unlike RP2040 the bootrom does not contain functions for floating point arithmetic. On Arm there is standard processor support for single-precision arithmetic via the Cortex-M FPU, and RP2350 provides an Arm coprocessor which dramatically accelerates double-precision arithmetic (the DCP, [Section 3.6.2](#page-104-0)). The SDK defaults to the most performant hardware or software implementation available.

