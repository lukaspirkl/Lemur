# 5.9.2 Common Block Items

The following items may appear in a IMAGE\_DEF or a PARTITION\_TABLE block.

#### <span id="page-417-1"></span>**5.9.2.1. VERSION item**

A major/minor version number for the binary, 32 bits total, plus optionally a 16-bit rollback version and a list of OTP rows which can be read to determine the (thermometer-coded) minimum major rollback version which this device will allow to be installed. The major and minor are always present, whereas the rollback version and OTP row list are generally only included if rollback protection is required.

![](_page_417_Figure_8.jpeg)

The rollback version and OTP row list are only valid for IMAGE\_DEFs, and are ignored on a RP2350 that has not been secured.

If the number of OTP row entries is zero, there is no rollback version for this block.

| Word | Bytes | Value                                                                                  |
|------|-------|----------------------------------------------------------------------------------------|
| 0    | 1     | 0x48 (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_1BS_VERSION)                     |
|      | 1     | 2 + ((num_otp_row_entries != 0) + num_row_entries + 1) / 2                             |
|      | 1     | 0x00 (pad)                                                                             |
|      | 1     | num_otp_row_entries                                                                    |
| 1    | 2     | Minor Version                                                                          |
|      | 2     | Major Version                                                                          |
| (2)  | (2)   | Rollback version (if num_otp_entries != 0)                                             |
|      | (2)   | First 16-bit OTP Row index (if num_otp_entries != 0`)                                  |
| …    | …     | Remaining 16-OTP Row indexes (padded with a zero to make a word boundary if necessary) |

Each OTP row entry indicates the row number (1 through 4095 inclusive) of the first in a group of 3 OTP rows. The three OTP rows are each read as a 24 bit raw value, combined via a bitwise majority vote, and then the index of the mostsignificant 1 bit determines the version number. So, a single group of three rows can encode rollback versions from 0 to 23 inclusive, or, when all 24 bits are set, an indeterminate version of at least 24. Each additional OTP row index indicates a further group of 3 rows that increases the maximum version by 24.

There is no requirement for different OTP row entries to be contiguous in OTP. They should not overlap, though the

bootrom does not need to check this (the boot signing tool may).

![](_page_418_Figure_2.jpeg)

For this entry to be considered valid, the number of available bits in the indicated OTP rows must be *strictly greater than* the rollback version. This means that it is always possible to determine that the device's minimum rollback version is greater than the rollback version indicated in this block, even if we don't know the full list of OTP rows used by later major versions.

The major/minor version are used to disambiguate which is newer out of two binaries with the same major rollback version. For example, to select which A/B image to boot from. when no major rollback version is specified, A/B comparisons will treat the missing major version as zero, but no rollback check will be performed.

#### <span id="page-418-1"></span>**5.9.2.2. HASH\_DEF item**

Optional item with information about what how to hash:

| Word | Bytes | Value                                                                                 |  |
|------|-------|---------------------------------------------------------------------------------------|--|
| 0    | 1     | 0x47 (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_1BS_HASH_DEF)                   |  |
|      | 1     | 0x03 (size_lo)                                                                        |  |
|      | 1     | 0x00 (pad)                                                                            |  |
|      | 1     | 0x01 (PICOBIN_HASH_SHA-256)                                                           |  |
| 1    | 2     | Number of words of block hashed (not including HEADER word at the start of the block) |  |
|      | 2     | 0x0000 (pad)                                                                          |  |

block\_words\_hashed must include this item if using this item for a signature.

The most recent LOAD\_MAP item (see [Section 5.9.3.2\)](#page-420-0) that defines *what* to hash.

#### <span id="page-418-0"></span>**5.9.2.3. HASH\_VALUE item**

Optional item containing a hash value that can be used by the bootrom to verify the hash of an image or partition table when not using signatures.

| Word | Bytes | Value                                                             |  |
|------|-------|-------------------------------------------------------------------|--|
| 0    | 1     | 0x09 (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_HASH_VALUE) |  |
|      | 1     | 0x01 + n where n is the number of hash words included (1-8)       |  |
|      | 2     | 0x0000 (pad)                                                      |  |
| 1    | 4     | Hash Value (lowest significant 32 bits)                           |  |
| …    | …     | …                                                                 |  |
| n    | 4     | Hash Value (highest significant 32 bits)                          |  |

#### **TIP**

Whilst a SHA-256 hash is 8 words, you can include fewer (down to 1 word) to save space if you like, and only that many words will be compared against the full 8-word hash at runtime.

This HASH\_VALUE item is paired with the most recent HASH\_DEF item [\(Section 5.9.2.2\)](#page-418-1) which defines what is being hashed.

#### <span id="page-419-1"></span>**5.9.2.4. SIGNATURE item**

Optional item containing cryptographic signature that can be used by the bootrom to signature check the hashed contents of an image or partition table.

| Word | Bytes | Value                                                            |
|------|-------|------------------------------------------------------------------|
| 0    | 1     | 0x4b (size_flag == 0, item_type == PICOBIN_BLOCK_ITEM_SIGNATURE) |
|      | 1     | 0x21 (Block size in words)                                       |
|      | 1     | 0x00 (pad)                                                       |
|      | 1     | 0x01 (PICOBIN_SIGNATURE_SECP256K1)                               |
| 1    | 4     | Public Key (lowest significant 32 bits)                          |
| …    | …     | …                                                                |
| 16   | 4     | Public Key (highest significant 32 bits)                         |
| 17   | 4     | Signature of hash (lowest significant 32 bits)                   |
| …    | …     | …                                                                |
| 32   | 4     | Signature of hash (highest significant 32 bits)                  |

This SIGNATURE item is paired with the most recent HASH\_DEF item [\(Section 5.9.2.2](#page-418-1)) which defines what the hash value whose signature is checked.

