using System;

namespace VentureUI.Terminal;

/// <summary>
/// The active terminal grid. Applies ITerminalCommand instances to maintain
/// cursor position, cell content, and scroll region state.
/// Fires RowEvicted when a line scrolls off the top (for the scrollback buffer).
/// </summary>
public class TerminalScreen
{
    private TerminalCell[,] _grid;
    private int _rows;
    private int _cols;

    private int _cursorRow;
    private int _cursorCol;
    private int _savedRow;
    private int _savedCol;
    private bool _cursorVisible = true;

    private int _scrollTop;
    private int _scrollBottom;

    private AnsiColor _currentFg = AnsiColor.Default;
    private AnsiColor _currentBg = AnsiColor.Default;
    private TextAttributes _currentAttrs = TextAttributes.None;

    public int Rows => _rows;
    public int Cols => _cols;
    public int CursorRow => _cursorRow;
    public int CursorCol => _cursorCol;
    public bool CursorVisible => _cursorVisible;

    /// <summary>Fired when a line is scrolled off the top of the scroll region.</summary>
    public event Action<TerminalCell[]>? RowEvicted;

    public TerminalScreen(int rows, int cols)
    {
        _rows = rows;
        _cols = cols;
        _grid = new TerminalCell[rows, cols];
        FillGrid(0, 0, rows, cols, TerminalCell.Empty);
        _scrollBottom = rows - 1;
    }

    public TerminalCell GetCell(int row, int col)
    {
        if (row < 0 || row >= _rows || col < 0 || col >= _cols)
            return TerminalCell.Empty;
        return _grid[row, col];
    }

    public void Resize(int newRows, int newCols)
    {
        var newGrid = new TerminalCell[newRows, newCols];
        int copyRows = Math.Min(_rows, newRows);
        int copyCols = Math.Min(_cols, newCols);

        for (int r = 0; r < copyRows; r++)
            for (int c = 0; c < copyCols; c++)
                newGrid[r, c] = _grid[r, c];

        // Fill any new cells
        for (int r = 0; r < newRows; r++)
            for (int c = copyCols; c < newCols; c++)
                newGrid[r, c] = TerminalCell.Empty;
        for (int r = copyRows; r < newRows; r++)
            for (int c = 0; c < newCols; c++)
                newGrid[r, c] = TerminalCell.Empty;

        _grid = newGrid;
        _rows = newRows;
        _cols = newCols;
        _cursorRow = Math.Min(_cursorRow, _rows - 1);
        _cursorCol = Math.Min(_cursorCol, _cols - 1);
        _scrollTop = 0;
        _scrollBottom = _rows - 1;
    }

    public void Apply(ITerminalCommand command)
    {
        switch (command)
        {
            case PrintChar p:
                WriteChar(p.Char);
                break;

            case LineFeed:
                if (_cursorRow == _scrollBottom)
                    ScrollUp();
                else
                    _cursorRow = Math.Min(_cursorRow + 1, _rows - 1);
                break;

            case CarriageReturn:
                _cursorCol = 0;
                break;

            case Backspace:
                if (_cursorCol > 0) _cursorCol--;
                break;

            case Tab:
                _cursorCol = Math.Min((_cursorCol / 8 + 1) * 8, _cols - 1);
                break;

            case Bell:
                break; // Could trigger a visual bell later

            case MoveCursor mc:
                _cursorRow = Clamp(mc.Row, 0, _rows - 1);
                _cursorCol = Clamp(mc.Col, 0, _cols - 1);
                break;

            case CursorUp cu:
                _cursorRow = Math.Max(_cursorRow - cu.Count, _scrollTop);
                break;

            case CursorDown cd:
                _cursorRow = Math.Min(_cursorRow + cd.Count, _scrollBottom);
                break;

            case CursorForward cf:
                _cursorCol = Math.Min(_cursorCol + cf.Count, _cols - 1);
                break;

            case CursorBack cb:
                _cursorCol = Math.Max(_cursorCol - cb.Count, 0);
                break;

            case CursorNextLine cnl:
                _cursorRow = Math.Min(_cursorRow + cnl.Count, _rows - 1);
                _cursorCol = 0;
                break;

            case CursorPreviousLine cpl:
                _cursorRow = Math.Max(_cursorRow - cpl.Count, 0);
                _cursorCol = 0;
                break;

            case CursorHorizontalAbsolute cha:
                _cursorCol = Clamp(cha.Col, 0, _cols - 1);
                break;

            case EraseInDisplay eid:
                switch (eid.Mode)
                {
                    case 0: // cursor to end
                        EraseRange(_cursorRow, _cursorCol, _rows - 1, _cols - 1);
                        break;
                    case 1: // start to cursor
                        EraseRange(0, 0, _cursorRow, _cursorCol);
                        break;
                    case 2: // all
                        EraseRange(0, 0, _rows - 1, _cols - 1);
                        break;
                }
                break;

            case EraseInLine eil:
                switch (eil.Mode)
                {
                    case 0: EraseRange(_cursorRow, _cursorCol, _cursorRow, _cols - 1); break;
                    case 1: EraseRange(_cursorRow, 0, _cursorRow, _cursorCol); break;
                    case 2: EraseRange(_cursorRow, 0, _cursorRow, _cols - 1); break;
                }
                break;

            case ReverseIndex:
                if (_cursorRow == _scrollTop)
                    ScrollDown();
                else
                    _cursorRow = Math.Max(_cursorRow - 1, 0);
                break;

            case SetScrollRegion ssr:
                _scrollTop = Clamp(ssr.Top, 0, _rows - 2);
                _scrollBottom = Clamp(ssr.Bottom, _scrollTop + 1, _rows - 1);
                _cursorRow = _scrollTop;
                _cursorCol = 0;
                break;

            case InsertLines il:
                for (int i = 0; i < il.Count; i++)
                    InsertLineAt(_cursorRow);
                break;

            case DeleteLines dl:
                for (int i = 0; i < dl.Count; i++)
                    DeleteLineAt(_cursorRow);
                break;

            case DeleteChars dc:
                DeleteCharsAt(_cursorRow, _cursorCol, dc.Count);
                break;

            case SaveCursor:
                _savedRow = _cursorRow;
                _savedCol = _cursorCol;
                break;

            case RestoreCursor:
                _cursorRow = _savedRow;
                _cursorCol = _savedCol;
                break;

            case ShowCursor sc:
                _cursorVisible = sc.Visible;
                break;

            case SetGraphicsRendition sgr:
                ApplySgr(sgr.Params);
                break;
        }
    }

    // --- Private helpers ---

    private void WriteChar(char c)
    {
        if (_cursorCol >= _cols)
        {
            // Wrap to next line
            _cursorCol = 0;
            if (_cursorRow == _scrollBottom)
                ScrollUp();
            else
                _cursorRow = Math.Min(_cursorRow + 1, _rows - 1);
        }

        _grid[_cursorRow, _cursorCol] = new TerminalCell(c, _currentFg, _currentBg, _currentAttrs);
        _cursorCol++;
    }

    private void ScrollUp()
    {
        // Evict the top row of the scroll region into scrollback
        var evicted = new TerminalCell[_cols];
        for (int c = 0; c < _cols; c++)
            evicted[c] = _grid[_scrollTop, c];
        RowEvicted?.Invoke(evicted);

        // Shift rows up within scroll region
        for (int r = _scrollTop; r < _scrollBottom; r++)
            for (int c = 0; c < _cols; c++)
                _grid[r, c] = _grid[r + 1, c];

        // Clear new bottom row
        for (int c = 0; c < _cols; c++)
            _grid[_scrollBottom, c] = TerminalCell.Empty;
    }

    private void ScrollDown()
    {
        // Shift rows down within scroll region (bottom row is lost)
        for (int r = _scrollBottom; r > _scrollTop; r--)
            for (int c = 0; c < _cols; c++)
                _grid[r, c] = _grid[r - 1, c];

        // Clear new top row
        for (int c = 0; c < _cols; c++)
            _grid[_scrollTop, c] = TerminalCell.Empty;
    }

    private void InsertLineAt(int row)
    {
        // Rows from `row` to scrollBottom-1 shift down; scrollBottom row is lost
        for (int r = _scrollBottom; r > row; r--)
            for (int c = 0; c < _cols; c++)
                _grid[r, c] = _grid[r - 1, c];

        for (int c = 0; c < _cols; c++)
            _grid[row, c] = TerminalCell.Empty;
    }

    private void DeleteLineAt(int row)
    {
        for (int r = row; r < _scrollBottom; r++)
            for (int c = 0; c < _cols; c++)
                _grid[r, c] = _grid[r + 1, c];

        for (int c = 0; c < _cols; c++)
            _grid[_scrollBottom, c] = TerminalCell.Empty;
    }

    private void DeleteCharsAt(int row, int col, int count)
    {
        for (int c = col; c < _cols; c++)
            _grid[row, c] = c + count < _cols ? _grid[row, c + count] : TerminalCell.Empty;
    }

    private void EraseRange(int startRow, int startCol, int endRow, int endCol)
    {
        for (int r = startRow; r <= endRow; r++)
        {
            int c0 = r == startRow ? startCol : 0;
            int c1 = r == endRow ? endCol : _cols - 1;
            for (int c = c0; c <= c1; c++)
                _grid[r, c] = TerminalCell.Empty;
        }
    }

    private void FillGrid(int startRow, int startCol, int endRow, int endCol, TerminalCell cell)
    {
        for (int r = startRow; r < endRow; r++)
            for (int c = startCol; c < endCol; c++)
                _grid[r, c] = cell;
    }

    private void ApplySgr(int[] ps)
    {
        if (ps.Length == 0)
        {
            ResetSgr();
            return;
        }

        int i = 0;
        while (i < ps.Length)
        {
            int p = ps[i++];
            switch (p)
            {
                case 0: ResetSgr(); break;
                case 1: _currentAttrs |= TextAttributes.Bold; break;
                case 4: _currentAttrs |= TextAttributes.Underline; break;
                case 5: _currentAttrs |= TextAttributes.Blink; break;
                case 7: _currentAttrs |= TextAttributes.Inverse; break;
                case 21:
                case 22: _currentAttrs &= ~TextAttributes.Bold; break;
                case 24: _currentAttrs &= ~TextAttributes.Underline; break;
                case 25: _currentAttrs &= ~TextAttributes.Blink; break;
                case 27: _currentAttrs &= ~TextAttributes.Inverse; break;
                case 39: _currentFg = AnsiColor.Default; break;
                case 49: _currentBg = AnsiColor.Default; break;

                // Standard foreground colors 30–37, bright 90–97
                case >= 30 and <= 37: _currentFg = (AnsiColor)(p - 30 + 1); break;
                case >= 90 and <= 97: _currentFg = (AnsiColor)(p - 90 + 9); break;

                // Standard background colors 40–47, bright 100–107
                case >= 40 and <= 47: _currentBg = (AnsiColor)(p - 40 + 1); break;
                case >= 100 and <= 107: _currentBg = (AnsiColor)(p - 100 + 9); break;
            }
        }
    }

    private void ResetSgr()
    {
        _currentFg = AnsiColor.Default;
        _currentBg = AnsiColor.Default;
        _currentAttrs = TextAttributes.None;
    }

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
}
