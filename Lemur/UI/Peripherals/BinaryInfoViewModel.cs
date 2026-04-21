using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lemur.UI.Peripherals;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lemur.UI;

public class BinaryInfoViewModelForPreviewer : BinaryInfoViewModel
{
    public BinaryInfoViewModelForPreviewer() : base(null)
    {
        HasMetadata = true;
        ProgramName = "blink_simple";
        ProgramVersion = "1.0.0";
        BuildDate = "Apr 10 2026";
        BuildType = "Release";
        TargetBoard = "pico2";
        ProgramUrl = "https://github.com/raspberrypi/pico-examples";
        Pins.Add(new PinInfoViewModel([25], "LED"));
        Pins.Add(new PinInfoViewModel([0, 1], "UART TX/RX"));
    }
}

public partial class BinaryInfoViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "Binary Info";

    [ObservableProperty] private bool m_HasMetadata;
    [ObservableProperty] private string? m_ProgramName;
    [ObservableProperty] private string? m_ProgramVersion;
    [ObservableProperty] private string? m_ProgramUrl;
    [ObservableProperty] private string? m_BuildDate;
    [ObservableProperty] private string? m_BuildType;
    [ObservableProperty] private string? m_TargetBoard;
    [ObservableProperty] private string? m_Boot2Name;

    public ObservableCollection<PinInfoViewModel> Pins { get; } = [];

    public BinaryInfoViewModel(BinaryInfoService? service)
    {
        if (service != null)
            service.MetadataChanged += info => Dispatcher.UIThread.Post(() => Apply(info));
    }

    private void Apply(BinaryInfo? info)
    {
        HasMetadata    = info != null;
        ProgramName    = info?.ProgramName;
        ProgramVersion = info?.ProgramVersion;
        ProgramUrl     = info?.ProgramUrl;
        BuildDate      = info?.BuildDate;
        BuildType      = info?.BuildType;
        TargetBoard    = info?.TargetBoard;
        Boot2Name      = info?.Boot2Name;

        Pins.Clear();
        if (info != null)
            foreach (var pin in info.Pins)
                Pins.Add(new PinInfoViewModel(pin.Pins, pin.Label));
    }
}

public class PinInfoViewModel(IReadOnlyList<int> pins, string label)
{
    public string Pins  { get; } = pins.Count == 1
        ? $"GPIO{pins[0]}"
        : $"GPIO[{string.Join(", ", pins)}]";

    public string Label { get; } = label;
}
