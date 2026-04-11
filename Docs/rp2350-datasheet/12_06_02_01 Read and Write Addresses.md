# 12.6.2.1 Read and Write Addresses

READ\_ADDR and WRITE\_ADDR contain the address the channel will next read from, and write to, respectively. These registers update automatically after each read/write access, incrementing to the next read/write address as required. The size of the increment varies according to:

- the transfer size: 1, 2 or 4 byte bus accesses as per [CH0\\_CTRL\\_TRIG.](#page-1124-0)DATA\_SIZE
- the increment enable for each address register: [CH0\\_CTRL\\_TRIG](#page-1124-0).INCR\_READ and [CH0\\_CTRL\\_TRIG](#page-1124-0).INCR\_WRITE
- the increment direction: [CH0\\_CTRL\\_TRIG.](#page-1124-0)INCR\_READ\_REV and [CH0\\_CTRL\\_TRIG](#page-1124-0).INCR\_WRITE\_REV

Software should generally program these registers with new start addresses each time a new transfer sequence starts. If READ\_ADDR and WRITE\_ADDR are not reprogrammed, the DMA will use the current values as start addresses for the next transfer. For example:

- If the address does not increment (e.g. it is the address of a peripheral FIFO), and the next transfer sequence is to/from that *same* address, there is no need to write to the register again.
- When transferring to/from a consecutive series of buffers in memory (e.g. scattering and gathering), an address register will already have incremented to the start of the next buffer at the completion of a transfer.

By not programming all four CSRs for each transfer sequence, software can use shorter interrupt handlers, and more compact control block formats when used with channel chaining (see register aliases in [Section 12.6.3.1,](#page-1096-0) chaining in [Section 12.6.3.2](#page-1097-2)).

#### **12.6.2.1.1. Address Alignment**

READ\_ADDR and WRITE\_ADDR must be aligned to the transfer size, specified in [CH0\\_CTRL\\_TRIG.](#page-1124-0)DATA\_SIZE. For 32-bit transfers, the address must be a multiple of four, and for 16-bit transfers, the address must be a multiple of two. Software is responsible for correctly aligning addresses written to READ\_ADDR and WRITE\_ADDR: the DMA does not enforce alignment.

If software initially writes a correctly aligned address, the address will remain correctly aligned throughout the transfer sequence, because the DMA always increments READ\_ADDR and WRITE\_ADDR by a multiple of the transfer size. Specifically, it increments by transfer size times -1, 0, 1 or 2, depending on the values of [CH0\\_CTRL\\_TRIG](#page-1124-0).INCR\_READ, [CH0\\_CTRL\\_TRIG](#page-1124-0).INCR\_WRITE, [CH0\\_CTRL\\_TRIG.](#page-1124-0)INCR\_READ\_REV and [CH0\\_CTRL\\_TRIG](#page-1124-0).INCR\_WRITE\_REV.

The DMA MPU and system-level bus security filters perform protection checks on the lowest byte address of all bytes transferred on a given cycle (i.e. to the present value of READ\_ADDR/WRITE\_ADDR). RP2350 memory hardware ensures unaligned bus accesses do not cause data to be read/written from the other side of a protection boundary. This means that unaligned access can not be used to violate the memory protection model. Other than this, the result of an unaligned access is unspecified.

#### <span id="page-1094-0"></span>**12.6.2.2. Transfer Count**

Reading TRANS\_COUNT ([CH0\\_TRANS\\_COUNT](#page-1123-0)) returns the number of transfers remaining in the current transfer sequence. This value updates continuously as the channel progresses. Writing to TRANS\_COUNT sets the length of the *next* transfer sequence. Up to 2<sup>28</sup>-1 transfers can be performed in one sequence (0x0fffffff, approximately 256 million).

Each time the channel starts a new transfer sequence, the most recent value written to TRANS\_COUNT is copied to the live transfer counter, which will then start to decrement again as the new transfer sequence makes progress. For debugging purposes, the DBG\_TCR (TRANS\_COUNT reload value) registers display the last value written to each channel's TRANS\_COUNT.

If the channel is triggered multiple times without intervening writes to TRANS\_COUNT, it performs the same number of transfers each time. For example, when chained to, one channel might load a fixed-size control block into another channel's CSRs. TRANS\_COUNT would be programmed once by software, and then reload automatically every time.

Alternatively, TRANS\_COUNT can be written with a new value before starting each transfer sequence. If TRANS\_COUNT is the channel trigger (see [Section 12.6.3.1\)](#page-1096-0), the channel will start immediately, and the value just written will be used, *not* the value currently in the reload register.

# **NOTE**

The TRANS\_COUNT is the number of *transfers* to be performed. The total number of bytes transferred is TRANS\_COUNT times the size of each transfer in bytes, given by CTRL.DATA\_SIZE.

#### **12.6.2.2.1. Count Modes**

The four most-significant bits of TRANS\_COUNT contain the MODE field ([CH0\\_TRANS\\_COUNT](#page-1123-0).MODE), which modifies the counting behaviour of TRANS\_COUNT. Mode 0x0 is the default: TRANS\_COUNT decrements once for every bus transfer, and the channel halts once TRANS\_COUNT reaches zero and all in-flight transfers have finished. The value of 0x0 is chosen for backward-compatibility with RP2040 software, which expects the TRANS\_COUNT register to contain a 32-bit count rather than a 4-bit mode and a 28-bit count. There are few use cases for a *finite* number of transfers greater than 2<sup>28</sup>, which is why the four most-significant bits have been reallocated for use with endless transfers.

Mode 0x1, TRIGGER\_SELF, behaves the same as mode 0x0, except that rather than halting upon completion, the channel immediately re-triggers itself. This is equivalent to a trigger performed by any other mechanism ([Section 12.6.3\)](#page-1095-0): TRANS\_COUNT is reloaded, and the channel resumes from the current READ\_ADDR and WRITE\_ADDR addresses. A completion interrupt is still raised (if CTRL.IRQ\_QUIET is not set) and the specified CHAIN\_TO operation is still performed. The main use for this mode is streaming through SRAM ring buffers, where some action is required at regular intervals, for example requesting the processor to refill an audio buffer once it is half-empty.

Mode 0xf, ENDLESS, disables the decrement of TRANS\_COUNT. This means a channel will generally run indefinitely without pause, though triggering a channel with a mode of 0xf and a count of 0x0 will result in the channel halting immediately.

All other values are reserved for future use and their effect is unspecified.

#### **12.6.2.3. Control/Status**

The CTRL register [\(CH0\\_CTRL\\_TRIG\)](#page-1124-0) has more, smaller fields than the other 3 registers. Among other things, CTRL is used to:

- Configure the size of this channel's data transfers, via the DATA\_SIZE field. Reads are always the same size as writes.
- Configure if and how READ\_ADDR and WRITE\_ADDR increment after each read or write, via the INCR\_READ, INCR\_READ\_REV, INCR\_WRITE, INCR\_WRITE\_REV, RING\_SEL and RING\_SIZE fields. Ring transfers are available, where one of the address pointers wraps at some power-of-2 boundary.
- Select another channel (or none) to trigger when this channel completes, via the CHAIN\_TO field.
- Select a peripheral data request (DREQ) signal to pace this channel's transfers, via the TREQ\_SEL field.
- See when the channel is idle, using the BUSY flag.
- See if the channel has encountered a bus error the READ\_ERROR and WRITE\_ERROR flags, or the combined error status in the AHB\_ERROR flag.

