using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Venture;
using Venture.Peripherals;
using VentureUI.Terminal;

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

    public TerminalEmulator Emulator { get; } = new();

    public UARTViewModel(IEnumerable<IAddressableResource> resources)
    {
        var uart = resources.OfType<UART0>().FirstOrDefault();
        if (uart != null)
        {
            uart.ReceivedData += c =>
                Emulator.Feed(Encoding.Latin1.GetBytes([c]));

            _uart = uart;
        }
    }

    private readonly UART0? _uart;

    public void Transmit(byte[] bytes)
    {
        if (_uart == null) return;
        foreach (var b in bytes)
            _uart.EnqueueRxByte(b);
    }
}
