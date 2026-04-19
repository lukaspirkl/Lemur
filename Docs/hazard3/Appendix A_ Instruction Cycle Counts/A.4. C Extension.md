## <span id="page-54-1"></span> A.4. C Extension

All C extension 16-bit instructions are aliases of base RV32I instructions. On Hazard3, they perform identically to their 32-bit counterparts.

A consequence of the C extension is that 32-bit instructions can be non-naturally-aligned. This has no penalty during sequential execution, but branching to a 32-bit instruction that is not 32-bitaligned carries a 1 cycle penalty, because the instruction fetch is cracked into two naturally-aligned bus accesses.

