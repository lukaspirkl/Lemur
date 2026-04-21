using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals;

public class Clocks : PeripheralBase
{
    enum RefSource
    {
        ROSC_CLKSRC_PH = 0x0,
        CLKSRC_CLK_REF_AUX = 0x1,
        XOSC_CLKSRC = 0x2,
        LPOSC_CLKSRC = 0x3
    }

    private RefSource m_RefSource = RefSource.ROSC_CLKSRC_PH;

    enum RefAuxSource
    {
        CLKSRC_PLL_USB = 0x0,
        CLKSRC_GPIN0 = 0x1,
        CLKSRC_GPIN1 = 0x2,
        CLKSRC_PLL_USB_PRIMARY_REF_OPCG = 0x3
    }

    private RefAuxSource m_RefAuxSource = RefAuxSource.CLKSRC_PLL_USB;


    enum SysSource
    {
        CLK_REF = 0x0,
        CLKSRC_CLK_SYS_AUX = 0x1,
    }

    private SysSource m_SysSource = SysSource.CLK_REF;


    public Clocks(uint baseAddress, string name, ILogger<Clocks> logger) : base(baseAddress, name, logger)
    {
        AddRegister(0x30, "CLK_REF_CTRL")
            .Field(5, 2, () => m_RefAuxSource, v => m_RefAuxSource = v)
            .Field(0, 2, () => m_RefSource, v => m_RefSource = v);

        AddRegister(0x34, "CLK_REF_DIV", 0x00010000);

        AddRegister(0x38, "CLK_REF_SELECTED")
            .OnRead(() => 1u << (int)m_RefSource);

        AddRegister(0x3c, "CLK_SYS_CTRL")
            .Field(0, 1, () => m_SysSource, v => m_SysSource = v);

        AddRegister(0x40, "CLK_SYS_DIV", 0x00010000);

        AddRegister(0x44, "CLK_SYS_SELECTED")
            .OnRead(() => 1u << (int)m_SysSource);

        AddRegister(0x4c, "CLK_PERI_DIV", 0x00010000);

        AddRegister(0x58, "CLK_HSTX_DIV", 0x00010000);

        AddRegister(0x64, "CLK_USB_DIV", 0x00010000);

        AddRegister(0x70, "CLK_ADC_DIV", 0x00010000);
    }
}
