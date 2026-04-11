# 12.11.8 List of FIFO Registers

The FIFO registers start at a base address of 0x50600000 (defined as [HSTX\\_FIFO\\_BASE](#page-33-0) in the SDK).

*Table 1257. List of HSTX\_FIFO registers*

<span id="page-1209-5"></span>

| Offset | Name | Info                 |
|--------|------|----------------------|
| 0x0    | STAT | FIFO status          |
| 0x4    | FIFO | Write access to FIFO |

# <span id="page-1209-3"></span>**[HSTX\\_FIFO](#page-1209-5): STAT Register**

**Offset**: 0x0 **Description**

FIFO status

*Table 1258. STAT Register*

| Bits  | Description                                        | Type | Reset |
|-------|----------------------------------------------------|------|-------|
| 31:11 | Reserved.                                          | -    | -     |
| 10    | WOF: FIFO was written when full. Write 1 to clear. | WC   | 0x0   |
| 9     | EMPTY                                              | RO   | -     |
| 8     | FULL                                               | RO   | -     |
| 7:0   | LEVEL                                              | RO   | 0x00  |

# <span id="page-1209-4"></span>**[HSTX\\_FIFO](#page-1209-5): FIFO Register**

**Offset**: 0x4

*Table 1259. FIFO Register*

| Bits | Description          | Type | Reset      |
|------|----------------------|------|------------|
| 31:0 | Write access to FIFO | WF   | 0x00000000 |

