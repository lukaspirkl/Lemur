using System;

namespace Lemur.Peripherals.Sio;

public class SioGpioFunction : GpioFunctionBase
{
    public bool InputState { get; private set; }

    public void Apply(TimeSpan time, bool? driven) => SetOutput(time, driven);

    public override void OnInput(TimeSpan time, bool value)
    {
        InputState = value;
    }
}
