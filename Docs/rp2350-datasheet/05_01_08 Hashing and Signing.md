# 5.1.8 Hashing and Signing

Any block may be **hashed** or **signed**. A hashed block stores the image hash value (see [Section 5.9.2.3](#page-418-0)). At runtime, the bootrom calculates a hash and compares it to the stored hash to determine if the block is valid. Hashes guard against

corruption of an image, but do not provide any security guarantees.

On a secured RP2350, a hash is not sufficient for an image to be considered valid. All images must have a **signature**: a hash encrypted by a private key, plus metadata (also covered by the hash) describing how the hash was generated. This signature is stored as part of an IMAGE\_DEF block. An image with a signature in its IMAGE\_DEF block is called a **signed image**.

## **NOTE**

For background on signatures and boot keys, see the introduction to secure boot in the security chapter ([Section](#page-813-2) [10.1.1](#page-813-2)).

To verify a signed image, the bootrom decrypts the hash stored in the signature using a *secp256k1* public key. The bootrom also computes its own hash of the image and compares its measured hash value with the one in the signature.

The public key is also stored in the block via a SIGNATURE item (see [Section 5.9.2.4](#page-419-1)): this key's (SHA-256) hash must match one of the boot key hashes stored in OTP locations [BOOTKEY0\\_0](#page-1315-1) onwards. Up to four public keys can be registered in OTP, with the count defined by [BOOT\\_FLAGS1](#page-1306-0).KEY\_VALID and [BOOT\\_FLAGS1.](#page-1306-0)KEY\_INVALID. A hash of a key is also referred to as a **key fingerprint**.

The data to be hashed is defined by a HASH\_DEF item (see [Section 5.9.2.2](#page-418-1)), which indicates the type of hash. It also indicates how much of the block itself is to be hashed. For a signed block, the hash *must* contain all contents of the block up to the final SIGNATURE item.

To be useful your hash or signature must cover actual image data in addition to the metadata stored in the block. The block's [load map](#page-358-0) item specifies which data the bootrom hashes during hash or signature verification.

The above discussion mostly applies to IMAGE\_DEFs. On a secured RP2350 with the [BOOT\\_FLAGS0.](#page-1304-1)SECURE\_PARTITION\_TABLE flag set, the bootrom also enforces signatures on PARTITION\_TABLEs.

## <span id="page-358-0"></span>**5.1.9. Load Maps**

A **load map** describes regions of the binary and what to do with them before the bootrom runs the binary.

The load map supports:

- Copying portions of the binary from flash to RAM (or to the XIP cache)
- Clearing parts of RAM (either .bss clear, or erasing uninitialised memory during secure boot)
- Defining what parts of the binary are included in a [hash or signature](#page-357-2)
- Preventing the flushing of the XIP cache when to keep loaded lines pinned up to the point the binary starts

For full details on the LOAD\_MAP item type of IMAGE\_DEF blocks, see [Section 5.9.3.2](#page-420-0).

When booting a signed binary from flash, it is desirable to load the signed data and code into RAM *before* checking the signature and subsequently executing it. Otherwise, an adversary could replace the flash device in between the signature check and execution, subverting the check. For this reason, the load map also serves as a convenient description of what to include in a hash or signature. The load map itself is covered by the hash or signature, and the entire metadata [block](#page-356-2) is loaded into RAM before processing, so it is not itself subject to this time-of-check versus timeof-use concern.

