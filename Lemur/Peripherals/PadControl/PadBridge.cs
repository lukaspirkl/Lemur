using System;

namespace Lemur.Peripherals.PadControl;

// Bridges a MuxedGpioFunction to a Pin, owning the pad control register fields.
//
// Output path (inner → pin): gated by ISO and OD.
// Input path (pin → inner):  gated by IE only; ISO does not isolate input per spec §9.7.
// Pulls (PUE/PDE):            drives the Pin's pull with Up/Down; bus keeper mode
//                             (both set) tracks the live line state so the pad retains its
//                             last driven level.
public class PadBridge
{
    private readonly IGpioFunction m_Inner;
    private readonly Pin           m_Pin;
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

    public PadBridge(Register32 reg, IGpioFunction inner, Pin pin, IElapsedTime elapsedTime,
        bool initialInputEnable = false, bool initialPullUpEnable = false, bool initialPullDownEnable = true)
    {
        m_Inner          = inner;
        m_Pin            = pin;
        m_ElapsedTime    = elapsedTime;
        m_InputEnable    = initialInputEnable;
        m_PullUpEnable   = initialPullUpEnable;
        m_PullDownEnable = initialPullDownEnable;

        reg.Field(8,    () => m_IsolationControl,    v => { m_IsolationControl    = v; Refresh(); })
           .Field(7,    () => m_OutputDisable,        v => { m_OutputDisable        = v; ApplyOutput(m_ElapsedTime.Now); })
           .Field(6,    () => m_InputEnable,          v =>   m_InputEnable          = v)
           .Field(4, 2, () => m_DriveStrength,        v =>   m_DriveStrength        = v)
           .Field(3,    () => m_PullUpEnable,         v => { m_PullUpEnable         = v; ApplyPulls(m_ElapsedTime.Now); })
           .Field(2,    () => m_PullDownEnable,       v => { m_PullDownEnable       = v; ApplyPulls(m_ElapsedTime.Now); })
           .Field(1,    () => m_EnableSchmittTrigger, v =>   m_EnableSchmittTrigger = v)
           .Field(0,    () => m_SlewRateFast,         v =>   m_SlewRateFast         = v);

        // Apply pull before subscribing so the initial SetPull() doesn't trigger OnPinChanged.
        ApplyPulls(m_ElapsedTime.Now);

        inner.OutputChanged += OnInnerOutputChanged;
        pin.Changed         += OnPinChanged;

        ApplyOutput(m_ElapsedTime.Now);
    }

    private void Refresh()
    {
        ApplyPulls(m_ElapsedTime.Now);
        ApplyOutput(m_ElapsedTime.Now);
    }

    private void OnInnerOutputChanged(GpioFunctionOutput e)
    {
        // ISO=1 latches the output — do not forward new changes until ISO is cleared.
        if (m_IsolationControl) return;
        DriveFromInner(e.Time, e.NewValue);
    }

    private void OnPinChanged(SignalChange data)
    {
        // Bus keeper: keep weak drive in sync with the line so the level is retained on release.
        // Blocked by ISO because the bus keeper logic is in the switched core domain (§9.6.1).
        if (!m_IsolationControl && m_PullUpEnable && m_PullDownEnable)
            ApplyPulls(data.Time);

        // IE gates the input path; ISO does not (§9.7: "input signal … is not isolated").
        // Floating line (null) reads as 0 at the pad input.
        if (m_InputEnable)
            m_Inner.OnInput(data.Time, data.NewState ?? false);
    }

    private void ApplyOutput(TimeSpan time)
    {
        // ISO=1: output is latched — leave whatever we last drove on the line unchanged.
        if (m_IsolationControl) return;
        DriveFromInner(time, m_Inner.Output);
    }

    private void DriveFromInner(TimeSpan time, bool? value)
    {
        m_Pin.SetOutput(m_OutputDisable ? null : value, time);
    }

    private void ApplyPulls(TimeSpan time)
    {
        var pull = (m_PullUpEnable, m_PullDownEnable) switch
        {
            // Bus keeper: match current pin state — floating line treated as low (§9.6.1).
            (true,  true)  => m_Pin.State == true ? PullDirection.Up : PullDirection.Down,
            (true,  false) => PullDirection.Up,
            (false, true)  => PullDirection.Down,
            _              => PullDirection.None,
        };

        m_Pin.SetPull(pull, time);
    }
}
