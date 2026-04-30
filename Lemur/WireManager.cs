using System;
using System.Collections.Generic;

namespace Lemur;

// Owns and tracks all Wire instances in the simulation.
// Subclass and call CreateWire() in the constructor to set up a hardcoded wiring topology.
// All wires created here are visible to the Logic Analyzer via WiresChanged / Wires.
public class WireManager
{
    private readonly List<Wire> m_Wires = new();

    public IReadOnlyList<Wire> Wires => m_Wires.AsReadOnly();

    public event Action? WiresChanged;

    protected Wire CreateWire()
    {
        var wire = new Wire();
        m_Wires.Add(wire);
        WiresChanged?.Invoke();
        return wire;
    }

    protected void RemoveWire(Wire wire)
    {
        if (m_Wires.Remove(wire))
            WiresChanged?.Invoke();
    }
}
