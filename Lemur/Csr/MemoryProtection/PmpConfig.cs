namespace Lemur.Csr.MemoryProtection;

// Controls which optional PMP address-matching modes are active.
// Default (TorEnabled = false) matches Hazard3 behaviour: A=TOR (value 1) is
// treated as OFF on pmpcfg writes and ignored during address matching.
// Set TorEnabled = true for a full RISC-V implementation where TOR is stored
// and enforced as a top-of-range region.
public class PmpConfig
{
    public bool TorEnabled { get; set; } = false;
}
