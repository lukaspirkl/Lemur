using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lemur.UI.Peripherals;
using System.Collections.ObjectModel;

namespace Lemur.UI;

public class GpioInterfaceViewModelForPreviewer : GpioInterfaceViewModel
{
    public GpioInterfaceViewModelForPreviewer() : base(null, new NullElapsedTime()) { }
}

public class GpioInterfaceViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "GPIO Interface";

    public ObservableCollection<GpioPinViewModel> Pins { get; } = new();

    public GpioInterfaceViewModel(RP2350Emulator? emulator, IElapsedTime elapsedTime)
    {
        if (emulator == null) return;

        for (int i = 0; i < 48; i++)
            Pins.Add(new GpioPinViewModel(i, emulator.GetPin(i), elapsedTime));
    }
}

public partial class GpioPinViewModel : ObservableObject
{
    private readonly Pin m_Pin;
    private readonly IElapsedTime m_ElapsedTime;
    private readonly int m_PinNumber;

    public string Name { get; }
    public int PinNumber => m_PinNumber;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    private bool m_Value;

    public string ValueText => Value ? "High" : "Low";

    [ObservableProperty]
    private bool m_DriveHigh;

    partial void OnDriveHighChanged(bool value) => m_Pin.SetOutput(value, m_ElapsedTime.Now);

    public GpioPinViewModel(int number, Pin pin, IElapsedTime elapsedTime)
    {
        m_Pin = pin;
        m_ElapsedTime = elapsedTime;
        m_PinNumber = number;
        Name = $"GPIO{number}";
        Value = pin.State ?? false;
        pin.Changed += data =>
        {
            if (data.NewState != data.OldState)
            {
                Dispatcher.UIThread.Post(() => Value = data.NewState ?? false);
            }
        };
    }
}
