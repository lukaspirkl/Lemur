using System;
using System.Collections.Generic;
using System.IO;
using static Lemur.SignalLine;

namespace Lemur.ExternalDevices;

public sealed class LogicAnalyzer
{
    private readonly RP2350Emulator m_Emulator;
    
    private string m_OutputFile = string.Empty;
    private List<int> m_PinNumbers = [];
    private List<Action<ChangeData>> m_Handlers = [];
    private List<(int pin, bool value, TimeSpan time)> m_Events = [];
    private Dictionary<int, bool> m_InitialValues = [];
    private readonly object m_Lock = new();

    public bool IsRecording { get; private set; }
    public event Action? RecordingChanged;

    public LogicAnalyzer(RP2350Emulator emulator)
    {
        m_Emulator = emulator;
    }

    public void StartRecording(IEnumerable<int> pins, string outputFile)
    {
        if (IsRecording) return;

        m_OutputFile = outputFile;
        m_PinNumbers = [.. pins];
        m_Handlers = [];
        m_InitialValues = [];

        lock (m_Lock)
        {
            m_Events = [];
        }

        foreach (var pinNumber in m_PinNumbers)
        {
            var pin = m_Emulator.GetPin(pinNumber);
            m_InitialValues[pinNumber] = pin.State;

            var captured = pinNumber;
            Action<ChangeData> handler = data =>
            {
                if (data.OldState != data.NewState)
                {
                    lock (m_Lock)
                    {
                        m_Events.Add((captured, data.NewState, data.Time));
                    }
                }
            };
            pin.Changed += handler;
            m_Handlers.Add(handler);
        }

        IsRecording = true;
        RecordingChanged?.Invoke();
    }

    public void StopRecording()
    {
        if (!IsRecording) return;

        for (int i = 0; i < m_PinNumbers.Count; i++)
            m_Emulator.GetPin(m_PinNumbers[i]).Changed -= m_Handlers[i];

        IsRecording = false;
        RecordingChanged?.Invoke();

        List<(int pin, bool value, TimeSpan time)> snapshot;
        lock (m_Lock)
        {
            snapshot = [.. m_Events];
        }

        WriteVcd(snapshot);
    }

    private void WriteVcd(List<(int pin, bool value, TimeSpan time)> events)
    {
        events.Sort((a, b) => a.time.CompareTo(b.time));

        using var writer = new StreamWriter(m_OutputFile);

        writer.WriteLine("$timescale 100ns $end"); // One tick is 100ns
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

        TimeSpan currentTime = TimeSpan.Zero;
        foreach (var (pin, value, time) in events)
        {
            if (time != currentTime)
            {
                writer.WriteLine($"#{time.Ticks}");
                currentTime = time;
            }

            int idx = m_PinNumbers.IndexOf(pin);
            if (idx < 0) continue;

            writer.Write(value ? '1' : '0');
            writer.WriteLine((char)('!' + idx));
        }
    }
}
