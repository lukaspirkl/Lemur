using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemur;

public class Wire
{
    private readonly Dictionary<Pin, PinDrive> m_PinDrives = new();

    public bool? State { get; private set; }

    public event Action<SignalChange>? Changed;

    // Auto-derived from connected pin names — useful for Logic Analyzer display.
    public string Name => m_PinDrives.Count == 0
        ? "(no pins)"
        : string.Join(" ↔ ", m_PinDrives.Keys.Select(p => p.Name));

    public IReadOnlyCollection<Pin> ConnectedPins => m_PinDrives.Keys.ToList();

    public void Connect(Pin pin, TimeSpan time)    => pin.Connect(this, time);
    public void Disconnect(Pin pin, TimeSpan time) => pin.Disconnect(time);

    internal void UpdatePinDrive(Pin pin, TimeSpan time, bool? output, PullDirection pull)
    {
        m_PinDrives[pin] = new PinDrive(output, pull);
        ResolveAndNotify(time);
    }

    internal void RemovePinDrive(Pin pin, TimeSpan time)
    {
        m_PinDrives.Remove(pin);
        ResolveAndNotify(time);
    }

    private void ResolveAndNotify(TimeSpan time)
    {
        var oldState = State;
        State = Resolve();
        Changed?.Invoke(new SignalChange(time, State, oldState));
    }

    private bool? Resolve()
    {
        bool hasStrongHigh = m_PinDrives.Values.Any(d => d.Output == true);
        bool hasStrongLow  = m_PinDrives.Values.Any(d => d.Output == false);

        if (hasStrongHigh && hasStrongLow)
            throw new InvalidOperationException(
                $"Bus conflict on wire '{Name}': opposing strong drivers.");

        if (hasStrongHigh) return true;
        if (hasStrongLow)  return false;

        bool hasWeakHigh = m_PinDrives.Values.Any(d => d.Output == null && d.Pull == PullDirection.Up);
        bool hasWeakLow  = m_PinDrives.Values.Any(d => d.Output == null && d.Pull == PullDirection.Down);

        if (hasWeakHigh && hasWeakLow)
            throw new InvalidOperationException(
                $"Pull conflict on wire '{Name}': opposing weak drivers with no strong driver.");

        if (hasWeakHigh) return true;
        if (hasWeakLow)  return false;

        return null; // floating
    }

    private record PinDrive(bool? Output, PullDirection Pull);
}

public record SignalChange(TimeSpan Time, bool? NewState, bool? OldState);
