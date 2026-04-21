using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals;

public class OTPData : PeripheralBase
{
    public OTPData(uint baseAddress, string name, ILogger<OTPData> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        m_Logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

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
            return 0;
        }
    }

    protected override void HandleWrite(uint offset, uint data)
    {
        m_Logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), data.ToHex());
    }
}