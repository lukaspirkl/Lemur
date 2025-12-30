namespace Venture.Peripherals;

public class Clocks : PeripheralBase
{
    private const uint CLK_REF_CTRL = 0x30;
    private const uint CLK_REF_DIV = 0x34;
    private const uint CLK_REF_SELECTED = 0x38;

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



    private const uint CLK_SYS_CTRL = 0x3c;
    private const uint CLK_SYS_DIV = 0x40;
    private const uint CLK_SYS_SELECTED = 0x44;

    enum SysSource
    {
        CLK_REF = 0x0,
        CLKSRC_CLK_SYS_AUX = 0x1,
    }

    private SysSource sysSource = SysSource.CLK_REF;



    public Clocks() : base(0x40010000)
    {
        AddRegister(CLK_REF_CTRL)
            .Field(5, 2, () => refAuxSource, v => refAuxSource = v)
            .Field(0, 2, () => refSource, v => refSource = v);

        AddRegister(CLK_REF_DIV, 0x00010000);

        AddRegister(CLK_REF_SELECTED)
            .OnRead(() => 1u << (int)refSource);

        AddRegister(CLK_SYS_CTRL)
            .Field(0, 1, () => sysSource, v => sysSource = v);

        AddRegister(CLK_SYS_DIV, 0x00010000);

        AddRegister(CLK_SYS_SELECTED)
            .OnRead(() => 1u << (int)sysSource);

        // CLK_PERI_DIV Register
        AddRegister(0x4c, 0x00010000);

        // CLK_HSTX_DIV Register
        AddRegister(0x58, 0x00010000);

        // CLK_USB_DIV Register
        AddRegister(0x64, 0x00010000);

        // CLK_ADC_DIV Register
        AddRegister(0x70, 0x00010000);
    }
}
