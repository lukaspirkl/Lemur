using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lemur.ExternalPeripherals;
using Lemur.UI.Peripherals;
using System.Collections.ObjectModel;

namespace Lemur.UI;

public class GpioInterfaceViewModelForPreviewer : GpioInterfaceViewModel
{
    public GpioInterfaceViewModelForPreviewer() : base(null, null) { }
}

public class GpioInterfaceViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "GPIO Interface";

    public ObservableCollection<GpioPinViewModel> Pins { get; } = new();

    public LogicAnalyzerViewModel? LogicAnalyzer { get; }

    public GpioInterfaceViewModel(RP2350Emulator? emulator, LogicAnalyzer? logicAnalyzer)
    {
        if (emulator == null) return;

        for (int i = 0; i < 48; i++)
            Pins.Add(new GpioPinViewModel(emulator.GetPin(i)));

        if (logicAnalyzer != null)
            LogicAnalyzer = new LogicAnalyzerViewModel(logicAnalyzer, Pins);
    }
}

public partial class GpioPinViewModel : ObservableObject
{
    private readonly IGpioPin m_Pin;

    public string Name { get; }
    public int PinNumber => m_Pin.Number;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    private bool m_Value;

    public string ValueText => Value ? "High" : "Low";

    [ObservableProperty]
    private bool m_DriveHigh;

    [ObservableProperty]
    private bool m_IsCapture;

    partial void OnDriveHighChanged(bool value) => m_Pin.Drive(value);

    public GpioPinViewModel(IGpioPin pin)
    {
        m_Pin = pin;
        Name = $"GPIO{pin.Number}";
        Value = pin.Value;
        pin.Changed += change => Dispatcher.UIThread.Post(() => Value = change.Value);
    }
}
