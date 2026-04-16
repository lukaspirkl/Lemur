using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Venture;
using Venture.Processor;

namespace VentureUI;

public class CsrViewModelForPreviewer : CsrViewModel
{
    public CsrViewModelForPreviewer() : base(null!, null!) { }
}

public partial class CsrViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "CSRs";

    public ObservableCollection<CsrGroupViewModel> Groups { get; } = new();

    public CsrViewModel(Hazard3Processor processor, IDebuggable system)
    {
        if (processor == null) return; // previewer path

        var csr = processor.CSR;

        foreach (var groupDef in csr.GetGroups())
            Groups.Add(new CsrGroupViewModel(groupDef, csr));

        system.Stopped += () =>
        {
            foreach (var group in Groups)
                group.RefreshAll();
        };
    }

    [RelayCommand]
    private void Refresh()
    {
        foreach (var group in Groups)
            group.RefreshAll();
    }
}
