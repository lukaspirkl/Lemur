# 6.1.5 ADC Supply (ADC\_AVDD)

ADC\_AVDD supplies the chip's Analogue to Digital Converter (ADC). It can be powered at a nominal voltage between 1.8 V and 3.3 V, but the performance of the ADC will be compromised at voltages below 2.97 V. To reduce the number of external power supplies, ADC\_AVDD can use the same power source as the core voltage regulator analogue supply (VREG\_AVDD) or digital IO supply (IOVDD).

#### **NOTE**

It is safe to supply ADC\_AVDD at a higher or lower voltage than IOVDD, e.g. to power the ADC at 3.3 V, for optimum performance, while supporting 1.8 V signal levels on the digital IO. But the voltage on the ADC analogue inputs must not exceed IOVDD, e.g. if IOVDD is powered at 1.8 V, the voltage on the ADC inputs should be limited to 1.8 V. Voltages greater than IOVDD will result in leakage currents through the ESD protection diodes. See [Section 14.9](#page-1334-0) for details.

ADC\_AVDD should be decoupled with a 100nF capacitor close to the chip's ADC\_AVDD pin.

#### <span id="page-440-2"></span>**6.1.6. Core Voltage Regulator Input Supply (VREG\_VIN)**

VREG\_VIN is the input supply for the on-chip core voltage regulator, and should be in the range 2.7 V to 5.5 V. To reduce the number of external power supplies, VREG\_VIN can use the same power source as the voltage regulator analogue supply (VREG\_AVDD), or digital IO supply (IOVDD). Though care should be taken to minimise the noise on VREG\_AVDD.

A 4.7μF capacitor should be connected between VREG\_VIN and ground close to the chip's VREG\_VIN pin.

For more details on the on-chip voltage regulator see [Section 6.3](#page-446-0).

#### <span id="page-440-3"></span>**6.1.7. On-Chip Voltage Regulator Analogue Supply (VREG\_AVDD)**

VREG\_AVDD supplies the on chip voltage regulator's analogue control circuits, and should be powered at a nominal 3.3 V. To reduce the number of external power supplies, VREG\_AVDD can use the same power source as the voltage regulator input supply (VREG\_VIN), or the digital IO supply (IOVDD). Though care should be taken to minimise the noise on VREG\_AVDD. A passive low pass filter may be required, see [Section 6.3.7](#page-448-3) for details.

#### **NOTE**

VREG\_AVDD also powers the chip's power-on reset and brownout detection blocks, so it must be powered even if the on-chip voltage regulator is not used.

6.1. Power Supplies **440**

