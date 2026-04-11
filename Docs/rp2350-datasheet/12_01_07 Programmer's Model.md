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
