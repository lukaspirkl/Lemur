using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using Venture.Processor;

namespace VentureUI;

/// <summary>
/// Represents one CSR register row in the viewer.
/// Subscribes to <see cref="CSR.Changed"/> so it updates immediately when the CPU writes.
/// Windowed registers (<see cref="CsrWindowedEntry"/>) expose all windows at once via
/// <see cref="CsrWindowedEntry.ReadAllWindows"/> — no write-then-read loop needed.
/// </summary>
public partial class CsrEntryViewModel : ObservableObject
{
    private readonly CsrEntry m_Entry;
    private readonly CSR      m_Csr;

    public string Name    => m_Entry.Name;
    public string Address => $"0x{m_Entry.Address:X3}";
    public bool   IsWindowed => m_Entry is CsrWindowedEntry;

    [ObservableProperty] private uint   m_RawValue;
    [ObservableProperty] private string m_RawValueHex = "0x00000000";

    public ObservableCollection<CsrFieldViewModel>  Fields     { get; } = new();
    public ObservableCollection<WindowRowViewModel> WindowRows { get; } = new();

    [ObservableProperty] private bool m_HasWindowData;

    public CsrEntryViewModel(CsrEntry entry, CSR csr)
    {
        m_Entry = entry;
        m_Csr   = csr;

        foreach (var field in entry.Fields)
            Fields.Add(new CsrFieldViewModel(field));

        csr.Changed += OnCsrChanged;
        RefreshFromEntry();
    }

    private void OnCsrChanged(ushort address, uint value)
    {
        if (address != m_Entry.Address) return;
        Dispatcher.UIThread.Post(() => ApplyValue(value));
    }

    public void RefreshFromEntry()
    {
        ApplyValue(m_Entry.Read());

        if (m_Entry is CsrWindowedEntry windowed)
            LoadWindowData(windowed);
    }

    private void ApplyValue(uint value)
    {
        RawValue    = value;
        RawValueHex = $"0x{value:X8}";
        foreach (var field in Fields)
            field.Refresh(value);
    }

    private void LoadWindowData(CsrWindowedEntry windowed)
    {
        WindowRows.Clear();
        var windows = windowed.ReadAllWindows();
        for (int i = 0; i < windows.Count; i++)
            WindowRows.Add(new WindowRowViewModel((ushort)i, windows[i], m_Entry.Address));
        HasWindowData = true;
    }
}

/// <summary>One row in the windowed IRQ data table.</summary>
public class WindowRowViewModel
{
    public ushort Index { get; }
    public string Label { get; }
    public string Value { get; }
    public string Bits  { get; }

    public WindowRowViewModel(ushort index, uint window, ushort csrAddress)
    {
        Index = index;

        if (csrAddress == 0xBE3) // meipra — four 4-bit priorities per window
        {
            int baseIrq = index * 4;
            Label = $"IRQ {baseIrq}–{baseIrq + 3} priority";
            var p = new uint[4];
            for (int j = 0; j < 4; j++)
                p[j] = (window >> (j * 4)) & 0xF;
            Value = $"{p[0]}, {p[1]}, {p[2]}, {p[3]}";
            Bits  = $"0x{window:X4}";
        }
        else // meiea / meipa / meifa — 16 IRQ bits per window
        {
            int baseIrq = index * 16;
            Label = $"IRQ {baseIrq}–{baseIrq + 15}";
            Value = Convert.ToString(window, 2).PadLeft(16, '0');
            Bits  = $"0x{window:X4}";
        }
    }
}
