using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lemur.ExternalDevices;
using Lemur.UI.Peripherals;
using System.Threading.Tasks;

namespace Lemur.UI;

public class LogicAnalyzerTabViewModelForPreviewer : LogicAnalyzerTabViewModel
{
    public LogicAnalyzerTabViewModelForPreviewer() : base(new LogicAnalyzer(new NullElapsedTime())) { }
}

public partial class LogicAnalyzerTabViewModel : ObservableObject, IPeripheralTab
{
    private readonly LogicAnalyzer m_LogicAnalyzer;

    public string TabName => "Logic Analyzer";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool m_IsRecording;

    [ObservableProperty]
    private string m_OutputFile = "capture.vcd";

    public string StatusText => IsRecording ? "Recording..." : "Idle";

    public LogicAnalyzerTabViewModel(LogicAnalyzer logicAnalyzer)
    {
        m_LogicAnalyzer = logicAnalyzer;
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        m_LogicAnalyzer.StartRecording(OutputFile);
        IsRecording = true;
    }

    private bool CanStart() => !IsRecording;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        m_LogicAnalyzer.StopRecording();
        IsRecording = false;
    }

    private bool CanStop() => IsRecording;

    public async Task BrowseOutputFileAsync(TopLevel topLevel)
    {
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save VCD capture",
            SuggestedFileName = "capture.vcd",
            FileTypeChoices = [new FilePickerFileType("VCD") { Patterns = ["*.vcd"] }]
        });

        if (file != null)
            OutputFile = file.Path.LocalPath;
    }
}
