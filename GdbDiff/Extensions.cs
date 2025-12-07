namespace GdbDiff;

// TODO: This should be in some shared library
public static class Extensions
{
    public static string ToBin(this uint value, int padLeft = 32)
    {
        return $"0b{Convert.ToString(value, 2).PadLeft(padLeft, '0')}";
    }

    public static string ToHex(this uint value, int padLeft = 8, bool prefix = true)
    {
        return $"{(prefix ? "0x" : "")}{Convert.ToString(value, 16).PadLeft(padLeft, '0').ToUpper()}";
    }

    public static string ToHex(this byte[] data)
    {
        var s = string.Join("", data.Select(x => Convert.ToString(x, 16).PadLeft(2, '0')).Reverse()).ToUpper();
        return $"0x{s}";
    }

    public static uint ExtractBits(this uint instruction, int startBit, int length)
    {
        // 1. Calculate the mask: (1 << length) - 1
        //    Example (length = 3): (1 << 3) - 1 = 8 - 1 = 7 (Binary 00...0111)
        uint mask = (1U << length) - 1;

        // 2. Right-shift to move the desired bits to the least significant position.
        //    Example (startBit = 12): Moves bits 14-12 to positions 2-0.
        uint shifted = instruction >> startBit;

        // 3. Bitwise AND with the mask to clear any higher bits that were shifted in.
        //    This isolates the required value.
        uint extractedValue = shifted & mask;

        return extractedValue;
    }

    /// <summary>
    /// Sign-extends an integer value from a specific bit width.
    /// Assumes the value is in the lower bits and the sign bit
    /// is at (bitWidth - 1).
    /// </summary>
    /// <param name="value">The integer to sign-extend.</param>
    /// <param name="bitWidth">The original number of bits (e.g., 12 for your offset).</param>
    /// <returns>The 32-bit sign-extended integer.</returns>
    public static int SignExtend(this int value, int bitWidth)
    {
        // 32 is the total bits in an 'int'
        int shiftAmount = 32 - bitWidth;

        // Shift the value left to align its sign bit with the 'int' sign bit,
        // then arithmetic shift right to perform the sign extension.
        return (value << shiftAmount) >> shiftAmount;
    }
}