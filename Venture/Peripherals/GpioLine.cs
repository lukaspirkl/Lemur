namespace Venture.Peripherals;

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

public class GpioLine : IGpioLine
{
    public const int Count = 48;

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