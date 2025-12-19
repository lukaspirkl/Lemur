using Microsoft.Extensions.Logging;

namespace Venture.Processor;

public interface IRegisters
{
    uint this[uint index] { get; set; }
    uint Length { get; }
}


public class Registers : IRegisters
{
    private uint[] data = new uint[32];
    private readonly ILogger<Registers> logger;

    public uint Length => 32;

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
