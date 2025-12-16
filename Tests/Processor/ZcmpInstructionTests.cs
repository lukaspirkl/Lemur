using Venture;

namespace Tests.Processor;

public class ZcmpInstructionTests
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
    public void cm_push_0110_00()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                        cm.push {ra, s0, s1}, -16         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11000_0110_00_10));

        rp2350.MemoryWrite(0x20001000 - 0x00, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x04, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x08, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x0c, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x10, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x14, new byte[4]);

        rp2350.Registers[1] = 0x08001234;      // ra
        rp2350.Registers[2] = 0x20001000;      // sp
        rp2350.Registers[8] = 0x11111111;      // s0
        rp2350.Registers[9] = 0x22222222;      // s1
        rp2350.Registers[18] = 0x33333333;     // s2

        rp2350.Registers[32] = 0x20000000;     // pc

        rp2350.Step();

        Assert.Equal((uint)0x20000002, rp2350.Registers[32]);      // pc

        Assert.Equal((uint)0x20001000 - 16, rp2350.Registers[2]);  // sp

        // All other registers unchanged
        Assert.Equal("0x08001234", rp2350.Registers[1].ToHex());
        Assert.Equal("0x11111111", rp2350.Registers[8].ToHex());
        Assert.Equal("0x22222222", rp2350.Registers[9].ToHex());

        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 4, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 8, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 12, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 16, 4)).ToHex());

        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x00, 4)).ToHex());
        Assert.Equal("0x22222222", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x04, 4)).ToHex());
        Assert.Equal("0x11111111", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x08, 4)).ToHex());
        Assert.Equal("0x08001234", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x0c, 4)).ToHex());
        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x10, 4)).ToHex());
        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x14, 4)).ToHex());
    }

    [Fact]
    public void cm_push_0111_00()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                    cm.push {ra, s0, s1, s2}, -16         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11000_0111_00_10));

        rp2350.MemoryWrite(0x20001000 - 0x00, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x04, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x08, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x0c, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x10, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x14, new byte[4]);

        rp2350.Registers[1] = 0x08001234;      // ra
        rp2350.Registers[2] = 0x20001000;      // sp
        rp2350.Registers[8] = 0x11111111;      // s0
        rp2350.Registers[9] = 0x22222222;      // s1
        rp2350.Registers[18] = 0x33333333;     // s2

        rp2350.Registers[32] = 0x20000000;     // pc

        rp2350.Step();

        Assert.Equal((uint)0x20000002, rp2350.Registers[32]);      // pc

        Assert.Equal((uint)0x20001000 - 16, rp2350.Registers[2]);  // sp

        // All other registers unchanged
        Assert.Equal("0x08001234", rp2350.Registers[1].ToHex());  
        Assert.Equal("0x11111111", rp2350.Registers[8].ToHex());  
        Assert.Equal("0x22222222", rp2350.Registers[9].ToHex());
        Assert.Equal("0x33333333", rp2350.Registers[18].ToHex());

        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 4, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 8, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 12, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 16, 4)).ToHex());

        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x00, 4)).ToHex());
        Assert.Equal("0x33333333", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x04, 4)).ToHex());
        Assert.Equal("0x22222222", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x08, 4)).ToHex());
        Assert.Equal("0x11111111", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x0c, 4)).ToHex());
        Assert.Equal("0x08001234", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x10, 4)).ToHex());
        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x14, 4)).ToHex());
    }

    [Fact]
    public void cm_push_1111_00()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                    cm.push {ra, s0, s1, s2}, -16         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11000_1111_00_10));

        // Clear the destination memory
        for (uint i = 0; i < 16; i++)
        {
            rp2350.MemoryWrite(0x20001000 - (i * 4), new byte[4]);
        }

        rp2350.Registers[01] = 0x08001234;     // ra
        rp2350.Registers[08] = 0x11111111;     // s0
        rp2350.Registers[09] = 0x22222222;     // s1
        rp2350.Registers[18] = 0x33333333;     // s2
        rp2350.Registers[19] = 0x44444444;     // s3
        rp2350.Registers[20] = 0x55555555;     // s4
        rp2350.Registers[21] = 0x66666666;     // s5
        rp2350.Registers[22] = 0x77777777;     // s6
        rp2350.Registers[23] = 0x88888888;     // s7
        rp2350.Registers[24] = 0x99999999;     // s8
        rp2350.Registers[25] = 0xAAAAAAAA;     // s9
        rp2350.Registers[26] = 0xBBBBBBBB;     // s10
        rp2350.Registers[27] = 0xCCCCCCCC;     // s11

        rp2350.Registers[2] = 0x20001000;      // sp
        rp2350.Registers[32] = 0x20000000;     // pc

        rp2350.Step();

        Assert.Equal((uint)0x20000002, rp2350.Registers[32]);      // pc
        Assert.Equal((uint)0x20001000 - 64, rp2350.Registers[2]);  // sp

        // All other registers unchanged
        Assert.Equal("0x08001234", rp2350.Registers[01].ToHex());
        Assert.Equal("0x11111111", rp2350.Registers[08].ToHex());
        Assert.Equal("0x22222222", rp2350.Registers[09].ToHex());
        Assert.Equal("0x33333333", rp2350.Registers[18].ToHex());
        Assert.Equal("0x44444444", rp2350.Registers[19].ToHex());
        Assert.Equal("0x55555555", rp2350.Registers[20].ToHex());
        Assert.Equal("0x66666666", rp2350.Registers[21].ToHex());
        Assert.Equal("0x77777777", rp2350.Registers[22].ToHex());
        Assert.Equal("0x88888888", rp2350.Registers[23].ToHex());
        Assert.Equal("0x99999999", rp2350.Registers[24].ToHex());
        Assert.Equal("0xAAAAAAAA", rp2350.Registers[25].ToHex());
        Assert.Equal("0xBBBBBBBB", rp2350.Registers[26].ToHex());
        Assert.Equal("0xCCCCCCCC", rp2350.Registers[27].ToHex());

        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x00, 4)).ToHex());
        Assert.Equal("0xCCCCCCCC", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x04, 4)).ToHex());
        Assert.Equal("0xBBBBBBBB", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x08, 4)).ToHex());
        Assert.Equal("0xAAAAAAAA", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x0c, 4)).ToHex());
        Assert.Equal("0x99999999", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x10, 4)).ToHex());
        Assert.Equal("0x88888888", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x14, 4)).ToHex());
        Assert.Equal("0x77777777", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x18, 4)).ToHex());
        Assert.Equal("0x66666666", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x1c, 4)).ToHex());
        Assert.Equal("0x55555555", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x20, 4)).ToHex());
        Assert.Equal("0x44444444", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x24, 4)).ToHex());
        Assert.Equal("0x33333333", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x28, 4)).ToHex());
        Assert.Equal("0x22222222", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x2c, 4)).ToHex());
        Assert.Equal("0x11111111", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x30, 4)).ToHex());
        Assert.Equal("0x08001234", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x34, 4)).ToHex());
        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x38, 4)).ToHex());
        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x3c, 4)).ToHex());
    }

    [Fact]
    public void cm_push_0111_01()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                    cm.push {ra, s0, s1, s2}, -32         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11000_0111_01_10));

        rp2350.MemoryWrite(0x20001000 - 0x00, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x04, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x08, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x0c, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x10, new byte[4]);
        rp2350.MemoryWrite(0x20001000 - 0x14, new byte[4]);

        rp2350.Registers[1] = 0x08001234;      // ra
        rp2350.Registers[2] = 0x20001000;      // sp
        rp2350.Registers[8] = 0x11111111;      // s0
        rp2350.Registers[9] = 0x22222222;      // s1
        rp2350.Registers[18] = 0x33333333;     // s2

        rp2350.Registers[32] = 0x20000000;     // pc

        rp2350.Step();

        Assert.Equal((uint)0x20000002, rp2350.Registers[32]);      // pc

        Assert.Equal((uint)0x20001000 - 32, rp2350.Registers[2]);  // sp

        // All other registers unchanged
        Assert.Equal("0x08001234", rp2350.Registers[1].ToHex());
        Assert.Equal("0x11111111", rp2350.Registers[8].ToHex());
        Assert.Equal("0x22222222", rp2350.Registers[9].ToHex());
        Assert.Equal("0x33333333", rp2350.Registers[18].ToHex());

        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 -  0, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 -  4, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 -  8, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 12, 4)).ToHex());
        //Console.WriteLine(BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 16, 4)).ToHex());

        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x00, 4)).ToHex());
        Assert.Equal("0x33333333", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x04, 4)).ToHex());
        Assert.Equal("0x22222222", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x08, 4)).ToHex());
        Assert.Equal("0x11111111", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x0c, 4)).ToHex());
        Assert.Equal("0x08001234", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x10, 4)).ToHex());
        Assert.Equal("0x00000000", BitConverter.ToUInt32(rp2350.MemoryRead(0x20001000 - 0x14, 4)).ToHex());
    }

    [Fact]
    public void cm_pop_0110_00()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                         cm.pop {ra, s0, s1}, -16         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11010_0110_00_10));

        rp2350.MemoryWrite(0x20001000 - 0x00, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x04, BitConverter.GetBytes((uint)0x22222222));
        rp2350.MemoryWrite(0x20001000 - 0x08, BitConverter.GetBytes((uint)0x11111111));
        rp2350.MemoryWrite(0x20001000 - 0x0c, BitConverter.GetBytes((uint)0x08001234));
        rp2350.MemoryWrite(0x20001000 - 0x10, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x14, BitConverter.GetBytes((uint)0x00000000));

        // Clear all registers
        for (uint i = 0; i < rp2350.Registers.Length; i++)
        {
            rp2350.Registers[i] = 0x0; 
        }

        rp2350.Registers[2] = 0x20001000 - 16; // sp
        rp2350.Registers[32] = 0x20000000;     // pc

        rp2350.Step();

        Assert.Equal((uint)0x20000002, rp2350.Registers[32]); // pc
        Assert.Equal((uint)0x20001000, rp2350.Registers[2]);  // sp

        Assert.Equal("0x08001234", rp2350.Registers[1].ToHex());
        Assert.Equal("0x11111111", rp2350.Registers[8].ToHex());
        Assert.Equal("0x22222222", rp2350.Registers[9].ToHex());
    }

    [Fact]
    public void cm_pop_0111_00()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                     cm.pop {ra, s0, s1, s2}, -16         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11010_0111_00_10));

        rp2350.MemoryWrite(0x20001000 - 0x00, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x04, BitConverter.GetBytes((uint)0x33333333));
        rp2350.MemoryWrite(0x20001000 - 0x08, BitConverter.GetBytes((uint)0x22222222));
        rp2350.MemoryWrite(0x20001000 - 0x0c, BitConverter.GetBytes((uint)0x11111111));
        rp2350.MemoryWrite(0x20001000 - 0x10, BitConverter.GetBytes((uint)0x08001234));
        rp2350.MemoryWrite(0x20001000 - 0x14, BitConverter.GetBytes((uint)0x00000000));

        // Clear all registers
        for (uint i = 0; i < rp2350.Registers.Length; i++)
        {
            rp2350.Registers[i] = 0x0;
        }

        rp2350.Registers[2] = 0x20001000 - 16; // sp
        rp2350.Registers[32] = 0x20000000;     // pc

        rp2350.Step();

        Assert.Equal((uint)0x20000002, rp2350.Registers[32]); // pc
        Assert.Equal((uint)0x20001000, rp2350.Registers[2]);  // sp

        // All other registers unchanged
        Assert.Equal("0x08001234", rp2350.Registers[1].ToHex());
        Assert.Equal("0x11111111", rp2350.Registers[8].ToHex());
        Assert.Equal("0x22222222", rp2350.Registers[9].ToHex());
        Assert.Equal("0x33333333", rp2350.Registers[18].ToHex());
    }

    [Fact]
    public void cm_pop_1111_00()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                     cm.pop {ra, s0, s1, s2}, -16         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11010_1111_00_10));

        rp2350.MemoryWrite(0x20001000 - 0x00, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x04, BitConverter.GetBytes((uint)0xCCCCCCCC));
        rp2350.MemoryWrite(0x20001000 - 0x08, BitConverter.GetBytes((uint)0xBBBBBBBB));
        rp2350.MemoryWrite(0x20001000 - 0x0c, BitConverter.GetBytes((uint)0xAAAAAAAA));
        rp2350.MemoryWrite(0x20001000 - 0x10, BitConverter.GetBytes((uint)0x99999999));
        rp2350.MemoryWrite(0x20001000 - 0x14, BitConverter.GetBytes((uint)0x88888888));
        rp2350.MemoryWrite(0x20001000 - 0x18, BitConverter.GetBytes((uint)0x77777777));
        rp2350.MemoryWrite(0x20001000 - 0x1c, BitConverter.GetBytes((uint)0x66666666));
        rp2350.MemoryWrite(0x20001000 - 0x20, BitConverter.GetBytes((uint)0x55555555));
        rp2350.MemoryWrite(0x20001000 - 0x24, BitConverter.GetBytes((uint)0x44444444));
        rp2350.MemoryWrite(0x20001000 - 0x28, BitConverter.GetBytes((uint)0x33333333));
        rp2350.MemoryWrite(0x20001000 - 0x2c, BitConverter.GetBytes((uint)0x22222222));
        rp2350.MemoryWrite(0x20001000 - 0x30, BitConverter.GetBytes((uint)0x11111111));
        rp2350.MemoryWrite(0x20001000 - 0x34, BitConverter.GetBytes((uint)0x08001234));
        rp2350.MemoryWrite(0x20001000 - 0x38, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x3c, BitConverter.GetBytes((uint)0x00000000));

        // Clear all registers
        for (uint i = 0; i < rp2350.Registers.Length; i++)
        {
            rp2350.Registers[i] = 0x0;
        }

        rp2350.Registers[01] = 0x08001234;     // ra
        rp2350.Registers[08] = 0x11111111;     // s0
        rp2350.Registers[09] = 0x22222222;     // s1
        rp2350.Registers[18] = 0x33333333;     // s2
        rp2350.Registers[19] = 0x44444444;     // s3
        rp2350.Registers[20] = 0x55555555;     // s4
        rp2350.Registers[21] = 0x66666666;     // s5
        rp2350.Registers[22] = 0x77777777;     // s6
        rp2350.Registers[23] = 0x88888888;     // s7
        rp2350.Registers[24] = 0x99999999;     // s8
        rp2350.Registers[25] = 0xAAAAAAAA;     // s9
        rp2350.Registers[26] = 0xBBBBBBBB;     // s10
        rp2350.Registers[27] = 0xCCCCCCCC;     // s11

        rp2350.Registers[2] = 0x20001000 - 64; // sp
        rp2350.Registers[32] = 0x20000000;     // pc

        rp2350.Step();

        Assert.Equal((uint)0x20000002, rp2350.Registers[32]); // pc
        Assert.Equal((uint)0x20001000, rp2350.Registers[2]);  // sp

        // All other registers unchanged
        Assert.Equal("0x08001234", rp2350.Registers[01].ToHex());
        Assert.Equal("0x11111111", rp2350.Registers[08].ToHex());
        Assert.Equal("0x22222222", rp2350.Registers[09].ToHex());
        Assert.Equal("0x33333333", rp2350.Registers[18].ToHex());
        Assert.Equal("0x44444444", rp2350.Registers[19].ToHex());
        Assert.Equal("0x55555555", rp2350.Registers[20].ToHex());
        Assert.Equal("0x66666666", rp2350.Registers[21].ToHex());
        Assert.Equal("0x77777777", rp2350.Registers[22].ToHex());
        Assert.Equal("0x88888888", rp2350.Registers[23].ToHex());
        Assert.Equal("0x99999999", rp2350.Registers[24].ToHex());
        Assert.Equal("0xAAAAAAAA", rp2350.Registers[25].ToHex());
        Assert.Equal("0xBBBBBBBB", rp2350.Registers[26].ToHex());
        Assert.Equal("0xCCCCCCCC", rp2350.Registers[27].ToHex());
    }

    [Fact]
    public void cm_pop_0111_01()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                   cm.pop {ra, s0, s1, s2}, -32         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11010_0111_01_10));

        rp2350.MemoryWrite(0x20001000 - 0x00, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x04, BitConverter.GetBytes((uint)0x33333333));
        rp2350.MemoryWrite(0x20001000 - 0x08, BitConverter.GetBytes((uint)0x22222222));
        rp2350.MemoryWrite(0x20001000 - 0x0c, BitConverter.GetBytes((uint)0x11111111));
        rp2350.MemoryWrite(0x20001000 - 0x10, BitConverter.GetBytes((uint)0x08001234));
        rp2350.MemoryWrite(0x20001000 - 0x14, BitConverter.GetBytes((uint)0x00000000));

        // Clear all registers
        for (uint i = 0; i < rp2350.Registers.Length; i++)
        {
            rp2350.Registers[i] = 0x0;
        }

        rp2350.Registers[2] = 0x20001000 - 32; // sp
        rp2350.Registers[32] = 0x20000000;     // pc

        rp2350.Step();

        Assert.Equal((uint)0x20000002, rp2350.Registers[32]);      // pc
        Assert.Equal((uint)0x20001000, rp2350.Registers[2]);  // sp

        // All other registers unchanged
        Assert.Equal("0x08001234", rp2350.Registers[1].ToHex());
        Assert.Equal("0x11111111", rp2350.Registers[8].ToHex());
        Assert.Equal("0x22222222", rp2350.Registers[9].ToHex());
        Assert.Equal("0x33333333", rp2350.Registers[18].ToHex());
    }

    [Fact]
    public void cm_popret_0110_00()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                      cm.popret {ra, s0, s1}, -16         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11110_0110_00_10));

        rp2350.MemoryWrite(0x20001000 - 0x00, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x04, BitConverter.GetBytes((uint)0x22222222));
        rp2350.MemoryWrite(0x20001000 - 0x08, BitConverter.GetBytes((uint)0x11111111));
        rp2350.MemoryWrite(0x20001000 - 0x0c, BitConverter.GetBytes((uint)0x08001234));
        rp2350.MemoryWrite(0x20001000 - 0x10, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x14, BitConverter.GetBytes((uint)0x00000000));

        // Clear all registers
        for (uint i = 0; i < rp2350.Registers.Length; i++)
        {
            rp2350.Registers[i] = 0x0;
        }

        rp2350.Registers[2] = 0x20001000 - 16; // sp
        rp2350.Registers[32] = 0x20000000;     // pc

        rp2350.Step();

        Assert.Equal((uint)0x08001234, rp2350.Registers[32]); // pc
        Assert.Equal((uint)0x20001000, rp2350.Registers[2]);  // sp

        Assert.Equal("0x08001234", rp2350.Registers[1].ToHex());
        Assert.Equal("0x11111111", rp2350.Registers[8].ToHex());
        Assert.Equal("0x22222222", rp2350.Registers[9].ToHex());
    }

    [Fact]
    public void cm_popretz_0110_00()
    {
        var rp2350 = RP2350Builder.CreateEmulator();

        //                      cm.popret {ra, s0, s1}, -16         funct3      rlist spimm
        rp2350.MemoryWrite(0x20000000, BitConverter.GetBytes((ushort)0b101_11100_0110_00_10));

        rp2350.MemoryWrite(0x20001000 - 0x00, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x04, BitConverter.GetBytes((uint)0x22222222));
        rp2350.MemoryWrite(0x20001000 - 0x08, BitConverter.GetBytes((uint)0x11111111));
        rp2350.MemoryWrite(0x20001000 - 0x0c, BitConverter.GetBytes((uint)0x08001234));
        rp2350.MemoryWrite(0x20001000 - 0x10, BitConverter.GetBytes((uint)0x00000000));
        rp2350.MemoryWrite(0x20001000 - 0x14, BitConverter.GetBytes((uint)0x00000000));

        // Clear all registers
        for (uint i = 0; i < rp2350.Registers.Length; i++)
        {
            rp2350.Registers[i] = 0x0;
        }

        rp2350.Registers[2] = 0x20001000 - 16; // sp
        rp2350.Registers[32] = 0x20000000;     // pc
        rp2350.Registers[10] = 0xCAFECAFE;     // a0

        rp2350.Step();

        Assert.Equal((uint)0x08001234, rp2350.Registers[32]); // pc
        Assert.Equal((uint)0x20001000, rp2350.Registers[2]);  // sp
        Assert.Equal((uint)0x00000000, rp2350.Registers[10]); // a0

        Assert.Equal("0x08001234", rp2350.Registers[1].ToHex());
        Assert.Equal("0x11111111", rp2350.Registers[8].ToHex());
        Assert.Equal("0x22222222", rp2350.Registers[9].ToHex());
    }
}
