using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class OTPData : PeripheralBase
{
    private readonly ILogger<OTPData> logger;

    public OTPData(ILogger<OTPData> logger)
        : base(0x40120000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x15C)
        {
            return 0x00000003;
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