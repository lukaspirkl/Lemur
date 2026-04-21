using CommunityToolkit.Mvvm.ComponentModel;
using Lemur.Peripherals;
using Lemur.UI.Peripherals;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lemur.UI;

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

    public ObservableCollection<PadViewModel> Pads { get; } = new();

    public PinsViewModel(IEnumerable<IAddressableResource> resources)
    {
        foreach (var source in resources.OfType<IGpioSource>())
        {
            GpioSources.Add(new GpioSourceViewModel(source));
        }

        var userBankPadControl = resources.OfType<UserBankPadControl>().FirstOrDefault();
        if (userBankPadControl != null)
        {
            int i = 0;
            foreach (var pad in userBankPadControl.Pads)
            {
                var vm = new PadViewModel
                {
                    Name = $"GPIO{i}",
                    InputEnabled = pad.InputEnable,
                    OutputEnabled = !pad.OutputDisable,
                };
                Pads.Add(vm);
                i++;
            }

            userBankPadControl.PadControlChanged += i =>
            {
                if (i < userBankPadControl.Pads.Length)
                {
                    Pads[(int)i].InputEnabled = userBankPadControl.Pads[i].InputEnable;
                    Pads[(int)i].OutputEnabled = !userBankPadControl.Pads[i].OutputDisable;
                }
            };
        }
    }
}

public partial class PadViewModel : ObservableObject
{
    [ObservableProperty]
    private string m_Name = "";

    [ObservableProperty]
    private bool m_InputEnabled = false;

    [ObservableProperty]
    private bool m_OutputEnabled = false;
}

public partial class GpioSourceViewModel : ObservableObject
{
    [ObservableProperty]
    private string m_Name;

    public ObservableCollection<GpioLineViewModel> GpioLines { get; } = new();

    public GpioSourceViewModel(IGpioSource source)
    {
        Name = source.GetType().Name;

        for (int i = 0; i < GpioLine.COUNT; i++)
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
