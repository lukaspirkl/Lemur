using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class ZcmpFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b10];

    public override uint[] ForFunct3 => [0b101];

    // Zcmp instructions exist in Quadrant 2 (ending in 10)
    // We register for the generic compressed opcodes that might contain Zcmp
    // In a 32-bit container, these end in 0b10 (0x2).
    //public override uint[] ForOpcodes => [0b0000010, 0b0100010, 0b1000010, 0b1100010];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        // Use Bits [12:10] to distinguish Zcmp operations
        var subOp = instruction.ExtractBits(10, 3);
        var mnemonic = "";

        // Generic fields (will be populated based on specific instruction)
        uint rlist = 0;
        uint spimm = 0;
        uint sreg1 = 0;
        uint sreg2 = 0;

        switch (subOp)
        {
            // cm.mvsa01: 101 001 sreg1 sreg2 10
            case 0b001:
                mnemonic = ZcmpFormat.cm_mvsa01;
                sreg1 = instruction.ExtractBits(7, 3);
                sreg2 = instruction.ExtractBits(4, 3);
                break;

            // cm.mva01s: 101 011 sreg1 sreg2 10
            case 0b011:
                mnemonic = ZcmpFormat.cm_mva01s;
                sreg1 = instruction.ExtractBits(7, 3);
                sreg2 = instruction.ExtractBits(4, 3);
                break;

            // cm.push: 101 11000 rlist spimm 10
            // Top 5 bits [15:11] must be 10111. 
            // We checked funct3=101. subOp (12:10) is 110.
            // But we must ensure bit 12 is 1 and bit 11 is 0. 
            // Actually, subOp 110 covers (11000 and 11010).
            // Let's refine the switch or use bit 12-8 logic for Push/Pop.
            case 0b110:
            case 0b111:
                // Push/Pop area. Let's look at bits [12:8] specifically.
                // Re-extract bits 12-8 for precise matching
                var top5 = instruction.ExtractBits(8, 5); // 12 down to 8? No, ExtractBits(start, len) usually?
                // Assuming ExtractBits(offset, count):
                var code = instruction.ExtractBits(8, 5); // Extracting bits 12:8

                // cm.push:    11000 (0x18)
                // cm.pop:     11010 (0x1A)
                // cm.popret:  11100 (0x1C)
                // cm.popretz: 11110 (0x1E)

                if (code == 0b11000) mnemonic = ZcmpFormat.cm_push;
                else if (code == 0b11010) mnemonic = ZcmpFormat.cm_pop;
                else if (code == 0b11100) mnemonic = ZcmpFormat.cm_popret;
                else if (code == 0b11110) mnemonic = ZcmpFormat.cm_popretz;
                else return null;

                rlist = instruction.ExtractBits(4, 4);
                spimm = instruction.ExtractBits(2, 2);
                break;

            default:
                return null;
        }

        if (mnemonic == "") return null;

        return new ZcmpFormat
        {
            Mnemonic = mnemonic,
            rlist = rlist,
            spimm = spimm,
            sreg1 = sreg1,
            sreg2 = sreg2,
            StepSize = 2,
        };
    }
}

public class ZcmpFormat : FormatBase
{
    public const string cm_push = "cm.push";
    public const string cm_pop = "cm.pop";
    public const string cm_popret = "cm.popret";
    public const string cm_popretz = "cm.popretz";
    public const string cm_mva01s = "cm.mva01s";
    public const string cm_mvsa01 = "cm.mvsa01";

    public required uint rlist { get; init; } // 4 bits
    public required uint spimm { get; init; } // 2 bits
    public required uint sreg1 { get; init; } // 3 bits
    public required uint sreg2 { get; init; } // 3 bits

    private uint MapSReg(uint sregVal)
    {
        return sregVal switch
        {
            0 => 8,  // s0
            1 => 9,  // s1
            2 => 18, // s2
            3 => 19, // s3
            4 => 20, // s4
            5 => 21, // s5
            6 => 22, // s6
            7 => 23, // s7
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override void Execute(Hazard3Processor e)
    {
        var x = e.Registers;

        switch (Mnemonic)
        {
            case cm_mva01s:
                x[10] = x[MapSReg(sreg1)];
                x[11] = x[MapSReg(sreg2)];
                return;

            case cm_mvsa01:
                x[MapSReg(sreg1)] = x[10];
                x[MapSReg(sreg2)] = x[11];
                return;

            case cm_push:
                ExecutePush(e);
                return;

            case cm_pop:
            case cm_popret:
            case cm_popretz:
                ExecutePop(e);
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in ZcmpFormat.");
        }
    }

    private void ExecutePush(Hazard3Processor e)
    {
        int stackAdj = CalculateStackAdjustment();

        uint sp = e.Registers[2]; // x2 is SP
        uint spStart = sp;
        uint spNew = (uint)(sp - stackAdj);
        e.Registers[2] = spNew;

        var regsToSave = GetRegisterList();

        // Push order: RA is at the highest address.
        // Addresses: sp_new + stack_adj - 4, sp_new + stack_adj - 8, ...
        // We iterate the list (which starts with RA, then S0...) and decrement address.

        uint currentAddr = spStart;

        foreach (var regIndex in regsToSave)
        {
            currentAddr -= 4;
            e.Memory.WriteWord(currentAddr, e.Registers[regIndex]);
        }
    }

    private void ExecutePop(Hazard3Processor e)
    {
        int stackAdj = CalculateStackAdjustment();
        uint sp = e.Registers[2];

        var regsToLoad = GetRegisterList();

        // Pop order: Same addresses as push.
        // RA at sp + stack_adj - 4.

        uint currentAddr = sp + (uint)stackAdj;

        foreach (var regIndex in regsToLoad)
        {
            currentAddr -= 4;
            e.Registers[regIndex] = e.Memory.ReadWord(currentAddr);
        }

        // Deallocate stack
        e.Registers[2] = sp + (uint)stackAdj;

        if (Mnemonic == cm_popret || Mnemonic == cm_popretz)
        {
            e.PC = e.Registers[1]; // ret

            if (Mnemonic == cm_popretz)
            {
                e.Registers[10] = 0; // a0 = 0
            }
        }
    }

    private int CalculateStackAdjustment()
    {
        int stackAdjBase;
        if (rlist >= 4 && rlist <= 7) stackAdjBase = 16;
        else if (rlist >= 8 && rlist <= 11) stackAdjBase = 32;
        else if (rlist >= 12 && rlist <= 14) stackAdjBase = 48;
        else if (rlist == 15) stackAdjBase = 64;
        else throw new InvalidOperationException($"Invalid rlist {rlist} for Zcmp Push/Pop");

        // total = base + (spimm * 16)
        return stackAdjBase + ((int)spimm * 16);
    }

    private List<uint> GetRegisterList()
    {
        var list = new List<uint>();

        if (rlist < 4) throw new InvalidOperationException($"Invalid rlist {rlist} for Zcmp Push/Pop");

        list.Add(1);
        if (rlist == 4) return list;

        list.Add(8);
        if (rlist == 5) return list;

        list.Add(9);
        if (rlist == 6) return list;

        list.Add(18);
        if (rlist == 7) return list;

        list.Add(19);
        if (rlist == 8) return list;

        list.Add(20);
        if (rlist == 9) return list;

        list.Add(21);
        if (rlist == 10) return list;

        list.Add(22);
        if (rlist == 11) return list;

        list.Add(23);
        if (rlist == 12) return list;

        list.Add(24);
        if (rlist == 13) return list;

        list.Add(25);
        if (rlist == 14) return list;

        list.Add(26);
        list.Add(27);
        if (rlist == 15) return list;

        throw new InvalidOperationException($"Invalid rlist {rlist} for Zcmp Push/Pop");
    }
}