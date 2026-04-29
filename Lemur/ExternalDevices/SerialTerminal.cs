using Lemur.Peripherals.Uart;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lemur.ExternalDevices;

/// <summary>
/// External serial terminal hard-wired to the emulator's GPIO pins:
///   Pin 0 — emulator UART0 TX  →  terminal RX  (decoded by UartRxGpioFunction)
///   Pin 1 — terminal TX        →  emulator UART0 RX  (serialised by UartTxGpioFunction)
/// </summary>
public sealed class SerialTerminal : BackgroundService
{
    private readonly UartTxGpioFunction m_TxFunction;
    private readonly UartRxGpioFunction m_RxFunction;
    private readonly IElapsedTime m_ElapsedTime;

    public event Action<byte>? DataReceived;

    public int BaudRate
    {
        get => m_TxFunction.BaudRate;
        set
        {
            m_TxFunction.BaudRate = value;
            m_RxFunction.BaudRate = value;
        }
    }

    public SerialTerminal(RP2350Emulator emulator, ILogger<UartRxGpioFunction> rxLogger, IElapsedTime elapsedTime)
    {
        m_TxFunction = new UartTxGpioFunction();
        m_RxFunction = new UartRxGpioFunction(rxLogger, "SerialTerminal");

        var pin0 = emulator.GetPin(0); // emulator UART0 TX → terminal RX
        var pin1 = emulator.GetPin(1); // terminal TX → emulator UART0 RX

        pin0.Changed += data => m_RxFunction.OnInput(data.Time, data.NewState);

        m_TxFunction.OutputChanged += data => pin1.Drive(this, data.Time, ToLineState(data.NewValue));

        // Establish idle-HIGH on pin 1 immediately (UartTxGpioFunction already set Output=true in its ctor).
        pin1.Drive(this, TimeSpan.Zero, ToLineState(m_TxFunction.Output));

        m_RxFunction.DataReceived += () =>
        {
            while (m_RxFunction.TryRead(out var b))
            {
                DataReceived?.Invoke(b);
            }
        };
        m_ElapsedTime = elapsedTime;
    }

    private static SignalLine.LineState ToLineState(bool? value) => value switch
    {
        true  => SignalLine.LineState.Up,
        false => SignalLine.LineState.Down,
        null  => SignalLine.LineState.HiZ,
    };

    public void Transmit(byte b) => m_TxFunction.Transmit(b);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            m_TxFunction.Tick(m_ElapsedTime.Now);
            await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
        }
    }
}
