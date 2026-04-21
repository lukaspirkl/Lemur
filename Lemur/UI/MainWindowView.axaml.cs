using Avalonia.Controls;

namespace Lemur.UI;

public partial class MainWindowView : Window
{
    public MainWindowView()
    {
        InitializeComponent();
    }

    public override void Show()
    {
        base.Show();
        if (DataContext is MainWindowViewModel vm)
        {
            vm.LoadCommand.Execute(null);
        }
    }

}