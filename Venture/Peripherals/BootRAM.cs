using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class BootRAM : PeripheralBase
{
    private readonly ILogger<BootRAM> logger;
    private byte[] ram = new byte[1024];

    public BootRAM(ILogger<BootRAM> logger)
        : base(0x400e0000)
    {
        this.logger = logger;
    }


    // TODO: There is also BOOTRAM_BASE register on 0x400e0800

    protected override byte[] HandleRead(uint offset, int count)
    {
        if (offset + count <= ram.Length)
        {
            return ram.Skip((int)offset).Take(count).ToArray();
        }

        logger.LogWarning("Reading from unhandled offset {offset} (count:{count})", offset.ToHex(), count);
        return new byte[count];
    }

    protected override void HandleWrite(uint offset, byte[] data)
    {
        if (offset + data.Length <= ram.Length)
        {
            data.CopyTo(ram, offset);
        }

        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), data.ToHex());
    }
}
