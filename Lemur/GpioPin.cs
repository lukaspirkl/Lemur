using System;
using Lemur.Peripherals;

namespace Lemur;

internal sealed class GpioPin : IGpioPin
{
    private readonly UserBankIO m_Io;
    private readonly Func<ulong> m_StepIndex;

    public int Number { get; }
    public event Action<PinChange>? Changed;

    public bool Value => m_Io.GetPinValue(Number) == GpioValue.High;

    public GpioPin(int number, UserBankIO io, Func<ulong> stepIndex)
    {
        Number = number;
        m_Io = io;
        m_StepIndex = stepIndex;
    }

    public void Drive(bool value) => m_Io.DriveExternalInput(Number, value);

    internal void NotifyChanged(bool value) =>
        Changed?.Invoke(new PinChange(value, m_StepIndex()));
}
