using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lemur.ExternalPeripherals;

public sealed class LogicAnalyzer : BackgroundService
{
    private readonly RP2350Emulator m_Emulator;

    private string m_OutputFile = string.Empty;
    private List<int> m_PinNumbers = [];
    private List<Action<PinChange>> m_Handlers = [];
    private List<(int pin, bool value, long nanoseconds)> m_Events = [];
    private Dictionary<int, bool> m_InitialValues = [];
    private readonly object m_Lock = new();
    private Stopwatch m_Stopwatch = new();

    public bool IsRecording { get; private set; }
    public event Action? RecordingChanged;

    public LogicAnalyzer(RP2350Emulator emulator)
    {
        m_Emulator = emulator;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public void StartRecording(IEnumerable<int> pins, string outputFile)
    {
        if (IsRecording) return;

        m_OutputFile = outputFile;
        m_PinNumbers = [.. pins];
        m_Handlers = [];
        m_InitialValues = [];

        lock (m_Lock)
            m_Events = [];

        foreach (var pinNumber in m_PinNumbers)
        {
            var pin = m_Emulator.GetPin(pinNumber);
            m_InitialValues[pinNumber] = pin.Value;

            var captured = pinNumber;
            Action<PinChange> handler = change =>
            {
                long ns = m_Stopwatch.Elapsed.Ticks; // Ticks are 100 ns each
                lock (m_Lock)
                    m_Events.Add((captured, change.Value, ns));
            };
            pin.Changed += handler;
            m_Handlers.Add(handler);
        }

        m_Stopwatch = Stopwatch.StartNew();
        IsRecording = true;
        RecordingChanged?.Invoke();
    }

    public void StopRecording()
    {
        if (!IsRecording) return;

        m_Stopwatch.Stop();

        for (int i = 0; i < m_PinNumbers.Count; i++)
            m_Emulator.GetPin(m_PinNumbers[i]).Changed -= m_Handlers[i];

        IsRecording = false;
        RecordingChanged?.Invoke();

        List<(int pin, bool value, long nanoseconds)> snapshot;
        lock (m_Lock)
            snapshot = [.. m_Events];

        WriteVcd(snapshot);
    }

    private void WriteVcd(List<(int pin, bool value, long nanoseconds)> events)
    {
        events.Sort((a, b) => a.nanoseconds.CompareTo(b.nanoseconds));

        using var writer = new StreamWriter(m_OutputFile);

        writer.WriteLine("$timescale 100ns $end");
        writer.WriteLine("$scope module logic_analyzer $end");

        for (int i = 0; i < m_PinNumbers.Count; i++)
        {
            char id = (char)('!' + i);
            writer.WriteLine($"$var wire 1 {id} GPIO{m_PinNumbers[i]} $end");
        }

        writer.WriteLine("$upscope $end");
        writer.WriteLine("$enddefinitions $end");
        writer.WriteLine("#0");
        writer.WriteLine("$dumpvars");

        for (int i = 0; i < m_PinNumbers.Count; i++)
        {
            char id = (char)('!' + i);
            bool initial = m_InitialValues.GetValueOrDefault(m_PinNumbers[i]);
            writer.WriteLine($"{(initial ? '1' : '0')}{id}");
        }

        writer.WriteLine("$end");

        long currentNs = -1;
        foreach (var (pin, value, nanoseconds) in events)
        {
            if (nanoseconds != currentNs)
            {
                writer.WriteLine($"#{nanoseconds}");
                currentNs = nanoseconds;
            }

            int idx = m_PinNumbers.IndexOf(pin);
            if (idx < 0) continue;

            writer.Write(value ? '1' : '0');
            writer.WriteLine((char)('!' + idx));
        }
    }
}
