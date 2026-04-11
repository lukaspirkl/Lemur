# 6.5.4 Memory Periphery Power Down

The main system memories (SRAM0 → SRAM9, mapped to bus addresses 0x20000000 to 0x20081fff), as well as the USB DPRAM, can be partially powered down via the [MEMPOWERDOWN](#page-1250-1) register in the SYSCFG registers (see [Section](#page-1248-0) [12.15.2\)](#page-1248-0). This powers down the analogue circuitry used to access the SRAM storage array (the **periphery** of the SRAM) but the storage array itself remains powered. Memories retain their current contents, but cannot be accessed. Static power is reduced.

#### **CAUTION**

Memories must not be accessed when powered down. Doing so can corrupt memory contents.

When powering a memory back up, a 20ns delay is required before accessing the memory again.

The XIP cache (see [Section 4.4](#page-340-0)) can also be powered down, with [CTRL](#page-346-2).POWER\_DOWN. The XIP hardware will not generate cache accesses whilst the cache is powered down. Note that this is unlikely to produce a net power savings if code continues to execute from XIP, due to the comparatively high voltages and switching capacitances of the external QSPI bus.

#### <span id="page-487-1"></span>**6.5.5. Full Memory Power Down**

RP2350 can completely power down its internal SRAM. Unlike the memory periphery power down described in [Section](#page-487-0) [6.5.4,](#page-487-0) this completely disconnects the SRAM from the power supply, reducing static power to near zero.

Contents are lost when fully powering down memories. When you power memories up again following a power down,

the contents is completely undefined.

There are three distinct SRAM power domains:

#### **SRAM0**

Contains main system SRAM for addresses 0x20000000 through 0x2003ffff (SRAM banks 0 through 3).

#### **SRAM1**

Contains main system SRAM for addresses 0x20040000 through 0x20081fff (SRAM banks 4 through 9).

#### **XIP**

Contains the XIP cache and the boot RAM.

The XIP power domain is always powered when the switched core domain is powered. The switched core domain is the domain which includes all core logic, such as processors, bus fabric and peripherals. This means the memories in this domain are always powered whenever software is running.

Besides powering memory down to save power, you can also leave memories powered *up* whilst powering down the switched core domain. This retains program state in SRAM while eliminating static power dissipation in core logic.

For more information see:

- [Chapter 4](#page-337-0) for a list of RP2350 memory resources, including main system SRAM, the XIP cache and boot RAM
- [Section 6.2.1](#page-441-2) for the definition of core power domains, including the memory power domains enumerated above
- [Section 6.2.2](#page-442-0) for the list of supported memory power states
- [Section 6.2.3](#page-443-0) for information on initiating power state transitions to power memories up or down
- [Section 14.9.7.2](#page-1342-0) for typical power consumption in low-power states including memory power down

