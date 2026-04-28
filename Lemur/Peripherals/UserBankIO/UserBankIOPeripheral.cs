using Lemur.Peripherals.Uart;
using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals.UserBankIO;

public class UserBankIOPeripheral : PeripheralBase
{
    private const uint FuncselUart    = 0x02;
    private const uint FuncselUartAlt = 0x0b;

    public MuxedGpioFunction[] GpioMux { get; }
    public int GpioLineCount => 48;

    public UserBankIOPeripheral(uint baseAddress, string name, ILogger<UserBankIOPeripheral> logger,
        UART0 uart0, UART1 uart1, IElapsedTime elapsedTime) : base(baseAddress, name, logger)
    {
        GpioMux = new MuxedGpioFunction[48];
        for (uint i = 0; i < 48; i++)
        {
            AddRegister(i * 8,     $"GPIO{i}_STATUS");
            GpioMux[i] = new MuxedGpioFunction(AddRegister(i * 8 + 4, $"GPIO{i}_CTRL"), elapsedTime);
        }

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

        // ── IRQ summary registers ─────────────────────────────────────────
        AddRegister(0x200, "IRQSUMMARY_PROC0_SECURE0");
        AddRegister(0x204, "IRQSUMMARY_PROC0_SECURE1");
        AddRegister(0x208, "IRQSUMMARY_PROC0_NONSECURE0");
        AddRegister(0x20c, "IRQSUMMARY_PROC0_NONSECURE1");
        AddRegister(0x210, "IRQSUMMARY_PROC1_SECURE0");
        AddRegister(0x214, "IRQSUMMARY_PROC1_SECURE1");
        AddRegister(0x218, "IRQSUMMARY_PROC1_NONSECURE0");
        AddRegister(0x21c, "IRQSUMMARY_PROC1_NONSECURE1");
        AddRegister(0x220, "IRQSUMMARY_COMA_WAKE_SECURE");
        AddRegister(0x224, "IRQSUMMARY_COMA_WAKE_SECURE");
        AddRegister(0x228, "IRQSUMMARY_COMA_WAKE_NONSECURE0");
        AddRegister(0x22c, "IRQSUMMARY_COMA_WAKE_NONSECURE1");

        for (uint i = 0; i <= 5; i++) AddRegister(0x230 + (i * 4), $"INTR{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x248 + (i * 4), $"PROC0_INTE{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x260 + (i * 4), $"PROC0_INTF{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x278 + (i * 4), $"PROC0_INTS{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x290 + (i * 4), $"PROC1_INTE{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x2a8 + (i * 4), $"PROC1_INTF{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x2c0 + (i * 4), $"PROC1_INTS{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x2d8 + (i * 4), $"DORMANT_WAKE_INTE{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x2f0 + (i * 4), $"DORMANT_WAKE_INTF{i}");
        for (uint i = 0; i <= 5; i++) AddRegister(0x308 + (i * 4), $"DORMANT_WAKE_INTS{i}");
    }
}
