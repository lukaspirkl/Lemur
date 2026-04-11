# 6.3.2 Software Control

![](_page_447_Picture_3.jpeg)

Once enabled, software control *cannot be disabled*.

The regulator can be directly controlled by software, but must first be unlocked by writing a 1 to the UNLOCK field in the [VREG\\_CTRL](#page-460-0) register. Once unlocked, the regulator can be controlled via the [VREG](#page-461-0) register.

The regulator's operating mode defaults to Normal, at initial power up or after a reset event, but can be switched to High Impedance by writing a 1 to the [VREG](#page-461-0) register's HIZ field. The regulator's output voltage can be set by writing to the register's VSEL field, see the [VREG](#page-461-0) register description for details on available settings. To prevent accidental overvoltage, the output voltage is limited to 1.3 V unless the DISABLE\_VOLTAGE\_LIMIT field in the [VREG\\_CTRL](#page-460-0) is set. The output voltage defaults to 1.1 V at initial power-on or after a reset event.

The UPDATE\_IN\_PROGRESS field in the [VREG](#page-461-0) register is set while the regulator's operating mode or output voltage are being updated. When UPDATE\_IN\_PROGRESS is set, writes to the register are ignored.

It is not possible to place the regulator in Low Power mode under software control, as the load current will exceed 1mA when software is running.

# **CAUTION**

The regulator's output voltage can be varied between 0.55 V and 3.3 V, but RP2350 may not operate reliably with its digital core supply (DVDD) at a voltage other than 1.1 V.

