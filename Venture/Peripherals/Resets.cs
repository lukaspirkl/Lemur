namespace Venture.Peripherals;

public class Resets : Peripheral32
{
    public Resets() : base(0x40020000)
    {
    }

    protected override uint HandleRead(uint offset)
    {
        if (offset == 0x8) // RESET_DONE Register
        {
            Console.WriteLine("Read RESET_DONE Register");
            // This register contains a bit for each component that is automatically set when the component is out of
            // reset.This allows software to wait for this status bit in case the component requires initialisation before use.
            return 0xFFFFFFFF;
        }

        Console.WriteLine($"WARNING: Reading from {offset.ToHex()} - RESET");
        return 0;
        //throw new NotImplementedException();
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        Console.WriteLine($"WARNING: Writing to {offset.ToHex()} data {value.ToHex()} - RESET");
        //throw new NotImplementedException();
    }
}