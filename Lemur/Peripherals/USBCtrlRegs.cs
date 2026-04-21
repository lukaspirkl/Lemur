using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals;

public class USBCtrlRegs : PeripheralBase
{
    public USBCtrlRegs(uint baseAddress, string name, ILogger<USBCtrlRegs> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        m_Logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x4C) // SIE_CTRL Register
        {
            m_Logger.LogWarning("Reading SIE_CTRL register from USBCTRL_REGS");
            return 0x00048000;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        m_Logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
