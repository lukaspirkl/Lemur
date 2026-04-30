using System;

namespace Lemur.ExternalDevices;

public class Switch
{
    private bool m_Value;

    public Pin Pin { get; }

    public string Name { get; }

    public Switch(string name = "Switch")
    {
        Name = name;
        Pin  = new Pin(name);
    }

    public bool Value
    {
        get => m_Value;
        set
        {
            m_Value = value;
            Pin.SetOutput(value, TimeSpan.Zero);
        }
    }
}
