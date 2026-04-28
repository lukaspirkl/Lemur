using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lemur.ExternalPeripherals;
using Lemur.UI.Peripherals;
using System.Collections.ObjectModel;

namespace Lemur.UI;

public class GpioInterfaceViewModelForPreviewer : GpioInterfaceViewModel
{
    public GpioInterfaceViewModelForPreviewer() : base(null, null, new NullElapsedTime()) { }
}

public class GpioInterfaceViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "GPIO Interface";

    public ObservableCollection<GpioPinViewModel> Pins { get; } = new();

    public LogicAnalyzerViewModel? LogicAnalyzer { get; }

    public GpioInterfaceViewModel(RP2350Emulator? emulator, LogicAnalyzer? logicAnalyzer, IElapsedTime elapsedTime)
    {
        if (emulator == null) return;

        for (int i = 0; i < 48; i++)
            Pins.Add(new GpioPinViewModel(i, emulator.GetPin(i), elapsedTime));

        if (logicAnalyzer != null)
            LogicAnalyzer = new LogicAnalyzerViewModel(logicAnalyzer, Pins);
    }
}

public partial class GpioPinViewModel : ObservableObject
{
    private readonly SignalLine m_Pin;
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

    [ObservableProperty]
    private bool m_IsCapture;

    partial void OnDriveHighChanged(bool value) => m_Pin.Drive(this, m_ElapsedTime.Now, value ? SignalLine.LineState.Up : SignalLine.LineState.Down);

    public GpioPinViewModel(int number, SignalLine pin, IElapsedTime elapsedTime)
    {
        m_Pin = pin;
        m_ElapsedTime = elapsedTime;
        m_PinNumber = number;
        Name = $"GPIO{number}";
        Value = pin.State;
        pin.Changed += data =>
        {
            if (data.NewState != data.OldState)
            {
                Dispatcher.UIThread.Post(() => Value = data.NewState);
            }
        };
    }
}
