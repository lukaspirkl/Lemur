using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lemur.UI;

public partial class LogicAnalyzerTabView : UserControl
{
    public LogicAnalyzerTabView()
    {
        InitializeComponent();
    }

    private async void OnBrowseOutputFile(object sender, RoutedEventArgs e)
    {
        var vm = (LogicAnalyzerTabViewModel)DataContext!;
        await vm.BrowseOutputFileAsync(TopLevel.GetTopLevel(this)!);
    }
}
