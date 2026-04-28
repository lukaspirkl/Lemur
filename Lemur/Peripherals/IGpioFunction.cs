using System;

namespace Lemur.Peripherals;

// A peripheral's logical connection to one GPIO pin.
//
// Data flow (per pin):
//
//   Peripheral ──OutputChanged──▶ IoUserBank ──▶ PadControlUserBank ──▶ SignalLine
//        ▲                                                                 │
//        └──────────────OnInput──── IoUserBank ◀── PadControlUserBank ◀────┘
//
// The peripheral raises OutputChanged whenever Output or OutputEnable change,
// so the mux can re-evaluate what to drive onto the pad.
public interface IGpioFunction
{
    // null = HiZ (output not driven); true/false = actively driven high/low.
    bool? Output { get; }

    event Action<GpioFunctionOutput>? OutputChanged;

    void OnInput(TimeSpan time, bool value);
}

public record GpioFunctionOutput(TimeSpan Time, bool? NewValue, bool? OldValue);

public abstract class GpioFunctionBase : IGpioFunction
{
    public bool? Output { get; private set; }

    public event Action<GpioFunctionOutput>? OutputChanged;
    public abstract void OnInput(TimeSpan time, bool value);

    protected void SetOutput(TimeSpan time, bool? newValue)
    {
        var oldValue = Output;
        Output = newValue;
        OutputChanged?.Invoke(new GpioFunctionOutput(time, newValue, oldValue));
    }
}

/// <summary>A function that always drives a fixed output level. Useful for idle/default states.</summary>
public class StaticGpioFunction : GpioFunctionBase
{
    public StaticGpioFunction(bool high) => SetOutput(TimeSpan.Zero, high);
    public override void OnInput(TimeSpan time, bool value) { }
}
