using Avalonia.Controls;
using Avalonia.Interactivity;
using Lemur.UI.Terminal;

namespace Lemur.UI;

public partial class PinTerminalWindow : Window
{
    public PinTerminalWindow()
    {
        InitializeComponent();

        var terminal = this.Find<TerminalView>("m_Terminal")!;
        terminal.Input += bytes =>
        {
            if (DataContext is PinTerminalViewModel vm)
                vm.Transmit(bytes);
        };
    }

    private void Clear(object? sender, RoutedEventArgs e)
    {
        if (DataContext is PinTerminalViewModel vm)
            vm.Emulator.Clear();
    }
}
