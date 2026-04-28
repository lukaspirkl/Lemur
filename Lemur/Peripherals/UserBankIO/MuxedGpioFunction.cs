using System;
using System.Collections.Generic;

namespace Lemur.Peripherals.UserBankIO;

public class MuxedGpioFunction : GpioFunctionBase
{
    public enum OutOver
    {
        Normal = 0,
        Invert = 1,
        Low    = 2,
        High   = 3,
    }

    public enum OeOver
    {
        Normal  = 0,
        Invert  = 1,
        Disable = 2,
        Enable  = 3,
    }

    public enum InOver
    {
        Normal = 0,
        Invert = 1,
        Low    = 2,
        High   = 3,
    }

    public enum IrqOver
    {
        Normal = 0,
        Invert = 1,
        Low    = 2,
        High   = 3,
    }

    private readonly Dictionary<uint, (IGpioFunction Function, string Name)> m_Functions = new();
    private readonly IElapsedTime m_ElapsedTime;
    private IGpioFunction? m_Selected;
    private string m_SelectedName = "NULL";
    private uint m_Funcsel = 0x1f;

    private OutOver m_OutOver = OutOver.Normal;
    private OeOver  m_OeOver  = OeOver.Normal;
    private InOver  m_InOver  = InOver.Normal;
    private IrqOver m_IrqOver = IrqOver.Normal;

    private bool? m_LastRawInput;

    public string SelectedFunctionName => m_SelectedName;

    public event Action? SelectionChanged;

    public MuxedGpioFunction(Register32 ctrlReg, IElapsedTime elapsedTime)
    {
        m_ElapsedTime = elapsedTime;

        ctrlReg.Field(0,  5,  () => m_Funcsel,  v => Select(v));
        ctrlReg.Field<OutOver>(12, 2, () => m_OutOver, v => { m_OutOver = v; RefreshOutput(); });
        ctrlReg.Field<OeOver> (14, 2, () => m_OeOver,  v => { m_OeOver = v; RefreshOutput(); });
        ctrlReg.Field<InOver> (16, 2, () => m_InOver,  v => { m_InOver = v; RefreshInput(); });
        ctrlReg.Field<IrqOver>(28, 2, () => m_IrqOver, v => m_IrqOver = v);

        Select(0x1f);
    }

    public void Add(uint funcsel, string name, IGpioFunction function)
    {
        m_Functions[funcsel] = (function, name);
    }

    public void Select(uint funcsel)
    {
        m_Funcsel = funcsel;

        if (m_Selected != null)
            m_Selected.OutputChanged -= OnSelectedOutputChanged;

        if (m_Functions.TryGetValue(funcsel, out var entry))
        {
            m_Selected     = entry.Function;
            m_SelectedName = entry.Name;
            m_Selected.OutputChanged += OnSelectedOutputChanged;
        }
        else
        {
            m_Selected     = null;
            m_SelectedName = "NULL";
        }

        RefreshOutput();
        SelectionChanged?.Invoke();
    }

    private void OnSelectedOutputChanged(GpioFunctionOutput change)
    {
        SetOutput(change.Time, ComputeOutput(change.NewValue));
    }

    private void RefreshOutput()
    {
        SetOutput(m_ElapsedTime.Now, ComputeOutput(m_Selected?.Output));
    }

    // Applies OUTOVER (value) and OEOVER (enable) as independent stages, matching hardware.
    // OUTOVER modifies *what* is driven; OEOVER modifies *whether* it is driven.
    // null input means the peripheral is not driving (OE=0).
    private bool? ComputeOutput(bool? peripheral)
    {
        bool value = m_OutOver switch
        {
            OutOver.Invert => !(peripheral ?? false),
            OutOver.Low    => false,
            OutOver.High   => true,
            _              => peripheral ?? false,
        };

        bool oe = m_OeOver switch
        {
            OeOver.Invert  => !peripheral.HasValue,
            OeOver.Disable => false,
            OeOver.Enable  => true,
            _              => peripheral.HasValue,
        };

        return oe ? value : null;
    }

    public override void OnInput(TimeSpan time, bool value)
    {
        m_LastRawInput = value;
        m_Selected?.OnInput(time, ApplyInOver(value));
    }

    private void RefreshInput()
    {
        if (m_LastRawInput.HasValue)
            m_Selected?.OnInput(m_ElapsedTime.Now, ApplyInOver(m_LastRawInput.Value));
    }

    private bool ApplyInOver(bool raw) => m_InOver switch
    {
        InOver.Invert => !raw,
        InOver.Low    => false,
        InOver.High   => true,
        _             => raw,
    };
}
