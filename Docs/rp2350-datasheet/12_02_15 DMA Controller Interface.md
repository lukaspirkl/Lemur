# 12.2.15 DMA Controller Interface

The DW\_apb\_i2c has built-in DMA capability; it has a handshaking interface to the DMA Controller to request and control transfers. The APB bus is used to perform data transfers to and from the DMA. DMA transfers use single accesses, since the data rate is relatively low.

#### **12.2.15.1. Enabling the DMA Controller Interface**

To enable the DMA Controller interface on the DW\_apb\_i2c, you must write the DMA Control Register [\(IC\\_DMA\\_CR](#page-1037-0)). Writing a one into the TDMAE bit field of [IC\\_DMA\\_CR](#page-1037-0) register enables the DW\_apb\_i2c transmit handshaking interface. Writing a one into the RDMAE bit field of the [IC\\_DMA\\_CR](#page-1037-0) register enables the DW\_apb\_i2c receive handshaking interface.

#### **12.2.15.2. Overview of Operation**

The DMA Controller is programmed with the number of data items (transfer count) that are to be transmitted or received by DW\_apb\_i2c.

The transfer is broken into single transfers on the bus, each initiated by a request from the DW\_apb\_i2c.

For example, where the transfer count programmed into the DMA Controller is four. The DMA transfer consists of a series of four single transactions. If the DW\_apb\_i2c makes a transmit request to this channel, a single data item is written to the DW\_apb\_i2c TX FIFO. Similarly, if the DW\_apb\_i2c makes a receive request to this channel, a single data item is read from the DW\_apb\_i2c RX FIFO. Four separate requests must be made to this DMA channel before all four data items are written or read.

#### **12.2.15.3. Watermark Levels**

In DW\_apb\_i2c the registers for setting watermarks to allow DMA bursts do not need be set to anything other than their reset value. Specifically, [IC\\_DMA\\_TDLR](#page-1038-1) and [IC\\_DMA\\_RDLR](#page-1038-2) can be left at reset values of zero. This is because only single transfers are needed due to the low bandwidth of I2C relative to system bandwidth. Because the DMA controller normally has the highest priority on the system bus, transfers complete quickly.

