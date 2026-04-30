using Lemur.Peripherals.Uart;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lemur.ExternalDevices;

public sealed class SerialTerminal : BackgroundService
{
    private readonly UartTxGpioFunction m_TxFunction;
    private readonly UartRxGpioFunction m_RxFunction;
    private readonly IElapsedTime m_ElapsedTime;

    // Pin the chip drives toward the terminal (e.g. emulator UART0 TX → terminal RX).
    public Pin RxPin { get; } = new Pin("SerialTerminal - RX");

    // Pin the terminal drives toward the chip (e.g. terminal TX → emulator UART0 RX).
    public Pin TxPin { get; } = new Pin("SerialTerminal - TX");

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

    public SerialTerminal(ILogger<UartRxGpioFunction> rxLogger, ILogger<UartTxGpioFunction> txLogger, IElapsedTime elapsedTime)
    {
        //TxPin.SetPull(PullDirection.Up, elapsedTime.Now);

        m_TxFunction = new UartTxGpioFunction(txLogger);
        m_RxFunction = new UartRxGpioFunction(rxLogger, "SerialTerminal");
        m_ElapsedTime = elapsedTime;

        RxPin.Changed += data => m_RxFunction.OnInput(data.Time, data.NewState ?? false);

        m_TxFunction.OutputChanged += data => TxPin.SetOutput(data.NewValue, data.Time);

        m_RxFunction.DataReceived += () =>
        {
            while (m_RxFunction.TryRead(out var b))
            {
                DataReceived?.Invoke(b);
            }
        };
    }

    public void Transmit(byte b) => m_TxFunction.Transmit(b);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            m_TxFunction.Tick(m_ElapsedTime.Now);
            await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
        }
    }
}
