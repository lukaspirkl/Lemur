# 1.3 Why is the chip called RP2350?

*Figure 4. An explanation for the name of the RP2350 chip.*

![](_page_22_Figure_6.jpeg)

The post-fix numeral on RP2350 comes from the following,

- 1. Number of processor cores
  - **2** indicates a dual-core system
- 2. Loosely which type of processor
  - **3** indicates Cortex-M33 or Hazard3
- 3. Internal memory capacity:
  - **5** indicates at least 2<sup>5</sup> × 16 kB = 512 kB
  - RP2350 has 520 kB of main system SRAM
- 4. Internal storage capacity: (or 0 if no onboard nonvolatile storage)

- RP235**0** uses external flash
- RP235**4** has 2<sup>4</sup> × 128 kB = 2 MB of internal flash

# <span id="page-24-0"></span>Chapter 2. System Bus

#### <span id="page-24-1"></span>2.1. Bus Fabric

The RP2350 bus fabric routes addresses and data across the chip.

Figure 5 shows the high-level structure of the bus fabric. The main AHB5 crossbar routes addresses and data between its 6 upstream ports and 17 downstream ports, with up to six bus transfers taking place each cycle. All data paths are 32 bits wide. Memories connect to multiple dedicated ports on the main crossbar, for the best possible memory bandwidth. High-bandwidth AHB peripherals share a port on the crossbar. An APB bridge provides access to system control registers and lower-bandwidth peripherals. The SIO peripherals are accessed via a dedicated path from each processor.

Figure 5. RP2350 bus

<span id="page-24-2"></span>![](_page_24_Figure_6.jpeg)

The bus fabric connects 6 AHB5 managers, i.e. bus ports which generate addresses:

- Core 0: Instruction port (instruction fetch), and Data port (load/store access)
- Core 1: Instruction port (instruction fetch), and Data port (load/store access)
- DMA controller: Read port, Write port

The following 13 downstream ports are symmetrically accessible from all 6 upstream ports:

- Boot ROM (1 port)
- XIP (2 ports, striped)
- SRAM (10 ports, striped)

Additionally, the following 2 ports are accessible for processor load/store and DMA read/write only:

- 1 shared port for fast AHB5 peripherals: PIO0, PIO1, PIO2, USB, DMA control registers, XIP DMA FIFOs, HSTX FIFO, CoreSight trace DMA FIFO
- 1 port for the APB bridge, to all APB peripherals and control registers

#### **NOTE**

Instruction fetch from peripherals is *physically disconnected*, to avoid this IDAU-Exempt region ever becoming both Non-secure-writable and Secure-executable. This includes USB RAM, OTP and boot RAM. See [Section 10.2.2](#page-817-0).

The SIO block, which was connected to the Cortex-M0+ IOPORT on RP2040, provides two AHB ports, each dedicated to load/store access from one core.

The six managers can access any six *different* crossbar ports simultaneously. So, at a system clock of 150 MHz, the maximum sustained bus bandwidth is 3.6 GB/s.

