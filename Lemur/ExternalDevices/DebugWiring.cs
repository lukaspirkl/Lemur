using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemur.ExternalDevices;

// Wires the emulator's GPIO pins to the serial terminal for the default debug configuration:
//   GPIO 0 — UART0 TX  (chip → SerialTerminal RX)
//   GPIO 1 — UART0 RX  (SerialTerminal TX → chip)
//
// Registered as IHostedService before SerialTerminal so the Wires exist before
// SerialTerminal.ExecuteAsync drives the initial idle state.
public class DebugWiring : IHostedService
{
    public List<Switch> Switches { get; set; }

    private Switch m_Pin5Switch = new Switch("Pin 5 Switch");

    public DebugWiring(RP2350Emulator emulator, SerialTerminal terminal, LogicAnalyzer logicAnalyzer)
    {
        logicAnalyzer.AddPin(emulator.GetPin(0));
        logicAnalyzer.AddPin(emulator.GetPin(1));

        logicAnalyzer.AddPin(terminal.RxPin);
        logicAnalyzer.AddPin(terminal.TxPin);

        var gpio0 = new Wire();
        emulator.GetPin(0).Connect(gpio0, TimeSpan.Zero);
        terminal.RxPin.Connect(gpio0, TimeSpan.Zero);

        var gpio1 = new Wire();
        emulator.GetPin(1).Connect(gpio1, TimeSpan.Zero);
        terminal.TxPin.Connect(gpio1, TimeSpan.Zero);

        Switches = new List<Switch>
        {
            m_Pin5Switch,
        };

        var gpio5 = new Wire();
        emulator.GetPin(5).Connect(gpio5, TimeSpan.Zero);
        m_Pin5Switch.Pin.Connect(gpio5, TimeSpan.Zero);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
