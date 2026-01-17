using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Venture;

namespace VentureUI;

public class MainWindowViewModelForPreviewer : MainWindowViewModel
{
    public MainWindowViewModelForPreviewer()
        : base(Enumerable.Empty<IPeripheralTab>(), new NullDebuggable())
    {
    }

    protected override void Load()
    {
        Examples.Add(new ExampleViewModel { Name = "One", Path = string.Empty });
        Examples.Add(new ExampleViewModel { Name = "Two", Path = string.Empty });
    }
}

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDebuggable m_System;

    public IEnumerable<IPeripheralTab> Tabs { get; }

    public ObservableCollection<ExampleViewModel> Examples { get; } = new();

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

    [RelayCommand]
    protected virtual void Load()
    {
        var appDirectory = Path.GetDirectoryName(Environment.ProcessPath);
        if (appDirectory != null)
        {
            var path = Path.Combine(appDirectory, "Examples");
            foreach (var item in Directory.EnumerateFiles(path))
            {
                Examples.Add(new ExampleViewModel
                {
                    Name = Path.GetFileNameWithoutExtension(item).Replace('_', ' '),
                    Path = item,
                });
            }
        }
    }

    [RelayCommand]
    private void LoadExample(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var loaded = File.ReadAllBytes(path);
        m_System.MemoryWrite(0x10000000, loaded);
        m_System.Reset();
    }
}

public class ExampleViewModel
{
    public required string Name { get; init; }

    public required string Path { get; init; }
}