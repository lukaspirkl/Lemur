using Avalonia.Controls;
using Avalonia.Input;

namespace Lemur.UI;

public partial class DevicesTabView : UserControl
{
    public DevicesTabView()
    {
        InitializeComponent();
    }

    private void OnButtonPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is InputElement el && el.DataContext is ButtonViewModel vm)
        {
            e.Pointer.Capture(el);
            el.Opacity = 0.6;
            vm.Press();
        }
    }

    private void OnButtonPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is InputElement el && el.DataContext is ButtonViewModel vm)
        {
            e.Pointer.Capture(null);
            el.Opacity = 1.0;
            vm.Release();
        }
    }

    private void OnButtonPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (sender is InputElement el && el.DataContext is ButtonViewModel vm)
        {
            el.Opacity = 1.0;
            vm.Release();
        }
    }
}
