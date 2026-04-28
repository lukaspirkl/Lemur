using System;
using Lemur.Csr;
using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals.Uart;

public class UART0 : UartPeripheral
{
    public UART0(uint baseAddress, string name, ILogger<UART0> logger, ILogger<UartRxGpioFunction> rxLogger, CsrController csrController, Clocks clocks)
        : base(baseAddress, name, logger, rxLogger, csrController, Irq.UART0_IRQ, clocks)
    {
    }
}

public class UART1 : UartPeripheral
{
    public UART1(uint baseAddress, string name, ILogger<UART1> logger, ILogger<UartRxGpioFunction> rxLogger, CsrController csrController, Clocks clocks)
        : base(baseAddress, name, logger, rxLogger, csrController, Irq.UART1_IRQ, clocks)
    {
    }
}

/// <summary>
/// Arm PrimeCell PL011 UART (revision r1p5), as instantiated twice in RP2350.
///
/// TX serialisation and RX decoding are delegated to UartTxGpioFunction and
/// UartRxGpioFunction respectively; this class manages registers, interrupts,
/// baud rate, and flow control.
///
/// Spec: RP2350 Datasheet §12.1 — "UART"; ARM PrimeCell UART (PL011) r1p5 TRM.
/// </summary>
public class UartPeripheral : PeripheralBase, ITickable
{
    // ─── GPIO functions ───────────────────────────────────────────────────────

    /// <summary>Serialises TX bytes onto the UART TX pin one bit at a time.</summary>
    public UartTxGpioFunction TxFunction { get; } = new();

    /// <summary>Decodes incoming bits from the UART RX pin into bytes.</summary>
    public UartRxGpioFunction RxFunction { get; }

    /// <summary>Drives nUARTRTS — LOW when asserted (ready to receive).</summary>
    public UartRtsGpioFunction RtsFunction { get; } = new();

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Raised when a byte is enqueued for transmission (used by the UI terminal
    /// to display what firmware sends). Not raised in loopback mode.
    /// </summary>
    public event Action<char>? ReceivedData;

    /// <summary>Inject a byte into the RX FIFO as if it arrived on the UARTRXD pin.</summary>
    public void EnqueueRxByte(byte b)
    {
        if (RxFunction.RxFifoCount >= RxFifoCapacity)
            return;
        RxFunction.EnqueueByte(b);
        UpdateRxInterruptStatus();
        RestartRtimTimer();
    }

    // ─── CTS ─────────────────────────────────────────────────────────────────

    private Func<bool> m_GetCtsAsserted = () => false;
    public void SetCtsGetter(Func<bool> getter) => m_GetCtsAsserted = getter;

    // ─── IRQ ─────────────────────────────────────────────────────────────────

    private readonly CsrController m_CsrController;
    private readonly Irq m_IrqNumber;

    // ─── LCR_H state ─────────────────────────────────────────────────────────

    private bool m_Fen;
    private uint m_Wlen = 3; // 3 → 8 data bits

    // ─── CR fields ────────────────────────────────────────────────────────────

    private bool m_Uarten;
    private bool m_Txe = true;
    private bool m_Rxe = true;
    private bool m_Lbe;
    private bool m_Rtsen, m_Ctsen;
    private bool m_Rts, m_Dtr, m_Out1, m_Out2;

    // ─── IFLS fields ──────────────────────────────────────────────────────────

    private uint m_TxIflSel = 0x2;
    private uint m_RxIflSel = 0x2;

    // ─── Interrupt state ──────────────────────────────────────────────────────

    private uint m_Imsc;
    private bool m_TxRis;
    private bool m_RxRis;

    // ─── Baud rate registers ──────────────────────────────────────────────────

    private uint m_IBrd;
    private uint m_FBrd;
    private uint m_IlpDvsr;

    private readonly Clocks m_Clocks;

    // ─── Receive timeout (RTIM) ───────────────────────────────────────────────

    private const int RTIM_TIMEOUT_MS = 50;
    private System.Threading.Timer? m_RtimTimer;
    private bool m_RtimRis;

    // ─────────────────────────────────────────────────────────────────────────

    public UartPeripheral(uint baseAddress, string name, ILogger<UartPeripheral> logger,
        ILogger<UartRxGpioFunction> rxLogger, CsrController csrController, Irq irqNumber, Clocks clocks)
        : base(baseAddress, name, logger)
    {
        m_CsrController = csrController;
        m_IrqNumber     = irqNumber;
        m_Clocks        = clocks;
        RxFunction      = new UartRxGpioFunction(rxLogger, "UartPeripheral");

        RxFunction.DataReceived += () =>
        {
            UpdateRxInterruptStatus();
            RestartRtimTimer();
        };

        // UARTDR (0x000) — Data Register
        AddRegister(0x000, "UARTDR")
            .Field(0, 8, ReceiveData, TransmitData);

        // UARTRSR/UARTECR (0x004)
        AddRegister(0x004, "UARTRSR");

        // UARTFR (0x018) — Flag Register (read-only)
        AddRegister(0x018, "UARTFR")
            .Field(lsb: 0, getter: () => m_GetCtsAsserted())          // CTS
            .Field(lsb: 1, getter: () => false)                        // DSR
            .Field(lsb: 2, getter: () => false)                        // DCD
            .Field(lsb: 3, getter: () => TxFunction.IsBusy)            // BUSY
            .Field(lsb: 4, getter: () => RxFunction.RxFifoCount == 0)  // RXFE
            .Field(lsb: 5, getter: () => TxFunction.IsFifoFull)        // TXFF
            .Field(lsb: 6, getter: () => RxFifoFull)                   // RXFF
            .Field(lsb: 7, getter: () => TxFunction.IsFifoEmpty)       // TXFE
            .Field(lsb: 8, getter: () => false);                       // RI

        // UARTILPR (0x020)
        AddRegister(0x020, "UARTILPR")
            .Field(0, 8, () => m_IlpDvsr, v => m_IlpDvsr = v);

        // UARTIBRD (0x024)
        AddRegister(0x024, "UARTIBRD")
            .Field(0, 16, () => m_IBrd, v => { m_IBrd = v; UpdateBaudRate(); });

        // UARTFBRD (0x028)
        AddRegister(0x028, "UARTFBRD")
            .Field(0, 6, () => m_FBrd, v => { m_FBrd = v; UpdateBaudRate(); });

        // UARTLCR_H (0x02C) — Line Control Register
        AddRegister(0x02C, "UARTLCR_H")
            .Field(lsb: 0, getter: () => TxFunction.SendBreak,    setter: v => TxFunction.SendBreak = v)
            .Field(lsb: 1, getter: () => TxFunction.ParityEnable, setter: v => { TxFunction.ParityEnable = v; RxFunction.ParityEnable = v; })
            .Field(lsb: 2, getter: () => TxFunction.EvenParity,   setter: v => { TxFunction.EvenParity   = v; RxFunction.EvenParity   = v; })
            .Field(lsb: 3, getter: () => TxFunction.TwoStopBits,  setter: v => { TxFunction.TwoStopBits  = v; RxFunction.TwoStopBits  = v; })
            .Field(lsb: 4, getter: () => m_Fen,                   setter: v => m_Fen = v)
            .Field(5, 2,   getter: () => m_Wlen,                  setter: v => { m_Wlen = v; UpdateDataBits(); })
            .Field(lsb: 7, getter: () => TxFunction.StickParity,  setter: v => { TxFunction.StickParity  = v; RxFunction.StickParity  = v; });

        // UARTCR (0x030) — Control Register
        AddRegister(0x030, "UARTCR", resetValue: (1u << 8) | (1u << 9))
            .Field(lsb:  0, getter: () => m_Uarten, setter: v => m_Uarten = v)
            .Field(lsb:  7, getter: () => m_Lbe,    setter: v => m_Lbe    = v)
            .Field(lsb:  8, getter: () => m_Txe,    setter: v => m_Txe    = v)
            .Field(lsb:  9, getter: () => m_Rxe,    setter: v => m_Rxe    = v)
            .Field(lsb: 10, getter: () => m_Dtr,    setter: v => m_Dtr    = v)
            .Field(lsb: 11, getter: () => m_Rts,    setter: v => { m_Rts   = v; UpdateRtsLine(); })
            .Field(lsb: 12, getter: () => m_Out1,   setter: v => m_Out1   = v)
            .Field(lsb: 13, getter: () => m_Out2,   setter: v => m_Out2   = v)
            .Field(lsb: 14, getter: () => m_Rtsen,  setter: v => { m_Rtsen = v; UpdateRtsLine(); })
            .Field(lsb: 15, getter: () => m_Ctsen,  setter: v => m_Ctsen  = v);

        // UARTIFLS (0x034)
        AddRegister(0x034, "UARTIFLS", resetValue: 0b010_010u)
            .Field(0, 3, () => m_TxIflSel, v => m_TxIflSel = v)
            .Field(3, 3, () => m_RxIflSel, v => m_RxIflSel = v);

        // UARTIMSC (0x038)
        AddRegister(0x038, "UARTIMSC")
            .Field(0, 11, () => m_Imsc, v => m_Imsc = v)
            .OnWrite(_ => UpdateUartIrq());

        // UARTRIS (0x03C)
        AddRegister(0x03C, "UARTRIS")
            .OnRead(ComputeRawInterruptStatus);

        // UARTMIS (0x040)
        AddRegister(0x040, "UARTMIS")
            .OnRead(() => ComputeRawInterruptStatus() & m_Imsc);

        // UARTICR (0x044)
        AddRegister(0x044, "UARTICR")
            .OnRead(() => 0u)
            .OnWrite(ClearInterrupts);

        // UARTDMACR (0x048)
        AddRegister(0x048, "UARTDMACR");

        AddRegister(0xFE0, "UARTPERIPHID0", resetValue: 0x11);
        AddRegister(0xFE4, "UARTPERIPHID1", resetValue: 0x10);
        AddRegister(0xFE8, "UARTPERIPHID2", resetValue: 0x34);
        AddRegister(0xFEC, "UARTPERIPHID3", resetValue: 0x00);
        AddRegister(0xFF0, "UARTPCELLID0",  resetValue: 0x0D);
        AddRegister(0xFF4, "UARTPCELLID1",  resetValue: 0xF0);
        AddRegister(0xFF8, "UARTPCELLID2",  resetValue: 0x05);
        AddRegister(0xFFC, "UARTPCELLID3",  resetValue: 0xB1);

        UpdateDataBits(); // apply m_Wlen default (3 → 8 bits)
        UpdateRtsLine();  // initialise RTS to deasserted (HIGH)
    }

    // ─── ITickable ────────────────────────────────────────────────────────────

    void ITickable.Tick(TimeSpan now)
    {
        TxFunction.Tick(now);
    }

    // ─── UARTDR handlers ──────────────────────────────────────────────────────

    private void TransmitData(uint data)
    {
        var b = (byte)(data & 0xFF);

        if (m_Lbe)
        {
            // Loopback: route directly into the RX FIFO, bypassing TX serialisation.
            if (RxFunction.RxFifoCount < RxFifoCapacity)
            {
                RxFunction.EnqueueByte(b);
                UpdateRxInterruptStatus();
                RestartRtimTimer();
            }
        }
        else
        {
            TxFunction.Transmit(b);
            ReceivedData?.Invoke((char)b);
        }

        m_TxRis = true;
        UpdateUartIrq();
    }

    private uint ReceiveData()
    {
        if (RxFunction.TryRead(out var b))
        {
            UpdateRxInterruptStatus();
            if (RxFunction.RxFifoCount == 0)
            {
                CancelRtimTimer();
                m_RtimRis = false;
                UpdateUartIrq();
            }
            return b;
        }
        return 0;
    }

    // ─── Interrupt helpers ────────────────────────────────────────────────────

    private uint ComputeRawInterruptStatus()
    {
        uint ris = 0;
        if (m_RxRis)   ris |= 1u << 4;
        if (m_TxRis)   ris |= 1u << 5;
        if (m_RtimRis) ris |= 1u << 6;
        return ris;
    }

    private void UpdateUartIrq()
    {
        uint mis = ComputeRawInterruptStatus() & m_Imsc;
        m_CsrController.Meipa.SetHardwarePending(m_IrqNumber, mis != 0);
    }

    private void ClearInterrupts(uint icr)
    {
        if ((icr & (1u << 4)) != 0) m_RxRis   = false;
        if ((icr & (1u << 5)) != 0) m_TxRis   = false;
        if ((icr & (1u << 6)) != 0)
        {
            m_RtimRis = false;
            if (RxFunction.RxFifoCount == 0) CancelRtimTimer();
        }
        UpdateUartIrq();
    }

    private void UpdateRxInterruptStatus()
    {
        m_RxRis = RxFunction.RxFifoCount >= RxFifoThreshold;
        UpdateUartIrq();
        UpdateRtsLine();
    }

    // ─── Baud rate & frame format ─────────────────────────────────────────────

    private void UpdateBaudRate()
    {
        if (m_IBrd == 0) return;
        double divisor = 16.0 * (m_IBrd + m_FBrd / 64.0);
        int baud = (int)(m_Clocks.ClkPeriFrequency / divisor);
        TxFunction.BaudRate = baud;
        RxFunction.BaudRate = baud;
    }

    private void UpdateDataBits()
    {
        int bits = (int)m_Wlen + 5;
        TxFunction.DataBits = bits;
        RxFunction.DataBits = bits;
    }

    // ─── Receive timeout (RTIM) ───────────────────────────────────────────────

    private void RestartRtimTimer()
    {
        m_RtimTimer?.Dispose();
        m_RtimTimer = new System.Threading.Timer(_ =>
        {
            if (RxFunction.RxFifoCount > 0)
            {
                m_RtimRis = true;
                UpdateUartIrq();
            }
        }, null, RTIM_TIMEOUT_MS, System.Threading.Timeout.Infinite);
    }

    private void CancelRtimTimer()
    {
        m_RtimTimer?.Dispose();
        m_RtimTimer = null;
    }

    // ─── RTS ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Drives RtsFunction to reflect current flow-control state.
    /// RTSEN=0 (manual): nUARTRTS = NOT(m_Rts).
    /// RTSEN=1 (auto):   assert when RX FIFO is below threshold, deassert when full.
    /// </summary>
    private void UpdateRtsLine()
    {
        bool deassert = m_Rtsen
            ? RxFunction.RxFifoCount >= RxFifoThreshold
            : !m_Rts;
        RtsFunction.Set(deassert); // true=HIGH=deasserted, false=LOW=asserted
    }

    // ─── FIFO helpers ─────────────────────────────────────────────────────────

    private int  RxFifoCapacity  => m_Fen ? 32 : 1;
    private bool RxFifoFull      => RxFunction.RxFifoCount >= RxFifoCapacity;

    private int RxFifoThreshold => m_Fen
        ? m_RxIflSel switch { 0 => 4, 1 => 8, 2 => 16, 3 => 24, 4 => 28, _ => 16 }
        : 1;
}

/// <summary>
/// Output-only GPIO function for nUARTRTS.
/// Drives HIGH (deasserted) or LOW (asserted) based on flow-control state.
/// </summary>
public class UartRtsGpioFunction : GpioFunctionBase
{
    public void Set(bool high) => SetOutput(TimeSpan.Zero, high);
    public override void OnInput(TimeSpan time, bool value) { }
}
