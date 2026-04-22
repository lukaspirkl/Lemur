using System;

namespace Lemur;

public record PinChange(bool Value, ulong StepIndex);

public interface IGpioPin
{
    int Number { get; }
    bool Value { get; }
    void Drive(bool value);

    event Action<PinChange> Changed;
}
