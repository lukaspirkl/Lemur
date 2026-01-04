using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class PPLUSB : PeripheralBase
{
    public PPLUSB(uint baseAddress, string name, ILogger<PPLUSB> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        m_Logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x0) // CS Register
        {
            m_Logger.LogWarning("Reading STATUS register from PPLUSB");
            return 0x80000001;
        }
        else if (offset == 0x8) // FBDIV_INT Register
        {
            m_Logger.LogWarning("Reading FBDIV_INT register from PPLUSB");
            return 0x00000064;
        }
        else if (offset == 0xC) // PRIM Register
        {
            m_Logger.LogWarning("Reading PRIM register from PPLUSB");
            return 0x00055000;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        m_Logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
