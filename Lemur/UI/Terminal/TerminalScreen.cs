using System;

namespace Lemur.UI.Terminal;

/// <summary>
/// The active terminal grid. Applies ITerminalCommand instances to maintain
/// cursor position, cell content, and scroll region state.
/// Fires RowEvicted when a line scrolls off the top (for the scrollback buffer).
/// </summary>
public class TerminalScreen
{
    private TerminalCell[,] m_Grid;
    private int m_Rows;
    private int m_Cols;

    private int m_CursorRow;
    private int m_CursorCol;
    private int m_SavedRow;
    private int m_SavedCol;
    private bool m_CursorVisible = true;

    private int m_ScrollTop;
    private int m_ScrollBottom;

    private AnsiColor m_CurrentFg = AnsiColor.Default;
    private AnsiColor m_CurrentBg = AnsiColor.Default;
    private TextAttributes m_CurrentAttrs = TextAttributes.None;

    public int Rows => m_Rows;
    public int Cols => m_Cols;
    public int CursorRow => m_CursorRow;
    public int CursorCol => m_CursorCol;
    public bool CursorVisible => m_CursorVisible;

    /// <summary>Fired when a line is scrolled off the top of the scroll region.</summary>
    public event Action<TerminalCell[]>? RowEvicted;

    public TerminalScreen(int rows, int cols)
    {
        m_Rows = rows;
        m_Cols = cols;
        m_Grid = new TerminalCell[rows, cols];
        FillGrid(0, 0, rows, cols, TerminalCell.Empty);
        m_ScrollBottom = rows - 1;
    }

    public TerminalCell GetCell(int row, int col)
    {
        if (row < 0 || row >= m_Rows || col < 0 || col >= m_Cols)
            return TerminalCell.Empty;
        return m_Grid[row, col];
    }

    public void Resize(int newRows, int newCols)
    {
        var newGrid = new TerminalCell[newRows, newCols];
        int copyRows = Math.Min(m_Rows, newRows);
        int copyCols = Math.Min(m_Cols, newCols);

        for (int r = 0; r < copyRows; r++)
            for (int c = 0; c < copyCols; c++)
                newGrid[r, c] = m_Grid[r, c];

        // Fill any new cells
        for (int r = 0; r < newRows; r++)
            for (int c = copyCols; c < newCols; c++)
                newGrid[r, c] = TerminalCell.Empty;
        for (int r = copyRows; r < newRows; r++)
            for (int c = 0; c < newCols; c++)
                newGrid[r, c] = TerminalCell.Empty;

        m_Grid = newGrid;
        m_Rows = newRows;
        m_Cols = newCols;
        m_CursorRow = Math.Min(m_CursorRow, m_Rows - 1);
        m_CursorCol = Math.Min(m_CursorCol, m_Cols - 1);
        m_ScrollTop = 0;
        m_ScrollBottom = m_Rows - 1;
    }

    public void Apply(ITerminalCommand command)
    {
        switch (command)
        {
            case PrintChar p:
                WriteChar(p.Char);
                break;

            case LineFeed:
                if (m_CursorRow == m_ScrollBottom)
                    ScrollUp();
                else
                    m_CursorRow = Math.Min(m_CursorRow + 1, m_Rows - 1);
                break;

            case CarriageReturn:
                m_CursorCol = 0;
                break;

            case Backspace:
                if (m_CursorCol > 0) m_CursorCol--;
                break;

            case Tab:
                m_CursorCol = Math.Min((m_CursorCol / 8 + 1) * 8, m_Cols - 1);
                break;

            case Bell:
                break; // Could trigger a visual bell later

            case MoveCursor mc:
                m_CursorRow = Clamp(mc.Row, 0, m_Rows - 1);
                m_CursorCol = Clamp(mc.Col, 0, m_Cols - 1);
                break;

            case CursorUp cu:
                m_CursorRow = Math.Max(m_CursorRow - cu.Count, m_ScrollTop);
                break;

            case CursorDown cd:
                m_CursorRow = Math.Min(m_CursorRow + cd.Count, m_ScrollBottom);
                break;

            case CursorForward cf:
                m_CursorCol = Math.Min(m_CursorCol + cf.Count, m_Cols - 1);
                break;

            case CursorBack cb:
                m_CursorCol = Math.Max(m_CursorCol - cb.Count, 0);
                break;

            case CursorNextLine cnl:
                m_CursorRow = Math.Min(m_CursorRow + cnl.Count, m_Rows - 1);
                m_CursorCol = 0;
                break;

            case CursorPreviousLine cpl:
                m_CursorRow = Math.Max(m_CursorRow - cpl.Count, 0);
                m_CursorCol = 0;
                break;

            case CursorHorizontalAbsolute cha:
                m_CursorCol = Clamp(cha.Col, 0, m_Cols - 1);
                break;

            case EraseInDisplay eid:
                switch (eid.Mode)
                {
                    case 0: // cursor to end
                        EraseRange(m_CursorRow, m_CursorCol, m_Rows - 1, m_Cols - 1);
                        break;
                    case 1: // start to cursor
                        EraseRange(0, 0, m_CursorRow, m_CursorCol);
                        break;
                    case 2: // all
                        EraseRange(0, 0, m_Rows - 1, m_Cols - 1);
                        break;
                }
                break;

            case EraseInLine eil:
                switch (eil.Mode)
                {
                    case 0: EraseRange(m_CursorRow, m_CursorCol, m_CursorRow, m_Cols - 1); break;
                    case 1: EraseRange(m_CursorRow, 0, m_CursorRow, m_CursorCol); break;
                    case 2: EraseRange(m_CursorRow, 0, m_CursorRow, m_Cols - 1); break;
                }
                break;

            case ReverseIndex:
                if (m_CursorRow == m_ScrollTop)
                    ScrollDown();
                else
                    m_CursorRow = Math.Max(m_CursorRow - 1, 0);
                break;

            case SetScrollRegion ssr:
                m_ScrollTop = Clamp(ssr.Top, 0, m_Rows - 2);
                m_ScrollBottom = Clamp(ssr.Bottom, m_ScrollTop + 1, m_Rows - 1);
                m_CursorRow = m_ScrollTop;
                m_CursorCol = 0;
                break;

            case InsertLines il:
                for (int i = 0; i < il.Count; i++)
                    InsertLineAt(m_CursorRow);
                break;

            case DeleteLines dl:
                for (int i = 0; i < dl.Count; i++)
                    DeleteLineAt(m_CursorRow);
                break;

            case DeleteChars dc:
                DeleteCharsAt(m_CursorRow, m_CursorCol, dc.Count);
                break;

            case SaveCursor:
                m_SavedRow = m_CursorRow;
                m_SavedCol = m_CursorCol;
                break;

            case RestoreCursor:
                m_CursorRow = m_SavedRow;
                m_CursorCol = m_SavedCol;
                break;

            case ShowCursor sc:
                m_CursorVisible = sc.Visible;
                break;

            case SetGraphicsRendition sgr:
                ApplySgr(sgr.Params);
                break;
        }
    }

    // --- Private helpers ---

    private void WriteChar(char c)
    {
        if (m_CursorCol >= m_Cols)
        {
            // Wrap to next line
            m_CursorCol = 0;
            if (m_CursorRow == m_ScrollBottom)
                ScrollUp();
            else
                m_CursorRow = Math.Min(m_CursorRow + 1, m_Rows - 1);
        }

        m_Grid[m_CursorRow, m_CursorCol] = new TerminalCell(c, m_CurrentFg, m_CurrentBg, m_CurrentAttrs);
        m_CursorCol++;
    }

    private void ScrollUp()
    {
        // Evict the top row of the scroll region into scrollback
        var evicted = new TerminalCell[m_Cols];
        for (int c = 0; c < m_Cols; c++)
            evicted[c] = m_Grid[m_ScrollTop, c];
        RowEvicted?.Invoke(evicted);

        // Shift rows up within scroll region
        for (int r = m_ScrollTop; r < m_ScrollBottom; r++)
            for (int c = 0; c < m_Cols; c++)
                m_Grid[r, c] = m_Grid[r + 1, c];

        // Clear new bottom row
        for (int c = 0; c < m_Cols; c++)
            m_Grid[m_ScrollBottom, c] = TerminalCell.Empty;
    }

    private void ScrollDown()
    {
        // Shift rows down within scroll region (bottom row is lost)
        for (int r = m_ScrollBottom; r > m_ScrollTop; r--)
            for (int c = 0; c < m_Cols; c++)
                m_Grid[r, c] = m_Grid[r - 1, c];

        // Clear new top row
        for (int c = 0; c < m_Cols; c++)
            m_Grid[m_ScrollTop, c] = TerminalCell.Empty;
    }

    private void InsertLineAt(int row)
    {
        // Rows from `row` to scrollBottom-1 shift down; scrollBottom row is lost
        for (int r = m_ScrollBottom; r > row; r--)
            for (int c = 0; c < m_Cols; c++)
                m_Grid[r, c] = m_Grid[r - 1, c];

        for (int c = 0; c < m_Cols; c++)
            m_Grid[row, c] = TerminalCell.Empty;
    }

    private void DeleteLineAt(int row)
    {
        for (int r = row; r < m_ScrollBottom; r++)
            for (int c = 0; c < m_Cols; c++)
                m_Grid[r, c] = m_Grid[r + 1, c];

        for (int c = 0; c < m_Cols; c++)
            m_Grid[m_ScrollBottom, c] = TerminalCell.Empty;
    }

    private void DeleteCharsAt(int row, int col, int count)
    {
        for (int c = col; c < m_Cols; c++)
            m_Grid[row, c] = c + count < m_Cols ? m_Grid[row, c + count] : TerminalCell.Empty;
    }

    private void EraseRange(int startRow, int startCol, int endRow, int endCol)
    {
        for (int r = startRow; r <= endRow; r++)
        {
            int c0 = r == startRow ? startCol : 0;
            int c1 = r == endRow ? endCol : m_Cols - 1;
            for (int c = c0; c <= c1; c++)
                m_Grid[r, c] = TerminalCell.Empty;
        }
    }

    private void FillGrid(int startRow, int startCol, int endRow, int endCol, TerminalCell cell)
    {
        for (int r = startRow; r < endRow; r++)
            for (int c = startCol; c < endCol; c++)
                m_Grid[r, c] = cell;
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
                case 1: m_CurrentAttrs |= TextAttributes.Bold; break;
                case 4: m_CurrentAttrs |= TextAttributes.Underline; break;
                case 5: m_CurrentAttrs |= TextAttributes.Blink; break;
                case 7: m_CurrentAttrs |= TextAttributes.Inverse; break;
                case 21:
                case 22: m_CurrentAttrs &= ~TextAttributes.Bold; break;
                case 24: m_CurrentAttrs &= ~TextAttributes.Underline; break;
                case 25: m_CurrentAttrs &= ~TextAttributes.Blink; break;
                case 27: m_CurrentAttrs &= ~TextAttributes.Inverse; break;
                case 39: m_CurrentFg = AnsiColor.Default; break;
                case 49: m_CurrentBg = AnsiColor.Default; break;

                // Standard foreground colors 30–37, bright 90–97
                case >= 30 and <= 37: m_CurrentFg = (AnsiColor)(p - 30 + 1); break;
                case >= 90 and <= 97: m_CurrentFg = (AnsiColor)(p - 90 + 9); break;

                // Standard background colors 40–47, bright 100–107
                case >= 40 and <= 47: m_CurrentBg = (AnsiColor)(p - 40 + 1); break;
                case >= 100 and <= 107: m_CurrentBg = (AnsiColor)(p - 100 + 9); break;
            }
        }
    }

    private void ResetSgr()
    {
        m_CurrentFg = AnsiColor.Default;
        m_CurrentBg = AnsiColor.Default;
        m_CurrentAttrs = TextAttributes.None;
    }

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
}
