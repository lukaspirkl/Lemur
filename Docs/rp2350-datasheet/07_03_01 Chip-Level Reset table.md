# 7.3.1 Chip-Level Reset table

[Table 527, "List of chip-level reset causes"](#page-492-2) shows the components reset by each of the chip-level reset sources. A dash (—) indicates no change caused by this source.

*Table 527. List of chip-level reset causes*

<span id="page-492-2"></span>

| Reset Source                | SW-DP | AON Scratch | POWMAN     | Power State | Double Tap | Rescue |
|-----------------------------|-------|-------------|------------|-------------|------------|--------|
| POR                         | reset | reset       | hard reset | → P0.0      | reset      | reset  |
| BOR                         | reset | reset       | hard reset | → P0.0      | reset      | reset  |
| EXTERNAL RESET (RUN)        | reset | reset       | hard reset | → P0.0      | —          | reset  |
| DEBUGGER RESET REQ          | —     | —           | hard reset | → P0.0      | —          | reset  |
| DEBUGGER RESCUE             | —     | —           | hard reset | → P0.0      | —          | set    |
| WATCHDOG POWMAN ASYNC RESET | —     | —           | hard reset | → P0.0      | —          | —      |
| WATCHDOG POWMAN RESET       | —     | —           | soft reset | → P0.0      | —          | —      |
| WATCHDOG SWCORE RESET       | —     | —           | —          | → P0.0      | —          | —      |
| SWCORE POWERDOWN            | —     | —           | —          | → P0.x      | —          | —      |
| GLITCH_DETECTOR             | —     | —           | —          | —           | —          | —      |
| WATCHDOG RESET PSM          | —     | —           | —          | —           | —          | —      |
|                             |       |             |            |             |            |        |

All chip-level resets sources in the table also reset the Power-on State Machine (PSM). This asserts all of the system resets downstream of the PSM. System resets includes low-level chip infrastructure like the system-level clock generators, as well as the processor cold and warm reset domains.

All chip-level reset sources in the table also reset the system watchdog peripheral. This includes watchdog scratch registers [SCRATCH0](#page-1194-2) → [SCRATCH7.](#page-1194-2)

You can interpret the table columns as follows:

#### **Reset Source**

Indicates which of the events listed in [Chip-level Reset Sources](#page-493-1) is responsible for this chip-level reset.

#### **SW-DP**

Indicates the SWD Debug Port and the RP-AP [\(Section 3.5.10, "RP-AP"](#page-92-0)) are reset.

#### **AON Scratch**

Indicates scratch register state in POWMAN [SCRATCH0](#page-484-2) → [SCRATCH7](#page-484-2) and [BOOT0](#page-484-0) → [BOOT3](#page-484-0) registers is lost. These registers are always-on, meaning they are preserved across power-down of the switched core domain.

7.3. Chip Level Resets **492**

#### **POWMAN**

Indicates some or all of the register state of the power manager (POWMAN) is reset.

#### **Power State**

Indicates a change to the powered/unpowered status of core voltage domains.

#### **Double Tap**

Indicates the [CHIP\\_RESET.](#page-467-0)DOUBLE\_TAP bit is reset.

#### **Rescue**

Indicates changes to the [CHIP\\_RESET](#page-467-0).RESCUE\_FLAG bit.

