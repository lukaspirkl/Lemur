using System;
using System.Collections.Generic;

namespace VentureUI.Terminal;

/// <summary>
/// Orchestrates the parser, active screen, and scrollback buffer.
/// Thread-safe for Feed() calls from non-UI threads.
/// </summary>
public class TerminalEmulator
{
    private readonly TerminalParser _parser = new();
    private readonly TerminalScrollbackBuffer _scrollback = new();
    private TerminalScreen _screen;

    private readonly object _lock = new();
    private int _scrollOffset; // lines scrolled up from the live view (0 = live)

    public int Rows => _screen.Rows;
    public int Cols => _screen.Cols;
    public int CursorRow => _screen.CursorRow;
    public int CursorCol => _screen.CursorCol;
    public bool CursorVisible => _screen.CursorVisible;

    /// <summary>Total lines: scrollback history + active screen rows.</summary>
    public int TotalLines => _scrollback.Count + _screen.Rows;

    /// <summary>How far the view is scrolled up from the live bottom (0 = live).</summary>
    public int ScrollOffset
    {
        get => _scrollOffset;
        set => _scrollOffset = Math.Clamp(value, 0, MaxScrollOffset);
    }

    public int MaxScrollOffset => _scrollback.Count;

    /// <summary>Fired on the calling thread after each Feed() completes.</summary>
    public event Action? Changed;

    public TerminalEmulator(int rows = 24, int cols = 80)
    {
        _screen = new TerminalScreen(rows, cols);
        _screen.RowEvicted += line =>
        {
            _scrollback.Push(line);
            // Keep the scroll offset pinned to the same content when new lines arrive
            if (_scrollOffset > 0)
                _scrollOffset = Math.Min(_scrollOffset + 1, MaxScrollOffset);
        };
    }

    public void Feed(ReadOnlySpan<byte> data)
    {
        List<ITerminalCommand> commands;
        lock (_lock)
        {
            commands = _parser.Feed(data);
            foreach (var cmd in commands)
                _screen.Apply(cmd);
        }
        Changed?.Invoke();
    }

    public void Resize(int rows, int cols)
    {
        if (rows == _screen.Rows && cols == _screen.Cols)
            return;
        lock (_lock)
        {
            _screen.Resize(rows, cols);
            _scrollOffset = Math.Min(_scrollOffset, MaxScrollOffset);
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
        int firstVisible = _scrollback.Count - _scrollOffset;
        int absoluteRow = firstVisible + viewRow;

        if (absoluteRow < 0 || absoluteRow >= TotalLines)
            return TerminalCell.Empty;

        if (absoluteRow < _scrollback.Count)
        {
            var line = _scrollback[absoluteRow];
            return col < line.Length ? line[col] : TerminalCell.Empty;
        }

        return _screen.GetCell(absoluteRow - _scrollback.Count, col);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _scrollback.Clear();
            _screen = new TerminalScreen(_screen.Rows, _screen.Cols);
            _screen.RowEvicted += line =>
            {
                _scrollback.Push(line);
                if (_scrollOffset > 0)
                    _scrollOffset = Math.Min(_scrollOffset + 1, MaxScrollOffset);
            };
            _scrollOffset = 0;
        }
        Changed?.Invoke();
    }
}
