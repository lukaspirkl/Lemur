# 12.5.2 Programmer's Model

All GPIO pins on RP2350 can be used for PWM:

*Table 1129. Mapping of PWM channels to GPIO pins on RP2350. This is also shown in the main GPIO function table, [Table](#page-590-0) [645](#page-590-0)*

| GPIO        | 0  | 1  | 2  | 3  | 4   | 5   | 6   | 7   | 8  | 9  | 10 | 11 | 12  | 13  | 14  | 15  |
|-------------|----|----|----|----|-----|-----|-----|-----|----|----|----|----|-----|-----|-----|-----|
| PWM Channel | 0A | 0B | 1A | 1B | 2A  | 2B  | 3A  | 3B  | 4A | 4B | 5A | 5B | 6A  | 6B  | 7A  | 7B  |
| GPIO        | 16 | 17 | 18 | 19 | 20  | 21  | 22  | 23  | 24 | 25 | 26 | 27 | 28  | 29  | 30  | 31  |
| PWM Channel | 0A | 0B | 1A | 1B | 2A  | 2B  | 3A  | 3B  | 4A | 4B | 5A | 5B | 6A  | 6B  | 7A  | 7B  |
| GPIO        | 32 | 33 | 34 | 35 | 36  | 37  | 38  | 39  | 40 | 41 | 42 | 43 | 44  | 45  | 46  | 47  |
| PWM Channel | 8A | 8B | 9A | 9B | 10A | 10B | 11A | 11B | 8A | 8B | 9A | 9B | 10A | 10B | 11A | 11B |

- The first 16 PWM channels (8 × 2-channel slices) appear on GPIOs 0 through 15, in the order PWM0 A, PWM0 B, PWM1 A, and so on.
- This pattern repeats for GPIOs 16 through 31. GPIO16 is PWM0 A, GPIO17 is PWM0 B, and so on up to PWM7 B on GPIO31. GPIO30 and above are available only in the QFN-80 package.
- The remaining 8 PWM channels (4 × 2-channel slices) appear on GPIOs 32 through 39, and then repeat on GPIOs 40 through 47.
- If you select the same PWM output on two GPIO pins, the same signal appears on both.
- If you use B pin as an input and select it on multiple GPIO pins, the PWM slice sees the logical OR of those two GPIO inputs.

# **NOTE**

GPIOs 0 through 29 have the same channel assignment as RP2040 for pinout compatibility. This reduces the maximum number of independent PWM outputs in the QFN-60 package option of RP2350, but you can still use slices 8 through 11 for repeating timer interrupts in this package.

#### **12.5.2.1. Pulse Width Modulation**

The PWM hardware continuously compares an input value to a free-running counter. This produces a toggling output; the amount of time spent at the high output level corresponds to the input value. The fraction of time spent at the high signal level is known as the duty cycle of the signal.

The counting period is controlled by the TOP register, with a maximum possible period of 65536 cycles, as the counter and TOP are 16 bits in size. Use the CC register to configure input values.

*Figure 111. The counter repeatedly counts from 0 to TOP, forming a sawtooth shape. The counter is continuously compared with some input value. When the input value is higher than the counter, the output is driven high. Otherwise, the output is low. The output period T is defined by the TOP value of the counter, and how fast the counter is configured to count. The* **average** *output voltage, as a fraction of the IO power supply, is the input value divided by the counter period (TOP +*

![](_page_1076_Figure_2.jpeg)

This example shows the counting period and the A and B counter compare levels being configured on one of RP2350's PWM slices.

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/pwm/hello\\_pwm/hello\\_pwm.c](https://github.com/raspberrypi/pico-examples/blob/master/pwm/hello_pwm/hello_pwm.c#L14-L29) Lines 14 - 29*

```
14 // Tell GPIO 0 and 1 they are allocated to the PWM
15 gpio_set_function(0, GPIO_FUNC_PWM);
16 gpio_set_function(1, GPIO_FUNC_PWM);
17 
18 // Find out which PWM slice is connected to GPIO 0 (it's slice 0)
19 uint slice_num = pwm_gpio_to_slice_num(0);
20 
21 // Set period of 4 cycles (0 to 3 inclusive)
22 pwm_set_wrap(slice_num, 3);
23 // Set channel A output high for one cycle before dropping
24 pwm_set_chan_level(slice_num, PWM_CHAN_A, 1);
25 // Set initial B output high for three cycles before dropping
26 pwm_set_chan_level(slice_num, PWM_CHAN_B, 3);
27 // Set the PWM running
28 pwm_set_enabled(slice_num, true);
```

<span id="page-1076-0"></span>[Figure 112](#page-1076-0) shows how the PWM hardware operates once it has been configured.

![](_page_1076_Figure_7.jpeg)

![](_page_1076_Figure_8.jpeg)

By default, PWM slices count upward until they reach the value of the TOP register. After they reach the TOP value, they wrap to 0. Alternatively, set CSR\_PH\_CORRECT to 1 to enable **phase-correct mode**, where the counter counts downward after reaching TOP, until it reaches 0 again.

Phase-correct mode centres the pulse on the same point no matter the duty cycle; its phase is not a function of duty cycle. When phase-correct mode is enabled, the output frequency is halved. The slice spends two cycles at a count of TOP and two cycles at a count of 0 each PWM period.

*Figure 113. In phasecorrect mode, the counter counts back down from TOP to 0 once it reaches TOP.*

![](_page_1077_Figure_2.jpeg)

#### **12.5.2.2. 0% and 100% Duty Cycle**

The RP2350 PWM can produce toggle-free 0% and 100% duty cycle output.

*Figure 114. Glitch-free 0% duty cycle output for CC = 0, and glitchfree 100% duty cycle output for CC = TOP + 1*

![](_page_1077_Figure_6.jpeg)

A CC value of 0 produces a 0% output: the output signal is always low. A CC value of TOP + 1 (equal to the period when not phase-corrected) produces a 100% output. If TOP is 254, the counter has a period of 255 cycles, and CC values in the range of 0 to 255 inclusive will produce duty cycles in the range 0% to 100% inclusive.

Glitch-free output at 0% and 100% helps avoid switching losses, for instance, when a MOSFET is controlled at its minimum and maximum current levels.

#### **12.5.2.3. Double Buffering**

[Figure 115](#page-1078-0) shows how a change in input value produces a change in output duty cycle. This can approximate analogue waveforms such as a sine wave.

*Figure 115. The input value varies with each counter period: first TOP / 3, then 2 × TOP / 3, and finally TOP + 1 for 100% duty cycle. Each increase in the input value causes a corresponding increase in the output duty cycle.*

<span id="page-1078-0"></span>![](_page_1078_Figure_2.jpeg)

In [Figure 115,](#page-1078-0) the input value only changes at the instant where the counter wraps through 0. [Figure 116](#page-1078-1) shows what happens if the input value is allowed to change at any other time: an unwanted glitch is produced at the output.

*Figure 116. The input value changes whilst the counter is midramp. This produces additional toggling at the output.*

<span id="page-1078-1"></span>![](_page_1078_Figure_5.jpeg)

The behaviour becomes even more perplexing if the TOP register is also modified. It would be difficult for software to write to CC or TOP with the correct timing. To solve this, each slice has two copies of the CC and TOP registers: one copy which software can modify, and another, internal copy which is updated from the first register at the instant the counter wraps. Software can modify its copy of the register at will, but the changes are not captured by the PWM output until the next wrap.

[Figure 117](#page-1079-0) shows the sequence of events where a software interrupt handler changes the value of CC\_A each time the counter wraps.

*Figure 117. Each counter wrap causes the interrupt request signal to assert. The processor enters its interrupt handler, writes to its copy of the CC register, and clears the interrupt. When the counter wraps again, the latched version of the CC register is instantaneously updated with the most recent value written by software, and this value controls the duty cycle for the next period. The IRQ is reasserted so that software can write another fresh value to its copy of the CC register.*

*Figure 118. The clock divider generates an enable signal. The counter only counts on cycles where this signal is high. A clock divisor of 1 causes the enable to be asserted on every cycle, so the counter counts by one on every system clock cycle. Higher divisors cause the count enable to be asserted less frequently. Fractional division achieves an average fractional counting rate by spacing some enable pulses further apart than others.*

<span id="page-1079-0"></span>![](_page_1079_Figure_3.jpeg)

There is no limitation on what values can be written to CC or TOP, or when they are written. In normal PWM mode (CSR\_PH\_CORRECT is 0), the latched copies update when the counter wraps to 0, which occurs once every TOP + 1 cycles. In phase-correct mode (CSR\_PH\_CORRECT is 1), the latched copies update on the 0 to 0 count transition, when the counter stops counting downward and begins to count upward again.

#### **12.5.2.4. Clock Divider**

Each slice has a 8 integer bit, 4 fractional bit fractional clock divider configured by the DIV register. The clock divider allows you to slow the count rate by a factor of up to 256. To do this, the PWM generates an enable signal that gates counter operation. This allows you to achieve output frequencies significantly lower than the system clock. For instance, from a 125MHz system clock, the clock divider can slow the count rate to approximately 7.5Hz. Lower frequencies than this require a system timer interrupt ([Section 12.8\)](#page-1179-0).

![](_page_1079_Figure_7.jpeg)

The fractional divider is a first-order delta-sigma type.

The clock divider also extends the effective count range when using level-sensitive or edge-sensitive modes to take duty cycle or frequency measurements.

#### **12.5.2.5. Level-sensitive and Edge-sensitive Triggering**

The PWM provides the following counter modes:

- Default free-running, counting continuously whenever the slice is enabled (free-running)
- Count continuously when a high level is detected on the B pin (level sensitive)
- Count once with each rising edge detected on the B pin (rising edge-sensitive)
- Count once with each falling edge detected on the B pin (falling edge-sensitive)

*Figure 119. PWM slice event selection. The counter advances when its enable input is high. This enable is generated by two sequential stages. First, any one of four event types (always on, pin B high, pin B rise, pin B fall) can generate enable pulses for the fractional clock divider. The divider can reduce the rate of the enable pulses, before passing them on to the counter.*

![](_page_1080_Figure_2.jpeg)

Use the DIVMODE field in each slice's CSR to select a mode. In free-running mode, the A and B pins are both outputs. In any other mode, the B pin becomes an input that controls counter operation. CC\_B is ignored when not in free-running mode.

You can measure the duty cycle or frequency of an input signal by running the slice for a fixed amount of time in levelsensitive or edge-sensitive mode. Due to the type of edge-detect circuit used, the low period and high period of the measured signal must both be strictly greater than the system clock period when taking frequency measurements.

The clock divider still operates in level-sensitive and edge-sensitive modes. At maximum division (DIV\_INT is 0), the counter only advances once per 256 high input cycles in level-sensitive modes, or once per 256 edges in edge-sensitive mode. This allows you to take longer-running measurements, although the resolution is still 16 bits.

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/pwm/measure\\_duty\\_cycle/measure\\_duty\\_cycle.c](https://github.com/raspberrypi/pico-examples/blob/master/pwm/measure_duty_cycle/measure_duty_cycle.c#L19-L37) Lines 19 - 37*

```
19 float measure_duty_cycle(uint gpio) {
20 // Only the PWM B pins can be used as inputs.
21 assert(pwm_gpio_to_channel(gpio) == PWM_CHAN_B);
22 uint slice_num = pwm_gpio_to_slice_num(gpio);
23 
24 // Count once for every 100 cycles the PWM B input is high
25 pwm_config cfg = pwm_get_default_config();
26 pwm_config_set_clkdiv_mode(&cfg, PWM_DIV_B_HIGH);
27 pwm_config_set_clkdiv(&cfg, 100);
28 pwm_init(slice_num, &cfg, false);
29 gpio_set_function(gpio, GPIO_FUNC_PWM);
30 
31 pwm_set_enabled(slice_num, true);
32 sleep_ms(10);
33 pwm_set_enabled(slice_num, false);
34 float counting_rate = clock_get_hz(clk_sys) / 100;
35 float max_possible_count = counting_rate * 0.01;
36 return pwm_get_counter(slice_num) / max_possible_count;
37 }
```

#### **12.5.2.6. Configuring PWM Period**

When free-running, use the following three parameters to control the period of a PWM slice's output (measured in system clock cycles):

- The TOP register, which controls the maximum value of the counting period
- The CSR\_PH\_CORRECT bit, which enables phase-correct mode
- The DIV register, which controls the clock divider

The slice counts from 0 to TOP, then either wraps or begins counting backward, depending on the setting of CSR\_PH\_CORRECT. The clock divider slows the rate of counting, with a maximum speed of one count per cycle, and a minimum speed of one count per 256 cycles. Calculate the period in clock cycles with the following equation:

period = 
$$(TOP + 1) \times (CSR\_PH\_CORRECT + 1) \times \left(DIV\_INT + \frac{DIV\_FRAC}{16}\right)$$

To determine the output frequency based on the system clock frequency, use the following equation:

$$f_{PWM} = \frac{f_{sys}}{\text{period}} = \frac{f_{sys}}{(\text{TOP} + 1) \times (\text{CSR\_PH\_CORRECT} + 1) \times \left(\text{DIV\_INT} + \frac{\text{DIV\_FRAC}}{16}\right)}$$

Set DIV\_INT to 0 to divide the count rate by the maximum possible value of 256. You must not set any DIV\_FRAC bits when DIV\_INT is 0.

#### **12.5.2.7. Interrupt Request (IRQ) and DMA Data Request (DREQ)**

The PWM block has two IRQ outputs. The interrupt status registers INTR, INTS0, INTS1, INTE0 and INTE1 allow software to:

- control which slices assert each of the two IRQs
- check which slices caused the assertion of an IRQ
- clear and acknowledge the interrupt

A slice generates an interrupt request each time its counter wraps (or, in phase-correct mode, each time the counter returns to 0). This sets the flag corresponding to this slice in the raw interrupt status register, INTR. If this slice's interrupt is enabled in INTE, this flag causes the PWM block's IRQ to be asserted, and the flag also appears in the masked interrupt status register INTS.

To clear flags, write a mask back to INTR. This is demonstrated in the LED fade SDK example below:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/pwm/led\\_fade/pwm\\_led\\_fade.c](https://github.com/raspberrypi/pico-examples/blob/master/pwm/led_fade/pwm_led_fade.c)*

```
 1 /**
 2 * Copyright (c) 2020 Raspberry Pi (Trading) Ltd.
 3 *
 4 * SPDX-License-Identifier: BSD-3-Clause
 5 */
 6 
 7 // Fade an LED between low and high brightness. An interrupt handler updates
 8 // the PWM slice's output level each time the counter wraps.
 9 
10 #include "pico/stdlib.h"
11 #include <stdio.h>
12 #include "pico/time.h"
13 #include "hardware/irq.h"
14 #include "hardware/pwm.h"
15 
16 void on_pwm_wrap() {
17 static int fade = 0;
18 static bool going_up = true;
19 // Clear the interrupt flag that brought us here
20 pwm_clear_irq(pwm_gpio_to_slice_num(PICO_DEFAULT_LED_PIN));
21 
22 if (going_up) {
23 ++fade;
24 if (fade > 255) {
25 fade = 255;
26 going_up = false;
27 }
28 } else {
29 --fade;
30 if (fade < 0) {
31 fade = 0;
32 going_up = true;
```

```
33 }
34 }
35 // Square the fade value to make the LED's brightness appear more linear
36 // Note this range matches with the wrap value
37 pwm_set_gpio_level(PICO_DEFAULT_LED_PIN, fade * fade);
38 }
39 
40 int main() {
41 #ifndef PICO_DEFAULT_LED_PIN
42 #warning pwm/led_fade example requires a board with a regular LED
43 #else
44 // Tell the LED pin that the PWM is in charge of its value.
45 gpio_set_function(PICO_DEFAULT_LED_PIN, GPIO_FUNC_PWM);
46 // Figure out which slice we just connected to the LED pin
47 uint slice_num = pwm_gpio_to_slice_num(PICO_DEFAULT_LED_PIN);
48 
49 // Mask our slice's IRQ output into the PWM block's single interrupt line,
50 // and register our interrupt handler
51 pwm_clear_irq(slice_num);
52 pwm_set_irq_enabled(slice_num, true);
53 irq_set_exclusive_handler(PWM_DEFAULT_IRQ_NUM(), on_pwm_wrap);
54 irq_set_enabled(PWM_DEFAULT_IRQ_NUM(), true);
55 
56 // Get some sensible defaults for the slice configuration. By default, the
57 // counter is allowed to wrap over its maximum range (0 to 2**16-1)
58 pwm_config config = pwm_get_default_config();
59 // Set divider, reduces counter clock to sysclock/this value
60 pwm_config_set_clkdiv(&config, 4.f);
61 // Load the configuration into our PWM slice, and set it running.
62 pwm_init(slice_num, &config, true);
63 
64 // Everything after this point happens in the PWM interrupt handler, so we
65 // can twiddle our thumbs
66 while (1)
67 tight_loop_contents();
68 #endif
69 }
```

This scheme allows multiple slices to generate interrupts concurrently. A system interrupt handler determines which slices caused the most recent interruption, and handles them appropriately. Normally, this means reloading those slices' TOP or CC registers, but the PWM block can also be used as a source of regular interrupt requests for non-PWM purposes.

The same pulse which sets the interrupt flag in INTR is also available as a one-cycle data request to the RP2350 system DMA. For each cycle the DMA sees a DREQ asserted, it makes one data transfer to its programmed location in as timely a manner as possible. Combined with the double-buffered behaviour of CC and TOP, the DMA can efficiently stream data to a PWM slice at a rate of one transfer per counter period. Alternatively, a PWM slice could serve as a pacing timer for DMA transfers to some other memory-mapped hardware.

#### **12.5.2.8. On-the-fly Phase Adjustment**

For some applications, it is necessary to control the phase relationship between two PWM outputs on different slices.

The global enable register EN contains an alias of the CSR\_EN flag for each slice. Use this register to start and stop several slices simultaneously. If two slices with the same output frequency start at the same time, they run in perfect lockstep, with a fixed phase relationship determined by the initial counter values.

The CSR\_PH\_ADV and CSR\_PH\_RET fields advance or retard a slice's output phase by one count whilst it is running. They do so by inserting or deleting pulses from the clock enable (the output of the clock divider), as shown in [Figure 120.](#page-1083-1)

*Figure 120. The clock enable signal, output by the clock divider, controls the rate of counting. Phase advance forces the clock enable high on cycles where it is low, causing the counter to jump forward by one count. Phase retard forces the clock enable low when it would be high, holding the counter back by one count.*

<span id="page-1083-1"></span>![](_page_1083_Figure_2.jpeg)

The counter cannot count faster than once per cycle, so PH\_ADV requires DIV\_INT > 1 or DIV\_FRAC > 0. Likewise, the counter will not start to count backward if PH\_RET is asserted when the clock enable is permanently low.

To advance or retard the phase by one count, software writes 1 to PH\_ADV or PH\_RET. Once an enable pulse has been inserted or deleted, the PH\_ADV or PH\_RET register bit returns to 0. Software can poll CSR until this happens. PH\_ADV always inserts a pulse into the next available gap; PH\_RET always deletes the next available pulse.

