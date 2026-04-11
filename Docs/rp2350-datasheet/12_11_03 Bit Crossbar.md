# 12.11.3 Bit Crossbar

The bit crossbar controls which bits of the output shift register appear on which GPIOs during the first and second half of each HSTX clock cycle. There is a configuration register for each pin, [BIT0](#page-1207-0) through [BIT7:](#page-1207-0)

- BITx.SEL\_P selects which shift register bit (0 through 31) is output for the first half of each HSTX clock cycle
- BITx.SEL\_N selects which shift register bit (0 through 31) is output for the second half of each clock cycle
- BITx.INV inverts the output (logical NOT)
- BITx.CLK indicates that this pin should be connected to the clock generator ([Section 12.11.4](#page-1202-0)) rather than the output shift register

To disable DDR behaviour set SEL\_N equal to SEL\_P. To implement a differential output, configure two pins identically except for the INV bit, which should be set for one pin and clear for the other.

#### **12.11.3.1. Examples: One Pin**

Together with the SHIFT and N\_SHIFTS controls for the shift register, the pin configuration determines the data layout passed through the HSTX. Since not all of us are accustomed to thinking in four dimensions, it's worth going through some examples with a single pin:

- N\_SHIFTS = 32, SHIFT = 1, SEL\_P = 0, SEL\_N = 0:
  - Shift out one bit per HSTX clock cycle, LSB-first.
  - Each cycle, the shift register advances to the right by one, and the least-significant bit at that time is presented to the pin for both halves of the cycle, since SEL\_P and SEL\_N both select the same bit.
- N\_SHIFTS = 32, SHIFT = 31, SEL\_P = 31, SEL\_N = 31:
  - Shift out one bit per HSTX clock cycle, MSB-first.
  - Each cycle, the shift register advances to the left by one (or rather, wraps around the right-hand edge of the register and ends up one bit left of where it started), and the most-significant bit at that time is presented to the pin.
- N\_SHIFTS = 16, SHIFT = 2, SEL\_P = 0, SEL\_N = 1:
  - Shift out two bits per HSTX clock cycle, LSB-first.
  - Each cycle, the shift register advances to the right by two. The least-significant bit is presented to the pin for the first half of that cycle, and the neighbouring bit is presented for the second half.
- N\_SHIFTS = 16, SHIFT = 30, SEL\_P = 31, SEL\_N = 30:
  - Shift out two bits per HSTX clock cycle, MSB-first.
  - Each cycle, the shift register advances to the left by two. The most-significant bit is presented to the pin for the first half of that cycle, and the neighbouring bit is presented for the latter half.
- N\_SHIFTS = 8, SHIFT = 4, SEL\_P = 0, SEL\_N = 0:
  - Shift out the least-significant bit in each group of 4 bits, over the course of 8 clock cycles.
  - Each cycle, the shift register advances by to the right by four. The least-significant bit of the shift register is presented to the pin. The bit indices presented to the pin are therefore 0, 4, 8, 12, 16, 20, 24, and 28.
- N\_SHIFTS = 32, SHIFT = 4, SEL\_P = 0, SEL\_N = 0:

- Same as the previous, but repeats the 8-cycle pattern four times before refreshing the shift register.
- Rotating by 32 restores the original value that was popped into the shift register from the FIFO or command expander.

#### **12.11.3.2. Examples: Multiple Pins**

The separation of shift register and bit crossbar allows both zipped and unzipped multi-bit records, once multiple pins are involved. For example, compare these two configurations:

```
• N_SHIFTS = 8, SHIFT = 4, BIT0.SEL_P = 0, BIT0.SEL_N = 2, BIT1.SEL_P = 1, BIT1.SEL_N = 3:
```

- Each 32-bit word consists of 16 bit-pairs, and a new bit-pair is presented to BIT0 and BIT1 twice per cycle.
- The shift register advances by 4 every cycle, introducing two new bit-pairs to the rightmost four bits of the shift register

```
• N_SHIFTS = 8, SHIFT = 2, BIT0.SEL_P = 0, BIT0.SEL_N = 1, BIT1.SEL_P = 16, BIT1.SEL_N = 17:
```

- Each 32-bit word consists of a pair of 16-bit values, each of which is shifted to one pin out of BIT0 and BIT1 at a rate of two bits per cycle.
- The shift register advances by two every cycle, introducing a new bit-pair to bits 1:0 for the BIT0 pin, and also introducing a new bit-pair to bits 17:16 for the BIT1 pin.

Depending on software needs, it may be preferable to pack together all of the bits output on the same cycle (zipped records), or all of the bits that go through the same pin (unzipped records), so HSTX supports both.

As a final, concrete example, take TMDS (used in DVI): here each 32-bit word contains 3 × 10-bit TMDS symbols, each of which is serialised to a differential pair over the course of 10 TMDS bit times. For performance, it's preferable to make each HSTX clock period equal to *two* TMDS bit periods, by leveraging the DDR capability. A possible configuration would therefore be:

```
• CSR: N_SHIFTS = 5, SHIFT = 2
• BIT0: SEL_P = 0, SEL_N = 1, INV = 0
• BIT1: SEL_P = 0, SEL_N = 1, INV = 1
• BIT2: SEL_P = 10, SEL_N = 11, INV = 0
• BIT3: SEL_P = 10, SEL_N = 11, INV = 1
• BIT4: SEL_P = 20, SEL_N = 21, INV = 0
• BIT5: SEL_P = 20, SEL_N = 21, INV = 1
```

The missing piece for TMDS is the clock, which has a period of 10 TMDS bit periods, or 5 HSTX clock periods when shifting two bits per cycle per pin. HSTX has a special-purpose clock generator so that pseudo-clock bits do not have to be packed into the FIFO data stream. The clock generator is covered in the next section.

