namespace Lemur.UI.Terminal;

public interface ITerminalCommand { }

// Character output
public record PrintChar(char Char) : ITerminalCommand;

// Cursor movement (all positions are 0-based)
public record MoveCursor(int Row, int Col) : ITerminalCommand;
public record CursorUp(int Count) : ITerminalCommand;
public record CursorDown(int Count) : ITerminalCommand;
public record CursorForward(int Count) : ITerminalCommand;
public record CursorBack(int Count) : ITerminalCommand;
public record CursorNextLine(int Count) : ITerminalCommand;
public record CursorPreviousLine(int Count) : ITerminalCommand;
public record CursorHorizontalAbsolute(int Col) : ITerminalCommand;
public record SaveCursor : ITerminalCommand;
public record RestoreCursor : ITerminalCommand;
public record ShowCursor(bool Visible) : ITerminalCommand;

// Erase
public record EraseInDisplay(int Mode) : ITerminalCommand;  // 0=cursor→end, 1=start→cursor, 2=all
public record EraseInLine(int Mode) : ITerminalCommand;     // 0=cursor→end, 1=start→cursor, 2=all

// Scrolling
public record ReverseIndex : ITerminalCommand;              // ESC M  — scroll down / move cursor up
public record SetScrollRegion(int Top, int Bottom) : ITerminalCommand;

// Line/character editing
public record InsertLines(int Count) : ITerminalCommand;
public record DeleteLines(int Count) : ITerminalCommand;
public record DeleteChars(int Count) : ITerminalCommand;

// C0 controls
public record LineFeed : ITerminalCommand;
public record CarriageReturn : ITerminalCommand;
public record Backspace : ITerminalCommand;
public record Tab : ITerminalCommand;
public record Bell : ITerminalCommand;

// Appearance
public record SetGraphicsRendition(int[] Params) : ITerminalCommand;
