using CommunityToolkit.Mvvm.ComponentModel;
using Lemur.ExternalDevices;
using Lemur.UI.Peripherals;
using System.Collections.ObjectModel;

namespace Lemur.UI;

public class SwitchesTabViewModelForPreviewer : SwitchesTabViewModel
{
    public SwitchesTabViewModelForPreviewer() : base(null!) { }
}

public class SwitchesTabViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "Switches";

    public ObservableCollection<SwitchViewModel> Switches { get; } = new();

    public SwitchesTabViewModel(DebugWiring wiring)
    {
        if (wiring == null) return;

        foreach (var sw in wiring.Switches)
            Switches.Add(new SwitchViewModel(sw));
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
        m_IsOn = sw.Value;
    }
}
