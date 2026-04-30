using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals.Uart;

public class UartRxGpioFunction : GpioFunctionBase
{
    private const int FifoCapacity = 32;

    private readonly Queue<byte> m_RxFifo = new();
    private RxState m_State;
    private int m_DataBitIndex;
    private int m_CurrentData;
    private bool m_ReceivedParity;

    private TimeSpan m_BitDuration;

    public int BaudRate
    {
        get;
        set
        {
            field = value;
            m_BitDuration = TimeSpan.FromSeconds(1.0 / value);
        }
    }

    public TimeSpan SafetyMargin { get; set; }

    // Frame format — must match the transmitter's configuration.
    public int DataBits { get; set; } = 8;
    public bool TwoStopBits { get; set; } = false;
    public bool ParityEnable { get; set; } = false;
    public bool EvenParity { get; set; } = false;
    public bool StickParity { get; set; } = false;

    // Sticky error flags — persist until ClearErrors().
    public bool OverrunError { get; private set; }
    public bool FramingError { get; private set; }
    public bool ParityError { get; private set; }
    public bool BreakDetected { get; private set; }

    public bool HasData => m_RxFifo.Count > 0;
    public int RxFifoCount => m_RxFifo.Count;

    public event Action? DataReceived;
    public event Action? OverrunOccurred;
    public event Action? FramingErrorOccurred;
    public event Action? ParityErrorOccurred;
    public event Action? BreakDetectedOccurred;

    private readonly ILogger<UartRxGpioFunction> m_Logger;
    private readonly string m_Name;

    private TimeSpan m_LastBitTime = TimeSpan.Zero;

    public UartRxGpioFunction(ILogger<UartRxGpioFunction> logger, string name = "unset")
    {
        m_Logger = logger;
        m_Name = name;
        
        BaudRate = 9600;
        SafetyMargin = TimeSpan.FromMicroseconds(5);
    }

    public override void OnInput(TimeSpan time, bool value)
    {
        m_Logger.LogDebug("[RX-{name}]Input received {bit}. Diff: {diff}us ({duration})", m_Name, value, time.TotalMicroseconds - m_LastBitTime.TotalMicroseconds, m_BitDuration.TotalMicroseconds);

        // When next input is longer than expected we should reset to idle state
        if (m_LastBitTime == TimeSpan.Zero || m_LastBitTime + m_BitDuration + SafetyMargin < time)
        {
            m_Logger.LogDebug("[RX-{name}]Switch to idle.", m_Name);
            m_State = RxState.Idle;
        }

        // TODO: Ignore events that are received too soon

        m_LastBitTime = time;
        ProcessBit(value);
    }

    public bool TryRead(out byte b) => m_RxFifo.TryDequeue(out b);

    // Direct byte injection — used for loopback and external RX simulation.
    public void EnqueueByte(byte b)
    {
        m_RxFifo.Enqueue(b);
        DataReceived?.Invoke();
    }

    public void ClearErrors()
    {
        OverrunError = false;
        FramingError = false;
        ParityError = false;
        BreakDetected = false;
    }

    private void ProcessBit(bool bit)
    {
        switch (m_State)
        {
            case RxState.Idle:
                m_Logger.LogDebug("[RX-{name}]Idle {bit}", m_Name, bit);
                if (!bit)
                {
                    m_State = RxState.Data;
                    m_DataBitIndex = 0;
                    m_CurrentData = 0;
                    m_ReceivedParity = false;
                }
                break;

            case RxState.Data:
                m_Logger.LogDebug("[RX-{name}]Data {bit}", m_Name, bit);
                if (bit)
                    m_CurrentData |= 1 << m_DataBitIndex;

                m_DataBitIndex++;
                if (m_DataBitIndex >= DataBits)
                    m_State = ParityEnable ? RxState.Parity : RxState.Stop1;
                break;

            case RxState.Parity:
                m_Logger.LogDebug("[RX-{name}]Parity {bit}", m_Name, bit);
                m_ReceivedParity = bit;
                m_State = RxState.Stop1;
                break;

            case RxState.Stop1:
                m_Logger.LogDebug("[RX-{name}]Stop1 {bit}", m_Name, bit);
                HandleStopBitErrors(bit);
                m_State = TwoStopBits ? RxState.Stop2 : RxState.Idle;
                if (!TwoStopBits)
                    CompleteFrame();
                break;

            case RxState.Stop2:
                m_Logger.LogDebug("[RX-{name}]Stop2 {bit}", m_Name, bit);
                if (!bit)
                {
                    FramingError = true;
                    m_Logger.LogWarning("UART RX-{name}: framing error (data=0x{Data:X2})", m_Name, (byte)m_CurrentData);
                    FramingErrorOccurred?.Invoke();
                }
                CompleteFrame();
                m_State = RxState.Idle;
                break;
        }
    }

    private void HandleStopBitErrors(bool stopBit)
    {
        if (m_CurrentData == 0 && !stopBit)
        {
            BreakDetected = true;
            m_Logger.LogWarning("UART RX-{name}: break condition detected", m_Name);
            BreakDetectedOccurred?.Invoke();
        }

        if (!stopBit)
        {
            FramingError = true;
            m_Logger.LogWarning("UART RX-{name}: framing error (data=0x{Data:X2})", m_Name, (byte)m_CurrentData);
            FramingErrorOccurred?.Invoke();
        }
    }

    private void CompleteFrame()
    {
        if (ParityEnable)
        {
            bool expectedParity = ComputeParityBit(m_CurrentData);
            if (m_ReceivedParity != expectedParity)
            {
                ParityError = true;
                m_Logger.LogWarning("UART RX-{name}: parity error (data=0x{Data:X2}, received={Received}, expected={Expected})", m_Name, (byte)m_CurrentData, m_ReceivedParity, expectedParity);
                ParityErrorOccurred?.Invoke();
            }
        }

        if (m_RxFifo.Count >= FifoCapacity)
        {
            OverrunError = true;
            m_Logger.LogWarning("UART RX-{name}: overrun, byte 0x{Data:X2} discarded", m_Name, (byte)m_CurrentData);
            OverrunOccurred?.Invoke();
        }
        else
        {
            m_Logger.LogDebug("[RX-{name}]Complete frame 0x{Data:X2} ({char})", m_Name, (byte)m_CurrentData, (char)m_CurrentData);
            m_RxFifo.Enqueue((byte)m_CurrentData);
            DataReceived?.Invoke();
        }
    }

    private bool ComputeParityBit(int data)
    {
        if (StickParity) return !EvenParity;
        bool hasOddOnes = (BitOperations.PopCount((uint)data) & 1) != 0;
        return EvenParity ? hasOddOnes : !hasOddOnes;
    }

    private enum RxState
    {
        Idle,
        Data,
        Parity,
        Stop1,
        Stop2,
    }
}
