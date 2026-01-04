using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class Resets : PeripheralBase
{
    public Resets(uint baseAddress, string name, ILogger<Resets> logger) : base(baseAddress, name, logger)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        m_Logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());

        /*
            - reset: This register contains a bit for each component that can be reset. 
                     When set to 1, the reset is asserted. If the bit is cleared, the reset is deasserted.

            - wdsel: This register contains a bit for each component that can be reset. 
                     When set to 1, this component will reset if the watchdog fires. If you reset the power-on state 
                     machine, the entire reset controller will reset, which includes every component.

            - reset_done: This register contains a bit for each component that is automatically set when the component is out of reset. 
                          This allows software to wait for this status bit in case the component requires initialisation before use.
         */

        if (offset == 0x8) // RESET_DONE Register
        {
            m_Logger.LogInformation("Read RESET_DONE Register");

            // bits 31-29 are reserved
            return 0x1FFFFFFF;
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        m_Logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
    }
}
