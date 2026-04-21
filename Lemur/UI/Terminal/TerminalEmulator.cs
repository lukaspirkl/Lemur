using System;
using System.Collections.Generic;

namespace Lemur.UI.Terminal;

/// <summary>
/// Orchestrates the parser, active screen, and scrollback buffer.
/// Thread-safe for Feed() calls from non-UI threads.
/// </summary>
public class TerminalEmulator
{
    private readonly TerminalParser m_Parser = new();
    private readonly TerminalScrollbackBuffer m_Scrollback = new();
    private TerminalScreen m_Screen;

    private readonly object m_Lock = new();
    private int m_ScrollOffset; // lines scrolled up from the live view (0 = live)

    public int Rows => m_Screen.Rows;
    public int Cols => m_Screen.Cols;
    public int CursorRow => m_Screen.CursorRow;
    public int CursorCol => m_Screen.CursorCol;
    public bool CursorVisible => m_Screen.CursorVisible;

    /// <summary>Total lines: scrollback history + active screen rows.</summary>
    public int TotalLines => m_Scrollback.Count + m_Screen.Rows;

    /// <summary>How far the view is scrolled up from the live bottom (0 = live).</summary>
    public int ScrollOffset
    {
        get => m_ScrollOffset;
        set => m_ScrollOffset = Math.Clamp(value, 0, MaxScrollOffset);
    }

    public int MaxScrollOffset => m_Scrollback.Count;

    /// <summary>Fired on the calling thread after each Feed() completes.</summary>
    public event Action? Changed;

    public TerminalEmulator(int rows = 24, int cols = 80)
    {
        m_Screen = new TerminalScreen(rows, cols);
        m_Screen.RowEvicted += line =>
        {
            m_Scrollback.Push(line);
            // Keep the scroll offset pinned to the same content when new lines arrive
            if (m_ScrollOffset > 0)
                m_ScrollOffset = Math.Min(m_ScrollOffset + 1, MaxScrollOffset);
        };
    }

    public void Feed(ReadOnlySpan<byte> data)
    {
        List<ITerminalCommand> commands;
        lock (m_Lock)
        {
            commands = m_Parser.Feed(data);
            foreach (var cmd in commands)
                m_Screen.Apply(cmd);
        }
        Changed?.Invoke();
    }

    public void Resize(int rows, int cols)
    {
        if (rows == m_Screen.Rows && cols == m_Screen.Cols)
            return;
        lock (m_Lock)
        {
            m_Screen.Resize(rows, cols);
            m_ScrollOffset = Math.Min(m_ScrollOffset, MaxScrollOffset);
        }
        Changed?.Invoke();
    }

    /// <summary>
    /// Returns the cell that should appear at (viewRow, col) in the current viewport.
    /// viewRow 0 is the top of what the user sees.
    /// </summary>
    public TerminalCell GetCell(int viewRow, int col)
    {
        // The viewport shows `_screen.Rows` lines ending at the bottom of the virtual buffer,
        // then scrolled up by `_scrollOffset`.
        //
        // Virtual line index (0 = oldest scrollback):
        //   firstVisible = scrollback.Count + screen.Rows - screen.Rows - scrollOffset
        //                = scrollback.Count - scrollOffset
        int firstVisible = m_Scrollback.Count - m_ScrollOffset;
        int absoluteRow = firstVisible + viewRow;

        if (absoluteRow < 0 || absoluteRow >= TotalLines)
            return TerminalCell.Empty;

        if (absoluteRow < m_Scrollback.Count)
        {
            var line = m_Scrollback[absoluteRow];
            return col < line.Length ? line[col] : TerminalCell.Empty;
        }

        return m_Screen.GetCell(absoluteRow - m_Scrollback.Count, col);
    }

    public void Clear()
    {
        lock (m_Lock)
        {
            m_Scrollback.Clear();
            m_Screen = new TerminalScreen(m_Screen.Rows, m_Screen.Cols);
            m_Screen.RowEvicted += line =>
            {
                m_Scrollback.Push(line);
                if (m_ScrollOffset > 0)
                    m_ScrollOffset = Math.Min(m_ScrollOffset + 1, MaxScrollOffset);
            };
            m_ScrollOffset = 0;
        }
        Changed?.Invoke();
    }
}
