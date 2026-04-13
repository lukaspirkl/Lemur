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
        Buttons.Add(new ButtonViewModel("User Button", [15], null));
        Switches.Add(new SwitchViewModel("Mode Select", [14], null));
        OtherPins.Add(new UnrecognizedPinViewModel([0, 1], "UART TX/RX"));
    }
}

public partial class PinDevicesViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "Devices";

    private readonly UserBankIO? m_UserBankIO;

    public ObservableCollection<LedViewModel>    Leds      { get; } = [];
    public ObservableCollection<ButtonViewModel> Buttons   { get; } = [];
    public ObservableCollection<SwitchViewModel> Switches  { get; } = [];
    public ObservableCollection<UnrecognizedPinViewModel> OtherPins { get; } = [];

    public bool HasLeds      => Leds.Count > 0;
    public bool HasButtons   => Buttons.Count > 0;
    public bool HasSwitches  => Switches.Count > 0;
    public bool HasOtherPins => OtherPins.Count > 0;
    public bool HasAnyPins   => HasLeds || HasButtons || HasSwitches || HasOtherPins;

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
            OnPropertyChanged(nameof(HasButtons));
            OnPropertyChanged(nameof(HasSwitches));
            OnPropertyChanged(nameof(HasOtherPins));
            OnPropertyChanged(nameof(HasAnyPins));
        }

        Leds.CollectionChanged     += NotifyCollectionDerived;
        Buttons.CollectionChanged  += NotifyCollectionDerived;
        Switches.CollectionChanged += NotifyCollectionDerived;
        OtherPins.CollectionChanged += NotifyCollectionDerived;

        if (binaryInfoService != null)
            binaryInfoService.MetadataChanged += info =>
                Dispatcher.UIThread.Post(() => Apply(info));
    }

    private void Apply(BinaryInfo? info)
    {
        Leds.Clear();
        Buttons.Clear();
        Switches.Clear();
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
                    var (color, activeLow) = ParseColorOption(option);
                    var ledLine = m_UserBankIO.GetGpioLine(pin.Pins[0]);
                    Leds.Add(new LedViewModel(displayName, pin.Pins, ledLine, color, activeLow));
                    break;

                case "BUTTON":
                    var btnLine = m_UserBankIO.GetManualInputLine(pin.Pins[0]);
                    Buttons.Add(new ButtonViewModel(displayName, pin.Pins, btnLine));
                    break;

                case "SWITCH":
                    var swLine = m_UserBankIO.GetManualInputLine(pin.Pins[0]);
                    Switches.Add(new SwitchViewModel(displayName, pin.Pins, swLine));
                    break;

                default:
                    OtherPins.Add(new UnrecognizedPinViewModel(pin.Pins, pin.Label));
                    break;
            }
        }
    }

    /// <summary>
    /// Parses an LED option string (the part inside parentheses, e.g. "red" or "red-" or "-").
    /// Returns the LED color and whether the LED is active-low.
    /// Active-low is indicated by a trailing '-' (e.g. "[LED(-)]" or "[LED(red-)]").
    /// </summary>
    private static (Color color, bool activeLow) ParseColorOption(string? option)
    {
        if (option == null) return (Color.FromRgb(0x22, 0xDD, 0x55), false);
        option = option.Trim();

        bool activeLow = option.EndsWith('-');
        var colorPart = activeLow ? option[..^1].TrimEnd() : option;

        return (ParseColorName(colorPart), activeLow);
    }

    private static Color ParseColorName(string? name)
    {
        if (string.IsNullOrEmpty(name)) return Color.FromRgb(0x22, 0xDD, 0x55);
        if (name.StartsWith('#') && Color.TryParse(name, out var hex)) return hex;
        return s_NamedColors.TryGetValue(name, out var named) ? named : Color.FromRgb(0x22, 0xDD, 0x55);
    }

    internal static string FormatPinLabel(IReadOnlyList<int> pins) =>
        pins.Count == 1 ? $"GPIO{pins[0]}" : $"GPIO[{string.Join(", ", pins)}]";
}

public partial class LedViewModel : ObservableObject
{
    public string Label    { get; }
    public string PinLabel { get; }

    private readonly Color m_LedColor;
    private readonly bool  m_ActiveLow;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LedBrush))]
    private bool m_IsOn;

    public IBrush LedBrush => IsOn
        ? new SolidColorBrush(m_LedColor)
        : new SolidColorBrush(Color.FromRgb(
            (byte)(m_LedColor.R >> 3),
            (byte)(m_LedColor.G >> 3),
            (byte)(m_LedColor.B >> 3)));

    public LedViewModel(string label, IReadOnlyList<int> pins, IGpioLine? line, Color ledColor, bool activeLow = false)
    {
        Label      = label;
        PinLabel   = PinDevicesViewModel.FormatPinLabel(pins);
        m_LedColor = ledColor;
        m_ActiveLow = activeLow;

        if (line != null)
        {
            IsOn = IsOnFromValue(line.Value);
            line.Changed += v => Dispatcher.UIThread.Post(() => IsOn = IsOnFromValue(v));
        }
    }

    private bool IsOnFromValue(GpioValue v) =>
        m_ActiveLow ? v == GpioValue.Low : v == GpioValue.High;
}

public partial class ButtonViewModel : ObservableObject
{
    public string Label    { get; }
    public string PinLabel { get; }

    private readonly ManualGpioLine? m_Line;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PressLabel))]
    private bool m_IsPressed;

    public string PressLabel => IsPressed ? "Release" : "Press";

    partial void OnIsPressedChanged(bool value) =>
        m_Line?.Set(value ? GpioValue.Low : GpioValue.High); // active-low

    public ButtonViewModel(string label, IReadOnlyList<int> pins, ManualGpioLine? line)
    {
        Label    = label;
        PinLabel = PinDevicesViewModel.FormatPinLabel(pins);
        m_Line   = line;
        line?.Set(GpioValue.High); // default: released (active-low)
    }
}

public partial class SwitchViewModel : ObservableObject
{
    public string Label    { get; }
    public string PinLabel { get; }

    private readonly ManualGpioLine? m_Line;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleLabel))]
    private bool m_IsOn;

    public string ToggleLabel => IsOn ? "ON" : "OFF";

    partial void OnIsOnChanged(bool value) =>
        m_Line?.Set(value ? GpioValue.High : GpioValue.HiZ); // active-high: ON drives High, OFF floats

    public SwitchViewModel(string label, IReadOnlyList<int> pins, ManualGpioLine? line)
    {
        Label    = label;
        PinLabel = PinDevicesViewModel.FormatPinLabel(pins);
        m_Line   = line;
        // Leave line at HiZ (default) — reads as LOW in GPIO_IN, matching "not asserted" state.
    }
}

public class UnrecognizedPinViewModel(IReadOnlyList<int> pins, string label)
{
    public string PinLabel { get; } = PinDevicesViewModel.FormatPinLabel(pins);
    public string Label    { get; } = label;
}
