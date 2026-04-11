# 4.4.2 QSPI Memory Interface (QMI)

Uncached accesses and cache misses require access to external memory. The QSPI memory interface (QMI) provides this access, as documented in [Section 12.14.](#page-1223-0) The QMI supports:

- Up to two external QSPI devices, with separate chip selects and shared clock/data pins
  - Banked configuration registers, including different SCK frequencies and QSPI opcodes
- Memory-mapped reads and writes (writes must be enabled via [CTRL.](#page-346-2)WRITABLE\_M0[/CTRL](#page-346-2).WRITABLE\_M1)
- Serial/dual/quad-SPI transfer formats
- SCK speeds as high as clk\_sys
- 8/16/32-bit accesses for uncached accesses, and 64-bit accesses for cache line fills
- Automatic chaining of sequentially addressed accesses into a single QSPI transfer
- Address translation (4 × 4 MB windows per QSPI device)
  - Flash storage addresses can differ from runtime addresses, e.g. for multiple OTA upgrade image slots
  - Allows code and data segments, or Secure and Non-secure images, to be mapped separately
- Direct-mode FIFO interface for programming and configuring external QSPI devices

XIP accesses via the two cache AHB ports, and from the DMA streaming hardware, arbitrate for access to the QMI. A separate APB port configures the QMI.

The QMI is a new memory interface designed for RP2350, replacing the SSI peripheral on RP2040.

