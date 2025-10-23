namespace Venture;

public static class UInt32Extensions
{
    public static string ToBin(this uint value)
    {
        return $"0b{Convert.ToString(value, 2).PadLeft(32, '0')}";
    }

    public static string ToHex(this uint value)
    {
        return $"0x{Convert.ToString(value, 16).PadLeft(8, '0')}";
    }
}