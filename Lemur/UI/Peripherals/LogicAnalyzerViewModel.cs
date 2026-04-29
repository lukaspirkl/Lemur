using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lemur.ExternalDevices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lemur.UI;

public partial class LogicAnalyzerViewModel : ObservableObject
{
    private readonly LogicAnalyzer m_LogicAnalyzer;
    private readonly IReadOnlyList<GpioPinViewModel> m_Pins;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool m_IsRecording;

    [ObservableProperty]
    private string m_OutputFile = "capture.vcd";

    public string StatusText => IsRecording ? "Recording..." : "Idle";

    public LogicAnalyzerViewModel(LogicAnalyzer logicAnalyzer, IReadOnlyList<GpioPinViewModel> pins)
    {
        m_LogicAnalyzer = logicAnalyzer;
        m_Pins = pins;
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        var selectedPins = m_Pins.Where(p => p.IsCapture).Select(p => p.PinNumber).ToList();
        if (selectedPins.Count == 0) return;
        m_LogicAnalyzer.StartRecording(selectedPins, OutputFile);
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
