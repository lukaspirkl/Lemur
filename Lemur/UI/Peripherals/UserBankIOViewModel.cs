using CommunityToolkit.Mvvm.ComponentModel;
using Lemur.Peripherals;
using Lemur.UI.Peripherals;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lemur.UI;

public class UserBankIOViewModelForPreviewer : UserBankIOViewModel
{
    public UserBankIOViewModelForPreviewer()
        : base(Enumerable.Empty<IAddressableResource>())
    {
    }
}

public class UserBankIOViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "User Bank IO";

    public ObservableCollection<MuxedGpioLineViewModel> GpioLines { get; } = new();

    public UserBankIOViewModel(IEnumerable<IAddressableResource> resources)
    {
        var bank = resources.OfType<UserBankIO>().FirstOrDefault();
        if (bank == null)
        {
            return;
        }

        for (int i = 0; i < GpioLine.COUNT; i++)
        {
            if (bank.GetGpioLine(i) is MuxedGpioLine muxedGpioLine)
            {
                GpioLines.Add(new MuxedGpioLineViewModel(i, muxedGpioLine));
            }
        }
    }
}

public partial class MuxedGpioLineViewModel : ObservableObject
{
    private readonly IGpioLine m_Line;

    [ObservableProperty]
    private string m_Name;

    [ObservableProperty]
    private string m_SelectedFunctionName;

    [ObservableProperty]
    private GpioValue m_Value;

    public MuxedGpioLineViewModel(int index, MuxedGpioLine line)
    {
        m_Line = line;
        Name = $"GPIO{index}";

        Value = line.Value;
        line.Changed += x => Value = x;
        
        SelectedFunctionName = line.Name;
        line.FunctionChanged += () => SelectedFunctionName = line.Name;
    }
}
