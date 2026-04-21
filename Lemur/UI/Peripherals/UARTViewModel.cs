using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;
using Lemur.UI.Terminal;
using Lemur.Peripherals;
using System.Collections.Generic;
using System.Linq;

namespace Lemur.UI.Peripherals;

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

    public TerminalEmulator Emulator { get; } = new();

    public UARTViewModel(IEnumerable<IAddressableResource> resources)
    {
        var uart = resources.OfType<UART0>().FirstOrDefault();
        if (uart != null)
        {
            uart.ReceivedData += c =>
                Emulator.Feed(Encoding.Latin1.GetBytes([c]));

            m_Uart = uart;
        }
    }

    private readonly UART0? m_Uart;

    public void Transmit(byte[] bytes)
    {
        if (m_Uart == null) return;
        foreach (var b in bytes)
            m_Uart.EnqueueRxByte(b);
    }
}
