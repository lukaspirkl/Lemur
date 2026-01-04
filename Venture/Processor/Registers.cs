namespace Venture.Processor;

public interface IRegisters
{
    uint this[uint index] { get; set; }
    uint Length { get; }
}


public class Registers : IRegisters
{
    private uint[] m_Data = new uint[32];
    private readonly IEmuLogger<Registers> m_Logger;

    public uint Length => 32;

    public Registers(IEmuLogger<Registers> logger)
    {
        this.m_Logger = logger;
    }

    public uint this[uint index]
    {
        get
        {
            if (index == 0)
            {
                return 0;
            }

            return m_Data[index]; 
        }
        set
        {
            if (index == 0)
            {
                return;
            }

            m_Logger.LogRegisterSet(index, value);

            m_Data[index] = value;
        }
    }

}
