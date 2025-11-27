namespace Venture.Processor;

public class Registers
{
    private uint[] data = new uint[32];

    public uint this[uint index]
    {
        get 
        {
            if (index == 0)
            {
                return 0;
            }

            return data[index]; 
        }
        set
        {
            if (index == 0)
            {
                return;
            }
                
            //Console.WriteLine($"x{index} <- {value.ToHex()}");

            data[index] = value;
        }
    }
}
