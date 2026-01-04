using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Venture;
using Venture.Peripherals;

namespace VentureUI;

public class PinsViewModelForPreviewer : PinsViewModel
{
    public PinsViewModelForPreviewer()
        : base(Enumerable.Empty<IAddressableResource>())
    {
    }
}

public class PinsViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "GPIO";

    public ObservableCollection<GpioSourceViewModel> GpioSources { get; } = new();

    public PinsViewModel(IEnumerable<IAddressableResource> resources)
    {
        foreach (var source in resources.OfType<IGpioSource>())
        {
            GpioSources.Add(new GpioSourceViewModel(source));
        }
    }
}

public partial class GpioSourceViewModel : ObservableObject
{
    [ObservableProperty]
    private string m_Name;

    public ObservableCollection<GpioLineViewModel> GpioLines { get; } = new();

    public GpioSourceViewModel(IGpioSource source)
    {
        Name = source.GetType().Name;

        for (int i = 0; i < GpioLine.Count; i++)
        {
            GpioLines.Add(new GpioLineViewModel(i, source.GetGpioLine(i)));
        }
    }
}

public partial class GpioLineViewModel : ObservableObject
{
    private readonly IGpioLine m_Line;

    [ObservableProperty]
    private string m_Name;

    [ObservableProperty]
    private GpioValue m_Value;

    public GpioLineViewModel(int index, IGpioLine line)
    {
        m_Line = line;
        Name = $"GPIO{index}";
        Value = line.Value;
        line.Changed += x => Value = x;
    }
}
