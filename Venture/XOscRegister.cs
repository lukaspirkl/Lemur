
namespace Venture;

public class XOscRegister : Peripheral
{
    public XOscRegister()
        : base(0x40048000)
    {
        Status = (uint)1 << 31  // Stable
               | (uint)1 << 12  // Enabled
               | 0x3;           // Freqency range - 40-100MHz

        Dormant = 0x77616b65; // Wake
    }

    public override void Write(uint address, byte[] data)
    {
        base.Write(address, data);
    }

    public override ArraySegment<byte> Read(uint address, int count)
    {
        return base.Read(address, count);
    }

    /// <summary>
    /// Crystal Oscillator Control
    /// </summary>
    public uint Ctrl
    {
        get => ReadWord(StartAddress + 0x00);
        set => WriteWord(StartAddress + 0x00, value);
    }

    /// <summary>
    /// Crystal Oscillator Status
    /// </summary>
    public uint Status
    {
        get => ReadWord(StartAddress + 0x04);
        set => WriteWord(StartAddress + 0x04, value);
    }

    /// <summary>
    /// Crystal Oscillator pause control
    /// </summary>
    public uint Dormant
    {
        get => ReadWord(StartAddress + 0x08);
        set => WriteWord(StartAddress + 0x08, value);
    }

    /// <summary>
    /// Controls the startup delay
    /// </summary>
    public uint Startup
    {
        get => ReadWord(StartAddress + 0x0c);
        set => WriteWord(StartAddress + 0x0c, value);
    }

    /// <summary>
    /// A down counter running at the XOSC frequency which counts to zero and stops
    /// </summary>
    public uint Count
    {
        get => ReadWord(StartAddress + 0x10);
        set => WriteWord(StartAddress + 0x10, value);
    }
}
