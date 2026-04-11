# 9.9 GPIO Coprocessor Port

Coprocessor port 0 on each Cortex-M33 processor connects to a GPIO coprocessor interface. These coprocessor instructions provide fast access to the SIO GPIO registers from Arm software:

- The equivalent of any SIO GPIO register access is a single instruction, without having to materialise a 32-bit register address beforehand
- An indexed write operation on any single GPIO is a single instruction
- 64 bits can be read/written in a single instruction

This reduces the timing impact of GPIO accesses on surrounding software, for example when GPIO tracing has been added to interrupt handlers diagnose complex timing issues.

Both Secure and Non-secure code may access the coprocessor. Non-secure code sees a restricted view of the GPIO registers, defined by ACCESSCTRL [GPIO\\_NSMASK0/](#page-828-0)1.

The GPIO coprocessor instruction set is documented in [Section 3.6.1.](#page-101-0)

# <span id="page-595-1"></span><span id="page-595-0"></span>**9.10.1. Select an IO function**

An IO pin can perform many different functions and must be configured before use. For example, you may want it to be a UART\_TX pin, or a PWM output. The SDK provides gpio\_set\_function for this purpose. Many SDK examples call gpio\_set\_function early on to enable printing to a UART.

The SDK starts by defining a structure to represent the registers of IO Bank 0, the User IO bank. Each IO has a status register, followed by a control register. For N IOs, the SDK instantiates the structure containing a status and control register as io[N] to repeat it N times.

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2350/hardware\\_structs/include/hardware/structs/io\\_bank0.h](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2350/hardware_structs/include/hardware/structs/io_bank0.h#L179-L445) Lines 179 - 445*

```
179 typedef struct {
180 io_bank0_status_ctrl_hw_t io[48];
181 
182 uint32_t _pad0[32];
183 
184 // (Description copied from array index 0 register IO_BANK0_IRQSUMMARY_PROC0_SECURE0
  applies similarly to other array indexes)
185 _REG_(IO_BANK0_IRQSUMMARY_PROC0_SECURE0_OFFSET) // IO_BANK0_IRQSUMMARY_PROC0_SECURE0
186 // 0x80000000 [31] GPIO31 (0)
187 // 0x40000000 [30] GPIO30 (0)
188 // 0x20000000 [29] GPIO29 (0)
189 // 0x10000000 [28] GPIO28 (0)
190 // 0x08000000 [27] GPIO27 (0)
191 // 0x04000000 [26] GPIO26 (0)
192 // 0x02000000 [25] GPIO25 (0)
193 // 0x01000000 [24] GPIO24 (0)
194 // 0x00800000 [23] GPIO23 (0)
195 // 0x00400000 [22] GPIO22 (0)
196 // 0x00200000 [21] GPIO21 (0)
197 // 0x00100000 [20] GPIO20 (0)
198 // 0x00080000 [19] GPIO19 (0)
199 // 0x00040000 [18] GPIO18 (0)
200 // 0x00020000 [17] GPIO17 (0)
201 // 0x00010000 [16] GPIO16 (0)
202 // 0x00008000 [15] GPIO15 (0)
203 // 0x00004000 [14] GPIO14 (0)
204 // 0x00002000 [13] GPIO13 (0)
205 // 0x00001000 [12] GPIO12 (0)
206 // 0x00000800 [11] GPIO11 (0)
207 // 0x00000400 [10] GPIO10 (0)
208 // 0x00000200 [9] GPIO9 (0)
209 // 0x00000100 [8] GPIO8 (0)
210 // 0x00000080 [7] GPIO7 (0)
211 // 0x00000040 [6] GPIO6 (0)
212 // 0x00000020 [5] GPIO5 (0)
213 // 0x00000010 [4] GPIO4 (0)
214 // 0x00000008 [3] GPIO3 (0)
215 // 0x00000004 [2] GPIO2 (0)
216 // 0x00000002 [1] GPIO1 (0)
217 // 0x00000001 [0] GPIO0 (0)
218 io_ro_32 irqsummary_proc0_secure[2];
219 
220 // (Description copied from array index 0 register IO_BANK0_IRQSUMMARY_PROC0_NONSECURE0
  applies similarly to other array indexes)
221 _REG_(IO_BANK0_IRQSUMMARY_PROC0_NONSECURE0_OFFSET) //
  IO_BANK0_IRQSUMMARY_PROC0_NONSECURE0
222 // 0x80000000 [31] GPIO31 (0)
```

```
223 // 0x40000000 [30] GPIO30 (0)
224 // 0x20000000 [29] GPIO29 (0)
225 // 0x10000000 [28] GPIO28 (0)
226 // 0x08000000 [27] GPIO27 (0)
227 // 0x04000000 [26] GPIO26 (0)
228 // 0x02000000 [25] GPIO25 (0)
229 // 0x01000000 [24] GPIO24 (0)
230 // 0x00800000 [23] GPIO23 (0)
231 // 0x00400000 [22] GPIO22 (0)
232 // 0x00200000 [21] GPIO21 (0)
233 // 0x00100000 [20] GPIO20 (0)
234 // 0x00080000 [19] GPIO19 (0)
235 // 0x00040000 [18] GPIO18 (0)
236 // 0x00020000 [17] GPIO17 (0)
237 // 0x00010000 [16] GPIO16 (0)
238 // 0x00008000 [15] GPIO15 (0)
239 // 0x00004000 [14] GPIO14 (0)
240 // 0x00002000 [13] GPIO13 (0)
241 // 0x00001000 [12] GPIO12 (0)
242 // 0x00000800 [11] GPIO11 (0)
243 // 0x00000400 [10] GPIO10 (0)
244 // 0x00000200 [9] GPIO9 (0)
245 // 0x00000100 [8] GPIO8 (0)
246 // 0x00000080 [7] GPIO7 (0)
247 // 0x00000040 [6] GPIO6 (0)
248 // 0x00000020 [5] GPIO5 (0)
249 // 0x00000010 [4] GPIO4 (0)
250 // 0x00000008 [3] GPIO3 (0)
251 // 0x00000004 [2] GPIO2 (0)
252 // 0x00000002 [1] GPIO1 (0)
253 // 0x00000001 [0] GPIO0 (0)
254 io_ro_32 irqsummary_proc0_nonsecure[2];
255 
256 // (Description copied from array index 0 register IO_BANK0_IRQSUMMARY_PROC1_SECURE0
  applies similarly to other array indexes)
257 _REG_(IO_BANK0_IRQSUMMARY_PROC1_SECURE0_OFFSET) // IO_BANK0_IRQSUMMARY_PROC1_SECURE0
258 // 0x80000000 [31] GPIO31 (0)
259 // 0x40000000 [30] GPIO30 (0)
260 // 0x20000000 [29] GPIO29 (0)
261 // 0x10000000 [28] GPIO28 (0)
262 // 0x08000000 [27] GPIO27 (0)
263 // 0x04000000 [26] GPIO26 (0)
264 // 0x02000000 [25] GPIO25 (0)
265 // 0x01000000 [24] GPIO24 (0)
266 // 0x00800000 [23] GPIO23 (0)
267 // 0x00400000 [22] GPIO22 (0)
268 // 0x00200000 [21] GPIO21 (0)
269 // 0x00100000 [20] GPIO20 (0)
270 // 0x00080000 [19] GPIO19 (0)
271 // 0x00040000 [18] GPIO18 (0)
272 // 0x00020000 [17] GPIO17 (0)
273 // 0x00010000 [16] GPIO16 (0)
274 // 0x00008000 [15] GPIO15 (0)
275 // 0x00004000 [14] GPIO14 (0)
276 // 0x00002000 [13] GPIO13 (0)
277 // 0x00001000 [12] GPIO12 (0)
278 // 0x00000800 [11] GPIO11 (0)
279 // 0x00000400 [10] GPIO10 (0)
280 // 0x00000200 [9] GPIO9 (0)
281 // 0x00000100 [8] GPIO8 (0)
282 // 0x00000080 [7] GPIO7 (0)
283 // 0x00000040 [6] GPIO6 (0)
284 // 0x00000020 [5] GPIO5 (0)
285 // 0x00000010 [4] GPIO4 (0)
```

```
286 // 0x00000008 [3] GPIO3 (0)
287 // 0x00000004 [2] GPIO2 (0)
288 // 0x00000002 [1] GPIO1 (0)
289 // 0x00000001 [0] GPIO0 (0)
290 io_ro_32 irqsummary_proc1_secure[2];
291 
292 // (Description copied from array index 0 register IO_BANK0_IRQSUMMARY_PROC1_NONSECURE0
  applies similarly to other array indexes)
293 _REG_(IO_BANK0_IRQSUMMARY_PROC1_NONSECURE0_OFFSET) //
  IO_BANK0_IRQSUMMARY_PROC1_NONSECURE0
294 // 0x80000000 [31] GPIO31 (0)
295 // 0x40000000 [30] GPIO30 (0)
296 // 0x20000000 [29] GPIO29 (0)
297 // 0x10000000 [28] GPIO28 (0)
298 // 0x08000000 [27] GPIO27 (0)
299 // 0x04000000 [26] GPIO26 (0)
300 // 0x02000000 [25] GPIO25 (0)
301 // 0x01000000 [24] GPIO24 (0)
302 // 0x00800000 [23] GPIO23 (0)
303 // 0x00400000 [22] GPIO22 (0)
304 // 0x00200000 [21] GPIO21 (0)
305 // 0x00100000 [20] GPIO20 (0)
306 // 0x00080000 [19] GPIO19 (0)
307 // 0x00040000 [18] GPIO18 (0)
308 // 0x00020000 [17] GPIO17 (0)
309 // 0x00010000 [16] GPIO16 (0)
310 // 0x00008000 [15] GPIO15 (0)
311 // 0x00004000 [14] GPIO14 (0)
312 // 0x00002000 [13] GPIO13 (0)
313 // 0x00001000 [12] GPIO12 (0)
314 // 0x00000800 [11] GPIO11 (0)
315 // 0x00000400 [10] GPIO10 (0)
316 // 0x00000200 [9] GPIO9 (0)
317 // 0x00000100 [8] GPIO8 (0)
318 // 0x00000080 [7] GPIO7 (0)
319 // 0x00000040 [6] GPIO6 (0)
320 // 0x00000020 [5] GPIO5 (0)
321 // 0x00000010 [4] GPIO4 (0)
322 // 0x00000008 [3] GPIO3 (0)
323 // 0x00000004 [2] GPIO2 (0)
324 // 0x00000002 [1] GPIO1 (0)
325 // 0x00000001 [0] GPIO0 (0)
326 io_ro_32 irqsummary_proc1_nonsecure[2];
327 
328 // (Description copied from array index 0 register
  IO_BANK0_IRQSUMMARY_DORMANT_WAKE_SECURE0 applies similarly to other array indexes)
329 _REG_(IO_BANK0_IRQSUMMARY_DORMANT_WAKE_SECURE0_OFFSET) //
  IO_BANK0_IRQSUMMARY_DORMANT_WAKE_SECURE0
330 // 0x80000000 [31] GPIO31 (0)
331 // 0x40000000 [30] GPIO30 (0)
332 // 0x20000000 [29] GPIO29 (0)
333 // 0x10000000 [28] GPIO28 (0)
334 // 0x08000000 [27] GPIO27 (0)
335 // 0x04000000 [26] GPIO26 (0)
336 // 0x02000000 [25] GPIO25 (0)
337 // 0x01000000 [24] GPIO24 (0)
338 // 0x00800000 [23] GPIO23 (0)
339 // 0x00400000 [22] GPIO22 (0)
340 // 0x00200000 [21] GPIO21 (0)
341 // 0x00100000 [20] GPIO20 (0)
342 // 0x00080000 [19] GPIO19 (0)
343 // 0x00040000 [18] GPIO18 (0)
344 // 0x00020000 [17] GPIO17 (0)
345 // 0x00010000 [16] GPIO16 (0)
```

```
346 // 0x00008000 [15] GPIO15 (0)
347 // 0x00004000 [14] GPIO14 (0)
348 // 0x00002000 [13] GPIO13 (0)
349 // 0x00001000 [12] GPIO12 (0)
350 // 0x00000800 [11] GPIO11 (0)
351 // 0x00000400 [10] GPIO10 (0)
352 // 0x00000200 [9] GPIO9 (0)
353 // 0x00000100 [8] GPIO8 (0)
354 // 0x00000080 [7] GPIO7 (0)
355 // 0x00000040 [6] GPIO6 (0)
356 // 0x00000020 [5] GPIO5 (0)
357 // 0x00000010 [4] GPIO4 (0)
358 // 0x00000008 [3] GPIO3 (0)
359 // 0x00000004 [2] GPIO2 (0)
360 // 0x00000002 [1] GPIO1 (0)
361 // 0x00000001 [0] GPIO0 (0)
362 io_ro_32 irqsummary_dormant_wake_secure[2];
363 
364 // (Description copied from array index 0 register
  IO_BANK0_IRQSUMMARY_DORMANT_WAKE_NONSECURE0 applies similarly to other array indexes)
365 _REG_(IO_BANK0_IRQSUMMARY_DORMANT_WAKE_NONSECURE0_OFFSET) //
  IO_BANK0_IRQSUMMARY_DORMANT_WAKE_NONSECURE0
366 // 0x80000000 [31] GPIO31 (0)
367 // 0x40000000 [30] GPIO30 (0)
368 // 0x20000000 [29] GPIO29 (0)
369 // 0x10000000 [28] GPIO28 (0)
370 // 0x08000000 [27] GPIO27 (0)
371 // 0x04000000 [26] GPIO26 (0)
372 // 0x02000000 [25] GPIO25 (0)
373 // 0x01000000 [24] GPIO24 (0)
374 // 0x00800000 [23] GPIO23 (0)
375 // 0x00400000 [22] GPIO22 (0)
376 // 0x00200000 [21] GPIO21 (0)
377 // 0x00100000 [20] GPIO20 (0)
378 // 0x00080000 [19] GPIO19 (0)
379 // 0x00040000 [18] GPIO18 (0)
380 // 0x00020000 [17] GPIO17 (0)
381 // 0x00010000 [16] GPIO16 (0)
382 // 0x00008000 [15] GPIO15 (0)
383 // 0x00004000 [14] GPIO14 (0)
384 // 0x00002000 [13] GPIO13 (0)
385 // 0x00001000 [12] GPIO12 (0)
386 // 0x00000800 [11] GPIO11 (0)
387 // 0x00000400 [10] GPIO10 (0)
388 // 0x00000200 [9] GPIO9 (0)
389 // 0x00000100 [8] GPIO8 (0)
390 // 0x00000080 [7] GPIO7 (0)
391 // 0x00000040 [6] GPIO6 (0)
392 // 0x00000020 [5] GPIO5 (0)
393 // 0x00000010 [4] GPIO4 (0)
394 // 0x00000008 [3] GPIO3 (0)
395 // 0x00000004 [2] GPIO2 (0)
396 // 0x00000002 [1] GPIO1 (0)
397 // 0x00000001 [0] GPIO0 (0)
398 io_ro_32 irqsummary_dormant_wake_nonsecure[2];
399 
400 // (Description copied from array index 0 register IO_BANK0_INTR0 applies similarly to
  other array indexes)
401 _REG_(IO_BANK0_INTR0_OFFSET) // IO_BANK0_INTR0
402 // Raw Interrupts
403 // 0x80000000 [31] GPIO7_EDGE_HIGH (0)
404 // 0x40000000 [30] GPIO7_EDGE_LOW (0)
405 // 0x20000000 [29] GPIO7_LEVEL_HIGH (0)
406 // 0x10000000 [28] GPIO7_LEVEL_LOW (0)
```

```
407 // 0x08000000 [27] GPIO6_EDGE_HIGH (0)
408 // 0x04000000 [26] GPIO6_EDGE_LOW (0)
409 // 0x02000000 [25] GPIO6_LEVEL_HIGH (0)
410 // 0x01000000 [24] GPIO6_LEVEL_LOW (0)
411 // 0x00800000 [23] GPIO5_EDGE_HIGH (0)
412 // 0x00400000 [22] GPIO5_EDGE_LOW (0)
413 // 0x00200000 [21] GPIO5_LEVEL_HIGH (0)
414 // 0x00100000 [20] GPIO5_LEVEL_LOW (0)
415 // 0x00080000 [19] GPIO4_EDGE_HIGH (0)
416 // 0x00040000 [18] GPIO4_EDGE_LOW (0)
417 // 0x00020000 [17] GPIO4_LEVEL_HIGH (0)
418 // 0x00010000 [16] GPIO4_LEVEL_LOW (0)
419 // 0x00008000 [15] GPIO3_EDGE_HIGH (0)
420 // 0x00004000 [14] GPIO3_EDGE_LOW (0)
421 // 0x00002000 [13] GPIO3_LEVEL_HIGH (0)
422 // 0x00001000 [12] GPIO3_LEVEL_LOW (0)
423 // 0x00000800 [11] GPIO2_EDGE_HIGH (0)
424 // 0x00000400 [10] GPIO2_EDGE_LOW (0)
425 // 0x00000200 [9] GPIO2_LEVEL_HIGH (0)
426 // 0x00000100 [8] GPIO2_LEVEL_LOW (0)
427 // 0x00000080 [7] GPIO1_EDGE_HIGH (0)
428 // 0x00000040 [6] GPIO1_EDGE_LOW (0)
429 // 0x00000020 [5] GPIO1_LEVEL_HIGH (0)
430 // 0x00000010 [4] GPIO1_LEVEL_LOW (0)
431 // 0x00000008 [3] GPIO0_EDGE_HIGH (0)
432 // 0x00000004 [2] GPIO0_EDGE_LOW (0)
433 // 0x00000002 [1] GPIO0_LEVEL_HIGH (0)
434 // 0x00000001 [0] GPIO0_LEVEL_LOW (0)
435 io_rw_32 intr[6];
436 
437 union {
438 struct {
439 io_bank0_irq_ctrl_hw_t proc0_irq_ctrl;
440 io_bank0_irq_ctrl_hw_t proc1_irq_ctrl;
441 io_bank0_irq_ctrl_hw_t dormant_wake_irq_ctrl;
442 };
443 io_bank0_irq_ctrl_hw_t irq_ctrl[3];
444 };
445 } io_bank0_hw_t;
```

A similar structure is defined for the pad control registers for IO bank 1. By default, all pads come out of reset ready to use, with input enabled and output disable set to 0. Regardless, gpio\_set\_function in the SDK sets the input enable and clears the output disable to engage the pad's IO buffers and connect internal signals to the outside world. Finally, the desired function select is written to the IO control register (see [GPIO0\\_CTRL](#page-607-0) for an example of an IO control register).

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_gpio/gpio.c](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_gpio/gpio.c#L36-L53) Lines 36 - 53*

```
36 // Select function for this GPIO, and ensure input/output are enabled at the pad.
37 // This also clears the input/output/irq override bits.
38 void gpio_set_function(uint gpio, gpio_function_t fn) {
39 check_gpio_param(gpio);
40 invalid_params_if(HARDWARE_GPIO, ((uint32_t)fn << IO_BANK0_GPIO0_CTRL_FUNCSEL_LSB) &
  ~IO_BANK0_GPIO0_CTRL_FUNCSEL_BITS);
41 // Set input enable on, output disable off
42 hw_write_masked(&pads_bank0_hw->io[gpio],
43 PADS_BANK0_GPIO0_IE_BITS,
44 PADS_BANK0_GPIO0_IE_BITS | PADS_BANK0_GPIO0_OD_BITS
45 );
46 // Zero all fields apart from fsel; we want this IO to do what the peripheral tells it.
47 // This doesn't affect e.g. pullup/pulldown, as these are in pad controls.
48 io_bank0_hw->io[gpio].ctrl = fn << IO_BANK0_GPIO0_CTRL_FUNCSEL_LSB;
49 // Remove pad isolation now that the correct peripheral is in control of the pad
```

```
50 hw_clear_bits(&pads_bank0_hw->io[gpio], PADS_BANK0_GPIO0_ISO_BITS);
51 }
```

### <span id="page-600-0"></span>**9.10.2. Enable a GPIO interrupt**

The SDK provides a method of being interrupted when a GPIO pin changes state:

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_gpio/gpio.c](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_gpio/gpio.c#L186-L196) Lines 186 - 196*

```
186 void gpio_set_irq_enabled(uint gpio, uint32_t events, bool enabled) {
187 // either this call disables the interrupt or callback should already be set.
188 // this protects against enabling the interrupt without callback set
189 assert(!enabled || irq_has_handler(IO_IRQ_BANK0));
190 
191 // Separate mask/force/status per-core, so check which core called, and
192 // set the relevant IRQ controls.
193 io_bank0_irq_ctrl_hw_t *irq_ctrl_base = get_core_num() ?
194 &io_bank0_hw->proc1_irq_ctrl : &io_bank0_hw-
  >proc0_irq_ctrl;
195 _gpio_set_irq_enabled(gpio, events, enabled, irq_ctrl_base);
196 }
```

gpio\_set\_irq\_enabled uses a lower level function \_gpio\_set\_irq\_enabled:

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_gpio/gpio.c](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_gpio/gpio.c#L173-L184) Lines 173 - 184*

```
173 static void _gpio_set_irq_enabled(uint gpio, uint32_t events, bool enabled,
  io_bank0_irq_ctrl_hw_t *irq_ctrl_base) {
174 // Clear stale events which might cause immediate spurious handler entry
175 gpio_acknowledge_irq(gpio, events);
176 
177 io_rw_32 *en_reg = &irq_ctrl_base->inte[gpio / 8];
178 events <<= 4 * (gpio % 8);
179 
180 if (enabled)
181 hw_set_bits(en_reg, events);
182 else
183 hw_clear_bits(en_reg, events);
184 }
```

The user provides a pointer to a callback function that is called when the GPIO event happens. An example application that uses this system is hello\_gpio\_irq:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/gpio/hello\\_gpio\\_irq/hello\\_gpio\\_irq.c](https://github.com/raspberrypi/pico-examples/blob/master/gpio/hello_gpio_irq/hello_gpio_irq.c)*

```
 1 /**
 2 * Copyright (c) 2020 Raspberry Pi (Trading) Ltd.
 3 *
 4 * SPDX-License-Identifier: BSD-3-Clause
 5 */
 6 
 7 #include <stdio.h>
 8 #include "pico/stdlib.h"
 9 #include "hardware/gpio.h"
10 
11 #define GPIO_WATCH_PIN 2
12
```

```
13 static char event_str[128];
14 
15 void gpio_event_string(char *buf, uint32_t events);
16 
17 void gpio_callback(uint gpio, uint32_t events) {
18 // Put the GPIO event(s) that just happened into event_str
19 // so we can print it
20 gpio_event_string(event_str, events);
21 printf("GPIO %d %s\n", gpio, event_str);
22 }
23 
24 int main() {
25 stdio_init_all();
26 
27 printf("Hello GPIO IRQ\n");
28 gpio_init(GPIO_WATCH_PIN);
29 gpio_set_irq_enabled_with_callback(GPIO_WATCH_PIN, GPIO_IRQ_EDGE_RISE |
  GPIO_IRQ_EDGE_FALL, true, &gpio_callback);
30 
31 // Wait forever
32 while (1);
33 }
34 
35 
36 static const char *gpio_irq_str[] = {
37 "LEVEL_LOW", // 0x1
38 "LEVEL_HIGH", // 0x2
39 "EDGE_FALL", // 0x4
40 "EDGE_RISE" // 0x8
41 };
42 
43 void gpio_event_string(char *buf, uint32_t events) {
44 for (uint i = 0; i < 4; i++) {
45 uint mask = (1 << i);
46 if (events & mask) {
47 // Copy this event string into the user string
48 const char *event_str = gpio_irq_str[i];
49 while (*event_str != '\0') {
50 *buf++ = *event_str++;
51 }
52 events &= ~mask;
53 
54 // If more events add ", "
55 if (events) {
56 *buf++ = ',';
57 *buf++ = ' ';
58 }
59 }
60 }
61 *buf++ = '\0';
62 }
```

#### <span id="page-601-1"></span><span id="page-601-0"></span>**9.11.1. IO - User Bank**

<span id="page-601-2"></span>The User Bank IO registers start at a base address of 0x40028000 (defined as [IO\\_BANK0\\_BASE](#page-31-1) in SDK).

*Table 648. List of IO\_BANK0 registers*

| Offset | Name          | Info |
|--------|---------------|------|
| 0x000  | GPIO0_STATUS  |      |
| 0x004  | GPIO0_CTRL    |      |
| 0x008  | GPIO1_STATUS  |      |
| 0x00c  | GPIO1_CTRL    |      |
| 0x010  | GPIO2_STATUS  |      |
| 0x014  | GPIO2_CTRL    |      |
| 0x018  | GPIO3_STATUS  |      |
| 0x01c  | GPIO3_CTRL    |      |
| 0x020  | GPIO4_STATUS  |      |
| 0x024  | GPIO4_CTRL    |      |
| 0x028  | GPIO5_STATUS  |      |
| 0x02c  | GPIO5_CTRL    |      |
| 0x030  | GPIO6_STATUS  |      |
| 0x034  | GPIO6_CTRL    |      |
| 0x038  | GPIO7_STATUS  |      |
| 0x03c  | GPIO7_CTRL    |      |
| 0x040  | GPIO8_STATUS  |      |
| 0x044  | GPIO8_CTRL    |      |
| 0x048  | GPIO9_STATUS  |      |
| 0x04c  | GPIO9_CTRL    |      |
| 0x050  | GPIO10_STATUS |      |
| 0x054  | GPIO10_CTRL   |      |
| 0x058  | GPIO11_STATUS |      |
| 0x05c  | GPIO11_CTRL   |      |
| 0x060  | GPIO12_STATUS |      |
| 0x064  | GPIO12_CTRL   |      |
| 0x068  | GPIO13_STATUS |      |
| 0x06c  | GPIO13_CTRL   |      |
| 0x070  | GPIO14_STATUS |      |
| 0x074  | GPIO14_CTRL   |      |
| 0x078  | GPIO15_STATUS |      |
| 0x07c  | GPIO15_CTRL   |      |
| 0x080  | GPIO16_STATUS |      |
| 0x084  | GPIO16_CTRL   |      |
| 0x088  | GPIO17_STATUS |      |
| 0x08c  | GPIO17_CTRL   |      |

| Offset | Name          | Info |
|--------|---------------|------|
| 0x090  | GPIO18_STATUS |      |
| 0x094  | GPIO18_CTRL   |      |
| 0x098  | GPIO19_STATUS |      |
| 0x09c  | GPIO19_CTRL   |      |
| 0x0a0  | GPIO20_STATUS |      |
| 0x0a4  | GPIO20_CTRL   |      |
| 0x0a8  | GPIO21_STATUS |      |
| 0x0ac  | GPIO21_CTRL   |      |
| 0x0b0  | GPIO22_STATUS |      |
| 0x0b4  | GPIO22_CTRL   |      |
| 0x0b8  | GPIO23_STATUS |      |
| 0x0bc  | GPIO23_CTRL   |      |
| 0x0c0  | GPIO24_STATUS |      |
| 0x0c4  | GPIO24_CTRL   |      |
| 0x0c8  | GPIO25_STATUS |      |
| 0x0cc  | GPIO25_CTRL   |      |
| 0x0d0  | GPIO26_STATUS |      |
| 0x0d4  | GPIO26_CTRL   |      |
| 0x0d8  | GPIO27_STATUS |      |
| 0x0dc  | GPIO27_CTRL   |      |
| 0x0e0  | GPIO28_STATUS |      |
| 0x0e4  | GPIO28_CTRL   |      |
| 0x0e8  | GPIO29_STATUS |      |
| 0x0ec  | GPIO29_CTRL   |      |
| 0x0f0  | GPIO30_STATUS |      |
| 0x0f4  | GPIO30_CTRL   |      |
| 0x0f8  | GPIO31_STATUS |      |
| 0x0fc  | GPIO31_CTRL   |      |
| 0x100  | GPIO32_STATUS |      |
| 0x104  | GPIO32_CTRL   |      |
| 0x108  | GPIO33_STATUS |      |
| 0x10c  | GPIO33_CTRL   |      |
| 0x110  | GPIO34_STATUS |      |
| 0x114  | GPIO34_CTRL   |      |
| 0x118  | GPIO35_STATUS |      |
| 0x11c  | GPIO35_CTRL   |      |

| Offset | Name                             | Info |
|--------|----------------------------------|------|
| 0x120  | GPIO36_STATUS                    |      |
| 0x124  | GPIO36_CTRL                      |      |
| 0x128  | GPIO37_STATUS                    |      |
| 0x12c  | GPIO37_CTRL                      |      |
| 0x130  | GPIO38_STATUS                    |      |
| 0x134  | GPIO38_CTRL                      |      |
| 0x138  | GPIO39_STATUS                    |      |
| 0x13c  | GPIO39_CTRL                      |      |
| 0x140  | GPIO40_STATUS                    |      |
| 0x144  | GPIO40_CTRL                      |      |
| 0x148  | GPIO41_STATUS                    |      |
| 0x14c  | GPIO41_CTRL                      |      |
| 0x150  | GPIO42_STATUS                    |      |
| 0x154  | GPIO42_CTRL                      |      |
| 0x158  | GPIO43_STATUS                    |      |
| 0x15c  | GPIO43_CTRL                      |      |
| 0x160  | GPIO44_STATUS                    |      |
| 0x164  | GPIO44_CTRL                      |      |
| 0x168  | GPIO45_STATUS                    |      |
| 0x16c  | GPIO45_CTRL                      |      |
| 0x170  | GPIO46_STATUS                    |      |
| 0x174  | GPIO46_CTRL                      |      |
| 0x178  | GPIO47_STATUS                    |      |
| 0x17c  | GPIO47_CTRL                      |      |
| 0x200  | IRQSUMMARY_PROC0_SECURE0         |      |
| 0x204  | IRQSUMMARY_PROC0_SECURE1         |      |
| 0x208  | IRQSUMMARY_PROC0_NONSECURE0      |      |
| 0x20c  | IRQSUMMARY_PROC0_NONSECURE1      |      |
| 0x210  | IRQSUMMARY_PROC1_SECURE0         |      |
| 0x214  | IRQSUMMARY_PROC1_SECURE1         |      |
| 0x218  | IRQSUMMARY_PROC1_NONSECURE0      |      |
| 0x21c  | IRQSUMMARY_PROC1_NONSECURE1      |      |
| 0x220  | IRQSUMMARY_COMA_WAKE_SECURE<br>0 |      |
| 0x224  | IRQSUMMARY_COMA_WAKE_SECURE<br>1 |      |

| Offset | Name                                | Info                                               |
|--------|-------------------------------------|----------------------------------------------------|
| 0x228  | IRQSUMMARY_COMA_WAKE_NONSE<br>CURE0 |                                                    |
| 0x22c  | IRQSUMMARY_COMA_WAKE_NONSE<br>CURE1 |                                                    |
| 0x230  | INTR0                               | Raw Interrupts                                     |
| 0x234  | INTR1                               | Raw Interrupts                                     |
| 0x238  | INTR2                               | Raw Interrupts                                     |
| 0x23c  | INTR3                               | Raw Interrupts                                     |
| 0x240  | INTR4                               | Raw Interrupts                                     |
| 0x244  | INTR5                               | Raw Interrupts                                     |
| 0x248  | PROC0_INTE0                         | Interrupt Enable for proc0                         |
| 0x24c  | PROC0_INTE1                         | Interrupt Enable for proc0                         |
| 0x250  | PROC0_INTE2                         | Interrupt Enable for proc0                         |
| 0x254  | PROC0_INTE3                         | Interrupt Enable for proc0                         |
| 0x258  | PROC0_INTE4                         | Interrupt Enable for proc0                         |
| 0x25c  | PROC0_INTE5                         | Interrupt Enable for proc0                         |
| 0x260  | PROC0_INTF0                         | Interrupt Force for proc0                          |
| 0x264  | PROC0_INTF1                         | Interrupt Force for proc0                          |
| 0x268  | PROC0_INTF2                         | Interrupt Force for proc0                          |
| 0x26c  | PROC0_INTF3                         | Interrupt Force for proc0                          |
| 0x270  | PROC0_INTF4                         | Interrupt Force for proc0                          |
| 0x274  | PROC0_INTF5                         | Interrupt Force for proc0                          |
| 0x278  | PROC0_INTS0                         | Interrupt status after masking & forcing for proc0 |
| 0x27c  | PROC0_INTS1                         | Interrupt status after masking & forcing for proc0 |
| 0x280  | PROC0_INTS2                         | Interrupt status after masking & forcing for proc0 |
| 0x284  | PROC0_INTS3                         | Interrupt status after masking & forcing for proc0 |
| 0x288  | PROC0_INTS4                         | Interrupt status after masking & forcing for proc0 |
| 0x28c  | PROC0_INTS5                         | Interrupt status after masking & forcing for proc0 |
| 0x290  | PROC1_INTE0                         | Interrupt Enable for proc1                         |
| 0x294  | PROC1_INTE1                         | Interrupt Enable for proc1                         |
| 0x298  | PROC1_INTE2                         | Interrupt Enable for proc1                         |
| 0x29c  | PROC1_INTE3                         | Interrupt Enable for proc1                         |
| 0x2a0  | PROC1_INTE4                         | Interrupt Enable for proc1                         |
| 0x2a4  | PROC1_INTE5                         | Interrupt Enable for proc1                         |
| 0x2a8  | PROC1_INTF0                         | Interrupt Force for proc1                          |
| 0x2ac  | PROC1_INTF1                         | Interrupt Force for proc1                          |

| Offset | Name               | Info                                                      |
|--------|--------------------|-----------------------------------------------------------|
| 0x2b0  | PROC1_INTF2        | Interrupt Force for proc1                                 |
| 0x2b4  | PROC1_INTF3        | Interrupt Force for proc1                                 |
| 0x2b8  | PROC1_INTF4        | Interrupt Force for proc1                                 |
| 0x2bc  | PROC1_INTF5        | Interrupt Force for proc1                                 |
| 0x2c0  | PROC1_INTS0        | Interrupt status after masking & forcing for proc1        |
| 0x2c4  | PROC1_INTS1        | Interrupt status after masking & forcing for proc1        |
| 0x2c8  | PROC1_INTS2        | Interrupt status after masking & forcing for proc1        |
| 0x2cc  | PROC1_INTS3        | Interrupt status after masking & forcing for proc1        |
| 0x2d0  | PROC1_INTS4        | Interrupt status after masking & forcing for proc1        |
| 0x2d4  | PROC1_INTS5        | Interrupt status after masking & forcing for proc1        |
| 0x2d8  | DORMANT_WAKE_INTE0 | Interrupt Enable for dormant_wake                         |
| 0x2dc  | DORMANT_WAKE_INTE1 | Interrupt Enable for dormant_wake                         |
| 0x2e0  | DORMANT_WAKE_INTE2 | Interrupt Enable for dormant_wake                         |
| 0x2e4  | DORMANT_WAKE_INTE3 | Interrupt Enable for dormant_wake                         |
| 0x2e8  | DORMANT_WAKE_INTE4 | Interrupt Enable for dormant_wake                         |
| 0x2ec  | DORMANT_WAKE_INTE5 | Interrupt Enable for dormant_wake                         |
| 0x2f0  | DORMANT_WAKE_INTF0 | Interrupt Force for dormant_wake                          |
| 0x2f4  | DORMANT_WAKE_INTF1 | Interrupt Force for dormant_wake                          |
| 0x2f8  | DORMANT_WAKE_INTF2 | Interrupt Force for dormant_wake                          |
| 0x2fc  | DORMANT_WAKE_INTF3 | Interrupt Force for dormant_wake                          |
| 0x300  | DORMANT_WAKE_INTF4 | Interrupt Force for dormant_wake                          |
| 0x304  | DORMANT_WAKE_INTF5 | Interrupt Force for dormant_wake                          |
| 0x308  | DORMANT_WAKE_INTS0 | Interrupt status after masking & forcing for dormant_wake |
| 0x30c  | DORMANT_WAKE_INTS1 | Interrupt status after masking & forcing for dormant_wake |
| 0x310  | DORMANT_WAKE_INTS2 | Interrupt status after masking & forcing for dormant_wake |
| 0x314  | DORMANT_WAKE_INTS3 | Interrupt status after masking & forcing for dormant_wake |
| 0x318  | DORMANT_WAKE_INTS4 | Interrupt status after masking & forcing for dormant_wake |
| 0x31c  | DORMANT_WAKE_INTS5 | Interrupt status after masking & forcing for dormant_wake |

# <span id="page-606-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO0\_STATUS Register**

**Offset**: 0x000

*Table 649. GPIO0\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |

| Bits  | Description                                                       | Type | Reset |
|-------|-------------------------------------------------------------------|------|-------|
| 16:14 | Reserved.                                                         | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied  | RO   | 0x0   |
| 12:10 | Reserved.                                                         | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied | RO   | 0x0   |
| 8:0   | Reserved.                                                         | -    | -     |

# <span id="page-607-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO0\_CTRL Register**

#### **Offset**: 0x004

*Table 650. GPIO0\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |

| Bits | Description                                                                    | Type | Reset |
|------|--------------------------------------------------------------------------------|------|-------|
|      | 0x2 → LOW: drive output low                                                    |      |       |
|      | 0x3 → HIGH: drive output high                                                  |      |       |
| 11:5 | Reserved.                                                                      | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL | RW   | 0x1f  |
|      | Enumerated values:                                                             |      |       |
|      | 0x00 → JTAG_TCK                                                                |      |       |
|      | 0x01 → SPI0_RX                                                                 |      |       |
|      | 0x02 → UART0_TX                                                                |      |       |
|      | 0x03 → I2C0_SDA                                                                |      |       |
|      | 0x04 → PWM_A_0                                                                 |      |       |
|      | 0x05 → SIO_0                                                                   |      |       |
|      | 0x06 → PIO0_0                                                                  |      |       |
|      | 0x07 → PIO1_0                                                                  |      |       |
|      | 0x08 → PIO2_0                                                                  |      |       |
|      | 0x09 → XIP_SS_N_1                                                              |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                              |      |       |
|      | 0x1f → NULL                                                                    |      |       |

# <span id="page-608-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO1\_STATUS Register**

**Offset**: 0x008

*Table 651. GPIO1\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

#### <span id="page-608-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO1\_CTRL Register**

**Offset**: 0x00c

*Table 652. GPIO1\_CTRL Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:30 | Reserved.   | -    | -     |

| 29:28<br>IRQOVER<br>RW<br>0x0<br>Enumerated values:<br>0x0 → NORMAL: don't invert the interrupt<br>0x1 → INVERT: invert the interrupt<br>0x2 → LOW: drive interrupt low<br>0x3 → HIGH: drive interrupt high<br>27:18<br>Reserved.<br>-<br>-<br>17:16<br>INOVER<br>RW<br>0x0<br>Enumerated values:<br>0x0 → NORMAL: don't invert the peri input<br>0x1 → INVERT: invert the peri input<br>0x2 → LOW: drive peri input low<br>0x3 → HIGH: drive peri input high<br>15:14<br>OEOVER<br>RW<br>0x0<br>Enumerated values:<br>0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel<br>0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel<br>0x2 → DISABLE: disable output<br>0x3 → ENABLE: enable output<br>13:12<br>OUTOVER<br>RW<br>0x0<br>Enumerated values:<br>0x0 → NORMAL: drive output from peripheral signal selected by funcsel<br>0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel<br>0x2 → LOW: drive output low<br>0x3 → HIGH: drive output high<br>11:5<br>Reserved.<br>-<br>-<br>4:0<br>FUNCSEL: 0-31 → selects pin function according to the gpio table<br>RW<br>0x1f<br>31 == NULL<br>Enumerated values:<br>0x00 → JTAG_TMS<br>0x01 → SPI0_SS_N<br>0x02 → UART0_RX | Bits | Description     | Type | Reset |
|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-----------------|------|-------|
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      |                 |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |      | 0x03 → I2C0_SCL |      |       |
| 0x04 → PWM_B_0                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |                 |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x05 → SIO_1                  |      |       |
|      | 0x06 → PIO0_1                 |      |       |
|      | 0x07 → PIO1_1                 |      |       |
|      | 0x08 → PIO2_1                 |      |       |
|      | 0x09 → CORESIGHT_TRACECLK     |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-610-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO2\_STATUS Register**

**Offset**: 0x010

*Table 653. GPIO2\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-610-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO2\_CTRL Register**

**Offset**: 0x014

*Table 654. GPIO2\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x00 → JTAG_TDI                                                                            |      |       |
|       | 0x01 → SPI0_SCLK                                                                           |      |       |
|       | 0x02 → UART0_CTS                                                                           |      |       |
|       | 0x03 → I2C1_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_1                                                                             |      |       |
|       | 0x05 → SIO_2                                                                               |      |       |
|       | 0x06 → PIO0_2                                                                              |      |       |
|       | 0x07 → PIO1_2                                                                              |      |       |
|       | 0x08 → PIO2_2                                                                              |      |       |
|       | 0x09 → CORESIGHT_TRACEDATA_0                                                               |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x0b → UART0_TX                                                                            |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-611-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO3\_STATUS Register**

**Offset**: 0x018

*Table 655. GPIO3\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-612-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO3\_CTRL Register**

**Offset**: 0x01c

*Table 656. GPIO3\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | Enumerated values:                                                                  |      |       |
|      | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel               |      |       |
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x00 → JTAG_TDO                                                                     |      |       |
|      | 0x01 → SPI0_TX                                                                      |      |       |
|      | 0x02 → UART0_RTS                                                                    |      |       |
|      | 0x03 → I2C1_SCL                                                                     |      |       |
|      | 0x04 → PWM_B_1                                                                      |      |       |
|      | 0x05 → SIO_3                                                                        |      |       |
|      | 0x06 → PIO0_3                                                                       |      |       |
|      | 0x07 → PIO1_3                                                                       |      |       |
|      | 0x08 → PIO2_3                                                                       |      |       |
|      | 0x09 → CORESIGHT_TRACEDATA_1                                                        |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x0b → UART0_RX                                                                     |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-613-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO4\_STATUS Register**

#### **Offset**: 0x020

*Table 657. GPIO4\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-614-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO4\_CTRL Register**

**Offset**: 0x024

*Table 658. GPIO4\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_RX                                                                             |      |       |
|       | 0x02 → UART1_TX                                                                            |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x03 → I2C0_SDA               |      |       |
|      | 0x04 → PWM_A_2                |      |       |
|      | 0x05 → SIO_4                  |      |       |
|      | 0x06 → PIO0_4                 |      |       |
|      | 0x07 → PIO1_4                 |      |       |
|      | 0x08 → PIO2_4                 |      |       |
|      | 0x09 → CORESIGHT_TRACEDATA_2  |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-615-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO5\_STATUS Register**

**Offset**: 0x028

*Table 659. GPIO5\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-615-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO5\_CTRL Register**

**Offset**: 0x02c

*Table 660. GPIO5\_CTRL Register*

| Bits  | Description                              | Type | Reset |
|-------|------------------------------------------|------|-------|
| 31:30 | Reserved.                                | -    | -     |
| 29:28 | IRQOVER                                  | RW   | 0x0   |
|       | Enumerated values:                       |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt |      |       |
|       | 0x1 → INVERT: invert the interrupt       |      |       |
|       | 0x2 → LOW: drive interrupt low           |      |       |
|       | 0x3 → HIGH: drive interrupt high         |      |       |
| 27:18 | Reserved.                                | -    | -     |
| 17:16 | INOVER                                   | RW   | 0x0   |
|       | Enumerated values:                       |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_SS_N                                                                           |      |       |
|       | 0x02 → UART1_RX                                                                            |      |       |
|       | 0x03 → I2C0_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_2                                                                             |      |       |
|       | 0x05 → SIO_5                                                                               |      |       |
|       | 0x06 → PIO0_5                                                                              |      |       |
|       | 0x07 → PIO1_5                                                                              |      |       |
|       | 0x08 → PIO2_5                                                                              |      |       |
|       | 0x09 → CORESIGHT_TRACEDATA_3                                                               |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-616-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO6\_STATUS Register**

**Offset**: 0x030

*Table 661. GPIO6\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-617-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO6\_CTRL Register**

**Offset**: 0x034

*Table 662. GPIO6\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | Enumerated values:                                                                  |      |       |
|      | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel               |      |       |
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI0_SCLK                                                                    |      |       |
|      | 0x02 → UART1_CTS                                                                    |      |       |
|      | 0x03 → I2C1_SDA                                                                     |      |       |
|      | 0x04 → PWM_A_3                                                                      |      |       |
|      | 0x05 → SIO_6                                                                        |      |       |
|      | 0x06 → PIO0_6                                                                       |      |       |
|      | 0x07 → PIO1_6                                                                       |      |       |
|      | 0x08 → PIO2_6                                                                       |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x0b → UART1_TX                                                                     |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-618-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO7\_STATUS Register**

**Offset**: 0x038

*Table 663. GPIO7\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

## <span id="page-618-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO7\_CTRL Register**

**Offset**: 0x03c

*Table 664. GPIO7\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_TX                                                                             |      |       |
|       | 0x02 → UART1_RTS                                                                           |      |       |
|       | 0x03 → I2C1_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_3                                                                             |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x05 → SIO_7                  |      |       |
|      | 0x06 → PIO0_7                 |      |       |
|      | 0x07 → PIO1_7                 |      |       |
|      | 0x08 → PIO2_7                 |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x0b → UART1_RX               |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-620-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO8\_STATUS Register**

**Offset**: 0x040

*Table 665. GPIO8\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-620-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO8\_CTRL Register**

**Offset**: 0x044

*Table 666. GPIO8\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_RX                                                                             |      |       |
|       | 0x02 → UART1_TX                                                                            |      |       |
|       | 0x03 → I2C0_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_4                                                                             |      |       |
|       | 0x05 → SIO_8                                                                               |      |       |
|       | 0x06 → PIO0_8                                                                              |      |       |
|       | 0x07 → PIO1_8                                                                              |      |       |
|       | 0x08 → PIO2_8                                                                              |      |       |
|       | 0x09 → XIP_SS_N_1                                                                          |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-621-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO9\_STATUS Register**

#### **Offset**: 0x048

*Table 667. GPIO9\_STATUS Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:27 | Reserved.   | -    | -     |

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-622-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO9\_CTRL Register**

#### **Offset**: 0x04c

*Table 668. GPIO9\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel               |      |       |
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI1_SS_N                                                                    |      |       |
|      | 0x02 → UART1_RX                                                                     |      |       |
|      | 0x03 → I2C0_SCL                                                                     |      |       |
|      | 0x04 → PWM_B_4                                                                      |      |       |
|      | 0x05 → SIO_9                                                                        |      |       |
|      | 0x06 → PIO0_9                                                                       |      |       |
|      | 0x07 → PIO1_9                                                                       |      |       |
|      | 0x08 → PIO2_9                                                                       |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-623-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO10\_STATUS Register**

**Offset**: 0x050

*Table 669. GPIO10\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

<span id="page-623-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO10\_CTRL Register**

**Offset**: 0x054

*Table 670. GPIO10\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_SCLK                                                                           |      |       |
|       | 0x02 → UART1_CTS                                                                           |      |       |
|       | 0x03 → I2C1_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_5                                                                             |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x05 → SIO_10                 |      |       |
|      | 0x06 → PIO0_10                |      |       |
|      | 0x07 → PIO1_10                |      |       |
|      | 0x08 → PIO2_10                |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x0b → UART1_TX               |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-625-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO11\_STATUS Register**

**Offset**: 0x058

*Table 671. GPIO11\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-625-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO11\_CTRL Register**

**Offset**: 0x05c

*Table 672. GPIO11\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_TX                                                                             |      |       |
|       | 0x02 → UART1_RTS                                                                           |      |       |
|       | 0x03 → I2C1_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_5                                                                             |      |       |
|       | 0x05 → SIO_11                                                                              |      |       |
|       | 0x06 → PIO0_11                                                                             |      |       |
|       | 0x07 → PIO1_11                                                                             |      |       |
|       | 0x08 → PIO2_11                                                                             |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x0b → UART1_RX                                                                            |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-626-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO12\_STATUS Register**

**Offset**: 0x060

*Table 673. GPIO12\_STATUS Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:27 | Reserved.   | -    | -     |

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-627-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO12\_CTRL Register**

**Offset**: 0x064

*Table 674. GPIO12\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel               |      |       |
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x00 → HSTX_0                                                                       |      |       |
|      | 0x01 → SPI1_RX                                                                      |      |       |
|      | 0x02 → UART0_TX                                                                     |      |       |
|      | 0x03 → I2C0_SDA                                                                     |      |       |
|      | 0x04 → PWM_A_6                                                                      |      |       |
|      | 0x05 → SIO_12                                                                       |      |       |
|      | 0x06 → PIO0_12                                                                      |      |       |
|      | 0x07 → PIO1_12                                                                      |      |       |
|      | 0x08 → PIO2_12                                                                      |      |       |
|      | 0x09 → CLOCKS_GPIN_0                                                                |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-628-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO13\_STATUS Register**

**Offset**: 0x068

*Table 675. GPIO13\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

## <span id="page-628-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO13\_CTRL Register**

**Offset**: 0x06c

*Table 676. GPIO13\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x00 → HSTX_1                                                                              |      |       |
|       | 0x01 → SPI1_SS_N                                                                           |      |       |
|       | 0x02 → UART0_RX                                                                            |      |       |
|       | 0x03 → I2C0_SCL                                                                            |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x04 → PWM_B_6                |      |       |
|      | 0x05 → SIO_13                 |      |       |
|      | 0x06 → PIO0_13                |      |       |
|      | 0x07 → PIO1_13                |      |       |
|      | 0x08 → PIO2_13                |      |       |
|      | 0x09 → CLOCKS_GPOUT_0         |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-630-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO14\_STATUS Register**

**Offset**: 0x070

*Table 677. GPIO14\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-630-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO14\_CTRL Register**

**Offset**: 0x074

*Table 678. GPIO14\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x00 → HSTX_2                                                                              |      |       |
|       | 0x01 → SPI1_SCLK                                                                           |      |       |
|       | 0x02 → UART0_CTS                                                                           |      |       |
|       | 0x03 → I2C1_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_7                                                                             |      |       |
|       | 0x05 → SIO_14                                                                              |      |       |
|       | 0x06 → PIO0_14                                                                             |      |       |
|       | 0x07 → PIO1_14                                                                             |      |       |
|       | 0x08 → PIO2_14                                                                             |      |       |
|       | 0x09 → CLOCKS_GPIN_1                                                                       |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x0b → UART0_TX                                                                            |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-631-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO15\_STATUS Register**

**Offset**: 0x078

*Table 679. GPIO15\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-632-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO15\_CTRL Register**

**Offset**: 0x07c

*Table 680. GPIO15\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | Enumerated values:                                                                  |      |       |
|      | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel               |      |       |
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x00 → HSTX_3                                                                       |      |       |
|      | 0x01 → SPI1_TX                                                                      |      |       |
|      | 0x02 → UART0_RTS                                                                    |      |       |
|      | 0x03 → I2C1_SCL                                                                     |      |       |
|      | 0x04 → PWM_B_7                                                                      |      |       |
|      | 0x05 → SIO_15                                                                       |      |       |
|      | 0x06 → PIO0_15                                                                      |      |       |
|      | 0x07 → PIO1_15                                                                      |      |       |
|      | 0x08 → PIO2_15                                                                      |      |       |
|      | 0x09 → CLOCKS_GPOUT_1                                                               |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x0b → UART0_RX                                                                     |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-633-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO16\_STATUS Register**

#### **Offset**: 0x080

*Table 681. GPIO16\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-634-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO16\_CTRL Register**

**Offset**: 0x084

*Table 682. GPIO16\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |  |
|-------|--------------------------------------------------------------------------------------------|------|-------|--|
| 31:30 | Reserved.                                                                                  | -    | -     |  |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |  |
|       | Enumerated values:                                                                         |      |       |  |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |  |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |  |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |  |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |  |
| 27:18 | Reserved.                                                                                  | -    | -     |  |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |  |
|       | Enumerated values:                                                                         |      |       |  |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |  |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |  |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |  |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |  |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |  |
|       | Enumerated values:                                                                         |      |       |  |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |  |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |  |
|       | 0x2 → DISABLE: disable output                                                              |      |       |  |
|       | 0x3 → ENABLE: enable output                                                                |      |       |  |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |  |
|       | Enumerated values:                                                                         |      |       |  |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |  |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |  |
|       | 0x2 → LOW: drive output low                                                                |      |       |  |
|       | 0x3 → HIGH: drive output high                                                              |      |       |  |
| 11:5  | Reserved.                                                                                  | -    | -     |  |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |  |
|       | Enumerated values:                                                                         |      |       |  |
|       | 0x00 → HSTX_4                                                                              |      |       |  |
|       | 0x01 → SPI0_RX                                                                             |      |       |  |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x02 → UART0_TX               |      |       |
|      | 0x03 → I2C0_SDA               |      |       |
|      | 0x04 → PWM_A_0                |      |       |
|      | 0x05 → SIO_16                 |      |       |
|      | 0x06 → PIO0_16                |      |       |
|      | 0x07 → PIO1_16                |      |       |
|      | 0x08 → PIO2_16                |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-635-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO17\_STATUS Register**

**Offset**: 0x088

*Table 683. GPIO17\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-635-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO17\_CTRL Register**

**Offset**: 0x08c

*Table 684. GPIO17\_CTRL Register*

| Bits  | Description                              | Type | Reset |
|-------|------------------------------------------|------|-------|
| 31:30 | Reserved.                                | -    | -     |
| 29:28 | IRQOVER                                  | RW   | 0x0   |
|       | Enumerated values:                       |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt |      |       |
|       | 0x1 → INVERT: invert the interrupt       |      |       |
|       | 0x2 → LOW: drive interrupt low           |      |       |
|       | 0x3 → HIGH: drive interrupt high         |      |       |
| 27:18 | Reserved.                                | -    | -     |
| 17:16 | INOVER                                   | RW   | 0x0   |
|       | Enumerated values:                       |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x00 → HSTX_5                                                                              |      |       |
|       | 0x01 → SPI0_SS_N                                                                           |      |       |
|       | 0x02 → UART0_RX                                                                            |      |       |
|       | 0x03 → I2C0_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_0                                                                             |      |       |
|       | 0x05 → SIO_17                                                                              |      |       |
|       | 0x06 → PIO0_17                                                                             |      |       |
|       | 0x07 → PIO1_17                                                                             |      |       |
|       | 0x08 → PIO2_17                                                                             |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-636-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO18\_STATUS Register**

**Offset**: 0x090

*Table 685. GPIO18\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-637-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO18\_CTRL Register**

**Offset**: 0x094

*Table 686. GPIO18\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | Enumerated values:                                                                  |      |       |
|      | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel               |      |       |
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x00 → HSTX_6                                                                       |      |       |
|      | 0x01 → SPI0_SCLK                                                                    |      |       |
|      | 0x02 → UART0_CTS                                                                    |      |       |
|      | 0x03 → I2C1_SDA                                                                     |      |       |
|      | 0x04 → PWM_A_1                                                                      |      |       |
|      | 0x05 → SIO_18                                                                       |      |       |
|      | 0x06 → PIO0_18                                                                      |      |       |
|      | 0x07 → PIO1_18                                                                      |      |       |
|      | 0x08 → PIO2_18                                                                      |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x0b → UART0_TX                                                                     |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-638-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO19\_STATUS Register**

#### **Offset**: 0x098

*Table 687. GPIO19\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-638-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO19\_CTRL Register**

#### **Offset**: 0x09c

*Table 688. GPIO19\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x00 → HSTX_7                                                                              |      |       |
|       | 0x01 → SPI0_TX                                                                             |      |       |
|       | 0x02 → UART0_RTS                                                                           |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x03 → I2C1_SCL               |      |       |
|      | 0x04 → PWM_B_1                |      |       |
|      | 0x05 → SIO_19                 |      |       |
|      | 0x06 → PIO0_19                |      |       |
|      | 0x07 → PIO1_19                |      |       |
|      | 0x08 → PIO2_19                |      |       |
|      | 0x09 → XIP_SS_N_1             |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x0b → UART0_RX               |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-640-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO20\_STATUS Register**

**Offset**: 0x0a0

*Table 689. GPIO20\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-640-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO20\_CTRL Register**

**Offset**: 0x0a4

*Table 690. GPIO20\_CTRL Register*

| Bits  | Description                              | Type | Reset |
|-------|------------------------------------------|------|-------|
| 31:30 | Reserved.                                | -    | -     |
| 29:28 | IRQOVER                                  | RW   | 0x0   |
|       | Enumerated values:                       |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt |      |       |
|       | 0x1 → INVERT: invert the interrupt       |      |       |
|       | 0x2 → LOW: drive interrupt low           |      |       |
|       | 0x3 → HIGH: drive interrupt high         |      |       |
| 27:18 | Reserved.                                | -    | -     |
| 17:16 | INOVER                                   | RW   | 0x0   |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_RX                                                                             |      |       |
|       | 0x02 → UART1_TX                                                                            |      |       |
|       | 0x03 → I2C0_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_2                                                                             |      |       |
|       | 0x05 → SIO_20                                                                              |      |       |
|       | 0x06 → PIO0_20                                                                             |      |       |
|       | 0x07 → PIO1_20                                                                             |      |       |
|       | 0x08 → PIO2_20                                                                             |      |       |
|       | 0x09 → CLOCKS_GPIN_0                                                                       |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-641-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO21\_STATUS Register**

**Offset**: 0x0a8

*Table 691. GPIO21\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-642-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO21\_CTRL Register**

**Offset**: 0x0ac

*Table 692. GPIO21\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | Enumerated values:                                                                  |      |       |
|      | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel               |      |       |
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI0_SS_N                                                                    |      |       |
|      | 0x02 → UART1_RX                                                                     |      |       |
|      | 0x03 → I2C0_SCL                                                                     |      |       |
|      | 0x04 → PWM_B_2                                                                      |      |       |
|      | 0x05 → SIO_21                                                                       |      |       |
|      | 0x06 → PIO0_21                                                                      |      |       |
|      | 0x07 → PIO1_21                                                                      |      |       |
|      | 0x08 → PIO2_21                                                                      |      |       |
|      | 0x09 → CLOCKS_GPOUT_0                                                               |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-643-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO22\_STATUS Register**

**Offset**: 0x0b0

*Table 693. GPIO22\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

## <span id="page-643-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO22\_CTRL Register**

**Offset**: 0x0b4

*Table 694. GPIO22\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_SCLK                                                                           |      |       |
|       | 0x02 → UART1_CTS                                                                           |      |       |
|       | 0x03 → I2C1_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_3                                                                             |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x05 → SIO_22                 |      |       |
|      | 0x06 → PIO0_22                |      |       |
|      | 0x07 → PIO1_22                |      |       |
|      | 0x08 → PIO2_22                |      |       |
|      | 0x09 → CLOCKS_GPIN_1          |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x0b → UART1_TX               |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-645-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO23\_STATUS Register**

**Offset**: 0x0b8

*Table 695. GPIO23\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-645-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO23\_CTRL Register**

**Offset**: 0x0bc

*Table 696. GPIO23\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       |                                           |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_TX                                                                             |      |       |
|       | 0x02 → UART1_RTS                                                                           |      |       |
|       | 0x03 → I2C1_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_3                                                                             |      |       |
|       | 0x05 → SIO_23                                                                              |      |       |
|       | 0x06 → PIO0_23                                                                             |      |       |
|       | 0x07 → PIO1_23                                                                             |      |       |
|       | 0x08 → PIO2_23                                                                             |      |       |
|       | 0x09 → CLOCKS_GPOUT_1                                                                      |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x0b → UART1_RX                                                                            |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-646-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO24\_STATUS Register**

**Offset**: 0x0c0

*Table 697. GPIO24\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-647-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO24\_CTRL Register**

**Offset**: 0x0c4

*Table 698. GPIO24\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | Enumerated values:                                                                  |      |       |
|      | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel               |      |       |
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI1_RX                                                                      |      |       |
|      | 0x02 → UART1_TX                                                                     |      |       |
|      | 0x03 → I2C0_SDA                                                                     |      |       |
|      | 0x04 → PWM_A_4                                                                      |      |       |
|      | 0x05 → SIO_24                                                                       |      |       |
|      | 0x06 → PIO0_24                                                                      |      |       |
|      | 0x07 → PIO1_24                                                                      |      |       |
|      | 0x08 → PIO2_24                                                                      |      |       |
|      | 0x09 → CLOCKS_GPOUT_2                                                               |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-648-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO25\_STATUS Register**

**Offset**: 0x0c8

*Table 699. GPIO25\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

## <span id="page-648-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO25\_CTRL Register**

**Offset**: 0x0cc

*Table 700. GPIO25\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_SS_N                                                                           |      |       |
|       | 0x02 → UART1_RX                                                                            |      |       |
|       | 0x03 → I2C0_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_4                                                                             |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x05 → SIO_25                 |      |       |
|      | 0x06 → PIO0_25                |      |       |
|      | 0x07 → PIO1_25                |      |       |
|      | 0x08 → PIO2_25                |      |       |
|      | 0x09 → CLOCKS_GPOUT_3         |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-650-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO26\_STATUS Register**

**Offset**: 0x0d0

*Table 701. GPIO26\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-650-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO26\_CTRL Register**

**Offset**: 0x0d4

*Table 702. GPIO26\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_SCLK                                                                           |      |       |
|       | 0x02 → UART1_CTS                                                                           |      |       |
|       | 0x03 → I2C1_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_5                                                                             |      |       |
|       | 0x05 → SIO_26                                                                              |      |       |
|       | 0x06 → PIO0_26                                                                             |      |       |
|       | 0x07 → PIO1_26                                                                             |      |       |
|       | 0x08 → PIO2_26                                                                             |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x0b → UART1_TX                                                                            |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-651-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO27\_STATUS Register**

**Offset**: 0x0d8

*Table 703. GPIO27\_STATUS Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:27 | Reserved.   | -    | -     |

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-652-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO27\_CTRL Register**

#### **Offset**: 0x0dc

*Table 704. GPIO27\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel               |      |       |
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI1_TX                                                                      |      |       |
|      | 0x02 → UART1_RTS                                                                    |      |       |
|      | 0x03 → I2C1_SCL                                                                     |      |       |
|      | 0x04 → PWM_B_5                                                                      |      |       |
|      | 0x05 → SIO_27                                                                       |      |       |
|      | 0x06 → PIO0_27                                                                      |      |       |
|      | 0x07 → PIO1_27                                                                      |      |       |
|      | 0x08 → PIO2_27                                                                      |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x0b → UART1_RX                                                                     |      |       |
|      | 0x1f → NULL                                                                         |      |       |
|      |                                                                                     |      |       |

# <span id="page-653-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO28\_STATUS Register**

**Offset**: 0x0e0

*Table 705. GPIO28\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

#### <span id="page-653-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO28\_CTRL Register**

**Offset**: 0x0e4

*Table 706. GPIO28\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_RX                                                                             |      |       |
|       | 0x02 → UART0_TX                                                                            |      |       |
|       | 0x03 → I2C0_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_6                                                                             |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x05 → SIO_28                 |      |       |
|      | 0x06 → PIO0_28                |      |       |
|      | 0x07 → PIO1_28                |      |       |
|      | 0x08 → PIO2_28                |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-655-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO29\_STATUS Register**

**Offset**: 0x0e8

*Table 707. GPIO29\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-655-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO29\_CTRL Register**

**Offset**: 0x0ec

*Table 708. GPIO29\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |
|       | 0x2 → LOW: drive peri input low           |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_SS_N                                                                           |      |       |
|       | 0x02 → UART0_RX                                                                            |      |       |
|       | 0x03 → I2C0_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_6                                                                             |      |       |
|       | 0x05 → SIO_29                                                                              |      |       |
|       | 0x06 → PIO0_29                                                                             |      |       |
|       | 0x07 → PIO1_29                                                                             |      |       |
|       | 0x08 → PIO2_29                                                                             |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x1f → NULL                                                                                |      |       |

#### <span id="page-656-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO30\_STATUS Register**

**Offset**: 0x0f0

*Table 709. GPIO30\_STATUS Register*

| Bits  | Description                                                   | Type | Reset |
|-------|---------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                     | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied | RO   | 0x0   |
| 25:18 | Reserved.                                                     | -    | -     |

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-657-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO30\_CTRL Register**

**Offset**: 0x0f4

*Table 710. GPIO30\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI1_SCLK                                                                    |      |       |
|      | 0x02 → UART0_CTS                                                                    |      |       |
|      | 0x03 → I2C1_SDA                                                                     |      |       |
|      | 0x04 → PWM_A_7                                                                      |      |       |
|      | 0x05 → SIO_30                                                                       |      |       |
|      | 0x06 → PIO0_30                                                                      |      |       |
|      | 0x07 → PIO1_30                                                                      |      |       |
|      | 0x08 → PIO2_30                                                                      |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x0b → UART0_TX                                                                     |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-658-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO31\_STATUS Register**

**Offset**: 0x0f8

*Table 711. GPIO31\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

<span id="page-658-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO31\_CTRL Register**

**Offset**: 0x0fc

*Table 712. GPIO31\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_TX                                                                             |      |       |
|       | 0x02 → UART0_RTS                                                                           |      |       |
|       | 0x03 → I2C1_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_7                                                                             |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x05 → SIO_31                 |      |       |
|      | 0x06 → PIO0_31                |      |       |
|      | 0x07 → PIO1_31                |      |       |
|      | 0x08 → PIO2_31                |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x0b → UART0_RX               |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-660-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO32\_STATUS Register**

**Offset**: 0x100

*Table 713. GPIO32\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-660-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO32\_CTRL Register**

**Offset**: 0x104

*Table 714. GPIO32\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_RX                                                                             |      |       |
|       | 0x02 → UART0_TX                                                                            |      |       |
|       | 0x03 → I2C0_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_8                                                                             |      |       |
|       | 0x05 → SIO_32                                                                              |      |       |
|       | 0x06 → PIO0_32                                                                             |      |       |
|       | 0x07 → PIO1_32                                                                             |      |       |
|       | 0x08 → PIO2_32                                                                             |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x1f → NULL                                                                                |      |       |

## <span id="page-661-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO33\_STATUS Register**

**Offset**: 0x108

*Table 715. GPIO33\_STATUS Register*

| Bits  | Description                                                   | Type | Reset |
|-------|---------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                     | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied | RO   | 0x0   |

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

## <span id="page-662-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO33\_CTRL Register**

#### **Offset**: 0x10c

*Table 716. GPIO33\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI0_SS_N                                                                    |      |       |
|      | 0x02 → UART0_RX                                                                     |      |       |
|      | 0x03 → I2C0_SCL                                                                     |      |       |
|      | 0x04 → PWM_B_8                                                                      |      |       |
|      | 0x05 → SIO_33                                                                       |      |       |
|      | 0x06 → PIO0_33                                                                      |      |       |
|      | 0x07 → PIO1_33                                                                      |      |       |
|      | 0x08 → PIO2_33                                                                      |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-663-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO34\_STATUS Register**

**Offset**: 0x110

*Table 717. GPIO34\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-663-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO34\_CTRL Register**

**Offset**: 0x114

*Table 718. GPIO34\_CTRL Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:30 | Reserved.   | -    | -     |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_SCLK                                                                           |      |       |
|       | 0x02 → UART0_CTS                                                                           |      |       |
|       | 0x03 → I2C1_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_9                                                                             |      |       |
|       | 0x05 → SIO_34                                                                              |      |       |
|       |                                                                                            |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x06 → PIO0_34                |      |       |
|      | 0x07 → PIO1_34                |      |       |
|      | 0x08 → PIO2_34                |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x0b → UART0_TX               |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-665-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO35\_STATUS Register**

**Offset**: 0x118

*Table 719. GPIO35\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-665-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO35\_CTRL Register**

**Offset**: 0x11c

*Table 720. GPIO35\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |
|       | 0x2 → LOW: drive peri input low           |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_TX                                                                             |      |       |
|       | 0x02 → UART0_RTS                                                                           |      |       |
|       | 0x03 → I2C1_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_9                                                                             |      |       |
|       | 0x05 → SIO_35                                                                              |      |       |
|       | 0x06 → PIO0_35                                                                             |      |       |
|       | 0x07 → PIO1_35                                                                             |      |       |
|       | 0x08 → PIO2_35                                                                             |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x0b → UART0_RX                                                                            |      |       |
|       | 0x1f → NULL                                                                                |      |       |

# <span id="page-666-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO36\_STATUS Register**

**Offset**: 0x120

*Table 721. GPIO36\_STATUS Register*

| Bits  | Description                                                   | Type | Reset |
|-------|---------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                     | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied | RO   | 0x0   |

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-667-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO36\_CTRL Register**

**Offset**: 0x124

*Table 722. GPIO36\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI0_RX                                                                      |      |       |
|      | 0x02 → UART1_TX                                                                     |      |       |
|      | 0x03 → I2C0_SDA                                                                     |      |       |
|      | 0x04 → PWM_A_10                                                                     |      |       |
|      | 0x05 → SIO_36                                                                       |      |       |
|      | 0x06 → PIO0_36                                                                      |      |       |
|      | 0x07 → PIO1_36                                                                      |      |       |
|      | 0x08 → PIO2_36                                                                      |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-668-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO37\_STATUS Register**

**Offset**: 0x128

*Table 723. GPIO37\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-668-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO37\_CTRL Register**

**Offset**: 0x12c

*Table 724. GPIO37\_CTRL Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:30 | Reserved.   | -    | -     |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_SS_N                                                                           |      |       |
|       | 0x02 → UART1_RX                                                                            |      |       |
|       | 0x03 → I2C0_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_10                                                                            |      |       |
|       | 0x05 → SIO_37                                                                              |      |       |
|       |                                                                                            |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x06 → PIO0_37                |      |       |
|      | 0x07 → PIO1_37                |      |       |
|      | 0x08 → PIO2_37                |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-670-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO38\_STATUS Register**

**Offset**: 0x130

*Table 725. GPIO38\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-670-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO38\_CTRL Register**

**Offset**: 0x134

*Table 726. GPIO38\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |
|       | 0x2 → LOW: drive peri input low           |      |       |
|       | 0x3 → HIGH: drive peri input high         |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI0_SCLK                                                                           |      |       |
|       | 0x02 → UART1_CTS                                                                           |      |       |
|       | 0x03 → I2C1_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_11                                                                            |      |       |
|       | 0x05 → SIO_38                                                                              |      |       |
|       | 0x06 → PIO0_38                                                                             |      |       |
|       | 0x07 → PIO1_38                                                                             |      |       |
|       | 0x08 → PIO2_38                                                                             |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x0b → UART1_TX                                                                            |      |       |
|       | 0x1f → NULL                                                                                |      |       |

#### <span id="page-671-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO39\_STATUS Register**

**Offset**: 0x138

*Table 727. GPIO39\_STATUS Register*

| Bits  | Description                                                   | Type | Reset |
|-------|---------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                     | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied | RO   | 0x0   |
| 25:18 | Reserved.                                                     | -    | -     |

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-672-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO39\_CTRL Register**

**Offset**: 0x13c

*Table 728. GPIO39\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI0_TX                                                                      |      |       |
|      | 0x02 → UART1_RTS                                                                    |      |       |
|      | 0x03 → I2C1_SCL                                                                     |      |       |
|      | 0x04 → PWM_B_11                                                                     |      |       |
|      | 0x05 → SIO_39                                                                       |      |       |
|      | 0x06 → PIO0_39                                                                      |      |       |
|      | 0x07 → PIO1_39                                                                      |      |       |
|      | 0x08 → PIO2_39                                                                      |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x0b → UART1_RX                                                                     |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-673-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO40\_STATUS Register**

**Offset**: 0x140

*Table 729. GPIO40\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-673-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO40\_CTRL Register**

**Offset**: 0x144

*Table 730. GPIO40\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_RX                                                                             |      |       |
|       | 0x02 → UART1_TX                                                                            |      |       |
|       | 0x03 → I2C0_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_8                                                                             |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x05 → SIO_40                 |      |       |
|      | 0x06 → PIO0_40                |      |       |
|      | 0x07 → PIO1_40                |      |       |
|      | 0x08 → PIO2_40                |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-675-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO41\_STATUS Register**

**Offset**: 0x148

*Table 731. GPIO41\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-675-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO41\_CTRL Register**

**Offset**: 0x14c

*Table 732. GPIO41\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |
|       | 0x2 → LOW: drive peri input low           |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_SS_N                                                                           |      |       |
|       | 0x02 → UART1_RX                                                                            |      |       |
|       | 0x03 → I2C0_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_8                                                                             |      |       |
|       | 0x05 → SIO_41                                                                              |      |       |
|       | 0x06 → PIO0_41                                                                             |      |       |
|       | 0x07 → PIO1_41                                                                             |      |       |
|       | 0x08 → PIO2_41                                                                             |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x1f → NULL                                                                                |      |       |

#### <span id="page-676-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO42\_STATUS Register**

**Offset**: 0x150

*Table 733. GPIO42\_STATUS Register*

| Bits  | Description                                                   | Type | Reset |
|-------|---------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                     | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied | RO   | 0x0   |
| 25:18 | Reserved.                                                     | -    | -     |

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-677-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO42\_CTRL Register**

**Offset**: 0x154

*Table 734. GPIO42\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       |                                                                                            |      |       |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI1_SCLK                                                                    |      |       |
|      | 0x02 → UART1_CTS                                                                    |      |       |
|      | 0x03 → I2C1_SDA                                                                     |      |       |
|      | 0x04 → PWM_A_9                                                                      |      |       |
|      | 0x05 → SIO_42                                                                       |      |       |
|      | 0x06 → PIO0_42                                                                      |      |       |
|      | 0x07 → PIO1_42                                                                      |      |       |
|      | 0x08 → PIO2_42                                                                      |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x0b → UART1_TX                                                                     |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-678-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO43\_STATUS Register**

**Offset**: 0x158

*Table 735. GPIO43\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

<span id="page-678-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO43\_CTRL Register**

**Offset**: 0x15c

*Table 736. GPIO43\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_TX                                                                             |      |       |
|       | 0x02 → UART1_RTS                                                                           |      |       |
|       | 0x03 → I2C1_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_9                                                                             |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x05 → SIO_43                 |      |       |
|      | 0x06 → PIO0_43                |      |       |
|      | 0x07 → PIO1_43                |      |       |
|      | 0x08 → PIO2_43                |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x0b → UART1_RX               |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-680-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO44\_STATUS Register**

**Offset**: 0x160

*Table 737. GPIO44\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-680-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO44\_CTRL Register**

**Offset**: 0x164

*Table 738. GPIO44\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_RX                                                                             |      |       |
|       | 0x02 → UART0_TX                                                                            |      |       |
|       | 0x03 → I2C0_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_10                                                                            |      |       |
|       | 0x05 → SIO_44                                                                              |      |       |
|       | 0x06 → PIO0_44                                                                             |      |       |
|       | 0x07 → PIO1_44                                                                             |      |       |
|       | 0x08 → PIO2_44                                                                             |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x1f → NULL                                                                                |      |       |

## <span id="page-681-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO45\_STATUS Register**

**Offset**: 0x168

*Table 739. GPIO45\_STATUS Register*

| Bits  | Description                                                   | Type | Reset |
|-------|---------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                     | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied | RO   | 0x0   |

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

## <span id="page-682-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO45\_CTRL Register**

#### **Offset**: 0x16c

*Table 740. GPIO45\_CTRL Register*

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 31:30 | Reserved.                                                                                  | -    | -     |
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |

| Bits | Description                                                                         | Type | Reset |
|------|-------------------------------------------------------------------------------------|------|-------|
|      | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel |      |       |
|      | 0x2 → LOW: drive output low                                                         |      |       |
|      | 0x3 → HIGH: drive output high                                                       |      |       |
| 11:5 | Reserved.                                                                           | -    | -     |
| 4:0  | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL      | RW   | 0x1f  |
|      | Enumerated values:                                                                  |      |       |
|      | 0x01 → SPI1_SS_N                                                                    |      |       |
|      | 0x02 → UART0_RX                                                                     |      |       |
|      | 0x03 → I2C0_SCL                                                                     |      |       |
|      | 0x04 → PWM_B_10                                                                     |      |       |
|      | 0x05 → SIO_45                                                                       |      |       |
|      | 0x06 → PIO0_45                                                                      |      |       |
|      | 0x07 → PIO1_45                                                                      |      |       |
|      | 0x08 → PIO2_45                                                                      |      |       |
|      | 0x0a → USB_MUXING_OVERCURR_DETECT                                                   |      |       |
|      | 0x1f → NULL                                                                         |      |       |

# <span id="page-683-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO46\_STATUS Register**

**Offset**: 0x170

*Table 741. GPIO46\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-683-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO46\_CTRL Register**

**Offset**: 0x174

*Table 742. GPIO46\_CTRL Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:30 | Reserved.   | -    | -     |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
| 29:28 | IRQOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt                                                   |      |       |
|       | 0x1 → INVERT: invert the interrupt                                                         |      |       |
|       | 0x2 → LOW: drive interrupt low                                                             |      |       |
|       | 0x3 → HIGH: drive interrupt high                                                           |      |       |
| 27:18 | Reserved.                                                                                  | -    | -     |
| 17:16 | INOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: don't invert the peri input                                                  |      |       |
|       | 0x1 → INVERT: invert the peri input                                                        |      |       |
|       | 0x2 → LOW: drive peri input low                                                            |      |       |
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_SCLK                                                                           |      |       |
|       | 0x02 → UART0_CTS                                                                           |      |       |
|       | 0x03 → I2C1_SDA                                                                            |      |       |
|       | 0x04 → PWM_A_11                                                                            |      |       |
|       | 0x05 → SIO_46                                                                              |      |       |
|       |                                                                                            |      |       |

| Bits | Description                   | Type | Reset |
|------|-------------------------------|------|-------|
|      | 0x06 → PIO0_46                |      |       |
|      | 0x07 → PIO1_46                |      |       |
|      | 0x08 → PIO2_46                |      |       |
|      | 0x0a → USB_MUXING_VBUS_DETECT |      |       |
|      | 0x0b → UART0_TX               |      |       |
|      | 0x1f → NULL                   |      |       |

# <span id="page-685-0"></span>**[IO\\_BANK0:](#page-601-2) GPIO47\_STATUS Register**

**Offset**: 0x178

*Table 743. GPIO47\_STATUS Register*

| Bits  | Description                                                                 | Type | Reset |
|-------|-----------------------------------------------------------------------------|------|-------|
| 31:27 | Reserved.                                                                   | -    | -     |
| 26    | IRQTOPROC: interrupt to processors, after override is applied               | RO   | 0x0   |
| 25:18 | Reserved.                                                                   | -    | -     |
| 17    | INFROMPAD: input signal from pad, before filtering and override are applied | RO   | 0x0   |
| 16:14 | Reserved.                                                                   | -    | -     |
| 13    | OETOPAD: output enable to pad after register override is applied            | RO   | 0x0   |
| 12:10 | Reserved.                                                                   | -    | -     |
| 9     | OUTTOPAD: output signal to pad after register override is applied           | RO   | 0x0   |
| 8:0   | Reserved.                                                                   | -    | -     |

# <span id="page-685-1"></span>**[IO\\_BANK0:](#page-601-2) GPIO47\_CTRL Register**

**Offset**: 0x17c

*Table 744. GPIO47\_CTRL Register*

| Bits  | Description                               | Type | Reset |
|-------|-------------------------------------------|------|-------|
| 31:30 | Reserved.                                 | -    | -     |
| 29:28 | IRQOVER                                   | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the interrupt  |      |       |
|       | 0x1 → INVERT: invert the interrupt        |      |       |
|       | 0x2 → LOW: drive interrupt low            |      |       |
|       | 0x3 → HIGH: drive interrupt high          |      |       |
| 27:18 | Reserved.                                 | -    | -     |
| 17:16 | INOVER                                    | RW   | 0x0   |
|       | Enumerated values:                        |      |       |
|       | 0x0 → NORMAL: don't invert the peri input |      |       |
|       | 0x1 → INVERT: invert the peri input       |      |       |
|       | 0x2 → LOW: drive peri input low           |      |       |

| Bits  | Description                                                                                | Type | Reset |
|-------|--------------------------------------------------------------------------------------------|------|-------|
|       | 0x3 → HIGH: drive peri input high                                                          |      |       |
| 15:14 | OEOVER                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output enable from peripheral signal selected by<br>funcsel            |      |       |
|       | 0x1 → INVERT: drive output enable from inverse of peripheral signal selected<br>by funcsel |      |       |
|       | 0x2 → DISABLE: disable output                                                              |      |       |
|       | 0x3 → ENABLE: enable output                                                                |      |       |
| 13:12 | OUTOVER                                                                                    | RW   | 0x0   |
|       | Enumerated values:                                                                         |      |       |
|       | 0x0 → NORMAL: drive output from peripheral signal selected by funcsel                      |      |       |
|       | 0x1 → INVERT: drive output from inverse of peripheral signal selected by<br>funcsel        |      |       |
|       | 0x2 → LOW: drive output low                                                                |      |       |
|       | 0x3 → HIGH: drive output high                                                              |      |       |
| 11:5  | Reserved.                                                                                  | -    | -     |
| 4:0   | FUNCSEL: 0-31 → selects pin function according to the gpio table<br>31 == NULL             | RW   | 0x1f  |
|       | Enumerated values:                                                                         |      |       |
|       | 0x01 → SPI1_TX                                                                             |      |       |
|       | 0x02 → UART0_RTS                                                                           |      |       |
|       | 0x03 → I2C1_SCL                                                                            |      |       |
|       | 0x04 → PWM_B_11                                                                            |      |       |
|       | 0x05 → SIO_47                                                                              |      |       |
|       | 0x06 → PIO0_47                                                                             |      |       |
|       |                                                                                            |      |       |
|       | 0x07 → PIO1_47                                                                             |      |       |
|       | 0x08 → PIO2_47                                                                             |      |       |
|       | 0x09 → XIP_SS_N_1                                                                          |      |       |
|       | 0x0a → USB_MUXING_VBUS_EN                                                                  |      |       |
|       | 0x0b → UART0_RX                                                                            |      |       |

# <span id="page-686-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_PROC0\_SECURE0 Register**

**Offset**: 0x200

*Table 745. IRQSUMMARY\_PROC0 \_SECURE0 Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31   | GPIO31      | RO   | 0x0   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 30   | GPIO30      | RO   | 0x0   |
| 29   | GPIO29      | RO   | 0x0   |
| 28   | GPIO28      | RO   | 0x0   |
| 27   | GPIO27      | RO   | 0x0   |
| 26   | GPIO26      | RO   | 0x0   |
| 25   | GPIO25      | RO   | 0x0   |
| 24   | GPIO24      | RO   | 0x0   |
| 23   | GPIO23      | RO   | 0x0   |
| 22   | GPIO22      | RO   | 0x0   |
| 21   | GPIO21      | RO   | 0x0   |
| 20   | GPIO20      | RO   | 0x0   |
| 19   | GPIO19      | RO   | 0x0   |
| 18   | GPIO18      | RO   | 0x0   |
| 17   | GPIO17      | RO   | 0x0   |
| 16   | GPIO16      | RO   | 0x0   |
| 15   | GPIO15      | RO   | 0x0   |
| 14   | GPIO14      | RO   | 0x0   |
| 13   | GPIO13      | RO   | 0x0   |
| 12   | GPIO12      | RO   | 0x0   |
| 11   | GPIO11      | RO   | 0x0   |
| 10   | GPIO10      | RO   | 0x0   |
| 9    | GPIO9       | RO   | 0x0   |
| 8    | GPIO8       | RO   | 0x0   |
| 7    | GPIO7       | RO   | 0x0   |
| 6    | GPIO6       | RO   | 0x0   |
| 5    | GPIO5       | RO   | 0x0   |
| 4    | GPIO4       | RO   | 0x0   |
| 3    | GPIO3       | RO   | 0x0   |
| 2    | GPIO2       | RO   | 0x0   |
| 1    | GPIO1       | RO   | 0x0   |
| 0    | GPIO0       | RO   | 0x0   |

#### <span id="page-687-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_PROC0\_SECURE1 Register**

**Offset**: 0x204

*Table 746. IRQSUMMARY\_PROC0 \_SECURE1 Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:16 | Reserved.   | -    | -     |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 15   | GPIO47      | RO   | 0x0   |
| 14   | GPIO46      | RO   | 0x0   |
| 13   | GPIO45      | RO   | 0x0   |
| 12   | GPIO44      | RO   | 0x0   |
| 11   | GPIO43      | RO   | 0x0   |
| 10   | GPIO42      | RO   | 0x0   |
| 9    | GPIO41      | RO   | 0x0   |
| 8    | GPIO40      | RO   | 0x0   |
| 7    | GPIO39      | RO   | 0x0   |
| 6    | GPIO38      | RO   | 0x0   |
| 5    | GPIO37      | RO   | 0x0   |
| 4    | GPIO36      | RO   | 0x0   |
| 3    | GPIO35      | RO   | 0x0   |
| 2    | GPIO34      | RO   | 0x0   |
| 1    | GPIO33      | RO   | 0x0   |
| 0    | GPIO32      | RO   | 0x0   |

# <span id="page-688-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_PROC0\_NONSECURE0 Register**

**Offset**: 0x208

*Table 747. IRQSUMMARY\_PROC0 \_NONSECURE0 Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31   | GPIO31      | RO   | 0x0   |
| 30   | GPIO30      | RO   | 0x0   |
| 29   | GPIO29      | RO   | 0x0   |
| 28   | GPIO28      | RO   | 0x0   |
| 27   | GPIO27      | RO   | 0x0   |
| 26   | GPIO26      | RO   | 0x0   |
| 25   | GPIO25      | RO   | 0x0   |
| 24   | GPIO24      | RO   | 0x0   |
| 23   | GPIO23      | RO   | 0x0   |
| 22   | GPIO22      | RO   | 0x0   |
| 21   | GPIO21      | RO   | 0x0   |
| 20   | GPIO20      | RO   | 0x0   |
| 19   | GPIO19      | RO   | 0x0   |
| 18   | GPIO18      | RO   | 0x0   |
| 17   | GPIO17      | RO   | 0x0   |
| 16   | GPIO16      | RO   | 0x0   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 15   | GPIO15      | RO   | 0x0   |
| 14   | GPIO14      | RO   | 0x0   |
| 13   | GPIO13      | RO   | 0x0   |
| 12   | GPIO12      | RO   | 0x0   |
| 11   | GPIO11      | RO   | 0x0   |
| 10   | GPIO10      | RO   | 0x0   |
| 9    | GPIO9       | RO   | 0x0   |
| 8    | GPIO8       | RO   | 0x0   |
| 7    | GPIO7       | RO   | 0x0   |
| 6    | GPIO6       | RO   | 0x0   |
| 5    | GPIO5       | RO   | 0x0   |
| 4    | GPIO4       | RO   | 0x0   |
| 3    | GPIO3       | RO   | 0x0   |
| 2    | GPIO2       | RO   | 0x0   |
| 1    | GPIO1       | RO   | 0x0   |
| 0    | GPIO0       | RO   | 0x0   |

# <span id="page-689-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_PROC0\_NONSECURE1 Register**

**Offset**: 0x20c

*Table 748. IRQSUMMARY\_PROC0 \_NONSECURE1 Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
|       |             |      |       |
| 31:16 | Reserved.   | -    | -     |
| 15    | GPIO47      | RO   | 0x0   |
| 14    | GPIO46      | RO   | 0x0   |
| 13    | GPIO45      | RO   | 0x0   |
| 12    | GPIO44      | RO   | 0x0   |
| 11    | GPIO43      | RO   | 0x0   |
| 10    | GPIO42      | RO   | 0x0   |
| 9     | GPIO41      | RO   | 0x0   |
| 8     | GPIO40      | RO   | 0x0   |
| 7     | GPIO39      | RO   | 0x0   |
| 6     | GPIO38      | RO   | 0x0   |
| 5     | GPIO37      | RO   | 0x0   |
| 4     | GPIO36      | RO   | 0x0   |
| 3     | GPIO35      | RO   | 0x0   |
| 2     | GPIO34      | RO   | 0x0   |
| 1     | GPIO33      | RO   | 0x0   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 0    | GPIO32      | RO   | 0x0   |

# <span id="page-690-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_PROC1\_SECURE0 Register**

**Offset**: 0x210

*Table 749. IRQSUMMARY\_PROC1 \_SECURE0 Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31   | GPIO31      | RO   | 0x0   |
| 30   | GPIO30      | RO   | 0x0   |
| 29   | GPIO29      | RO   | 0x0   |
| 28   | GPIO28      | RO   | 0x0   |
| 27   | GPIO27      | RO   | 0x0   |
| 26   | GPIO26      | RO   | 0x0   |
| 25   | GPIO25      | RO   | 0x0   |
| 24   | GPIO24      | RO   | 0x0   |
| 23   | GPIO23      | RO   | 0x0   |
| 22   | GPIO22      | RO   | 0x0   |
| 21   | GPIO21      | RO   | 0x0   |
| 20   | GPIO20      | RO   | 0x0   |
| 19   | GPIO19      | RO   | 0x0   |
| 18   | GPIO18      | RO   | 0x0   |
| 17   | GPIO17      | RO   | 0x0   |
| 16   | GPIO16      | RO   | 0x0   |
| 15   | GPIO15      | RO   | 0x0   |
| 14   | GPIO14      | RO   | 0x0   |
| 13   | GPIO13      | RO   | 0x0   |
| 12   | GPIO12      | RO   | 0x0   |
| 11   | GPIO11      | RO   | 0x0   |
| 10   | GPIO10      | RO   | 0x0   |
| 9    | GPIO9       | RO   | 0x0   |
| 8    | GPIO8       | RO   | 0x0   |
| 7    | GPIO7       | RO   | 0x0   |
| 6    | GPIO6       | RO   | 0x0   |
| 5    | GPIO5       | RO   | 0x0   |
| 4    | GPIO4       | RO   | 0x0   |
| 3    | GPIO3       | RO   | 0x0   |
| 2    | GPIO2       | RO   | 0x0   |
| 1    | GPIO1       | RO   | 0x0   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 0    | GPIO0       | RO   | 0x0   |

# <span id="page-691-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_PROC1\_SECURE1 Register**

**Offset**: 0x214

*Table 750. IRQSUMMARY\_PROC1 \_SECURE1 Register*

| Description | Type | Reset |
|-------------|------|-------|
| Reserved.   | -    | -     |
| GPIO47      | RO   | 0x0   |
| GPIO46      | RO   | 0x0   |
| GPIO45      | RO   | 0x0   |
| GPIO44      | RO   | 0x0   |
| GPIO43      | RO   | 0x0   |
| GPIO42      | RO   | 0x0   |
| GPIO41      | RO   | 0x0   |
| GPIO40      | RO   | 0x0   |
| GPIO39      | RO   | 0x0   |
| GPIO38      | RO   | 0x0   |
| GPIO37      | RO   | 0x0   |
| GPIO36      | RO   | 0x0   |
| GPIO35      | RO   | 0x0   |
| GPIO34      | RO   | 0x0   |
| GPIO33      | RO   | 0x0   |
| GPIO32      | RO   | 0x0   |
|             |      |       |

# <span id="page-691-1"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_PROC1\_NONSECURE0 Register**

**Offset**: 0x218

*Table 751. IRQSUMMARY\_PROC1 \_NONSECURE0 Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31   | GPIO31      | RO   | 0x0   |
| 30   | GPIO30      | RO   | 0x0   |
| 29   | GPIO29      | RO   | 0x0   |
| 28   | GPIO28      | RO   | 0x0   |
| 27   | GPIO27      | RO   | 0x0   |
| 26   | GPIO26      | RO   | 0x0   |
| 25   | GPIO25      | RO   | 0x0   |
| 24   | GPIO24      | RO   | 0x0   |
| 23   | GPIO23      | RO   | 0x0   |
| 22   | GPIO22      | RO   | 0x0   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 21   | GPIO21      | RO   | 0x0   |
| 20   | GPIO20      | RO   | 0x0   |
| 19   | GPIO19      | RO   | 0x0   |
| 18   | GPIO18      | RO   | 0x0   |
| 17   | GPIO17      | RO   | 0x0   |
| 16   | GPIO16      | RO   | 0x0   |
| 15   | GPIO15      | RO   | 0x0   |
| 14   | GPIO14      | RO   | 0x0   |
| 13   | GPIO13      | RO   | 0x0   |
| 12   | GPIO12      | RO   | 0x0   |
| 11   | GPIO11      | RO   | 0x0   |
| 10   | GPIO10      | RO   | 0x0   |
| 9    | GPIO9       | RO   | 0x0   |
| 8    | GPIO8       | RO   | 0x0   |
| 7    | GPIO7       | RO   | 0x0   |
| 6    | GPIO6       | RO   | 0x0   |
| 5    | GPIO5       | RO   | 0x0   |
| 4    | GPIO4       | RO   | 0x0   |
| 3    | GPIO3       | RO   | 0x0   |
| 2    | GPIO2       | RO   | 0x0   |
| 1    | GPIO1       | RO   | 0x0   |
| 0    | GPIO0       | RO   | 0x0   |

# <span id="page-692-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_PROC1\_NONSECURE1 Register**

**Offset**: 0x21c

*Table 752. IRQSUMMARY\_PROC1 \_NONSECURE1 Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:16 | Reserved.   | -    | -     |
| 15    | GPIO47      | RO   | 0x0   |
| 14    | GPIO46      | RO   | 0x0   |
| 13    | GPIO45      | RO   | 0x0   |
| 12    | GPIO44      | RO   | 0x0   |
| 11    | GPIO43      | RO   | 0x0   |
| 10    | GPIO42      | RO   | 0x0   |
| 9     | GPIO41      | RO   | 0x0   |
| 8     | GPIO40      | RO   | 0x0   |
| 7     | GPIO39      | RO   | 0x0   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 6    | GPIO38      | RO   | 0x0   |
| 5    | GPIO37      | RO   | 0x0   |
| 4    | GPIO36      | RO   | 0x0   |
| 3    | GPIO35      | RO   | 0x0   |
| 2    | GPIO34      | RO   | 0x0   |
| 1    | GPIO33      | RO   | 0x0   |
| 0    | GPIO32      | RO   | 0x0   |

# <span id="page-693-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_COMA\_WAKE\_SECURE0 Register**

**Offset**: 0x220

*Table 753. IRQSUMMARY\_COMA\_ WAKE\_SECURE0 Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31   | GPIO31      | RO   | 0x0   |
| 30   | GPIO30      | RO   | 0x0   |
| 29   | GPIO29      | RO   | 0x0   |
| 28   | GPIO28      | RO   | 0x0   |
| 27   | GPIO27      | RO   | 0x0   |
| 26   | GPIO26      | RO   | 0x0   |
| 25   | GPIO25      | RO   | 0x0   |
| 24   | GPIO24      | RO   | 0x0   |
| 23   | GPIO23      | RO   | 0x0   |
| 22   | GPIO22      | RO   | 0x0   |
| 21   | GPIO21      | RO   | 0x0   |
| 20   | GPIO20      | RO   | 0x0   |
| 19   | GPIO19      | RO   | 0x0   |
| 18   | GPIO18      | RO   | 0x0   |
| 17   | GPIO17      | RO   | 0x0   |
| 16   | GPIO16      | RO   | 0x0   |
| 15   | GPIO15      | RO   | 0x0   |
| 14   | GPIO14      | RO   | 0x0   |
| 13   | GPIO13      | RO   | 0x0   |
| 12   | GPIO12      | RO   | 0x0   |
| 11   | GPIO11      | RO   | 0x0   |
| 10   | GPIO10      | RO   | 0x0   |
| 9    | GPIO9       | RO   | 0x0   |
| 8    | GPIO8       | RO   | 0x0   |
| 7    | GPIO7       | RO   | 0x0   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 6    | GPIO6       | RO   | 0x0   |
| 5    | GPIO5       | RO   | 0x0   |
| 4    | GPIO4       | RO   | 0x0   |
| 3    | GPIO3       | RO   | 0x0   |
| 2    | GPIO2       | RO   | 0x0   |
| 1    | GPIO1       | RO   | 0x0   |
| 0    | GPIO0       | RO   | 0x0   |

# <span id="page-694-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_COMA\_WAKE\_SECURE1 Register**

**Offset**: 0x224

*Table 754. IRQSUMMARY\_COMA\_ WAKE\_SECURE1 Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:16 | Reserved.   | -    | -     |
| 15    | GPIO47      | RO   | 0x0   |
| 14    | GPIO46      | RO   | 0x0   |
| 13    | GPIO45      | RO   | 0x0   |
| 12    | GPIO44      | RO   | 0x0   |
| 11    | GPIO43      | RO   | 0x0   |
| 10    | GPIO42      | RO   | 0x0   |
| 9     | GPIO41      | RO   | 0x0   |
| 8     | GPIO40      | RO   | 0x0   |
| 7     | GPIO39      | RO   | 0x0   |
| 6     | GPIO38      | RO   | 0x0   |
| 5     | GPIO37      | RO   | 0x0   |
| 4     | GPIO36      | RO   | 0x0   |
| 3     | GPIO35      | RO   | 0x0   |
| 2     | GPIO34      | RO   | 0x0   |
| 1     | GPIO33      | RO   | 0x0   |
| 0     | GPIO32      | RO   | 0x0   |

## <span id="page-694-1"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_COMA\_WAKE\_NONSECURE0 Register**

**Offset**: 0x228

*Table 755. IRQSUMMARY\_COMA\_ WAKE\_NONSECURE0 Register*

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 31   | GPIO31      | RO   | 0x0   |
| 30   | GPIO30      | RO   | 0x0   |
| 29   | GPIO29      | RO   | 0x0   |
| 28   | GPIO28      | RO   | 0x0   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 27   | GPIO27      | RO   | 0x0   |
| 26   | GPIO26      | RO   | 0x0   |
| 25   | GPIO25      | RO   | 0x0   |
| 24   | GPIO24      | RO   | 0x0   |
| 23   | GPIO23      | RO   | 0x0   |
| 22   | GPIO22      | RO   | 0x0   |
| 21   | GPIO21      | RO   | 0x0   |
| 20   | GPIO20      | RO   | 0x0   |
| 19   | GPIO19      | RO   | 0x0   |
| 18   | GPIO18      | RO   | 0x0   |
| 17   | GPIO17      | RO   | 0x0   |
| 16   | GPIO16      | RO   | 0x0   |
| 15   | GPIO15      | RO   | 0x0   |
| 14   | GPIO14      | RO   | 0x0   |
| 13   | GPIO13      | RO   | 0x0   |
| 12   | GPIO12      | RO   | 0x0   |
| 11   | GPIO11      | RO   | 0x0   |
| 10   | GPIO10      | RO   | 0x0   |
| 9    | GPIO9       | RO   | 0x0   |
| 8    | GPIO8       | RO   | 0x0   |
| 7    | GPIO7       | RO   | 0x0   |
| 6    | GPIO6       | RO   | 0x0   |
| 5    | GPIO5       | RO   | 0x0   |
| 4    | GPIO4       | RO   | 0x0   |
| 3    | GPIO3       | RO   | 0x0   |
| 2    | GPIO2       | RO   | 0x0   |
| 1    | GPIO1       | RO   | 0x0   |
| 0    | GPIO0       | RO   | 0x0   |

# <span id="page-695-0"></span>**[IO\\_BANK0:](#page-601-2) IRQSUMMARY\_COMA\_WAKE\_NONSECURE1 Register**

**Offset**: 0x22c

*Table 756. IRQSUMMARY\_COMA\_ WAKE\_NONSECURE1 Register*

| Bits  | Description | Type | Reset |
|-------|-------------|------|-------|
| 31:16 | Reserved.   | -    | -     |
| 15    | GPIO47      | RO   | 0x0   |
| 14    | GPIO46      | RO   | 0x0   |
| 13    | GPIO45      | RO   | 0x0   |

| Bits | Description | Type | Reset |
|------|-------------|------|-------|
| 12   | GPIO44      | RO   | 0x0   |
| 11   | GPIO43      | RO   | 0x0   |
| 10   | GPIO42      | RO   | 0x0   |
| 9    | GPIO41      | RO   | 0x0   |
| 8    | GPIO40      | RO   | 0x0   |
| 7    | GPIO39      | RO   | 0x0   |
| 6    | GPIO38      | RO   | 0x0   |
| 5    | GPIO37      | RO   | 0x0   |
| 4    | GPIO36      | RO   | 0x0   |
| 3    | GPIO35      | RO   | 0x0   |
| 2    | GPIO34      | RO   | 0x0   |
| 1    | GPIO33      | RO   | 0x0   |
| 0    | GPIO32      | RO   | 0x0   |

# <span id="page-696-0"></span>**[IO\\_BANK0:](#page-601-2) INTR0 Register**

**Offset**: 0x230 **Description**

Raw Interrupts

*Table 757. INTR0 Register*

| Bits | Description      | Type | Reset |  |
|------|------------------|------|-------|--|
| 31   | GPIO7_EDGE_HIGH  | WC   | 0x0   |  |
| 30   | GPIO7_EDGE_LOW   | WC   | 0x0   |  |
| 29   | GPIO7_LEVEL_HIGH | RO   | 0x0   |  |
| 28   | GPIO7_LEVEL_LOW  | RO   | 0x0   |  |
| 27   | GPIO6_EDGE_HIGH  | WC   | 0x0   |  |
| 26   | GPIO6_EDGE_LOW   | WC   | 0x0   |  |
| 25   | GPIO6_LEVEL_HIGH | RO   | 0x0   |  |
| 24   | GPIO6_LEVEL_LOW  | RO   | 0x0   |  |
| 23   | GPIO5_EDGE_HIGH  | WC   | 0x0   |  |
| 22   | GPIO5_EDGE_LOW   | WC   | 0x0   |  |
| 21   | GPIO5_LEVEL_HIGH | RO   | 0x0   |  |
| 20   | GPIO5_LEVEL_LOW  | RO   | 0x0   |  |
| 19   | GPIO4_EDGE_HIGH  | WC   | 0x0   |  |
| 18   | GPIO4_EDGE_LOW   | WC   | 0x0   |  |
| 17   | GPIO4_LEVEL_HIGH | RO   | 0x0   |  |
| 16   | GPIO4_LEVEL_LOW  | RO   | 0x0   |  |
| 15   | GPIO3_EDGE_HIGH  | WC   | 0x0   |  |

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 14   | GPIO3_EDGE_LOW   | WC   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | WC   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | WC   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | WC   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | WC   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | WC   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | WC   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RO   | 0x0   |

# <span id="page-697-0"></span>**[IO\\_BANK0:](#page-601-2) INTR1 Register**

**Offset**: 0x234 **Description**

Raw Interrupts

*Table 758. INTR1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | WC   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | WC   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO15_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | WC   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | WC   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | WC   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | WC   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | WC   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | WC   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RO   | 0x0   |

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 16   | GPIO12_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | WC   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | WC   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | WC   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | WC   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | WC   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | WC   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RO   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RO   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | WC   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | WC   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RO   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RO   | 0x0   |

# <span id="page-698-0"></span>**[IO\\_BANK0:](#page-601-2) INTR2 Register**

**Offset**: 0x238

#### **Description**

Raw Interrupts

*Table 759. INTR2 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH  | WC   | 0x0   |
| 30   | GPIO23_EDGE_LOW   | WC   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | WC   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | WC   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | WC   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | WC   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | WC   | 0x0   |

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 18   | GPIO20_EDGE_LOW   | WC   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | WC   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | WC   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | WC   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | WC   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | WC   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | WC   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | WC   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | WC   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RO   | 0x0   |

# <span id="page-699-0"></span>**[IO\\_BANK0:](#page-601-2) INTR3 Register**

**Offset**: 0x23c

#### **Description**

Raw Interrupts

*Table 760. INTR3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | WC   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | WC   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | WC   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | WC   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | WC   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | WC   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RO   | 0x0   |

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 20   | GPIO29_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | WC   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | WC   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | WC   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | WC   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | WC   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | WC   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | WC   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | WC   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | WC   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | WC   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RO   | 0x0   |

# <span id="page-700-0"></span>**[IO\\_BANK0:](#page-601-2) INTR4 Register**

**Offset**: 0x240 **Description**

Raw Interrupts

*Table 761. INTR4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | WC   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | WC   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | WC   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | WC   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | WC   | 0x0   |

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 22   | GPIO37_EDGE_LOW   | WC   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | WC   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | WC   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | WC   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | WC   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | WC   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | WC   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | WC   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | WC   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | WC   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | WC   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RO   | 0x0   |

# <span id="page-701-0"></span>**[IO\\_BANK0:](#page-601-2) INTR5 Register**

**Offset**: 0x244

#### **Description**

Raw Interrupts

*Table 762. INTR5 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO47_EDGE_HIGH  | WC   | 0x0   |
| 30   | GPIO47_EDGE_LOW   | WC   | 0x0   |
| 29   | GPIO47_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO47_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO46_EDGE_HIGH  | WC   | 0x0   |
| 26   | GPIO46_EDGE_LOW   | WC   | 0x0   |
| 25   | GPIO46_LEVEL_HIGH | RO   | 0x0   |

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 24   | GPIO46_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO45_EDGE_HIGH  | WC   | 0x0   |
| 22   | GPIO45_EDGE_LOW   | WC   | 0x0   |
| 21   | GPIO45_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO45_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO44_EDGE_HIGH  | WC   | 0x0   |
| 18   | GPIO44_EDGE_LOW   | WC   | 0x0   |
| 17   | GPIO44_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO44_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO43_EDGE_HIGH  | WC   | 0x0   |
| 14   | GPIO43_EDGE_LOW   | WC   | 0x0   |
| 13   | GPIO43_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO43_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO42_EDGE_HIGH  | WC   | 0x0   |
| 10   | GPIO42_EDGE_LOW   | WC   | 0x0   |
| 9    | GPIO42_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO42_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO41_EDGE_HIGH  | WC   | 0x0   |
| 6    | GPIO41_EDGE_LOW   | WC   | 0x0   |
| 5    | GPIO41_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO41_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO40_EDGE_HIGH  | WC   | 0x0   |
| 2    | GPIO40_EDGE_LOW   | WC   | 0x0   |
| 1    | GPIO40_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO40_LEVEL_LOW  | RO   | 0x0   |

# <span id="page-702-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTE0 Register**

**Offset**: 0x248

**Description**

Interrupt Enable for proc0

*Table 763. PROC0\_INTE0 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO7_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO7_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO7_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO7_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO6_EDGE_HIGH  | RW   | 0x0   |

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 26   | GPIO6_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO6_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO6_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO5_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO5_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO5_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO5_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO4_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO4_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO4_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO4_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO3_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO3_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RW   | 0x0   |
|      |                  |      |       |

# <span id="page-703-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTE1 Register**

**Offset**: 0x24c **Description**

Interrupt Enable for proc0

*Table 764. PROC0\_INTE1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RW   | 0x0   |

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 28   | GPIO15_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO12_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | RW   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | RW   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RW   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RW   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | RW   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | RW   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RW   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RW   | 0x0   |

#### <span id="page-704-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTE2 Register**

**Offset**: 0x250

#### **Description**

Interrupt Enable for proc0

*Table 765. PROC0\_INTE2 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH | RW   | 0x0   |

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 30   | GPIO23_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO20_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RW   | 0x0   |

#### <span id="page-705-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTE3 Register**

**Offset**: 0x254 **Description**

Interrupt Enable for proc0

*Table 766. PROC0\_INTE3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO29_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RW   | 0x0   |

#### <span id="page-706-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTE4 Register**

**Offset**: 0x258

Interrupt Enable for proc0

*Table 767. PROC0\_INTE4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO37_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RW   | 0x0   |

<span id="page-707-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTE5 Register**

**Offset**: 0x25c

Interrupt Enable for proc0

*Table 768. PROC0\_INTE5 Register*

| 31<br>GPIO47_EDGE_HIGH<br>RW<br>0x0<br>30<br>GPIO47_EDGE_LOW<br>RW<br>0x0<br>29<br>GPIO47_LEVEL_HIGH<br>RW<br>0x0<br>28<br>GPIO47_LEVEL_LOW<br>RW<br>0x0<br>27<br>GPIO46_EDGE_HIGH<br>RW<br>0x0<br>26<br>GPIO46_EDGE_LOW<br>RW<br>0x0<br>25<br>GPIO46_LEVEL_HIGH<br>RW<br>0x0<br>24<br>GPIO46_LEVEL_LOW<br>RW<br>0x0<br>23<br>GPIO45_EDGE_HIGH<br>RW<br>0x0<br>22<br>GPIO45_EDGE_LOW<br>RW<br>0x0<br>21<br>GPIO45_LEVEL_HIGH<br>RW<br>0x0<br>20<br>GPIO45_LEVEL_LOW<br>RW<br>0x0<br>19<br>GPIO44_EDGE_HIGH<br>RW<br>0x0<br>18<br>GPIO44_EDGE_LOW<br>RW<br>0x0<br>17<br>GPIO44_LEVEL_HIGH<br>RW<br>0x0<br>16<br>GPIO44_LEVEL_LOW<br>RW<br>0x0<br>15<br>GPIO43_EDGE_HIGH<br>RW<br>0x0<br>14<br>GPIO43_EDGE_LOW<br>RW<br>0x0<br>13<br>GPIO43_LEVEL_HIGH<br>RW<br>0x0<br>12<br>GPIO43_LEVEL_LOW<br>RW<br>0x0<br>11<br>GPIO42_EDGE_HIGH<br>RW<br>0x0<br>10<br>GPIO42_EDGE_LOW<br>RW<br>0x0<br>9<br>GPIO42_LEVEL_HIGH<br>RW<br>0x0<br>8<br>GPIO42_LEVEL_LOW<br>RW<br>0x0<br>7<br>GPIO41_EDGE_HIGH<br>RW<br>0x0<br>6<br>GPIO41_EDGE_LOW<br>RW<br>0x0<br>5<br>GPIO41_LEVEL_HIGH<br>RW<br>0x0<br>4<br>GPIO41_LEVEL_LOW<br>RW<br>0x0<br>3<br>GPIO40_EDGE_HIGH<br>RW<br>0x0<br>2<br>GPIO40_EDGE_LOW<br>RW<br>0x0<br>1<br>GPIO40_LEVEL_HIGH<br>RW<br>0x0<br>0<br>GPIO40_LEVEL_LOW<br>RW<br>0x0 | Bits | Description | Type | Reset |
|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------------|------|-------|
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |      |             |      |       |

<span id="page-708-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTF0 Register**

**Offset**: 0x260

Interrupt Force for proc0

*Table 769. PROC0\_INTF0 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO7_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO7_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO7_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO7_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO6_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO6_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO6_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO6_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO5_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO5_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO5_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO5_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO4_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO4_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO4_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO4_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO3_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO3_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RW   | 0x0   |

<span id="page-709-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTF1 Register**

**Offset**: 0x264

Interrupt Force for proc0

*Table 770. PROC0\_INTF1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO15_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO12_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | RW   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | RW   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RW   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RW   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | RW   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | RW   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RW   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RW   | 0x0   |

<span id="page-710-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTF2 Register**

**Offset**: 0x268

Interrupt Force for proc0

*Table 771. PROC0\_INTF2 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO23_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO20_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RW   | 0x0   |

<span id="page-711-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTF3 Register**

**Offset**: 0x26c

Interrupt Force for proc0

*Table 772. PROC0\_INTF3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO29_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RW   | 0x0   |

<span id="page-712-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTF4 Register**

**Offset**: 0x270

Interrupt Force for proc0

*Table 773. PROC0\_INTF4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO37_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RW   | 0x0   |

<span id="page-713-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTF5 Register**

**Offset**: 0x274

Interrupt Force for proc0

*Table 774. PROC0\_INTF5 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO47_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO47_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO47_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO47_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO46_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO46_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO46_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO46_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO45_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO45_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO45_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO45_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO44_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO44_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO44_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO44_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO43_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO43_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO43_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO43_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO42_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO42_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO42_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO42_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO41_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO41_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO41_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO41_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO40_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO40_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO40_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO40_LEVEL_LOW  | RW   | 0x0   |

<span id="page-714-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTS0 Register**

**Offset**: 0x278

Interrupt status after masking & forcing for proc0

*Table 775. PROC0\_INTS0 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO7_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO7_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO7_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO7_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO6_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO6_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO6_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO6_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO5_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO5_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO5_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO5_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO4_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO4_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO4_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO4_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO3_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO3_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RO   | 0x0   |

<span id="page-715-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTS1 Register**

**Offset**: 0x27c

Interrupt status after masking & forcing for proc0

*Table 776. PROC0\_INTS1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO15_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO12_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | RO   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | RO   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RO   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RO   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | RO   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | RO   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RO   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RO   | 0x0   |

<span id="page-716-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTS2 Register**

**Offset**: 0x280

Interrupt status after masking & forcing for proc0

*Table 777. PROC0\_INTS2 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO23_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO20_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RO   | 0x0   |

<span id="page-717-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTS3 Register**

**Offset**: 0x284

Interrupt status after masking & forcing for proc0

*Table 778. PROC0\_INTS3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO29_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RO   | 0x0   |

<span id="page-718-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTS4 Register**

**Offset**: 0x288

Interrupt status after masking & forcing for proc0

*Table 779. PROC0\_INTS4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO37_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RO   | 0x0   |

<span id="page-719-0"></span>**[IO\\_BANK0:](#page-601-2) PROC0\_INTS5 Register**

**Offset**: 0x28c

Interrupt status after masking & forcing for proc0

*Table 780. PROC0\_INTS5 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO47_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO47_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO47_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO47_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO46_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO46_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO46_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO46_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO45_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO45_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO45_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO45_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO44_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO44_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO44_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO44_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO43_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO43_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO43_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO43_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO42_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO42_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO42_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO42_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO41_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO41_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO41_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO41_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO40_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO40_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO40_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO40_LEVEL_LOW  | RO   | 0x0   |

<span id="page-720-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTE0 Register**

**Offset**: 0x290

Interrupt Enable for proc1

*Table 781. PROC1\_INTE0 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO7_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO7_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO7_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO7_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO6_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO6_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO6_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO6_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO5_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO5_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO5_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO5_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO4_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO4_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO4_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO4_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO3_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO3_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RW   | 0x0   |

<span id="page-721-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTE1 Register**

**Offset**: 0x294

Interrupt Enable for proc1

*Table 782. PROC1\_INTE1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO15_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO12_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | RW   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | RW   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RW   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RW   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | RW   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | RW   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RW   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RW   | 0x0   |

<span id="page-722-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTE2 Register**

**Offset**: 0x298

Interrupt Enable for proc1

*Table 783. PROC1\_INTE2 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO23_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO20_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RW   | 0x0   |

<span id="page-723-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTE3 Register**

**Offset**: 0x29c

Interrupt Enable for proc1

*Table 784. PROC1\_INTE3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO29_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RW   | 0x0   |

<span id="page-724-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTE4 Register**

**Offset**: 0x2a0

Interrupt Enable for proc1

*Table 785. PROC1\_INTE4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO37_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RW   | 0x0   |

<span id="page-725-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTE5 Register**

**Offset**: 0x2a4

Interrupt Enable for proc1

*Table 786. PROC1\_INTE5 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO47_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO47_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO47_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO47_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO46_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO46_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO46_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO46_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO45_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO45_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO45_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO45_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO44_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO44_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO44_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO44_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO43_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO43_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO43_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO43_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO42_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO42_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO42_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO42_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO41_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO41_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO41_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO41_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO40_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO40_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO40_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO40_LEVEL_LOW  | RW   | 0x0   |

<span id="page-726-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTF0 Register**

**Offset**: 0x2a8

Interrupt Force for proc1

*Table 787. PROC1\_INTF0 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO7_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO7_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO7_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO7_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO6_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO6_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO6_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO6_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO5_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO5_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO5_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO5_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO4_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO4_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO4_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO4_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO3_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO3_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RW   | 0x0   |

<span id="page-727-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTF1 Register**

**Offset**: 0x2ac

Interrupt Force for proc1

*Table 788. PROC1\_INTF1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO15_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO12_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | RW   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | RW   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RW   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RW   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | RW   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | RW   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RW   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RW   | 0x0   |

<span id="page-728-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTF2 Register**

**Offset**: 0x2b0

Interrupt Force for proc1

*Table 789. PROC1\_INTF2 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO23_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO20_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RW   | 0x0   |

<span id="page-729-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTF3 Register**

**Offset**: 0x2b4

Interrupt Force for proc1

*Table 790. PROC1\_INTF3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO29_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RW   | 0x0   |

<span id="page-730-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTF4 Register**

**Offset**: 0x2b8

Interrupt Force for proc1

*Table 791. PROC1\_INTF4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO37_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RW   | 0x0   |

<span id="page-731-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTF5 Register**

**Offset**: 0x2bc

Interrupt Force for proc1

*Table 792. PROC1\_INTF5 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO47_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO47_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO47_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO47_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO46_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO46_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO46_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO46_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO45_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO45_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO45_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO45_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO44_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO44_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO44_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO44_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO43_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO43_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO43_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO43_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO42_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO42_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO42_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO42_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO41_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO41_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO41_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO41_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO40_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO40_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO40_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO40_LEVEL_LOW  | RW   | 0x0   |

<span id="page-732-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTS0 Register**

**Offset**: 0x2c0

Interrupt status after masking & forcing for proc1

*Table 793. PROC1\_INTS0 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO7_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO7_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO7_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO7_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO6_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO6_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO6_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO6_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO5_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO5_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO5_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO5_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO4_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO4_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO4_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO4_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO3_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO3_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RO   | 0x0   |

<span id="page-733-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTS1 Register**

**Offset**: 0x2c4

Interrupt status after masking & forcing for proc1

*Table 794. PROC1\_INTS1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO15_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO12_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | RO   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | RO   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RO   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RO   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | RO   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | RO   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RO   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RO   | 0x0   |

<span id="page-734-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTS2 Register**

**Offset**: 0x2c8

Interrupt status after masking & forcing for proc1

*Table 795. PROC1\_INTS2 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO23_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO20_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RO   | 0x0   |

<span id="page-735-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTS3 Register**

**Offset**: 0x2cc

Interrupt status after masking & forcing for proc1

*Table 796. PROC1\_INTS3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO29_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RO   | 0x0   |

<span id="page-736-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTS4 Register**

**Offset**: 0x2d0

Interrupt status after masking & forcing for proc1

*Table 797. PROC1\_INTS4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO37_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RO   | 0x0   |

<span id="page-737-0"></span>**[IO\\_BANK0:](#page-601-2) PROC1\_INTS5 Register**

**Offset**: 0x2d4

Interrupt status after masking & forcing for proc1

*Table 798. PROC1\_INTS5 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO47_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO47_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO47_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO47_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO46_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO46_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO46_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO46_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO45_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO45_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO45_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO45_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO44_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO44_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO44_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO44_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO43_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO43_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO43_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO43_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO42_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO42_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO42_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO42_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO41_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO41_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO41_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO41_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO40_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO40_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO40_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO40_LEVEL_LOW  | RO   | 0x0   |

#### <span id="page-738-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTE0 Register**

**Offset**: 0x2d8

Interrupt Enable for dormant\_wake

*Table 799. DORMANT\_WAKE\_INT E0 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO7_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO7_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO7_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO7_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO6_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO6_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO6_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO6_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO5_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO5_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO5_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO5_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO4_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO4_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO4_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO4_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO3_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO3_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RW   | 0x0   |

# <span id="page-739-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTE1 Register**

**Offset**: 0x2dc

Interrupt Enable for dormant\_wake

*Table 800. DORMANT\_WAKE\_INT E1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO15_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO12_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | RW   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | RW   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RW   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RW   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | RW   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | RW   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RW   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RW   | 0x0   |

# <span id="page-740-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTE2 Register**

**Offset**: 0x2e0

Interrupt Enable for dormant\_wake

*Table 801. DORMANT\_WAKE\_INT E2 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO23_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO20_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RW   | 0x0   |

# <span id="page-741-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTE3 Register**

**Offset**: 0x2e4

Interrupt Enable for dormant\_wake

*Table 802. DORMANT\_WAKE\_INT E3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO29_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RW   | 0x0   |

# <span id="page-742-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTE4 Register**

**Offset**: 0x2e8

Interrupt Enable for dormant\_wake

*Table 803. DORMANT\_WAKE\_INT E4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO37_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RW   | 0x0   |

# <span id="page-743-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTE5 Register**

**Offset**: 0x2ec

Interrupt Enable for dormant\_wake

*Table 804. DORMANT\_WAKE\_INT E5 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO47_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO47_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO47_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO47_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO46_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO46_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO46_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO46_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO45_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO45_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO45_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO45_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO44_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO44_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO44_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO44_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO43_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO43_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO43_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO43_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO42_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO42_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO42_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO42_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO41_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO41_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO41_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO41_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO40_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO40_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO40_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO40_LEVEL_LOW  | RW   | 0x0   |

# <span id="page-744-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTF0 Register**

**Offset**: 0x2f0

Interrupt Force for dormant\_wake

*Table 805. DORMANT\_WAKE\_INT F0 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO7_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO7_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO7_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO7_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO6_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO6_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO6_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO6_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO5_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO5_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO5_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO5_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO4_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO4_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO4_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO4_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO3_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO3_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RW   | 0x0   |

#### <span id="page-745-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTF1 Register**

**Offset**: 0x2f4

Interrupt Force for dormant\_wake

*Table 806. DORMANT\_WAKE\_INT F1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO15_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO12_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | RW   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | RW   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RW   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RW   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | RW   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | RW   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RW   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RW   | 0x0   |

#### <span id="page-746-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTF2 Register**

**Offset**: 0x2f8

Interrupt Force for dormant\_wake

*Table 807. DORMANT\_WAKE\_INT F2 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO23_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO20_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RW   | 0x0   |

# <span id="page-747-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTF3 Register**

**Offset**: 0x2fc

Interrupt Force for dormant\_wake

*Table 808. DORMANT\_WAKE\_INT F3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO29_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RW   | 0x0   |

#### <span id="page-748-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTF4 Register**

**Offset**: 0x300

Interrupt Force for dormant\_wake

*Table 809. DORMANT\_WAKE\_INT F4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO37_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RW   | 0x0   |

#### <span id="page-749-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTF5 Register**

**Offset**: 0x304

Interrupt Force for dormant\_wake

*Table 810. DORMANT\_WAKE\_INT F5 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO47_EDGE_HIGH  | RW   | 0x0   |
| 30   | GPIO47_EDGE_LOW   | RW   | 0x0   |
| 29   | GPIO47_LEVEL_HIGH | RW   | 0x0   |
| 28   | GPIO47_LEVEL_LOW  | RW   | 0x0   |
| 27   | GPIO46_EDGE_HIGH  | RW   | 0x0   |
| 26   | GPIO46_EDGE_LOW   | RW   | 0x0   |
| 25   | GPIO46_LEVEL_HIGH | RW   | 0x0   |
| 24   | GPIO46_LEVEL_LOW  | RW   | 0x0   |
| 23   | GPIO45_EDGE_HIGH  | RW   | 0x0   |
| 22   | GPIO45_EDGE_LOW   | RW   | 0x0   |
| 21   | GPIO45_LEVEL_HIGH | RW   | 0x0   |
| 20   | GPIO45_LEVEL_LOW  | RW   | 0x0   |
| 19   | GPIO44_EDGE_HIGH  | RW   | 0x0   |
| 18   | GPIO44_EDGE_LOW   | RW   | 0x0   |
| 17   | GPIO44_LEVEL_HIGH | RW   | 0x0   |
| 16   | GPIO44_LEVEL_LOW  | RW   | 0x0   |
| 15   | GPIO43_EDGE_HIGH  | RW   | 0x0   |
| 14   | GPIO43_EDGE_LOW   | RW   | 0x0   |
| 13   | GPIO43_LEVEL_HIGH | RW   | 0x0   |
| 12   | GPIO43_LEVEL_LOW  | RW   | 0x0   |
| 11   | GPIO42_EDGE_HIGH  | RW   | 0x0   |
| 10   | GPIO42_EDGE_LOW   | RW   | 0x0   |
| 9    | GPIO42_LEVEL_HIGH | RW   | 0x0   |
| 8    | GPIO42_LEVEL_LOW  | RW   | 0x0   |
| 7    | GPIO41_EDGE_HIGH  | RW   | 0x0   |
| 6    | GPIO41_EDGE_LOW   | RW   | 0x0   |
| 5    | GPIO41_LEVEL_HIGH | RW   | 0x0   |
| 4    | GPIO41_LEVEL_LOW  | RW   | 0x0   |
| 3    | GPIO40_EDGE_HIGH  | RW   | 0x0   |
| 2    | GPIO40_EDGE_LOW   | RW   | 0x0   |
| 1    | GPIO40_LEVEL_HIGH | RW   | 0x0   |
| 0    | GPIO40_LEVEL_LOW  | RW   | 0x0   |

# <span id="page-750-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTS0 Register**

**Offset**: 0x308

Interrupt status after masking & forcing for dormant\_wake

*Table 811. DORMANT\_WAKE\_INT S0 Register*

| Bits | Description      | Type | Reset |
|------|------------------|------|-------|
| 31   | GPIO7_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO7_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO7_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO7_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO6_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO6_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO6_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO6_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO5_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO5_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO5_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO5_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO4_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO4_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO4_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO4_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO3_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO3_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO3_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO3_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO2_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO2_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO2_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO2_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO1_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO1_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO1_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO1_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO0_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO0_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO0_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO0_LEVEL_LOW  | RO   | 0x0   |

#### <span id="page-751-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTS1 Register**

**Offset**: 0x30c

Interrupt status after masking & forcing for dormant\_wake

*Table 812. DORMANT\_WAKE\_INT S1 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO15_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO15_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO15_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO15_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO14_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO14_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO14_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO14_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO13_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO13_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO13_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO13_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO12_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO12_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO12_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO12_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO11_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO11_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO11_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO11_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO10_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO10_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO10_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO10_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO9_EDGE_HIGH   | RO   | 0x0   |
| 6    | GPIO9_EDGE_LOW    | RO   | 0x0   |
| 5    | GPIO9_LEVEL_HIGH  | RO   | 0x0   |
| 4    | GPIO9_LEVEL_LOW   | RO   | 0x0   |
| 3    | GPIO8_EDGE_HIGH   | RO   | 0x0   |
| 2    | GPIO8_EDGE_LOW    | RO   | 0x0   |
| 1    | GPIO8_LEVEL_HIGH  | RO   | 0x0   |
| 0    | GPIO8_LEVEL_LOW   | RO   | 0x0   |

#### <span id="page-752-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTS2 Register**

**Offset**: 0x310

Interrupt status after masking & forcing for dormant\_wake

*Table 813. DORMANT\_WAKE\_INT S2 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO23_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO23_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO23_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO23_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO22_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO22_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO22_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO22_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO21_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO21_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO21_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO21_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO20_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO20_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO20_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO20_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO19_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO19_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO19_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO19_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO18_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO18_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO18_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO18_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO17_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO17_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO17_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO17_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO16_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO16_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO16_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO16_LEVEL_LOW  | RO   | 0x0   |

#### <span id="page-753-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTS3 Register**

**Offset**: 0x314

Interrupt status after masking & forcing for dormant\_wake

*Table 814. DORMANT\_WAKE\_INT S3 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO31_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO31_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO31_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO31_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO30_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO30_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO30_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO30_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO29_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO29_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO29_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO29_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO28_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO28_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO28_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO28_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO27_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO27_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO27_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO27_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO26_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO26_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO26_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO26_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO25_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO25_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO25_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO25_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO24_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO24_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO24_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO24_LEVEL_LOW  | RO   | 0x0   |

#### <span id="page-754-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTS4 Register**

**Offset**: 0x318

Interrupt status after masking & forcing for dormant\_wake

*Table 815. DORMANT\_WAKE\_INT S4 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO39_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO39_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO39_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO39_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO38_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO38_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO38_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO38_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO37_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO37_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO37_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO37_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO36_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO36_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO36_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO36_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO35_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO35_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO35_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO35_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO34_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO34_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO34_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO34_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO33_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO33_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO33_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO33_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO32_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO32_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO32_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO32_LEVEL_LOW  | RO   | 0x0   |

#### <span id="page-755-0"></span>**[IO\\_BANK0:](#page-601-2) DORMANT\_WAKE\_INTS5 Register**

**Offset**: 0x31c

Interrupt status after masking & forcing for dormant\_wake

*Table 816. DORMANT\_WAKE\_INT S5 Register*

| Bits | Description       | Type | Reset |
|------|-------------------|------|-------|
| 31   | GPIO47_EDGE_HIGH  | RO   | 0x0   |
| 30   | GPIO47_EDGE_LOW   | RO   | 0x0   |
| 29   | GPIO47_LEVEL_HIGH | RO   | 0x0   |
| 28   | GPIO47_LEVEL_LOW  | RO   | 0x0   |
| 27   | GPIO46_EDGE_HIGH  | RO   | 0x0   |
| 26   | GPIO46_EDGE_LOW   | RO   | 0x0   |
| 25   | GPIO46_LEVEL_HIGH | RO   | 0x0   |
| 24   | GPIO46_LEVEL_LOW  | RO   | 0x0   |
| 23   | GPIO45_EDGE_HIGH  | RO   | 0x0   |
| 22   | GPIO45_EDGE_LOW   | RO   | 0x0   |
| 21   | GPIO45_LEVEL_HIGH | RO   | 0x0   |
| 20   | GPIO45_LEVEL_LOW  | RO   | 0x0   |
| 19   | GPIO44_EDGE_HIGH  | RO   | 0x0   |
| 18   | GPIO44_EDGE_LOW   | RO   | 0x0   |
| 17   | GPIO44_LEVEL_HIGH | RO   | 0x0   |
| 16   | GPIO44_LEVEL_LOW  | RO   | 0x0   |
| 15   | GPIO43_EDGE_HIGH  | RO   | 0x0   |
| 14   | GPIO43_EDGE_LOW   | RO   | 0x0   |
| 13   | GPIO43_LEVEL_HIGH | RO   | 0x0   |
| 12   | GPIO43_LEVEL_LOW  | RO   | 0x0   |
| 11   | GPIO42_EDGE_HIGH  | RO   | 0x0   |
| 10   | GPIO42_EDGE_LOW   | RO   | 0x0   |
| 9    | GPIO42_LEVEL_HIGH | RO   | 0x0   |
| 8    | GPIO42_LEVEL_LOW  | RO   | 0x0   |
| 7    | GPIO41_EDGE_HIGH  | RO   | 0x0   |
| 6    | GPIO41_EDGE_LOW   | RO   | 0x0   |
| 5    | GPIO41_LEVEL_HIGH | RO   | 0x0   |
| 4    | GPIO41_LEVEL_LOW  | RO   | 0x0   |
| 3    | GPIO40_EDGE_HIGH  | RO   | 0x0   |
| 2    | GPIO40_EDGE_LOW   | RO   | 0x0   |
| 1    | GPIO40_LEVEL_HIGH | RO   | 0x0   |
| 0    | GPIO40_LEVEL_LOW  | RO   | 0x0   |

