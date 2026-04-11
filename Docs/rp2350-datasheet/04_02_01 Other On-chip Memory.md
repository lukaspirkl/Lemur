# 4.2.1 Other On-chip Memory

Besides the 520 kB main memory, there are two other dedicated RAM blocks that may be used in some circumstances:

- Cache lines can be individually pinned within the XIP address space for use as SRAM, up to the total cache size of 16 kB (see [Section 4.4.1.3](#page-344-0)). Unpinned cache lines remain available for transparent caching of XIP accesses.
- If USB is not used, the USB data DPRAM can be used as a 4 kB memory starting at 0x50100000.

There is also 1 kB of dedicated boot RAM, hardwired to Secure access only, whose contents and layout is defined by the boot ROM — see [Chapter 5](#page-353-0).

4.2. SRAM **338**

# **NOTE**

Memory in the peripheral address space (addresses starting with 0x4, 0x5 or 0xd) does not support code execution. This includes USB RAM and boot RAM. These address ranges are made IDAU-Exempt to simplify assigning peripherals to security domains using ACCESSCTRL, and consequently must be made non-executable to avoid the possibility of Non-secure-writable, Secure-executable memory.

