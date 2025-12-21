using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class Clocks : PeripheralBase
{
    private readonly ILogger<Clocks> logger;

    public Clocks(ILogger<Clocks> logger) 
        : base(0x40010000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x34) // CLK_REF_DIV Register
        {
            logger.LogWarning("Reading CLK_REF_DIV register from Clocks");
            return 0x00010000;
        }
        else if (offset == 0x38) // CLK_REF_SELECTED Register
        {
            logger.LogWarning("Reading CLK_REF_SELECTED register from Clocks");
            return 0x00000004;
        }
        else if (offset == 0x40) //  CLK_SYS_DIV Register
        {
            logger.LogWarning("Reading  CLK_SYS_DIV register from Clocks");
            return 0x00010000;
        }
        else if (offset == 0x44) // CLK_SYS_SELECTED Register
        {
            logger.LogWarning("Reading CLK_SYS_SELECTED register from Clocks");
            return 0x00000002;
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
    }
}
