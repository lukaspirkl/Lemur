using System;

namespace VentureUI.Terminal;

[Flags]
public enum TextAttributes : byte
{
    None      = 0,
    Bold      = 1 << 0,
    Underline = 1 << 1,
    Blink     = 1 << 2,
    Inverse   = 1 << 3,
}

public enum AnsiColor : byte
{
    Default = 0,
    Black, Red, Green, Yellow, Blue, Magenta, Cyan, White,
    BrightBlack, BrightRed, BrightGreen, BrightYellow, BrightBlue, BrightMagenta, BrightCyan, BrightWhite,
}

public record struct TerminalCell(char Char, AnsiColor Foreground, AnsiColor Background, TextAttributes Attributes)
{
    public static readonly TerminalCell Empty = new(' ', AnsiColor.Default, AnsiColor.Default, TextAttributes.None);
}
