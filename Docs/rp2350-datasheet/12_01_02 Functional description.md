# 12.1.2 Functional description

*Figure 62. UART block diagram. Test logic is not shown for clarity.*

![](_page_959_Figure_11.jpeg)

#### **12.1.2.1. AMBA APB interface**

The AMBA APB interface generates read and write decodes for accesses to status/control registers, and the transmit and receive FIFOs.

#### **12.1.2.2. Register block**

The register block stores data written, or to be read across the AMBA APB interface.

#### **12.1.2.3. Baud rate generator**

The baud rate generator contains free-running counters that generate the internal clocks: Baud16 and IrLPBaud16 signals. Baud16 provides timing information for UART transmit and receive control. Baud16 is a stream of pulses with a width of one UARTCLK clock period and a frequency of 16 times the baud rate.

#### **12.1.2.4. Transmit FIFO**

The transmit FIFO is an 8-bit wide, 32 location deep, FIFO memory buffer. CPU data written across the APB interface is stored in the FIFO until read out by the transmit logic. When disabled, the transmit FIFO acts like a one byte holding register.

#### **12.1.2.5. Receive FIFO**

The receive FIFO is a 12-bit wide, 32 location deep, FIFO memory buffer. Received data and corresponding error bits are stored in the receive FIFO by the receive logic until read out by the CPU across the APB interface. When disabled, the receive FIFO acts like a one byte holding register.

#### **12.1.2.6. Transmit logic**

The transmit logic performs parallel-to-serial conversion on the data read from the transmit FIFO. Control logic outputs the serial bit stream in the following order:

- 1. Start bit
- 2. Data bits (Least Significant Bit (LSB) first)
- 3. Parity bit
- 4. Stop bits according to the programmed configuration in control registers

#### **12.1.2.7. Receive logic**

The receive logic performs serial-to-parallel conversion on the received bit stream after a valid start pulse has been detected. Receive logic includes overrun, parity, frame error checking, and line break detection; you can find the output of these checks in the status that accompanies the data written to the receive FIFO.

#### **12.1.2.8. Interrupt generation logic**

The UART generates individual maskable active HIGH interrupts to the processor interrupt controllers. To generate combined interrupts, the UART outputs an OR function of the individual interrupt requests.

For more information, see [Section 12.1.6](#page-966-0).

#### **12.1.2.9. DMA interface**

The UART provides an interface to connect to the DMA controller as a UART DMA; for more information, see [Section](#page-964-0) [12.1.5](#page-964-0).

#### **12.1.2.10. Synchronizing registers and logic**

The UART supports both asynchronous and synchronous operation of the clocks, PCLK and UARTCLK. The UART implements always-on synchronisation registers and handshaking logic. This has a minimal impact on performance and area. The UART performs control signal synchronisation on both directions of data flow (from the PCLK to the UARTCLK domain, and from the UARTCLK to the PCLK domain).

