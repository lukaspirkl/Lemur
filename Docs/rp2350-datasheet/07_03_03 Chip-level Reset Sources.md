# 7.3.3 Chip-level Reset Sources

In order of severity, the following events can trigger a chip-level reset:

#### **Power-On Reset (POR)**

The power-on reset ensures the chip starts up cleanly when power is first applied by holding it in reset until the digital core supply (DVDD) reaches a voltage high enough to reliably power the chip's core logic. The POR component is described in detail in [Section 7.6.1, "Power-on Reset \(POR\)".](#page-506-0)

7.3. Chip Level Resets **493**

#### **Brownout Detection (BOD)**

The brownout detector prevents unreliable operation when the digital core supply (DVDD) drops below a safe operating level. The BOD component is described in detail in [Section 7.6.2, "Brownout Detection \(BOD\)".](#page-506-1) The reset asserted by the BOD is referred to as the brownout reset, or BOR.

#### **External Reset**

The chip can be reset by taking the RUN pin low. This holds the chip in reset irrespective of the state of the core power supply (DVDD), the power-on reset block, and brownout detection block. RUN can be used to extend the initial power-on reset, or can be driven from an external source to start and stop the chip as required. If RUN is not used, it should be tied high. Double-tapping the RUN low will set [CHIP\\_RESET](#page-467-0).DOUBLE\_TAP. Boot code reads this flag and selects an alternate boot sequence if the flag is set.

#### **Debugger Reset Request**

The debugger is able to initiate a chip-level reset using the CDBGPWRUPREQ control. For more information, see [Section](#page-84-2) [3.5, "Debug"](#page-84-2).

#### **Rescue Debug Port Reset**

The chip can also be reset via the Rescue Debug Port. This allows the chip to be recovered from a locked-up state. In addition to resetting the chip, a Rescue Debug Port reset also sets [CHIP\\_RESET.](#page-467-0)RESCUE\_FLAG. This is checked by boot code at startup, causing it to enter a safe state if the bit is set. See [Section 3.5.8, "Rescue Reset"](#page-90-0) for more information.

#### **Watchdog**

The watchdog can trigger various levels of chip-level reset by setting appropriate bits in the [WDSEL](#page-470-0) register. A chiplevel reset triggered by a watchdog reset will reset the watchdog and the watchdog scratch registers. Additional general purpose scratch registers are available in POWMAN. These are not reset by a chip-level reset triggered by the watchdog.

#### **SWCORE Powerdown**

For a list of operations that power down the switched-core power domain (SWCORE) and trigger this reset, see [Section 6.2, "Power Management".](#page-441-1)

#### **Glitch Detector**

This reset fires if a glitch is detected in SWCORE power supply. For more information, see [Section 10.9, "Glitch](#page-866-1) [Detector"](#page-866-1).

#### **RISC-V Non-Debug-Module Reset**

The dmcontrol.ndmreset bit in the RISC-V Debug Module resets all RISC-V harts in the system. It resets no other hardware. However, it is recorded as a chip-level reset reason in [CHIP\\_RESET](#page-467-0).HAD\_HZD\_SYS\_RESET\_REQ. See [Section 3.5.3, "RISC-V Debug"](#page-86-1) for details of the RISC-V debug subsystem.

The source of the last chip-level reset is recorded in the [CHIP\\_RESET](#page-467-0) register.

A complete list of POWMAN registers is provided in [Section 6.4, "Power Management \(POWMAN\) Registers"](#page-454-1).

