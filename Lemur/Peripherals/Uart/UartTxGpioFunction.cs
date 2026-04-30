using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Lemur.Peripherals.Uart;

public class UartTxGpioFunction : GpioFunctionBase
{
    private const int FifoCapacity = 32;

    private readonly Queue<byte> m_TxFifo = new Queue<byte>();
    private readonly ILogger<UartTxGpioFunction> m_Logger;
    private readonly string m_Name;
    private TimeSpan m_NextTimeToTransmit;
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

    // Frame construction properties — set from UARTLCR_H register fields by UartPeripheral.
    // WLEN (bits[6:5]): 0→5 bits, 1→6 bits, 2→7 bits, 3→8 bits — caller converts before setting.
    public int  DataBits     { get; set; } = 8;
    public bool TwoStopBits  { get; set; } = false;  // STP2
    public bool ParityEnable { get; set; } = false;  // PEN
    public bool EvenParity   { get; set; } = false;  // EPS
    public bool StickParity  { get; set; } = false;  // SPS
    public bool SendBreak    { get; set; } = false;  // BRK

    public UartTxGpioFunction(ILogger<UartTxGpioFunction> logger, string name = "unset")
    {
        BaudRate = 9600;
        SetOutput(TimeSpan.Zero, true); // UART idle line is high
        m_Logger = logger;
        m_Name = name;
    }

    public bool CanTransmit()  => m_TxFifo.Count < FifoCapacity;
    public bool IsFifoEmpty   => m_TxFifo.Count == 0;
    public bool IsFifoFull    => m_TxFifo.Count >= FifoCapacity;
    public bool IsBusy        => !IsFifoEmpty;

    public void Transmit(byte b)
    {
        if (m_TxFifo.Count >= FifoCapacity)
        {
            throw new InvalidOperationException("TX FIFO is full"); // TODO: Isn't there some IRQ for that?
        }

        m_TxFifo.Enqueue(b);
    }

    public void Tick(TimeSpan elapsed)
    {
        // If this thick is behind next time we can transmit, wait for the next tick
        if (m_NextTimeToTransmit > elapsed)
        {
            return;
        }

        // Transmission start right now
        m_NextTimeToTransmit = elapsed;

        // Dump everything
        while (m_TxFifo.TryDequeue(out var nextByte))
        {
            m_Logger.LogDebug("[TX-{name}] Byte start", m_Name);

            foreach (var bit in BuildFrame(nextByte))
            {
                SetOutput(m_NextTimeToTransmit, bit);
                m_NextTimeToTransmit += m_BitDuration;
            }

            m_Logger.LogDebug("[TX-{name}] Byte complete", m_Name);
        }
    }

    private IEnumerable<bool> BuildFrame(byte b)
    {
        if (SendBreak)
        {
            int breakBits = 1 + DataBits + (ParityEnable ? 1 : 0) + (TwoStopBits ? 2 : 1);
            for (int i = 0; i < breakBits; i++)
            {
                m_Logger.LogDebug("[TX-{name}] Send break", m_Name);
                yield return false;
            }

            yield break;
        }

        m_Logger.LogDebug("[TX-{name}] Start bit {bit}", m_Name, false);
        yield return false; // start bit

        int mask = (1 << DataBits) - 1;
        int data = b & mask;
        for (int i = 0; i < DataBits; i++)
        {
            var bit = ((data >> i) & 1) != 0;
            m_Logger.LogDebug("[TX-{name}] data {bit}", m_Name, bit);
            yield return bit;
        }

        if (ParityEnable)
        {
            var bit = ComputeParityBit(data);
            m_Logger.LogDebug("[TX-{name}] Parity {bit}", m_Name, bit);
            yield return bit;
        }

        m_Logger.LogDebug("[TX-{name}] Stop bit 1 {bit}", m_Name, true);
        yield return true; // stop bit 1

        if (TwoStopBits)
        {
            m_Logger.LogDebug("[TX-{name}] Stop bit 2 {bit}", m_Name, true);
            yield return true; // stop bit 2
        }
    }

    // Even parity: parity bit = 1 when data has an odd number of ones (makes total even).
    // Odd parity: parity bit = 1 when data has an even number of ones (makes total odd).
    // Stick parity: parity bit is fixed — 0 when EPS=1, 1 when EPS=0.
    private bool ComputeParityBit(int data)
    {
        if (StickParity)
        {
            return !EvenParity;
        }

        bool hasOddOnes = (BitOperations.PopCount((uint)data) & 1) != 0;
        return EvenParity ? hasOddOnes : !hasOddOnes;
    }

    public override void OnInput(TimeSpan time, bool value)
    {
    }
}
