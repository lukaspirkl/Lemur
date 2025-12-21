using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class PadsQSPI : PeripheralBase
{
    private readonly ILogger<PadsQSPI> logger;

    public PadsQSPI(ILogger<PadsQSPI> logger)
        : base(0x40040000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x8)
        {
            return 0x00000056;
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
