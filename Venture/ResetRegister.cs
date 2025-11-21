namespace Venture;

public class ResetRegister : Peripheral
{
    public ResetRegister()
        : base(0x40020000)
    {
        ResetDone = 0xFFFF_FFFF;
    }

    public uint Reset
    {
        get => ReadWord(0x40020000);
        set => WriteWord(0x40020000, value);
    }

    public uint WDSel
    {
        get => ReadWord(0x40020004);
        set => WriteWord(0x40020004, value);
    }

    public uint ResetDone
    {
        get => ReadWord(0x40020008);
        set => WriteWord(0x40020008, value);
    }
}
