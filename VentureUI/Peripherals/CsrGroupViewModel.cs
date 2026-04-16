using System.Collections.ObjectModel;
using Venture.Processor;

namespace VentureUI;

public class CsrGroupViewModel
{
    public string Name { get; }
    public ObservableCollection<CsrEntryViewModel> Registers { get; } = new();

    public CsrGroupViewModel(CsrGroupDef def, CSR csr)
    {
        Name = def.Name;
        foreach (var entry in def.Registers)
            Registers.Add(new CsrEntryViewModel(entry, csr));
    }

    public void RefreshAll()
    {
        foreach (var reg in Registers)
            reg.RefreshFromEntry();
    }
}
