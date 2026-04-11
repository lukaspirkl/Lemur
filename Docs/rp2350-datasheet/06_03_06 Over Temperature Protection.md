# 6.3.6 Over Temperature Protection

The voltage regulator will terminate regulation, and disable its power transistors, if the transistor junction temperature rises above a threshold set by the HT\_TH field in the [VREG\\_CTRL](#page-460-0) register. The regulator will restart regulation when the transistor junction temperature drops to approximately 20°C below the temperature threshold.

