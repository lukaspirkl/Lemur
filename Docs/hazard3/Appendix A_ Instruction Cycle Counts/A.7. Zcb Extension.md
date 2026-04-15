## <span id="page-56-0"></span>**A.7. Zcb Extension**

Similarly to the C extension, this extension contains 16-bit variants of common 32-bit instructions:

- RV32I base ISA: lbu, lh, lhu, sb, sh, zext.b (alias of andi), not (alias of xori)
- Zbb extension: sext.b, zext.h, sext.h
- M extension: mul

They perform identically to their 32-bit counterparts.

