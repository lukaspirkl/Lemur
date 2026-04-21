using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lemur.Csr;
using Lemur.UI.Peripherals;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lemur.UI;

public class CsrTabViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "CSR";

    public ObservableCollection<CsrGroupViewModel> Groups { get; } = new();

    private CsrGroupViewModel? m_SelectedGroup;
    public CsrGroupViewModel? SelectedGroup
    {
        get => m_SelectedGroup;
        set
        {
            m_SelectedGroup?.Unsubscribe();
            SetProperty(ref m_SelectedGroup, value);
            m_SelectedGroup?.Subscribe();
        }
    }

    public CsrTabViewModel(CsrController csr)
    {
        foreach (var group in csr.AllEntries.GroupBy(e => e.Group).OrderBy(g => g.Key))
            Groups.Add(new CsrGroupViewModel(group.Key, group));

        SelectedGroup = Groups.FirstOrDefault();
    }
}

public class CsrGroupViewModel
{
    public string GroupName { get; }
    public ObservableCollection<CsrEntryViewModel> Entries { get; } = new();

    public CsrGroupViewModel(string name, IEnumerable<CsrEntry> entries)
    {
        GroupName = name;
        foreach (var entry in entries.OrderBy(e => e.Address))
            Entries.Add(new CsrEntryViewModel(entry));
    }

    public void Subscribe()
    {
        foreach (var vm in Entries)
            vm.Subscribe();
    }

    public void Unsubscribe()
    {
        foreach (var vm in Entries)
            vm.Unsubscribe();
    }
}

public partial class CsrEntryViewModel : ObservableObject
{
    private readonly CsrEntry m_Entry;

    public string Name    => m_Entry.Name;
    public string Address => $"0x{m_Entry.Address:X3}";

    [ObservableProperty]
    private string m_Value = "0x00000000";

    public ObservableCollection<CsrFieldViewModel> Fields { get; } = new();

    public CsrEntryViewModel(CsrEntry entry)
    {
        m_Entry = entry;
        foreach (var f in entry.GetValues())
            Fields.Add(new CsrFieldViewModel(f));
        Refresh();
    }

    public void Subscribe()
    {
        m_Entry.Changed += OnChanged;
        Refresh();
    }

    public void Unsubscribe()
    {
        m_Entry.Changed -= OnChanged;
    }

    private void OnChanged() => Dispatcher.UIThread.Post(Refresh);

    private void Refresh()
    {
        Value = $"0x{m_Entry.Peek():X8}";

        var fieldValues = m_Entry.GetValues().ToDictionary(f => f.Name);
        foreach (var field in Fields)
            if (fieldValues.TryGetValue(field.Name, out var ev))
                field.Refresh(ev.Value);
    }
}

public partial class CsrFieldViewModel : ObservableObject
{
    public string Name        { get; }
    public string Bits        { get; }
    public string Description { get; }

    [ObservableProperty]
    private string m_Value = "0x0";

    public CsrFieldViewModel(EntryValue ev)
    {
        Name        = ev.Name;
        Bits        = ev.Bits;
        Description = ev.Description;
        Refresh(ev.Value);
    }

    public void Refresh(uint raw) => Value = $"0x{raw:X}";
}
