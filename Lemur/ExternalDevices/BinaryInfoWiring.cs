using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lemur.ExternalDevices;

public class BinaryInfoWiring
{
    private readonly RP2350Emulator m_Emulator;
    private readonly IElapsedTime   m_ElapsedTime;

    private List<Switch>      m_Switches  = [];
    private List<Led>         m_Leds      = [];
    private List<Button>      m_Buttons   = [];
    private List<PinTerminal> m_Terminals = [];

    public IReadOnlyList<Switch>      Switches  => m_Switches;
    public IReadOnlyList<Led>         Leds      => m_Leds;
    public IReadOnlyList<Button>      Buttons   => m_Buttons;
    public IReadOnlyList<PinTerminal> Terminals => m_Terminals;

    public event Action? DevicesChanged;

    public BinaryInfoWiring(RP2350Emulator emulator, BinaryInfoService binaryInfoService, IElapsedTime elapsedTime)
    {
        m_Emulator    = emulator;
        m_ElapsedTime = elapsedTime;
        binaryInfoService.MetadataChanged += OnMetadataChanged;
    }

    private void OnMetadataChanged(BinaryInfo? info)
    {
        foreach (var sw  in m_Switches)  sw.Pin.Disconnect(TimeSpan.Zero);
        foreach (var led in m_Leds)      led.Pin.Disconnect(TimeSpan.Zero);
        foreach (var btn in m_Buttons)   btn.Pin.Disconnect(TimeSpan.Zero);
        foreach (var t   in m_Terminals) t.Dispose();

        m_Switches  = [];
        m_Leds      = [];
        m_Buttons   = [];
        m_Terminals = [];

        if (info?.Pins != null)
        {
            // Collect terminal-pin candidates keyed by description.
            var rxCandidates = new Dictionary<string, int>();
            var txCandidates = new Dictionary<string, int>();

            foreach (var pinInfo in info.Pins)
            {
                if (pinInfo.Pins.Count == 0) continue;

                var (name, type, arg) = ParseLabel(pinInfo.Label);
                if (type == null) continue;

                var gpioIndex = pinInfo.Pins[0];

                switch (type)
                {
                    case "SWITCH":
                    {
                        var wire = NewWire(gpioIndex);
                        var sw   = new Switch(name);
                        sw.Pin.Connect(wire, TimeSpan.Zero);
                        m_Switches.Add(sw);
                        break;
                    }

                    case "LED":
                    {
                        var color = string.IsNullOrWhiteSpace(arg) ? "#00CC44" : arg;
                        var wire  = NewWire(gpioIndex);
                        var led   = new Led(name, color);
                        led.Pin.Connect(wire, TimeSpan.Zero);
                        m_Leds.Add(led);
                        break;
                    }

                    case "BUTTON":
                    {
                        var wire = NewWire(gpioIndex);
                        var btn  = new Button(name);
                        btn.Pin.Connect(wire, TimeSpan.Zero);
                        m_Buttons.Add(btn);
                        break;
                    }

                    case "SERIALTERMINAL-RX":
                        rxCandidates[name] = gpioIndex;
                        break;

                    case "SERIALTERMINAL-TX":
                        txCandidates[name] = gpioIndex;
                        break;
                }
            }

            // Pair matched RX + TX entries into PinTerminals.
            foreach (var (name, rxIndex) in rxCandidates)
            {
                if (!txCandidates.TryGetValue(name, out var txIndex)) continue;

                var terminal = new PinTerminal(name, m_ElapsedTime);

                var rxWire = NewWire(rxIndex);
                terminal.RxPin.Connect(rxWire, TimeSpan.Zero);

                var txWire = NewWire(txIndex);
                terminal.TxPin.Connect(txWire, TimeSpan.Zero);

                m_Terminals.Add(terminal);
            }
        }

        DevicesChanged?.Invoke();
    }

    private Wire NewWire(int gpioIndex)
    {
        var wire = new Wire();
        m_Emulator.GetPin(gpioIndex).Connect(wire, TimeSpan.Zero);
        return wire;
    }

    // Matches: "Name [TYPE]" or "Name [TYPE(arg)]"
    // TYPE may contain letters, digits, underscores, or hyphens.
    private static readonly Regex s_LabelPattern =
        new(@"^(.*?)\s*\[([\w-]+)(?:\(([^)]*)\))?\]\s*$", RegexOptions.Compiled);

    private static (string name, string? type, string? arg) ParseLabel(string label)
    {
        var match = s_LabelPattern.Match(label);
        if (!match.Success) return (label, null, null);
        return (
            match.Groups[1].Value.Trim(),
            match.Groups[2].Value.ToUpperInvariant(),
            match.Groups[3].Success ? match.Groups[3].Value.Trim() : null
        );
    }
}
