using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class PPLSYS : PeripheralBase
{
    public PPLSYS(uint baseAddress, string name, ILogger<PPLSYS> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x0) // CS Register
        {
            logger.LogWarning("Reading STATUS register from PPLSYS");
            return 0x80000001;
        }
        else if (offset == 0x8) // FBDIV_INT Register
        {
            logger.LogWarning("Reading FBDIV_INT register from PPLSYS");
            return 0x0000007D;
        }
        else if (offset == 0xC) // PRIM Register
        {
            logger.LogWarning("Reading PRIM register from PPLSYS");
            return 0x00052000;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
