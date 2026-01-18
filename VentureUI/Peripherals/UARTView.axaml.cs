using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
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
            m_Output.Text = vm.Output.ToString();
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
        Dispatcher.UIThread.Post(() => m_Output.AppendText(data.ToString()));
    }

    private void Clear(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is UARTViewModel vm)
        {
            vm.Output.Clear();
        }
        m_Output.Clear();
    }
}
