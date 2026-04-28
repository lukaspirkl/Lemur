using Avalonia.Controls;
using Avalonia.Interactivity;
using Lemur.UI.Terminal;

namespace Lemur.UI.Peripherals;

public partial class SerialTerminalView : UserControl
{
    public SerialTerminalView()
    {
        InitializeComponent();

        var terminal = this.Find<TerminalView>("m_Terminal")!;
        terminal.Input += bytes =>
        {
            if (DataContext is SerialTerminalViewModel vm)
                vm.Transmit(bytes);
        };
    }

    private void Clear(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SerialTerminalViewModel vm)
            vm.Emulator.Clear();
    }
}
