using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using Venture;

namespace VentureUI;

public class MainWindowViewModelForPreviewer : MainWindowViewModel
{
    public MainWindowViewModelForPreviewer()
        : base(Enumerable.Empty<IPeripheralTab>(), new NullDebuggable())
    {
    }
}

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDebuggable system;

    public IEnumerable<IPeripheralTab> Tabs { get; }

    public MainWindowViewModel(IEnumerable<IPeripheralTab> tabs, IDebuggable system)
    {
        Tabs = tabs;
        this.system = system;
    }

    [RelayCommand]
    private void Run()
    {
        system.Run();
    }

    [RelayCommand]
    private void Stop()
    {
        system.Stop();
    }

    [RelayCommand]
    private void Reset()
    {
        system.Reset();
    }
}
