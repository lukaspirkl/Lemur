using Venture;

namespace Tests.Peripherals;

public class ClocksTests
{
    private const uint CLOCKS_BASE = 0x40010000;
    private const uint CLK_REF_CTRL = CLOCKS_BASE + 0x30;
    private const uint CLK_REF_DIV = CLOCKS_BASE + 0x34;
    private const uint CLK_REF_SELECTED = CLOCKS_BASE + 0x38;

    private const uint CLK_REF_CTRL_SRC_ROSC_CLKSRC_PH = 0x0;
    private const uint CLK_REF_CTRL_SRC_CLKSRC_CLK_REF_AUX = 0x1;
    private const uint CLK_REF_CTRL_SRC_XOSC_CLKSRC = 0x2;
    private const uint CLK_REF_CTRL_SRC_LPOSC_CLKSRC = 0x3;

    private const uint CLK_REF_CTRL_AUXSRC_CLKSRC_PLL_USB = 0x0;
    private const uint CLK_REF_CTRL_AUXSRC_CLKSRC_GPIN0 = 0x1;
    private const uint CLK_REF_CTRL_AUXSRC_CLKSRC_GPIN1 = 0x2;
    private const uint CLK_REF_CTRL_AUXSRC_CLKSRC_PLL_USB_PRIMARY_REF_OPCG = 0x3;

    [Fact]
    public async Task CLK_REF_CTRL_SRC()
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite32(CLK_REF_CTRL, CLK_REF_CTRL_SRC_ROSC_CLKSRC_PH);
        await Task.Delay(1, TestContext.Current.CancellationToken);

        Assert.Equal("0x00000000", sut.MemoryRead32(CLK_REF_CTRL).ToHex());
        Assert.Equal("0x00010000", sut.MemoryRead32(CLK_REF_DIV).ToHex());
        Assert.Equal("0x00000001", sut.MemoryRead32(CLK_REF_SELECTED).ToHex());

        sut.MemoryWrite32(CLK_REF_CTRL, CLK_REF_CTRL_SRC_CLKSRC_CLK_REF_AUX | CLK_REF_CTRL_AUXSRC_CLKSRC_GPIN0 << 5);
        await Task.Delay(1, TestContext.Current.CancellationToken);

        Assert.Equal("0x00000021", sut.MemoryRead32(CLK_REF_CTRL).ToHex());
        Assert.Equal("0x00010000", sut.MemoryRead32(CLK_REF_DIV).ToHex());
        Assert.Equal("0x00000002", sut.MemoryRead32(CLK_REF_SELECTED).ToHex());

        sut.MemoryWrite32(CLK_REF_CTRL, CLK_REF_CTRL_SRC_XOSC_CLKSRC);
        await Task.Delay(1, TestContext.Current.CancellationToken);

        Assert.Equal("0x00000002", sut.MemoryRead32(CLK_REF_CTRL).ToHex());
        Assert.Equal("0x00010000", sut.MemoryRead32(CLK_REF_DIV).ToHex());
        Assert.Equal("0x00000004", sut.MemoryRead32(CLK_REF_SELECTED).ToHex());

        sut.MemoryWrite32(CLK_REF_CTRL, CLK_REF_CTRL_SRC_LPOSC_CLKSRC);
        await Task.Delay(1, TestContext.Current.CancellationToken);

        Assert.Equal("0x00000003", sut.MemoryRead32(CLK_REF_CTRL).ToHex());
        Assert.Equal("0x00010000", sut.MemoryRead32(CLK_REF_DIV).ToHex());
        Assert.Equal("0x00000008", sut.MemoryRead32(CLK_REF_SELECTED).ToHex());
    }

    private const uint CLK_SYS_CTRL = CLOCKS_BASE + 0x3c;
    private const uint CLK_SYS_DIV = CLOCKS_BASE + 0x40;
    private const uint CLK_SYS_SELECTED = CLOCKS_BASE + 0x44;

    private const uint CLK_SYS_CTRL_SRC_CLK_REF = 0x0;
    private const uint CLK_SYS_CTRL_SRC_CLKSRC_CLK_SYS_AUX = 0x1;

    [Fact]
    public async Task CLK_SYS_CTRL_SRC()
    {
        using var sut = RP2350Builder.Create();

        sut.MemoryWrite32(CLK_SYS_CTRL, CLK_SYS_CTRL_SRC_CLK_REF);
        await Task.Delay(1, TestContext.Current.CancellationToken);

        Assert.Equal("0x00000000", sut.MemoryRead32(CLK_SYS_CTRL).ToHex());
        Assert.Equal("0x00010000", sut.MemoryRead32(CLK_SYS_DIV).ToHex());
        Assert.Equal("0x00000001", sut.MemoryRead32(CLK_SYS_SELECTED).ToHex());

        sut.MemoryWrite32(CLK_SYS_CTRL, CLK_SYS_CTRL_SRC_CLKSRC_CLK_SYS_AUX);
        await Task.Delay(1, TestContext.Current.CancellationToken);

        Assert.Equal("0x00000001", sut.MemoryRead32(CLK_SYS_CTRL).ToHex());
        Assert.Equal("0x00010000", sut.MemoryRead32(CLK_SYS_DIV).ToHex());
        Assert.Equal("0x00000002", sut.MemoryRead32(CLK_SYS_SELECTED).ToHex());
    }
}
