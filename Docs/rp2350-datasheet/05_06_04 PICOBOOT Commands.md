# 5.6.4 PICOBOOT Commands

The two bulk endpoints are used for sending commands and retrieved successful command results. All commands are exactly 32 bytes (see [Table 457\)](#page-403-4) and sent to the BULK\_OUT endpoint.

*Table 457. PICOBOOT Command Definition*

<span id="page-403-4"></span>

| Offset | Name   | Description                                                                                   |
|--------|--------|-----------------------------------------------------------------------------------------------|
| 0x00   | dMagic | The value 0x431fd10b                                                                          |
| 0x04   | dToken | A user provided token to identify this request by                                             |
| 0x08   | bCmdId | The ID of the command. Note that the top bit indicates data transfer direction<br>(0x80 = IN) |

| Offset | Name            | Description                                                                   |
|--------|-----------------|-------------------------------------------------------------------------------|
| 0x09   | bCmdSize        | Number of bytes of valid data in the args field                               |
| 0x0a   | reserved        | 0x0000                                                                        |
| 0x0c   | dTransferLength | The number of bytes the host expects to send or receive over the bulk channel |
| 0x10   | args            | 16 bytes of command-specific data padded with zeros                           |

If a command sent is invalid or not recognised, the bulk endpoints will be stalled. Further information will be available via the GET\_COMMAND\_STATUS request (see [Section 5.6.5.2\)](#page-409-0).

Following the initial 32 byte packet, if dTransferLength is non-zero, then that many bytes are transferred over the bulk pipe and the command is completed with an empty packet in the opposite direction. If dTransferLength is zero then command success is indicated by an empty IN packet.

The following commands are supported (note common fields dMagic, dToken, and reserved are omitted for clarity)

