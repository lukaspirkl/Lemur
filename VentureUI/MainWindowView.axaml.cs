using Avalonia.Controls;

namespace VentureUI;

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