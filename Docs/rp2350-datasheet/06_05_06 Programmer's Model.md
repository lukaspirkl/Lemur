# 6.5.6 Programmer's Model

#### **6.5.6.1. Sleep**

The hello\_sleep example ([hello\\_sleep\\_aon.c](https://github.com/raspberrypi/pico-playground/blob/master/sleep/hello_sleep/hello_sleep_aon.c) [in the](https://github.com/raspberrypi/pico-playground/blob/master/sleep/hello_sleep/hello_sleep_aon.c) [pico-playground](https://github.com/raspberrypi/pico-playground/blob/master/sleep/hello_sleep/hello_sleep_aon.c) [GitHub repository\)](https://github.com/raspberrypi/pico-playground/blob/master/sleep/hello_sleep/hello_sleep_aon.c) demonstrates sleep mode. The hello\_sleep application (and underlying functions) takes the following steps:

- 1. Switches all clocks in the system to run from XOSC.
- 2. Configures an alarm in the AON Timer for 10 seconds in the future.
- 3. Sets the AON Timer clock as the only clock running in sleep mode using the SLEEP\_ENx registers (see [SLEEP\\_EN0](#page-547-0)).
- 4. Enables deep sleep in the processor.
- 5. Calls \_\_wfi on processor, which will put the processor into deep sleep until woken by the AON Timer interrupt.
- 6. After 10 seconds, the AON Timer interrupt clears the alarm and then calls a user supplied callback function.
- 7. The callback function ends the example application.

#### **NOTE**

To enter sleep mode, you must enable deep sleep on both proc0 and proc1, call \_\_wfi, and ensure the DMA is stopped.

hello\_sleep makes use of functions in pico\_sleep of the [Pico Extras.](https://github.com/raspberrypi/pico-extras) In particular, sleep\_goto\_sleep\_until puts the processor to sleep until woken up by an AON Timer time assumed to be in the future.

*Pico Extras: [https://github.com/raspberrypi/pico-extras/blob/master/src/rp2\\_common/pico\\_sleep/sleep.c](https://github.com/raspberrypi/pico-extras/blob/master/src/rp2_common/pico_sleep/sleep.c#L159-L183) Lines 159 - 183*

```
159 void sleep_goto_sleep_until(struct timespec *ts, aon_timer_alarm_handler_t callback)
160 {
161 
162 // We should have already called the sleep_run_from_dormant_source function
163 // This is only needed for dormancy although it saves power running from xosc while
  sleeping
164 //assert(dormant_source_valid(_dormant_source));
165 
166 clocks_hw->sleep_en0 = CLOCKS_SLEEP_EN0_CLK_REF_POWMAN_BITS;
167 clocks_hw->sleep_en1 = 0x0;
168 
169 aon_timer_enable_alarm(ts, callback, false);
170 
171 stdio_flush();
172 
173 // Enable deep sleep at the proc
174 processor_deep_sleep();
175 
176 // Go to sleep
177 __wfi();
178 }
```

#### <span id="page-489-0"></span>**6.5.6.2. DORMANT**

The hello\_dormant example, [hello\\_dormant\\_gpio.c](https://github.com/raspberrypi/pico-playground/blob/master/sleep/hello_dormant/hello_dormant_gpio.c) [in the](https://github.com/raspberrypi/pico-playground/blob/master/sleep/hello_dormant/hello_dormant_gpio.c) [pico-playground](https://github.com/raspberrypi/pico-playground/blob/master/sleep/hello_dormant/hello_dormant_gpio.c) [GitHub repository,](https://github.com/raspberrypi/pico-playground/blob/master/sleep/hello_dormant/hello_dormant_gpio.c) demonstrates the DORMANT state. The example takes the following steps:

- 1. Switches all clocks in the system to run from XOSC.
- 2. Configures a GPIO interrupt for the dormant\_wake hardware, which can wake both the ROSC and XOSC from dormant mode.
- 3. Puts the XOSC into dormant mode, which stops all processor execution (and all other clocked logic on the chip) immediately.
- 4. When GPIO 10 goes high, the XOSC restarts and program execution continues.

hello\_dormant uses sleep\_goto\_dormant\_until\_pin under the hood:

*Pico Extras: [https://github.com/raspberrypi/pico-extras/blob/master/src/rp2\\_common/pico\\_sleep/sleep.c](https://github.com/raspberrypi/pico-extras/blob/master/src/rp2_common/pico_sleep/sleep.c#L258-L282) Lines 258 - 282*

```
258 void sleep_goto_dormant_until_pin(uint gpio_pin, bool edge, bool high) {
259 bool low = !high;
260 bool level = !edge;
261 
262 // Configure the appropriate IRQ at IO bank 0
263 assert(gpio_pin < NUM_BANK0_GPIOS);
264 
265 uint32_t event = 0;
266 
267 if (level && low) event = IO_BANK0_DORMANT_WAKE_INTE0_GPIO0_LEVEL_LOW_BITS;
268 if (level && high) event = IO_BANK0_DORMANT_WAKE_INTE0_GPIO0_LEVEL_HIGH_BITS;
269 if (edge && high) event = IO_BANK0_DORMANT_WAKE_INTE0_GPIO0_EDGE_HIGH_BITS;
270 if (edge && low) event = IO_BANK0_DORMANT_WAKE_INTE0_GPIO0_EDGE_LOW_BITS;
271 
272 gpio_init(gpio_pin);
273 gpio_set_input_enabled(gpio_pin, true);
274 gpio_set_dormant_irq_enabled(gpio_pin, event, true);
275
```

```
276 _go_dormant();
277 // Execution stops here until woken up
278 
279 // Clear the irq so we can go back to dormant mode again if we want
280 gpio_acknowledge_irq(gpio_pin, event);
281 gpio_set_input_enabled(gpio_pin, false);
282 }
```

# <span id="page-491-0"></span>**Chapter 7. Resets**

