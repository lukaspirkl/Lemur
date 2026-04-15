using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using Venture.Processor;

namespace VentureUI;

/// <summary>
/// Represents one CSR register row in the viewer.
/// Subscribes to <see cref="CSR.Changed"/> so it updates immediately when the CPU writes the register.
/// </summary>
public partial class CsrEntryViewModel : ObservableObject
{
    private readonly CsrDefinitions.CsrDef m_Def;
    private readonly CSR m_Csr;

    public string  Name    => m_Def.Name;
    public string  Address => $"0x{m_Def.Address:X3}";
    public bool    IsWindowed => m_Def.IsWindowed;

    [ObservableProperty] private uint   m_RawValue;
    [ObservableProperty] private string m_RawValueHex = "0x00000000";

    public ObservableCollection<CsrFieldViewModel> Fields { get; } = new();

    // Window data — populated on demand via FetchAllWindowsCommand
    [ObservableProperty] private bool m_HasWindowData;
    public ObservableCollection<WindowRowViewModel> WindowRows { get; } = new();

    public CsrEntryViewModel(CsrDefinitions.CsrDef def, CSR csr)
    {
        m_Def = def;
        m_Csr = csr;

        foreach (var field in def.Fields)
            Fields.Add(new CsrFieldViewModel(field));

        // Subscribe to live changes from the CPU
        csr.Changed += OnCsrChanged;

        // Read initial value
        RefreshFromCsr();
    }

    private void OnCsrChanged(ushort address, uint value)
    {
        if (address != m_Def.Address) return;
        Dispatcher.UIThread.Post(() =>
        {
            ApplyValue(value);
        });
    }

    public void RefreshFromCsr()
    {
        ApplyValue(m_Csr.Peek(m_Def.Address));
    }

    private void ApplyValue(uint value)
    {
        RawValue    = value;
        RawValueHex = $"0x{value:X8}";
        foreach (var field in Fields)
            field.Refresh(value);
    }

    [RelayCommand]
    private void FetchAllWindows()
    {
        // Read all 32 windows (16 bits each = 512 interrupt bits total).
        // We write the index into the CSR, then read the window bits back via Peek.
        // This uses the software CSR path (Set) so the index register updates properly.
        WindowRows.Clear();

        int windowCount = m_Def.Address == 0xBE3 ? 128 : 32; // meipra has 4-bit priorities → more entries

        for (ushort i = 0; i < windowCount; i++)
        {
            // Write index into bits [4:0] (or [6:0] for meipra)
            m_Csr.Set(m_Def.Address, (uint)i);
            uint window = (m_Csr.Peek(m_Def.Address) >> 16) & 0xFFFF;
            WindowRows.Add(new WindowRowViewModel(i, window, m_Def.Address));
        }

        // Restore original index
        m_Csr.Set(m_Def.Address, RawValue & 0xFFFF); // restore lower bits (index)

        HasWindowData = true;
    }
}

/// <summary>One row in the windowed IRQ data table.</summary>
public class WindowRowViewModel
{
    public ushort Index  { get; }
    public string Label  { get; }
    public string Value  { get; }
    public string Bits   { get; }

    public WindowRowViewModel(ushort index, uint window, ushort csrAddress)
    {
        Index = index;

        if (csrAddress == 0xBE3) // meipra — four 4-bit priorities per window
        {
            // index here represents a 4-entry group; base IRQ = index * 4
            int baseIrq = index * 4;
            Label = $"IRQ {baseIrq}–{baseIrq + 3} priority";
            var p = new uint[4];
            for (int j = 0; j < 4; j++)
                p[j] = (window >> (j * 4)) & 0xF;
            Value = $"{p[0]}, {p[1]}, {p[2]}, {p[3]}";
            Bits  = $"0x{window:X4}";
        }
        else
        {
            // meiea/meipa/meifa — 16 IRQ bits per window
            int baseIrq = index * 16;
            Label = $"IRQ {baseIrq}–{baseIrq + 15}";
            Value = Convert.ToString(window, 2).PadLeft(16, '0');
            Bits  = $"0x{window:X4}";
        }
    }
}
