namespace Venture.InstructionFormats;

public class BTypeInstructionFormat : InstructionFormatBase
{
    public int imm_b { get; }

    public BTypeInstructionFormat(uint instruction)
        : base(instruction)
    {
        // R-Type and I-Type fields are extracted by the base class:
        // rs1: Bits 19-15
        // rs2: Bits 24-20
        // funct3: Bits 14-12
        // opcode: Bits 6-0

        // The B-Type immediate field is non-contiguous (scrambled) and is implicitly scaled by 2 (bit 0 is always 0).

        // 1. Extract Immediate Fragments (using the instruction bit positions, not the immediate bit positions)

        // Bits 7 and 31 (Sign and Offset[11])
        uint imm_11 = (instruction >> 7) & 0x01; // inst[7] -> imm[11]
        uint imm_12 = (instruction >> 31) & 0x01; // inst[31] -> imm[12] (Sign Bit)

        // Bits 30-25 (Offset[10:5])
        uint imm_10_5 = (instruction >> 25) & 0x3F; // inst[30:25] -> imm[10:5]

        // Bits 11-8 (Offset[4:1])
        uint imm_4_1 = (instruction >> 8) & 0x0F; // inst[11:8] -> imm[4:1]

        // 2. Reassemble the 13-bit Immediate (imm[12] is the sign bit)
        // The final immediate is always shifted left by 1 because the B-Type offset is always a multiple of 2.
        uint unsigned_imm_b_shifted =
                            (imm_12 << 12)       // imm[12] (Sign Bit)
                          | (imm_11 << 11)       // imm[11]
                          | (imm_10_5 << 5)      // imm[10:5]
                          | (imm_4_1 << 1);      // imm[4:1]

        // 3. Sign Extension (Convert 13-bit signed value to 32-bit signed int)
        // Check the sign bit (imm[12]) and extend if necessary.

        // Note: The J-Type class logic is used as a model, but for B-Type the sign bit is imm[12] *before* the final left shift.
        // The most compact C# way is to shift the 13-bit number to the 31st position, cast to int, and shift back.

        // Temporary 32-bit integer holding the 13-bit immediate shifted to the MSB
        int temp_imm = (int)(unsigned_imm_b_shifted << (31 - 12));

        // Shift back and store the final sign-extended, 2's complement value
        // The value is already implicitly multiplied by 2 due to the assembly encoding pattern.
        imm_b = temp_imm >> (31 - 12);
    }
}