using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class XOSC : PeripheralBase
{
    private readonly ILogger<XOSC> logger;

    public XOSC(ILogger<XOSC> logger) 
        : base(0x40048000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x4) // STATUS Register
        {
            logger.LogWarning("Reading STATUS register from XOSC");
            return 0x81001000;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
