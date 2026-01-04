using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class XOSC : PeripheralBase
{
    public XOSC(uint baseAddress, string name, ILogger<XOSC> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        m_Logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x4) // STATUS Register
        {
            m_Logger.LogWarning("Reading STATUS register from XOSC");
            return 0x81001000;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        m_Logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
