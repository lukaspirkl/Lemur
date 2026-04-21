using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lemur.Debug;

internal class GdbCommandRouter
{
    private record Route(Func<string, bool> Matches, Func<string, string?> Handle);
    private readonly List<Route> m_Routes = [];

    public void Map(string exact, Func<string, string?> handler) =>
        m_Routes.Add(new(cmd => cmd == exact, handler));

    public void MapPrefix(string prefix, Func<string, string?> handler) =>
        m_Routes.Add(new(cmd => cmd.StartsWith(prefix), handler));

    public void MapPattern(Regex pattern, Func<string, string?> handler) =>
        m_Routes.Add(new(cmd => pattern.IsMatch(cmd), handler));

    public bool TryDispatch(string command, out string? response)
    {
        var route = m_Routes.FirstOrDefault(r => r.Matches(command));
        if (route == null) { response = null; return false; }
        response = route.Handle(command);
        return true;
    }
}
