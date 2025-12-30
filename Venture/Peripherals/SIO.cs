using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class SIO : PeripheralBase
{
    public SIO(uint baseAddress, string name, ILogger<SIO> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {   
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x000) // CPUID Register
        {
            return 0;
        }
        else if (offset == 0x004) // GPIO_IN Register
        {
            return 0x02000000;
        }
        else if (offset == 0x008) // GPIO_HI_IN Register
        {
            return 0xC8000000;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
