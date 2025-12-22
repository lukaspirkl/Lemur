using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class Clocks : PeripheralBase
{
    private readonly ILogger<Clocks> logger;



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




    private const uint CLK_SYS_CTRL = 0x3c;
    private const uint CLK_SYS_DIV = 0x40;
    private const uint CLK_SYS_SELECTED = 0x44;

    enum SysSource
    {
        CLK_REF = 0x0,
        CLKSRC_CLK_SYS_AUX = 0x1,
    }

    private SysSource sysSource = SysSource.CLK_REF;



    public Clocks(ILogger<Clocks> logger) 
        : base(0x40010000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == CLK_REF_DIV)
        {
            logger.LogWarning("Reading CLK_REF_DIV register from Clocks");
            return 0x00010000;
        }
        else if (offset == CLK_REF_SELECTED)
        {
            logger.LogWarning("Reading CLK_REF_SELECTED register from Clocks");
            return (uint)(1 << (int)refSource);
        }

        else if (offset == CLK_SYS_DIV)
        {
            logger.LogWarning("Reading  CLK_SYS_DIV register from Clocks");
            return 0x00010000;
        }
        else if (offset == CLK_SYS_SELECTED)
        {
            logger.LogWarning("Reading CLK_SYS_SELECTED register from Clocks");
            return (uint)(1 << (int)sysSource);
        }
        
        else if (offset == 0x4c) // CLK_PERI_DIV Register
        {
            logger.LogWarning("Reading CLK_PERI_DIV register from Clocks");
            return 0x00010000;
        }
        else if (offset == 0x58) // CLK_HSTX_DIV Register
        {
            logger.LogWarning("Reading CLK_HSTX_DIV register from Clocks");
            return 0x00010000;
        }
        else if (offset == 0x64) // CLK_USB_DIV Register
        {
            logger.LogWarning("Reading CLK_USB_DIV register from Clocks");
            return 0x00010000;
        }
        else if (offset == 0x70) // CLK_ADC_DIV Register
        {
            logger.LogWarning("Reading CLK_ADC_DIV register from Clocks");
            return 0x00010000;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());

        if (offset == CLK_REF_CTRL)
        {
            refSource = (RefSource)value.ExtractBits(0, 2);
        }

        else if (offset == CLK_SYS_CTRL)
        {
            sysSource = (SysSource)value.ExtractBits(0, 1);
        }
    }
}
