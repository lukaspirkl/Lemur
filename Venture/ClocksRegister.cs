namespace Venture;

public class ClocksRegister : Peripheral
{
    public ClocksRegister()
        : base(0x40010000)
    {
    }

    public override void Write(uint address, byte[] data)
    {
        base.Write(address, data);
    }

    public override ArraySegment<byte> Read(uint address, int count)
    {
        return base.Read(address, count);
    }
}
