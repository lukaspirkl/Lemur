namespace Venture.Peripherals;

public class SIO : Peripheral32
{
    public SIO() : base(0xd0000000)
    {
    }

    protected override uint HandleRead(uint offset)
    {   
        if (offset == 0x000) // CPUID Register
        {
            Console.WriteLine("Read CPUID Register");
            return 0;
        }

        Console.WriteLine($"WARNING: Reading from {offset.ToHex()} - SIO");
        return 0;
        //throw new NotImplementedException();
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        Console.WriteLine($"WARNING: Writing to {offset.ToHex()} data {value.ToHex()} - SIO");
        //throw new NotImplementedException();
    }
}
