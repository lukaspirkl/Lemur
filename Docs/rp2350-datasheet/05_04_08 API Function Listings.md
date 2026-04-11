# 5.4.8 API Function Listings

#### <span id="page-383-1"></span>**5.4.8.1. bootrom\_state\_reset**

Code: 'S','R'

Signature: void bootrom\_state\_reset(uint32\_t flags)

Supported architectures: Arm-S, RISC-V

Resets internal bootrom state, based on the following flags:

- 0x0001 : STATE\_RESET\_CURRENT\_CORE Resets any internal bootrom state for the current core to a known state. This method should be called prior to calling any other bootrom APIs on the current core, and is called automatically by the bootrom during normal boot of core 0 or launch of code on core 1.
- 0x0002 : STATE\_RESET\_OTHER\_CORE Resets any internal bootrom state for the other core into a clean state. This is generally called by a debugger when resetting the state of one core via code running on the other.
- 0x0004 : STATE\_RESET\_GLOBAL\_STATE Resets all non core-specific state, including:
  - Disables access to bootrom APIs from Arm-NS (see also [set\\_ns\\_api\\_permission\(\)\)](#page-397-0).
  - Unlocks all boot locks [\(Section 5.4.4\)](#page-379-0).
  - Clears any Secure code callbacks. (see also [set\\_rom\\_callback\(\)](#page-397-2))

Note that the SDK calls this method on runtime initialisation to put the bootrom into a known state. This allows the program to function correctly if it is entered via a debugger, or otherwise without taking the usual boot path through the bootrom, which itself would reset the state.

#### <span id="page-383-2"></span>**5.4.8.2. chain\_image**

Code: 'C','I'

Signature: int chain\_image(uint8\_t \*workarea\_base, uint32\_t workarea\_size, int32\_t region\_base, uint32\_t region\_size)

Supported architectures: Arm-S, RISC-V. Note on RISC-V this function may require additional stack; see [Section 5.4.8.26](#page-397-1).

Returns: BOOTROM\_OK (0) on success, or a negative error code on error.

Searches a memory region for a launchable image, and executes it if possible.

The region\_base and region\_size specify a word-aligned, word-multiple-sized area of RAM, XIP RAM or flash to search. The first 4 kB of the region must contain the start of a block loop with an IMAGE\_DEF. If the new image is launched, the call does not return otherwise an error is returned.

The region\_base is signed, as a negative value can be passed, which indicates that the (negated back to positive value) is *both* the region\_base *and* the base of the "flash update" region.

This method potentially requires similar complexity to the boot path in terms of picking amongst versions, checking signatures etc. As a result it requires a user provided memory buffer as a work area. The work area should be word aligned, and of sufficient size or BOOTROM\_ERROR\_BAD\_ALIGNMENT / BOOTROM\_ERROR\_INSUFFICIENT\_RESOURCES will be returned. The work area size currently required is 3064, so 3 kB is a good choice.

This method is primarily expected to be used when implementing bootloaders.

# **NOTE**

When chaining into an image, the [BOOT\\_FLAGS0](#page-1304-1).ROLLBACK\_REQUIRED flag will not be set, to prevent invalidating a bootloader without a rollback version by booting a binary which has one (see [Section 5.10.8\)](#page-438-0).

#### <span id="page-384-1"></span>**5.4.8.3. connect\_internal\_flash**

Code: 'I','F'

Signature: void connect\_internal\_flash(void)

Supported architectures: Arm-S, RISC-V

Restores all QSPI pad controls to their default state, and connects the QMI peripheral to the QSPI pads.

If a secondary flash chip select GPIO has been configured via OTP [FLASH\\_DEVINFO,](#page-1308-0) or by writing to the runtime copy of FLASH\_DEVINFO in boot RAM, then this bank 0 GPIO is also initialised and the QMI peripheral is connected. Otherwise, bank 0 IOs are untouched.

#### <span id="page-384-0"></span>**5.4.8.4. explicit\_buy**

Code: 'E','B'

Signature: int explicit\_buy(uint8\_t \*buffer, uint32\_t buffer\_size)

Supported architectures: Arm-S RISC-V

Returns: BOOTROM\_OK (0) on success, negative error code on error.

Perform an "explicit buy" of an executable launched via an IMAGE\_DEF which was TBYB ([Section 5.1.17](#page-362-0)) flagged. A "flash update" boot of such an image is a way to have the image execute once, but only become the "current" image if it safely calls back into the bootrom via this call.

This call may perform the following:

- Erase and rewrite the part of flash containing the TBYB flag in order to clear said flag.
- Erase the first sector of the other partition in an A/B partition scenario, if this new IMAGE\_DEF is a version downgrade (so this image will boot again when not doing a normal boot)
- Update the rollback version in OTP if the chip is secure, and a rollback version is present in the image.

#### **NOTE**

The device may reboot while updating the rollback version, if multiple rollback rows need to be written - this occurs when the version crosses a multiple of 24 (for example upgrading from version 23 to 25 requires a reboot, but 23 to 24 or 24 to 25 doesn't). The application should therefore be prepared to reboot when calling this function, if rollback versions are in use.

# **NOTE**

The first of the above requires 4 kB of scratch space, so you should pass a word aligned buffer of at least 4 kB to this method in this case, or BOOTROM\_ERROR\_BAD\_ALIGNMENT / BOOTROM\_ERROR\_INSUFFICIENT\_RESOURCES will be returned.

#### <span id="page-384-2"></span>**5.4.8.5. flash\_devinfo16-ptr**

Code: 'F','D'

Type: uint16\_t \*flash\_devinfo16\_ptr

Pointer to the flash device info used by the flash APIs, e.g. for bounds checking against size of flash devices, and configuring the GPIO used for secondary QSPI chip select.

If [BOOT\\_FLAGS0.](#page-1304-1)FLASH\_DEVINFO\_ENABLE is set, this boot RAM location is initialised from [FLASH\\_DEVINFO](#page-1308-0) at startup, otherwise it is initialised to:

• Chip select 0 size: 16 MB

• Chip select 1 size: 0 bytes

• No chip select 1 GPIO

• No D8h erase command support

The flash APIs use this boot RAM copy of FLASH\_DEVINFO, so flash device info can updated by Secure code at runtime by writing through this pointer.

### <span id="page-385-0"></span>**5.4.8.6. flash\_enter\_cmd\_xip**

Code: 'C','X'

Signature: void flash\_enter\_cmd\_xip(void)

Supported architectures: Arm-S, RISC-V

Compatibility alias for flash\_select\_xip\_read\_mode(0, 12);.

Configure the QMI to generate a standard 03h serial read command, with 24 address bits, upon each XIP access. This is a slow XIP configuration, but is widely supported. CLKDIV is set to 12. The debugger may call this function to ensure that flash is readable following a program/erase operation.

Note that the same setup is performed by [flash\\_exit\\_xip\(\),](#page-385-1) and the RP2350 flash program/erase functions do not leave XIP in an inaccessible state, so calls to this function are largely redundant. It is provided for compatibility with RP2040.

#### <span id="page-385-1"></span>**5.4.8.7. flash\_exit\_xip**

Code: 'E','X'

Signature: void flash\_exit\_xip(void)

Supported architectures: Arm-S, RISC-V

Initialise the QMI for serial operations (direct mode), and also initialise a basic XIP mode, where the QMI will perform 03h serial read commands at low speed (CLKDIV=12) in response to XIP reads.

Then, issue a sequence to the QSPI device on chip select 0, designed to return it from continuous read mode ("XIP mode") and/or QPI mode to a state where it will accept serial commands. This is necessary after system reset to restore the QSPI device to a known state, because resetting RP2350 does not reset attached QSPI devices. It is also necessary when user code, having already performed some continuous-read-mode or QPI-mode accesses, wishes to return the QSPI device to a state where it will accept the serial erase and programming commands issued by the bootrom's flash access functions.

If a GPIO for the secondary chip select is configured via FLASH\_DEVINFO, then the XIP exit sequence is also issued to chip select 1.

The QSPI device should be accessible for XIP reads after calling this function; the name flash\_exit\_xip refers to returning the *QSPI device* from its XIP state to a serial command state.

#### <span id="page-385-2"></span>**5.4.8.8. flash\_flush\_cache**

Code: 'F','C'

Signature: void flash\_flush\_cache(void)

Supported architectures: Arm-S, RISC-V

Flush the entire XIP cache, by issuing an invalidate by set/way maintenance operation to every cache line ([Section](#page-341-0) [4.4.1\)](#page-341-0). This ensures that flash program/erase operations are visible to subsequent cached XIP reads.

Note that this unpins pinned cache lines, which may interfere with cache-as-SRAM use of the XIP cache.

No other operations are performed.

#### <span id="page-386-0"></span>**5.4.8.9. flash\_op**

Code: 'F','O'

Signature: int flash\_op(uint32\_t flags, uint32\_t addr, uint32\_t size\_bytes, uint8\_t \*buf)

Supported architectures: Arm-S, Arm-NS, RISC-V

Returns: BOOTROM\_OK (0) on success, negative error code on error.

Perform a flash read, erase, or program operation. Erase operations must be sector-aligned (4096 bytes) and sectormultiple-sized, and program operations must be page-aligned (256 bytes) and page-multiple-sized; misaligned erase and program operations will return BOOTROM\_ERROR\_BAD\_ALIGNMENT. The operation — erase, read, program — is selected by the CFLASH\_OP\_BITS bitfield of the flags argument.

addr is the address of the first flash byte to be accessed, ranging from XIP\_BASE to XIP\_BASE + 0x1ffffff inclusive. This may be a runtime or storage address. buf contains data to be written to flash, for program operations, and data read back from flash, for read operations. buf is never written by program operations, and is completely ignored for erase operations.

The flash operation is bounds-checked against the known flash devices specified by the runtime value of FLASH\_DEVINFO, stored in boot RAM. This is initialised by the bootrom to the OTP value [FLASH\\_DEVINFO,](#page-1308-0) if [BOOT\\_FLAGS0.](#page-1304-1)FLASH\_DEVINFO\_ENABLE is set; otherwise it is initialised to 16 MB for chip select 0 and 0 bytes for chip select 1. FLASH\_DEVINFO can be updated at runtime by writing to its location in boot RAM, the pointer to which can be looked up in the ROM table.

If a resident partition table is in effect, then the flash operation is also checked against the partition permissions. The Secure version of this function can specify the caller's effective security level (Secure, Non-secure, bootloader) using the CFLASH\_SECLEVEL\_BITS bitfield of the flags argument, whereas the Non-secure function is always checked against the Non-secure permissions for the partition. Flash operations which span two partitions are not allowed, and will fail address validation.

If [FLASH\\_DEVINFO.](#page-1308-0)D8H\_ERASE\_SUPPORTED is set, erase operations will use a D8h 64 kB block erase command where possible (without erasing outside the specified region), for faster erase time. Otherwise, only 20h 4 kB sector erase commands are used.

Optionally, this API can translate addr from flash runtime addresses to flash storage addresses, according to the translation currently configured by QMI address translation registers, [ATRANS0](#page-1244-0) through [ATRANS7](#page-1246-2). For example, an image stored at a +2 MB offset in flash (but mapped at XIP address 0 at runtime), writing to an offset of +1 MB into the image, will write to a physical flash storage address of 3 MB. Translation is enabled by setting the CFLASH\_ASPACE\_BITS bitfield in the flags argument.

When translation is enabled, flash operations which cross address holes in the XIP runtime address space (created by non-maximum ATRANSx\_SIZE) will return an error response. This check may tear: the transfer may be partially performed before encountering an address hole and ultimately returning failure.

When translation is enabled, flash operations are permitted to cross chip select boundaries, provided this does not span an ATRANS address hole. When translation is disabled, the entire operation must target a single flash chip select (as determined by bits 24 and upward of the address), else address validation will fail.

A typical call sequence for erasing a flash sector in the runtime address space from Secure code would be:

- connect\_internal\_flash();
- flash\_exit\_xip();
- flash\_op((CFLASH\_OP\_VALUE\_ERASE << CFLASH\_OP\_LSB) | (CFLASH\_SECLEVEL\_VALUE\_SECURE << CFLASH\_SECLEVEL\_LSB) | (CFLASH\_ASPACE\_VALUE\_RUNTIME << CFLASH\_ASPACE\_LSB), addr, 4096, NULL);
- flash\_flush\_cache();
- Copy the XIP setup function from boot RAM to SRAM and execute it, to restore the original XIP mode
  - The bootrom will have written a default setup function which restores the mode/clkdiv parameters found during flash search; user code can overwrite this with its own custom setup function.

A similar sequence is required for program operations. Read operations can leave the current XIP mode in effect, so only the flash\_op(…); call is required.

Note that the RP2350 bootrom leaves the flash in a basic XIP state in between program/erase operations. However, during a program/erase operation, the QMI is in direct mode [\(Section 12.14.5\)](#page-1232-0) and any attempted XIP access will return a bus error response.

#### <span id="page-387-0"></span>**5.4.8.10. flash\_range\_erase**

```
Code: 'R','E'
```

Signature: void flash\_range\_erase(uint32\_t addr, size\_t count, uint32\_t block\_size, uint8\_t block\_cmd)

Supported architectures: Arm-S, RISC-V

Erase count bytes, starting at addr (offset from start of flash). Optionally, pass a block erase command e.g. D8h block erase, and the size of the block erased by this command — this function will use the larger block erase where possible, for much higher erase speed. addr must be aligned to a 4096-byte sector, and count must be a multiple of 4096 bytes.

This is a low-level flash API, and no validation of the arguments is performed. See [flash\\_op\(\)](#page-386-0) for a higher-level API which checks alignment, flash bounds and partition permissions, and can transparently apply a runtime-to-storage address translation.

The QSPI device must be in a serial command state before calling this API, which can be achieved by calling connect\_internal\_flash() followed by flash\_exit\_xip(). After the erase, the flash cache should be flushed via flash\_flush\_cache() to ensure the modified flash data is visible to cached XIP accesses.

Finally, the original XIP mode should be restored by copying the saved XIP setup function from boot RAM into SRAM, and executing it: the bootrom provides a default function which restores the flash mode/clkdiv discovered during flash scanning, and user programs can override this with their own XIP setup function.

For the duration of the erase operation, QMI is in direct mode [\(Section 12.14.5](#page-1232-0)) and attempting to access XIP from DMA, the debugger or the other core will return a bus fault. XIP becomes accessible again once the function returns.

#### <span id="page-387-1"></span>**5.4.8.11. flash\_range\_program**

```
Code: 'R','P'
```

Signature: void flash\_range\_program(uint32\_t addr, const uint8\_t \*data, size\_t count)

Supported architectures: Arm-S, RISC-V

Program data to a range of flash storage addresses starting at addr (offset from the start of flash) and count bytes in size. addr must be aligned to a 256-byte boundary, and count must be a multiple of 256.

This is a low-level flash API, and no validation of the arguments is performed. See [flash\\_op\(\)](#page-386-0) for a higher-level API which checks alignment, flash bounds and partition permissions, and can transparently apply a runtime-to-storage address translation.

The QSPI device must be in a serial command state before calling this API — see notes on [flash\\_range\\_erase\(\).](#page-387-0)

#### <span id="page-388-0"></span>**5.4.8.12. flash\_reset\_address\_trans**

```
Code: 'R','A'
```

Signature: void flash\_reset\_address\_trans(void)

Supported architectures: Arm-S, RISC-V

Restore the QMI address translation registers, [ATRANS0](#page-1244-0) through [ATRANS7](#page-1246-2), to their reset state. This makes the runtimeto-storage address map an identity map, i.e. the mapped and unmapped address are equal, and the entire space is fully mapped. See [Section 12.14.4.](#page-1231-0)

#### <span id="page-388-2"></span>**5.4.8.13. flash\_runtime\_to\_storage\_addr**

```
Code: 'F','A'
```

Signature: int flash\_runtime\_to\_storage\_addr(uint32\_t addr)

Supported architectures: Arm-S, Arm-NS, RISC-V

Returns: A positive value on success (the translated address), or negative error code on error

Applies the address translation currently configured by QMI address translation registers, [ATRANS0](#page-1244-0) through [ATRANS7](#page-1246-2). See [Section 12.14.4](#page-1231-0).

Translating an address outside of the XIP runtime address window, or beyond the bounds of an ATRANSx\_SIZE field, returns BOOTROM\_ERROR\_INVALID\_ADDRESS, which is not a valid flash storage address. Otherwise, return the storage address which QMI would access when presented with the runtime address addr. This is effectively a virtual-to-physical address translation for QMI.

#### <span id="page-388-1"></span>**5.4.8.14. flash\_select\_xip\_read\_mode**

Code: 'X','M'

Signature: void flash\_select\_xip\_read\_mode(bootrom\_xip\_mode\_t mode, uint8\_t clkdiv)

Supported architectures: Arm-S, RISC-V

Configure QMI for one of a small menu of XIP read modes supported by the bootrom. This mode is configured for both memory windows (both chip selects), and the clock divisor is also applied to direct mode.

The available modes are:

- <sup>0</sup>: 03h serial read: serial address, serial data, no wait cycles
- <sup>1</sup>: 0Bh serial read: serial address, serial data, 8 wait cycles
- <sup>2</sup>: BBh dual-IO read: dual address, dual data, 4 wait cycles (including MODE bits, which are driven to 0)
- <sup>3</sup>: EBh quad-IO read: quad address, quad data, 6 wait cycles (including MODE bits, which are driven to 0)

The XIP write command/format are not configured by this function.

When booting from flash, the bootrom tries each of these modes in turn, from 3 down to 0. The first mode that is found to work is remembered, and a default XIP setup function is written into boot RAM that calls *this function* (flash\_select\_xip\_read\_mode) with the parameters discovered during flash scanning. This can be called at any time to restore the flash parameters discovered during flash boot.

All XIP modes configured by the bootrom have an 8-bit serial command prefix, so that the flash device can remain in a serial command state, meaning XIP accesses can be mixed more freely with program/erase serial operations. This has a performance penalty, so users can perform their own flash setup *after flash boot* using continuous read mode or QPI mode to avoid or alleviate the command prefix cost.

#### <span id="page-389-1"></span>**5.4.8.15. get\_b\_partition**

```
Code: 'G','B'
```

Signature: int get\_b\_partition(uint partition\_a)

Supported architectures: Arm-S RISC-V

Returns: The index of the B partition of partition A if a partition table is present and loaded, and there is a partition A with a corresponding B partition; otherwise returns BOOTROM\_ERROR\_NOT\_FOUND.

#### <span id="page-389-0"></span>**5.4.8.16. get\_partition\_table\_info**

```
Code: 'G','P'
```

Signature: int get\_partition\_table\_info(uint32\_t \*out\_buffer, uint32\_t out\_buffer\_word\_size, uint32\_t flags\_and\_partition)

Supported architectures: Arm-S, Arm-NS, RISC-V

Returns: >= 0 on success (the number of words filled in out\_buffer), negative error code on error.

Fills a buffer with information from the partition table. Note that this API is also used to return information over the PICOBOOT interface.

On success, the buffer is filled, and the number of words filled in the buffer is returned. If the partition table has not been loaded (e.g. from a watchdog or RAM boot), then this method will return BOOTROM\_ERROR\_PRECONDITION\_NOT\_MET, and you should load the partition table via [load\\_partition\\_table\(\)](#page-393-1) first.

Note that not all data from the partition table is kept resident in memory by the bootrom due to size constraints. To protect against changes being made in flash after the bootrom has loaded the resident portion, the bootrom keeps a hash of the partition table as of the time it loaded it. If the hash has changed by the time this method is called, then it will return BOOTROM\_ERROR\_INVALID\_STATE.

The information returned is chosen by the flags\_and\_partition parameter; the first word in the returned buffer, is the (sub)set of those flags that the API supports. You should always check this value before interpreting the buffer.

Following the first word, returns words of data for each present flag in order. With the exception of PT\_INFO, all the flags select "per partition" information, so each field is returned in flag order for one partition after the next. The special SINGLE\_PARTITION flag indicates that data for only a single partition is required. Flags include:

- 0x0001 PT\_INFO : information about the partition table as a whole. The second two words for unpartitioned space in the same form described in [Section 5.9.4.2](#page-424-0).
  - Word 0 : partition\_count (low 8 bits), partition\_table\_present (bit 8)
  - Word 1 : unpartitioned\_space\_permissions\_and\_location
  - Word 2 : unpartitioned\_space\_permissions\_and\_flags
- 0x8000 SINGLE\_PARTITION : only return data for a single partition; the partition number is stored in the top 8 bits of flags\_and\_partition

#### Per-partition fields:

- 0x0010 PARTITION\_LOCATION\_AND\_FLAGS : the core information about a partition. The format of these fields is described in [Section 5.9.4.2](#page-424-0).
  - Word 0 permissions\_and\_location
  - Word 1 permissions\_and\_flags
- 0x0020 PARTITION\_ID : the optional 64-bit identifier for the partition. If the HAS\_ID bit is set in the partition flags, then the 64 bit ID is returned:
  - Word 0 first 32 bits

- Word 1 second 32 bits
- 0x0040 PARTITION\_FAMILY\_IDS : Any additional UF2 family IDs that the partition supports being downloaded into it via the MSD bootloader beyond the standard ones flagged in the permissions\_and\_flags field (see [Section 5.9.4.2\)](#page-424-0).
- 0x0080 PARTITION\_NAME : The optional name for the partition. If the HAS\_NAME field bit in permissions\_and\_flags is not set, then no data is returned for this partition; otherwise the format is as follows:
  - Byte 0 : 7 bit length of the name (LEN); top bit reserved
  - Byte 1 : first character of name
  - …
  - Byte LEN : last character of name
  - … *(padded up to the next word boundary)*

#### **NOTE**

Unpartitioned space is always reported in Word 1 as having a base offset of 0x0 and a size of 0x2000 sectors (32 MB). The bootrom applies unpartitioned space permissions to *any* flash storage address that is not covered by a partition.

#### <span id="page-390-0"></span>**5.4.8.17. get\_sys\_info**

```
Code: 'G','S'
```

Signature: int get\_sys\_info(uint32\_t \*out\_buffer, uint32\_t out\_buffer\_word\_size, uint32\_t flags)

Supported architectures: Arm-S, Arm-NS, RISC-V

Returns: A positive value on success (the number of words filled in out\_buffer), negative error code on error.

Fills a buffer with various system information. Note that this API is also used to return information over the PICOBOOT interface.

The information returned is chosen by the flags parameter; the first word in the returned buffer, is the (sub)set of those flags that the API supports. You should always check this value before interpreting the buffer.

Following the first word, returns words of data for each present flag in order:

- 0x0001 : CHIP\_INFO unique identifier for the chip (3 words)
  - Word 0 : Value of the CHIP\_INFO\_PACKAGE\_SEL register
  - Word 1 : RP2350 device id
  - Word 2 : RP2350 wafer id
- 0x0002 : CRITICAL (1 word)
  - Word 0 : Value of the OTP [CRITICAL](#page-1283-0) register, containing critical boot flags read out on last OTP reset event
- 0x0004 : CPU\_INFO (1 word)
  - Word 0 : Current CPU architecture
    - 0 Arm
    - 1 RISC-V
- 0x0008 : FLASH\_DEV\_INFO (1 word)
  - Word 0 : Flash device info in the format of OTP [FLASH\\_DEVINFO](#page-1308-0)
- 0x0010 : BOOT\_RANDOM a 128-bit random number generated on each boot (4 words)
  - Word 0 : Per boot random number 0

- Word 1 : Per boot random number 1
- Word 2 : Per boot random number 2
- Word 3 : Per boot random number 3
- 0x0020 : NONCE not supported
- 0x0040 : BOOT\_INFO (4 words)
  - Word 0 : 0xttppbbdd
    - tt recent boot TBYB and update info (updated on regular non BOOTSEL boots)
    - pp recent boot partition (updated on regular not BOOTSEL boots)
    - bb boot type of the most recent boot
    - dd recent boot diagnostic "partition"
  - Word 1 : Recent boot diagnostic. Diagnostic information from a recent boot (with information from the partition (or slot) indicated by dd above. "partition" numbers here are:
    - 0-15 : a partition number
    - -1 : none
    - -2 : slot 0
    - -3 : slot 1
    - -4 : image (the diagnostic came from the launch of a RAM image, OTP boot image or user chain\_image() call).
  - Word 2 : Last reboot param 0
  - Word 3 : Last reboot param 1

"Boot Diagnostic" information is intended to help identify the cause of a failed boot, or booting into an unexpected binary. This information can be retrieved via PICOBOOT after a watchdog reboot, however it will not survive a reset via the RUN pin or POWMAN reset.

There is only one word of diagnostic information. What it records is based on the pp selection above, which is itself set as a parameter when rebooting programmatically into a normal boot.

To get diagnostic info, pp must refer to a slot or an "A" partition; image diagnostics are automatically selected on boot from OTP or RAM image, or when chain\_image() is called.)

The diagnostic word thus contains data for either slot 0 and slot 1, or the "A" partition (and its "B" partition if it has one). The low half word of the diagnostic word contains information from slot 0 or partition A; the high half word contains information from slot 1 or partition B.

The format of each half-word is as follows (using the word *region* to refer to slot or partition)

- 0x0001 : REGION\_SEARCHED The region was searched for a block loop.
- 0x0002 : INVALID\_BLOCK\_LOOP A block loop was found but it was invalid
- 0x0004 : VALID\_BLOCK\_LOOP A valid block loop was found (Blocks from a loop wholly contained within the region, and the blocks have the correct structure. Each block consists of items whose sizes sum to the size of the block)
- 0x0008 : VALID\_IMAGE\_DEF A valid IMAGE\_DEF was found in the region. A valid IMAGE\_DEF must parse correctly and must be executable.
- 0x0010 : HAS\_PARTITION\_TABLE Whether a partition table is present. This partition table must have a correct structure formed if VALID\_BLOCK\_LOOP is set. If the partition table turns out to be invalid, then INVALID\_BLOCK\_LOOP is set too (thus both VALID\_BLOCK\_LOOP and INVALID\_BLOCK\_LOOP will both be set).
- 0x0020 : CONSIDERED There was a choice of partition/slot and this one was considered. The first slot/partition is chosen based on a number of factors. If the first choice fails verification, then the other choice will be considered.

- the version of the PARTITION\_TABLE/IMAGE\_DEF present in the slot/partition respectively.
- whether the slot/partition is the "update region" as per a FLASH\_UPDATE reboot.
- whether an IMAGE\_DEF is marked as "explicit buy"
- 0x0040 : CHOSEN This slot/partition was chosen (or was the only choice)
- 0x0080 : PARTITION\_TABLE\_MATCHING\_KEY\_FOR\_VERIFY if a signature is required for the PARTITION\_TABLE (via OTP setting), then whether the PARTITION\_TABLE is signed with a key matching one of the four stored in OTP
- 0x0100 : PARTITION\_TABLE\_HASH\_FOR\_VERIFY set if a hash value check could be performed. In the case a signature is required, this value is identical to PARTITION\_TABLE\_MATCHING\_KEY\_FOR\_VERIFY
- 0x0200 : PARTITION\_TABLE\_VERIFIED\_OK whether the PARTITION\_TABLE passed verification (signature/hash if present/required)
- 0x0400 : IMAGE\_DEF\_MATCHING\_KEY\_FOR\_VERIFY if a signature is required for the IMAGE\_DEF due to secure boot, then whether the IMAGE\_DEF is signed with a key matching one of the four stored in OTP.
- 0x0800 : IMAGE\_DEF\_HASH\_FOR\_VERIFY set if a hash value check could be performed. In the case a signature is required, this value is identical to IMAGE\_DEF\_MATCHING\_KEY\_FOR\_VERIFY
- 0x1000 : IMAGE\_DEF\_VERIFIED\_OK whether the PARTITION\_TABLE passed verification (signature/hash if present/required) and any LOAD\_MAP is valid
- 0x2000 : LOAD\_MAP\_ENTRIES\_LOADED whether any code was copied into RAM due to a LOAD\_MAP
- 0x4000 : IMAGE\_LAUNCHED whether an IMAGE\_DEF from this region was launched
- 0x8000 : IMAGE\_CONDITION\_FAILURE whether the IMAGE\_DEF failed final checks before launching; these checks include:
  - verification failed (if it hasn't been verified earlier in the CONSIDERED phase).
  - a problem occurred setting up any rolling window.
  - the rollback version could not be set in OTP (if required in Secure mode)
  - the image was marked as Non-secure
  - the image was marked as "explicit buy", and this was a flash boot, but then region was not the "flash update" region
  - the image has the wrong architecture, but architecture auto-switch is disabled (or the correct architecture is disabled)

# **NOTE**

The non-sensical combination of BOOT\_DIAGNOSTIC\_INVALID\_BLOCK\_LOOP and BOOT\_DIAGNOSTIC\_VALID\_BLOCK\_LOOP both being set is used to flag a PARTITION\_TABLE which passed the initial verification (and hash/sig), but was later discovered to have invalid contents when it was fully parsed.

To get a full picture of a failed boot involving slots and multiple partitions, the device can be rebooted multiple times to gather the information.

#### <span id="page-392-0"></span>**5.4.8.18. get\_uf2\_target\_partition**

```
Code: 'G','U'
```

Signature: int get\_uf2\_target\_partition(uint8\_t \*workarea\_base, uint32\_t workarea\_size, uint32\_t family\_id, resident\_partition\_t \*partition\_out)

Supported architectures: Arm-S RISC-V. Note on RISC-V this function requires additional stack; see [Section 5.4.8.26.](#page-397-1)

Returns: >= 0 on success (the target partition index), or a negative error code on error.

This method performs the same operation to decide on a taget partition for a UF2 family ID as when a UF2 is dragged

onto the USB drive in BOOTSEL mode.

This method potentially requires similar complexity to the boot path in terms of picking amongst versions, checking signatures etc. As a result it requires a user provided memory buffer as a work area. The work area should byte wordaligned and of sufficient size or BOOTROM\_ERROR\_INSUFFICIENT\_RESOURCES will be returned. The work area size currently required is 3064, so 3K is a good choice.

If the partition table has not been loaded (e.g. from a watchdog or RAM boot), then this method will return BOOTROM\_ERROR\_PRECONDITION\_NOT\_MET, and you should load the partition table via [load\\_partition\\_table\(\)](#page-393-1) first.

#### <span id="page-393-0"></span>**5.4.8.19. git\_revision**

Code: 'G','R'

Type: const uint32\_t git\_revision

The 8 most significant hex digits of the bootrom git revision. Uniquely identifies this version of the bootrom.

![](_page_393_Picture_8.jpeg)

This is the git revision built at chip tapeout; the git hash in the public repository is different due to squashed history, even though the contents are identical. The contents can be verified by building the public bootrom source and comparing the resulting binary with one binary dumped from the chip.

#### <span id="page-393-1"></span>**5.4.8.20. load\_partition\_table**

Code: 'L','P'

Signature: int load\_partition\_table(uint8\_t \*workarea\_base, uint32\_t workarea\_size, bool force\_reload)

Supported architectures: Arm-S, RISC-V. Note on RISC-V this function requires additional stack; see [Section 5.4.8.26](#page-397-1).

Returns: BOOTROM\_OK (0) on success, or a negative error code on error.

Loads the current partition table from flash, if present.

This method potentially requires similar complexity to the boot path in terms of picking amongst versions, checking signatures etc. As a result it requires a user provided memory buffer as a work area. The work area should byte wordaligned and of sufficient size or BOOTROM\_ERROR\_INSUFFICIENT\_RESOURCES will be returned. The work area size currently required is 3064, so 3K is a good choice.

If force\_reload is false, then this method will return BOOTROM\_OK immediately if the bootrom is loaded, otherwise it will reload the partition table if it has been loaded already, allowing for the partition table to be updated in a running program.

#### <span id="page-393-2"></span>**5.4.8.21. otp\_access**

```
Code: 'O','A'
```

Signature: int otp\_access(uint8\_t \*buf, uint32\_t buf\_len, uint32\_t row\_and\_flags)

Supported architectures: Arm-S, Arm-NS, RISC-V

Returns: BOOTROM\_OK (0) on success, or a negative error code on error.

Writes data from a buffer into OTP, or reads data from OTP into a buffer.

- 0x0000ffff ROW\_NUMBER: 16 low bits are row number (0-4095)
- 0x00010000 IS\_WRITE: if set, do a write (not a read)

• 0x00020000 - IS\_ECC: if this bit is set, each value in the buffer is 2 bytes and ECC is used when read/writing from 24 bit value in OTP. If this bit is not set, each value in the buffer is 4 bytes, the low 24-bits of which are written to or read from OTP.

The buffer must be aligned to 2 bytes or 4 bytes according to the IS\_ECC flag.

This method will read and write rows until the first row it encounters that fails a key or permission check at which it will return BOOTROM\_ERROR\_NOT\_PERMITTED.

Writing will also stop at the first row where an attempt is made to set an OTP bit from a 1 to a 0, and BOOTROM\_ERROR\_UNSUPPORTED\_MODIFICATION will be returned.

If all rows are read/written successfully, then BOOTROM\_OK will be returned.

#### <span id="page-394-1"></span>**5.4.8.22. partition\_table\_ptr**

Code: 'P','T'

Type: resident\_partition\_table \*\*partition\_table\_ptr

A pointer to the pointer to the resident partition table info. The resident partition table is the subset of the full partition table that is kept in memory, and used for flash permissions.

The public part of the resident partition table info is of the form:

| Word   | Bytes        | Value                                                                                                                                                             |  |  |
|--------|--------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|--|--|
| 0<br>1 |              | partition_count (0-16)                                                                                                                                            |  |  |
|        | 1            | partition_count_with_permissions (0-16). Set this to > partition_count when adding extra<br>permission regions at runtime (do not modify the original partitions) |  |  |
|        | 1            | loaded (0x01 if a partition table has been loaded from flash)                                                                                                     |  |  |
|        | 1            | 0x00 (pad)                                                                                                                                                        |  |  |
| 1      | 1            | unpartitioned_space_permissions_and_flags                                                                                                                         |  |  |
| 2-3    | Partition 0  |                                                                                                                                                                   |  |  |
|        | 1            | permissions_and_location for partition 0                                                                                                                          |  |  |
|        | 1            | permissions_and_flags for partition 0                                                                                                                             |  |  |
| 4-5    | Partition 1  |                                                                                                                                                                   |  |  |
| 1      |              | permissions_and_location for partition 1                                                                                                                          |  |  |
|        | 1            | permissions_and_flags for partition 1                                                                                                                             |  |  |
| …      | …            | …                                                                                                                                                                 |  |  |
| 32-33  | Partition 15 |                                                                                                                                                                   |  |  |
|        | 1            | permissions_and_location for partition 15                                                                                                                         |  |  |
|        | 1            | permissions_and_flags for partition 15                                                                                                                            |  |  |

Details of the fields *permissions\_and\_location* and *permissions\_and\_flags* can be found in [Section 5.9.4](#page-423-0).

#### <span id="page-394-0"></span>**5.4.8.23. pick\_ab\_partition**

Code: 'A','B'

Signature: int pick\_ab\_partition(uint8\_t \*workarea\_base, uint32\_t workarea\_size, uint partition\_a\_num)

Supported architectures: Arm-S, RISC-V. Note on RISC-V this function requires additional stack; see [Section 5.4.8.26](#page-397-1).

Returns: >= 0 on success (the partition index), or a negative error code on error.

Determines which of the partitions has the "better" IMAGE\_DEF. In the case of executable images, this is the one that would be booted

This method potentially requires similar complexity to the boot path in terms of picking amongst versions, checking signatures etc. As a result it requires a user provided memory buffer as a work area. The work area should bye word aligned, and of sufficient size or BOOTROM\_ERROR\_INSUFFICIENT\_RESOURCES will be returned. The work area size currently required is 3064, so 3K is a good choice.

The passed partition number can be any valid partition number other than the "B" partition of an A/B pair.

This method returns a negative error code, or the partition number of the picked partition if (i.e. partition\_a\_num or the number of its "B" partition if any).

![](_page_395_Figure_7.jpeg)

This method does not look at owner partitions, only the A partition passed and its corresponding B partition.

#### <span id="page-395-0"></span>**5.4.8.24. reboot**

```
Code: 'R','B'
Signature: int reboot(uint32_t flags, uint32_t delay_ms, uint32_t p0, uint32_t p1)
Supported architectures: Arm-S, Arm-NS, RISC-V
```

Returns: BOOTROM\_OK (or doesn't return) on success, a negative error code on error.

Resets the RP2350 and uses the watchdog facility to restart.

The *delay\_ms* is the millisecond delay before the reboot occurs. Note: by default this method is asynchronous (unless NO\_RETURN\_ON\_SUCCESS is set - see below), so the method will return and the reboot will happen this many milliseconds later.

The *flags* field contains one of the following values:

- 0x0000 : REBOOT\_TYPE\_NORMAL reboot into the normal boot path.
  - p0 - the boot diagnostic "partition" (low 8 bits only)
- 0x0002 : REBOOT\_TYPE\_BOOTSEL reboot into BOOTSEL mode.
  - p0 - the GPIO number to use as an activity indicator (enabled by flag in p1).
  - p1 - a set of flags:
    - 0x01 : DISABLE\_MSD\_INTERFACE Disable the BOOTSEL USB drive (see [Section 5.5\)](#page-398-0)
    - 0x02 : DISABLE\_PICOBOOT\_INTERFACE Disable the PICOBOOT interface (see [Section 5.6\)](#page-402-0).
    - 0x10 : GPIO\_PIN\_ACTIVE\_LOW The GPIO in p0 is active low.
    - 0x20 : GPIO\_PIN\_ENABLED Enable the activity indicator on the specified GPIO.
- 0x0003 : REBOOT\_TYPE\_RAM\_IMAGE reboot into an image in RAM. The region of RAM or XIP RAM is searched for an image to run. This is the type of reboot used when a RAM UF2 is dragged onto the BOOTSEL USB drive.
  - p0 - the region start address (word-aligned).
  - p1 - the region size (word-aligned).
- 0x0004 : REBOOT\_TYPE\_FLASH\_UPDATE variant of REBOOT\_TYPE\_NORMAL to use when flash has been updated. This is the type of reboot used after dragging a flash UF2 onto the BOOTSEL USB drive.

- p0 - the address of the start of the region of flash that was updated. If this address matches the start address of a partition or slot, then that partition or slot is treated preferentially during boot (when there is a choice). This type of boot facilitates TBYB [\(Section 5.1.17\)](#page-362-0) and version downgrades.
- 0x000d : REBOOT\_TYPE\_PC\_SP reboot to a specific PC and SP. Note: this is not allowed in the Arm-NS variant.
  - p0 - the initial program counter (PC) to start executing at. This must have the lowest bit set for Arm and clear for RISC-V
  - p1 - the initial stack pointer (SP).

All of the above, can have optional flags ORed in:

- 0x0010 : REBOOT\_TO\_ARM switch both cores to the Arm architecture (rather than leaving them as is). The call will fail with BOOTROM\_ERROR\_INVALID\_STATE if the Arm architecture is not supported.
- 0x0020 : REBOOT\_TO\_RISCV switch both cores to the RISC-V architecture (rather than leaving them as is). The call will fail with BOOTROM\_ERROR\_INVALID\_STATE if the RISC-V architecture is not supported.
- 0x0100 : NO\_RETURN\_ON\_SUCCESS the watchdog hardware is asynchronous. Setting this bit forces this method not to return *if* the reboot is successfully initiated.

#### **NOTE**

The p0 and p1 parameters are generally written to watchdog scratch registers 2 & 3, and are interpreted post-reboot by the boot path code. The exception is REBOOT\_TYPE\_NORMAL where this API handles the p0 value before rebooting; the boot path itself does not accept any parameters for REBOOT\_TYPE\_NORMAL.

#### <span id="page-396-0"></span>**5.4.8.25. secure\_call**

Code: 'S','C'

Signature: int secure\_call(…)

Supported architectures: Arm-NS

Returns: >= 0 on success, a negative error code on error.

Call a Secure method from Non-secure code, passing the method to be called in the register r4 (other arguments passed as normal).

This method provides the ability to decouple the Non-secure code from the Secure code, allowing the former to call methods in the latter without needing to know the location of the methods.

This call will always return BOOTROM\_ERROR\_INVALID\_STATE unless Secure Arm code has provided a handler function via [set\\_rom\\_callback\(\)](#page-397-2); if there is a handler function, this method will return the return code that the handler returns, with the convention that BOOTROM\_ERROR\_INVALID\_ARG if the "function selector" (in r4) is not supported.

Certain well-known "function selectors" will be pre-defined to facilitate interaction between Secure and Non-secure SDK code, or indeed with other environments (e.g. logging to secure UART/USB CDC, launch of core 1 from NS code, watchdog reboot from NS code back into NS code, etc.)

To avoid conflicts the following bit patterns are used for "function selectors":

- 0b0xxx xxxx xxxx xxxx xxxx xxxx xxxx xxxx a "well known" function selector; do not use for your own methods
- 0b10xx xxxx xxxx xxxx xxxx xxxx xxxx xxxx a "unique" function selector intended to be unlikely to clash with others'. The lower 30 bits should be chosen at random
- 0b11xx xxxx xxxx xxxx xxxx xxxx xxxx xxxx a "private" function selector intended for use by tightly coupled NS and S code

#### <span id="page-397-1"></span>**5.4.8.26. set\_bootrom\_stack**

```
Code: 'S','S'
Signature: int set_bootrom_stack(uint32_t base_size[2])
Supported architectures: RISC-V
```

Returns: BOOTROM\_OK (0) on success, a negative error code on error.

Most bootrom functions are written just once, in Arm code, to save space. As a result these functions are emulated when running under the RISC-V architecture. This is largely transparent to the user, however the stack used by the Arm emulation is separate from the calling user's stack, and is stored in boot RAM and is of quite limited size. When using certain of the more complex APIs or if nesting bootrom calls from within IRQs, you may need to provide a larger stack.

This method allows the caller to specify a region of RAM to use as the stack *for the current core* by passing a pointer to two values: the word aligned base address, and the size in bytes (multiple of 4).

The method fills in the previous base/size values into the passed array before returning.

#### <span id="page-397-0"></span>**5.4.8.27. set\_ns\_api\_permission**

```
Code: 'S','P'
Signature: int set_ns_api_permission(uint ns_api_num, bool allowed)
Supported architectures: Arm-S
```

Returns: BOOTROM\_OK (0) on success, a negative error code on error.

Allow or disallow the specific NS API (note all NS APIs default to disabled).

ns\_api\_num is one of the following, configuring Arm-NS access to the given API. When an NS API is disabled, calling it will return BOOTROM\_ERROR\_NOT\_PERMITTED.

- 0x0: get\_sys\_info
- 0x1: flash\_op
- 0x2: flash\_runtime\_to\_storage\_addr
- 0x3: get\_partition\_table\_info
- 0x4: secure\_call
- 0x5: otp\_access
- 0x6: reboot
- 0x7: get\_b\_partition

#### **NOTE**

All permissions default to disallowed after a reset (see also [bootrom\\_state\\_reset\(\)\)](#page-383-1).

#### <span id="page-397-2"></span>**5.4.8.28. set\_rom\_callback**

```
Code: 'R','C'
Signature: int set_rom_callback(uint callback_number, int (*callback)(…))
Supported architectures: Arm-S, RISC-V
```

Returns: >= 0 (the old callback pointer) on success, a negative error code on error.

The only currently supported *callback\_number* is 0 which sets the callback used for the secure\_call API.

A callback pointer of 0 deletes the callback function, a positive callback pointer (all valid function pointers are on RP2350) sets the callback function, but a negative callback pointer can be passed to get the old value without setting a new value.

If successful, returns >=0 (the existing value of the function pointer on entry to the function).

#### <span id="page-398-3"></span>**5.4.8.29. validate\_ns\_buffer**

```
Code: 'V','B'
```

Signature: void \*validate\_ns\_buffer(const void \*addr, uint32\_t size, uint32\_t write, uint32\_t \*ok)

Supported architectures: Arm-S

Returns: addr on success, a negative error code on error.

Utility method that can be used by Secure Arm code to validate a buffer passed to it from Non-secure code.

Both the write parameter and the (out) result parameter ok are RCP booleans, so 0xa500a500 for true, and 0x00c300c3 for false. This enables hardening of this function, and indeed the write parameter must be one of these values or the RCP will hang the system.

For success, the entire buffer must fit in range XIP\_BASE → SRAM\_END, and must be accessible by the Non-secure caller according to SAU + NS MPU (privileged or not based on current processor IPSR and NS CONTROL flag). Buffers in USB RAM are also allowed if access is granted to NS via ACCESSCTRL.

#### <span id="page-398-2"></span>**5.4.8.30. xip\_setup\_func\_ptr**

```
Code: 'X','F'
```

Type: void \*(xip\_setup\_func\_ptr)(void)'

