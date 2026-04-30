using System;
using System.Collections.Generic;
using System.IO;

namespace Lemur.ExternalDevices;

public sealed class LogicAnalyzer
{
    private readonly List<Pin> m_AvailablePins = [];

    private string m_OutputFile = string.Empty;
    private List<Pin> m_RecordingPins = [];
    private List<Action<SignalChange>> m_Handlers = [];
    private List<(Pin pin, bool value, TimeSpan time)> m_Events = [];
    private Dictionary<Pin, bool> m_InitialValues = [];
    private readonly object m_Lock = new();
    private readonly IElapsedTime m_ElapsedTime;

    private TimeSpan m_StartTime;

    public bool IsRecording { get; private set; }
    public event Action? RecordingChanged;

    public LogicAnalyzer(IElapsedTime elapsedTime)
    {
        m_ElapsedTime = elapsedTime;
    }

    public void AddPin(Pin pin)
    {
        if (!m_AvailablePins.Contains(pin))
            m_AvailablePins.Add(pin);
    }

    public void RemovePin(Pin pin) => m_AvailablePins.Remove(pin);

    public void StartRecording(string outputFile)
    {
        if (IsRecording) return;

        m_StartTime = m_ElapsedTime.Now;
        m_OutputFile = outputFile;
        m_RecordingPins = [.. m_AvailablePins];
        m_Handlers = [];
        m_InitialValues = [];

        lock (m_Lock)
        {
            m_Events = [];
        }

        foreach (var pin in m_RecordingPins)
        {
            m_InitialValues[pin] = pin.State ?? false;

            Action<SignalChange> handler = data =>
            {
                if (data.OldState != data.NewState)
                {
                    lock (m_Lock)
                    {
                        m_Events.Add((pin, data.NewState ?? false, data.Time - m_StartTime));
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

        for (int i = 0; i < m_RecordingPins.Count; i++)
            m_RecordingPins[i].Changed -= m_Handlers[i];

        IsRecording = false;
        RecordingChanged?.Invoke();

        List<(Pin pin, bool value, TimeSpan time)> snapshot;
        lock (m_Lock)
        {
            snapshot = [.. m_Events];
        }

        WriteVcd(snapshot);
    }

    private void WriteVcd(List<(Pin pin, bool value, TimeSpan time)> events)
    {
        events.Sort((a, b) => a.time.CompareTo(b.time));

        using var writer = new StreamWriter(m_OutputFile);

        writer.WriteLine("$timescale 100ns $end"); // One tick is 100ns
        writer.WriteLine("$scope module logic_analyzer $end");

        for (int i = 0; i < m_RecordingPins.Count; i++)
        {
            char id = (char)('!' + i);
            writer.WriteLine($"$var wire 1 {id} {SanitizeName(m_RecordingPins[i].Name)} $end");
        }

        writer.WriteLine("$upscope $end");
        writer.WriteLine("$enddefinitions $end");
        writer.WriteLine("#0");
        writer.WriteLine("$dumpvars");

        for (int i = 0; i < m_RecordingPins.Count; i++)
        {
            char id = (char)('!' + i);
            bool initial = m_InitialValues.GetValueOrDefault(m_RecordingPins[i]);
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

            int idx = m_RecordingPins.IndexOf(pin);
            if (idx < 0) continue;

            writer.Write(value ? '1' : '0');
            writer.WriteLine((char)('!' + idx));
        }

        writer.WriteLine("$end");
    }

    private static string SanitizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "_";

        var chars = new char[name.Length];
        for (int i = 0; i < name.Length; i++)
            chars[i] = char.IsLetterOrDigit(name[i]) || name[i] == '_' ? name[i] : '_';
        return new string(chars);
    }
}
