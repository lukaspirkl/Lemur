using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class Clocks : PeripheralBase
{
    enum RefSource
    {
        ROSC_CLKSRC_PH = 0x0,
        CLKSRC_CLK_REF_AUX = 0x1,
        XOSC_CLKSRC = 0x2,
        LPOSC_CLKSRC = 0x3
    }

    private RefSource refSource = RefSource.ROSC_CLKSRC_PH;

    enum RefAuxSource
    {
        CLKSRC_PLL_USB = 0x0,
        CLKSRC_GPIN0 = 0x1,
        CLKSRC_GPIN1 = 0x2,
        CLKSRC_PLL_USB_PRIMARY_REF_OPCG = 0x3
    }

    private RefAuxSource refAuxSource = RefAuxSource.CLKSRC_PLL_USB;


    enum SysSource
    {
        CLK_REF = 0x0,
        CLKSRC_CLK_SYS_AUX = 0x1,
    }

    private SysSource sysSource = SysSource.CLK_REF;


    public Clocks(uint baseAddress, string name, ILogger<Clocks> logger) : base(baseAddress, name, logger)
    {
        AddRegister(0x30, "CLK_REF_CTRL")
            .Field(5, 2, () => refAuxSource, v => refAuxSource = v)
            .Field(0, 2, () => refSource, v => refSource = v);

        AddRegister(0x34, "CLK_REF_DIV", 0x00010000);

        AddRegister(0x38, "CLK_REF_SELECTED")
            .OnRead(() => 1u << (int)refSource);

        AddRegister(0x3c, "CLK_SYS_CTRL")
            .Field(0, 1, () => sysSource, v => sysSource = v);

        AddRegister(0x40, "CLK_SYS_DIV", 0x00010000);

        AddRegister(0x44, "CLK_SYS_SELECTED")
            .OnRead(() => 1u << (int)sysSource);

        AddRegister(0x4c, "CLK_PERI_DIV", 0x00010000);

        AddRegister(0x58, "CLK_HSTX_DIV", 0x00010000);

        AddRegister(0x64, "CLK_USB_DIV", 0x00010000);

        AddRegister(0x70, "CLK_ADC_DIV", 0x00010000);
    }
}
