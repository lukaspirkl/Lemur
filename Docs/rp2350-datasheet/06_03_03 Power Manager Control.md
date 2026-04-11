# 6.3.3 Power Manager Control

The regulator's operating mode and output voltage can also be controlled by the Power Manager. Power Manager control is typically used when the chip enters or exits a low power (P1.x) state, when software may not be running.

In addition to Normal and High Impedance modes, Power Manager control allows the regulator to be placed in Low Power mode. By default, the regulator switches to Low Power mode when entering a low power (P1.x) state, and returns to Normal mode when returning to a normal (P0.x) state.

The operating mode and output voltage in the low power state are set by the values in the [VREG\\_LP\\_ENTRY](#page-462-0) register. And the operating mode and output voltage to be used when the chip has returned to a normal state are set by values in the [VREG\\_LP\\_EXIT](#page-463-0) register. The registers contain an additional MODE field that allows Low Power mode to be selected.

The values in the registers must be written by software *before* requesting a transition to a low power state, as software will not be running during or after the transition. The actual transitions to and from the low power state are handled by the Power Manager. Once the chip has returned to a normal state, software can be run and the regulator controlled directly. The values in the [VREG](#page-461-0) register reflect the regulator's current operating mode and output voltage once the chip has returned to a normal state.

# **CAUTION**

Low Power mode should only be used when the regulator is providing the chip's digital core supply (DVDD), as the regulator's low power output is connected to DVDD on chip.

