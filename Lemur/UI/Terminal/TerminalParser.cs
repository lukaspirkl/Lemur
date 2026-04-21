using System;
using System.Collections.Generic;

namespace Lemur.UI.Terminal;

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

    private State m_State = State.Ground;
    private readonly List<int> m_Params = new(8);
    private int? m_CurrentParam;       // null = no digit seen yet
    private byte m_PrivateMarker;      // e.g. '?' (0x3F) for DEC private sequences

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
            if (m_State == State.OscString && b == 0x07) // BEL terminates OSC
                m_State = State.Ground;
            return;
        }

        switch (m_State)
        {
            case State.Ground:
                if (b == 0x1B)
                    m_State = State.Escape;
                else if (b >= 0x20)
                    commands.Add(new PrintChar((char)b));
                break;

            case State.Escape:
                m_State = State.Ground;
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
                        m_Params.Clear();
                        m_CurrentParam = null;
                        m_PrivateMarker = 0;
                        m_State = State.CsiEntry;
                        break;
                    case 0x5D:                  // ESC ]  — OSC (ignore content)
                        m_State = State.OscString;
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
                    m_CurrentParam = (m_CurrentParam ?? 0) * 10 + (b - 0x30);
                    m_State = State.CsiParam;
                }
                else if (b == 0x3B) // ';' parameter separator
                {
                    m_Params.Add(m_CurrentParam ?? 0);
                    m_CurrentParam = null;
                    m_State = State.CsiParam;
                }
                else if (b >= 0x3C && b <= 0x3F) // private marker: < = > ?
                {
                    m_PrivateMarker = b;
                    m_State = State.CsiParam;
                }
                else if (b >= 0x40 && b <= 0x7E) // final byte — dispatch
                {
                    m_Params.Add(m_CurrentParam ?? 0);
                    DispatchCsi((char)b, commands);
                    m_Params.Clear();
                    m_CurrentParam = null;
                    m_PrivateMarker = 0;
                    m_State = State.Ground;
                }
                else if (b >= 0x20 && b <= 0x2F) // intermediate byte — absorb, keep going
                {
                    // not tracking intermediates beyond private marker for now
                }
                break;

            case State.OscString:
                if (b == 0x1B) // ESC inside OSC — could be ESC \ (ST)
                    m_State = State.Escape;
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
            i < m_Params.Count && m_Params[i] != 0 ? m_Params[i] : def;

        if (m_PrivateMarker == 0x3F) // DEC private sequences
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
            case 'J': commands.Add(new EraseInDisplay(m_Params.Count > 0 ? m_Params[0] : 0)); break;
            case 'K': commands.Add(new EraseInLine(m_Params.Count > 0 ? m_Params[0] : 0)); break;
            case 'L': commands.Add(new InsertLines(P(0))); break;
            case 'M': commands.Add(new DeleteLines(P(0))); break;
            case 'P': commands.Add(new DeleteChars(P(0))); break;
            case 'm': commands.Add(new SetGraphicsRendition(m_Params.ToArray())); break;
            case 'r':
                commands.Add(new SetScrollRegion(P(0) - 1, P(1) - 1)); // 1-based → 0-based
                break;
            case 's': commands.Add(new SaveCursor()); break;
            case 'u': commands.Add(new RestoreCursor()); break;
        }
    }
}
