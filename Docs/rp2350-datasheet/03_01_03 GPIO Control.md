# 3.1.3 GPIO Control

The SIO GPIO registers control GPIOs which have the SIO function selected (function 5). This function is supported on the following pins:

- all user GPIOs (GPIOs 0 through 29, or 0 through 47, depending on package option)
- QSPI pins
- USB DP/DM pins

All SIO GPIO control registers come in pairs. The lower-addressed register in each pair (e.g. [GPIO\\_IN](#page-61-0)) is connected to GPIOs 0 through 31, and the higher-addressed register in each pair (e.g. [GPIO\\_HI\\_IN\)](#page-61-1) is connected to GPIOs 32 through 47, the QSPI pins, and the USB DP/DM pins.

# **NOTE**

To drive a pin with the SIO's GPIO registers, the GPIO multiplexer for this pin must first be configured to select the SIO GPIO function. See [Table 645](#page-590-0).

These GPIO registers are *shared* between the two cores: both cores can access them simultaneously. There are three groups of registers:

- Output registers, [GPIO\\_OUT](#page-61-2) and [GPIO\\_HI\\_OUT](#page-62-0) set the output level of the GPIO. 0 for low output, 1 for high output.
- Output enable registers, [GPIO\\_OE](#page-64-0) and [GPIO\\_HI\\_OE,](#page-64-1) are used to enable the output driver. 0 for high-impedance, 1 for drive high or low based on [GPIO\\_OUT](#page-61-2) and [GPIO\\_HI\\_OUT.](#page-62-0)
- Input registers, [GPIO\\_IN](#page-61-0) and [GPIO\\_HI\\_IN,](#page-61-1) allow the processor to sample the current state of the GPIOs.

Reading [GPIO\\_IN](#page-61-0) returns up to 32 input values in a single read, and software then masks out individual pins it is interested in.

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_gpio/include/hardware/gpio.h](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_gpio/include/hardware/gpio.h#L859-L869) Lines 859 - 869*

```
859 static inline bool gpio_get(uint gpio) {
860 #ifdef NUM_BANK0_GPIOS <= 32
861 return sio_hw->gpio_in & (1u << gpio);
862 #else
863 if (gpio < 32) {
864 return sio_hw->gpio_in & (1u << gpio);
865 } else {
866 return sio_hw->gpio_hi_in & (1u << (gpio - 32));
867 }
868 #endif
869 }
```

The OUT and OE registers also have atomic SET, CLR, and XOR aliases. This allows software to update a subset of the pins in one operation. This ensures safety for concurrent GPIO access, both between the two cores and between a single core's interrupt handler and foreground code.

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_gpio/include/hardware/gpio.h](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_gpio/include/hardware/gpio.h#L908-L914) Lines 908 - 914*

```
908 static inline void gpio_set_mask(uint32_t mask) {
909 #ifdef PICO_USE_GPIO_COPROCESSOR
910 gpioc_lo_out_set(mask);
```

```
911 #else
912 sio_hw->gpio_set = mask;
913 #endif
914 }
```

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_gpio/include/hardware/gpio.h](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_gpio/include/hardware/gpio.h#L955-L961) Lines 955 - 961*

```
955 static inline void gpio_clr_mask(uint32_t mask) {
956 #ifdef PICO_USE_GPIO_COPROCESSOR
957 gpioc_lo_out_clr(mask);
958 #else
959 sio_hw->gpio_clr = mask;
960 #endif
961 }
```

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_gpio/include/hardware/gpio.h](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_gpio/include/hardware/gpio.h#L1145-L1170) Lines 1145 - 1170*

```
1145 static inline void gpio_put(uint gpio, bool value) {
1146 #ifdef PICO_USE_GPIO_COPROCESSOR
1147 gpioc_bit_out_put(gpio, value);
1148 #elif NUM_BANK0_GPIOS <= 32
1149 uint32_t mask = 1ul << gpio;
1150 if (value)
1151 gpio_set_mask(mask);
1152 else
1153 gpio_clr_mask(mask);
1154 #else
1155 uint32_t mask = 1ul << (gpio & 0x1fu);
1156 if (gpio < 32) {
1157 if (value) {
1158 sio_hw->gpio_set = mask;
1159 } else {
1160 sio_hw->gpio_clr = mask;
1161 }
1162 } else {
1163 if (value) {
1164 sio_hw->gpio_hi_set = mask;
1165 } else {
1166 sio_hw->gpio_hi_clr = mask;
1167 }
1168 }
1169 #endif
1170 }
```

If both processors write to an OUT or OE register (or any of its SET/CLR/XOR aliases) on the same clock cycle, the result is as though core 0 wrote first, then core 1 wrote immediately afterward. For example, if core 0 SETs a bit and core 1 XORs it on the same clock cycle, the bit ends up with a value of 0.

# **NOTE**

This is a conceptual model for the result produced when two cores write to a GPIO register simultaneously. The register never contains the intermediate values at any point. In the previous example, if the pin is initially 0, and core 0 performs a SET while core 1 performs a XOR, the GPIO output remains low throughout the clock cycle.

As well as being shared between cores, the GPIO registers are also shared between security domains. The Secure and Non-secure SIO offer alternative views of the same GPIO registers, which are always mapped as GPIO function 5. However, the Non-secure SIO can only access pins which are enabled in the GPIO Non-secure mask configured by the ACCESSCTRL registers [GPIO\\_NSMASK0](#page-828-0) and [GPIO\\_NSMASK1.](#page-828-1) The layout of the NSMASK registers matches the layout of the SIO registers — for example, QSPI\_SCK is bit 26 in both [GPIO\\_HI\\_IN](#page-61-1) and [GPIO\\_NSMASK1](#page-828-1).

When a pin is not enabled in Non-secure code:

- writes to the corresponding GPIO registers from a Non-secure context have no effect
- reads from a Non-secure context return zeroes
- reads and writes from a Secure context function as usual using the Secure bank

The GPIO coprocessor port ([Section 3.6.1\)](#page-101-0) provides dedicated instructions for accessing the SIO GPIO registers from the Cortex-M33 processors. This includes the ability to read and write 64 bits in a single operation.

