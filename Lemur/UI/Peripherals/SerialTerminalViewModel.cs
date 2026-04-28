using CommunityToolkit.Mvvm.ComponentModel;
using Lemur.ExternalPeripherals;
using Lemur.UI.Terminal;

namespace Lemur.UI.Peripherals;

public class SerialTerminalViewModelForPreviewer : SerialTerminalViewModel
{
    public SerialTerminalViewModelForPreviewer() : base(null!) { }
}

public class SerialTerminalViewModel : ObservableObject, IPeripheralTab
{
    public string TabName => "Serial Terminal";

    public TerminalEmulator Emulator { get; } = new();

    private readonly SerialTerminal? m_Terminal;

    public SerialTerminalViewModel(SerialTerminal terminal)
    {
        m_Terminal = terminal;
        if (terminal != null)
            terminal.DataReceived += b => Emulator.Feed([b]);
    }

    public void Transmit(byte[] bytes)
    {
        if (m_Terminal == null) return;
        foreach (var b in bytes)
            m_Terminal.Transmit(b);
    }
}
