using Avalonia.Controls;
using Avalonia.Interactivity;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;

namespace VentureUI;

public partial class PinDevicesView : UserControl
{
    public PinDevicesView()
    {
        InitializeComponent();
    }

    private void OnHelpClick(object? sender, RoutedEventArgs e)
    {
        Drawer.ShowCustom<PinDevicesHelpView, PinDevicesHelpViewModel>(
            new PinDevicesHelpViewModel(),
            options: new DrawerOptions
            {
                Position         = Position.Right,
                Title            = "Pin Device Naming",
                IsCloseButtonVisible = true,
                CanLightDismiss  = true,
                MinWidth         = 380,
            });
    }
}
