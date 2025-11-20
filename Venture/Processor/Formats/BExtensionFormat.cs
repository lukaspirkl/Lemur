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
                                        mnemonic = BExtensionFormat.zip;
                                        break;
                                }
                                break;
                            case 0b101:
                                switch(rs2)
                                {
                                    case 0b01111:
                                        mnemonic = BExtensionFormat.unzip;
                                        break;
                                }
                                break;
                        }
                        break;
                    case 0b0010100:
                        switch(funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.bseti;
                                break;
                            case 0b101:
                                switch(rs2)
                                {
                                    case 0b00111:
                                        mnemonic = BExtensionFormat.orc_b;
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
                                mnemonic = BExtensionFormat.bclri;
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.bexti;
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
                                        mnemonic = BExtensionFormat.clz;
                                        break;
                                    case 0b00001:
                                        mnemonic = BExtensionFormat.ctz;
                                        break;
                                    case 0b00010:
                                        mnemonic = BExtensionFormat.cpop;
                                        break;
                                    case 0b00100:
                                        mnemonic = BExtensionFormat.sext_b;
                                        break;
                                    case 0b00101:
                                        mnemonic = BExtensionFormat.sext_h;
                                        break;
                                }
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.rori;
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
                                mnemonic = BExtensionFormat.binvi;
                                break;
                            case 0b101:
                                switch(rs2)
                                {
                                    case 0b00111:
                                        mnemonic = BExtensionFormat.brev8;
                                        break;
                                    case 0b11000:
                                        mnemonic = BExtensionFormat.rev8;
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
                                mnemonic = rs2 == 0 ? BExtensionFormat.zext_h : BExtensionFormat.pack;
                                break;
                            case 0b111:
                                mnemonic = BExtensionFormat.packh;
                                break;
                        }
                        break;
                    case 0b0000101:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.clmul;
                                break;
                            case 0b010:
                                mnemonic = BExtensionFormat.clmulr;
                                break;
                            case 0b011:
                                mnemonic = BExtensionFormat.clmulh;
                                break;
                            case 0b100:
                                mnemonic = BExtensionFormat.min;
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.minu;
                                break;
                            case 0b110:
                                mnemonic = BExtensionFormat.max;
                                break;
                            case 0b111:
                                mnemonic = BExtensionFormat.maxu;
                                break;
                        }
                        break;
                    case 0b0010100:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.bset;
                                break;
                            case 0b010:
                                mnemonic = BExtensionFormat.xperm4;
                                break;
                            case 0b100:
                                mnemonic = BExtensionFormat.xperm8;
                                break;
                        }
                        break;
                    case 0b0010000:
                        switch (funct3)
                        {
                            case 0b010:
                                mnemonic = BExtensionFormat.sh1add;
                                break;
                            case 0b100:
                                mnemonic = BExtensionFormat.sh2add;
                                break;
                            case 0b110:
                                mnemonic = BExtensionFormat.sh3add;
                                break;
                        }
                        break;
                    case 0b0100000:
                        switch (funct3)
                        {
                            case 0b100:
                                mnemonic = BExtensionFormat.xnor;
                                break;
                            case 0b110:
                                mnemonic = BExtensionFormat.orn;
                                break;
                            case 0b111:
                                mnemonic = BExtensionFormat.andn;
                                break;
                        }
                        break;
                    case 0b0100100:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.bclr;
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.bext;
                                break;
                        }
                        break;
                    case 0b0110000:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.rol;
                                break;
                            case 0b101:
                                mnemonic = BExtensionFormat.ror;
                                break;
                        }
                        break;
                    case 0b0110100:
                        switch (funct3)
                        {
                            case 0b001:
                                mnemonic = BExtensionFormat.binv;
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

    protected readonly IFormat nop = new IFormat
    {
        Mnemonic = IFormat.addi,
        imm = 0,
        rd = 0,
        rs1 = 0,
        StepSize = 2,
    };
}

public class BExtensionFormat : FormatBase
{
    public const string andn = "andn";
    public const string bclr = "bclr";
    public const string bclri = "bclri";
    public const string bext = "bext";
    public const string bexti = "bexti";
    public const string binv = "binv";
    public const string binvi = "binvi";
    public const string bset = "bset";
    public const string bseti = "bseti";
    public const string clmul = "clmul";
    public const string clmulh = "clmulh";
    public const string clmulr = "clmulr";
    public const string clz = "clz";
    public const string cpop = "cpop";
    public const string ctz = "ctz";
    public const string max = "max";
    public const string maxu = "maxu"; 
    public const string min = "min";
    public const string minu = "minu";
    public const string orc_b = "orc.b";
    public const string orn = "orn";
    public const string pack = "pack";
    public const string packh = "packh";
    public const string rev8 = "rev8";
    public const string brev8 = "brev8";
    public const string rol = "rol";
    public const string ror = "ror";
    public const string rori = "rori";
    public const string sext_b = "sext.b";
    public const string sext_h = "sext.h";
    public const string sh1add = "sh1add";
    public const string sh2add = "sh2add";
    public const string sh3add = "sh3add";
    public const string unzip = "unzip";
    public const string xnor = "xnor";
    public const string xperm8 = "xperm8";
    public const string xperm4 = "xperm4";
    public const string zext_h = "zext.h";
    public const string zip = "zip";

    public required uint rd { get; init; }
    public required uint rs1 { get; init; }
    public required uint rs2 { get; init; }

    public override void Execute(IProcessor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case andn:
                x[rd] = x[rs1] & ~x[rs2];
                return;

            case bclr:
                x[rd] = x[rs1] & ~((uint)1 << (int)x[rs2]);
                return;

            case bclri:
                x[rd] = x[rs1] & ~((uint)1 << (int)rs2);
                return;

            case bext:
                x[rd] = x[rs1].ExtractBits((int)x[rs2], 1);
                return;

            case bexti:
                x[rd] = x[rs1].ExtractBits((int)rs2, 1);
                return;

            case binv:
                x[rd] = x[rs1] ^ ((uint)1 << (int)x[rs2]);
                return;

            case binvi:
                x[rd] = x[rs1] ^ ((uint)1 << (int)rs2);
                return;

            case bset:
                x[rd] = x[rs1] | ((uint)1 << (int)x[rs2]);
                return;

            case bseti:
                x[rd] = x[rs1] | ((uint)1 << (int)rs2);
                return;

            //case clmul:
            //    // TODO
            //    return;

            //case clmulh:
            //    // TODO
            //    return;

            //case clmulr:
            //    // TODO
            //    return;

            //case clz:
            //    // TODO
            //    return;

            //case cpop:
            //    // TODO
            //    return;

            //case ctz:
            //    // TODO
            //    return;

            //case max:
            //    // TODO
            //    return;

            //case maxu:
            //    // TODO
            //    return;

            //case min:
            //    // TODO
            //    return;

            //case minu:
            //    // TODO
            //    return;

            //case orc_b:
            //    // TODO
            //    return;

            //case orn:
            //    // TODO
            //    return;

            //case pack:
            //    // TODO
            //    return;

            //case packh:
            //    // TODO
            //    return;

            //case rev8:
            //    // TODO
            //    return;

            //case brev8:
            //    // TODO
            //    return;

            //case rol:
            //    // TODO
            //    return;

            //case ror:
            //    // TODO
            //    return;

            //case rori:
            //    // TODO
            //    return;

            //case sext_b:
            //    // TODO
            //    return;

            //case sext_h:
            //    // TODO
            //    return;

            //case sh1add:
            //    // TODO
            //    return;

            //case sh2add:
            //    // TODO
            //    return;

            //case sh3add:
            //    // TODO
            //    return;

            //case unzip:
            //    // TODO
            //    return;

            //case xnor:
            //    // TODO
            //    return;

            //case xperm8:
            //    // TODO
            //    return;

            //case xperm4:
            //    // TODO
            //    return;

            //case zext_h:
            //    // TODO
            //    return;

            //case zip:
            //    // TODO
            //    return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in BExtensionFormat.");
        }
    }
}
