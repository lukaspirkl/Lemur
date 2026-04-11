using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Venture;
using Venture.Peripherals;

namespace VentureUI;

public class PinDevicesViewModelForPreviewer : PinDevicesViewModel
{
    public PinDevicesViewModelForPreviewer()
        : base(null, Enumerable.Empty<IAddressableResource>())
    {
        Leds.Add(new LedViewModel("Status", [25], null, Colors.Orange));
        Leds.Add(new LedViewModel("Power", [26], null, Color.FromRgb(0x22, 0xDD, 0x55)) { IsOn = true });
        OtherPins.Add(new UnrecognizedPinViewModel([0, 1], "UART TX/RX"));
    }
}

public partial class PinDevicesViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "Devices";

    private readonly UserBankIO? m_UserBankIO;

    public ObservableCollection<LedViewModel> Leds { get; } = [];
    public ObservableCollection<UnrecognizedPinViewModel> OtherPins { get; } = [];

    public bool HasLeds => Leds.Count > 0;
    public bool HasOtherPins => OtherPins.Count > 0;
    public bool HasAnyPins => HasLeds || HasOtherPins;

    // Matches:  Some Label [KEYWORD]  or  Some Label [KEYWORD(option)]
    private static readonly Regex s_DeviceTag =
        new(@"^(.*?)\s*\[([A-Za-z]+)(?:\(([^)]*)\))?\]\s*$", RegexOptions.Compiled);

    private static readonly IReadOnlyDictionary<string, Color> s_NamedColors =
        new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        {
            ["red"]     = Colors.OrangeRed,
            ["green"]   = Color.FromRgb(0x22, 0xDD, 0x55),
            ["blue"]    = Colors.DodgerBlue,
            ["yellow"]  = Colors.Yellow,
            ["orange"]  = Colors.Orange,
            ["white"]   = Colors.White,
            ["cyan"]    = Colors.Cyan,
            ["magenta"] = Colors.Magenta,
            ["pink"]    = Colors.HotPink,
            ["purple"]  = Colors.MediumPurple,
        };

    public PinDevicesViewModel(BinaryInfoService? binaryInfoService, IEnumerable<IAddressableResource> resources)
    {
        m_UserBankIO = resources.OfType<UserBankIO>().FirstOrDefault();

        void NotifyCollectionDerived(object? s, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasLeds));
            OnPropertyChanged(nameof(HasOtherPins));
            OnPropertyChanged(nameof(HasAnyPins));
        }

        Leds.CollectionChanged += NotifyCollectionDerived;
        OtherPins.CollectionChanged += NotifyCollectionDerived;

        if (binaryInfoService != null)
            binaryInfoService.MetadataChanged += info =>
                Dispatcher.UIThread.Post(() => Apply(info));
    }

    private void Apply(BinaryInfo? info)
    {
        Leds.Clear();
        OtherPins.Clear();
        if (info == null || m_UserBankIO == null) return;

        foreach (var pin in info.Pins)
        {
            var match = s_DeviceTag.Match(pin.Label);
            if (!match.Success)
            {
                OtherPins.Add(new UnrecognizedPinViewModel(pin.Pins, pin.Label));
                continue;
            }

            var displayName = match.Groups[1].Value.Trim();
            if (string.IsNullOrEmpty(displayName))
                displayName = FormatPinLabel(pin.Pins);

            var keyword = match.Groups[2].Value;
            var option  = match.Groups[3].Success ? match.Groups[3].Value : null;

            switch (keyword.ToUpperInvariant())
            {
                case "LED":
                    var color = ParseColor(option);
                    var line  = m_UserBankIO.GetGpioLine(pin.Pins[0]);
                    Leds.Add(new LedViewModel(displayName, pin.Pins, line, color));
                    break;

                // TODO: BUTTON, SWITCH — see TODO-PinDevices.md
                default:
                    OtherPins.Add(new UnrecognizedPinViewModel(pin.Pins, pin.Label));
                    break;
            }
        }
    }

    private static Color ParseColor(string? name)
    {
        if (name == null) return Color.FromRgb(0x22, 0xDD, 0x55);
        name = name.Trim();
        if (name.StartsWith('#') && Color.TryParse(name, out var hex)) return hex;
        return s_NamedColors.TryGetValue(name, out var named) ? named : Color.FromRgb(0x22, 0xDD, 0x55);
    }

    internal static string FormatPinLabel(IReadOnlyList<int> pins) =>
        pins.Count == 1 ? $"GPIO{pins[0]}" : $"GPIO[{string.Join(", ", pins)}]";
}

public partial class LedViewModel : ObservableObject
{
    public string Label   { get; }
    public string PinLabel { get; }

    private readonly Color m_LedColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LedBrush))]
    private bool m_IsOn;

    public IBrush LedBrush => IsOn
        ? new SolidColorBrush(m_LedColor)
        : new SolidColorBrush(Color.FromRgb(
            (byte)(m_LedColor.R >> 3),
            (byte)(m_LedColor.G >> 3),
            (byte)(m_LedColor.B >> 3)));

    public LedViewModel(string label, IReadOnlyList<int> pins, IGpioLine? line, Color ledColor)
    {
        Label    = label;
        PinLabel = PinDevicesViewModel.FormatPinLabel(pins);
        m_LedColor = ledColor;

        if (line != null)
        {
            IsOn = line.Value == GpioValue.High;
            line.Changed += v => Dispatcher.UIThread.Post(() => IsOn = v == GpioValue.High);
        }
    }
}

public class UnrecognizedPinViewModel(IReadOnlyList<int> pins, string label)
{
    public string PinLabel { get; } = PinDevicesViewModel.FormatPinLabel(pins);
    public string Label    { get; } = label;
}
