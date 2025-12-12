using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class SIO : Peripheral32
{
    private readonly ILogger<SIO> logger;

    public SIO(ILogger<SIO> logger) 
        : base(0xd0000000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {   
        if (offset == 0x000) // CPUID Register
        {
            logger.LogInformation("Read CPUID Register");
            return 0;
        }

        // 0xD0000008 has value 0xC8000000

        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());
        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
