# 12.6.2 Configuring Channels

Each channel has four control/status registers:

- READ\_ADDR [\(CH0\\_READ\\_ADDR](#page-1122-0)) is the address of the next memory location to read.
- WRITE\_ADDR ([CH0\\_WRITE\\_ADDR](#page-1122-1)) is the address of the next memory location to write.
- TRANS\_COUNT ([CH0\\_TRANS\\_COUNT](#page-1123-0)) shows the number of transfers remaining in the current transfer sequence and programs the number of transfers in the next transfer sequence (see [Section 12.6.2.2\)](#page-1094-0).
- CTRL ([CH0\\_CTRL\\_TRIG\)](#page-1124-0) configures all other aspects of the channel's behaviour, enables/disables the channel, and provides completion status.

To directly instruct the DMA channel to perform a data transfer, software writes to these four registers, and then triggers the channel [\(Section 12.6.3](#page-1095-0)). To make the DMA more autonomous, you can also program one DMA channel to write to another channel's configuration registers, queueing up many transfer sequences in advance.

All four are live registers; they update their status continuously as the channel progresses.

