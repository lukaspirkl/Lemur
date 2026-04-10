using System;
using System.Collections.Generic;

namespace VentureUI.Terminal;

/// <summary>
/// VT100/ANSI escape code parser. Stateful; call Feed() with incoming bytes.
/// Emits ITerminalCommand instances; does not know about screen state.
/// </summary>
public class TerminalParser
{
    private enum State
    {
        Ground,
        Escape,
        CsiEntry,
        CsiParam,
        OscString,
    }

    private State _state = State.Ground;
    private readonly List<int> _params = new(8);
    private int? _currentParam;       // null = no digit seen yet
    private byte _privateMarker;      // e.g. '?' (0x3F) for DEC private sequences

    public List<ITerminalCommand> Feed(ReadOnlySpan<byte> data)
    {
        var commands = new List<ITerminalCommand>(data.Length);
        foreach (var b in data)
            ProcessByte(b, commands);
        return commands;
    }

    private void ProcessByte(byte b, List<ITerminalCommand> commands)
    {
        // C0 controls (except ESC) are acted upon in any state.
        if (b < 0x20 && b != 0x1B)
        {
            HandleC0(b, commands);
            if (_state == State.OscString && b == 0x07) // BEL terminates OSC
                _state = State.Ground;
            return;
        }

        switch (_state)
        {
            case State.Ground:
                if (b == 0x1B)
                    _state = State.Escape;
                else if (b >= 0x20)
                    commands.Add(new PrintChar((char)b));
                break;

            case State.Escape:
                _state = State.Ground;
                switch (b)
                {
                    case 0x4D:                  // ESC M  — Reverse Index
                        commands.Add(new ReverseIndex());
                        break;
                    case 0x37:                  // ESC 7  — Save Cursor
                        commands.Add(new SaveCursor());
                        break;
                    case 0x38:                  // ESC 8  — Restore Cursor
                        commands.Add(new RestoreCursor());
                        break;
                    case 0x5B:                  // ESC [  — CSI
                        _params.Clear();
                        _currentParam = null;
                        _privateMarker = 0;
                        _state = State.CsiEntry;
                        break;
                    case 0x5D:                  // ESC ]  — OSC (ignore content)
                        _state = State.OscString;
                        break;
                    case 0x5C:                  // ESC \  — ST (String Terminator, ends OSC)
                        break;
                    // ESC c (RIS) could reset the terminal — ignore for now
                }
                break;

            case State.CsiEntry:
            case State.CsiParam:
                if (b >= 0x30 && b <= 0x39) // digit
                {
                    _currentParam = (_currentParam ?? 0) * 10 + (b - 0x30);
                    _state = State.CsiParam;
                }
                else if (b == 0x3B) // ';' parameter separator
                {
                    _params.Add(_currentParam ?? 0);
                    _currentParam = null;
                    _state = State.CsiParam;
                }
                else if (b >= 0x3C && b <= 0x3F) // private marker: < = > ?
                {
                    _privateMarker = b;
                    _state = State.CsiParam;
                }
                else if (b >= 0x40 && b <= 0x7E) // final byte — dispatch
                {
                    _params.Add(_currentParam ?? 0);
                    DispatchCsi((char)b, commands);
                    _params.Clear();
                    _currentParam = null;
                    _privateMarker = 0;
                    _state = State.Ground;
                }
                else if (b >= 0x20 && b <= 0x2F) // intermediate byte — absorb, keep going
                {
                    // not tracking intermediates beyond private marker for now
                }
                break;

            case State.OscString:
                if (b == 0x1B) // ESC inside OSC — could be ESC \ (ST)
                    _state = State.Escape;
                // otherwise discard
                break;
        }
    }

    private static void HandleC0(byte b, List<ITerminalCommand> commands)
    {
        switch (b)
        {
            case 0x07: commands.Add(new Bell()); break;
            case 0x08: commands.Add(new Backspace()); break;
            case 0x09: commands.Add(new Tab()); break;
            case 0x0A:
            case 0x0B:
            case 0x0C: commands.Add(new LineFeed()); break;
            case 0x0D: commands.Add(new CarriageReturn()); break;
        }
    }

    private void DispatchCsi(char final, List<ITerminalCommand> commands)
    {
        // P(i, def): param i, treating absent-or-zero as `def`
        int P(int i, int def = 1) =>
            i < _params.Count && _params[i] != 0 ? _params[i] : def;

        if (_privateMarker == 0x3F) // DEC private sequences
        {
            switch (final)
            {
                case 'h':
                    if (P(0) == 25) commands.Add(new ShowCursor(true));
                    // ?1049h (alternate screen) — extensible here
                    break;
                case 'l':
                    if (P(0) == 25) commands.Add(new ShowCursor(false));
                    break;
            }
            return;
        }

        switch (final)
        {
            case 'A': commands.Add(new CursorUp(P(0))); break;
            case 'B': commands.Add(new CursorDown(P(0))); break;
            case 'C': commands.Add(new CursorForward(P(0))); break;
            case 'D': commands.Add(new CursorBack(P(0))); break;
            case 'E': commands.Add(new CursorNextLine(P(0))); break;
            case 'F': commands.Add(new CursorPreviousLine(P(0))); break;
            case 'G': commands.Add(new CursorHorizontalAbsolute(P(0) - 1)); break; // 1-based → 0-based
            case 'H':
            case 'f': commands.Add(new MoveCursor(P(0) - 1, P(1) - 1)); break;    // 1-based → 0-based
            case 'J': commands.Add(new EraseInDisplay(_params.Count > 0 ? _params[0] : 0)); break;
            case 'K': commands.Add(new EraseInLine(_params.Count > 0 ? _params[0] : 0)); break;
            case 'L': commands.Add(new InsertLines(P(0))); break;
            case 'M': commands.Add(new DeleteLines(P(0))); break;
            case 'P': commands.Add(new DeleteChars(P(0))); break;
            case 'm': commands.Add(new SetGraphicsRendition(_params.ToArray())); break;
            case 'r':
                commands.Add(new SetScrollRegion(P(0) - 1, P(1) - 1)); // 1-based → 0-based
                break;
            case 's': commands.Add(new SaveCursor()); break;
            case 'u': commands.Add(new RestoreCursor()); break;
        }
    }
}
