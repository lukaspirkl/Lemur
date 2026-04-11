# 12.6.6 Security

RP2350's processors support partitioning of memory and peripherals into multiple security domains. This partitioning is extended into the DMA, so that different security contexts can safely use their assigned channels without breaking any of the security invariants laid out by the processor security model. For example, an Arm processor in the Non-secure state must not be able to use the DMA to access memory or peripherals owned by Secure software.

The DMA defines four **security levels** which map onto Arm or RISC-V processor security states:

- <sup>3</sup>: SP (secure and privileged)
  - Equivalent to Arm processors in the Secure, Privileged state
  - Equivalent to RISC-V processors in Machine mode
- <sup>2</sup>: SU (secure and unprivileged)

- Equivalent to Arm processors in the Secure, Normal state
- <sup>1</sup>: NSP (nonsecure and privileged)
  - Equivalent to Arm processors in the Non-secure, Privileged state
  - Equivalent to RISC-V processors in Supervisor mode
- <sup>0</sup>: NSU (nonsecure and unprivileged)
  - Equivalent to Arm processors in the Non-secure, Normal state
  - Equivalent to RISC-V processors in User mode

So that the DMA can compare different security levels in a consistent way, they are considered ordered, with SP > SU > NSP > NSU. For example, when we say that a channel requires a *minimum* of SU to access its registers, this means that SP and SU are acceptable, and NSP and NSU are not. As a rule, every action has a reaction that is *at or below* the security level of the original action, and so the DMA can not be used to escalate accesses to a higher security level.

Software assigns internal DMA resources, like channels, interrupts, pacing timers and the CRC sniffer, to one of the four possible security levels. These resources are then accessible only at and above that level. Channel assignment in particular is discussed in [Section 12.6.6.1](#page-1100-0).

The DMA memory protection unit [\(Section 12.6.6.3](#page-1101-0)) defines the minimum security level required to access up to eight programmable address ranges, so that channels of a given security level can not access memory beyond their means. This MPU is intended to mirror the SRAM and XIP memory protection boundaries configured in the processor SAU or PMP. In addition to the internal filtering performed by the DMA MPU, accesses are filtered by the system bus according to the ACCESSCTRL filter rules described in [Section 10.6.2.](#page-821-0)

The combination of these features allows the DMA to be safely shared by software running in different security domains. If this is not desired, the entire DMA block can instead be assigned wholesale to a single security domain using the ACCESSCTRL [DMA](#page-836-1) register.

#### <span id="page-1100-0"></span>**12.6.6.1. Channel Security Assignment**

Channels are assigned to security domains using the channel SECCFG registers, [SECCFG\\_CH0](#page-1134-0) through [SECCFG\\_CH15](#page-1134-0). There is one register per channel. Each register contains a 2-bit security level, and a lock bit which prevents that SECCFG register from being changed once configured. At reset, all channels are assigned to the SP security level, which is the highest.

The security level of a channel defines:

- The security level of bus transfers performed by this channel, which is checked against both the DMA memory protection unit and the ACCESSCTRL bus-level filters described in [Section 10.6.2](#page-821-0)
- The minimum security level required to read or write this channel's registers: access from a lower level returns a bus fault
- The minimum security level which must be defined on a shared IRQ line for that IRQ to be able to observe this channel's interrupts ([Section 12.6.6.2\)](#page-1101-1), or for this channel's interrupt to be set/cleared through that IRQ's registers
- The minimum bus security level required to clear this channel's interrupts through the [INTR](#page-1128-1) register
- Which DREQs a channel can observe: channels assigned to the NSP or NSU security levels can not observe DREQs of Secure-only peripherals (as defined by the ACCESSCTRL peripheral configuration)
- Which pacing timer TREQs can be observed: pacing timer security levels are configured by [SECCFG\\_MISC](#page-1136-0) and must be no higher than the channel security level in order for the channel to observe the TREQ
- Whether the channel is visible to the CRC sniffer: the sniffer's security level is configured by [SECCFG\\_MISC](#page-1136-0) and must be no lower than the observed channel's security level
- Which channels this channel can trigger with a CHAIN\_TO: chaining from lower to higher security levels is not permitted

• The minimum bus security level required to trigger this channel with a write to [MULTI\\_CHAN\\_TRIGGER](#page-1132-0)

The channel SECCFG registers require privileged writes (SP/NSP), and will generate a bus fault on an attempted unprivileged write (SU/NSU). Additionally, the S bit (MSB of the security level) and the LOCK bit are writable only by SP, whilst the P bit (LSB of the security level) is also writable by NSP, if and only if the S bit is clear. Reads are always allowed: it is always possible to query which channels are assigned to you by reading the channel SECCFG registers.

Each channel SECCFG register can be locked manually by writing a one to the LOCK bit in that register, and will also lock automatically upon a successful write to one of the channel's control registers such as [CH0\\_CTRL\\_TRIG](#page-1124-0). This automatic locking avoids any race conditions that may arise from a channel's security level changing after it has already started making transfers, or from leaking secure pointers that have been written to its control registers. Once a channel SECCFG register has been locked, it becomes read-only. LOCK bits can be cleared only by a full reset of the DMA block.

Note that SECCFG registers can be written multiple times before being locked, so the full assignment does not have to be known up front: for example, Secure Arm software can set spare channels to NSP before launching the Non-secure software context, and Non-secure, Privileged software can then set the remaining channels it does not need to NSU before returning to the Non-secure, Normal context.

#### <span id="page-1101-1"></span>**12.6.6.2. Interrupt Security Assignment**

The RP2350 DMA has four system-level interrupt request lines (IRQs), each of which can be asserted on any combination of channel interrupts, as defined by the channel masks in the interrupt enable registers [INTE0](#page-1128-0) through [INTE3.](#page-1131-0) Because the timing of interrupts may leak information, and because it is possible to cause software to malfunction by deliberately manipulating its interrupts, access to the channel interrupt flags must be controlled.

The interrupt security configuration registers, [SECCFG\\_IRQ0](#page-1135-0) through [SECCFG\\_IRQ3](#page-1135-0), define the security level for each interrupt. This is one of the four security levels laid out in [Section 12.6.6](#page-1099-1). The security level of an IRQ defines:

- Which channels are visible in this IRQ's status registers: channels of a level higher than the IRQ's will read back as zero
- Whether a bus access to this IRQ's control and status registers is permitted: bus accesses below this IRQ's security level will return bus faults and have no effect on the DMA
- Which channels will assert this IRQ: channels of a level higher than this IRQ's level will not cause the interrupt to assert, even if relevant INTE bit is set
- Whether a channel's interrupt can be cleared through this IRQ's INTS register, or set through this channel's INTF register: the interrupt flags of channels of higher security level than the IRQ can not be set or cleared

Note that the [INTR](#page-1128-1) register is shared between all IRQs, so it does not respect any of the IRQ security levels. Instead, it follows the security level of the bus access: reads of [INTR](#page-1128-1) will return the interrupt flags of all channels at or below the security level of the bus access (with higher-level channels reading back as zeroes), and writes to INTR have write-oneclear behaviour on channels which are at or below the security level of the bus access.

#### <span id="page-1101-0"></span>**12.6.6.3. Memory Protection Unit**

The DMA memory protection unit (MPU) monitors the addresses of all read/write transfers performed by the DMA, and notes the security level of the originating channel. The MPU is configured in advance with a user-defined security address map, which specifies the *minimum* security level required to access up to eight dynamically configured regions. This is one of the four security levels defined in [Section 12.6.6](#page-1099-1).

Transfers which fail to meet the minimum security level for their address are shot down before reaching the system bus, and a bus error is returned to the originating channel. This will be reported as either a read or write bus error in the channel's CTRL register, depending on whether it was a read or write address that failed the security check.

The intended use for the DMA MPU is to mirror the security definitions of SRAM and XIP memory from the processor SAU or PMP. The number of DMA MPU regions is not sufficient for assigning individual peripherals, so the ACCESSCTRL bus access registers ([Section 10.6.2\)](#page-821-0) are provided for this purpose.

Each of the eight MPU regions is configured with a base address, [MPU\\_BAR0](#page-1137-1) through [MPU\\_BAR7](#page-1137-1) for each region, and a limit address, [MPU\\_LAR0](#page-1137-2) through [MPU\\_LAR7](#page-1137-2).

MPU regions have a granularity of 32 bytes, so the base/limit addresses are configured by the 27 most-significant bits of each BAR/LAR register (bits 31:5). Addresses match MPU regions when the 27 most-significant bits of the address are greater than or equal to the BAR address bits, and less than or equal to the LAR address bits. For example, when [MPU\\_BAR0](#page-1137-1) and [MPU\\_LAR0](#page-1137-2) both have the value 0x10000000, MPU region 0 matches on a 32-byte region extending from byte address 0x10000000 to 0x1000001f (inclusive). Regions can be enabled or disabled using the LAR.EN bits — if a region is disabled, it matches no addresses.

The minimum security level required to access each region is defined by the S and P bits in the LSBs of that region's LAR register. When an address matches multiple regions, the lowest-numbered region applies. This matches the tie-break rules for the RISC-V PMP, but is different from the Arm SAU tie-break rules, so care must be taken when mirroring SAU mappings with overlapping regions. When none of the MPU regions are matched, the security level is defined by the global [MPU\\_CTRL.](#page-1137-0)S and [MPU\\_CTRL.](#page-1137-0)P bits.

The MPU configuration registers ([MPU\\_CTRL,](#page-1137-0) [MPU\\_BAR0](#page-1137-1) through [MPU\\_BAR7](#page-1137-1) and [MPU\\_LAR0](#page-1137-2) through [MPU\\_LAR7\)](#page-1137-2) do not permit unprivileged access. Bus accesses at the SU and NSU security levels will return a bus fault and have no other effect.

The MPU registers are also mostly read-only to NSP accesses, with the sole exception being the region P bits which are NSP-writable if and only if the corresponding region's S bit is clear. This delegates to Privileged, Non-secure software the decision of whether Non-secure regions are NSU-accessible.

