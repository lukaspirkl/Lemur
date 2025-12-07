namespace Venture.Peripherals;

public class Powman : Peripheral32
{
    public Powman() : base(0x40100000)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        if (offset == 0x02c) // CHIP_RESET Register
        {
            // 27 - HAD_HZD_SYS_RESET_REQ: Last reset was a system reset from the hazard debugger.
            //      I need to set this for GdbDiff as it is reseting the real hardware before run.
            return 1 << 27; 
        }

        Console.WriteLine($"WARNING: Reading from {offset.ToHex()} - POWMAN");
        return 0;
        //throw new NotImplementedException();
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        Console.WriteLine($"WARNING: Writing to {offset.ToHex()} data {value.ToHex()} - POWMAN");
        //throw new NotImplementedException();
    }
}
