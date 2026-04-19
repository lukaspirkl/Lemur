## <span id="page-57-0"></span> A.9. Branch Predictor

Hazard3 includes a minimal branch predictor, to accelerate tight loops:

- The instruction frontend remembers the last taken, backward branch
- If the same branch is seen again, it is predicted taken
- All other branches are predicted nontaken
- If a predicted-taken branch is not taken, the predictor state is cleared, and it will be predicted nontaken on its next execution.

Correctly predicted branches execute in one cycle: the frontend is able to stitch together the two nonsequential fetch paths so that they appear sequential. Mispredicted branches incur a penalty cycle, since a nonsequential fetch address must be issued when the branch is executed.

<span id="page-57-1"></span>[<sup>\[1\]</sup>](#page-52-2) A jump or branch to a 32-bit instruction which is not 32-bit-aligned requires one additional cycle, because two naturally aligned bus cycles are required to fetch the target instruction.

<span id="page-57-2"></span>[<sup>\[2\]</sup>](#page-53-1) If an instruction in stage 2 (e.g. an add) uses data from stage 3 (e.g. a lw result), a 1-cycle bubble is inserted between the pair. A load data → store data dependency is *not* an example of this, because data is produced and consumed in stage 3. However, load data → load address *would* qualify, as would e.g. sc.w → beqz.

<span id="page-57-3"></span>[<sup>\[3\]</sup>](#page-54-3) A pipeline bubble is inserted between lr.w/sc.w and an immediately-following lr.w/sc.w/amo\*, because the AHB5 bus standard does not permit pipelined exclusive accesses. A stall would be inserted between lr.w and sc.w anyhow, so the local monitor can be updated based on the lr.w data phase in time to suppress the sc.w address phase.

<span id="page-57-4"></span>[<sup>\[4\]</sup>](#page-54-4) AMOs are issued as a paired exclusive read and exclusive write on the bus, at the maximum speed of 2 cycles per access, since the bus does not permit pipelining of exclusive reads/writes. If the write phase fails due to the global monitor reporting a lost reservation, the instruction loops at a rate of 4 cycles per loop, until success. If the read reservation is refused by the global monitor, the instruction generates a Store/AMO Fault exception, to avoid an infinite loop.

<span id="page-57-5"></span>[<sup>\[5\]</sup>](#page-56-2) The single-register variants of cm.popret and cm.popretz take the same number of cycles as the two-register variants, because of an internal load-use dependency on the loaded return address.
