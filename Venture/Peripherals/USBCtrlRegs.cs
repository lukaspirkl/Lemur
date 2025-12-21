using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class USBCtrlRegs : PeripheralBase
{
    private readonly ILogger<USBCtrlRegs> logger;

    public USBCtrlRegs(ILogger<USBCtrlRegs> logger) 
        : base(0x50110000)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {
        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x4C) // SIE_CTRL Register
        {
            logger.LogWarning("Reading SIE_CTRL register from USBCTRL_REGS");
            return 0x00048000;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
