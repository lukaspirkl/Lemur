using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lemur.UI;

public partial class GpioInterfaceView : UserControl
{
    public GpioInterfaceView()
    {
        InitializeComponent();
    }

    private async void OnBrowseOutputFile(object sender, RoutedEventArgs e)
    {
        var vm = (GpioInterfaceViewModel)DataContext!;
        if (vm.LogicAnalyzer == null) return;
        await vm.LogicAnalyzer.BrowseOutputFileAsync(TopLevel.GetTopLevel(this)!);
    }
}
