using Avalonia.Controls;
using Avalonia.Interactivity;
using VentureUI.Terminal;

namespace VentureUI;

public partial class UARTView : UserControl
{
    public UARTView()
    {
        InitializeComponent();

        var terminal = this.Find<TerminalView>("m_Terminal")!;
        terminal.Input += bytes =>
        {
            if (DataContext is UARTViewModel vm)
                vm.Transmit(bytes);
        };
    }

    private void Clear(object? sender, RoutedEventArgs e)
    {
        if (DataContext is UARTViewModel vm)
            vm.Emulator.Clear();
    }
}
