using System.Collections.ObjectModel;
using Venture.Processor;

namespace VentureUI;

public class CsrGroupViewModel
{
    public string Name { get; }
    public ObservableCollection<CsrEntryViewModel> Registers { get; } = new();

    public CsrGroupViewModel(CsrDefinitions.GroupDef def, CSR csr)
    {
        Name = def.Name;
        foreach (var csrDef in def.Registers)
            Registers.Add(new CsrEntryViewModel(csrDef, csr));
    }

    public void RefreshAll()
    {
        foreach (var reg in Registers)
            reg.RefreshFromCsr();
    }
}
