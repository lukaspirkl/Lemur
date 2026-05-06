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

    // When true (Hazard3 default): mtval reads always return 0, writes are
    // discarded. When false: mtval stores and returns the trap value written
    // by the trap-entry sequence, as required by the full RISC-V spec.
    public bool MtvalHardwiredToZero { get; set; } = true;
}
