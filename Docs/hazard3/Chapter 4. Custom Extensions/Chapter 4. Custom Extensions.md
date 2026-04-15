# <span id="page-43-0"></span>**Chapter 4. Custom Extensions**

Hazard3 implements a small number of custom extensions. All are optional: custom extensions are only included if the relevant feature flags are set to 1 when instantiating the processor ([Configuration Parameters\)](#page-16-0). Hazard3 is always a *conforming* RISC-V implementation, and when these extensions are disabled it is also a *standard* RISC-V implementation.

If any one of these extensions is enabled, the x bit in [misa](#page-25-2) is set to indicate the presence of a nonstandard extension.

