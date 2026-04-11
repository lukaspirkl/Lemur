# 3.8.8 Configuration

Hazard3 uses the parameters given in the [hazard3\\_config.vh](https://github.com/Wren6991/Hazard3/blob/86fc4e3f/hdl/hazard3_config.vh) header to customise the core. These values are set before taping out a Hazard3 instance on silicon, so they are *fixed* from a user point of view. They determine which instructions the processor supports, the area-performance trade-off for certain instructions, and static configuration for core peripherals like the PMP. RP2350 uses the following values for these parameters:

| Parameter          | Value |
|--------------------|-------|
| EXTENSION_A        | 1     |
| EXTENSION_C        | 1     |
| EXTENSION_M        | 1     |
| EXTENSION_ZBA      | 1     |
| EXTENSION_ZBB      | 1     |
| EXTENSION_ZBC      | 0     |
| EXTENSION_ZBS      | 1     |
| EXTENSION_ZCB      | 1     |
| EXTENSION_ZCMP     | 1     |
| EXTENSION_ZBKB     | 1     |
| EXTENSION_ZIFENCEI | 1     |
| EXTENSION_XH3BEXTM | 1     |
| EXTENSION_XH3IRQ   | 1     |

| Parameter           | Value               |
|---------------------|---------------------|
| EXTENSION_XH3PMPM   | 1                   |
| EXTENSION_XH3POWER  | 1                   |
| CSR_M_MANDATORY     | 1                   |
| CSR_M_TRAP          | 1                   |
| CSR_COUNTER         | 1                   |
| U_MODE              | 1                   |
| PMP_REGIONS         | 11                  |
| PMP_GRAIN           | 3                   |
| PMP_HARDWIRED       | 11'h700             |
| PMP_HARDWIRED_ADDR  | See Section 3.8.8.1 |
| PMP_HARDWIRED_CFG   | See Section 3.8.8.1 |
| DEBUG_SUPPORT       | 1                   |
| BREAKPOINT_TRIGGERS | 4                   |
| NUM_IRQS            | 52                  |
| IRQ_PRIORITY_BITS   | 4                   |
| IRQ_INPUT_BYPASS    | {NUM_IRQS{1'b1}}    |
| MVENDORID_VAL       | 32'h00000493        |
| MIMPID_VAL          | 32'h86fc4e3f        |
| MCONFIGPTR_VAL      | 32'h0               |
| REDUCED_BYPASS      | 0                   |
| MULDIV_UNROLL       | 2                   |
| MUL_FAST            | 1                   |
| MUL_FASTER          | 1                   |
| MULH_FAST           | 1                   |
| FAST_BRANCHCMP      | 1                   |
| RESET_REGFILE       | 1                   |
| BRANCH_PREDICTOR    | 1                   |
| MTVEC_WMASK         | 32'hfffffffd        |

#### <span id="page-303-0"></span>**3.8.8.1. Hardwired PMP Regions**

RP2350 configures Hazard3 with eight dynamically configured PMP regions, and three static ones. The static regions provide default U-mode RWX permissions on the following ranges:

- ROM: 0x00000000 through 0x0fffffff
- Peripherals: 0x40000000 through 0x5fffffff
- SIO: 0xd0000000 through 0xdfffffff

These addresses appear in [PMPADDR8,](#page-322-0) [PMPADDR9](#page-323-0) and [PMPADDR10.](#page-323-1) The hardwired PMP address registers behave the same as dynamic registers, except that they ignore writes (exercising the WARL rule). The permissions for these

regions are in [PMPCFG2.](#page-318-0)

The hardwired regions have a similar role to the Exempt regions added to the Cortex-M33 IDAU address map specified in [Section 10.2.2](#page-817-0).

RP2350 puts default U-mode permissions on AHB/APB peripherals because these are expected to be assigned using ACCESSCTRL ([Section 10.6\)](#page-819-1). ACCESSCTRL can assign each peripheral individually, using the existing address decoders in the bus fabric, whereas PMP regions are in limited supply so are less useful for peripheral assignment.

Similarly, SIO has internal banking over Secure/Non-secure bus attribution, which is mapped onto Machine and User modes as described in [Section 10.6.2.](#page-821-0)

The dynamic regions 0 through 7 take priority over the hardwired regions, because the PMP prioritises lower-numbered regions.

