using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lemur.ExternalDevices;
using Lemur.UI.Peripherals;
using Lemur.UI.Terminal;
using System.Collections.ObjectModel;

namespace Lemur.UI;

public class DevicesTabViewModelForPreviewer : DevicesTabViewModel
{
    public DevicesTabViewModelForPreviewer() : base(null!) { }
}

public class DevicesTabViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "Devices";

    public ObservableCollection<SwitchViewModel>       Switches  { get; } = new();
    public ObservableCollection<LedViewModel>          Leds      { get; } = new();
    public ObservableCollection<ButtonViewModel>       Buttons   { get; } = new();
    public ObservableCollection<PinTerminalViewModel>  Terminals { get; } = new();

    private readonly BinaryInfoWiring? m_Wiring;

    public DevicesTabViewModel(BinaryInfoWiring? wiring)
    {
        m_Wiring = wiring;
        if (wiring == null) return;

        wiring.DevicesChanged += () => Dispatcher.UIThread.Post(RefreshDevices);
        RefreshDevices();
    }

    private void RefreshDevices()
    {
        Switches.Clear();
        foreach (var sw in m_Wiring!.Switches)
            Switches.Add(new SwitchViewModel(sw));

        Leds.Clear();
        foreach (var led in m_Wiring.Leds)
            Leds.Add(new LedViewModel(led));

        Buttons.Clear();
        foreach (var btn in m_Wiring.Buttons)
            Buttons.Add(new ButtonViewModel(btn));

        Terminals.Clear();
        foreach (var t in m_Wiring.Terminals)
            Terminals.Add(new PinTerminalViewModel(t));
    }
}

public partial class SwitchViewModel : ObservableObject
{
    private readonly Switch m_Switch;

    public string Name => m_Switch.Name;

    [ObservableProperty]
    private bool m_IsOn;

    partial void OnIsOnChanged(bool value) => m_Switch.Value = value;

    public SwitchViewModel(Switch sw)
    {
        m_Switch = sw;
        m_IsOn   = sw.Value;
    }
}

public partial class LedViewModel : ObservableObject
{
    private readonly Led    m_Led;
    private readonly IBrush m_ActiveBrush;

    public string Name        => m_Led.Name;
    public IBrush StrokeBrush => m_ActiveBrush;
    public IBrush FillBrush   => IsOn ? m_ActiveBrush : Brushes.Transparent;

    [ObservableProperty]
    private bool m_IsOn;

    partial void OnIsOnChanged(bool value) => OnPropertyChanged(nameof(FillBrush));

    public LedViewModel(Led led)
    {
        m_Led         = led;
        m_ActiveBrush = ParseBrush(led.Color);
        m_IsOn        = led.Value;

        led.ValueChanged += () => Dispatcher.UIThread.Post(() => IsOn = m_Led.Value);
    }

    private static IBrush ParseBrush(string color)
    {
        try   { return new SolidColorBrush(Color.Parse(color)); }
        catch { return new SolidColorBrush(Color.Parse("#00CC44")); }
    }
}

public class ButtonViewModel
{
    private readonly Button m_Button;

    public string Name => m_Button.Name;

    public ButtonViewModel(Button button) => m_Button = button;

    public void Press()   => m_Button.Press();
    public void Release() => m_Button.Release();
}

public partial class PinTerminalViewModel
{
    private readonly PinTerminal m_Terminal;
    private PinTerminalWindow?   m_Window;

    public string          Name     => m_Terminal.Name;
    public string          Title    => $"{m_Terminal.Name} - Serial Terminal";
    public TerminalEmulator Emulator { get; } = new();

    public PinTerminalViewModel(PinTerminal terminal)
    {
        m_Terminal = terminal;
        terminal.DataReceived += b => Emulator.Feed([b]);
    }

    public void Transmit(byte[] bytes)
    {
        foreach (var b in bytes)
            m_Terminal.Transmit(b);
    }

    [RelayCommand]
    private void OpenTerminal()
    {
        if (m_Window != null)
        {
            m_Window.Activate();
            return;
        }
        m_Window = new PinTerminalWindow { DataContext = this };
        m_Window.Closed += (_, _) => m_Window = null;
        m_Window.Show();
    }
}
