using System;

namespace Lemur.Peripherals.PadControl;

// Bridges a MuxedGpioFunction to a SignalLine, owning the pad control register fields.
//
// Output path (inner → line): gated by ISO and OD.
// Input path (line → inner):  gated by IE only; ISO does not isolate input per spec §9.7.
// Pulls (PUE/PDE):            sets SignalLine.InitialState; bus keeper mode (both set) tracks
//                             the live line state so the pad retains its last driven level.
public class PadBridge
{
    private readonly IGpioFunction m_Inner;
    private readonly SignalLine    m_Line;
    private readonly IElapsedTime  m_ElapsedTime;

    private bool  m_IsolationControl     = true;
    private bool  m_OutputDisable        = false;
    private bool  m_InputEnable          = false;
    private Drive m_DriveStrength        = Drive.Drive4MA;
    private bool  m_PullUpEnable         = false;
    private bool  m_PullDownEnable       = true;
    private bool  m_EnableSchmittTrigger = true;
    private bool  m_SlewRateFast         = false;

    public enum Drive : byte
    {
        Drive2MA  = 0x0,
        Drive4MA  = 0x1,
        Drive8MA  = 0x2,
        Drive12MA = 0x3,
    }

    public PadBridge(Register32 reg, IGpioFunction inner, SignalLine line, IElapsedTime elapsedTime)
    {
        m_Inner       = inner;
        m_Line        = line;
        m_ElapsedTime = elapsedTime;

        reg.Field(8,    () => m_IsolationControl,    v => { m_IsolationControl    = v; Refresh(); })
           .Field(7,    () => m_OutputDisable,        v => { m_OutputDisable        = v; ApplyOutput(m_ElapsedTime.Now); })
           .Field(6,    () => m_InputEnable,          v =>   m_InputEnable          = v)
           .Field(4, 2, () => m_DriveStrength,        v =>   m_DriveStrength        = v)
           .Field(3,    () => m_PullUpEnable,         v => { m_PullUpEnable         = v; ApplyPulls(); })
           .Field(2,    () => m_PullDownEnable,       v => { m_PullDownEnable       = v; ApplyPulls(); })
           .Field(1,    () => m_EnableSchmittTrigger, v =>   m_EnableSchmittTrigger = v)
           .Field(0,    () => m_SlewRateFast,         v =>   m_SlewRateFast         = v);

        inner.OutputChanged += OnInnerOutputChanged;
        line.Changed        += OnLineChanged;

        Refresh();
    }

    private void Refresh()
    {
        ApplyOutput(m_ElapsedTime.Now);
        ApplyPulls();
    }

    private void OnInnerOutputChanged(GpioFunctionOutput e)
    {
        // ISO=1 latches the output — do not forward new changes until ISO is cleared.
        if (m_IsolationControl) return;
        DriveLineFromInner(e.Time, e.NewValue);
    }

    private void OnLineChanged(SignalLine.ChangeData data)
    {
        // Bus keeper: keep InitialState in sync with the line so the level is retained on release.
        // Blocked by ISO because the bus keeper logic is in the switched core domain (§9.6.1).
        if (!m_IsolationControl && m_PullUpEnable && m_PullDownEnable)
        {
            m_Line.InitialState = data.NewState
                ? SignalLine.InitialLineState.PullUp
                : SignalLine.InitialLineState.PullDown;
        }

        // IE gates the input path; ISO does not (§9.7: "input signal … is not isolated").
        if (m_InputEnable)
            m_Inner.OnInput(data.Time, data.NewState);
    }

    private void ApplyOutput(TimeSpan time)
    {
        // ISO=1: output is latched — leave whatever we last drove on the line unchanged.
        if (m_IsolationControl) return;
        DriveLineFromInner(time, m_Inner.Output);
    }

    private void DriveLineFromInner(TimeSpan time, bool? value)
    {
        var state = (m_OutputDisable || value == null)
            ? SignalLine.LineState.HiZ
            : (value.Value ? SignalLine.LineState.Up : SignalLine.LineState.Down);
        m_Line.Drive(this, time, state);
    }

    private void ApplyPulls()
    {
        // ISO=1: pull state is also latched — do not update InitialState.
        if (m_IsolationControl) return;

        // Bus keeper (PUE=PDE=1): snapshot current line state to seed the keeper direction.
        if (m_PullUpEnable && m_PullDownEnable)
        {
            m_Line.InitialState = m_Line.State
                ? SignalLine.InitialLineState.PullUp
                : SignalLine.InitialLineState.PullDown;
            return;
        }

        m_Line.InitialState = (m_PullUpEnable, m_PullDownEnable) switch
        {
            (true,  false) => SignalLine.InitialLineState.PullUp,
            (false, true)  => SignalLine.InitialLineState.PullDown,
            _              => SignalLine.InitialLineState.HiZ,
        };
    }
}
