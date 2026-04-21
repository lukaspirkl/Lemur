using System.Collections.Generic;

namespace Lemur.UI.Terminal;

public class TerminalScrollbackBuffer
{
    public const int MAX_LINES = 1000;

    private readonly List<TerminalCell[]> m_Lines = new(MAX_LINES);

    public int Count => m_Lines.Count;

    public TerminalCell[] this[int index] => m_Lines[index];

    public void Push(TerminalCell[] line)
    {
        if (m_Lines.Count == MAX_LINES)
            m_Lines.RemoveAt(0);
        m_Lines.Add((TerminalCell[])line.Clone());
    }

    public void Clear() => m_Lines.Clear();
}
