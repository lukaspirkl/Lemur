# 5.1.10 Packaged Binaries

As described in [Section 5.1.9,](#page-358-0) signed binaries in flash on a secured RP2350 are commonly loaded from flash into RAM, go through signature verification in RAM, and then execute from the verified version in RAM.

A **packaged binary** is a binary stored in flash that runs entirely from RAM. The binary is likely compiled to run from RAM as a RAM-only binary (unfortunately named no\_flash in SDK parlance), but subsequently post-processed for flash

residence. The bootrom **unpackages** the binary into RAM before execution.

As part of the packaging process, tooling like picotool adds a LOAD\_MAP that tells the bootrom which parts of the flashresident image it must load into RAM, and where to put them. This tooling may also [hash or sign](#page-357-2) the binary in the same step. In this case, the bootrom hashes the data it loads as it unpackages the binary, as well as relevant metadata such as the LOAD\_MAP itself. The bootrom compares the resulting hash to the precomputed hash or signature in the IMAGE\_DEF to verify the unpackaged contents in RAM before running those contents.

Compare this with RP2040, where a flash-resident binary which executes from RAM (a copy\_to\_ram binary in SDK parlance) must begin by executing from flash, then copy itself to RAM before continuing from there. In the RP2040 case, the loader itself (or rather the SDK crt0) executes in-place in flash to perform the copy. This makes it impossible to perform any trustworthy level of verification, because the loader itself executes in untrusted memory.

