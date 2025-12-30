using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class XIPQMI : PeripheralBase
{
    public XIPQMI(uint baseAddress, string name, ILogger<XIPQMI> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x0)
        {
            // Reading 0x400D0000 value should be 0x03010801
            // Reading 0x400D0000 value should be 0x03010805
            return 0x03010801;
        }
        else
        {
            return 0;
        }
    }

    protected override void HandleWrite(uint offset, uint data)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), data.ToHex());
    }
}
