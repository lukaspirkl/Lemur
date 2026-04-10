using System;
using System.Collections.Generic;

namespace VentureUI.Terminal;

public class TerminalScrollbackBuffer
{
    public const int MaxLines = 1000;

    private readonly List<TerminalCell[]> _lines = new(MaxLines);

    public int Count => _lines.Count;

    public TerminalCell[] this[int index] => _lines[index];

    public void Push(TerminalCell[] line)
    {
        if (_lines.Count == MaxLines)
            _lines.RemoveAt(0);
        _lines.Add((TerminalCell[])line.Clone());
    }

    public void Clear() => _lines.Clear();
}
