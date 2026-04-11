# 12.13.3 Data Size and Endianness

Data is sent in message blocks of 512 bits, padded as described in [Section 12.13.1](#page-1219-0). The SHA-256 accelerator updates its 256-bit output state for each input block. The SHA-256 algorithm is defined in terms of big-endian message words, but this accelerator provides a byte swap function via [CSR.](#page-1220-1)BSWAP to support little-endian data. BSWAP is set by default. For more information, see the register descriptions.

[WDATA](#page-1222-1) supports 8-bit, 16-bit and 32-bit writes. The bus interface accumulates 8 and 16-bit writes in a 32-bit shift register before passing them into the SHA-256 algorithm core. This means you must take care when mixing writes of different sizes, because taking the shift register level from less than to greater than 32 bits in a single write will silently drop data. You can avoid this issue by not mixing [WDATA](#page-1222-1) write sizes within a single SHA-256 message block (64 bytes).

