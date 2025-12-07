namespace Tests.Debug;


public class GdbConnectionHandlerTests
{
    [Fact]
    public async Task QueryTheReasonTheTargetHalted()
    {
        await using var fixture = new GdbConnectionFixture();
        var result = await fixture.SendPacketAsync("?", TestContext.Current.CancellationToken);
        Assert.Equal("S05", result);
    }

    [Fact]
    public async Task QueryFeatures()
    {
        await using var fixture = new GdbConnectionFixture();
        var result = await fixture.SendPacketAsync("qSupported", TestContext.Current.CancellationToken);
        Assert.Equal("PacketSize=400;vContSupported+;multiprocess-", result);
    }

    [Fact]
    public async Task MustReplyEmpty()
    {
        await using var fixture = new GdbConnectionFixture();
        var result = await fixture.SendPacketAsync("vMustReplyEmpty", TestContext.Current.CancellationToken);
        Assert.Equal("", result);
    }

    [Fact]
    public async Task ActionsSupportedByvCont()
    {
        await using var fixture = new GdbConnectionFixture();
        var result = await fixture.SendPacketAsync("vCont?", TestContext.Current.CancellationToken);
        Assert.Equal("vCont;c;C;s;S", result);
    }

    [Fact]
    public async Task ReadGeneralRegisters()
    {
        await using var fixture = new GdbConnectionFixture();

        fixture.Emulator.Registers[0] = 1;
        fixture.Emulator.Registers[1] = 2;
        fixture.Emulator.Registers[2] = 3;
        fixture.Emulator.Registers[3] = 4;

        var result = await fixture.SendPacketAsync("g", TestContext.Current.CancellationToken);
        Assert.Equal("01000000020000000300000004000000", result);
    }

    [Fact]
    public async Task WriteGeneralRegisters()
    {
        await using var fixture = new GdbConnectionFixture();

        var result = await fixture.SendPacketAsync("G01000000020000000300000004000000", TestContext.Current.CancellationToken);
        Assert.Equal("OK", result);

        Assert.Equal((uint)1, fixture.Emulator.Registers[0]);
        Assert.Equal((uint)2, fixture.Emulator.Registers[1]);
        Assert.Equal((uint)3, fixture.Emulator.Registers[2]);
        Assert.Equal((uint)4, fixture.Emulator.Registers[3]);
    }

    [Fact]
    public async Task ReadMemory()
    {
        await using var fixture = new GdbConnectionFixture();

        fixture.Emulator.Memory[0] = 1;
        fixture.Emulator.Memory[1] = 2;

        var result = await fixture.SendPacketAsync("m0,2", TestContext.Current.CancellationToken);
        Assert.Equal("0102", result);
    }

    [Fact]
    public async Task WriteMemory()
    {
        await using var fixture = new GdbConnectionFixture();

        var result = await fixture.SendPacketAsync("M0,2:0102", TestContext.Current.CancellationToken);
        Assert.Equal("OK", result);

        Assert.Equal((uint)1, fixture.Emulator.Memory[0]);
        Assert.Equal((uint)2, fixture.Emulator.Memory[1]);
    }
}

