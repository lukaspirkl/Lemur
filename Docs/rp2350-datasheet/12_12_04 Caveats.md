# 12.12.4 Caveats

The generation of random numbers by the TRNG block is not a deterministic process.

Although the modal and mean average times required to generate random numbers are quite similar, the generation process can occasionally take *much* longer to complete: in excess of 100 times the average. Any run resulting in a failed entropy check discards the result, requiring another generation process.

You can accommodate these unpredictable generation times in your system design. For example, you might generate a small pool of random numbers, initiating subsequent generation whenever space becomes available in the pool.

In the interests of simplicity and timing predictability, alternative approaches were adopted for the RP2350 bootrom and the SDK TRNG block drivers. The methodologies used can be found via the links below. However, nothing in the TRNG block in RP2350 precludes using the block as specified in Arm documentation.

#### **12.12.4.1. Bootrom**

The bootrom streams raw TRNG ROSC samples (the TRNG random source) directly into the hardware SHA-256 accelerator. It bypasses all internal checking and conditioning in the TRNG. SHA-256 is a robust hash function which avoids the pitfalls of some of the conditioning logic in the TRNG, most notably the von Neumann decorrelator.

The bootrom has some hard constraints which guide its implementation choices, most notably: *the bootrom must boot.* It cannot afford to poll the TRNG for an indeterminate amount of time to wait for a random number to appear. Complex error handling is also undesirable.

A link to the bootrom source can be found in [Chapter 5.](#page-353-0) Consult the source code for the exact implementation of the per-boot random number generation, in varm\_boot\_path.c.

The A2 bootrom TRNG code is written in assembly due to various implementation constraints, and may not be that illuminating. The following is excerpted from the A1 bootrom source, lightly edited for readability:

```
// Boot RNG is derived by streaming a large number of TRNG ROSC samples
// into the SHA-256. BOOT_TRNG_SAMPLE_BLOCKS is the number of SHA-256
// blocks to hash, each containing 384 samples from the TRNG ROSC:
const int BOOT_TRNG_SAMPLE_BLOCKS = 25;
// Fixed delay is required after TRNG soft reset
trng_hw->trng_sw_reset = -1u;
(void)trng_hw->trng_sw_reset;
(void)trng_hw->trng_sw_reset;
// Initialise SHA internal state by writing START bit
sha256_hw->csr = SHA256_CSR_RESET | SHA256_CSR_START_BITS;
// Sample one ROSC bit into EHR every cycle, subject to CPU keeping up. More
// temporal resolution to measure ROSC phase noise is better, if we use a
// high quality hash function instead of naive VN decorrelation. (Also more
// metastability events, which are a secondary noise source)
trng_hw->sample_cnt1 = 0;
// Disable checks and bypass decorrelators, to stream raw TRNG ROSC samples:
trng_hw->trng_debug_control = -1u;
// Start ROSC if it is not already started
trng_hw->rnd_source_enable = -1u;
// Clear all interrupts (including EHR_VLD) -- we will check this
// later, after seeding RCP.
trng_hw->rng_icr = -1u;
// Each half-block (192 samples) takes approx 235 cycles, so 470 cycles/block:
for (int half_blocks = 0; half_blocks < 2 * BOOT_TRNG_SAMPLE_BLOCKS; ++half_blocks) {
  // Wait for 192 ROSC samples to fill EHR, this should take constant time:
  while (trng_hw->trng_busy)
```

```
  ;
  // Copy 6 EHR words to SHA-256, plus garbage (RND_SOURCE_ENABLE and
  // SAMPLE_CNT1) which pads us out to half of a SHA-256 block. This means
  // we can avoid checking SHA-256 ready whilst reading EHR, so we restart
  // sampling sooner. (SHA-256 becomes non-ready for 57 cycles after each
  // 16 words written.)
  io_ro_32 *src = &trng_hw->ehr_data[0];
  io_wo_32 *dst = &sha256_hw->wdata;
  for (int i = 0; i < 8; ++i) {
  *dst = src[i];
  }
  // TRNG is now sampling again, having started after we read the last EHR
  // word. Grab some in-progress SHA bits and use them to modulate the
  // chain length, to reduce chance of injection locking:
  trng_hw->trng_config = sha256_hw->sum[0];
}
// Wait for SHA result -- if skipped we get the previous block's digest. Note
// this never becomes true if we wrote a number of words % 16 != 0.
while (!(sha256_hw->csr & SHA256_CSR_SUM_VLD_BITS))
  ;
// The per-boot random will change on every core 0 reset (except debugger
// skipping ROM). If this is a problem then the user can sample the
// per-boot random into a preserved variable in main SRAM.
for (int i = 0; i < 4; ++i) {
  bootram->always.boot_random.e[i] = sha256_hw->sum[4 + i];
}
trng_hw->trng_config = 0;
// Stop ROSC as it's a waste of power
trng_hw->rnd_source_enable = 0;
```

The bootrom resets the SHA-256 and TRNG via RESETS immediately before the above code runs. This code typically runs with clk\_sys running from the system ROSC, at its initial boot frequency of approximately 12 MHz. The 256-bit result is available in the [SUM0](#page-1222-0) through [SUM7](#page-1223-2) registers after the code completes.

This code does not represent best programming practice: for example it writes ones into reserved bits in the [TRNG\\_DEBUG\\_CONTROL](#page-1216-0) register. It was written with close reference to the hardware implementation. The above code listing serves only to document the *method* the bootrom uses to generate random numbers at boot time, for the onceper-boot random number available via the get\_sys\_info() ROM API as well as for initialising the RCP salt registers ([Section 3.6.3.1\)](#page-114-0).

#### **12.12.4.2. SDK**

The pico\_rand library uses the TRNG as one of its entropy sources. It streams raw ROSC samples from the TRNG ROSC in a similar manner to the bootrom. It uses the xoroshiro128\*\* and splitmix64() PRNG functions to condition the output.

#### <span id="page-1212-0"></span>**12.12.5. List of Registers**

The TRNG control registers start at a base address of 0x400f0000 (defined as [TRNG\\_BASE](#page-31-1) in the SDK).

*Table 1260. List of TRNG registers*

<span id="page-1212-1"></span>

| Offset | Name    | Info               |
|--------|---------|--------------------|
| 0x100  | RNG_IMR | Interrupt masking. |

| Offset | Name               | Info                                                                                              |  |
|--------|--------------------|---------------------------------------------------------------------------------------------------|--|
| 0x104  | RNG_ISR            | RNG status register. If corresponding RNG_IMR bit is unmasked,<br>an interrupt will be generated. |  |
| 0x108  | RNG_ICR            | Interrupt/status bit clear Register.                                                              |  |
| 0x10c  | TRNG_CONFIG        | Selecting the inverter-chain length.                                                              |  |
| 0x110  | TRNG_VALID         | 192 bit collection indication.                                                                    |  |
| 0x114  | EHR_DATA0          | RNG collected bits.                                                                               |  |
| 0x118  | EHR_DATA1          | RNG collected bits.                                                                               |  |
| 0x11c  | EHR_DATA2          | RNG collected bits.                                                                               |  |
| 0x120  | EHR_DATA3          | RNG collected bits.                                                                               |  |
| 0x124  | EHR_DATA4          | RNG collected bits.                                                                               |  |
| 0x128  | EHR_DATA5          | RNG collected bits.                                                                               |  |
| 0x12c  | RND_SOURCE_ENABLE  | Enable signal for the random source.                                                              |  |
| 0x130  | SAMPLE_CNT1        | Counts clocks between sampling of random bit.                                                     |  |
| 0x134  | AUTOCORR_STATISTIC | Statistics about autocorrelation test activations.                                                |  |
| 0x138  | TRNG_DEBUG_CONTROL | Debug register.                                                                                   |  |
| 0x140  | TRNG_SW_RESET      | Generate internal SW reset within the RNG block.                                                  |  |
| 0x1b4  | RNG_DEBUG_EN_INPUT | Enable the RNG debug mode                                                                         |  |
| 0x1b8  | TRNG_BUSY          | RNG Busy indication.                                                                              |  |
| 0x1bc  | RST_BITS_COUNTER   | Reset the counter of collected bits in the RNG.                                                   |  |
| 0x1c0  | RNG_VERSION        | Displays the version settings of the TRNG.                                                        |  |
| 0x1e0  | RNG_BIST_CNTR_0    | Collected BIST results.                                                                           |  |
| 0x1e4  | RNG_BIST_CNTR_1    | Collected BIST results.                                                                           |  |
| 0x1e8  | RNG_BIST_CNTR_2    | Collected BIST results.                                                                           |  |

# <span id="page-1213-0"></span>**[TRNG:](#page-1212-1) RNG\_IMR Register**

**Offset**: 0x100 **Description**

Interrupt masking.

*Table 1261. RNG\_IMR Register*

| Bits | Description                                                                                                                                            | Type | Reset |
|------|--------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                                                                                                              | -    | -     |
| 3    | VN_ERR_INT_MASK: Set to 1 to mask (disable) this interrupt: no interrupt will<br>be generated. See RNG_ISR for an explanation on this interrupt.       | RW   | 0x1   |
| 2    | CRNGT_ERR_INT_MASK: Set to 1 to mask (disable) this interrupt: no interrupt<br>will be generated. See RNG_ISR for an explanation on this interrupt.    | RW   | 0x1   |
| 1    | AUTOCORR_ERR_INT_MASK: Set to 1 to mask (disable) this interrupt: no<br>interrupt will be generated. See RNG_ISR for an explanation on this interrupt. | RW   | 0x1   |
| 0    | EHR_VALID_INT_MASK: Set to 1 to mask (disable) this interrupt: no interrupt<br>will be generated. See RNG_ISR for an explanation on this interrupt.    | RW   | 0x1   |

# <span id="page-1214-1"></span>**[TRNG:](#page-1212-1) RNG\_ISR Register**

**Offset**: 0x104

#### **Description**

RNG status register. If corresponding RNG\_IMR bit is unmasked, an interrupt will be generated.

*Table 1262. RNG\_ISR Register*

| Bits | Description                                                                                                                        | Type | Reset |
|------|------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                                                                                          | -    | -     |
| 3    | VN_ERR: 1 indicates von Neumann error. Error in von Neumann occurs if 32<br>consecutive collected bits are identical, ZERO or ONE. | RO   | 0x0   |
| 2    | CRNGT_ERR: 1 indicates CRNGT in the RNG test failed. Failure occurs when<br>two consecutive blocks of 16 collected bits are equal. | RO   | 0x0   |
| 1    | AUTOCORR_ERR: 1 indicates Autocorrelation test failed four times in a row.<br>When set, RNG ceases functioning until next reset.   | RO   | 0x0   |
| 0    | EHR_VALID: 1 indicates that 192 bits have been collected in the RNG, and are<br>ready to be read.                                  | RO   | 0x0   |

# <span id="page-1214-2"></span>**[TRNG:](#page-1212-1) RNG\_ICR Register**

**Offset**: 0x108

#### **Description**

Interrupt/status bit clear Register.

*Table 1263. RNG\_ICR Register*

| Bits | Description                                                            | Type | Reset |
|------|------------------------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                              | -    | -     |
| 3    | VN_ERR: Write 1 to clear corresponding bit in RNG_ISR.                 | RW   | 0x0   |
| 2    | CRNGT_ERR: Write 1 to clear corresponding bit in RNG_ISR.              | RW   | 0x0   |
| 1    | AUTOCORR_ERR: Cannot be cleared by SW! Only RNG reset clears this bit. | RW   | 0x0   |
| 0    | EHR_VALID: Write 1 - clear corresponding bit in RNG_ISR.               | RW   | 0x0   |

# <span id="page-1214-0"></span>**[TRNG:](#page-1212-1) TRNG\_CONFIG Register**

**Offset**: 0x10c **Description**

Selecting the inverter-chain length.

*Table 1264. TRNG\_CONFIG Register*

| Bits | Description                                                                                                                                                                           | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:2 | Reserved.                                                                                                                                                                             | -    | -     |
| 1:0  | RND_SRC_SEL: Selects the number of inverters (out of four possible<br>selections) in the ring oscillator (the entropy source). Higher values select<br>longer inverter chain lengths. | RW   | 0x0   |

#### <span id="page-1214-3"></span>**[TRNG:](#page-1212-1) TRNG\_VALID Register**

**Offset**: 0x110

#### **Description**

192 bit collection indication.

*Table 1265. TRNG\_VALID Register*

| Bits | Description                                                                                                             | Type | Reset |
|------|-------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:1 | Reserved.                                                                                                               | -    | -     |
| 0    | EHR_VALID: 1 indicates that collection of bits in the RNG is completed, and<br>data can be read from EHR_DATA register. | RO   | 0x0   |

# <span id="page-1215-2"></span>**[TRNG:](#page-1212-1) EHR\_DATA0, EHR\_DATA1, …, EHR\_DATA4, EHR\_DATA5 Registers**

**Offsets**: 0x114, 0x118, …, 0x124, 0x128

#### **Description**

RNG collected bits.

*Table 1266. EHR\_DATA0, EHR\_DATA1, …, EHR\_DATA4, EHR\_DATA5 Registers*

| Bits | Description                                             | Type | Reset      |
|------|---------------------------------------------------------|------|------------|
| 31:0 | Bits [(32*(i+1))-1:(32*i)] of Entropy Holding Register. | RO   | 0x00000000 |

# <span id="page-1215-1"></span>**[TRNG:](#page-1212-1) RND\_SOURCE\_ENABLE Register**

**Offset**: 0x12c

#### **Description**

Enable signal for the random source.

*Table 1267. RND\_SOURCE\_ENABLE Register*

| Bits | Description                                  | Type | Reset |
|------|----------------------------------------------|------|-------|
| 31:1 | Reserved.                                    | -    | -     |
| 0    | RND_SRC_EN: * 1 - entropy source is enabled. | RW   | 0x0   |
|      | * 0 - entropy source is disabled             |      |       |

### <span id="page-1215-0"></span>**[TRNG:](#page-1212-1) SAMPLE\_CNT1 Register**

**Offset**: 0x130

#### **Description**

Counts clocks between sampling of random bit.

*Table 1268. SAMPLE\_CNT1 Register*

| Bits | Description                                                                                                                                                                     | Type | Reset      |
|------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|------------|
| 31:0 | SAMPLE_CNTR1: Sets the number of rng_clk cycles between two consecutive<br>ring oscillator samples.<br>Note: If the von Neumann decorrelator is bypassed, the minimum value for | RW   | 0x0000ffff |
|      | sample counter must not be less than seventeen                                                                                                                                  |      |            |

#### <span id="page-1215-3"></span>**[TRNG:](#page-1212-1) AUTOCORR\_STATISTIC Register**

**Offset**: 0x134

#### **Description**

Statistics about autocorrelation test activations.

*Table 1269. AUTOCORR\_STATISTI C Register*

| Bits  | Description                                                                                                                                                                           | Type | Reset |
|-------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:22 | Reserved.                                                                                                                                                                             | -    | -     |
| 21:14 | AUTOCORR_FAILS: Count each time an autocorrelation test fails. Any write to<br>the register reset the counter. Stop collecting statistic if one of the counters<br>reached the limit. | RW   | 0x00  |

| Bits | Description                                                                                                                                                                           | Type | Reset  |
|------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|--------|
| 13:0 | AUTOCORR_TRYS: Count each time an autocorrelation test starts. Any write<br>to the register reset the counter. Stop collecting statistic if one of the counters<br>reached the limit. | RW   | 0x0000 |

# <span id="page-1216-0"></span>**[TRNG:](#page-1212-1) TRNG\_DEBUG\_CONTROL Register**

**Offset**: 0x138

#### **Description**

Debug register.

*Table 1270. TRNG\_DEBUG\_CONTR OL Register*

| Bits | Description                                                                                                   | Type | Reset |
|------|---------------------------------------------------------------------------------------------------------------|------|-------|
| 31:4 | Reserved.                                                                                                     | -    | -     |
| 3    | AUTO_CORRELATE_BYPASS: When set, the autocorrelation test in the TRNG<br>module is bypassed.                  | RW   | 0x0   |
| 2    | TRNG_CRNGT_BYPASS: When set, the CRNGT test in the RNG is bypassed.                                           | RW   | 0x0   |
| 1    | VNC_BYPASS: When set, the Von-Neuman balancer is bypassed (including the<br>32 consecutive bits test).<br>N/A | RW   | 0x0   |
| 0    | Reserved.                                                                                                     | -    | -     |

# <span id="page-1216-1"></span>**[TRNG:](#page-1212-1) TRNG\_SW\_RESET Register**

**Offset**: 0x140

#### **Description**

Generate internal SW reset within the RNG block.

*Table 1271. TRNG\_SW\_RESET Register*

| Bits | Description                                                             | Type | Reset |
|------|-------------------------------------------------------------------------|------|-------|
| 31:1 | Reserved.                                                               | -    | -     |
| 0    | TRNG_SW_RESET: Writing 1 to this register causes an internal RNG reset. | RW   | 0x0   |

# <span id="page-1216-2"></span>**[TRNG:](#page-1212-1) RNG\_DEBUG\_EN\_INPUT Register**

**Offset**: 0x1b4

#### **Description**

Enable the RNG debug mode

*Table 1272. RNG\_DEBUG\_EN\_INPU T Register*

| Bits | Description                                | Type | Reset |
|------|--------------------------------------------|------|-------|
| 31:1 | Reserved.                                  | -    | -     |
| 0    | RNG_DEBUG_EN: * 1 - debug mode is enabled. | RW   | 0x0   |
|      | * 0 - debug mode is disabled               |      |       |

# <span id="page-1216-3"></span>**[TRNG:](#page-1212-1) TRNG\_BUSY Register**

**Offset**: 0x1b8

#### **Description**

RNG Busy indication.

*Table 1273. TRNG\_BUSY Register*

| Bits | Description                          | Type | Reset |
|------|--------------------------------------|------|-------|
| 31:1 | Reserved.                            | -    | -     |
| 0    | TRNG_BUSY: Reflects rng_busy status. | RO   | 0x0   |

# <span id="page-1217-0"></span>**[TRNG:](#page-1212-1) RST\_BITS\_COUNTER Register**

**Offset**: 0x1bc

#### **Description**

Reset the counter of collected bits in the RNG.

*Table 1274. RST\_BITS\_COUNTER Register*

| Bits | Description                                                                                                                                                                                  | Type | Reset |
|------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:1 | Reserved.                                                                                                                                                                                    | -    | -     |
| 0    | RST_BITS_COUNTER: Writing any value to this address will reset the bits<br>counter and RNG valid registers. RND_SORCE_ENABLE register must be unset<br>in order for the reset to take place. | RW   | 0x0   |

# <span id="page-1217-1"></span>**[TRNG:](#page-1212-1) RNG\_VERSION Register**

**Offset**: 0x1c0

#### **Description**

Displays the version settings of the TRNG.

*Table 1275. RNG\_VERSION Register*

| Bits | Description                         | Type | Reset |
|------|-------------------------------------|------|-------|
| 31:8 | Reserved.                           | -    | -     |
| 7    | RNG_USE_5_SBOXES: * 1 - 5 SBOX AES. | RO   | 0x0   |
|      | * 0 - 20 SBOX AES                   |      |       |
| 6    | RESEEDING_EXISTS: * 1 - Exists.     | RO   | 0x0   |
|      | * 0 - Does not exist                |      |       |
| 5    | KAT_EXISTS: * 1 - Exists.           | RO   | 0x0   |
|      | * 0 - Does not exist                |      |       |
| 4    | PRNG_EXISTS: * 1 - Exists.          | RO   | 0x0   |
|      | * 0 - Does not exist                |      |       |
| 3    | TRNG_TESTS_BYPASS_EN: * 1 - Exists. | RO   | 0x0   |
|      | * 0 - Does not exist                |      |       |
| 2    | AUTOCORR_EXISTS: * 1 - Exists.      | RO   | 0x0   |
|      | * 0 - Does not exist                |      |       |
| 1    | CRNGT_EXISTS: * 1 - Exists.         | RO   | 0x0   |
|      | * 0 - Does not exist                |      |       |
| 0    | EHR_WIDTH_192: * 1 - 192-bit EHR.   | RO   | 0x0   |
|      | * 0 - 128-bit EHR                   |      |       |

# <span id="page-1218-1"></span>**[TRNG:](#page-1212-1) RNG\_BIST\_CNTR\_0 Register**

**Offset**: 0x1e0

#### **Description**

Collected BIST results.

*Table 1276. RNG\_BIST\_CNTR\_0 Register*

| Bits  | Description                                              | Type | Reset    |
|-------|----------------------------------------------------------|------|----------|
| 31:22 | Reserved.                                                | -    | -        |
| 21:0  | ROSC_CNTR_VAL: Reflects the results of RNG BIST counter. | RO   | 0x000000 |

# <span id="page-1218-2"></span>**[TRNG:](#page-1212-1) RNG\_BIST\_CNTR\_1 Register**

**Offset**: 0x1e4

#### **Description**

Collected BIST results.

*Table 1277. RNG\_BIST\_CNTR\_1 Register*

| Bits  | Description                                              | Type | Reset    |
|-------|----------------------------------------------------------|------|----------|
| 31:22 | Reserved.                                                | -    | -        |
| 21:0  | ROSC_CNTR_VAL: Reflects the results of RNG BIST counter. | RO   | 0x000000 |

# <span id="page-1218-3"></span>**[TRNG:](#page-1212-1) RNG\_BIST\_CNTR\_2 Register**

**Offset**: 0x1e8

#### **Description**

Collected BIST results.

*Table 1278. RNG\_BIST\_CNTR\_2 Register*

| Bits  | Description                                              | Type | Reset    |
|-------|----------------------------------------------------------|------|----------|
| 31:22 | Reserved.                                                | -    | -        |
| 21:0  | ROSC_CNTR_VAL: Reflects the results of RNG BIST counter. | RO   | 0x000000 |

