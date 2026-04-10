using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Venture;

namespace VentureUI;

public class MainWindowViewModelForPreviewer : MainWindowViewModel
{
    public MainWindowViewModelForPreviewer()
        : base(Enumerable.Empty<IPeripheralTab>(), new NullDebuggable(), null)
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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    private bool m_IsRunning = false;

    [ObservableProperty]
    private string m_ProgramLabel = "No binary loaded";

    public MainWindowViewModel(IEnumerable<IPeripheralTab> tabs, IDebuggable system, BinaryInfoService? binaryInfoService)
    {
        Tabs = tabs;
        m_System = system;

        m_System.EBreak += () => Task.Run(() =>
        {
            // TODO: Find some better way how to control execution
            m_System.Stop();
            m_System.Reset();
        });
        m_System.Stopped += () => Dispatcher.UIThread.Post(() => IsRunning = false);

        if (binaryInfoService != null)
            binaryInfoService.MetadataChanged += info =>
                Dispatcher.UIThread.Post(() => ProgramLabel = FormatProgramLabel(info));
    }

    private static string FormatProgramLabel(BinaryInfo? info)
    {
        if (info == null)
            return "Unknown";

        var name = info.ProgramName ?? "Unknown";
        return info.ProgramVersion != null ? $"{name}  {info.ProgramVersion}" : name;
    }

    private bool CanRun() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanRun))]
    private void Run()
    {
        m_System.Run();
        IsRunning = true;
    }

    private bool CanStop() => IsRunning;

    [RelayCommand(CanExecute = nameof(CanStop))]
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

    private bool CanLoad(string path) => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private void LoadExample(string path)
    {
        if (!File.Exists(path))
            return;

        var loaded = File.ReadAllBytes(path);
        m_System.MemoryWrite(0x10000000, loaded);
        m_System.Reset();
        // BinaryInfoService picks up the flash write automatically via XIP.Written.
    }
}

public class ExampleViewModel
{
    public required string Name { get; init; }

    public required string Path { get; init; }
}
