using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class Powman : Peripheral32
{
    private readonly ILogger<Powman> logger;

    public Powman(ILogger<Powman> logger)
        : base(0x40100000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x02c) // CHIP_RESET Register
        {
            // 27 - HAD_HZD_SYS_RESET_REQ: Last reset was a system reset from the hazard debugger.
            //      I need to set this for GdbDiff as it is reseting the real hardware before run.
            return 1 << 27;
        }
        else if (offset == 0x038) // STATE Register
        {
            // Can also be 0x00000000
            return 0x00000100;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
