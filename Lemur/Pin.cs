using System;

namespace Lemur;

public enum PullDirection { None, Up, Down }

public class Pin
{
    private bool?         m_Output;
    private PullDirection m_Pull;
    private Wire?         m_Wire;

    private readonly Action<SignalChange> m_WireChangedHandler;

    public string        Name   { get; }
    public bool?         Output => m_Output;
    public PullDirection Pull   => m_Pull;
    public Wire?         Wire   => m_Wire;

    // When connected: wire state. When disconnected: local resolution of output+pull.
    public bool? State => m_Wire?.State ?? ComputeLocalState();

    public event Action<SignalChange>? Changed;

    public Pin(string name)
    {
        Name = name;
        m_WireChangedHandler = change => Changed?.Invoke(change);
    }

    public void Connect(Wire wire, TimeSpan time)
    {
        if (wire == m_Wire) return;

        var oldState = State;

        if (m_Wire != null)
        {
            // Unsubscribe before removing so we don't receive the change triggered on the old wire.
            m_Wire.Changed -= m_WireChangedHandler;
            m_Wire.RemovePinDrive(this, time);
        }

        m_Wire = wire;
        // Push our drive before subscribing so the Wire.Changed from UpdatePinDrive goes to
        // already-connected pins, not us. We fire our own Changed below.
        m_Wire.UpdatePinDrive(this, time, m_Output, m_Pull);
        m_Wire.Changed += m_WireChangedHandler;

        var newState = State;
        if (newState != oldState)
            Changed?.Invoke(new SignalChange(time, newState, oldState));
    }

    public void Disconnect(TimeSpan time)
    {
        if (m_Wire == null) return;

        var oldState = State;

        m_Wire.Changed -= m_WireChangedHandler;
        m_Wire.RemovePinDrive(this, time);
        m_Wire = null;

        var newState = State;
        if (newState != oldState)
            Changed?.Invoke(new SignalChange(time, newState, oldState));
    }

    public void SetOutput(bool? value, TimeSpan time)
    {
        var oldState = State;
        m_Output = value;

        if (m_Wire != null)
            m_Wire.UpdatePinDrive(this, time, m_Output, m_Pull);
        else
            Changed?.Invoke(new SignalChange(time, State, oldState));
    }

    public void SetPull(PullDirection direction, TimeSpan time)
    {
        var oldState = State;
        m_Pull = direction;

        if (m_Wire != null)
            m_Wire.UpdatePinDrive(this, time, m_Output, m_Pull);
        else if (State != oldState)
            Changed?.Invoke(new SignalChange(time, State, oldState));
    }

    private bool? ComputeLocalState() => m_Output ?? m_Pull switch
    {
        PullDirection.Up   => true,
        PullDirection.Down => false,
        _                  => (bool?)null,
    };
}
