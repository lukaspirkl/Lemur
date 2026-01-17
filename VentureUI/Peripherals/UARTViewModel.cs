using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Venture;
using Venture.Peripherals;

namespace VentureUI;

public class UARTViewModelForPreviewer : UARTViewModel
{
    public UARTViewModelForPreviewer()
        : base(Enumerable.Empty<IAddressableResource>())
    {
    }
}

public class UARTViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "UART0";

    public event Action<char>? DataReceived;

    public UARTViewModel(IEnumerable<IAddressableResource> resources)
    {
        var uart = resources.OfType<UART0>().FirstOrDefault();
        if (uart != null)
        {
            uart.ReceivedData += data => DataReceived?.Invoke(data);
        }
    }
}
