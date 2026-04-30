/**
 * Copyright (c) 2020 Raspberry Pi (Trading) Ltd.
 *
 * SPDX-License-Identifier: BSD-3-Clause
 */


#include <stdio.h>
#include "pico/stdlib.h"
#include "hardware/uart.h"
#include "pico/binary_info.h"

/// \tag::hello_uart[]

#define UART_ID uart0
#define BAUD_RATE 115200

// We are using pins 0 and 1, but see the GPIO function select table in the
// datasheet for information on which other pins can be used.
#define UART_TX_PIN 0
#define UART_RX_PIN 1


#define ANSI_COLOR_RED "\x1b[31m"
#define ANSI_COLOR_GREEN "\x1b[32m"
#define ANSI_BOLD "\x1b[1m"
#define ANSI_COLOR_RESET "\x1b[0m"

bi_decl(bi_1pin_with_name(0, "Debug [SerialTerminal-RX]"));
bi_decl(bi_1pin_with_name(1, "Debug [SerialTerminal-TX]"));


int main() {
    // Set up our UART with the required speed.
    uart_init(UART_ID, BAUD_RATE);

    // Set the TX and RX pins by using the function select on the GPIO
    // Set datasheet for more information on function select
    gpio_set_function(UART_TX_PIN, UART_FUNCSEL_NUM(UART_ID, UART_TX_PIN));
    gpio_set_function(UART_RX_PIN, UART_FUNCSEL_NUM(UART_ID, UART_RX_PIN));

    // Use some the various UART functions to send out data
    // In a default system, printf will also output via the default UART

    // Send out a character without any conversions
    uart_putc_raw(UART_ID, 'A');

    // Send out a character but do CR/LF conversions
    uart_putc(UART_ID, 'B');

    // Send out a string, with CR/LF conversions
    uart_puts(UART_ID, " Hello, UART!\n\r");

    uart_puts(UART_ID, ANSI_COLOR_GREEN ANSI_BOLD "KeySquare_Control" ANSI_COLOR_RESET "> ");

    while(true)
    {
        if (uart_is_readable(UART_ID))
        {
            char c = uart_getc(UART_ID);
            uart_putc(UART_ID, c);
        }
    }
}

/// \end::hello_uart[]
