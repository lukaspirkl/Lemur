# 5.3 Launching Code On Processor Core 1

As described in [Section 5.2,](#page-364-1) after reset, processor core 1 sleeps at start-up, and remains asleep until woken by core 0 via the SIO FIFOs.

If you are using the SDK then you can use the multicore\_launch\_core1() function to launch code on processor core 1. However this section describes the procedure to launch code on processor core 1 yourself.

The procedure to start running on processor core 1 involves both cores moving in lockstep through a state machine coordinated by passing messages over the inter-processor FIFOs. This state machine is designed to be robust enough to cope with a recently reset processor core 1 which may be anywhere in its boot code, up to and including going to sleep. As result, the procedure may be performed at any point after processor core 1 has been reset (either by system reset, or explicitly resetting just processor core 1).

The following C code describes the procedure:

```
// values to be sent in order over the FIFO from core 0 to core 1
// vector_table is value for VTOR register
// sp is initial stack pointer (SP)
// entry is the initial program counter (PC) (don't forget to set the thumb bit!)
const uint32_t cmd_sequence[] =
  {0, 0, 1, (uintptr_t) vector_table, (uintptr_t) sp, (uintptr_t) entry};
uint seq = 0;
do {
  uint cmd = cmd_sequence[seq];
  // always drain the READ FIFO (from core 1) before sending a 0
  if (!cmd) {
  // discard data from read FIFO until empty
  multicore_fifo_drain();
  // execute a SEV as core 1 may be waiting for FIFO space
  __sev();
  }
  // write 32 bit value to write FIFO
  multicore_fifo_push_blocking(cmd);
  // read 32 bit value from read FIFO once available
```

```
  uint32_t response = multicore_fifo_pop_blocking();
  // move to next state on correct response (echo-d value) otherwise start over
  seq = cmd == response ? seq + 1 : 0;
} while (seq < count_of(cmd_sequence));
```

