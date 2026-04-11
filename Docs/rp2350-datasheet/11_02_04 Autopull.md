# 11.2.4 Autopull

**Autopull** (see [Section 11.5.4](#page-903-0)) allows the hardware to automatically refill the OSR in the majority of cases, with the state machine stalling if it tries to OUT from an empty OSR. This has two benefits:

- No instructions spent on explicitly pulling from FIFO at the right time
- Higher throughput: can output up to 32 bits on every single clock cycle, if the FIFO stays topped up

After configuring autopull, the above program can be simplified to the following, which behaves identically:

```
1 .program pull_example2
2 
3 loop:
4 out pins, 8
5 public entry_point:
6 jmp loop
```

Program wrapping [\(Section 11.5.2\)](#page-900-0) allows further simplification and, if desired, an output of 1 byte every system clock cycle.

```
1 .program pull_example3
2 
3 public entry_point:
4 .wrap_target
5 out pins, 8 [1]
6 .wrap
```

