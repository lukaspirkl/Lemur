# 12.1.6 Interrupts

There are eleven maskable interrupts generated in the UART. On RP2350, only the combined interrupt output, UARTINTR, is connected.

To enable or disable individual interrupts, change the mask bits in the Interrupt Mask Set/Clear Register, UARTIMSC. Set the appropriate mask bit HIGH to enable the interrupt.

The transmit and receive dataflow interrupts UARTRXINTR and UARTTXINTR have been separated from the status interrupts. This enables you to use UARTRXINTR and UARTTXINTR to read or write data in response to FIFO trigger levels.

The error interrupt, UARTEINTR, can be triggered when there is an error in the reception of data. A number of error conditions are possible.

The modem status interrupt, UARTMSINTR, is a combined interrupt of all the individual modem status signals.

The status of the individual interrupt sources can be read either from the Raw Interrupt Status Register, UARTRIS, or from the Masked Interrupt Status Register, UARTMIS.

#### **12.1.6.1. UARTMSINTR**

The modem status interrupt is asserted if any of the modem status signals (nUARTCTS, nUARTDCD, nUARTDSR, and nUARTRI) change. To clear the modem status interrupt, write a 1 to the bits corresponding to the modem status signals that generated the interrupt in the Interrupt Clear Register (UARTICR).

#### **12.1.6.2. UARTRXINTR**

The receive interrupt changes state when one of the following events occurs:

- The FIFOs are enabled and the receive FIFO reaches the programmed trigger level. This asserts the receive interrupt HIGH. To clear the receive interrupt, read data from the receive FIFO until it drops below the trigger level.
- The FIFOs are disabled (have a depth of one location) and data is received, thereby filling the receive FIFO. This asserts the receive interrupt HIGH. To clear the receive interrupt, perform a single read from the receive FIFO.

In both cases, you can also clear the interrupt manually.

#### **12.1.6.3. UARTTXINTR**

The transmit interrupt changes state when one of the following events occurs:

• The FIFOs are enabled and the transmit FIFO is equal to or lower than the programmed trigger level. This asserts the transmit interrupt HIGH. To clear the transmit interrupt, write data to the transmit FIFO until it exceeds the

trigger level.

• The FIFOs are disabled (have a depth of one location) and there is no data present in the transmit FIFO. This asserts the transmit interrupt HIGH. To clear the transmit interrupt, perform a single write to the transmit FIFO.

In both cases, you can also clear the interrupt manually.

To update the transmit FIFO, write data to the transmit FIFO before or after enabling the UART and the interrupts.

![](_page_967_Figure_5.jpeg)

The transmit interrupt is based on a transition through a level, rather than on the level itself. When the interrupt and the UART is enabled before any data is written to the transmit FIFO, the interrupt is not set. The interrupt is only set after written data leaves the single location of the transmit FIFO and it becomes empty.

#### **12.1.6.4. UARTRTINTR**

The receive timeout interrupt is asserted when the receive FIFO is not empty and no more data is received during a 32 bit period.

The receive timeout interrupt is cleared in the following scenarios:

- the FIFO becomes empty through reading all the data or by reading the holding register
- a 1 is written to the corresponding bit of the Interrupt Clear Register, UARTICR

#### **12.1.6.5. UARTEINTR**

The error interrupt is asserted when an error occurs in the reception of data by the UART. The interrupt can be caused by a number of different error conditions:

- framing
- parity
- break
- overrun

To determine the cause of the interrupt, read the Raw Interrupt Status Register (UARTRIS) or the Masked Interrupt Status Register (UARTMIS). To clear the interrupt, write to the relevant bits of the Interrupt Clear Register, UARTICR (bits 7 to 10 are the error clear bits).

#### **12.1.6.6. UARTINTR**

The interrupts are also combined into a single output, that is an OR function of the individual masked sources. You can connect this output to a system interrupt controller to provide another level of masking on a individual peripheral basis.

The combined UART interrupt is asserted if any of the individual interrupts are asserted and enabled.

#### <span id="page-967-0"></span>**12.1.7. Programmer's Model**

The SDK provides a uart\_init function to configure the UART with a particular baud rate. Once the UART is initialised, the user must configure a GPIO pin as UART\_TX and UART\_RX. See [Section 9.10.1](#page-595-1) for more information on selecting a GPIO function.

To initialise the UART, the uart\_init function takes the following steps:

1. De-asserts the reset

- 2. Enables clk\_peri
- 3. Sets enable bits in the control register
- 4. Enables the FIFOs
- 5. Sets the baud rate divisors
- 6. Sets the format

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_uart/uart.c](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_uart/uart.c#L42-L92) Lines 42 - 92*

```
42 uint uart_init(uart_inst_t *uart, uint baudrate) {
43 invalid_params_if(HARDWARE_UART, uart != uart0 && uart != uart1);
44 
45 if (uart_clock_get_hz(uart) == 0) {
46 return 0;
47 }
48 
49 uart_reset(uart);
50 uart_unreset(uart);
51 
52 uart_set_translate_crlf(uart, PICO_UART_DEFAULT_CRLF);
53 
54 // Any LCR writes need to take place before enabling the UART
55 uint baud = uart_set_baudrate(uart, baudrate);
56 
57 // inline the uart_set_format() call, as we don't need the CR disable/re-enable
58 // protection, and also many people will never call it again, so having
59 // the generic function is not useful, and much bigger than this inlined
60 // code which is only a handful of instructions.
61 //
62 // The UART_UARTLCR_H_FEN_BITS setting is combined as well as it is the same register
63 #ifdef 0
64 uart_set_format(uart, 8, 1, UART_PARITY_NONE);
65 // Enable FIFOs (must be before setting UARTEN, as this is an LCR access)
66 hw_set_bits(&uart_get_hw(uart)->lcr_h, UART_UARTLCR_H_FEN_BITS);
67 #else
68 uint data_bits = 8;
69 uint stop_bits = 1;
70 uint parity = UART_PARITY_NONE;
71 hw_write_masked(&uart_get_hw(uart)->lcr_h,
72 ((data_bits - 5u) << UART_UARTLCR_H_WLEN_LSB) |
73 ((stop_bits - 1u) << UART_UARTLCR_H_STP2_LSB) |
74 (bool_to_bit(parity != UART_PARITY_NONE) << UART_UARTLCR_H_PEN_LSB) |
75 (bool_to_bit(parity == UART_PARITY_EVEN) << UART_UARTLCR_H_EPS_LSB) |
76 UART_UARTLCR_H_FEN_BITS,
77 UART_UARTLCR_H_WLEN_BITS | UART_UARTLCR_H_STP2_BITS |
78 UART_UARTLCR_H_PEN_BITS | UART_UARTLCR_H_EPS_BITS |
79 UART_UARTLCR_H_FEN_BITS);
80 #endif
81 
82 // Enable the UART, both TX and RX
83 uart_get_hw(uart)->cr = UART_UARTCR_UARTEN_BITS | UART_UARTCR_TXE_BITS |
  UART_UARTCR_RXE_BITS;
84 // Always enable DREQ signals -- no harm in this if DMA is not listening
85 uart_get_hw(uart)->dmacr = UART_UARTDMACR_TXDMAE_BITS | UART_UARTDMACR_RXDMAE_BITS;
86 
87 return baud;
88 }
```

#### **12.1.7.1. Baud Rate Calculation**

```
The UART baud rate is derived from dividing clk_peri.
```

If the required baud rate is 115200 and UARTCLK = 125MHz then:

Baud Rate Divisor = (125 × 10<sup>6</sup> )/(16 × 115200) ~= 67.817

Therefore, BRDI = 67 and BRDF = 0.817,

Therefore, fractional part, m = integer((0.817 × 64) + 0.5) = 52

Generated baud rate divider = 67 + 52/64 = 67.8125

Generated baud rate = (125 × 10<sup>6</sup> )/(16 × 67.8125) ~= 115207

Error = (abs(115200 - 115207) / 115200) × 100 ~= 0.006%

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_uart/uart.c](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_uart/uart.c#L155-L180) Lines 155 - 180*

```
155 uint uart_set_baudrate(uart_inst_t *uart, uint baudrate) {
156 invalid_params_if(HARDWARE_UART, baudrate == 0);
157 uint32_t baud_rate_div = (8 * uart_clock_get_hz(uart) / baudrate) + 1;
158 uint32_t baud_ibrd = baud_rate_div >> 7;
159 uint32_t baud_fbrd;
160 
161 if (baud_ibrd == 0) {
162 baud_ibrd = 1;
163 baud_fbrd = 0;
164 } else if (baud_ibrd >= 65535) {
165 baud_ibrd = 65535;
166 baud_fbrd = 0;
167 } else {
168 baud_fbrd = (baud_rate_div & 0x7f) >> 1;
169 }
170 
171 uart_get_hw(uart)->ibrd = baud_ibrd;
172 uart_get_hw(uart)->fbrd = baud_fbrd;
173 
174 // PL011 needs a (dummy) LCR_H write to latch in the divisors.
175 // We don't want to actually change LCR_H contents here.
176 uart_write_lcr_bits_masked(uart, 0, 0);
177 
178 // See datasheet
179 return (4 * uart_clock_get_hz(uart)) / (64 * baud_ibrd + baud_fbrd);
180 }
```

