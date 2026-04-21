using System;
using System.Linq;

namespace Lemur;

public static class Extensions
{
    public static string ToBin(this uint value, int padLeft = 32)
    {
        return $"0b{Convert.ToString(value, 2).PadLeft(padLeft, '0')}";
    }

    public static string ToHex(this uint value, int padLeft = 8, bool prefix = true)
    {
        // Calculate the number of hex digits required.
        // value == 0 -> 1 digit ('0')
        // otherwise -> floor(log2(value) / 4) + 1
        int hexLength = value == 0 ? 1 : (System.Numerics.BitOperations.Log2(value) >> 2) + 1;

        int totalLength = (prefix ? 2 : 0) + (padLeft > hexLength ? padLeft : hexLength);

        return string.Create(totalLength, (value, prefix, padLeft, hexLength), (span, state) =>
        {
            var (v, hasPrefix, minWidth, digits) = state;
            int pos = 0;

            if (hasPrefix)
            {
                span[0] = '0';
                span[1] = 'x';
                pos = 2;
            }

            int padding = minWidth - digits;
            if (padding > 0)
            {
                span.Slice(pos, padding).Fill('0');
                pos += padding;
            }

            v.TryFormat(span.Slice(pos), out _, "X");
        });
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