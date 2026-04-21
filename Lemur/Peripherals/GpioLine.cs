using System;

namespace Lemur.Peripherals;

public interface IGpioLine
{
    GpioValue Value { get; }
    event Action<GpioValue>? Changed;
}

public enum GpioValue
{
    HiZ,
    High,
    Low,
}

public interface IGpioSource
{
    IGpioLine GetGpioLine(int index);
}

/// <summary>
/// A software-driven GPIO line that can be set by external code (e.g., a simulated button).
/// Used as the "external input" overlay in <see cref="MuxedGpioLine"/>.
/// </summary>
public class ManualGpioLine : IGpioLine
{
    private GpioValue m_Value = GpioValue.HiZ;
    public GpioValue Value => m_Value;
    public event Action<GpioValue>? Changed;

    public void Set(GpioValue v)
    {
        if (m_Value == v) return;
        m_Value = v;
        Changed?.Invoke(v);
    }
}

public class GpioLine : IGpioLine
{
    public const int COUNT = 48;

    public GpioValue Value
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                Changed?.Invoke(value);
            }
        }
    }

    public event Action<GpioValue>? Changed;
}