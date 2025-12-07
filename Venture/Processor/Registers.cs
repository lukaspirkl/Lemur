namespace Venture.Processor;

public interface IIndexable<T>
{
    T this[uint index] { get; set; }
    int Length { get; }
}


public class Registers : IIndexable<uint>
{
    private uint[] data = new uint[32];
    public int Length => 32;

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

            ;
            Console.WriteLine($"x{index}{(index.ToString().Length == 1 ? " " : "")} {value.ToHex()}");

            data[index] = value;
        }
    }

}
