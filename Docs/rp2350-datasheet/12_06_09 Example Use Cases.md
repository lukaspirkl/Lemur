# 12.6.9 Example Use Cases

#### **12.6.9.1. Using Interrupts to Reconfigure a Channel**

When a channel finishes a block of transfers, it becomes available for making more transfers. Software detects that the channel is no longer busy, and reconfigures and restarts the channel. One approach is to poll the CTRL\_BUSY bit until the channel is done, but this loses one of the key advantages of the DMA, namely that it does *not* have to operate in lockstep with a processor. By setting the correct bit in INTE0 through INTE3, you can instruct the DMA to raise one of its four interrupt request lines when a given channel completes. Rather than repeatedly asking if a channel is done, you are told.

![](_page_1105_Figure_12.jpeg)

Having four system interrupt lines allows different channel completion interrupts to be routed to different cores, or to pre-empt one another on the same core if one channel is more time-critical. It also allows channel interrupts to target different security domains.

When the interrupt is asserted, the processor can be configured to drop whatever it is doing and call a user-specified handler function. The handler can reconfigure and restart the channel. When the handler exits, the processor returns to the interrupted code running in the foreground.

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/dma/channel\\_irq/channel\\_irq.c](https://github.com/raspberrypi/pico-examples/blob/master/dma/channel_irq/channel_irq.c#L35-L52) Lines 35 - 52*

```
35 void dma_handler() {
36 static int pwm_level = 0;
37 static uint32_t wavetable[N_PWM_LEVELS];
38 static bool first_run = true;
39 // Entry number `i` has `i` one bits and `(32 - i)` zero bits.
40 if (first_run) {
41 first_run = false;
42 for (int i = 0; i < N_PWM_LEVELS; ++i)
43 wavetable[i] = ~(~0u << i);
44 }
45
```

```
46 // Clear the interrupt request.
47 dma_hw->ints0 = 1u << dma_chan;
48 // Give the channel a new wave table entry to read from, and re-trigger it
49 dma_channel_set_read_addr(dma_chan, &wavetable[pwm_level], true);
50 
51 pwm_level = (pwm_level + 1) % N_PWM_LEVELS;
52 }
```

In many cases, most of the configuration can be done the first time the channel starts. This way, only addresses and transfer lengths need reprogramming in the interrupt handler.

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/dma/channel\\_irq/channel\\_irq.c](https://github.com/raspberrypi/pico-examples/blob/master/dma/channel_irq/channel_irq.c#L54-L94) Lines 54 - 94*

```
54 int main() {
55 #ifndef PICO_DEFAULT_LED_PIN
56 #warning dma/channel_irq example requires a board with a regular LED
57 #else
58 // Set up a PIO state machine to serialise our bits
59 uint offset = pio_add_program(pio0, &pio_serialiser_program);
60 pio_serialiser_program_init(pio0, 0, offset, PICO_DEFAULT_LED_PIN, PIO_SERIAL_CLKDIV);
61 
62 // Configure a channel to write the same word (32 bits) repeatedly to PIO0
63 // SM0's TX FIFO, paced by the data request signal from that peripheral.
64 dma_chan = dma_claim_unused_channel(true);
65 dma_channel_config c = dma_channel_get_default_config(dma_chan);
66 channel_config_set_transfer_data_size(&c, DMA_SIZE_32);
67 channel_config_set_read_increment(&c, false);
68 channel_config_set_dreq(&c, DREQ_PIO0_TX0);
69 
70 dma_channel_configure(
71 dma_chan,
72 &c,
73 &pio0_hw->txf[0], // Write address (only need to set this once)
74 NULL, // Don't provide a read address yet
75 PWM_REPEAT_COUNT, // Write the same value many times, then halt and interrupt
76 false // Don't start yet
77 );
78 
79 // Tell the DMA to raise IRQ line 0 when the channel finishes a block
80 dma_channel_set_irq0_enabled(dma_chan, true);
81 
82 // Configure the processor to run dma_handler() when DMA IRQ 0 is asserted
83 irq_set_exclusive_handler(DMA_IRQ_0, dma_handler);
84 irq_set_enabled(DMA_IRQ_0, true);
85 
86 // Manually call the handler once, to trigger the first transfer
87 dma_handler();
88 
89 // Everything else from this point is interrupt-driven. The processor has
90 // time to sit and think about its early retirement -- maybe open a bakery?
91 while (true)
92 tight_loop_contents();
93 #endif
94 }
```

One disadvantage of this technique is that you don't start to reconfigure the channel until some time after the channel makes its last transfer. If there is heavy interrupt activity on the processor, this may be quite a long time, and quite a large gap in transfers. This makes it difficult to sustain a high data throughput.

This is solved by using two channels, with their CHAIN\_TO fields crossed over, so that channel A triggers channel B when it completes, and vice versa. At any point in time, one of the channels is transferring data. The other is either already

configured to start the next transfer immediately when the current one finishes, or it is in the process of being reconfigured. When channel A completes, it immediately starts the cued-up transfer on channel B. At the same time, the interrupt is fired, and the handler reconfigures channel A so that it is ready when channel B completes.

#### <span id="page-1107-0"></span>**12.6.9.2. DMA Control Blocks**

Frequently, multiple smaller buffers must be gathered together and sent to the same peripheral. To address this use case, the RP2350 DMA can execute a long and complex sequence of transfers without processor control. One channel repeatedly reconfigures a second channel, and the second channel restarts the first each time it completes block of transfers.

Because the first DMA channel transfers data directly from memory to the second channel's control registers, the format of the control blocks in memory must match those registers. Each time, the last register written to will be one of the trigger registers [\(Section 12.6.3.1](#page-1096-0)), which will start the second channel on its programmed block of transfers. The register aliases ([Section 12.6.3.1\)](#page-1096-0) give some flexibility for the block layout, and more importantly allow some registers to be omitted from the blocks, so they occupy less memory and can be loaded more quickly.

This example shows how multiple buffers can be gathered and transferred to the UART, by reprogramming TRANS\_COUNT and READ\_ADDR\_TRIG:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/dma/control\\_blocks/control\\_blocks.c](https://github.com/raspberrypi/pico-examples/blob/master/dma/control_blocks/control_blocks.c)*

```
  1 /**
  2 * Copyright (c) 2020 Raspberry Pi (Trading) Ltd.
  3 *
  4 * SPDX-License-Identifier: BSD-3-Clause
  5 */
  6 
  7 // Use two DMA channels to make a programmed sequence of data transfers to the
  8 // UART (a data gather operation). One channel is responsible for transferring
  9 // the actual data, the other repeatedly reprograms that channel.
 10 
 11 #include <stdio.h>
 12 #include "pico/stdlib.h"
 13 #include "hardware/dma.h"
 14 #include "hardware/structs/uart.h"
 15 
 16 // These buffers will be DMA'd to the UART, one after the other.
 17 
 18 const char word0[] = "Transferring ";
 19 const char word1[] = "one ";
 20 const char word2[] = "word ";
 21 const char word3[] = "at ";
 22 const char word4[] = "a ";
 23 const char word5[] = "time.\n";
 24 
 25 // Note the order of the fields here: it's important that the length is before
 26 // the read address, because the control channel is going to write to the last
 27 // two registers in alias 3 on the data channel:
 28 // +0x0 +0x4 +0x8 +0xC (Trigger)
 29 // Alias 0: READ_ADDR WRITE_ADDR TRANS_COUNT CTRL
 30 // Alias 1: CTRL READ_ADDR WRITE_ADDR TRANS_COUNT
 31 // Alias 2: CTRL TRANS_COUNT READ_ADDR WRITE_ADDR
 32 // Alias 3: CTRL WRITE_ADDR TRANS_COUNT READ_ADDR
 33 //
 34 // This will program the transfer count and read address of the data channel,
 35 // and trigger it. Once the data channel completes, it will restart the
 36 // control channel (via CHAIN_TO) to load the next two words into its control
 37 // registers.
 38 
 39 const struct {uint32_t len; const char *data;} control_blocks[] = {
```

```
 40 {count_of(word0) - 1, word0}, // Skip null terminator
 41 {count_of(word1) - 1, word1},
 42 {count_of(word2) - 1, word2},
 43 {count_of(word3) - 1, word3},
 44 {count_of(word4) - 1, word4},
 45 {count_of(word5) - 1, word5},
 46 {0, NULL} // Null trigger to end chain.
 47 };
 48 
 49 int main() {
 50 #ifndef uart_default
 51 #warning dma/control_blocks example requires a UART
 52 #else
 53 stdio_init_all();
 54 puts("DMA control block example:");
 55 
 56 // ctrl_chan loads control blocks into data_chan, which executes them.
 57 int ctrl_chan = dma_claim_unused_channel(true);
 58 int data_chan = dma_claim_unused_channel(true);
 59 
 60 // The control channel transfers two words into the data channel's control
 61 // registers, then halts. The write address wraps on a two-word
 62 // (eight-byte) boundary, so that the control channel writes the same two
 63 // registers when it is next triggered.
 64 
 65 dma_channel_config c = dma_channel_get_default_config(ctrl_chan);
 66 channel_config_set_transfer_data_size(&c, DMA_SIZE_32);
 67 channel_config_set_read_increment(&c, true);
 68 channel_config_set_write_increment(&c, true);
 69 channel_config_set_ring(&c, true, 3); // 1 << 3 byte boundary on write ptr
 70 
 71 dma_channel_configure(
 72 ctrl_chan,
 73 &c,
 74 &dma_hw->ch[data_chan].al3_transfer_count, // Initial write address
 75 &control_blocks[0], // Initial read address
 76 2, // Halt after each control block
 77 false // Don't start yet
 78 );
 79 
 80 // The data channel is set up to write to the UART FIFO (paced by the
 81 // UART's TX data request signal) and then chain to the control channel
 82 // once it completes. The control channel programs a new read address and
 83 // data length, and retriggers the data channel.
 84 
 85 c = dma_channel_get_default_config(data_chan);
 86 channel_config_set_transfer_data_size(&c, DMA_SIZE_8);
 87 channel_config_set_dreq(&c, uart_get_dreq(uart_default, true));
 88 // Trigger ctrl_chan when data_chan completes
 89 channel_config_set_chain_to(&c, ctrl_chan);
 90 // Raise the IRQ flag when 0 is written to a trigger register (end of chain):
 91 channel_config_set_irq_quiet(&c, true);
 92 
 93 dma_channel_configure(
 94 data_chan,
 95 &c,
 96 &uart_get_hw(uart_default)->dr,
 97 NULL, // Initial read address and transfer count are unimportant;
 98 0, // the control channel will reprogram them each time.
 99 false // Don't start yet.
100 );
101 
102 // Everything is ready to go. Tell the control channel to load the first
103 // control block. Everything is automatic from here.
```

```
104 dma_start_channel_mask(1u << ctrl_chan);
105 
106 // The data channel will assert its IRQ flag when it gets a null trigger,
107 // indicating the end of the control block list. We're just going to wait
108 // for the IRQ flag instead of setting up an interrupt handler.
109 while (!(dma_hw->intr & 1u << data_chan))
110 tight_loop_contents();
111 dma_hw->ints0 = 1u << data_chan;
112 
113 puts("DMA finished.");
114 #endif
115 }
```

<span id="page-1109-0"></span>The DMA registers start at a base address of 0x50000000 (defined as [DMA\\_BASE](#page-33-0) in SDK).

*Table 1146. List of DMA registers*

<span id="page-1109-1"></span>

| Offset | Name                     | Info                                                                                                                                                            |  |  |
|--------|--------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|--|--|
| 0x000  | CH0_READ_ADDR            | DMA Channel 0 Read Address pointer                                                                                                                              |  |  |
| 0x004  | CH0_WRITE_ADDR           | DMA Channel 0 Write Address pointer                                                                                                                             |  |  |
| 0x008  | CH0_TRANS_COUNT          | DMA Channel 0 Transfer Count                                                                                                                                    |  |  |
| 0x00c  | CH0_CTRL_TRIG            | DMA Channel 0 Control and Status                                                                                                                                |  |  |
| 0x010  | CH0_AL1_CTRL             | Alias for channel 0 CTRL register                                                                                                                               |  |  |
| 0x014  | CH0_AL1_READ_ADDR        | Alias for channel 0 READ_ADDR register                                                                                                                          |  |  |
| 0x018  | CH0_AL1_WRITE_ADDR       | Alias for channel 0 WRITE_ADDR register                                                                                                                         |  |  |
| 0x01c  | CH0_AL1_TRANS_COUNT_TRIG | Alias for channel 0 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |  |  |
| 0x020  | CH0_AL2_CTRL             | Alias for channel 0 CTRL register                                                                                                                               |  |  |
| 0x024  | CH0_AL2_TRANS_COUNT      | Alias for channel 0 TRANS_COUNT register                                                                                                                        |  |  |
| 0x028  | CH0_AL2_READ_ADDR        | Alias for channel 0 READ_ADDR register                                                                                                                          |  |  |
| 0x02c  | CH0_AL2_WRITE_ADDR_TRIG  | Alias for channel 0 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |  |  |
| 0x030  | CH0_AL3_CTRL             | Alias for channel 0 CTRL register                                                                                                                               |  |  |
| 0x034  | CH0_AL3_WRITE_ADDR       | Alias for channel 0 WRITE_ADDR register                                                                                                                         |  |  |
| 0x038  | CH0_AL3_TRANS_COUNT      | Alias for channel 0 TRANS_COUNT register                                                                                                                        |  |  |
| 0x03c  | CH0_AL3_READ_ADDR_TRIG   | Alias for channel 0 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |  |  |
| 0x040  | CH1_READ_ADDR            | DMA Channel 1 Read Address pointer                                                                                                                              |  |  |
| 0x044  | CH1_WRITE_ADDR           | DMA Channel 1 Write Address pointer                                                                                                                             |  |  |
| 0x048  | CH1_TRANS_COUNT          | DMA Channel 1 Transfer Count                                                                                                                                    |  |  |
| 0x04c  | CH1_CTRL_TRIG            | DMA Channel 1 Control and Status                                                                                                                                |  |  |

| Offset | Name                     | Info                                                                                                                                                            |  |  |
|--------|--------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|--|--|
| 0x050  | CH1_AL1_CTRL             | Alias for channel 1 CTRL register                                                                                                                               |  |  |
| 0x054  | CH1_AL1_READ_ADDR        | Alias for channel 1 READ_ADDR register                                                                                                                          |  |  |
| 0x058  | CH1_AL1_WRITE_ADDR       | Alias for channel 1 WRITE_ADDR register                                                                                                                         |  |  |
| 0x05c  | CH1_AL1_TRANS_COUNT_TRIG | Alias for channel 1 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |  |  |
| 0x060  | CH1_AL2_CTRL             | Alias for channel 1 CTRL register                                                                                                                               |  |  |
| 0x064  | CH1_AL2_TRANS_COUNT      | Alias for channel 1 TRANS_COUNT register                                                                                                                        |  |  |
| 0x068  | CH1_AL2_READ_ADDR        | Alias for channel 1 READ_ADDR register                                                                                                                          |  |  |
| 0x06c  | CH1_AL2_WRITE_ADDR_TRIG  | Alias for channel 1 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |  |  |
| 0x070  | CH1_AL3_CTRL             | Alias for channel 1 CTRL register                                                                                                                               |  |  |
| 0x074  | CH1_AL3_WRITE_ADDR       | Alias for channel 1 WRITE_ADDR register                                                                                                                         |  |  |
| 0x078  | CH1_AL3_TRANS_COUNT      | Alias for channel 1 TRANS_COUNT register                                                                                                                        |  |  |
| 0x07c  | CH1_AL3_READ_ADDR_TRIG   | Alias for channel 1 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |  |  |
| 0x080  | CH2_READ_ADDR            | DMA Channel 2 Read Address pointer                                                                                                                              |  |  |
| 0x084  | CH2_WRITE_ADDR           | DMA Channel 2 Write Address pointer                                                                                                                             |  |  |
| 0x088  | CH2_TRANS_COUNT          | DMA Channel 2 Transfer Count                                                                                                                                    |  |  |
| 0x08c  | CH2_CTRL_TRIG            | DMA Channel 2 Control and Status                                                                                                                                |  |  |
| 0x090  | CH2_AL1_CTRL             | Alias for channel 2 CTRL register                                                                                                                               |  |  |
| 0x094  | CH2_AL1_READ_ADDR        | Alias for channel 2 READ_ADDR register                                                                                                                          |  |  |
| 0x098  | CH2_AL1_WRITE_ADDR       | Alias for channel 2 WRITE_ADDR register                                                                                                                         |  |  |
| 0x09c  | CH2_AL1_TRANS_COUNT_TRIG | Alias for channel 2 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |  |  |
| 0x0a0  | CH2_AL2_CTRL             | Alias for channel 2 CTRL register                                                                                                                               |  |  |
| 0x0a4  | CH2_AL2_TRANS_COUNT      | Alias for channel 2 TRANS_COUNT register                                                                                                                        |  |  |
| 0x0a8  | CH2_AL2_READ_ADDR        | Alias for channel 2 READ_ADDR register                                                                                                                          |  |  |
| 0x0ac  | CH2_AL2_WRITE_ADDR_TRIG  | Alias for channel 2 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |  |  |
| 0x0b0  | CH2_AL3_CTRL             | Alias for channel 2 CTRL register                                                                                                                               |  |  |
| 0x0b4  | CH2_AL3_WRITE_ADDR       | Alias for channel 2 WRITE_ADDR register                                                                                                                         |  |  |
| 0x0b8  | CH2_AL3_TRANS_COUNT      | Alias for channel 2 TRANS_COUNT register                                                                                                                        |  |  |
| 0x0bc  | CH2_AL3_READ_ADDR_TRIG   | Alias for channel 2 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |  |  |

| Offset | Name                     | Info                                                                                                                                                            |
|--------|--------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x0c0  | CH3_READ_ADDR            | DMA Channel 3 Read Address pointer                                                                                                                              |
| 0x0c4  | CH3_WRITE_ADDR           | DMA Channel 3 Write Address pointer                                                                                                                             |
| 0x0c8  | CH3_TRANS_COUNT          | DMA Channel 3 Transfer Count                                                                                                                                    |
| 0x0cc  | CH3_CTRL_TRIG            | DMA Channel 3 Control and Status                                                                                                                                |
| 0x0d0  | CH3_AL1_CTRL             | Alias for channel 3 CTRL register                                                                                                                               |
| 0x0d4  | CH3_AL1_READ_ADDR        | Alias for channel 3 READ_ADDR register                                                                                                                          |
| 0x0d8  | CH3_AL1_WRITE_ADDR       | Alias for channel 3 WRITE_ADDR register                                                                                                                         |
| 0x0dc  | CH3_AL1_TRANS_COUNT_TRIG | Alias for channel 3 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x0e0  | CH3_AL2_CTRL             | Alias for channel 3 CTRL register                                                                                                                               |
| 0x0e4  | CH3_AL2_TRANS_COUNT      | Alias for channel 3 TRANS_COUNT register                                                                                                                        |
| 0x0e8  | CH3_AL2_READ_ADDR        | Alias for channel 3 READ_ADDR register                                                                                                                          |
| 0x0ec  | CH3_AL2_WRITE_ADDR_TRIG  | Alias for channel 3 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x0f0  | CH3_AL3_CTRL             | Alias for channel 3 CTRL register                                                                                                                               |
| 0x0f4  | CH3_AL3_WRITE_ADDR       | Alias for channel 3 WRITE_ADDR register                                                                                                                         |
| 0x0f8  | CH3_AL3_TRANS_COUNT      | Alias for channel 3 TRANS_COUNT register                                                                                                                        |
| 0x0fc  | CH3_AL3_READ_ADDR_TRIG   | Alias for channel 3 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x100  | CH4_READ_ADDR            | DMA Channel 4 Read Address pointer                                                                                                                              |
| 0x104  | CH4_WRITE_ADDR           | DMA Channel 4 Write Address pointer                                                                                                                             |
| 0x108  | CH4_TRANS_COUNT          | DMA Channel 4 Transfer Count                                                                                                                                    |
| 0x10c  | CH4_CTRL_TRIG            | DMA Channel 4 Control and Status                                                                                                                                |
| 0x110  | CH4_AL1_CTRL             | Alias for channel 4 CTRL register                                                                                                                               |
| 0x114  | CH4_AL1_READ_ADDR        | Alias for channel 4 READ_ADDR register                                                                                                                          |
| 0x118  | CH4_AL1_WRITE_ADDR       | Alias for channel 4 WRITE_ADDR register                                                                                                                         |
| 0x11c  | CH4_AL1_TRANS_COUNT_TRIG | Alias for channel 4 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x120  | CH4_AL2_CTRL             | Alias for channel 4 CTRL register                                                                                                                               |
| 0x124  | CH4_AL2_TRANS_COUNT      | Alias for channel 4 TRANS_COUNT register                                                                                                                        |
| 0x128  | CH4_AL2_READ_ADDR        | Alias for channel 4 READ_ADDR register                                                                                                                          |
| 0x12c  | CH4_AL2_WRITE_ADDR_TRIG  | Alias for channel 4 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x130  | CH4_AL3_CTRL             | Alias for channel 4 CTRL register                                                                                                                               |

| Offset | Name                     | Info                                                                                                                                                            |
|--------|--------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x134  | CH4_AL3_WRITE_ADDR       | Alias for channel 4 WRITE_ADDR register                                                                                                                         |
| 0x138  | CH4_AL3_TRANS_COUNT      | Alias for channel 4 TRANS_COUNT register                                                                                                                        |
| 0x13c  | CH4_AL3_READ_ADDR_TRIG   | Alias for channel 4 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x140  | CH5_READ_ADDR            | DMA Channel 5 Read Address pointer                                                                                                                              |
| 0x144  | CH5_WRITE_ADDR           | DMA Channel 5 Write Address pointer                                                                                                                             |
| 0x148  | CH5_TRANS_COUNT          | DMA Channel 5 Transfer Count                                                                                                                                    |
| 0x14c  | CH5_CTRL_TRIG            | DMA Channel 5 Control and Status                                                                                                                                |
| 0x150  | CH5_AL1_CTRL             | Alias for channel 5 CTRL register                                                                                                                               |
| 0x154  | CH5_AL1_READ_ADDR        | Alias for channel 5 READ_ADDR register                                                                                                                          |
| 0x158  | CH5_AL1_WRITE_ADDR       | Alias for channel 5 WRITE_ADDR register                                                                                                                         |
| 0x15c  | CH5_AL1_TRANS_COUNT_TRIG | Alias for channel 5 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x160  | CH5_AL2_CTRL             | Alias for channel 5 CTRL register                                                                                                                               |
| 0x164  | CH5_AL2_TRANS_COUNT      | Alias for channel 5 TRANS_COUNT register                                                                                                                        |
| 0x168  | CH5_AL2_READ_ADDR        | Alias for channel 5 READ_ADDR register                                                                                                                          |
| 0x16c  | CH5_AL2_WRITE_ADDR_TRIG  | Alias for channel 5 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x170  | CH5_AL3_CTRL             | Alias for channel 5 CTRL register                                                                                                                               |
| 0x174  | CH5_AL3_WRITE_ADDR       | Alias for channel 5 WRITE_ADDR register                                                                                                                         |
| 0x178  | CH5_AL3_TRANS_COUNT      | Alias for channel 5 TRANS_COUNT register                                                                                                                        |
| 0x17c  | CH5_AL3_READ_ADDR_TRIG   | Alias for channel 5 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x180  | CH6_READ_ADDR            | DMA Channel 6 Read Address pointer                                                                                                                              |
| 0x184  | CH6_WRITE_ADDR           | DMA Channel 6 Write Address pointer                                                                                                                             |
| 0x188  | CH6_TRANS_COUNT          | DMA Channel 6 Transfer Count                                                                                                                                    |
| 0x18c  | CH6_CTRL_TRIG            | DMA Channel 6 Control and Status                                                                                                                                |
| 0x190  | CH6_AL1_CTRL             | Alias for channel 6 CTRL register                                                                                                                               |
| 0x194  | CH6_AL1_READ_ADDR        | Alias for channel 6 READ_ADDR register                                                                                                                          |
| 0x198  | CH6_AL1_WRITE_ADDR       | Alias for channel 6 WRITE_ADDR register                                                                                                                         |
| 0x19c  | CH6_AL1_TRANS_COUNT_TRIG | Alias for channel 6 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x1a0  | CH6_AL2_CTRL             | Alias for channel 6 CTRL register                                                                                                                               |
| 0x1a4  | CH6_AL2_TRANS_COUNT      | Alias for channel 6 TRANS_COUNT register                                                                                                                        |

| Offset | Name                     | Info                                                                                                                                                            |
|--------|--------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x1a8  | CH6_AL2_READ_ADDR        | Alias for channel 6 READ_ADDR register                                                                                                                          |
| 0x1ac  | CH6_AL2_WRITE_ADDR_TRIG  | Alias for channel 6 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x1b0  | CH6_AL3_CTRL             | Alias for channel 6 CTRL register                                                                                                                               |
| 0x1b4  | CH6_AL3_WRITE_ADDR       | Alias for channel 6 WRITE_ADDR register                                                                                                                         |
| 0x1b8  | CH6_AL3_TRANS_COUNT      | Alias for channel 6 TRANS_COUNT register                                                                                                                        |
| 0x1bc  | CH6_AL3_READ_ADDR_TRIG   | Alias for channel 6 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x1c0  | CH7_READ_ADDR            | DMA Channel 7 Read Address pointer                                                                                                                              |
| 0x1c4  | CH7_WRITE_ADDR           | DMA Channel 7 Write Address pointer                                                                                                                             |
| 0x1c8  | CH7_TRANS_COUNT          | DMA Channel 7 Transfer Count                                                                                                                                    |
| 0x1cc  | CH7_CTRL_TRIG            | DMA Channel 7 Control and Status                                                                                                                                |
| 0x1d0  | CH7_AL1_CTRL             | Alias for channel 7 CTRL register                                                                                                                               |
| 0x1d4  | CH7_AL1_READ_ADDR        | Alias for channel 7 READ_ADDR register                                                                                                                          |
| 0x1d8  | CH7_AL1_WRITE_ADDR       | Alias for channel 7 WRITE_ADDR register                                                                                                                         |
| 0x1dc  | CH7_AL1_TRANS_COUNT_TRIG | Alias for channel 7 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x1e0  | CH7_AL2_CTRL             | Alias for channel 7 CTRL register                                                                                                                               |
| 0x1e4  | CH7_AL2_TRANS_COUNT      | Alias for channel 7 TRANS_COUNT register                                                                                                                        |
| 0x1e8  | CH7_AL2_READ_ADDR        | Alias for channel 7 READ_ADDR register                                                                                                                          |
| 0x1ec  | CH7_AL2_WRITE_ADDR_TRIG  | Alias for channel 7 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x1f0  | CH7_AL3_CTRL             | Alias for channel 7 CTRL register                                                                                                                               |
| 0x1f4  | CH7_AL3_WRITE_ADDR       | Alias for channel 7 WRITE_ADDR register                                                                                                                         |
| 0x1f8  | CH7_AL3_TRANS_COUNT      | Alias for channel 7 TRANS_COUNT register                                                                                                                        |
| 0x1fc  | CH7_AL3_READ_ADDR_TRIG   | Alias for channel 7 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x200  | CH8_READ_ADDR            | DMA Channel 8 Read Address pointer                                                                                                                              |
| 0x204  | CH8_WRITE_ADDR           | DMA Channel 8 Write Address pointer                                                                                                                             |
| 0x208  | CH8_TRANS_COUNT          | DMA Channel 8 Transfer Count                                                                                                                                    |
| 0x20c  | CH8_CTRL_TRIG            | DMA Channel 8 Control and Status                                                                                                                                |
| 0x210  | CH8_AL1_CTRL             | Alias for channel 8 CTRL register                                                                                                                               |
| 0x214  | CH8_AL1_READ_ADDR        | Alias for channel 8 READ_ADDR register                                                                                                                          |
| 0x218  | CH8_AL1_WRITE_ADDR       | Alias for channel 8 WRITE_ADDR register                                                                                                                         |

| Offset | Name                     | Info                                                                                                                                                            |
|--------|--------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x21c  | CH8_AL1_TRANS_COUNT_TRIG | Alias for channel 8 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x220  | CH8_AL2_CTRL             | Alias for channel 8 CTRL register                                                                                                                               |
| 0x224  | CH8_AL2_TRANS_COUNT      | Alias for channel 8 TRANS_COUNT register                                                                                                                        |
| 0x228  | CH8_AL2_READ_ADDR        | Alias for channel 8 READ_ADDR register                                                                                                                          |
| 0x22c  | CH8_AL2_WRITE_ADDR_TRIG  | Alias for channel 8 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x230  | CH8_AL3_CTRL             | Alias for channel 8 CTRL register                                                                                                                               |
| 0x234  | CH8_AL3_WRITE_ADDR       | Alias for channel 8 WRITE_ADDR register                                                                                                                         |
| 0x238  | CH8_AL3_TRANS_COUNT      | Alias for channel 8 TRANS_COUNT register                                                                                                                        |
| 0x23c  | CH8_AL3_READ_ADDR_TRIG   | Alias for channel 8 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x240  | CH9_READ_ADDR            | DMA Channel 9 Read Address pointer                                                                                                                              |
| 0x244  | CH9_WRITE_ADDR           | DMA Channel 9 Write Address pointer                                                                                                                             |
| 0x248  | CH9_TRANS_COUNT          | DMA Channel 9 Transfer Count                                                                                                                                    |
| 0x24c  | CH9_CTRL_TRIG            | DMA Channel 9 Control and Status                                                                                                                                |
| 0x250  | CH9_AL1_CTRL             | Alias for channel 9 CTRL register                                                                                                                               |
| 0x254  | CH9_AL1_READ_ADDR        | Alias for channel 9 READ_ADDR register                                                                                                                          |
| 0x258  | CH9_AL1_WRITE_ADDR       | Alias for channel 9 WRITE_ADDR register                                                                                                                         |
| 0x25c  | CH9_AL1_TRANS_COUNT_TRIG | Alias for channel 9 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x260  | CH9_AL2_CTRL             | Alias for channel 9 CTRL register                                                                                                                               |
| 0x264  | CH9_AL2_TRANS_COUNT      | Alias for channel 9 TRANS_COUNT register                                                                                                                        |
| 0x268  | CH9_AL2_READ_ADDR        | Alias for channel 9 READ_ADDR register                                                                                                                          |
| 0x26c  | CH9_AL2_WRITE_ADDR_TRIG  | Alias for channel 9 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x270  | CH9_AL3_CTRL             | Alias for channel 9 CTRL register                                                                                                                               |
| 0x274  | CH9_AL3_WRITE_ADDR       | Alias for channel 9 WRITE_ADDR register                                                                                                                         |
| 0x278  | CH9_AL3_TRANS_COUNT      | Alias for channel 9 TRANS_COUNT register                                                                                                                        |
| 0x27c  | CH9_AL3_READ_ADDR_TRIG   | Alias for channel 9 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x280  | CH10_READ_ADDR           | DMA Channel 10 Read Address pointer                                                                                                                             |
| 0x284  | CH10_WRITE_ADDR          | DMA Channel 10 Write Address pointer                                                                                                                            |
| 0x288  | CH10_TRANS_COUNT         | DMA Channel 10 Transfer Count                                                                                                                                   |

| Offset | Name                      | Info                                                                                                                                                             |
|--------|---------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x28c  | CH10_CTRL_TRIG            | DMA Channel 10 Control and Status                                                                                                                                |
| 0x290  | CH10_AL1_CTRL             | Alias for channel 10 CTRL register                                                                                                                               |
| 0x294  | CH10_AL1_READ_ADDR        | Alias for channel 10 READ_ADDR register                                                                                                                          |
| 0x298  | CH10_AL1_WRITE_ADDR       | Alias for channel 10 WRITE_ADDR register                                                                                                                         |
| 0x29c  | CH10_AL1_TRANS_COUNT_TRIG | Alias for channel 10 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x2a0  | CH10_AL2_CTRL             | Alias for channel 10 CTRL register                                                                                                                               |
| 0x2a4  | CH10_AL2_TRANS_COUNT      | Alias for channel 10 TRANS_COUNT register                                                                                                                        |
| 0x2a8  | CH10_AL2_READ_ADDR        | Alias for channel 10 READ_ADDR register                                                                                                                          |
| 0x2ac  | CH10_AL2_WRITE_ADDR_TRIG  | Alias for channel 10 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x2b0  | CH10_AL3_CTRL             | Alias for channel 10 CTRL register                                                                                                                               |
| 0x2b4  | CH10_AL3_WRITE_ADDR       | Alias for channel 10 WRITE_ADDR register                                                                                                                         |
| 0x2b8  | CH10_AL3_TRANS_COUNT      | Alias for channel 10 TRANS_COUNT register                                                                                                                        |
| 0x2bc  | CH10_AL3_READ_ADDR_TRIG   | Alias for channel 10 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x2c0  | CH11_READ_ADDR            | DMA Channel 11 Read Address pointer                                                                                                                              |
| 0x2c4  | CH11_WRITE_ADDR           | DMA Channel 11 Write Address pointer                                                                                                                             |
| 0x2c8  | CH11_TRANS_COUNT          | DMA Channel 11 Transfer Count                                                                                                                                    |
| 0x2cc  | CH11_CTRL_TRIG            | DMA Channel 11 Control and Status                                                                                                                                |
| 0x2d0  | CH11_AL1_CTRL             | Alias for channel 11 CTRL register                                                                                                                               |
| 0x2d4  | CH11_AL1_READ_ADDR        | Alias for channel 11 READ_ADDR register                                                                                                                          |
| 0x2d8  | CH11_AL1_WRITE_ADDR       | Alias for channel 11 WRITE_ADDR register                                                                                                                         |
| 0x2dc  | CH11_AL1_TRANS_COUNT_TRIG | Alias for channel 11 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x2e0  | CH11_AL2_CTRL             | Alias for channel 11 CTRL register                                                                                                                               |
| 0x2e4  | CH11_AL2_TRANS_COUNT      | Alias for channel 11 TRANS_COUNT register                                                                                                                        |
| 0x2e8  | CH11_AL2_READ_ADDR        | Alias for channel 11 READ_ADDR register                                                                                                                          |
| 0x2ec  | CH11_AL2_WRITE_ADDR_TRIG  | Alias for channel 11 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x2f0  | CH11_AL3_CTRL             | Alias for channel 11 CTRL register                                                                                                                               |
| 0x2f4  | CH11_AL3_WRITE_ADDR       | Alias for channel 11 WRITE_ADDR register                                                                                                                         |
| 0x2f8  | CH11_AL3_TRANS_COUNT      | Alias for channel 11 TRANS_COUNT register                                                                                                                        |

| Offset | Name                      | Info                                                                                                                                                             |
|--------|---------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x2fc  | CH11_AL3_READ_ADDR_TRIG   | Alias for channel 11 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x300  | CH12_READ_ADDR            | DMA Channel 12 Read Address pointer                                                                                                                              |
| 0x304  | CH12_WRITE_ADDR           | DMA Channel 12 Write Address pointer                                                                                                                             |
| 0x308  | CH12_TRANS_COUNT          | DMA Channel 12 Transfer Count                                                                                                                                    |
| 0x30c  | CH12_CTRL_TRIG            | DMA Channel 12 Control and Status                                                                                                                                |
| 0x310  | CH12_AL1_CTRL             | Alias for channel 12 CTRL register                                                                                                                               |
| 0x314  | CH12_AL1_READ_ADDR        | Alias for channel 12 READ_ADDR register                                                                                                                          |
| 0x318  | CH12_AL1_WRITE_ADDR       | Alias for channel 12 WRITE_ADDR register                                                                                                                         |
| 0x31c  | CH12_AL1_TRANS_COUNT_TRIG | Alias for channel 12 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x320  | CH12_AL2_CTRL             | Alias for channel 12 CTRL register                                                                                                                               |
| 0x324  | CH12_AL2_TRANS_COUNT      | Alias for channel 12 TRANS_COUNT register                                                                                                                        |
| 0x328  | CH12_AL2_READ_ADDR        | Alias for channel 12 READ_ADDR register                                                                                                                          |
| 0x32c  | CH12_AL2_WRITE_ADDR_TRIG  | Alias for channel 12 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x330  | CH12_AL3_CTRL             | Alias for channel 12 CTRL register                                                                                                                               |
| 0x334  | CH12_AL3_WRITE_ADDR       | Alias for channel 12 WRITE_ADDR register                                                                                                                         |
| 0x338  | CH12_AL3_TRANS_COUNT      | Alias for channel 12 TRANS_COUNT register                                                                                                                        |
| 0x33c  | CH12_AL3_READ_ADDR_TRIG   | Alias for channel 12 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x340  | CH13_READ_ADDR            | DMA Channel 13 Read Address pointer                                                                                                                              |
| 0x344  | CH13_WRITE_ADDR           | DMA Channel 13 Write Address pointer                                                                                                                             |
| 0x348  | CH13_TRANS_COUNT          | DMA Channel 13 Transfer Count                                                                                                                                    |
| 0x34c  | CH13_CTRL_TRIG            | DMA Channel 13 Control and Status                                                                                                                                |
| 0x350  | CH13_AL1_CTRL             | Alias for channel 13 CTRL register                                                                                                                               |
| 0x354  | CH13_AL1_READ_ADDR        | Alias for channel 13 READ_ADDR register                                                                                                                          |
| 0x358  | CH13_AL1_WRITE_ADDR       | Alias for channel 13 WRITE_ADDR register                                                                                                                         |
| 0x35c  | CH13_AL1_TRANS_COUNT_TRIG | Alias for channel 13 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x360  | CH13_AL2_CTRL             | Alias for channel 13 CTRL register                                                                                                                               |
| 0x364  | CH13_AL2_TRANS_COUNT      | Alias for channel 13 TRANS_COUNT register                                                                                                                        |
| 0x368  | CH13_AL2_READ_ADDR        | Alias for channel 13 READ_ADDR register                                                                                                                          |

| Offset | Name                      | Info                                                                                                                                                             |
|--------|---------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x36c  | CH13_AL2_WRITE_ADDR_TRIG  | Alias for channel 13 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x370  | CH13_AL3_CTRL             | Alias for channel 13 CTRL register                                                                                                                               |
| 0x374  | CH13_AL3_WRITE_ADDR       | Alias for channel 13 WRITE_ADDR register                                                                                                                         |
| 0x378  | CH13_AL3_TRANS_COUNT      | Alias for channel 13 TRANS_COUNT register                                                                                                                        |
| 0x37c  | CH13_AL3_READ_ADDR_TRIG   | Alias for channel 13 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x380  | CH14_READ_ADDR            | DMA Channel 14 Read Address pointer                                                                                                                              |
| 0x384  | CH14_WRITE_ADDR           | DMA Channel 14 Write Address pointer                                                                                                                             |
| 0x388  | CH14_TRANS_COUNT          | DMA Channel 14 Transfer Count                                                                                                                                    |
| 0x38c  | CH14_CTRL_TRIG            | DMA Channel 14 Control and Status                                                                                                                                |
| 0x390  | CH14_AL1_CTRL             | Alias for channel 14 CTRL register                                                                                                                               |
| 0x394  | CH14_AL1_READ_ADDR        | Alias for channel 14 READ_ADDR register                                                                                                                          |
| 0x398  | CH14_AL1_WRITE_ADDR       | Alias for channel 14 WRITE_ADDR register                                                                                                                         |
| 0x39c  | CH14_AL1_TRANS_COUNT_TRIG | Alias for channel 14 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x3a0  | CH14_AL2_CTRL             | Alias for channel 14 CTRL register                                                                                                                               |
| 0x3a4  | CH14_AL2_TRANS_COUNT      | Alias for channel 14 TRANS_COUNT register                                                                                                                        |
| 0x3a8  | CH14_AL2_READ_ADDR        | Alias for channel 14 READ_ADDR register                                                                                                                          |
| 0x3ac  | CH14_AL2_WRITE_ADDR_TRIG  | Alias for channel 14 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x3b0  | CH14_AL3_CTRL             | Alias for channel 14 CTRL register                                                                                                                               |
| 0x3b4  | CH14_AL3_WRITE_ADDR       | Alias for channel 14 WRITE_ADDR register                                                                                                                         |
| 0x3b8  | CH14_AL3_TRANS_COUNT      | Alias for channel 14 TRANS_COUNT register                                                                                                                        |
| 0x3bc  | CH14_AL3_READ_ADDR_TRIG   | Alias for channel 14 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x3c0  | CH15_READ_ADDR            | DMA Channel 15 Read Address pointer                                                                                                                              |
| 0x3c4  | CH15_WRITE_ADDR           | DMA Channel 15 Write Address pointer                                                                                                                             |
| 0x3c8  | CH15_TRANS_COUNT          | DMA Channel 15 Transfer Count                                                                                                                                    |
| 0x3cc  | CH15_CTRL_TRIG            | DMA Channel 15 Control and Status                                                                                                                                |
| 0x3d0  | CH15_AL1_CTRL             | Alias for channel 15 CTRL register                                                                                                                               |
| 0x3d4  | CH15_AL1_READ_ADDR        | Alias for channel 15 READ_ADDR register                                                                                                                          |
| 0x3d8  | CH15_AL1_WRITE_ADDR       | Alias for channel 15 WRITE_ADDR register                                                                                                                         |

| Offset | Name                      | Info                                                                                                                                                             |
|--------|---------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x3dc  | CH15_AL1_TRANS_COUNT_TRIG | Alias for channel 15 TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. |
| 0x3e0  | CH15_AL2_CTRL             | Alias for channel 15 CTRL register                                                                                                                               |
| 0x3e4  | CH15_AL2_TRANS_COUNT      | Alias for channel 15 TRANS_COUNT register                                                                                                                        |
| 0x3e8  | CH15_AL2_READ_ADDR        | Alias for channel 15 READ_ADDR register                                                                                                                          |
| 0x3ec  | CH15_AL2_WRITE_ADDR_TRIG  | Alias for channel 15 WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.  |
| 0x3f0  | CH15_AL3_CTRL             | Alias for channel 15 CTRL register                                                                                                                               |
| 0x3f4  | CH15_AL3_WRITE_ADDR       | Alias for channel 15 WRITE_ADDR register                                                                                                                         |
| 0x3f8  | CH15_AL3_TRANS_COUNT      | Alias for channel 15 TRANS_COUNT register                                                                                                                        |
| 0x3fc  | CH15_AL3_READ_ADDR_TRIG   | Alias for channel 15 READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel.   |
| 0x400  | INTR                      | Interrupt Status (raw)                                                                                                                                           |
| 0x404  | INTE0                     | Interrupt Enables for IRQ 0                                                                                                                                      |
| 0x408  | INTF0                     | Force Interrupts                                                                                                                                                 |
| 0x40c  | INTS0                     | Interrupt Status for IRQ 0                                                                                                                                       |
| 0x414  | INTE1                     | Interrupt Enables for IRQ 1                                                                                                                                      |
| 0x418  | INTF1                     | Force Interrupts                                                                                                                                                 |
| 0x41c  | INTS1                     | Interrupt Status for IRQ 1                                                                                                                                       |
| 0x424  | INTE2                     | Interrupt Enables for IRQ 2                                                                                                                                      |
| 0x428  | INTF2                     | Force Interrupts                                                                                                                                                 |
| 0x42c  | INTS2                     | Interrupt Status for IRQ 2                                                                                                                                       |
| 0x434  | INTE3                     | Interrupt Enables for IRQ 3                                                                                                                                      |
| 0x438  | INTF3                     | Force Interrupts                                                                                                                                                 |
| 0x43c  | INTS3                     | Interrupt Status for IRQ 3                                                                                                                                       |
| 0x440  | TIMER0                    | Pacing timer (generate periodic TREQs)                                                                                                                           |
| 0x444  | TIMER1                    | Pacing timer (generate periodic TREQs)                                                                                                                           |
| 0x448  | TIMER2                    | Pacing timer (generate periodic TREQs)                                                                                                                           |
| 0x44c  | TIMER3                    | Pacing timer (generate periodic TREQs)                                                                                                                           |
| 0x450  | MULTI_CHAN_TRIGGER        | Trigger one or more channels simultaneously                                                                                                                      |
| 0x454  | SNIFF_CTRL                | Sniffer Control                                                                                                                                                  |
| 0x458  | SNIFF_DATA                | Data accumulator for sniff hardware                                                                                                                              |
| 0x460  | FIFO_LEVELS               | Debug RAF, WAF, TDF levels                                                                                                                                       |
| 0x464  | CHAN_ABORT                | Abort an in-progress transfer sequence on one or more channels                                                                                                   |

| Offset | Name        | Info                                                                                                                                                                                             |
|--------|-------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x468  | N_CHANNELS  | The number of channels this DMA instance is equipped with.<br>This DMA supports up to 16 hardware channels, but can be<br>configured with as few as one, to minimise silicon area.               |
| 0x480  | SECCFG_CH0  | Security level configuration for channel 0.                                                                                                                                                      |
| 0x484  | SECCFG_CH1  | Security level configuration for channel 1.                                                                                                                                                      |
| 0x488  | SECCFG_CH2  | Security level configuration for channel 2.                                                                                                                                                      |
| 0x48c  | SECCFG_CH3  | Security level configuration for channel 3.                                                                                                                                                      |
| 0x490  | SECCFG_CH4  | Security level configuration for channel 4.                                                                                                                                                      |
| 0x494  | SECCFG_CH5  | Security level configuration for channel 5.                                                                                                                                                      |
| 0x498  | SECCFG_CH6  | Security level configuration for channel 6.                                                                                                                                                      |
| 0x49c  | SECCFG_CH7  | Security level configuration for channel 7.                                                                                                                                                      |
| 0x4a0  | SECCFG_CH8  | Security level configuration for channel 8.                                                                                                                                                      |
| 0x4a4  | SECCFG_CH9  | Security level configuration for channel 9.                                                                                                                                                      |
| 0x4a8  | SECCFG_CH10 | Security level configuration for channel 10.                                                                                                                                                     |
| 0x4ac  | SECCFG_CH11 | Security level configuration for channel 11.                                                                                                                                                     |
| 0x4b0  | SECCFG_CH12 | Security level configuration for channel 12.                                                                                                                                                     |
| 0x4b4  | SECCFG_CH13 | Security level configuration for channel 13.                                                                                                                                                     |
| 0x4b8  | SECCFG_CH14 | Security level configuration for channel 14.                                                                                                                                                     |
| 0x4bc  | SECCFG_CH15 | Security level configuration for channel 15.                                                                                                                                                     |
| 0x4c0  | SECCFG_IRQ0 | Security configuration for IRQ 0. Control whether the IRQ permits<br>configuration by Non-secure/Unprivileged contexts, and whether<br>it can observe Secure/Privileged channel interrupt flags. |
| 0x4c4  | SECCFG_IRQ1 | Security configuration for IRQ 1. Control whether the IRQ permits<br>configuration by Non-secure/Unprivileged contexts, and whether<br>it can observe Secure/Privileged channel interrupt flags. |
| 0x4c8  | SECCFG_IRQ2 | Security configuration for IRQ 2. Control whether the IRQ permits<br>configuration by Non-secure/Unprivileged contexts, and whether<br>it can observe Secure/Privileged channel interrupt flags. |
| 0x4cc  | SECCFG_IRQ3 | Security configuration for IRQ 3. Control whether the IRQ permits<br>configuration by Non-secure/Unprivileged contexts, and whether<br>it can observe Secure/Privileged channel interrupt flags. |
| 0x4d0  | SECCFG_MISC | Miscellaneous security configuration                                                                                                                                                             |
| 0x500  | MPU_CTRL    | Control register for DMA MPU. Accessible only from a Privileged<br>context.                                                                                                                      |
| 0x504  | MPU_BAR0    | Base address register for MPU region 0. Writable only from a<br>Secure, Privileged context.                                                                                                      |
| 0x508  | MPU_LAR0    | Limit address register for MPU region 0. Writable only from a<br>Secure, Privileged context, with the exception of the P bit.                                                                    |
| 0x50c  | MPU_BAR1    | Base address register for MPU region 1. Writable only from a<br>Secure, Privileged context.                                                                                                      |

| Offset | Name           | Info                                                                                                                                                                                                                               |
|--------|----------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x510  | MPU_LAR1       | Limit address register for MPU region 1. Writable only from a<br>Secure, Privileged context, with the exception of the P bit.                                                                                                      |
| 0x514  | MPU_BAR2       | Base address register for MPU region 2. Writable only from a<br>Secure, Privileged context.                                                                                                                                        |
| 0x518  | MPU_LAR2       | Limit address register for MPU region 2. Writable only from a<br>Secure, Privileged context, with the exception of the P bit.                                                                                                      |
| 0x51c  | MPU_BAR3       | Base address register for MPU region 3. Writable only from a<br>Secure, Privileged context.                                                                                                                                        |
| 0x520  | MPU_LAR3       | Limit address register for MPU region 3. Writable only from a<br>Secure, Privileged context, with the exception of the P bit.                                                                                                      |
| 0x524  | MPU_BAR4       | Base address register for MPU region 4. Writable only from a<br>Secure, Privileged context.                                                                                                                                        |
| 0x528  | MPU_LAR4       | Limit address register for MPU region 4. Writable only from a<br>Secure, Privileged context, with the exception of the P bit.                                                                                                      |
| 0x52c  | MPU_BAR5       | Base address register for MPU region 5. Writable only from a<br>Secure, Privileged context.                                                                                                                                        |
| 0x530  | MPU_LAR5       | Limit address register for MPU region 5. Writable only from a<br>Secure, Privileged context, with the exception of the P bit.                                                                                                      |
| 0x534  | MPU_BAR6       | Base address register for MPU region 6. Writable only from a<br>Secure, Privileged context.                                                                                                                                        |
| 0x538  | MPU_LAR6       | Limit address register for MPU region 6. Writable only from a<br>Secure, Privileged context, with the exception of the P bit.                                                                                                      |
| 0x53c  | MPU_BAR7       | Base address register for MPU region 7. Writable only from a<br>Secure, Privileged context.                                                                                                                                        |
| 0x540  | MPU_LAR7       | Limit address register for MPU region 7. Writable only from a<br>Secure, Privileged context, with the exception of the P bit.                                                                                                      |
| 0x800  | CH0_DBG_CTDREQ | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0x804  | CH0_DBG_TCR    | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0x840  | CH1_DBG_CTDREQ | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0x844  | CH1_DBG_TCR    | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0x880  | CH2_DBG_CTDREQ | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0x884  | CH2_DBG_TCR    | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |

| Offset | Name            | Info                                                                                                                                                                                                                               |
|--------|-----------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x8c0  | CH3_DBG_CTDREQ  | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0x8c4  | CH3_DBG_TCR     | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0x900  | CH4_DBG_CTDREQ  | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0x904  | CH4_DBG_TCR     | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0x940  | CH5_DBG_CTDREQ  | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0x944  | CH5_DBG_TCR     | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0x980  | CH6_DBG_CTDREQ  | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0x984  | CH6_DBG_TCR     | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0x9c0  | CH7_DBG_CTDREQ  | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0x9c4  | CH7_DBG_TCR     | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0xa00  | CH8_DBG_CTDREQ  | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0xa04  | CH8_DBG_TCR     | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0xa40  | CH9_DBG_CTDREQ  | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0xa44  | CH9_DBG_TCR     | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0xa80  | CH10_DBG_CTDREQ | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |

| Offset | Name            | Info                                                                                                                                                                                                                               |
|--------|-----------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0xa84  | CH10_DBG_TCR    | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0xac0  | CH11_DBG_CTDREQ | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0xac4  | CH11_DBG_TCR    | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0xb00  | CH12_DBG_CTDREQ | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0xb04  | CH12_DBG_TCR    | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0xb40  | CH13_DBG_CTDREQ | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0xb44  | CH13_DBG_TCR    | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0xb80  | CH14_DBG_CTDREQ | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0xb84  | CH14_DBG_TCR    | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |
| 0xbc0  | CH15_DBG_CTDREQ | Read: get channel DREQ counter (i.e. how many accesses the<br>DMA expects it can perform on the peripheral without<br>overflow/underflow. Write any value: clears the counter, and<br>cause channel to re-initiate DREQ handshake. |
| 0xbc4  | CH15_DBG_TCR    | Read to get channel TRANS_COUNT reload value, i.e. the length<br>of the next transfer                                                                                                                                              |

# <span id="page-1122-0"></span>**[DMA](#page-1109-1): CH0\_READ\_ADDR, CH1\_READ\_ADDR, …, CH14\_READ\_ADDR, CH15\_READ\_ADDR Registers**

**Offsets**: 0x000, 0x040, …, 0x380, 0x3c0

**Description**

DMA Channel *N* Read Address pointer

*Table 1147. CH0\_READ\_ADDR, CH1\_READ\_ADDR, …, CH14\_READ\_ADDR, CH15\_READ\_ADDR Registers*

| Bits | Description                                                                                                                          | Type | Reset      |
|------|--------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | This register updates automatically each time a read completes. The current<br>value is the next address to be read by this channel. | RW   | 0x00000000 |

# <span id="page-1122-1"></span>**[DMA](#page-1109-1): CH0\_WRITE\_ADDR, CH1\_WRITE\_ADDR, …, CH14\_WRITE\_ADDR, CH15\_WRITE\_ADDR Registers**

**Offsets**: 0x004, 0x044, …, 0x384, 0x3c4

DMA Channel *N* Write Address pointer

*Table 1148. CH0\_WRITE\_ADDR, CH1\_WRITE\_ADDR, …, CH14\_WRITE\_ADDR, CH15\_WRITE\_ADDR Registers*

| Bits | Description                                                                                                                              | Type | Reset      |
|------|------------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | This register updates automatically each time a write completes. The current<br>value is the next address to be written by this channel. | RW   | 0x00000000 |

# <span id="page-1123-0"></span>**[DMA](#page-1109-1): CH0\_TRANS\_COUNT, CH1\_TRANS\_COUNT, …, CH14\_TRANS\_COUNT, CH15\_TRANS\_COUNT Registers**

**Offsets**: 0x008, 0x048, …, 0x388, 0x3c8

#### **Description**

DMA Channel *N* Transfer Count

*Table 1149. CH0\_TRANS\_COUNT, CH1\_TRANS\_COUNT,*

*…, CH14\_TRANS\_COUNT, CH15\_TRANS\_COUNT Registers*

| Bits  | Description                                                                                                                                                                                                                                                                                                            | Type | Reset     |
|-------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-----------|
| 31:28 | MODE: When MODE is 0x0, the transfer count decrements with each transfer<br>until 0, and then the channel triggers the next channel indicated by<br>CTRL_CHAIN_TO.                                                                                                                                                     | RW   | 0x0       |
|       | When MODE is 0x1, the transfer count decrements with each transfer until 0,<br>and then the channel re-triggers itself, in addition to the trigger indicated by<br>CTRL_CHAIN_TO. This is useful for e.g. an endless ring-buffer DMA with<br>periodic interrupts.                                                      |      |           |
|       | When MODE is 0xf, the transfer count does not decrement. The DMA channel<br>performs an endless sequence of transfers, never triggering other channels or<br>raising interrupts, until an ABORT is raised.                                                                                                             |      |           |
|       | All other values are reserved.                                                                                                                                                                                                                                                                                         |      |           |
|       | Enumerated values:                                                                                                                                                                                                                                                                                                     |      |           |
|       | 0x0 → NORMAL                                                                                                                                                                                                                                                                                                           |      |           |
|       | 0x1 → TRIGGER_SELF                                                                                                                                                                                                                                                                                                     |      |           |
|       | 0xf → ENDLESS                                                                                                                                                                                                                                                                                                          |      |           |
| 27:0  | COUNT: 28-bit transfer count (256 million transfers maximum).                                                                                                                                                                                                                                                          | RW   | 0x0000000 |
|       | Program the number of bus transfers a channel will perform before halting.<br>Note that, if transfers are larger than one byte in size, this is not equal to the<br>number of bytes transferred (see CTRL_DATA_SIZE).                                                                                                  |      |           |
|       | When the channel is active, reading this register shows the number of<br>transfers remaining, updating automatically each time a write transfer<br>completes.                                                                                                                                                          |      |           |
|       | Writing this register sets the RELOAD value for the transfer counter. Each time<br>this channel is triggered, the RELOAD value is copied into the live transfer<br>counter. The channel can be started multiple times, and will perform the same<br>number of transfers each time, as programmed by most recent write. |      |           |
|       | The RELOAD value can be observed at CHx_DBG_TCR. If TRANS_COUNT is<br>used as a trigger, the written value is used immediately as the length of the<br>new transfer sequence, as well as being written to RELOAD.                                                                                                      |      |           |

# <span id="page-1124-0"></span>**[DMA](#page-1109-1): CH0\_CTRL\_TRIG, CH1\_CTRL\_TRIG, …, CH14\_CTRL\_TRIG, CH15\_CTRL\_TRIG Registers**

**Offsets**: 0x00c, 0x04c, …, 0x38c, 0x3cc

#### **Description**

DMA Channel *N* Control and Status

*Table 1150. CH0\_CTRL\_TRIG, CH1\_CTRL\_TRIG, …, CH14\_CTRL\_TRIG, CH15\_CTRL\_TRIG Registers*

| Bits  | Description                                                                                                                                                                                                                                                                                                      | Type | Reset |
|-------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31    | AHB_ERROR: Logical OR of the READ_ERROR and WRITE_ERROR flags. The<br>channel halts when it encounters any bus error, and always raises its channel<br>IRQ flag.                                                                                                                                                 | RO   | 0x0   |
| 30    | READ_ERROR: If 1, the channel received a read bus error. Write one to clear.<br>READ_ADDR shows the approximate address where the bus error was<br>encountered (will not be earlier, or more than 3 transfers later)                                                                                             | WC   | 0x0   |
| 29    | WRITE_ERROR: If 1, the channel received a write bus error. Write one to clear.<br>WRITE_ADDR shows the approximate address where the bus error was<br>encountered (will not be earlier, or more than 5 transfers later)                                                                                          | WC   | 0x0   |
| 28:27 | Reserved.                                                                                                                                                                                                                                                                                                        | -    | -     |
| 26    | BUSY: This flag goes high when the channel starts a new transfer sequence,<br>and low when the last transfer of that sequence completes. Clearing EN while<br>BUSY is high pauses the channel, and BUSY will stay high while paused.<br>To terminate a sequence early (and clear the BUSY flag), see CHAN_ABORT. | RO   | 0x0   |
| 25    | SNIFF_EN: If 1, this channel's data transfers are visible to the sniff hardware,<br>and each transfer will advance the state of the checksum. This only applies if<br>the sniff hardware is enabled, and has this channel selected.                                                                              | RW   | 0x0   |
|       | This allows checksum to be enabled or disabled on a per-control- block basis.                                                                                                                                                                                                                                    |      |       |
| 24    | BSWAP: Apply byte-swap transformation to DMA data.<br>For byte data, this has no effect. For halfword data, the two bytes of each<br>halfword are swapped. For word data, the four bytes of each word are<br>swapped to reverse order.                                                                           | RW   | 0x0   |
| 23    | IRQ_QUIET: In QUIET mode, the channel does not generate IRQs at the end of<br>every transfer block. Instead, an IRQ is raised when NULL is written to a trigger<br>register, indicating the end of a control block chain.                                                                                        | RW   | 0x0   |
|       | This reduces the number of interrupts to be serviced by the CPU when<br>transferring a DMA chain of many small control blocks.                                                                                                                                                                                   |      |       |
| 22:17 | TREQ_SEL: Select a Transfer Request signal.<br>The channel uses the transfer request signal to pace its data transfer rate.<br>Sources for TREQ signals are internal (TIMERS) or external (DREQ, a Data<br>Request from the system).<br>0x0 to 0x3a → select DREQ n as TREQ                                      | RW   | 0x00  |
|       | Enumerated values:                                                                                                                                                                                                                                                                                               |      |       |
|       | 0x3b → TIMER0: Select Timer 0 as TREQ                                                                                                                                                                                                                                                                            |      |       |
|       | 0x3c → TIMER1: Select Timer 1 as TREQ                                                                                                                                                                                                                                                                            |      |       |
|       | 0x3d → TIMER2: Select Timer 2 as TREQ (Optional)                                                                                                                                                                                                                                                                 |      |       |
|       | 0x3e → TIMER3: Select Timer 3 as TREQ (Optional)                                                                                                                                                                                                                                                                 |      |       |

| Bits  | Description                                                                                                                                                                                                                                    | Type | Reset |
|-------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
|       | 0x3f → PERMANENT: Permanent request, for unpaced transfers.                                                                                                                                                                                    |      |       |
| 16:13 | CHAIN_TO: When this channel completes, it will trigger the channel indicated<br>by CHAIN_TO. Disable by setting CHAIN_TO = (this channel).                                                                                                     | RW   | 0x0   |
|       | Note this field resets to 0, so channels 1 and above will chain to channel 0 by<br>default. Set this field to avoid this behaviour.                                                                                                            |      |       |
| 12    | RING_SEL: Select whether RING_SIZE applies to read or write addresses.<br>If 0, read addresses are wrapped on a (1 << RING_SIZE) boundary. If 1, write<br>addresses are wrapped.                                                               | RW   | 0x0   |
| 11:8  | RING_SIZE: Size of address wrap region. If 0, don't wrap. For values n > 0, only<br>the lower n bits of the address will change. This wraps the address on a (1 <<<br>n) byte boundary, facilitating access to naturally-aligned ring buffers. | RW   | 0x0   |
|       | Ring sizes between 2 and 32768 bytes are possible. This can apply to either<br>read or write addresses, based on value of RING_SEL.                                                                                                            |      |       |
|       | Enumerated values:                                                                                                                                                                                                                             |      |       |
|       | 0x0 → RING_NONE                                                                                                                                                                                                                                |      |       |
| 7     | INCR_WRITE_REV: If 1, and INCR_WRITE is 1, the write address is<br>decremented rather than incremented with each transfer.                                                                                                                     | RW   | 0x0   |
|       | If 1, and INCR_WRITE is 0, this otherwise-unused combination causes the<br>write address to be incremented by twice the transfer size, i.e. skipping over<br>alternate addresses.                                                              |      |       |
| 6     | INCR_WRITE: If 1, the write address increments with each transfer. If 0, each<br>write is directed to the same, initial address.                                                                                                               | RW   | 0x0   |
|       | Generally this should be disabled for memory-to-peripheral transfers.                                                                                                                                                                          |      |       |
| 5     | INCR_READ_REV: If 1, and INCR_READ is 1, the read address is decremented<br>rather than incremented with each transfer.                                                                                                                        | RW   | 0x0   |
|       | If 1, and INCR_READ is 0, this otherwise-unused combination causes the read<br>address to be incremented by twice the transfer size, i.e. skipping over<br>alternate addresses.                                                                |      |       |
| 4     | INCR_READ: If 1, the read address increments with each transfer. If 0, each<br>read is directed to the same, initial address.                                                                                                                  | RW   | 0x0   |
|       | Generally this should be disabled for peripheral-to-memory transfers.                                                                                                                                                                          |      |       |
| 3:2   | DATA_SIZE: Set the size of each bus transfer (byte/halfword/word).<br>READ_ADDR and WRITE_ADDR advance by this amount (1/2/4 bytes) with<br>each transfer.                                                                                     | RW   | 0x0   |
|       | Enumerated values:                                                                                                                                                                                                                             |      |       |
|       | 0x0 → SIZE_BYTE                                                                                                                                                                                                                                |      |       |
|       | 0x1 → SIZE_HALFWORD                                                                                                                                                                                                                            |      |       |
|       | 0x2 → SIZE_WORD                                                                                                                                                                                                                                |      |       |

| Bits | Description                                                                                                                                                                                                                                                                                                       | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 1    | HIGH_PRIORITY: HIGH_PRIORITY gives a channel preferential treatment in<br>issue scheduling: in each scheduling round, all high priority channels are<br>considered first, and then only a single low priority channel, before returning to<br>the high priority channels.                                         | RW   | 0x0   |
|      | This only affects the order in which the DMA schedules channels. The DMA's<br>bus priority is not changed. If the DMA is not saturated then a low priority<br>channel will see no loss of throughput.                                                                                                             |      |       |
| 0    | EN: DMA Channel Enable.<br>When 1, the channel will respond to triggering events, which will cause it to<br>become BUSY and start transferring data. When 0, the channel will ignore<br>triggers, stop issuing transfers, and pause the current transfer sequence (i.e.<br>BUSY will remain high if already high) | RW   | 0x0   |

# <span id="page-1126-0"></span>**[DMA](#page-1109-1): CH0\_AL1\_CTRL, CH1\_AL1\_CTRL, …, CH14\_AL1\_CTRL, CH15\_AL1\_CTRL Registers**

**Offsets**: 0x010, 0x050, …, 0x390, 0x3d0

*Table 1151. CH0\_AL1\_CTRL, CH1\_AL1\_CTRL, …, CH14\_AL1\_CTRL, CH15\_AL1\_CTRL Registers*

| Bits | Description                       | Type | Reset |
|------|-----------------------------------|------|-------|
| 31:0 | Alias for channel N CTRL register | RW   | -     |

# <span id="page-1126-1"></span>**[DMA](#page-1109-1): CH0\_AL1\_READ\_ADDR, CH1\_AL1\_READ\_ADDR, …, CH14\_AL1\_READ\_ADDR, CH15\_AL1\_READ\_ADDR Registers**

**Offsets**: 0x014, 0x054, …, 0x394, 0x3d4

*Table 1152. CH0\_AL1\_READ\_ADDR*

*CH1\_AL1\_READ\_ADDR*

*, …, CH14\_AL1\_READ\_ADD*

*R, CH15\_AL1\_READ\_ADD R Registers*

**Bits Description Type Reset** 31:0 Alias for channel *N* READ\_ADDR register RW -

<span id="page-1126-2"></span>**[DMA](#page-1109-1): CH0\_AL1\_WRITE\_ADDR, CH1\_AL1\_WRITE\_ADDR, …, CH14\_AL1\_WRITE\_ADDR, CH15\_AL1\_WRITE\_ADDR Registers**

**Bits Description Type Reset** 31:0 Alias for channel *N* WRITE\_ADDR register RW -

**Offsets**: 0x018, 0x058, …, 0x398, 0x3d8

*Table 1153. CH0\_AL1\_WRITE\_ADD R,*

*CH1\_AL1\_WRITE\_ADD R, …,*

*CH14\_AL1\_WRITE\_AD DR, CH15\_AL1\_WRITE\_AD*

*DR Registers*

<span id="page-1126-3"></span>

| DMA: | CH0_AL1_TRANS_COUNT_TRIG, | CH1_AL1_TRANS_COUNT_TRIG,                                      | …, |
|------|---------------------------|----------------------------------------------------------------|----|
|      |                           | CH14_AL1_TRANS_COUNT_TRIG, CH15_AL1_TRANS_COUNT_TRIG Registers |    |

**Offsets**: 0x01c, 0x05c, …, 0x39c, 0x3dc

*Table 1154. CH0\_AL1\_TRANS\_COU NT\_TRIG, CH1\_AL1\_TRANS\_COU NT\_TRIG, …, CH14\_AL1\_TRANS\_CO UNT\_TRIG, CH15\_AL1\_TRANS\_CO UNT\_TRIG Registers*

| Bits | Description                                                                                                                                                     | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:0 | Alias for channel N TRANS_COUNT register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. | RW   | -     |

# <span id="page-1126-4"></span>**[DMA](#page-1109-1): CH0\_AL2\_CTRL, CH1\_AL2\_CTRL, …, CH14\_AL2\_CTRL, CH15\_AL2\_CTRL Registers**

**Offsets**: 0x020, 0x060, …, 0x3a0, 0x3e0

*Table 1155. CH0\_AL2\_CTRL, CH1\_AL2\_CTRL, …, CH14\_AL2\_CTRL, CH15\_AL2\_CTRL Registers*

| Bits | Description                       | Type | Reset |
|------|-----------------------------------|------|-------|
| 31:0 | Alias for channel N CTRL register | RW   | -     |

# <span id="page-1127-0"></span>**[DMA](#page-1109-1): CH0\_AL2\_TRANS\_COUNT, CH1\_AL2\_TRANS\_COUNT, …, CH14\_AL2\_TRANS\_COUNT, CH15\_AL2\_TRANS\_COUNT Registers**

**Offsets**: 0x024, 0x064, …, 0x3a4, 0x3e4

*Table 1156. CH0\_AL2\_TRANS\_COU*

*NT, CH1\_AL2\_TRANS\_COU NT, …,*

*CH14\_AL2\_TRANS\_CO UNT,*

*CH15\_AL2\_TRANS\_CO UNT Registers*

| Bits | Description                              | Type | Reset |
|------|------------------------------------------|------|-------|
| 31:0 | Alias for channel N TRANS_COUNT register | RW   | -     |

<span id="page-1127-1"></span>**[DMA](#page-1109-1): CH0\_AL2\_READ\_ADDR, CH1\_AL2\_READ\_ADDR, …, CH14\_AL2\_READ\_ADDR, CH15\_AL2\_READ\_ADDR Registers**

**Offsets**: 0x028, 0x068, …, 0x3a8, 0x3e8

*Table 1157. CH0\_AL2\_READ\_ADDR*

*CH1\_AL2\_READ\_ADDR , …,*

*CH14\_AL2\_READ\_ADD R,*

*CH15\_AL2\_READ\_ADD R Registers*

| Bits | Description                            | Type | Reset |
|------|----------------------------------------|------|-------|
| 31:0 | Alias for channel N READ_ADDR register | RW   | -     |

# <span id="page-1127-2"></span>**[DMA](#page-1109-1): CH0\_AL2\_WRITE\_ADDR\_TRIG, CH1\_AL2\_WRITE\_ADDR\_TRIG, …, CH14\_AL2\_WRITE\_ADDR\_TRIG, CH15\_AL2\_WRITE\_ADDR\_TRIG Registers**

**Offsets**: 0x02c, 0x06c, …, 0x3ac, 0x3ec

*Table 1158. CH0\_AL2\_WRITE\_ADD R\_TRIG, CH1\_AL2\_WRITE\_ADD R\_TRIG, …, CH14\_AL2\_WRITE\_AD DR\_TRIG, CH15\_AL2\_WRITE\_AD DR\_TRIG Registers*

| Bits | Description                                                                                                                                                    | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:0 | Alias for channel N WRITE_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. | RW   | -     |

# <span id="page-1127-3"></span>**[DMA](#page-1109-1): CH0\_AL3\_CTRL, CH1\_AL3\_CTRL, …, CH14\_AL3\_CTRL, CH15\_AL3\_CTRL Registers**

**Offsets**: 0x030, 0x070, …, 0x3b0, 0x3f0

*Table 1159. CH0\_AL3\_CTRL, CH1\_AL3\_CTRL, …, CH14\_AL3\_CTRL, CH15\_AL3\_CTRL Registers*

| Bits | Description                       | Type | Reset |
|------|-----------------------------------|------|-------|
| 31:0 | Alias for channel N CTRL register | RW   | -     |

# <span id="page-1127-4"></span>**[DMA](#page-1109-1): CH0\_AL3\_WRITE\_ADDR, CH1\_AL3\_WRITE\_ADDR, …, CH14\_AL3\_WRITE\_ADDR, CH15\_AL3\_WRITE\_ADDR Registers**

**Offsets**: 0x034, 0x074, …, 0x3b4, 0x3f4

*Table 1160. CH0\_AL3\_WRITE\_ADD R, CH1\_AL3\_WRITE\_ADD*

*R, …, CH14\_AL3\_WRITE\_AD*

*DR, CH15\_AL3\_WRITE\_AD DR Registers*

| Bits | Description                             | Type | Reset |
|------|-----------------------------------------|------|-------|
| 31:0 | Alias for channel N WRITE_ADDR register | RW   | -     |

<span id="page-1127-5"></span>**[DMA](#page-1109-1): CH0\_AL3\_TRANS\_COUNT, CH1\_AL3\_TRANS\_COUNT, …, CH14\_AL3\_TRANS\_COUNT, CH15\_AL3\_TRANS\_COUNT Registers**

**Offsets**: 0x038, 0x078, …, 0x3b8, 0x3f8

*Table 1161. CH0\_AL3\_TRANS\_COU NT, CH1\_AL3\_TRANS\_COU*

*NT, …, CH14\_AL3\_TRANS\_CO*

*UNT, CH15\_AL3\_TRANS\_CO UNT Registers*

| Bits | Description                              | Type | Reset |
|------|------------------------------------------|------|-------|
| 31:0 | Alias for channel N TRANS_COUNT register | RW   | -     |

# <span id="page-1128-2"></span>**[DMA](#page-1109-1): CH0\_AL3\_READ\_ADDR\_TRIG, CH1\_AL3\_READ\_ADDR\_TRIG, …, CH14\_AL3\_READ\_ADDR\_TRIG, CH15\_AL3\_READ\_ADDR\_TRIG Registers**

**Offsets**: 0x03c, 0x07c, …, 0x3bc, 0x3fc

*Table 1162. CH0\_AL3\_READ\_ADDR \_TRIG, CH1\_AL3\_READ\_ADDR \_TRIG, …, CH14\_AL3\_READ\_ADD R\_TRIG,*

*CH15\_AL3\_READ\_ADD R\_TRIG Registers*

| Bits | Description                                                                                                                                                   | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:0 | Alias for channel N READ_ADDR register<br>This is a trigger register (0xc). Writing a nonzero value will<br>reload the channel counter and start the channel. | RW   | -     |

# <span id="page-1128-1"></span>**[DMA](#page-1109-1): INTR Register**

**Offset**: 0x400 **Description**

Interrupt Status (raw)

*Table 1163. INTR Register*

| Bits  | Description                                                                                                                                                                                                                | Type | Reset  |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                                                                  | -    | -      |
| 15:0  | Raw interrupt status for DMA Channels 015. Bit n corresponds to channel n.<br>Ignores any masking or forcing. Channel interrupts can be cleared by writing a<br>bit mask to INTR or INTS0/1/2/3.                           | WC   | 0x0000 |
|       | Channel interrupts can be routed to either of four system-level IRQs based on<br>INTE0, INTE1, INTE2 and INTE3.                                                                                                            |      |        |
|       | The multiple system-level interrupts might be used to allow NVIC IRQ<br>preemption for more time-critical channels, to spread IRQ load across<br>different cores, or to target IRQs to different security domains.         |      |        |
|       | It is also valid to ignore the multiple IRQs, and just use INTE0/INTS0/IRQ 0.                                                                                                                                              |      |        |
|       | If this register is accessed at a security/privilege level less than that of a given<br>channel (as defined by that channel's SECCFG_CHx register), then that<br>channel's interrupt status will read as 0, ignore writes. |      |        |

#### <span id="page-1128-0"></span>**[DMA](#page-1109-1): INTE0 Register**

**Offset**: 0x404 **Description**

Interrupt Enables for IRQ 0

*Table 1164. INTE0 Register*

| Bits  | Description                                                                                                                                                          | Type | Reset  |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                            | -    | -      |
| 15:0  | Set bit n to pass interrupts from channel n to DMA IRQ 0.                                                                                                            | RW   | 0x0000 |
|       | Note this bit has no effect if the channel security/privilege level, defined by<br>SECCFG_CHx, is greater than the IRQ security/privilege defined by<br>SECCFG_IRQ0. |      |        |

# <span id="page-1129-0"></span>**[DMA](#page-1109-1): INTF0 Register**

**Offset**: 0x408

**Description**

Force Interrupts

*Table 1165. INTF0 Register*

| Bits  | Description                                                                                                  | Type | Reset  |
|-------|--------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                    | -    | -      |
| 15:0  | Write 1s to force the corresponding bits in INTS0. The interrupt remains<br>asserted until INTF0 is cleared. | RW   | 0x0000 |

# <span id="page-1129-1"></span>**[DMA](#page-1109-1): INTS0 Register**

**Offset**: 0x40c

**Description**

Interrupt Status for IRQ 0

*Table 1166. INTS0 Register*

| Bits  | Description                                                                                                                                                       | Type | Reset  |
|-------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                         | -    | -      |
| 15:0  | Indicates active channel interrupt requests which are currently causing IRQ 0<br>to be asserted.<br>Channel interrupts can be cleared by writing a bit mask here. | WC   | 0x0000 |
|       | Channels with a security/privilege (SECCFG_CHx) greater SECCFG_IRQ0) read<br>as 0 in this register, and ignore writes.                                            |      |        |

#### <span id="page-1129-2"></span>**[DMA](#page-1109-1): INTE1 Register**

**Offset**: 0x414

**Description**

Interrupt Enables for IRQ 1

*Table 1167. INTE1 Register*

| Bits  | Description                                                                                                                                                          | Type | Reset  |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                            | -    | -      |
| 15:0  | Set bit n to pass interrupts from channel n to DMA IRQ 1.                                                                                                            | RW   | 0x0000 |
|       | Note this bit has no effect if the channel security/privilege level, defined by<br>SECCFG_CHx, is greater than the IRQ security/privilege defined by<br>SECCFG_IRQ1. |      |        |

# <span id="page-1130-0"></span>**[DMA](#page-1109-1): INTF1 Register**

**Offset**: 0x418

**Description**

Force Interrupts

*Table 1168. INTF1 Register*

| Bits  | Description                                                                                                  | Type | Reset  |
|-------|--------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                    | -    | -      |
| 15:0  | Write 1s to force the corresponding bits in INTS1. The interrupt remains<br>asserted until INTF1 is cleared. | RW   | 0x0000 |

# <span id="page-1130-1"></span>**[DMA](#page-1109-1): INTS1 Register**

**Offset**: 0x41c

#### **Description**

Interrupt Status for IRQ 1

*Table 1169. INTS1 Register*

| Bits  | Description                                                                                                                                                                                                                                                                                 | Type | Reset  |
|-------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                                                                                                                                   | -    | -      |
| 15:0  | Indicates active channel interrupt requests which are currently causing IRQ 1<br>to be asserted.<br>Channel interrupts can be cleared by writing a bit mask here.<br>Channels with a security/privilege (SECCFG_CHx) greater SECCFG_IRQ1) read<br>as 0 in this register, and ignore writes. | WC   | 0x0000 |

#### <span id="page-1130-2"></span>**[DMA](#page-1109-1): INTE2 Register**

**Offset**: 0x424

#### **Description**

Interrupt Enables for IRQ 2

*Table 1170. INTE2 Register*

| Bits  | Description                                                                                                                                                          | Type | Reset  |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                            | -    | -      |
| 15:0  | Set bit n to pass interrupts from channel n to DMA IRQ 2.                                                                                                            | RW   | 0x0000 |
|       | Note this bit has no effect if the channel security/privilege level, defined by<br>SECCFG_CHx, is greater than the IRQ security/privilege defined by<br>SECCFG_IRQ2. |      |        |

# <span id="page-1131-1"></span>**[DMA](#page-1109-1): INTF2 Register**

**Offset**: 0x428

#### **Description**

Force Interrupts

*Table 1171. INTF2 Register*

| Bits  | Description                                                                                                  | Type | Reset  |
|-------|--------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                    | -    | -      |
| 15:0  | Write 1s to force the corresponding bits in INTS2. The interrupt remains<br>asserted until INTF2 is cleared. | RW   | 0x0000 |

# <span id="page-1131-2"></span>**[DMA](#page-1109-1): INTS2 Register**

**Offset**: 0x42c

#### **Description**

Interrupt Status for IRQ 2

*Table 1172. INTS2 Register*

| Bits  | Description                                                                                                                                                                                                                                                                                 | Type | Reset  |
|-------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                                                                                                                                   | -    | -      |
| 15:0  | Indicates active channel interrupt requests which are currently causing IRQ 2<br>to be asserted.<br>Channel interrupts can be cleared by writing a bit mask here.<br>Channels with a security/privilege (SECCFG_CHx) greater SECCFG_IRQ2) read<br>as 0 in this register, and ignore writes. | WC   | 0x0000 |

#### <span id="page-1131-0"></span>**[DMA](#page-1109-1): INTE3 Register**

**Offset**: 0x434

#### **Description**

Interrupt Enables for IRQ 3

*Table 1173. INTE3 Register*

| Bits  | Description                                                                                                                                                          | Type | Reset  |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                            | -    | -      |
| 15:0  | Set bit n to pass interrupts from channel n to DMA IRQ 3.                                                                                                            | RW   | 0x0000 |
|       | Note this bit has no effect if the channel security/privilege level, defined by<br>SECCFG_CHx, is greater than the IRQ security/privilege defined by<br>SECCFG_IRQ3. |      |        |

# <span id="page-1132-2"></span>**[DMA](#page-1109-1): INTF3 Register**

**Offset**: 0x438 **Description**

Force Interrupts

*Table 1174. INTF3 Register*

| Bits  | Description                                                                                                  | Type | Reset  |
|-------|--------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                    | -    | -      |
| 15:0  | Write 1s to force the corresponding bits in INTS3. The interrupt remains<br>asserted until INTF3 is cleared. | RW   | 0x0000 |

# <span id="page-1132-3"></span>**[DMA](#page-1109-1): INTS3 Register**

**Offset**: 0x43c

**Description**

Interrupt Status for IRQ 3

*Table 1175. INTS3 Register*

| Bits  | Description                                                                                                                                                       | Type | Reset  |
|-------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                         | -    | -      |
| 15:0  | Indicates active channel interrupt requests which are currently causing IRQ 3<br>to be asserted.<br>Channel interrupts can be cleared by writing a bit mask here. | WC   | 0x0000 |
|       | Channels with a security/privilege (SECCFG_CHx) greater SECCFG_IRQ3) read<br>as 0 in this register, and ignore writes.                                            |      |        |

#### <span id="page-1132-1"></span>**[DMA](#page-1109-1): TIMER0, TIMER1, TIMER2, TIMER3 Registers**

**Offsets**: 0x440, 0x444, 0x448, 0x44c

#### **Description**

Pacing (X/Y) fractional timer

The pacing timer produces TREQ assertions at a rate set by ((X/Y) \* sys\_clk). This equation is evaluated every sys\_clk cycles and therefore can only generate TREQs at a rate of 1 per sys\_clk (i.e. permanent TREQ) or less.

*Table 1176. TIMER0, TIMER1, TIMER2, TIMER3 Registers*

| Bits  | Description                                                                     | Type | Reset  |
|-------|---------------------------------------------------------------------------------|------|--------|
| 31:16 | X: Pacing Timer Dividend. Specifies the X value for the (X/Y) fractional timer. | RW   | 0x0000 |
| 15:0  | Y: Pacing Timer Divisor. Specifies the Y value for the (X/Y) fractional timer.  | RW   | 0x0000 |

#### <span id="page-1132-0"></span>**[DMA](#page-1109-1): MULTI\_CHAN\_TRIGGER Register**

**Offset**: 0x450

Trigger one or more channels simultaneously

*Table 1177. MULTI\_CHAN\_TRIGGE R Register*

| Bits  | Description                                                                                                                                                                                                                      | Type | Reset  |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                                                                        | -    | -      |
| 15:0  | Each bit in this register corresponds to a DMA channel. Writing a 1 to the<br>relevant bit is the same as writing to that channel's trigger register; the<br>channel will start if it is currently enabled and not already busy. | SC   | 0x0000 |

# <span id="page-1133-0"></span>**[DMA](#page-1109-1): SNIFF\_CTRL Register**

**Offset**: 0x454

**Description**

Sniffer Control

*Table 1178. SNIFF\_CTRL Register*

| Bits  | Description                                                                                                                                                                                                                       | Type | Reset |  |  |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|--|--|
| 31:12 | Reserved.                                                                                                                                                                                                                         | -    | -     |  |  |
| 11    | OUT_INV: If set, the result appears inverted (bitwise complement) when read.<br>This does not affect the way the checksum is calculated; the result is<br>transformed on-the-fly between the result register and the bus.         | RW   | 0x0   |  |  |
| 10    | OUT_REV: If set, the result appears bit-reversed when read. This does not<br>affect the way the checksum is calculated; the result is transformed on-the-fly<br>between the result register and the bus.                          | RW   | 0x0   |  |  |
| 9     | BSWAP: Locally perform a byte reverse on the sniffed data, before feeding into<br>checksum.                                                                                                                                       | RW   | 0x0   |  |  |
|       | Note that the sniff hardware is downstream of the DMA channel byteswap<br>performed in the read master: if channel CTRL_BSWAP and<br>SNIFF_CTRL_BSWAP are both enabled, their effects cancel from the sniffer's<br>point of view. |      |       |  |  |
| 8:5   | CALC                                                                                                                                                                                                                              | RW   | 0x0   |  |  |
|       | Enumerated values:                                                                                                                                                                                                                |      |       |  |  |
|       | 0x0 → CRC32: Calculate a CRC-32 (IEEE802.3 polynomial)                                                                                                                                                                            |      |       |  |  |
|       | 0x1 → CRC32R: Calculate a CRC-32 (IEEE802.3 polynomial) with bit reversed<br>data                                                                                                                                                 |      |       |  |  |
|       | 0x2 → CRC16: Calculate a CRC-16-CCITT                                                                                                                                                                                             |      |       |  |  |
|       | 0x3 → CRC16R: Calculate a CRC-16-CCITT with bit reversed data                                                                                                                                                                     |      |       |  |  |
|       | 0xe → EVEN: XOR reduction over all data. == 1 if the total 1 population count<br>is odd.                                                                                                                                          |      |       |  |  |
|       | 0xf → SUM: Calculate a simple 32-bit checksum (addition with a 32 bit<br>accumulator)                                                                                                                                             |      |       |  |  |
| 4:1   | DMACH: DMA channel for Sniffer to observe                                                                                                                                                                                         | RW   | 0x0   |  |  |
| 0     | EN: Enable sniffer                                                                                                                                                                                                                | RW   | 0x0   |  |  |

## <span id="page-1133-1"></span>**[DMA](#page-1109-1): SNIFF\_DATA Register**

**Offset**: 0x458

Data accumulator for sniff hardware

*Table 1179. SNIFF\_DATA Register*

| Bits | Description                                                                                                                                                                                                                                                                                         | Type | Reset      |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | Write an initial seed value here before starting a DMA transfer on the channel<br>indicated by SNIFF_CTRL_DMACH. The hardware will update this register each<br>time it observes a read from the indicated channel. Once the channel<br>completes, the final result can be read from this register. | RW   | 0x00000000 |

# <span id="page-1134-2"></span>**[DMA](#page-1109-1): FIFO\_LEVELS Register**

**Offset**: 0x460

#### **Description**

Debug RAF, WAF, TDF levels

*Table 1180. FIFO\_LEVELS Register*

| Bits  | Description                                    | Type | Reset |
|-------|------------------------------------------------|------|-------|
| 31:24 | Reserved.                                      | -    | -     |
| 23:16 | RAF_LVL: Current Read-Address-FIFO fill level  | RO   | 0x00  |
| 15:8  | WAF_LVL: Current Write-Address-FIFO fill level | RO   | 0x00  |
| 7:0   | TDF_LVL: Current Transfer-Data-FIFO fill level | RO   | 0x00  |

# <span id="page-1134-1"></span>**[DMA](#page-1109-1): CHAN\_ABORT Register**

**Offset**: 0x464

#### **Description**

Abort an in-progress transfer sequence on one or more channels

*Table 1181. CHAN\_ABORT Register*

| Bits  | Description                                                                                                                                                                                                                                                                                                                                                             | Type | Reset  |
|-------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 31:16 | Reserved.                                                                                                                                                                                                                                                                                                                                                               | -    | -      |
| 15:0  | Each bit corresponds to a channel. Writing a 1 aborts whatever transfer<br>sequence is in progress on that channel. The bit will remain high until any in<br>flight transfers have been flushed through the address and data FIFOs.<br>After writing, this register must be polled until it returns all-zero. Until this<br>point, it is unsafe to restart the channel. | SC   | 0x0000 |

#### <span id="page-1134-3"></span>**[DMA](#page-1109-1): N\_CHANNELS Register**

**Offset**: 0x468

*Table 1182. N\_CHANNELS Register*

| Bits | Description                                                                                                                                                                        | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:5 | Reserved.                                                                                                                                                                          | -    | -     |
| 4:0  | The number of channels this DMA instance is equipped with. This DMA<br>supports up to 16 hardware channels, but can be configured with as few as<br>one, to minimise silicon area. | RO   | -     |

# <span id="page-1134-0"></span>**[DMA](#page-1109-1): SECCFG\_CH0, SECCFG\_CH1, …, SECCFG\_CH14, SECCFG\_CH15 Registers**

**Offsets**: 0x480, 0x484, …, 0x4b8, 0x4bc

Security configuration for channel *N*. Control whether this channel performs Secure/Non-secure and Privileged/Unprivileged bus accesses.

If this channel generates bus accesses of some security level, an access of at least that level (in the order S+P > S+U > NS+P > NS+U) is required to program, trigger, abort, check the status of, interrupt on or acknowledge the interrupt of this channel.

This register automatically locks down (becomes read-only) once software starts to configure the channel.

This register is world-readable, but is writable only from a Secure, Privileged context.

*Table 1183. SECCFG\_CH0, SECCFG\_CH1, …, SECCFG\_CH14, SECCFG\_CH15 Registers*

| Bits | Description                                                                                                                                                                                                | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:3 | Reserved.                                                                                                                                                                                                  | -    | -     |
| 2    | LOCK: LOCK is 0 at reset, and is set to 1 automatically upon a successful write<br>to this channel's control registers. That is, a write to CTRL, READ_ADDR,<br>WRITE_ADDR, TRANS_COUNT and their aliases. | RW   | 0x0   |
|      | Once its LOCK bit is set, this register becomes read-only.                                                                                                                                                 |      |       |
|      | A failed write, for example due to the write's privilege being lower than that<br>specified in the channel's SECCFG register, will not set the LOCK bit.                                                   |      |       |
| 1    | S: Secure channel. If 1, this channel performs Secure bus accesses. If 0, it<br>performs Non-secure bus accesses.                                                                                          | RW   | 0x1   |
|      | If 1, this channel is controllable only from a Secure context.                                                                                                                                             |      |       |
| 0    | P: Privileged channel. If 1, this channel performs Privileged bus accesses. If 0,<br>it performs Unprivileged bus accesses.                                                                                | RW   | 0x1   |
|      | If 1, this channel is controllable only from a Privileged context of the same<br>Secure/Non-secure level, or any context of a higher Secure/Non-secure level.                                              |      |       |

# <span id="page-1135-0"></span>**[DMA](#page-1109-1): SECCFG\_IRQ0, SECCFG\_IRQ1, SECCFG\_IRQ2, SECCFG\_IRQ3 Registers**

**Offsets**: 0x4c0, 0x4c4, 0x4c8, 0x4cc

#### **Description**

Security configuration for IRQ *N*. Control whether the IRQ permits configuration by Non-secure/Unprivileged contexts, and whether it can observe Secure/Privileged channel interrupt flags.

*Table 1184. SECCFG\_IRQ0, SECCFG\_IRQ1, SECCFG\_IRQ2, SECCFG\_IRQ3 Registers*

| Bits | Description                                                                                                                                                                                                                                                                                                                                                           | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:2 | Reserved.                                                                                                                                                                                                                                                                                                                                                             | -    | -     |
| 1    | S: Secure IRQ. If 1, this IRQ's control registers can only be accessed from a<br>Secure context.<br>If 0, this IRQ's control registers can be accessed from a Non-secure context,<br>but Secure channels (as per SECCFG_CHx) are masked from the IRQ status,<br>and this IRQ's registers can not be used to acknowledge the channel interrupts<br>of Secure channels. | RW   | 0x1   |

| Bits | Description                                                                                                                                                                                                                                                                  | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 0    | P: Privileged IRQ. If 1, this IRQ's control registers can only be accessed from a<br>Privileged context.                                                                                                                                                                     | RW   | 0x1   |
|      | If 0, this IRQ's control registers can be accessed from an Unprivileged context,<br>but Privileged channels (as per SECCFG_CHx) are masked from the IRQ status,<br>and this IRQ's registers can not be used to acknowledge the channel interrupts<br>of Privileged channels. |      |       |

# <span id="page-1136-0"></span>**[DMA](#page-1109-1): SECCFG\_MISC Register**

**Offset**: 0x4d0

#### **Description**

Miscellaneous security configuration

*Table 1185. SECCFG\_MISC Register*

| Bits  | Description                                                                                                                                                                              | Type | Reset |
|-------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:10 | Reserved.                                                                                                                                                                                | -    | -     |
| 9     | TIMER3_S: If 1, the TIMER3 register is only accessible from a Secure context,<br>and timer DREQ 3 is only visible to Secure channels.                                                    | RW   | 0x1   |
| 8     | TIMER3_P: If 1, the TIMER3 register is only accessible from a Privileged (or<br>more Secure) context, and timer DREQ 3 is only visible to Privileged (or more<br>Secure) channels.       | RW   | 0x1   |
| 7     | TIMER2_S: If 1, the TIMER2 register is only accessible from a Secure context,<br>and timer DREQ 2 is only visible to Secure channels.                                                    | RW   | 0x1   |
| 6     | TIMER2_P: If 1, the TIMER2 register is only accessible from a Privileged (or<br>more Secure) context, and timer DREQ 2 is only visible to Privileged (or more<br>Secure) channels.       | RW   | 0x1   |
| 5     | TIMER1_S: If 1, the TIMER1 register is only accessible from a Secure context,<br>and timer DREQ 1 is only visible to Secure channels.                                                    | RW   | 0x1   |
| 4     | TIMER1_P: If 1, the TIMER1 register is only accessible from a Privileged (or<br>more Secure) context, and timer DREQ 1 is only visible to Privileged (or more<br>Secure) channels.       | RW   | 0x1   |
| 3     | TIMER0_S: If 1, the TIMER0 register is only accessible from a Secure context,<br>and timer DREQ 0 is only visible to Secure channels.                                                    | RW   | 0x1   |
| 2     | TIMER0_P: If 1, the TIMER0 register is only accessible from a Privileged (or<br>more Secure) context, and timer DREQ 0 is only visible to Privileged (or more<br>Secure) channels.       | RW   | 0x1   |
| 1     | SNIFF_S: If 1, the sniffer can see data transfers from Secure channels, and can<br>itself only be accessed from a Secure context.                                                        | RW   | 0x1   |
|       | If 0, the sniffer can be accessed from either a Secure or Non-secure context,<br>but can not see data transfers of Secure channels.                                                      |      |       |
| 0     | SNIFF_P: If 1, the sniffer can see data transfers from Privileged channels, and<br>can itself only be accessed from a privileged context, or from a Secure context<br>when SNIFF_S is 0. | RW   | 0x1   |
|       | If 0, the sniffer can be accessed from either a Privileged or Unprivileged<br>context (with sufficient security level) but can not see transfers from<br>Privileged channels.            |      |       |

# <span id="page-1137-0"></span>**[DMA](#page-1109-1): MPU\_CTRL Register**

**Offset**: 0x500

#### **Description**

Control register for DMA MPU. Accessible only from a Privileged context.

*Table 1186. MPU\_CTRL Register*

| Bits | Description                                                                                                                                                                                                                                                                                                              | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                                                                                                                                                                                                                                                                                | -    | -     |
| 3    | NS_HIDE_ADDR: By default, when a region's S bit is clear, Non-secure<br>Privileged reads can see the region's base address and limit address. Set this<br>bit to make the addresses appear as 0 to Non-secure reads, even when the<br>region is Non-secure, to avoid leaking information about the processor SAU<br>map. | RW   | 0x0   |
| 2    | S: Determine whether an address not covered by an active MPU region is<br>Secure (1) or Non-secure (0)                                                                                                                                                                                                                   | RW   | 0x0   |
| 1    | P: Determine whether an address not covered by an active MPU region is<br>Privileged (1) or Unprivileged (0)                                                                                                                                                                                                             | RW   | 0x0   |
| 0    | Reserved.                                                                                                                                                                                                                                                                                                                | -    | -     |

# <span id="page-1137-1"></span>**[DMA](#page-1109-1): MPU\_BAR0, MPU\_BAR1, …, MPU\_BAR6, MPU\_BAR7 Registers**

**Offsets**: 0x504, 0x50c, …, 0x534, 0x53c

#### **Description**

Base address register for MPU region *N*. Writable only from a Secure, Privileged context.

*Table 1187. MPU\_BAR0, MPU\_BAR1, …, MPU\_BAR6, MPU\_BAR7 Registers*

| Bits | Description                                                                                                                                                                                                                                                                                                                                                         | Type | Reset     |
|------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-----------|
| 31:5 | ADDR: This MPU region matches addresses where addr[31:5] (the 27 most<br>significant bits) are greater than or equal to BAR_ADDR, and less than or equal<br>to LAR_ADDR.<br>Readable from any Privileged context, if and only if this region's S bit is clear,<br>and MPU_CTRL_NS_HIDE_ADDR is clear. Otherwise readable only from a<br>Secure, Privileged context. | RW   | 0x0000000 |
| 4:0  | Reserved.                                                                                                                                                                                                                                                                                                                                                           | -    | -         |

#### <span id="page-1137-2"></span>**[DMA](#page-1109-1): MPU\_LAR0, MPU\_LAR1, …, MPU\_LAR6, MPU\_LAR7 Registers**

**Offsets**: 0x508, 0x510, …, 0x538, 0x540

#### **Description**

Limit address register for MPU region *N*. Writable only from a Secure, Privileged context, with the exception of the P bit.

*Table 1188. MPU\_LAR0, MPU\_LAR1, …, MPU\_LAR6, MPU\_LAR7 Registers*

| Bits | Description                                                                                                                                                                                                            | Type | Reset     |
|------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-----------|
| 31:5 | ADDR: Limit address bits 31:5. Readable from any Privileged context, if and<br>only if this region's S bit is clear, and MPU_CTRL_NS_HIDE_ADDR is clear.<br>Otherwise readable only from a Secure, Privileged context. | RW   | 0x0000000 |
| 4:3  | Reserved.                                                                                                                                                                                                              | -    | -         |

| Bits | Description                                                                                                                                                                                                                                                         | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 2    | S: Determines the Secure/Non-secure (=1/0) status of addresses matching<br>this region, if this region is enabled.                                                                                                                                                  | RW   | 0x0   |
| 1    | P: Determines the Privileged/Unprivileged (=1/0) status of addresses<br>matching this region, if this region is enabled. Writable from any Privileged<br>context, if and only if the S bit is clear. Otherwise, writable only from a Secure,<br>Privileged context. | RW   | 0x0   |
| 0    | EN: Region enable. If 1, any address within range specified by the base<br>address (BAR_ADDR) and limit address (LAR_ADDR) has the attributes<br>specified by S and P.                                                                                              | RW   | 0x0   |

# <span id="page-1138-2"></span>**[DMA](#page-1109-1): CH0\_DBG\_CTDREQ, CH1\_DBG\_CTDREQ, …, CH14\_DBG\_CTDREQ, CH15\_DBG\_CTDREQ Registers**

**Offsets**: 0x800, 0x840, …, 0xb80, 0xbc0

*Table 1189. CH0\_DBG\_CTDREQ, CH1\_DBG\_CTDREQ, …, CH14\_DBG\_CTDREQ, CH15\_DBG\_CTDREQ Registers*

| Bits | Description                                                                                                                                                                                                                     | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:6 | Reserved.                                                                                                                                                                                                                       | -    | -     |
| 5:0  | Read: get channel DREQ counter (i.e. how many accesses the DMA expects it<br>can perform on the peripheral without overflow/underflow. Write any value:<br>clears the counter, and cause channel to re-initiate DREQ handshake. | WC   | 0x00  |

# <span id="page-1138-3"></span>**[DMA](#page-1109-1): CH0\_DBG\_TCR, CH1\_DBG\_TCR, …, CH14\_DBG\_TCR, CH15\_DBG\_TCR Registers**

**Offsets**: 0x804, 0x844, …, 0xb84, 0xbc4

*Table 1190. CH0\_DBG\_TCR, CH1\_DBG\_TCR, …, CH14\_DBG\_TCR, CH15\_DBG\_TCR Registers*

| Bits | Description                                                                           | Type | Reset      |
|------|---------------------------------------------------------------------------------------|------|------------|
| 31:0 | Read to get channel TRANS_COUNT reload value, i.e. the length of the next<br>transfer | RO   | 0x00000000 |

