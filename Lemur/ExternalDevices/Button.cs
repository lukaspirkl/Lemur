using System;

namespace Lemur.ExternalDevices;

// Momentary push-button: drives the wire high while held, low when released.
public class Button
{
    public Pin  Pin  { get; }
    public string Name { get; }

    public Button(string name)
    {
        Name = name;
        Pin  = new Pin(name);
        Pin.SetOutput(false, TimeSpan.Zero);
    }

    public void Press()   => Pin.SetOutput(true,  TimeSpan.Zero);
    public void Release() => Pin.SetOutput(false, TimeSpan.Zero);
}
