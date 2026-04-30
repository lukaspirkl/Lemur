using Lemur.Peripherals.Uart;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;

namespace Lemur.ExternalDevices;

// Self-contained serial terminal wired to a specific GPIO pin pair.
// Unlike SerialTerminal this is not an IHostedService — it manages its own
// ticker via a Timer and is created on demand by BinaryInfoWiring.
public sealed class PinTerminal : IDisposable
{
    private readonly UartTxGpioFunction m_TxFunction;
    private readonly UartRxGpioFunction m_RxFunction;
    private readonly Timer m_Ticker;

    public string Name { get; }

    // The GPIO pin that carries chip→terminal data (chip UART TX line).
    public Pin RxPin { get; }

    // The GPIO pin that carries terminal→chip data (chip UART RX line).
    public Pin TxPin { get; }

    public event Action<byte>? DataReceived;

    public PinTerminal(string name, IElapsedTime elapsedTime)
    {
        Name = name;

        RxPin = new Pin($"{name} - RX");
        TxPin = new Pin($"{name} - TX");

        m_TxFunction = new UartTxGpioFunction(NullLogger<UartTxGpioFunction>.Instance, name);
        m_RxFunction = new UartRxGpioFunction(NullLogger<UartRxGpioFunction>.Instance, name);

        RxPin.Changed += data => m_RxFunction.OnInput(data.Time, data.NewState ?? false);
        m_TxFunction.OutputChanged += data => TxPin.SetOutput(data.NewValue, data.Time);

        m_RxFunction.DataReceived += () =>
        {
            while (m_RxFunction.TryRead(out var b))
                DataReceived?.Invoke(b);
        };

        m_Ticker = new Timer(
            _ => m_TxFunction.Tick(elapsedTime.Now),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(100));
    }

    public void Transmit(byte b) => m_TxFunction.Transmit(b);

    public void Dispose()
    {
        m_Ticker.Dispose();
        RxPin.Disconnect(TimeSpan.Zero);
        TxPin.Disconnect(TimeSpan.Zero);
    }
}
