# 5.9.3 Image Definition Items

#### **5.9.3.1. IMAGE\_DEF item**

The IMAGE\_DEF item must be the first item within an Image Definition:

| Word | Bytes | Value                                                                 |
|------|-------|-----------------------------------------------------------------------|
| 0    | 1     | 0x42 (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_1BS_IMAGE_TYPE) |
|      | 1     | 0x01 (Block size in words)                                            |
|      | 2     | image_type_flags                                                      |

The flags are defined in the SDK in [picobin.h](https://github.com/raspberrypi/pico-sdk/blob/master/src/common/boot_picobin_headers/include/boot/picobin.h) [in the SDK](https://github.com/raspberrypi/pico-sdk/blob/master/src/common/boot_picobin_headers/include/boot/picobin.h), but are summarised here:

| Bits | Field      | Values                                                                 |
|------|------------|------------------------------------------------------------------------|
| 0-3  | Image Type | 0 IMAGE_TYPE_INVALID                                                   |
|      |            | 1 IMAGE_TYPE_EXE : Image is executable                                 |
|      |            | 2 IMAGE_TYPE_DATA : Image is valid, but is not intended to be executed |
|      |            | 3 reserved                                                             |

| Bits  | Field                                                                                                       | Values                                                                                                                                        |  |
|-------|-------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------|--|
|       | The remaining bits are specific to the Image Type Values are only currently defined for the EXE Image Type: |                                                                                                                                               |  |
| 4-5   | EXE Security                                                                                                | 0 EXE_SECURITY_UNSPECIFIED<br>1 EXE_SECURITY_NS : Image runs in Non-secure mode<br>2 EXE_SECURITY_S : Image runs in Secure mode<br>3 reserved |  |
| 6-7   | reserved                                                                                                    | 0                                                                                                                                             |  |
| 8-10  | EXE CPU                                                                                                     | 0 EXE_CPU_ARM : Image is for the Arm architecture<br>1 EXE_CPU_RISCV : Image is for the RISC-V architecture<br>2-7 reserved                   |  |
| 11    | reserved                                                                                                    | 0                                                                                                                                             |  |
| 12-14 | EXE CHIP                                                                                                    | 0 EXE_CHIP_RP2040<br>1 EXE_CHIP_RP2350<br>2-7 reserved                                                                                        |  |
| 15    | EXE TBYB                                                                                                    | 0 not set<br>1 EXE_TBYB : Image is flagged for "Try Before You Buy"                                                                           |  |

#### <span id="page-420-0"></span>**5.9.3.2. LOAD\_MAP item**

Optional item with a similar representation to the ELF program header. This is used both to define the content to hash, and also to "load" data before image execution (e.g. a secure flash binary can be loaded into RAM prior to both signature check and execution).

The load map is a collection of runtime address, physical address, size and flags.

- 1. For a "packaged" binary, the information tells the bootrom where to load the code/data.
- 2. For a hashed or signed binary, the runtime addresses and size indicate code/data that must be included in the hash to be verified or signature checked.

![](_page_420_Figure_7.jpeg)

If the runtime\_address is in equal to the storage\_address, then data is never copied, it is just hashed in place.

Explanation of terms:

#### **physical address**

Where the data is stored in the logical address space of the image. For instance, the start of a flash image, even if stored in a partition, could have a physical address of 0x10000000. The closest ELF concept is LMA.

#### **runtime address**

The address of the data at runtime. The closest ELF concept is VMA.

#### **storage address**

an absolute location where the data is stored in flash. Not necessarily the same as physical address for flash when partitions are in use.

RP2350 uses physical addresses in the LOAD\_MAP, not storage addresses, since this data is written by a tool working on the ELF which will not necessarily know where the binary will finally be stored in flash.

This serves several purposes:

| Word  | Bytes              | Value                                                                                                                                                                                                                                                                                                                                                                                                                          |  |
|-------|--------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--|
| 0     | 1                  | 0x06 (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_LOAD_MAP)                                                                                                                                                                                                                                                                                                                                                                |  |
|       | 2                  | 1 + num_entries * 3 (Block size in words)                                                                                                                                                                                                                                                                                                                                                                                      |  |
|       | 1                  | absolute:1, num_entries:7                                                                                                                                                                                                                                                                                                                                                                                                      |  |
| 1-3   | Load Map Entry 0   |                                                                                                                                                                                                                                                                                                                                                                                                                                |  |
|       | 4                  | •<br>if absolute == 0<br>storage_start_address_rel (storage start address relative to the address of this word)<br>•<br>if absolute == 1<br>storage_start_address (absolute storage start address)<br>Note: If this value is 0x00000000 irrespective of the value of the absolute flag, then the runtime<br>address range is filled with zeros. In this case, the 32 bit size itself is hashed rather than size<br>zero bytes. |  |
|       | 4                  | runtime_start_address (absolute runtime start address)                                                                                                                                                                                                                                                                                                                                                                         |  |
|       | 4                  | •<br>if absolute == 0<br>size (of memory range in bytes)<br>•<br>if absolute == 1<br>storage_end_address (absolute storage end address)                                                                                                                                                                                                                                                                                        |  |
| (4-6) | (Load Map Entry 1) |                                                                                                                                                                                                                                                                                                                                                                                                                                |  |
|       | …                  | …                                                                                                                                                                                                                                                                                                                                                                                                                              |  |

#### **NOTE**

All addresses must be word aligned, and sizes a multiple of 4

#### **5.9.3.2.1. XIP Pinning via LOAD\_MAP**

Normally, when entering a binary, the XIP cache is un-pinned and flushed. This makes sense both for entering a flash binary, and for security purposes.

If, however, you have a non-flash binary with code or data in the XIP RAM address space, then you need to add a special LOAD\_MAP entry to indicate to the bootrom that the XIP contents should be pinned.

Any load-map entry (with storage\_address == runtime\_address) and a valid size of greater than zero will suffice, as for example in this simple load map:

| Word | Bytes            | Value                                                           |
|------|------------------|-----------------------------------------------------------------|
| 0    | 1                | 0x06 (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_LOAD_MAP) |
|      | 2                | 0x04 (Block size in words)                                      |
|      | 1                | 0x81 (absolute == 1, num_entries == 1)                          |
| 1-3  | Load Map Entry 0 |                                                                 |
|      | 4                | XIP_SRAM_BASE (storage_start_address)                           |
|      | 4                | XIP_SRAM_BASE (runtime_start_address)                           |
|      | 4                | 0x04 (size in bytes)                                            |

#### **5.9.3.3. VECTOR\_TABLE item**

Optional Arm only item for that specifies the location of the initial Arm vector table. The entry\_point/initial\_sp will be taken from here if present (unless there is also an ENTRY\_POINT Item). Note if there is no ENTRY\_POINT or VECTOR\_TABLE, Item, then a VECTOR\_TABLE at the start of the image is assumed.

| Word | Bytes | Value                                                                   |
|------|-------|-------------------------------------------------------------------------|
| 0    | 1     | 0x03 (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_1BS_VECTOR_TABLE) |
|      | 1     | 0x02 (Block size in words)                                              |
|      | 2     | 0x0000 (pad)                                                            |
| 1    | 4     | Vector table (runtime) address                                          |

![](_page_422_Figure_4.jpeg)

The VECTOR\_TABLE Item is ignored on RISC-V.

#### **5.9.3.4. ENTRY\_POINT item**

Optional item with info on initial PC, SP, and optionally the SP limit

| Word | Bytes | Value                                                                  |
|------|-------|------------------------------------------------------------------------|
| 0    | 1     | 0x44 (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_1BS_ENTRY_POINT) |
|      | 1     | 0x03 or 0x04 (Block size in words)                                     |
|      | 2     | 0x0000 (pad)                                                           |
| 1    | 4     | Inital PC (runtime) address (aka entry point)                          |
| 2    | 4     | Initial SP address (aka stack pointer)                                 |
| (3)  | 4     | Optional SP limit address (aka stack limit)                            |

#### <span id="page-422-0"></span>**5.9.3.5. ROLLING\_WINDOW\_DELTA item**

Optional item that allow for binaries that aren't intended to be run at 0x10000000. Note that this delta is in addition to the roll resulting from the binary being stored in a partition in flash.

| Word | Bytes | Value                                                                           |
|------|-------|---------------------------------------------------------------------------------|
| 0    | 1     | 0x05 (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_1BS_ROLLING_WINDOW_DELTA) |
|      | 1     | 0x02 (Block size in words)                                                      |
|      | 2     | 0x0000 (pad)                                                                    |
| 1    | 4     | signed 32 bit delta                                                             |

The delta is the number of bytes into the image that 0x10000000 should be mapped.

If positive, the delta must be a multiple of 4 kB, and allows for "skipping over" other data before the start of the binary. If negative, the delta must be a multiple of 4 MB, and allows for running flash binaries linked to run at 0x10400000, 0x01080000 and 0x010c0000 as well as the standard 0x10000000

# **NOTE**

The ROLLING\_WINDOW\_DELTA Item is ignored for non-flash binaries.

