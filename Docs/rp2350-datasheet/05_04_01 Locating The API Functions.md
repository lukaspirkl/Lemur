# 5.4.1 Locating The API Functions

The API functions are normally made available to the user by wrappers in the SDK. However, a lower level method is provided to locate them (since their locations may change with each bootrom release) for other runtimes, or those who wish to locate them directly.

[Table 452](#page-376-2) shows the fixed memory layout of certain words in the bootrom used to locate these functions when using the Arm architecture. [Table 453](#page-376-3) shows the additional entries for use when using the RISC-V architecture.

*Table 452. Bootrom contents at fixed (well known) addresses for Arm code*

<span id="page-376-2"></span>

| Address    | Contents       | Description                                             |
|------------|----------------|---------------------------------------------------------|
| 0x00000000 | 32-bit pointer | Initial boot stack pointer                              |
| 0x00000004 | 32-bit pointer | Pointer to boot reset handler function                  |
| 0x00000008 | 32-bit pointer | Pointer to boot NMI handler function                    |
| 0x0000000c | 32-bit pointer | Pointer to boot Hard fault handler function             |
| 0x00000010 | 'M', 'u', 0x02 | Magic                                                   |
| 0x00000013 | byte           | Bootrom version                                         |
| 0x00000014 | 16-bit pointer | Pointer to ROM entry table (BOOTROM_ROMTABLE_START)     |
| 0x00000016 | 16-bit pointer | Pointer to a helper function (rom_table_lookup_val())   |
| 0x00000018 | 16-bit pointer | Pointer to a helper function (rom_table_lookup_entry()) |

*Table 453. Bootrom contents at fixed (well known) addresses for RISC-V code*

<span id="page-376-3"></span>

| Address    | Contents           | Description                                             |
|------------|--------------------|---------------------------------------------------------|
| 0x00007df6 | 16-bit pointer     | Pointer to ROM entry table (BOOTROM_ROMTABLE_START)     |
| 0x00007df8 | 16-bit pointer     | Pointer to a helper function (rom_table_lookup_val())   |
| 0x00007dfa | 16-bit pointer     | Pointer to a helper function (rom_table_lookup_entry()) |
| 0x00007dfc | 32-bit instruction | RISC-V Entry Point                                      |

Assuming the three bytes starting at address 0x00000010 are ('M', 'u', 0x02), the other fixed location fields can be assumed to be valid and used to lookup bootrom functionality.

The version byte at offset 0x00000013 is informational, and should not be used to infer the exact location of any functions. It has the value 2 for A2 silicon.

The following code from the SDK shows how the SDK looks up a bootrom function:

```
static __force_inline void *rom_func_lookup_inline(uint32_t code) {
#ifdef __riscv
  // on RISC-V the code (a jmp) is actually embedded in the table
  rom_table_lookup_fn rom_table_lookup =
  (rom_table_lookup_fn) (uintptr_t)*(uint16_t*)(BOOTROM_TABLE_LOOKUP_ENTRY_OFFSET
  + rom_offset_adjust);
  return rom_table_lookup(code, RT_FLAG_FUNC_RISCV);
#else
  // on Arm the function pointer is stored in the table, so we dereference it
  // via lookup() rather than lookup_entry()
  rom_table_lookup_fn rom_table_lookup =
  (rom_table_lookup_fn) (uintptr_t)*(uint16_t*)(BOOTROM_TABLE_LOOKUP_OFFSET);
  if (pico_processor_state_is_nonsecure()) {
  return rom_table_lookup(code, RT_FLAG_FUNC_ARM_NONSEC);
  } else {
  return rom_table_lookup(code, RT_FLAG_FUNC_ARM_SEC);
  }
#endif
}
```

As well as API functions, there are a few data values that can be looked up. The following code demonstrates:

```
void *rom_data_lookup(uint32_t code) {
  rom_table_lookup_fn rom_table_lookup =
  (rom_table_lookup_fn) (uintptr_t)*(uint16_t*)(BOOTROM_TABLE_LOOKUP_OFFSET);
  return rom_table_lookup(code, RT_FLAG_DATA);
}
```

The code parameter correspond to the CODE values in the tables below, and is calculated as follows:

```
uint32_t rom_table_code(char c1, char c2) {
  return (c2 << 8) | c1;
}
```

These codes are also available in [bootrom.h](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/pico_bootrom/include/pico/bootrom.h) [in the SDK](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/pico_bootrom/include/pico/bootrom.h) as #defines.

#### <span id="page-377-0"></span>**5.4.2. API Function Availability**

Some functions are not available under all architectures or security levels. The API listing in [Section 5.4.6](#page-380-0) uses the following terms to list the availability of each individual API entry point:

#### **Arm-S**

The function is available to Secure Arm code. The majority of functions are available for Arm-S unless they deal specifically with RISC-V or Non-secure functionality.

#### **RISC-V**

The function is available to RISC-V code. Most of the functions that are available under Arm-S are also exposed under RISC-V unless they deal specifically with Arm security states.

#### **Arm-NS**

The function is available to Non-secure Arm code. The function in this case performs additional permission and argument checks to prevent Secure data from leaking or being corrupted.

Each individual Arm-NS API function must be explicitly enabled by Secure code before use, via [set\\_ns\\_api\\_permission\(\).](#page-397-0) A

disabled Non-secure API returns BOOTROM\_ERROR\_NOT\_PERMITTED if disabled by Secure code. All Non-secure APIs are disabled initially. There is no permission control on Non-secure code calling Secure-only Arm-S functions, but such a call will crash when it attempts to access Secure-only hardware.

The Arm-NS functions may escalate through a Secure Gateway (SG) instruction to allow Non-secure code to perform limited operations on nominally Secure-only hardware, such as QSPI direct-mode interface used for flash programming.

The RISC-V functions do not have separate entry points based on privilege level. Both M-mode and U-mode software can call bootrom APIs, assuming they have execute permissions on ROM addresses in the PMP. However, U-mode calls will crash if they attempt to access M-mode-only hardware.

