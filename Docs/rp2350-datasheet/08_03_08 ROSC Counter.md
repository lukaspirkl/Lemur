# 8.3.8 ROSC Counter

The COUNT register provides a method of managing short software delays. To use this method:

- 1. Write a value to the COUNT register. The register automatically begins to count down to zero at the ROSC frequency.
- 2. Poll the register until it reaches zero.

This is preferable to using NOPs in software loops because it is independent of the core clock frequency, the compiler, and the execution time of the compiled code.

