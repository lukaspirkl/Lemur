using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class OtpData : Peripheral32
{
    private readonly ILogger<OtpData> logger;

    public OtpData(ILogger<OtpData> logger)
        : base(0x40130000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {
        // TODO: Read other values from hardware
        if (offset == 0x0)
        {
            return 0xCFAFF9B0;
        }
        else if (offset == 0x4)
        {
            return 0x9F6D2E4B;
        }
        else
        {
            logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());
            return 0;
        }
    }

    protected override void HandleWrite(uint offset, uint data)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), data.ToHex());
    }
}