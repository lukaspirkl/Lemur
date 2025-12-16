using Venture.Processor.Formats;

namespace Venture.Processor.CFormats;

public class ZcmpFormatFactory : CFormatFactoryBase
{
    public override uint[] ForQuadrant => [0b10];

    public override uint[] ForFunct3 => [0b101];

    public override FormatBase? Decode(uint funct3, uint instruction)
    {
        switch (instruction.ExtractBits(10, 3))
        {
            case 0b011:
                switch (instruction.ExtractBits(5, 2))
                {
                    case 0b01:
                        // cm.mvsa01:   101 011 sreg1 01 sreg2 10
                        return new ZcmpMVFormat
                        {
                            Mnemonic = ZcmpMVFormat.cm_mvsa01,
                            sreg1 = instruction.ExtractBits(7, 3),
                            sreg2 = instruction.ExtractBits(2, 3),
                            StepSize = 2,
                        };
                    case 0b11:
                        // cm.mva01s:   101 011 sreg1 11 sreg2 10
                        return new ZcmpMVFormat
                        {
                            Mnemonic = ZcmpMVFormat.cm_mva01s,
                            sreg1 = instruction.ExtractBits(7, 3),
                            sreg2 = instruction.ExtractBits(2, 3),
                            StepSize = 2,
                        };
                }
                break;
            case 0b110:
                switch (instruction.ExtractBits(8, 2))
                {
                    case 0b00:
                        // cm.push:    101 110 00 xxxx yy 10  (x: rlist y: spimm)
                        return new ZcmpPushPopFormat
                        {
                            Mnemonic = ZcmpPushPopFormat.cm_push,
                            rlist = instruction.ExtractBits(4, 4),
                            spimm = instruction.ExtractBits(2, 2),
                            StepSize = 2,
                        };
                    case 0b10:
                        // cm.pop:     101 110 10 xxxx yy 10  (x: rlist y: spimm)
                        return new ZcmpPushPopFormat
                        {
                            Mnemonic = ZcmpPushPopFormat.cm_pop,
                            rlist = instruction.ExtractBits(4, 4),
                            spimm = instruction.ExtractBits(2, 2),
                            StepSize = 2,
                        };
                }
                break;
            case 0b111:
                switch (instruction.ExtractBits(8, 2))
                {
                    case 0b00:
                        // cm.popretz: 101 111 00 xxxx yy 10  (x: rlist y: spimm[5:4])
                        return new ZcmpPushPopFormat
                        {
                            Mnemonic = ZcmpPushPopFormat.cm_popretz,
                            rlist = instruction.ExtractBits(4, 4),
                            spimm = instruction.ExtractBits(2, 2),
                            StepSize = 2,
                        };
                    case 0b10:
                        // cm.popret:  101 111 10 xxxx yy 10  (x: rlist y: spimm)
                        return new ZcmpPushPopFormat
                        {
                            Mnemonic = ZcmpPushPopFormat.cm_popret,
                            rlist = instruction.ExtractBits(4, 4),
                            spimm = instruction.ExtractBits(2, 2),
                            StepSize = 2,
                        };
                }
                break;
        }

        return null;
    }
}

public class ZcmpMVFormat : FormatBase
{
    public const string cm_mva01s = "cm.mva01s";
    public const string cm_mvsa01 = "cm.mvsa01";

    public required uint sreg1 { get; init; }
    public required uint sreg2 { get; init; }

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

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in ZcmpFormat.");
        }
    }

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
}


public class ZcmpPushPopFormat : FormatBase
{
    public const string cm_push = "cm.push";
    public const string cm_pop = "cm.pop";
    public const string cm_popret = "cm.popret";
    public const string cm_popretz = "cm.popretz";

    public required uint rlist { get; init; }
    public required uint spimm { get; init; }

    public override void Execute(Hazard3Processor e)
    {
        switch (Mnemonic)
        {
            case cm_push:
                ExecutePush(e);
                return;

            case cm_pop:
                ExecutePop(e);
                return;

            case cm_popret:
                ExecutePop(e);
                e.PC = e.Registers[1];
                return;

            case cm_popretz:
                ExecutePop(e);
                e.PC = e.Registers[1];
                e.Registers[10] = 0;
                return;

            default:
                throw new NotImplementedException($"Unimplemented mnemonic {Mnemonic} in ZcmpFormat.");
        }
    }

    private void ExecutePush(Hazard3Processor e)
    {
        uint sp = e.Registers[2];
        e.Registers[2] = (uint)(sp - CalculateStackAdjustment());

        foreach (var regIndex in GetRegisterList().Reverse())
        {
            sp -= 4;
            e.Memory.WriteWord(sp, e.Registers[regIndex]);
        }
    }

    private void ExecutePop(Hazard3Processor e)
    {
        uint sp = (uint)(e.Registers[2] + CalculateStackAdjustment());
        e.Registers[2] = sp;

        foreach (var regIndex in GetRegisterList().Reverse())
        {
            sp -= 4;
            e.Registers[regIndex] = e.Memory.ReadWord(sp);
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

        return stackAdjBase + ((int)spimm * 16);
    }

    private IEnumerable<uint> GetRegisterList()
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
