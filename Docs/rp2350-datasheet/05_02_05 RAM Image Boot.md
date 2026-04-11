# 5.2.5 RAM Image Boot

The bootrom is directed (via values in the watchdog registers) to boot into an image in SRAM or XIP SRAM. The two parameters indicate the start and size of the region to search for a block loop containing a valid (and correctly signed if necessary) IMAGE\_DEF. These are passed as parameter 0/1, in watchdog scratch 2/3.

If the image to be booted is contained in XIP SRAM, the XIP SRAM must be pinned in place by the bootrom prior to launch. For this reason, if you are using XIP SRAM for your binaries, you must add a special entry to the LOAD\_MAP item (see [Section 5.9.3.2](#page-420-0)).

