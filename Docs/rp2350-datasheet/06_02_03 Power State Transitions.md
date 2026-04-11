# 6.2.3 Power State Transitions

Transitions between power states can be initiated by software, hardware, or via the chip's debug subsystem. Once initiated, transitions are managed by autonomous power sequencers in the chip's AON power domain. The power sequencers can be configured, in a limited way, via the [SEQ\\_CFG](#page-471-0) register. The sequencers can also be observed and controlled, again in a limited way, via the RP-AP registers in the chip's debug subsystem. These registers are described in [Section 3.5.10](#page-92-0).

Valid power state transitions are as follows:

- all transitions from one P0.m state (switched core powered on) to another P0.m state (switched core powered on), if they increase or decrease the number of SRAM domains that are powered on
- all transitions from a P0.m state (switched core powered on) to a P1.m state (switched core powered off), except transitions that would result in a powered off SRAM domain becoming powered on
- all transitions from a P1.m state (switched core powered off) to a P0.m state (switched core powered on), except transitions that would result in a powered on SRAM domain becoming powered off

Transitions from one P1.m state (switched core powered off) to another P1.m state (switched core powered off) are not supported, and will be prevented by the hardware.

Valid transitions are shown in the table below.

*Table 475. valid power state transitions*

| From | To   |      |      |      |      |      |      |      |      |      |      |      |
|------|------|------|------|------|------|------|------|------|------|------|------|------|
| P0.0 |      | P0.1 | P0.2 | P0.3 | P1.0 | P1.1 | P1.2 | P1.3 | P1.4 | P1.5 | P1.6 | P1.7 |
| P0.1 | P0.0 |      |      | P0.3 |      | P1.1 |      | P1.3 |      | P1.5 |      | P1.7 |
| P0.2 | P0.0 |      |      | P0.3 |      |      | P1.2 | P1.3 |      |      | P1.6 | P1.7 |
| P0.3 | P0.0 | P0.1 | P0.2 |      |      |      |      | P1.3 |      |      |      | P1.7 |
| P1.0 | P0.0 |      |      |      |      |      |      |      |      |      |      |      |
| P1.1 | P0.0 | P0.1 |      |      |      |      |      |      |      |      |      |      |
| P1.2 | P0.0 |      | P0.2 |      |      |      |      |      |      |      |      |      |
| P1.3 | P0.0 | P0.1 | P0.2 | P0.3 |      |      |      |      |      |      |      |      |
| P1.4 | P0.0 |      |      |      |      |      |      |      |      |      |      |      |
| P1.5 | P0.0 | P0.1 |      |      |      |      |      |      |      |      |      |      |

| From | To   |      |      |      |  |  |  |  |  |  |
|------|------|------|------|------|--|--|--|--|--|--|
| P1.6 | P0.0 |      | P0.2 |      |  |  |  |  |  |  |
| P1.7 | P0.0 | P0.1 | P0.2 | P0.3 |  |  |  |  |  |  |

#### **6.2.3.1. Transitions from Normal Operating (P0.m) States**

Transitions from a Normal Operating (P0.m) state to either a Low Power (P1.m) state, or another Normal Operating (P0.m) state, are initiated by writing to the [STATE.](#page-472-0)REQ field. REQ is a 4-bit field representing the requested power state of the switched core and memory power domains. The [STATE](#page-472-0).WAITING field will be set immediately, followed by the the [STATE](#page-472-0).CHANGING field, once the actual state change starts. If a transition to a Low Power (P0.m) state is requested, WAITING will remain set until the processors have gone into a low power state (via \_\_wfi()). In the WAITING state, writing to the [STATE.](#page-472-0)REQ field can change or cancel the initial request. The requested state can not be changed once in the CHANGING state.

A request to move to an unsupported state, or a state that would result in an invalid transition, causes the [STATE](#page-472-0).BAD\_SW\_REQ field to be set.

If a hardware power up request is received while in the WAITING state, the transition requested via [STATE.](#page-472-0)REQ will be halted and the power up request completed. The [STATE.](#page-472-0)PWRUP\_WHILE\_WAITING and [STATE](#page-472-0).REQ\_IGNORED fields will be set.

On writing to [STATE](#page-472-0).REQ:

- If there is a pending power up request, [STATE](#page-472-0).REQ\_IGNORED is set and no further action is taken
- If the requested state is invalid, [STATE](#page-472-0).BAD\_SW\_REQ is set and no further action is taken
- If the switched core is being powered off, [STATE.](#page-472-0)WAITING is set until both processors enter \_\_wfi(). After which [STATE](#page-472-0).CHANGING will be set, but no processors will be powered up to read the flag at this time
  - If there is a power up request while in [STATE.](#page-472-0)WAITING, [STATE](#page-472-0).PWRUP\_WHILE\_WAITING is set, which can also raise an interrupt to bring the processors out of \_\_wfi(). No further action is taken
  - You can get out of the WAITING state by writing a new request to [STATE.](#page-472-0)REQ before both processors have gone into \_\_wfi()
- Any state request that isn't powering down the switched core, such as powering up or down SRAM domain 0 or 1 starts immediately. Software should wait until [STATE](#page-472-0).CHANGING has cleared to know the power down sequence. Once the [STATE.](#page-472-0)CHANGING flag is cleared [STATE.](#page-472-0)CURRENT is updated.
- If powering up, software should also wait for [STATE](#page-472-0).CHANGING to make sure everything is powered up before continuing. In practice this is handled by the RP2350 bootrom.

Invalid state transitions are:

- any combination of power up and power down requests
- any request which would result in power down of XIP/bootRAM and power up of swcore

If XIP, boot RAM, sram0, or sram1 remain powered while swcore is powered off, the sram will automatically switch to a low power state. Stored data will be retained.

Before transitioning to a switched-core power down state (P1.m), software needs to configure:

- the GPIO wakeup conditions if required
- the wakeup alarm if required
- the return state of the SRAM0 & SRAM1 domains

#### **6.2.3.2. Transitions from Low Power (P1.m) States**

Transitions from **P1.m** to **P0.m** states are initiated by GPIO events or the timer alarm.

There are up to 5 wakeup sources:

- up to 4 GPIO wakeups (level high/low or falling edge/rising edge)
- 1 alarm wakeup

GPIO wakeups are configured by the [PWRUP0](#page-478-0)-[PWRUP3](#page-481-0) registers. The wakeups are not enabled until the power sequencer completes the power down operation.

The alarm wakeup is configured by writing to the [ALARM\\_TIME\\_15TO0](#page-477-0)[-ALARM\\_TIME\\_63TO48](#page-477-1) registers. The alarm wakeup has a resolution of 1ms. Once set, the alarm wakeup is armed by writing a 1 to *both* [TIMER.](#page-478-1)PWRUP\_ON\_ALARM and [TIMER.](#page-478-1)ALARM\_ENAB. If the alarm fires during the power down sequence, a power up sequence will start when the power down sequence completes.

The [LAST\\_SWCORE\\_PWRUP](#page-482-0) register indicates which event caused the most recent power up.

#### **6.2.3.3. Debugger Initiated Power State Transitions**

The debugger can be used to trigger a power up sequence via the CSYSPWRUPREQ output from the SW-DP CTRL/STAT register. This powers all domains (i.e. returns to state P0.0) and also inhibits any further software initiated power state transitions.

When CSYSPWRUPREQ is asserted, the power sequencer will:

- complete any power state transitions that are in progress
- return to power state P0.0
- assert CSYSPWRUPACK to signal completion to the debug host

If CSYSPWRUPREQ is de-asserted then software initiated power transitions will be able to resume. The user can detect when a software requested transition is ignored because of CSYSPWRUPREQ using the following hints:

- Getting a [STATE](#page-472-0).REQ\_IGNORED after a write to [STATE.](#page-472-0)REQ
- [CURRENT\\_PWRUP\\_REQ](#page-482-1) will have bit 5 (coresight) set
- Either:
  - Get the debugger to de-assert CSYSPWRUPREQ or
  - Mask out CSYSPWRUPREQ by setting [DBG\\_PWRCFG.](#page-483-1)IGNORE

#### **NOTE**

[DBG\\_PWRCFG](#page-483-1).IGNOREis useful to test going to sleep with a debugger attached or ignoring CSYSPWRUPREQ. A debugger will likely leave CSYSPWRUPREQ set when disconnecting. It would be impossible to go to sleep after this without [DBG\\_PWRCFG](#page-483-1).IGNORE.

#### **6.2.3.4. Power mode aware GPIO Control**

The power manager sequencer is able to switch the state of two GPIO outputs on entry to and exit from a P1.m state, i.e. one where the switched core is powered down. This allows external devices to be power-aware. The GPIOs switch to indicate the low power state *after* the core is powered down and switch to indicate the high power state *before* the core is powered up. This ensures the high power state of the external components always overlaps the high power state of the core. The GPIOs are configured by the [EXT\\_CTRL0](#page-473-0) and [EXT\\_CTRL1](#page-474-0) registers.

#### **6.2.3.5. Isolation**

When powering down swcore, the pad control and data signals are latched and isolated from the IO logic. This avoids transitions on pads which could potentially corrupt external components. On swcore power up, the isolation is not released automatically. The user releases the isolation by clearing the ISO field of the pad control register (for example [GPIO0.](#page-784-1)ISO) once the IO logic has been configured.

