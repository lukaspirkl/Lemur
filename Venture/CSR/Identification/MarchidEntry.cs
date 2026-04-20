namespace Venture.Csr.Identification;

// marchid — 0xf12 — hardwired: bit31=0 (open-source), bits30:0=0x1b (Hazard3 ID 27)
public class MarchidEntry : CsrEntry
{
    [EntryValue("30:0", "Hazard3 architecture ID (27 decimal, registered with RISC-V International)")]
    public uint ArchId => 0x1bu;

    public MarchidEntry() : base(0xf12, "marchid", "Identification") { }

    public override uint Read()            => 0x1bu; // bit 31 = 0 (open-source)
    public override void Write(uint value) { }
}
