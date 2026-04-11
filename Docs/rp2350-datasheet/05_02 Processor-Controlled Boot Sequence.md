# 5.2 Processor-Controlled Boot Sequence

The bootrom contains the first instructions the processors execute following a reset. Both processors enter the bootrom at the same time, and in the same location, but the boot sequence runs mostly on core 0.

Core 1 redirects very early in the boot sequence to a low-power state where it waits to be launched, after boot, by user software on core 0. If core 1 is unused, it remains in this low-power state.

#### **Source Code Reference**

The sequence described in this section is implemented on Arm by the source files arm8\_bootrom\_rt0.S and varm\_boot\_path.c in the bootrom source code repository. RISC-V cores instead begin from riscv\_bootrom\_rt0.S, but share the boot path implementation with Arm.

