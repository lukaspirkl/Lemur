using System;

namespace Lemur.ExternalDevices;

public class Led
{
    public Pin    Pin   { get; }
    public string Name  { get; }
    public string Color { get; }

    public bool Value { get; private set; }

    public event Action? ValueChanged;

    public Led(string name, string color = "#00CC44")
    {
        Name  = name;
        Color = color;
        Pin   = new Pin(name);
        Pin.Changed += OnPinChanged;
    }

    private void OnPinChanged(SignalChange change)
    {
        var newValue = change.NewState == true;
        if (newValue == Value) return;
        Value = newValue;
        ValueChanged?.Invoke();
    }
}
