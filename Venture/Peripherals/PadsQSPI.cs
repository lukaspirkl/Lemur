using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class PadsQSPI : PeripheralBase
{
    public PadsQSPI(uint baseAddress, string name, ILogger<PadsQSPI> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        m_Logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

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
        m_Logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), data.ToHex());
    }
}
