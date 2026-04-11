# 12.12.3 Operation

To initiate TRNG generation, set the RND\_SRC\_EN bit in [RND\\_SOURCE\\_ENABLE](#page-1215-1). The TRNG will run until:

- It has successfully completed the generation of a random number.
- One, or more, of the internal entropy checking mechanisms indicates a failed run.

In either case, you can read the resultant status from [RNG\\_ISR.](#page-1214-1)

To generate TRNG block interrupts, set bits in [RNG\\_IMR](#page-1213-0). Use [RNG\\_ICR](#page-1214-2) to clear active interrupt status bits.

The EHR\_DATA[x] registers read 0 until successful generation has occurred, so the CPU cannot read random number results during generation,

After successful generation, read the last result register, EHR\_DATA[5] to clear all of the result registers. If the result fails an entropy check, no results are presented and the EHR\_DATA[x] registers all read as 0.

After TRNG generation and when not in use, the RND\_SRC\_EN bit should be cleared.

