namespace Lemur;

// Runtime switches for behaviour that differs between a strict Hazard3 implementation
// and the full RISC-V specification.
//
// Default values match Hazard3 hardware. Set flags to true to enable the
// corresponding full-spec behaviour, which is useful for compliance testing.
public class RiscVConfig
{
    // When false (Hazard3 default): A=TOR (value 1) in pmpcfg is sanitised to
    // OFF on write and never matched. When true: TOR is stored and enforced.
    public bool TorEnabled { get; set; } = false;

    // Number of PMP regions to support. Hazard3 on RP2350 has 11 (8 dynamic + 3 hardwired).
    // Compliance tests may expect more.
    public int PmpRegionsCount { get; set; } = 16;

    // G parameter for PMP grain. Minimum region size is 2^(G+2).
    // Hazard3 on RP2350 uses G=3 (32-byte grain).
    // Standard RISC-V default is G=0 (4-byte grain).
    public int PmpGrain { get; set; } = 0;

    // Hazard3 RP2350-E6 errata: PMPCFG RWX bits are transposed (R,W,X instead of X,W,R).
    // When true, bits 0 and 2 are swapped on read/write.
    public bool TransposedPmpBits { get; set; } = false;

    // When true (Hazard3 default): mtval reads always return 0, writes are
    // discarded. When false: mtval stores and returns the trap value written
    // by the trap-entry sequence, as required by the full RISC-V spec.
    public bool MtvalHardwiredToZero { get; set; } = true;
}
