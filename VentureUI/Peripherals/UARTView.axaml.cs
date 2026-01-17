using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace VentureUI;

public partial class UARTView : UserControl
{
    public UARTView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is UARTViewModel vm)
        {
            vm.DataReceived += DataReceived;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is UARTViewModel vm)
        {
            vm.DataReceived -= DataReceived;
        }
    }

    private void DataReceived(char data)
    {
        Dispatcher.UIThread.Post(() => m_Output.Text += data);
    }

    private void Clear(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        m_Output.Clear();
    }
}
