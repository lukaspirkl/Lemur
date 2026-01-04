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
    private readonly IDebuggable m_System;

    public IEnumerable<IPeripheralTab> Tabs { get; }

    public MainWindowViewModel(IEnumerable<IPeripheralTab> tabs, IDebuggable system)
    {
        Tabs = tabs;
        m_System = system;
    }

    [RelayCommand]
    private void Run()
    {
        m_System.Run();
    }

    [RelayCommand]
    private void Stop()
    {
        m_System.Stop();
    }

    [RelayCommand]
    private void Reset()
    {
        m_System.Reset();
    }
}
