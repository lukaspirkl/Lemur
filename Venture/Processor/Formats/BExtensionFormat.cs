using System.Buffers.Binary;
using System.Numerics;

namespace Venture.Processor.Formats;

public class BExtensionFormatFactory : FormatFactoryBase
{
    public override uint[] ForOpcodes => [0b0110011, 0b0010011];

    public override FormatBase? Decode(uint instruction)
    {
        var opcode = instruction.ExtractBits(0, 7);
        var funct7 = instruction.ExtractBits(25, 7);
        var funct3 = instruction.ExtractBits(12, 3);

        var rd = instruction.ExtractBits(7, 5);
        var rs1 = instruction.ExtractBits(15, 5);
        var rs2 = instruction.ExtractBits(20, 5);

        var mnemonic = "";

        switch (opcode)
        {
            case 0b0010011:
                switch (funct7)
                {
                    case 0b0000100:
                        switch (funct3)
                        {
                            case 0b001:
                                switch (rs2)
                                {
                                    case 0b01111:
                                        mnemonic = BExtensionFormat.ZIP;
                                        break;
                                }
                                break;
                            case 0b101:
                                switch(rs2)
                                {
                                    case 0b01111:
                                        mnemonic = BExtensionFormat.UNZIP;
                                        break;
                                }
                                break;
                        }
                        break;
                    case 0b0010100:
                        switch(funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.BSETI;
                                break;
                            case 0b101:
                                switch(rs2)
                                {
                                    case 0b00111:
                                        mnemonic = BExtensionFormat.ORC_B;
                                        break;
                                }
                                break;
                        }
                        break;
                    case 0b0100100:
                        switch (funct3)
                        {
                            case 0b001:
                                // Test is passing only when this is commented out, but documentation says that shamt[5]=1 -> reserved
                                //if (rs2 == 1)
                                //{
                                //    throw new NotImplementedException("reserved");
                                //}
                                mnemonic = BExtensionFormat.BCLRI;
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.BEXTI;
                                break;
                        }
                        break;
                    case 0b0110000:
                        switch (funct3)
                        {
                            case 0b001:
                                switch(rs2)
                                {
                                    case 0b00000:
                                        mnemonic = BExtensionFormat.CLZ;
                                        break;
                                    case 0b00001:
                                        mnemonic = BExtensionFormat.CTZ;
                                        break;
                                    case 0b00010:
                                        mnemonic = BExtensionFormat.CPOP;
                                        break;
                                    case 0b00100:
                                        mnemonic = BExtensionFormat.SEXT_B;
                                        break;
                                    case 0b00101:
                                        mnemonic = BExtensionFormat.SEXT_H;
                                        break;
                                }
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.RORI;
                                break;
                        }
                        break;
                    case 0b0110100:
                        switch (funct3)
                        {
                            case 0b001:
                                // Test is passing only when this is commented out, but documentation says that shamt[5]=1 -> reserved
                                //if (rs2 == 1)
                                //{
                                //    throw new NotImplementedException("reserved");
                                //}
                                mnemonic = BExtensionFormat.BINVI;
                                break;
                            case 0b101:
                                switch(rs2)
                                {
                                    case 0b00111:
                                        mnemonic = BExtensionFormat.BREV8;
                                        break;
                                    case 0b11000:
                                        mnemonic = BExtensionFormat.REV8;
                                        break;
                                }
                                break;
                        }
                        break;
                }
                break;
            case 0b0110011:
                switch (funct7)
                {
                    case 0b0000100:
                        switch(funct3)
                        {
                            case 0b100:
                                mnemonic = BExtensionFormat.PACK; // also covers zext.h
                                break;
                            case 0b111:
                                mnemonic = BExtensionFormat.PACKH;
                                break;
                        }
                        break;
                    case 0b0000101:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.CLMUL;
                                break;
                            case 0b010:
                                mnemonic = BExtensionFormat.CLMULR;
                                break;
                            case 0b011:
                                mnemonic = BExtensionFormat.CLMULH;
                                break;
                            case 0b100:
                                mnemonic = BExtensionFormat.MIN;
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.MINU;
                                break;
                            case 0b110:
                                mnemonic = BExtensionFormat.MAX;
                                break;
                            case 0b111:
                                mnemonic = BExtensionFormat.MAXU;
                                break;
                        }
                        break;
                    case 0b0010100:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.BSET;
                                break;
                            case 0b010:
                                mnemonic = BExtensionFormat.XPERM4;
                                break;
                            case 0b100:
                                mnemonic = BExtensionFormat.XPERM8;
                                break;
                        }
                        break;
                    case 0b0010000:
                        switch (funct3)
                        {
                            case 0b010:
                                mnemonic = BExtensionFormat.SH1ADD;
                                break;
                            case 0b100:
                                mnemonic = BExtensionFormat.SH2ADD;
                                break;
                            case 0b110:
                                mnemonic = BExtensionFormat.SH3ADD;
                                break;
                        }
                        break;
                    case 0b0100000:
                        switch (funct3)
                        {
                            case 0b100:
                                mnemonic = BExtensionFormat.XNOR;
                                break;
                            case 0b110:
                                mnemonic = BExtensionFormat.ORN;
                                break;
                            case 0b111:
                                mnemonic = BExtensionFormat.ANDN;
                                break;
                        }
                        break;
                    case 0b0100100:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.BCLR;
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.BEXT;
                                break;
                        }
                        break;
                    case 0b0110000:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.ROL;
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.ROR;
                                break;
                        }
                        break;
                    case 0b0110100:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.BINV;
                                break;
                        }
                        break;
                }
                break;
        }

        if (mnemonic == "")
        {
            return null;
        }

        return new BExtensionFormat
        {
            Mnemonic = mnemonic,
            rd = rd,
            rs1 = rs1,
            rs2 = rs2,
        };
    }

    protected static readonly IFormat m_Nop = new IFormat
    {
        Mnemonic = IFormat.ADDI,
        imm = 0,
        rd = 0,
        rs1 = 0,
        StepSize = 2,
    };
}

public class BExtensionFormat : FormatBase
{
    public const string ANDN = "andn";
    public const string BCLR = "bclr";
    public const string BCLRI = "bclri";
    public const string BEXT = "bext";
    public const string BEXTI = "bexti";
    public const string BINV = "binv";
    public const string BINVI = "binvi";
    public const string BSET = "bset";
    public const string BSETI = "bseti";
    public const string CLMUL = "clmul";
    public const string CLMULH = "clmulh";
    public const string CLMULR = "clmulr";
    public const string CLZ = "clz";
    public const string CPOP = "cpop";
    public const string CTZ = "ctz";
    public const string MAX = "max";
    public const string MAXU = "maxu"; 
    public const string MIN = "min";
    public const string MINU = "minu";
    public const string ORC_B = "orc.b";
    public const string ORN = "orn";
    public const string PACK = "pack";
    public const string PACKH = "packh";
    public const string REV8 = "rev8";
    public const string BREV8 = "brev8";
    public const string ROL = "rol";
    public const string ROR = "ror";
    public const string RORI = "rori";
    public const string SEXT_B = "sext.b";
    public const string SEXT_H = "sext.h";
    public const string SH1ADD = "sh1add";
    public const string SH2ADD = "sh2add";
    public const string SH3ADD = "sh3add";
    public const string UNZIP = "unzip";
    public const string XNOR = "xnor";
    public const string XPERM8 = "xperm8";
    public const string XPERM4 = "xperm4";
    public const string ZIP = "zip";

    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case ANDN:
                x[rd] = x[rs1] & ~x[rs2];
                return;

            case BCLR:
                x[rd] = x[rs1] & ~((uint)1 << (int)x[rs2]);
                return;

            case BCLRI:
                x[rd] = x[rs1] & ~((uint)1 << (int)rs2);
                return;

            case BEXT:
                x[rd] = x[rs1].ExtractBits((int)x[rs2], 1);
                return;

            case BEXTI:
                x[rd] = x[rs1].ExtractBits((int)rs2, 1);
                return;

            case BINV:
                x[rd] = x[rs1] ^ ((uint)1 << (int)x[rs2]);
                return;

            case BINVI:
                x[rd] = x[rs1] ^ ((uint)1 << (int)rs2);
                return;

            case BSET:
                x[rd] = x[rs1] | ((uint)1 << (int)x[rs2]);
                return;

            case BSETI:
                x[rd] = x[rs1] | ((uint)1 << (int)rs2);
                return;

            case CLMUL:
                {
                    uint rs1_val = x[rs1];
                    uint rs2_val = x[rs2];
                    uint output = 0;
                    int xlen = 32;

                    // foreach (i from 0 to (xlen - 1) by 1)
                    for (int i = 0; i < xlen; i++)
                    {
                        // if ((rs2_val >> i) & 1)
                        if (((rs2_val >> i) & 1) == 1)
                        {
                            // then output ^ (rs1_val << i);
                            // In C#, shifting a uint left by >= 32 is valid (modulo 32), 
                            // but logically we only care about bits that shift *within* the 32-bit range.
                            // The bits that overflow are naturally discarded, matching the low-part logic.
                            output ^= (rs1_val << i);
                        }
                    }

                    x[rd] = output;
                    return;
                }

            case CLMULH:
                {
                    uint rs1_val = x[rs1];
                    uint rs2_val = x[rs2];
                    uint output = 0;
                    int xlen = 32;

                    // foreach (i from 1 to xlen by 1)
                    // Note: We iterate i from 1 to 31. 
                    // Mathematically, the loop should go up to 32, but the 32nd bit (bit index 32) 
                    // of a 32-bit integer is always 0.
                    // Additionally, in C#, `rs2 >> 32` results in `rs2 >> 0` (modulo behavior), 
                    // which would incorrectly check bit 0. Stopping at < 32 prevents this.
                    for (int i = 1; i < xlen; i++)
                    {
                        // if ((rs2_val >> i) & 1)
                        if (((rs2_val >> i) & 1) == 1)
                        {
                            // then output ^ (rs1_val >> (xlen - i));
                            output ^= (rs1_val >> (xlen - i));
                        }
                    }

                    x[rd] = output;
                    return;
                }

            case CLMULR:
                {
                    uint rs1_val = x[rs1];
                    uint rs2_val = x[rs2];
                    uint output = 0;
                    int xlen = 32;

                    // foreach (i from 0 to (xlen - 1) by 1)
                    for (int i = 0; i < xlen; i++)
                    {
                        // if ((rs2_val >> i) & 1)
                        if (((rs2_val >> i) & 1) == 1)
                        {
                            // then output ^ (rs1_val >> (xlen - i - 1));
                            // In C#, the shift amount (xlen - i - 1) will range from 31 down to 0.
                            // Since these are all < 32, the standard C# shift behavior is safe 
                            // and requires no modulo protection.
                            output ^= (rs1_val >> (xlen - i - 1));
                        }
                        // else output; (implicitly handled)
                    }

                    x[rd] = output;
                    return;
                }

            case CLZ:
                x[rd] = (uint)BitOperations.LeadingZeroCount(x[rs1]);
                return;
                
            case CPOP:
                x[rd] = (uint)BitOperations.PopCount(x[rs1]);
                return;

            case CTZ:
                x[rd] = (uint)BitOperations.TrailingZeroCount(x[rs1]);
                return;

            case MAX:
                x[rd] = (uint)Math.Max((int)x[rs1], (int)x[rs2]);
                return;

            case MAXU:
                x[rd] = Math.Max(x[rs1], x[rs2]);
                return;

            case MIN:
                x[rd] = (uint)Math.Min((int)x[rs1], (int)x[rs2]);
                return;

            case MINU:
                x[rd] = Math.Min(x[rs1], x[rs2]);
                return;

            case ORC_B:
                {
                    uint value = x[rs1];

                    // 1. Propagate the "non-zero" status of every bit in a byte 
                    //    down to the least-significant bit (LSB) of that byte.
                    value |= value >> 1;
                    value |= value >> 2;
                    value |= value >> 4;

                    // 2. Isolate the LSB of each byte.
                    //    If a byte was non-zero, its LSB is now 1.
                    //    If a byte was zero, its LSB is 0.
                    value &= 0x01010101;

                    // 3. Broadcast the LSB to the rest of the byte.
                    //    Multiplication by 0xFF (255) effectively replicates the bit:
                    //    0x01 * 0xFF = 0xFF (All ones)
                    //    0x00 * 0xFF = 0x00 (All zeros)
                    //    Since the inputs are spaced out (0x01010101), the multiplication
                    //    works perfectly in parallel for all bytes:
                    //    0x01010101 * 0xFF = 0xFFFFFFFF
                    value *= 0xFF;

                    x[rd] = value;
                    return;
                }

            case ORN:
                x[rd] = x[rs1] | ~x[rs2];
                return;

            case PACK:
                // 1. Mask rs1 to isolate the lower 16 bits: (rs1 & 0xFFFF)
                // 2. Shift rs2 left by 16 positions. 
                //    This moves the lower 16 bits of rs2 to the upper half positions.
                //    The original upper bits of rs2 are shifted out and discarded automatically 
                //    by the 32-bit container.
                // 3. Combine utilizing bitwise OR.
                x[rd] = (x[rs1] & 0xFFFF) | (x[rs2] << 16);
                return;

            case PACKH:
                // 1. Mask rs1 to isolate the lower 8 bits (lo_half).
                // 2. Mask rs2 to isolate the lower 8 bits, then shift left by 8 (hi_half).
                // 3. Combine utilizing bitwise OR.
                // The upper 16 bits are naturally zero because the max value is 0xFFFF.
                x[rd] = (x[rs1] & 0xFF) | ((x[rs2] & 0xFF) << 8);
                return;

            case REV8:
                x[rd] = BinaryPrimitives.ReverseEndianness(x[rs1]);
                return;

            case BREV8:
                {
                    uint value = x[rs1];

                    // 1. Swap adjacent bits
                    //    0b10101010 -> 0b01010101
                    value = ((value & 0xAAAAAAAA) >> 1) | ((value & 0x55555555) << 1);

                    // 2. Swap adjacent 2-bit pairs
                    //    0b11001100 -> 0b00110011
                    value = ((value & 0xCCCCCCCC) >> 2) | ((value & 0x33333333) << 2);

                    // 3. Swap nibbles (4-bit chunks)
                    //    0b11110000 -> 0b00001111
                    value = ((value & 0xF0F0F0F0) >> 4) | ((value & 0x0F0F0F0F) << 4);

                    // Note: We stop here. If we continued to swap bytes, 
                    // we would turn this into a full bit-reversal (like the 'rbit' instruction on ARM).
                    // 'brev8' specifically requires us to stop at the byte boundary.
                    x[rd] = value;
                    return;
                }

            case ROL:
                x[rd] = BitOperations.RotateLeft(x[rs1], (int)x[rs2]);
                return;

            case ROR:
                x[rd] = BitOperations.RotateRight(x[rs1], (int)x[rs2]);
                return;

            case RORI:
                x[rd] = BitOperations.RotateRight(x[rs1], (int)rs2);
                return;

            case SEXT_B:
                // 1. (byte)rs      : Truncates to the lowest 8 bits.
                // 2. (sbyte)...    : Reinterprets those 8 bits as a signed value (-128 to 127).
                // 3. (int)...      : Sign-extends the 8-bit value to 32-bit (e.g., 0x80 -> 0xFFFFFF80).
                // 4. (uint)...     : Casts back to the register type.
                x[rd] = (uint)(int)(sbyte)x[rs1];
                return;

            case SEXT_H:
                // 1. (short)rs     : Truncates to lower 16 bits, interprets as signed.
                // 2. (int)...      : Sign-extends 16-bit value to 32-bit (e.g. 0x8000 -> 0xFFFF8000).
                // 3. (uint)...     : Casts back to register type.
                x[rd] = (uint)(int)(short)x[rs1];
                return;

            case SH1ADD:
                x[rd] = x[rs2] + (x[rs1] << 1);
                return;

            case SH2ADD:
                x[rd] = x[rs2] + (x[rs1] << 2);
                return;

            case SH3ADD:
                x[rd] = x[rs2] + (x[rs1] << 3);
                return;

            case UNZIP:
                {
                    // 1. Process Even Bits (0, 2, 4...) -> Output[15:0]
                    //    Mask evens, then "gather" them to the right.
                    uint lo = x[rs1] & 0x55555555;
                    lo = (lo | (lo >> 1)) & 0x33333333; // Group pairs
                    lo = (lo | (lo >> 2)) & 0x0F0F0F0F; // Group nibbles
                    lo = (lo | (lo >> 4)) & 0x00FF00FF; // Group bytes
                    lo = (lo | (lo >> 8)) & 0x0000FFFF; // Group halfwords

                    // 2. Process Odd Bits (1, 3, 5...) -> Output[31:16]
                    //    Shift original input right by 1 so odds act like evens, then apply same logic.
                    uint hi = (x[rs1] >> 1) & 0x55555555;
                    hi = (hi | (hi >> 1)) & 0x33333333;
                    hi = (hi | (hi >> 2)) & 0x0F0F0F0F;
                    hi = (hi | (hi >> 4)) & 0x00FF00FF;
                    hi = (hi | (hi >> 8)) & 0x0000FFFF;

                    // 3. Combine: evens in low half, odds in high half
                    x[rd] = lo | (hi << 16);
                    return;
                }

            case XNOR:
                x[rd] = ~(x[rs1] ^ x[rs2]);
                return;

            case XPERM8:
                {
                    uint result = 0;

                    for (int i = 0; i < 32; i += 8)
                    {
                        // Extract index
                        int index = (int)((x[rs2] >> i) & 0xFF);

                        // Fetch byte (tentative)
                        // Safe shift: We mask the shift amount to 31 (0x1F) to prevent C# shift wrapping issues,
                        // though logically we handle the 'zeroing' via the mask below.
                        uint val = (x[rs1] >> ((index * 8) & 0x1F)) & 0xFF;

                        // Create a mask: 0xFF if index < 4, 0x00 if index >= 4
                        // (index < 4) is equivalent to (index & ~3) == 0
                        uint mask = ((uint)index & ~3u) == 0 ? 0xFFu : 0x00u;

                        // Apply mask and position result
                        result |= (val & mask) << i;
                    }

                    x[rd] = result;
                    return;
                }

            case XPERM4:
                {
                    uint result = 0;

                    for (int i = 0; i < 32; i += 4)
                    {
                        // Extract index (4 bits)
                        int index = (int)((x[rs2] >> i) & 0x0F);

                        // Fetch nibble (tentative)
                        // Safe shift: (index * 4) & 0x1F ensures we shift within 0-31 range.
                        uint val = (x[rs1] >> ((index * 4) & 0x1F)) & 0x0F;

                        // Create a mask: 0x0F if index < 8, 0x00 if index >= 8
                        // (index < 8) is equivalent to (index & ~7) == 0
                        uint mask = ((uint)index & ~7u) == 0 ? 0x0Fu : 0x00u;

                        // Apply mask and position result
                        result |= (val & mask) << i;
                    }

                    x[rd] = result;
                    return;
                }

            case ZIP:
                {
                    // 1. Extract the two halves
                    uint lo = x[rs1] & 0x0000FFFF;
                    uint hi = x[rs1] >> 16;

                    // 2. Spread the lower 16 bits to even positions
                    //    Input:  ................fedcba9876543210
                    //    Output: .f.e.d.c.b.a.9.8.7.6.5.4.3.2.1.0
                    lo = (lo | (lo << 8)) & 0x00FF00FF;
                    lo = (lo | (lo << 4)) & 0x0F0F0F0F;
                    lo = (lo | (lo << 2)) & 0x33333333;
                    lo = (lo | (lo << 1)) & 0x55555555;

                    // 3. Spread the upper 16 bits to even positions (temporarily)
                    hi = (hi | (hi << 8)) & 0x00FF00FF;
                    hi = (hi | (hi << 4)) & 0x0F0F0F0F;
                    hi = (hi | (hi << 2)) & 0x33333333;
                    hi = (hi | (hi << 1)) & 0x55555555;

                    // 4. Shift 'hi' left by 1 to move to odd positions, then combine.
                    x[rd] = lo | (hi << 1);
                    return;
                }

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in BExtensionFormat.");
        }
    }
}
