using Microsoft.Extensions.Logging.Testing;
using Venture;
using Venture.Processor;

namespace Tests.Processor;

public class InstructionTests
{
    [Fact]
    public void cm_mvsa01()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                                                          funct3     r1s    r2s
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_011_000_01_001_10));

        rp2350.Registers[8] = 0x00000000;
        rp2350.Registers[9] = 0x00000000;
        rp2350.Registers[10] = 0x11111111;
        rp2350.Registers[11] = 0x22222222;
        rp2350.Registers[32] = 0x20000000;

        rp2350.Step();

        Assert.Equal((uint)0x11111111, rp2350.Registers[8]);
        Assert.Equal((uint)0x22222222, rp2350.Registers[9]);
    }

    [Fact]
    public void cm_mva01s()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                                                          funct3     r1s    r2s
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_011_000_11_001_10));

        rp2350.Registers[10] = 0x00000000;
        rp2350.Registers[11] = 0x00000000;
        rp2350.Registers[8] = 0x11111111;
        rp2350.Registers[9] = 0x22222222;
        rp2350.Registers[32] = 0x20000000;

        rp2350.Step();

        Assert.Equal((uint)0x11111111, rp2350.Registers[10]);
        Assert.Equal((uint)0x22222222, rp2350.Registers[11]);
    }



    [Fact]
    public void cm_push()
    {
        var instruction = new Memory("instruction", 0x0, 1024 * 5, false, new FakeLogger<Memory>()); // 5kB
        // b872 cm.push { ra,s0 - s2},-16
        instruction.Write(0x0, [0x72, 0xb8]);

        // 0x400E0204
        var memory = new Memory("memory", 0x400E0000, 0x500, false, new FakeLogger<Memory>());

        var r = new Registers(new FakeLogger<Registers>());
        var m = new BusFabric([instruction, memory], new FakeLogger<BusFabric>());
        var csr = new CSR(new FakeLogger<CSR>());
        var e = new Hazard3Processor(m, new FakeLogger<Hazard3Processor>(), r, csr);
        e.Registers[01] = 0x000071C8;
        e.Registers[02] = 0x400E0208;
        e.Registers[03] = 0xFFFFFFE1;
        e.Registers[04] = 0x00000000;
        e.Registers[05] = 0x00000000;
        e.Registers[06] = 0x00000000;
        e.Registers[07] = 0x400E0000;
        e.Registers[08] = 0x00000000;
        e.Registers[09] = 0x00000000;
        e.Registers[10] = 0x400E0000;
        e.Registers[11] = 0x000071E2;
        e.Registers[12] = 0x00007684;
        e.Registers[13] = 0x000071E2;
        e.Registers[14] = 0x00000000;
        e.Registers[15] = 0x00000001;
        e.Registers[16] = 0x00000000;
        e.Registers[17] = 0x00000000;
        e.Registers[18] = 0x00000000;
        e.Registers[19] = 0x00000000;
        e.Registers[20] = 0x00000000;
        e.Registers[21] = 0x00000000;
        e.Registers[22] = 0x00000000;
        e.Registers[23] = 0x00000000;
        e.Registers[24] = 0x00000000;
        e.Registers[25] = 0x00000000;
        e.Registers[26] = 0x00000000;
        e.Registers[27] = 0x00000000;
        e.Registers[28] = 0x00000000;
        e.Registers[29] = 0x00000000;
        e.Registers[30] = 0x00000000;
        e.Registers[31] = 0x000074D6;

        e.PC = 0x0;
        e.Step();

        Assert.Equal((uint)0x000071C8, e.Registers[01]);
        Assert.Equal((uint)0x400E01F8, e.Registers[02]);
        Assert.Equal((uint)0xFFFFFFE1, e.Registers[03]);
        Assert.Equal((uint)0x00000000, e.Registers[04]);
        Assert.Equal((uint)0x00000000, e.Registers[05]);
        Assert.Equal((uint)0x00000000, e.Registers[06]);
        Assert.Equal((uint)0x400E0000, e.Registers[07]);
        Assert.Equal((uint)0x00000000, e.Registers[08]);
        Assert.Equal((uint)0x00000000, e.Registers[09]);
        Assert.Equal((uint)0x400E0000, e.Registers[10]);
        Assert.Equal((uint)0x000071E2, e.Registers[11]);
        Assert.Equal((uint)0x00007684, e.Registers[12]);
        Assert.Equal((uint)0x000071E2, e.Registers[13]);
        Assert.Equal((uint)0x00000000, e.Registers[14]);
        Assert.Equal((uint)0x00000001, e.Registers[15]);
        Assert.Equal((uint)0x00000000, e.Registers[16]);
        Assert.Equal((uint)0x00000000, e.Registers[17]);
        Assert.Equal((uint)0x00000000, e.Registers[18]);
        Assert.Equal((uint)0x00000000, e.Registers[19]);
        Assert.Equal((uint)0x00000000, e.Registers[20]);
        Assert.Equal((uint)0x00000000, e.Registers[21]);
        Assert.Equal((uint)0x00000000, e.Registers[22]);
        Assert.Equal((uint)0x00000000, e.Registers[23]);
        Assert.Equal((uint)0x00000000, e.Registers[24]);
        Assert.Equal((uint)0x00000000, e.Registers[25]);
        Assert.Equal((uint)0x00000000, e.Registers[26]);
        Assert.Equal((uint)0x00000000, e.Registers[27]);
        Assert.Equal((uint)0x00000000, e.Registers[28]);
        Assert.Equal((uint)0x00000000, e.Registers[29]);
        Assert.Equal((uint)0x00000000, e.Registers[30]);
        Assert.Equal((uint)0x000074D6, e.Registers[31]);
    }
}
