using CommunityToolkit.Mvvm.ComponentModel;

namespace VentureUI;

public partial class CsrFieldViewModel : ObservableObject
{
    private readonly CsrDefinitions.FieldDef m_Def;

    public string Name  => m_Def.Name;
    public string Bits  => m_Def.Bits;

    [ObservableProperty] private uint   m_Value;
    [ObservableProperty] private string m_ValueHex = string.Empty;
    [ObservableProperty] private string m_Interpretation = string.Empty;

    public CsrFieldViewModel(CsrDefinitions.FieldDef def)
    {
        m_Def = def;
    }

    public void Refresh(uint rawRegisterValue)
    {
        uint mask  = m_Def.Width == 32 ? uint.MaxValue : (1u << m_Def.Width) - 1u;
        uint value = (rawRegisterValue >> m_Def.Lsb) & mask;
        Value          = value;
        ValueHex       = value.ToString("X");
        Interpretation = m_Def.Interpret?.Invoke(value) ?? string.Empty;
    }
}
