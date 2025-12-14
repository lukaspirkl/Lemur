using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class Resets : Peripheral32
{
    private readonly ILogger<Resets> logger;

    public Resets(ILogger<Resets> logger) 
        : base(0x40020000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x8) // RESET_DONE Register
        {
            logger.LogInformation("Read RESET_DONE Register");
            // This register contains a bit for each component that is automatically set when the component is out of
            // reset.This allows software to wait for this status bit in case the component requires initialisation before use.
            return 0x1FFFFFFF;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
