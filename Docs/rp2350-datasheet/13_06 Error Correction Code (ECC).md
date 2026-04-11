# 13.6 Error Correction Code (ECC)

ECC-protected rows store data in the following structure, accessible through a raw alias:

- Bits 23:22: bit repair by polarity (BRP) flag
- Bits 21:16: modified Hamming ECC code
- Bits 15:0 (the 16 LSBs): data

RP2350 stores the following error correction data in the 8 MSBs of each 24-bit row:

- a 6-bit modified Hamming code ECC, providing single-error-correct and double-error-detect capabilities
- 2 bits of bit repair by polarity (BRP), which supports inverting the entire row at programming time to repair a single set bit that should be clear

Writes first encode ECC, then BRP. Reads first decode BRP, then ECC. When reading through an ECC data alias ([Section](#page-1265-1) [13.1\)](#page-1265-1), hardware performs correction transparently. ECC programming operations (writes) automatically generate ECC bits when you use the bootrom otp\_access API [\(Section 5.4.8.21\)](#page-393-2).

ECC is not suitable for data that mutates one bit at a time, since the ECC value is derived from the entire 16-bit data value. When storing data without ECC, use another form of redundancy, such as 3-way majority vote.

