#### <span id="page-964-0"></span>**12.1.5. UART DMA Interface**

The UART provides an interface to connect to a DMA controller. The DMA operation of the UART is controlled using the DMA Control Register, UARTDMACR. The DMA interface includes the following signals:

For receive:

#### **UARTRXDMASREQ**

Single character DMA transfer request, asserted by the UART. For receive, one character consists of up to 12 bits. This signal is asserted when the receive FIFO contains at least one character.

#### **UARTRXDMABREQ**

Burst DMA transfer request, asserted by the UART. This signal is asserted when the receive FIFO contains more characters than the programmed watermark level. You can program the watermark level for each FIFO using the Interrupt FIFO Level Select Register (UARTIFLS).

#### **UARTRXDMACLR**

DMA request clear, asserted by a DMA controller to clear the receive request signals. If DMA burst transfer is requested, the clear signal is asserted during the transfer of the last data in the burst.

For transmit:

#### **UARTTXDMASREQ**

Single character DMA transfer request, asserted by the UART. For transmit, one character consists of up to eight bits. This signal is asserted when there is at least one empty location in the transmit FIFO.

#### **UARTTXDMABREQ**

Burst DMA transfer request, asserted by the UART. This signal is asserted when the transmit FIFO contains less characters than the watermark level. You can program the watermark level for each FIFO using the Interrupt FIFO Level Select Register (UARTIFLS).

#### **UARTTXDMACLR**

DMA request clear, asserted by a DMA controller to clear the transmit request signals. If DMA burst transfer is requested, the clear signal is asserted during the transfer of the last data in the burst.

The burst transfer and single transfer request signals are not mutually exclusive: they can both be asserted at the same time. When the receive FIFO exceeds the watermark level, the burst transfer request and the single transfer request signals are both asserted. When the receive FIFO is below than the watermark level, only the single transfer request signal is asserted. This is useful in situations where the number of characters left to be received in the stream is less than a burst.

Consider a scenario where the watermark level is set to four, but 19 characters are left to be received. The DMA controller then transfers four bursts of four characters and three single transfers to complete the stream.

![](_page_965_Figure_12.jpeg)

For the remaining three characters, the UART cannot assert the burst request.

Each request signal remains asserted until the relevant DMACLR signal is asserted. After the request clear signal is deasserted, a request signal can become active again, depending on the conditions described previously. All request signals are de-asserted if the UART is disabled or the relevant DMA enable bit, TXDMAE or RXDMAE, in the DMA Control Register, UARTDMACR, is cleared.

If you disable the FIFOs in the UART, it operates in character mode. Character mode limits FIFO transfers to a single character at a time, so only the DMA single transfer mode can operate. In character mode, only the UARTRXDMASREQ and UARTTXDMASREQ request signals can be asserted. For information about disabling the FIFOs, see the Line Control Register, UARTLCR\_H.

When the UART is in the FIFO enabled mode, data transfers can use either single or burst transfers depending on the programmed watermark level and the amount of data in the FIFO. [Table 1026](#page-965-0) lists the trigger points for UARTRXDMABREQ and UARTTXDMABREQ, depending on the watermark level, for the transmit and receive FIFOs.

*Table 1026. DMA trigger points for the transmit and receive FIFOs.*

<span id="page-965-0"></span>

| Watermark level | Burst length                            |                                      |  |  |
|-----------------|-----------------------------------------|--------------------------------------|--|--|
|                 | Transmit (number of empty<br>locations) | Receive (number of filled locations) |  |  |
| 1/8             | 28                                      | 4                                    |  |  |
| 1/4             | 24                                      | 8                                    |  |  |
| 1/2             | 16                                      | 16                                   |  |  |
| 3/4             | 8                                       | 24                                   |  |  |
| 7/8             | 4                                       | 28                                   |  |  |

In addition, the DMAONERR bit in the DMA Control Register, UARTDMACR, supports the use of the receive error interrupt,

UARTEINTR. It enables the DMA receive request outputs, UARTRXDMASREQ or UARTRXDMABREQ, to be masked out when the UART error interrupt, UARTEINTR, is asserted. The DMA receive request outputs remain inactive until the UARTEINTR is cleared. The DMA transmit request outputs are unaffected.

*Figure 66. DMA transfer waveforms.*

<span id="page-966-1"></span>![](_page_966_Figure_3.jpeg)

[Figure 66](#page-966-1) shows the timing diagram for both a single transfer request and a burst transfer request with the appropriate DMACLR signal. The signals are all synchronous to PCLK. For the sake of clarity it is assumed that there is no synchronization of the request signals in the DMA controller.

