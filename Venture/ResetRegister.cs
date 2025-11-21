using Venture.Processor;

namespace Venture;

public class ResetRegister : ReadWriteMemoryBase
{
    byte[] reset = [0, 0, 0, 0];
    byte[] wdsel = [0, 0, 0, 0];
    byte[] reset_done = [0xFF, 0xFF, 0xFF, 0xFF];

    public ResetRegister()
        : base(0x40020000, 0x12)
    {
    }

    public override ArraySegment<byte> Read(uint address, int count)
    {
        if (address == 0x40020000 && count == 4)
        {
            return reset;
        }

        if (address == 0x40020004 && count == 4)
        {
            return wdsel;
        }

        if (address == 0x40020008 && count == 4)
        {
            return reset_done;
        }

        throw new NotImplementedException();
    }

    public override void Write(uint address, byte[] data)
    {
        throw new NotImplementedException();
    }
}
