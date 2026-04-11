# 6.3.1 Operating Modes

The regulator has the following three modes of operation.

#### **6.3.1.1. Normal Mode**

In Normal mode, the regulator operates in a switching mode, and can supply up to 200mA. Normal mode is used for P0.x power states, when the chip's switched core is powered on. The regulator must be in Normal Mode *before* the core supply current is allowed to exceed 1mA. The regulator starts up in Normal mode when its input supplies are first applied.

#### **6.3.1.2. Low Power Mode**

In Low Power mode, the regulator operates in a linear mode, and can only supply up to 1mA. Low Power mode can be used for P1.x power states, where the chip's switched core is powered off. The core supply current must be less than 1mA *before* the regulator is moved to Low Power mode. The regulator's output voltage is limited to 1.3 V in Low Power mode.

#### **CAUTION**

In Low Power mode, the output of the regulator is directly connected to DVDD. It is not possible to disconnect the regulator from DVDD in this mode. Do not put the regulator into Low Power mode if DVDD is being powered from an external supply.

#### **6.3.1.3. High Impedance Mode**

In High Impedance mode, the regulator is disabled, its power consumption is minimised, and its outputs are set to a high impedance state. This mode should only be used if the digital core supply (DVDD) is provided by an external regulator. If the on-chip regulator is supplying DVDD, entering high impedance mode causes a reset event, returning the on-chip regulator to Normal mode.

