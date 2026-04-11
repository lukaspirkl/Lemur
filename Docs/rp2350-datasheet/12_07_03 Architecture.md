# 12.7.3 Architecture

#### **12.7.3.1. Clock speed**

This controller requires clk\_usb to be running at 48MHz.

![](_page_1141_Picture_10.jpeg)

clk\_sys must also be running at > 48MHz. See [RP2350-E12](#page-1365-1).

#### **12.7.3.2. Overview**

*Figure 123. A simplified overview of the USB controller architecture.*

![](_page_1142_Figure_2.jpeg)

The USB controller is an area-efficient design that muxes a device controller or host controller onto a common set of components. Each component is detailed below.

#### **12.7.3.3. USB PHY**

The USB PHY provides the electrical interface between the USB DP and DM pins and the digital logic of the controller. The DP and DM pins are a differential pair, meaning the values are always the inverse of each other, except to encode a specific line state (e.g. SE0). The USB PHY drives the DP and DM pins to transmit data and performs a differential receive of any incoming data. The USB PHY provides both single-ended and differential receive data to the line state detection module.

The USB PHY has built in pull-up and pull-down resistors. When the controller acts as a Full Speed device, the DP pin is pulled up to indicate to the host that a Full Speed device has been connected. In host mode, a weak pull-down is applied to DP and DM so that the lines are pulled to a logical zero until the device pulls up DP for Full Speed or DM for Low Speed.

#### **12.7.3.4. Line State Detection**

The [USB 2.0 Specification](https://www.usb.org/document-library/usb-20-specification) defines several line states (Bus Reset, Connected, Suspend, Resume, Data 1, Data 0, etc.) that need to be detected. The line state detection module has several state machines to detect these states and signal events to the other hardware components. There is no shared clock signal in USB, so the RX data must be sampled by an internal clock. The maximum data rate of USB Full Speed is 12 Mb/s. The RX data is sampled at 48MHz, giving 4 clock cycles to capture and filter the bus state. The line state detection module distributes the filtered RX data to the Serial RX Engine.

#### **12.7.3.5. Serial RX Engine**

The serial receive (RX) engine decodes receive data captured by the line state detection module. It produces the following information:

- The PID of the incoming data packet
- The device address for the incoming data
- The device endpoint for the incoming data
- Data bytes

The serial receive engine also detects errors in RX data by performing a CRC check on the incoming data. Any errors are signalled to the other hardware blocks and can raise an interrupt.

#### **NOTE**

If you disconnect the USB cable during packet transfer in either host or device mode, the hardware will raise errors. Software must account for this scenario if you enable error interrupts.

#### **12.7.3.6. Serial TX Engine**

The serial transmit (TX) engine is a mirror of the serial receive engine. It is connected to the currently active controller (either device or host). It creates TOKEN and DATA packets, calculates the CRC, and transmits them on the bus.

#### **12.7.3.7. DPSRAM**

The USB controller uses 4 kB (4096 bytes) of Dual Port SRAM (DPSRAM) to store control registers and data buffers. The DPSRAM is accessible as a 32-bit wide memory at address 0 of the USB controller (0x50100000).

The DPSRAM has the following characteristics, which differ from most registers on RP2350:

- Supports 8-bit, 16-bit, and 32-bit accesses (typically, RP2350 registers only support 32-bit accesses)
- Does *not* support set/clear aliases. (typically, RP2350 registers support these)

Data Buffers are typically 64 bytes long, as this is the maximum normal packet size for most Full Speed packets. Isochronous endpoints support a maximum buffer size of 1023 bytes. For other packet types, the maximum size is 64 bytes per buffer.

#### **12.7.3.7.1. Concurrent access**

The DPSRAM in the USB controller is asynchronous. The **dual port** part of the name indicates that *both* the processor and the USB controller have ports to read and write, and these two ports are in different clock domains. As a result, the processor and USB controller can access the same memory address at the same time. One could write and one could read simultaneously. This could result in inconsistent data reads. You can avoid this scenario by following the rules outlined in this section.

The AVAILABLE bit in the buffer control register indicates who has ownership of a buffer. Set this bit to 1 from the processor to give the controller ownership of the buffer. When it has finished using the buffer, the controller sets the bit back to 0. Set the AVAILABLE bit separately from the rest of the data in the buffer control register so that the rest of the data in the buffer control register is accurate when the AVAILABLE bit is set.

This is necessary because the processor clock clk\_sys can run several times faster than the clk\_usb clock. Therefore clk\_sys can update the data *during* a USB controller read on a slower clock. The correct process is:

- 1. Write buffer information (length, etc.) to the buffer control register.
- 2. nop for some clk\_sys cycles to ensure that at least one clk\_usb cycle passes. Consider a scenario where clk\_sys runs at 125MHz and clk\_usb runs at 48MHz. Because , you should issue 3 nop instructions between the writes to guarantee that at least one clk\_usb cycle has passed.
- 3. Set the AVAILABLE bit.

If clk\_sys and clk\_usb run at the same frequency, then it is not necessary to set the AVAILABLE bit separately.

# **NOTE**

When the USB controller writes the status back to the DPSRAM, it does a 16-bit write to the lower 2 bytes for buffer 0 and the upper 2 bytes for buffer 1. When using double-buffered mode, always treat the buffer control register as two 16-bit registers when updating it in software.

#### **12.7.3.7.2. Layout**

Addresses 0x0 → 0xff are used for control registers containing configuration data. The remaining space, addresses 0x100 → 0xfff (3840 bytes) can be used for data buffers. The controller has control registers that start at address 0x10000.

The memory layout depends on the USB controller mode:

- In Device mode, the host can access multiple endpoints, so each endpoint must have endpoint control and buffer control registers.
- In Host mode, the host software running on the processor decides which endpoints and devices to access. This only requires one set of endpoint control and buffer control registers. As well as software-driven transfers, the host controller can poll up to 15 interrupt endpoints and has a register for each of these interrupt endpoints.

*Table 1191. DPSRAM layout*

| Offset | Device Function        | Host Function                 |
|--------|------------------------|-------------------------------|
| 0x0    | Setup packet (8 bytes) |                               |
| 0x8    | EP1 in control         | Interrupt endpoint control 1  |
| 0xc    | EP1 out control        | Spare                         |
| 0x10   | EP2 in control         | Interrupt endpoint control 2  |
| 0x14   | EP2 out control        | Spare                         |
| 0x18   | EP3 in control         | Interrupt endpoint control 3  |
| 0x1c   | EP3 out control        | Spare                         |
| 0x20   | EP4 in control         | Interrupt endpoint control 4  |
| 0x24   | EP4 out control        | Spare                         |
| 0x28   | EP5 in control         | Interrupt endpoint control 5  |
| 0x2c   | EP5 out control        | Spare                         |
| 0x30   | EP6 in control         | Interrupt endpoint control 6  |
| 0x34   | EP6 out control        | Spare                         |
| 0x38   | EP7 in control         | Interrupt endpoint control 7  |
| 0x3c   | EP7 out control        | Spare                         |
| 0x40   | EP8 in control         | Interrupt endpoint control 8  |
| 0x44   | EP8 out control        | Spare                         |
| 0x48   | EP9 in control         | Interrupt endpoint control 9  |
| 0x4c   | EP9 out control        | Spare                         |
| 0x50   | EP10 in control        | Interrupt endpoint control 10 |
| 0x54   | EP10 out control       | Spare                         |
| 0x58   | EP11 in control        | Interrupt endpoint control 11 |

| Offset | Device Function                                              | Host Function                        |
|--------|--------------------------------------------------------------|--------------------------------------|
| 0x5c   | EP11 out control                                             | Spare                                |
| 0x60   | EP12 in control                                              | Interrupt endpoint control 12        |
| 0x64   | EP12 out control                                             | Spare                                |
| 0x68   | EP13 in control                                              | Interrupt endpoint control 13        |
| 0x6c   | EP13 out control                                             | Spare                                |
| 0x70   | EP14 in control                                              | Interrupt endpoint control 14        |
| 0x74   | EP14 out control                                             | Spare                                |
| 0x78   | EP15 in control                                              | Interrupt endpoint control 15        |
| 0x7c   | EP15 out control                                             | Spare                                |
| 0x80   | EP0 in buffer control                                        | EPx buffer control                   |
| 0x84   | EP0 out buffer control                                       | Spare                                |
| 0x88   | EP1 in buffer control                                        | Interrupt endpoint buffer control 1  |
| 0x8c   | EP1 out buffer control                                       | Spare                                |
| 0x90   | EP2 in buffer control<br>Interrupt endpoint buffer control 2 |                                      |
| 0x94   | EP2 out buffer control<br>Spare                              |                                      |
| 0x98   | EP3 in buffer control                                        | Interrupt endpoint buffer control 3  |
| 0x9c   | EP3 out buffer control<br>Spare                              |                                      |
| 0xa0   | EP4 in buffer control                                        | Interrupt endpoint buffer control 4  |
| 0xa4   | EP4 out buffer control                                       | Spare                                |
| 0xa8   | EP5 in buffer control                                        | Interrupt endpoint buffer control 5  |
| 0xac   | EP5 out buffer control<br>Spare                              |                                      |
| 0xb0   | EP6 in buffer control                                        | Interrupt endpoint buffer control 6  |
| 0xb4   | EP6 out buffer control                                       | Spare                                |
| 0xb8   | EP7 in buffer control                                        | Interrupt endpoint buffer control 7  |
| 0xbc   | EP7 out buffer control                                       | Spare                                |
| 0xc0   | EP8 in buffer control                                        | Interrupt endpoint buffer control 8  |
| 0xc4   | EP8 out buffer control<br>Spare                              |                                      |
| 0xc8   | EP9 in buffer control                                        | Interrupt endpoint buffer control 9  |
| 0xcc   | EP9 out buffer control<br>Spare                              |                                      |
| 0xd0   | EP10 in buffer control                                       | Interrupt endpoint buffer control 10 |
| 0xd4   | EP10 out buffer control                                      | Spare                                |
| 0xd8   | EP11 in buffer control                                       | Interrupt endpoint buffer control 11 |
| 0xdc   | EP11 out buffer control                                      | Spare                                |
| 0xe0   | EP12 in buffer control                                       | Interrupt endpoint buffer control 12 |
| 0xe4   | EP12 out buffer control<br>Spare                             |                                      |
| 0xe8   | EP13 in buffer control                                       | Interrupt endpoint buffer control 13 |

| Offset | Device Function<br>Host Function                           |                                      |
|--------|------------------------------------------------------------|--------------------------------------|
| 0xec   | EP13 out buffer control                                    | Spare                                |
| 0xf0   | EP14 in buffer control                                     | Interrupt endpoint buffer control 14 |
| 0xf4   | EP14 out buffer control                                    | Spare                                |
| 0xf8   | EP15 in buffer control                                     | Interrupt endpoint buffer control 15 |
| 0xfc   | EP15 out buffer control                                    | Spare                                |
| 0x100  | EP0 buffer 0 (shared between in and<br>EPx control<br>out) |                                      |
| 0x140  | Optional EP0 buffer 1<br>Spare                             |                                      |
| 0x180  | Data buffers                                               |                                      |

#### **12.7.3.7.3. Endpoint Control Register**

The endpoint control register is used to configure an endpoint. It defines:

- The endpoint type
- The base address of the endpoint's data buffer (or data buffers if double-buffered)
- Which endpoint events trigger the controller interrupt output

A device must support Endpoint 0 so that it can reply to SETUP packets and be enumerated. As a result, there is no endpoint control register for EP0. Its buffers begin at 0x100. All other endpoints can have either single or dual buffers and are mapped at the base address programmed. As EP0 has no endpoint control register, the interrupt enable controls for EP0 come from [SIE\\_CTRL.](#page-1160-0)

*Table 1192. Endpoint control register layout*

| Bit(s) | Device Function    |                                                                                   | Host Function                                                                                                                               |
|--------|--------------------|-----------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------|
| 31     | Endpoint enable    |                                                                                   |                                                                                                                                             |
| 30     |                    | Single buffered (64 bytes) = 0, Double buffered (64 bytes × 2) = 1                |                                                                                                                                             |
| 29     |                    | Enable interrupt for every transferred buffer                                     |                                                                                                                                             |
| 28     |                    | Enable interrupt for every 2 transferred buffers (valid for double-buffered only) |                                                                                                                                             |
| 27:26  |                    | Endpoint Type: Control = 0, Isochronous = 1, Bulk = 2, Interrupt = 3              |                                                                                                                                             |
| 25:18  | N/A                |                                                                                   | The interval the host controller should poll this                                                                                           |
| 17     | Interrupt on STALL |                                                                                   | endpoint. Only applicable for interrupt<br>endpoints. Specified in ms - 1. For example: a<br>value of 9 would poll the endpoint every 10ms. |
| 16     | Interrupt on NAK   |                                                                                   |                                                                                                                                             |
| 15:6   |                    | Address base offset in DPSRAM of data buffer(s)                                   |                                                                                                                                             |

#### **NOTE**

The data buffer base address must be 64-byte aligned, since bits 0 through 5 are ignored.

#### **12.7.3.7.4. Buffer Control Register**

The buffer control register contains information about the state of the data buffers for that endpoint. It is shared between the processor and the controller. If the endpoint is configured to be single-buffered, only the first half (bits 0 through 15) of the buffer are used.

If double buffering, the buffer select starts at buffer 0. From then on, the buffer select flips between buffer 0 and 1

unless the **reset buffer select** bit is set (which resets the buffer select to buffer 0). The value of the buffer select is internal to the controller and not accessible by the processor.

For host interrupt and isochronous packets on EPx, the buffer full bit will be set on completion even if the transfer was unsuccessful. To determine the error, read the error bits in the [SIE\\_STATUS](#page-1161-0) register.

*Table 1193. Buffer control register layout*

| Bit(s) | Function                                                                                                                                                                                                                                                                                                                                                     |  |
|--------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--|
| 31     | Buffer 1 full. Should be set to 1 by the processor for an IN transaction and 0 for an OUT<br>transaction. The controller sets this to 1 for an OUT transaction because it has filled the buffer.<br>The controller sets it to 0 for an IN transaction because it has emptied the buffer. Only valid<br>when double buffering.                                |  |
| 30     | Last buffer of transfer for buffer 1. Only valid when double buffering.                                                                                                                                                                                                                                                                                      |  |
| 29     | Data PID for buffer 1 - DATA0 = 0, DATA1 = 1. Only valid when double buffering.                                                                                                                                                                                                                                                                              |  |
| 27:28  | Double buffer offset for isochronous mode (0 = 128, 1 = 256, 2 = 512, 3 = 1024).                                                                                                                                                                                                                                                                             |  |
| 26     | Buffer 1 available. Whether the buffer can be used by the controller for a transfer. The<br>processor sets this to 1 when the buffer is configured. The controller sets this to 0 after it has<br>sent the data to the host for an IN transaction, or filled the buffer with data from the host for an<br>OUT transaction. Only valid when double buffering. |  |
| 25:16  | Buffer 1 transfer length. Only valid when double buffering.                                                                                                                                                                                                                                                                                                  |  |
| 15     | Buffer 0 full. Should be set to 1 by the processor for an IN transaction and 0 for an OUT<br>transaction. The controller sets this to 1 for an OUT transaction because it has filled the buffer.<br>The controller sets it to 0 for an IN transaction because it has emptied the buffer.                                                                     |  |
| 14     | Last buffer of transfer for buffer 0.                                                                                                                                                                                                                                                                                                                        |  |
| 13     | Data PID for buffer 0 - DATA0 = 0, DATA1 = 1.                                                                                                                                                                                                                                                                                                                |  |
| 12     | Reset buffer select to buffer 0 - cleared at end of transfer. For device only.                                                                                                                                                                                                                                                                               |  |
| 11     | Send STALL for device, STALL received for host.                                                                                                                                                                                                                                                                                                              |  |
| 10     | Buffer 0 available. Indicates whether the buffer can be used by the controller for a transfer.<br>The processor sets this to 1 when the buffer is configured. The controller sets this to 0 after it<br>has sent the data to the host for an IN transaction or filled the buffer with data from the host<br>for an OUT transaction.                          |  |
| 9:0    | Buffer 0 transfer length.                                                                                                                                                                                                                                                                                                                                    |  |

# **WARNING**

If you run clk\_sys and clk\_usb at different speeds, set the available and stall bits *after* the other data in the buffer control register. Otherwise, the controller may initiate a transaction with data from a previous packet. The controller could see the available bit set, but get the data PID or length from the previous packet.

#### **12.7.3.8. Device Controller**

This section details how the device controller operates when it receives various packet types from the host.

#### **12.7.3.8.1. SETUP**

The device controller **MUST** always accept a SETUP packet from the host. DPSRAM dedicates its first 8 bytes to the setup packet.

The [USB 2.0 Specification](https://www.usb.org/document-library/usb-20-specification) states that receiving a setup packet also clears any stall bits on EP0. For this reason, the stall

bits for EP0 are gated with two bits in the [EP\\_STALL\\_ARM](#page-1167-1) register. These bits are cleared when a setup packet is received. This means that to send a stall on EP0, you must set both the stall bit in the buffer control register and the appropriate bit in [EP\\_STALL\\_ARM](#page-1167-1).

Barring any errors, the setup packet will be put into the setup packet buffer at DPSRAM offset 0x0. The device controller will then reply with an ACK.

Finally, [SIE\\_STATUS](#page-1161-0).SETUP\_REC is set to indicate that a setup packet has been received. This will trigger an interrupt if the programmer has enabled the SETUP\_REC interrupt (see [INTE](#page-1173-0)).

#### **12.7.3.8.2. IN**

From the device's point of view, an IN transfer means transferring data *into* the host. When an IN token is received from the host, the request is handled as follows:

#### TOKEN phase:

- 1. If STALL is set in the buffer control register (and if EP0, the appropriate [EP\\_STALL\\_ARM](#page-1167-1) bit is set), send a STALL response and go to idle.
- 2. If AVAILABLE and FULL bits are set in buffer control, go to the DATA phase.
- 3. If this is an isochronous endpoint, go to idle.
  - Otherwise, send NAK and go to the DATA phase.

#### DATA phase:

- 1. Send data.
- 2. If this is an isochronous endpoint, go to idle.
  - Otherwise, go to the ACK phase.

#### ACK phase:

- 1. Wait for ACK packet from host.
- 2. If there is a timeout, raise a timeout error.
- 3. If ACK is received, the packet is done, so go to STATUS phase.

#### <span id="page-1148-0"></span>STATUS phase:

- 1. If this was the last buffer in the transfer (i.e. if the LAST\_BUFFER bit in the buffer control register was set), set [SIE\\_STATUS](#page-1161-0).TRANS\_COMPLETE.
- 2. If the endpoint is double buffered, flip the buffer select to the other buffer.
- 3. Set a bit in [BUFF\\_STATUS](#page-1162-0) to indicate the buffer is done. When handling this event, the programmer should read [BUFF\\_CPU\\_SHOULD\\_HANDLE](#page-1164-0) to see if it is buffer 0 or buffer 1 that is finished. If the endpoint is double-buffered, both buffers could be done. The cleared [BUFF\\_STATUS](#page-1162-0) bit will be set again, and [BUFF\\_CPU\\_SHOULD\\_HANDLE](#page-1164-0) will change in this instance.
- 4. Update status in the appropriate half of the buffer control register: length, pid, and last\_buff are set. Everything else is written to zero.

If the host receives a NAK, the host will retry again later.

#### **12.7.3.8.3. OUT**

When an OUT token is received from the host, the request is handled as follows:

#### TOKEN phase:

1. If this is *not* an Isochronous endpoint and the data PID does not match the buffer control register, raise

[SIE\\_STATUS](#page-1161-0).DATA\_SEQ\_ERROR (isochronous data is always sent with a DATA0 pid).

2. If the AVAILABLE bit is set and the FULL bit is clear, go to the DATA phase, unless the STALL bit is set in which case the device controller will reply with a STALL.

#### DATA phase:

1. Store received data in buffer. If this is an isochronous endpoint, go to the STATUS phase. Otherwise, go to the ACK phase.

#### ACK phase:

1. Send ACK. Go to the STATUS phase.

#### STATUS phase:

See IN STATUS phase: [\[usb-device-in-status-phase\]](#page-1148-0). There is one difference: the FULL bit is set in the buffer control register to indicate that data has been received. In the IN phase, the FULL bit is cleared to indicate that data has been sent.

#### **12.7.3.8.4. Suspend and Resume**

The USB device controller supports suspend, resume, and device-initiated remote resume (triggered with [SIE\\_CTRL](#page-1160-0).RESUME). There is an interrupt / status bit in [SIE\\_STATUS](#page-1161-0). It is not necessary to enable the suspend and resume interrupts, since suspend and resume are irrelevant to most devices.

The device goes into suspend when it does not see any start of frame packets (transmitted every 1ms) from the host.

![](_page_1149_Figure_12.jpeg)

If you enable the suspend interrupt, it is likely you will see a suspend interrupt when the device first connects, but the bus is idle. The bus can be idle for a few milliseconds before the host begins sending start of frame packets. If you do not have a VBUS detect circuit connected, you will also see a suspend interrupt when the device disconnects. Without VBUS detection, it is impossible to tell the difference between being disconnected and suspended.

#### **12.7.3.9. Host Controller**

The host controller design is similar to the device controller. The host starts all transactions, so the host always deals with transactions it has started. For this reason, there is only one set of endpoint control and endpoint buffer control registers. The host controller also contains additional hardware to poll interrupt endpoints in the background when there are no software controlled transactions taking place.

The host needs to send keep-alive packets to the device every 1ms to keep the device from suspending. Full Speed mode uses a SOF (start of frame) packet. Low Speed mode uses an EOP (end of packet) instead. Set [SIE\\_CTRL](#page-1160-0).KEEP\_ALIVE\_EN and [SIE\\_CTRL.](#page-1160-0)SOF\_EN to enable these packets.

Several bits in [SIE\\_CTRL](#page-1160-0) are used to begin a host transaction:

- SEND\_SETUP Send a setup packet. Typically used with RECEIVE\_TRANS, so the setup packet will be sent followed by the additional data transaction expected from the device.
- SEND\_TRANS This transfer is OUT from the host.
- RECEIVE\_TRANS This transfer is IN to the host.
- START\_TRANS Start the transfer (non-latching).
- STOP\_TRANS Stop the current transfer (non-latching).
- PREAMBLE\_ENABLE Used to send a packet to a Low Speed device on a Full Speed hub. Sends a PRE token packet before every packet the host sends (i.e. PRE, TOKEN, PRE, DATA, pre, ACK).
- SOF\_SYNC Used to delay the transaction until after the next SOF. Useful for interrupt and isochronous endpoints. The host controller prevents a transaction of 64 bytes from clashing with the SOF packets. For longer isochronous

packets, software is responsible for preventing collisions. To prevent collisions in software, use SOF\_SYNC and limit the number of packets sent in one frame. If a transaction is set up with multiple packets, SOF\_SYNC only applies to the first packet.

The START\_TRANS bit is synchronised separately from other control bits in the SIE\_CTRL register because the processor clock clk\_sys can be asynchronous to the clk\_usb clock. Always set the START\_TRANS bit separately from the rest of the data in the SIE\_CTRL register. Always ensure that at least two clk\_usb cycles pass between writing to START\_TRANS and other bits in SIE\_CTRL. This ensures that the register contents are stable when the controller is prompted to start a transfer.

Consider a scenario where clk\_sys runs at 125MHz and clk\_usb runs at 48MHz. Because , you should issue 6 nop instructions between the writes to guarantee that at least two clk\_usb cycles have passed.

#### **12.7.3.9.1. SETUP**

The SETUP packet sent from the host always comes from the dedicated 8 bytes of space at offset 0x0 of the DPSRAM. Like the device controller, there are no control registers associated with the setup packet. The parameters are hardcoded and loaded into the hardware when you write to START\_TRANS with the SEND\_SETUP bit set. Once the setup packet has been sent, the host state machine waits for an ACK from the device. If there is a timeout, an RX\_TIMEOUT error will be raised. If the SEND\_TRANS bit is set, the host state machine will move to the OUT phase. Typically, the SEND\_SETUP packet is used with the RECEIVE\_TRANS bit, so the controller moves to the IN phase after sending a setup packet.

#### <span id="page-1150-0"></span>**12.7.3.9.2. IN**

An IN transfer is triggered with the RECEIVE\_TRANS bit set when the START\_TRANS bit is set. If the SEND\_SETUP bit was set, this may be preceded by a SETUP packet.

#### CONTROL phase:

- 1. Read the EPx control register located at 0x80 to get the following endpoint information:
  - Is it double buffered?
  - What interrupts are enabled?
  - Base address of the data buffer (data buffers if in double-buffered mode)
  - What is the endpoint type?
- 2. Read the EPx buffer control register at 0x100 to get endpoint buffer information, such as transfer length and data PID.
- 3. Set the AVAILABLE bit (the host state machine checks for it).
- 4. Clear the FULL bit.

#### TOKEN phase:

1. Send the IN token packet to the device. The target device address and endpoint come from the [ADDR\\_ENDP](#page-1158-0) register.

#### DATA phase:

- 1. Receive the first data packet from the device.
- 2. Raise RX timeout error if the device doesn't reply.
- 3. If this is *not* an Isochronous endpoint and the data PID does not match the buffer control register, raise [SIE\\_STATUS](#page-1161-0).DATA\_SEQ\_ERROR (isochronous data is always sent with a DATA0 pid).

#### ACK phase:

1. Send ACK to device.

#### STATUS phase:

- 1. Set the [BUFF\\_STATUS](#page-1162-0) bit and update the buffer control register.
- 2. Set FULL, DATA\_PID, WR\_LEN, and LAST\_BUFF if applicable.
- 3. If this is the last buffer in the transfer, set TRANS\_COMPLETE.

#### CONTROL phase (continued):

The host state machine performs IN transactions until LAST\_BUFF is seen in the buffer\_control register.

If the host is in double buffered mode, the host controller toggles between the BUF0 and BUF1 sections of the buffer control register.

Otherwise, the controller reads the buffer control register for buffer 0, then waits for FULL to be clear and AVAILABLE to be set before starting the next IN transaction, waiting in the CONTROL phase.

If the host receives a zero length packet, the device has no more data. The host state machine stops listening for more data regardless of if the LAST\_BUFF flag was set or not. To detect this from host software, check BUFF\_DONE for a data length of 0 in the buffer control register.

#### **12.7.3.9.3. OUT**

An OUT transfer is triggered with the SEND\_TRANS bit set when the START\_TRANS bit is set. This may be preceded by a SETUP packet if the SEND\_SETUP bit was set.

#### CONTROL phase:

- 1. Read the EPx control register to get endpoint information (same as [Section 12.7.3.9.2](#page-1150-0)).
- 2. Read the EPx buffer control register to get the transfer length and data PID. AVAILABLE and FULL must be set before the transfer can start.

#### TOKEN phase

1. Send an OUT packet to the device. The target device address and endpoint come from the [ADDR\\_ENDP](#page-1158-0) register.

#### DATA phase:

- 1. Send the first data packet to the device. If the endpoint type is isochronous, there is no ACK phase, so the host controller goes straight to status phase. If ACK is received, go to status phase. Otherwise:
  - If the host receives no reply, raise [SIE\\_STATUS.](#page-1161-0)RX\_TIMEOUT.
  - If the host receives NAK, raise [SIE\\_STATUS](#page-1161-0).NAK\_REC and send the data packet again.
  - If the host receives STALL, raise [SIE\\_STATUS](#page-1161-0).STALL\_REC and go to idle.

#### STATUS phase:

1. Set the [BUFF\\_STATUS](#page-1162-0) bit and update the buffer control register. FULL will be set to 0. TRANS\_COMPLETE will be set if this is the last buffer in the transfer.

#### CONTROL phase (continued):

1. If this isn't the last buffer in the transfer, wait for FULL and AVAILABLE to be set in the EPx buffer control register again.

#### **12.7.3.9.4. Interrupt Endpoints**

The host controller can poll interrupt endpoints on a maximum of 15 endpoints. To enable interrupt endpoints, the programmer must:

- Pick the next free interrupt endpoint slot on the host controller (starting at 1, to a maximum of 15).
- Program the appropriate endpoint control register and buffer control register like you would with a normal IN or OUT transfer. Because interrupt endpoints are single-buffered, the BUF1 part of the buffer control register is invalid.
- Set the address and endpoint of the device in the appropriate ADDR\_ENDP register ([ADDR\\_ENDP1](#page-1158-1) to [ADDR\\_ENDP15\)](#page-1158-1).

If the device is Low Speed but attached to a Full Speed hub, the preamble bit should be set. The endpoint direction bit should also be set.

• Set the corresponding interrupt endpoint active bit (one of bits 1 through 15) in [INT\\_EP\\_CTRL](#page-1162-1).

Typically, interrupt endpoints use an IN transfer. The host might poll a USB hub to see if the state of any of its ports have changed. If there is no change, the hub replies with a NAK to the controller, and nothing happens. Similarly, a mouse replies with a NAK unless the mouse has been moved since the last time the interrupt endpoint was polled.

Interrupt endpoints are polled by the controller once a SOF packet has been sent by the host controller.

The controller loops from 1 to 15 and attempts to poll any interrupt endpoint with the EP\_ACTIVE bit set to 1 in [INT\\_EP\\_CTRL.](#page-1162-1) The controller will then read the endpoint control register and the buffer control register to see if there is an available buffer (i.e. FULL + AVAILABLE if an OUT transfer and NOT FULL + AVAILABLE for an IN transfer). If not, the controller will move onto the next interrupt endpoint slot.

If there is an available buffer, the transfer is dealt with the same as a normal IN or OUT transfer and the BUFF\_DONE flag in [BUFF\\_STATUS](#page-1162-0) will be set when the interrupt endpoint has a valid buffer.

#### <span id="page-1152-1"></span>**12.7.3.10. VBUS Control**

The USB controller can be connected to GPIO pins (see [Chapter 9\)](#page-584-0) for the following VBUS controls:

- **VBUS enable**, used to enable VBUS in host mode. Set in [SIE\\_CTRL.](#page-1160-0)
- **VBUS detect**, used to detect that VBUS is present in device mode. Set via a bit in [SIE\\_STATUS.](#page-1161-0) Can also raise a VBUS\_DETECT interrupt enabled in [INTE.](#page-1173-0)
- **VBUS overcurrent**, used to detect an overcurrent event. Applicable to both device and host. VBUS overcurrent is a bit in [SIE\\_STATUS.](#page-1161-0)

It is not necessary to connect up any of these pins to GPIO. The host can permanently supply VBUS and detect a device being connected when either the DP or DM pin is pulled high. VBUS detect can be forced in [USB\\_PWR.](#page-1169-0)

