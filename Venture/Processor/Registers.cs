using Microsoft.Extensions.Logging;

namespace Venture.Processor;

public interface IIndexable<T>
{
    T this[uint index] { get; set; }
    int Length { get; }
}


public class Registers : IIndexable<uint>
{
    private uint[] data = new uint[32];
    private readonly ILogger<Registers> logger;

    public int Length => 32;

    

    public Registers(ILogger<Registers> logger)
    {
        this.logger = logger;
    }

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

            logger.LogDebug("x{index} {value}", index, value.ToHex());

            data[index] = value;
        }
    }

}
