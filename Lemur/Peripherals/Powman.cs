using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Lemur.Peripherals;

public class Powman : PeripheralBase
{
    private readonly Dictionary<uint, uint> m_Registers = new Dictionary<uint, uint>();

    public Powman(uint baseAddress, string name, ILogger<Powman> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        m_Logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        if (offset == 0x02c) // CHIP_RESET Register
        {
            // 28 -  HAD_WATCHDOG_RESET_PSM: Last reset was a watchdog timeout which was configured to reset the power-on state machine

            // 27 - HAD_HZD_SYS_RESET_REQ: Last reset was a system reset from the hazard debugger
            //      I need to set this for GdbDiff as it is reseting the real hardware before run.
            return 1 << 27;
        }
        else if (offset == 0x038) // STATE Register
        {
            // Can also be 0x00000000
            return 0x00000100;
        }
        else
        {
            return m_Registers.GetValueOrDefault(offset, 0u);
        }
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        m_Logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());

        m_Registers[offset] = value;
    }
}
