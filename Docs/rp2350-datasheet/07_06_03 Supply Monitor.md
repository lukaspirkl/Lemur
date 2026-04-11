# 7.6.3 Supply Monitor

The power-on and brownout reset blocks are powered by the core voltage regulator's analogue supply (VREG\_AVDD). The blocks are initialised when power is first applied, but may not be reliably re-initialised if power is removed and then reapplied before VREG\_AVDD has dropped to a sufficiently low level. To prevent this happening, VREG\_AVDD is monitored and the power-on reset block is re-initialised if it drops below the **VREG\_AVDD activation threshold** (VREG\_AVDDTH.ACTIVE). VREG\_AVDDTH.ACTIVE is fixed at a nominal 1.1V, which should result in a threshold between 0.87V and 1.26V. This threshold does not represent a safe operating voltage. Instead, it represents the voltage that VREG\_AVD must drop below to reliably re-initialise the power-on reset block. For safe operation, VREG\_AVDD must be at a nominal voltage of 3.3V. See [Table 1440, "Power Supply Specifications".](#page-1339-1)

#### **7.6.3.1. Detailed Specifications**

*Table 539. Voltage Regulator Input Supply Monitor Parameters*

| Parameter         | Description                      | Min  | Typ | Max  | Units |
|-------------------|----------------------------------|------|-----|------|-------|
| VREG_VINTH.ACTIVE | VREG_VIN activation<br>threshold | 0.87 | 1.1 | 1.26 | V     |

