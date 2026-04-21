using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals;

public class BootRAM : BytePeripheralBase
{
    private byte[] m_Ram = new byte[1024]; // 1kB

    public BootRAM(uint baseAddress, string name, ILogger<BootRAM> logger) : base(baseAddress, name, logger)
    {
    }


    protected override byte HandleRead(uint offset)
    {
        if (offset >= 0x80c && offset <= 0x828)
        {
            return 1; // These are bootlocks and I should return something other than zero to signal that the lock was successfully taken
        }
        else if (offset >= m_Ram.Length)
        {
            // TODO: There is also BOOTRAM_BASE register on 0x400e0800
            m_Logger.LogWarning("Reading from unhandled offset {offset} of BootRAM", offset.ToHex());
            return 0;
        }
        else
        {
            return m_Ram[offset];
        }
    }

    protected override void HandleWrite(uint offset, byte data)
    {
        if (offset >= m_Ram.Length)
        {
            // TODO: There is also BOOTRAM_BASE register on 0x400e0800
            m_Logger.LogWarning("Writing to unhandled offset {offset} of BootRAM (data: {data})", offset.ToHex(), ((uint)data).ToHex(2));
        }
        else
        {
            m_Ram[offset] = data;
        }
    }
}
