using System;
using System.Collections.Generic;
using System.Numerics;

namespace Lemur.Peripherals.Uart;

public class UartTxGpioFunction : GpioFunctionBase
{
    private const int FifoCapacity = 32;

    private readonly Queue<byte> m_TxFifo = new Queue<byte>();
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

    public UartTxGpioFunction()
    {
        BaudRate = 9600;
        SetOutput(TimeSpan.Zero, true); // UART idle line is high
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
            foreach (var bit in BuildFrame(nextByte))
            {
                SetOutput(m_NextTimeToTransmit, bit);
                m_NextTimeToTransmit += m_BitDuration;
            }
        }
    }

    private IEnumerable<bool> BuildFrame(byte b)
    {
        if (SendBreak)
        {
            int breakBits = 1 + DataBits + (ParityEnable ? 1 : 0) + (TwoStopBits ? 2 : 1);
            for (int i = 0; i < breakBits; i++)
            {
                yield return false;
            }

            yield break;
        }

        yield return false; // start bit

        int mask = (1 << DataBits) - 1;
        int data = b & mask;
        for (int i = 0; i < DataBits; i++)
        {
            yield return ((data >> i) & 1) != 0;
        }

        if (ParityEnable)
        {
            yield return ComputeParityBit(data);
        }

        yield return true; // stop bit 1

        if (TwoStopBits)
        {
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
