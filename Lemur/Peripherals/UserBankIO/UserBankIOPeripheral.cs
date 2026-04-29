using System;
using Lemur.Csr;
using Lemur.Peripherals.Sio;
using Lemur.Peripherals.Uart;
using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals.UserBankIO;

public class UserBankIOPeripheral : PeripheralBase
{
    private const uint FuncselSio     = 0x05;
    private const uint FuncselUart    = 0x02;
    private const uint FuncselUartAlt = 0x0b;

    // Each INTR register covers 8 GPIOs × 4 bits (LEVEL_LOW, LEVEL_HIGH, EDGE_LOW, EDGE_HIGH).
    // Bit layout for GPIO n: bits [(n%8)*4 + 3 : (n%8)*4] in m_Intr[n/8].
    // EDGE bits (2,3 of each nibble) are sticky — set on transitions, write-1-to-clear.
    // LEVEL bits (0,1 of each nibble) are live — updated on every input change.
    private const uint EdgeBitMask = 0xCCCCCCCC; // bits 2,3 of each 4-bit group

    private readonly CsrController m_Csr;
    private readonly uint[] m_Intr       = new uint[6];
    private readonly uint[] m_Proc0Inte  = new uint[6];
    private readonly uint[] m_Proc0Intf  = new uint[6];
    private readonly bool?[] m_GpioPrevInput = new bool?[48];

    public MuxedGpioFunction[] GpioMux { get; }
    public int GpioLineCount => 48;

    public UserBankIOPeripheral(uint baseAddress, string name, ILogger<UserBankIOPeripheral> logger,
        CsrController csr, SioPeripheral sio, UART0 uart0, UART1 uart1, IElapsedTime elapsedTime)
        : base(baseAddress, name, logger)
    {
        m_Csr = csr;

        GpioMux = new MuxedGpioFunction[48];
        for (uint i = 0; i < 48; i++)
        {
            AddRegister(i * 8,     $"GPIO{i}_STATUS");
            GpioMux[i] = new MuxedGpioFunction(AddRegister(i * 8 + 4, $"GPIO{i}_CTRL"), elapsedTime);
        }

        // ── SIO F5 ────────────────────────────────────────────────────────
        for (uint i = 0; i < 32; i++)
            GpioMux[i].Add(FuncselSio, $"SIO_{i}", sio.GpioLo[i]);
        for (uint i = 0; i < 16; i++)
            GpioMux[32 + i].Add(FuncselSio, $"SIO_{32 + i}", sio.GpioHi[i]);

        // ── UART0 F2 (primary) ────────────────────────────────────────────
        GpioMux[0].Add(FuncselUart,  "UART0_TX", uart0.TxFunction);
        GpioMux[1].Add(FuncselUart,  "UART0_RX", uart0.RxFunction);
        GpioMux[12].Add(FuncselUart, "UART0_TX", uart0.TxFunction);
        GpioMux[13].Add(FuncselUart, "UART0_RX", uart0.RxFunction);
        GpioMux[16].Add(FuncselUart, "UART0_TX", uart0.TxFunction);
        GpioMux[17].Add(FuncselUart, "UART0_RX", uart0.RxFunction);
        GpioMux[28].Add(FuncselUart, "UART0_TX", uart0.TxFunction);
        GpioMux[29].Add(FuncselUart, "UART0_RX", uart0.RxFunction);
        GpioMux[32].Add(FuncselUart, "UART0_TX", uart0.TxFunction);
        GpioMux[33].Add(FuncselUart, "UART0_RX", uart0.RxFunction);
        GpioMux[44].Add(FuncselUart, "UART0_TX", uart0.TxFunction);
        GpioMux[45].Add(FuncselUart, "UART0_RX", uart0.RxFunction);

        // ── UART1 F2 (primary) ────────────────────────────────────────────
        GpioMux[4].Add(FuncselUart,  "UART1_TX", uart1.TxFunction);
        GpioMux[5].Add(FuncselUart,  "UART1_RX", uart1.RxFunction);
        GpioMux[8].Add(FuncselUart,  "UART1_TX", uart1.TxFunction);
        GpioMux[9].Add(FuncselUart,  "UART1_RX", uart1.RxFunction);
        GpioMux[20].Add(FuncselUart, "UART1_TX", uart1.TxFunction);
        GpioMux[21].Add(FuncselUart, "UART1_RX", uart1.RxFunction);
        GpioMux[24].Add(FuncselUart, "UART1_TX", uart1.TxFunction);
        GpioMux[25].Add(FuncselUart, "UART1_RX", uart1.RxFunction);
        GpioMux[36].Add(FuncselUart, "UART1_TX", uart1.TxFunction);
        GpioMux[37].Add(FuncselUart, "UART1_RX", uart1.RxFunction);
        GpioMux[40].Add(FuncselUart, "UART1_TX", uart1.TxFunction);
        GpioMux[41].Add(FuncselUart, "UART1_RX", uart1.RxFunction);

        // ── UART0 F11 (alternate) ─────────────────────────────────────────
        GpioMux[2].Add(FuncselUartAlt,  "UART0_TX", uart0.TxFunction);
        GpioMux[3].Add(FuncselUartAlt,  "UART0_RX", uart0.RxFunction);
        GpioMux[14].Add(FuncselUartAlt, "UART0_TX", uart0.TxFunction);
        GpioMux[15].Add(FuncselUartAlt, "UART0_RX", uart0.RxFunction);
        GpioMux[18].Add(FuncselUartAlt, "UART0_TX", uart0.TxFunction);
        GpioMux[19].Add(FuncselUartAlt, "UART0_RX", uart0.RxFunction);
        GpioMux[30].Add(FuncselUartAlt, "UART0_TX", uart0.TxFunction);
        GpioMux[31].Add(FuncselUartAlt, "UART0_RX", uart0.RxFunction);
        GpioMux[34].Add(FuncselUartAlt, "UART0_TX", uart0.TxFunction);
        GpioMux[35].Add(FuncselUartAlt, "UART0_RX", uart0.RxFunction);
        GpioMux[46].Add(FuncselUartAlt, "UART0_TX", uart0.TxFunction);
        GpioMux[47].Add(FuncselUartAlt, "UART0_RX", uart0.RxFunction);

        // ── UART1 F11 (alternate) ─────────────────────────────────────────
        GpioMux[6].Add(FuncselUartAlt,  "UART1_TX", uart1.TxFunction);
        GpioMux[7].Add(FuncselUartAlt,  "UART1_RX", uart1.RxFunction);
        GpioMux[10].Add(FuncselUartAlt, "UART1_TX", uart1.TxFunction);
        GpioMux[11].Add(FuncselUartAlt, "UART1_RX", uart1.RxFunction);
        GpioMux[22].Add(FuncselUartAlt, "UART1_TX", uart1.TxFunction);
        GpioMux[23].Add(FuncselUartAlt, "UART1_RX", uart1.RxFunction);
        GpioMux[26].Add(FuncselUartAlt, "UART1_TX", uart1.TxFunction);
        GpioMux[27].Add(FuncselUartAlt, "UART1_RX", uart1.RxFunction);
        GpioMux[38].Add(FuncselUartAlt, "UART1_TX", uart1.TxFunction);
        GpioMux[39].Add(FuncselUartAlt, "UART1_RX", uart1.RxFunction);
        GpioMux[42].Add(FuncselUartAlt, "UART1_TX", uart1.TxFunction);
        GpioMux[43].Add(FuncselUartAlt, "UART1_RX", uart1.RxFunction);

        // ── GPIO interrupt support ─────────────────────────────────────────
        for (int i = 0; i < 48; i++)
        {
            int gpio = i;
            GpioMux[i].InputChanged += (time, value) => OnGpioInput(gpio, time, value);
        }

        // ── IRQ summary registers (stubs) ─────────────────────────────────
        AddRegister(0x200, "IRQSUMMARY_PROC0_SECURE0");
        AddRegister(0x204, "IRQSUMMARY_PROC0_SECURE1");
        AddRegister(0x208, "IRQSUMMARY_PROC0_NONSECURE0");
        AddRegister(0x20c, "IRQSUMMARY_PROC0_NONSECURE1");
        AddRegister(0x210, "IRQSUMMARY_PROC1_SECURE0");
        AddRegister(0x214, "IRQSUMMARY_PROC1_SECURE1");
        AddRegister(0x218, "IRQSUMMARY_PROC1_NONSECURE0");
        AddRegister(0x21c, "IRQSUMMARY_PROC1_NONSECURE1");
        AddRegister(0x220, "IRQSUMMARY_COMA_WAKE_SECURE0");
        AddRegister(0x224, "IRQSUMMARY_COMA_WAKE_SECURE1");
        AddRegister(0x228, "IRQSUMMARY_COMA_WAKE_NONSECURE0");
        AddRegister(0x22c, "IRQSUMMARY_COMA_WAKE_NONSECURE1");

        // ── INTR — raw interrupt status, write-1-to-clear edge bits ───────
        for (int i = 0; i < 6; i++)
        {
            int idx = i;
            AddRegister((uint)(0x230 + idx * 4), $"INTR{idx}")
                .OnRead(() => m_Intr[idx])
                .OnWrite(v => { m_Intr[idx] &= ~(v & EdgeBitMask); UpdateIrq(); });
        }

        // ── PROC0_INTE — interrupt enable ─────────────────────────────────
        for (int i = 0; i < 6; i++)
        {
            int idx = i;
            AddRegister((uint)(0x248 + idx * 4), $"PROC0_INTE{idx}")
                .OnRead(() => m_Proc0Inte[idx])
                .OnWrite(v => { m_Proc0Inte[idx] = v; UpdateIrq(); });
        }

        // ── PROC0_INTF — interrupt force ──────────────────────────────────
        for (int i = 0; i < 6; i++)
        {
            int idx = i;
            AddRegister((uint)(0x260 + idx * 4), $"PROC0_INTF{idx}")
                .OnRead(() => m_Proc0Intf[idx])
                .OnWrite(v => { m_Proc0Intf[idx] = v; UpdateIrq(); });
        }

        // ── PROC0_INTS — interrupt status after masking (read-only) ───────
        for (int i = 0; i < 6; i++)
        {
            int idx = i;
            AddRegister((uint)(0x278 + idx * 4), $"PROC0_INTS{idx}")
                .OnRead(() => (m_Intr[idx] | m_Proc0Intf[idx]) & m_Proc0Inte[idx]);
        }

        // ── PROC1 / DORMANT_WAKE (stubs) ──────────────────────────────────
        for (uint i = 0; i <= 5; i++) AddRegister(0x290 + (i * 4), $"PROC1_INTE{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x2a8 + (i * 4), $"PROC1_INTF{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x2c0 + (i * 4), $"PROC1_INTS{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x2d8 + (i * 4), $"DORMANT_WAKE_INTE{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x2f0 + (i * 4), $"DORMANT_WAKE_INTF{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x308 + (i * 4), $"DORMANT_WAKE_INTS{i}");
    }

    private void OnGpioInput(int gpio, TimeSpan time, bool value)
    {
        int reg   = gpio / 8;
        int shift = (gpio % 8) * 4;

        // Update LEVEL bits (live, not sticky).
        uint levelMask = 0b11u  << shift;
        uint levelBits = (value ? 0b10u : 0b01u) << shift;
        m_Intr[reg] = (m_Intr[reg] & ~levelMask) | levelBits;

        // Set EDGE bit on transitions only.
        bool? prev = m_GpioPrevInput[gpio];
        if (prev.HasValue && prev.Value != value)
        {
            uint edgeBit = (value ? 0b1000u : 0b0100u) << shift;
            m_Intr[reg] |= edgeBit;
        }

        m_GpioPrevInput[gpio] = value;
        UpdateIrq();
    }

    private void UpdateIrq()
    {
        bool pending = false;
        for (int i = 0; i < 6; i++)
        {
            if (((m_Intr[i] | m_Proc0Intf[i]) & m_Proc0Inte[i]) != 0)
            {
                pending = true;
                break;
            }
        }
        m_Csr.Meipa.SetHardwarePending(Irq.IO_IRQ_BANK0, pending);
    }
}
