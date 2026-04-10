using Avalonia.Controls;
using Avalonia.Interactivity;

namespace VentureUI;

public partial class UARTView : UserControl
{
    public UARTView()
    {
        InitializeComponent();
    }

    private void Clear(object? sender, RoutedEventArgs e)
    {
        if (DataContext is UARTViewModel vm)
            vm.Emulator.Clear();
    }
}
