using Lemur.Peripherals.Sio;
using Lemur.Peripherals.UserBankIO;
using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals.IoQSPI;

// GPIO mux controller for the USB/QSPI pin bank (IO_QSPI_BASE).
// Analogous to UserBankIOPeripheral for Bank 0, but covers 8 pins:
//
//   Index  Signal       SIO number  GPIO_HI_IN bit
//   0      USBPHY_DP    SIO_56      24
//   1      USBPHY_DM    SIO_57      25
//   2      QSPI_SCLK    SIO_58      26
//   3      QSPI_SS      SIO_59      27
//   4      QSPI_SD0     SIO_60      28
//   5      QSPI_SD1     SIO_61      29
//   6      QSPI_SD2     SIO_62      30
//   7      QSPI_SD3     SIO_63      31
public class IoQspiPeripheral : PeripheralBase
{
    private const uint FuncselSio = 0x05;

    private static readonly string[] PinNames =
    [
        "USBPHY_DP", "USBPHY_DM",
        "GPIO_QSPI_SCLK", "GPIO_QSPI_SS",
        "GPIO_QSPI_SD0",  "GPIO_QSPI_SD1", "GPIO_QSPI_SD2", "GPIO_QSPI_SD3",
    ];

    public MuxedGpioFunction[] GpioMux { get; } = new MuxedGpioFunction[8];

    public IoQspiPeripheral(uint baseAddress, string name, ILogger<IoQspiPeripheral> logger,
        SioPeripheral sio, IElapsedTime elapsedTime)
        : base(baseAddress, name, logger)
    {
        for (uint i = 0; i < 8; i++)
        {
            AddRegister(i * 8,     $"{PinNames[i]}_STATUS");
            GpioMux[i] = new MuxedGpioFunction(AddRegister(i * 8 + 4, $"{PinNames[i]}_CTRL"), elapsedTime);
        }

        // SIO F5 — GpioHi[24..31] = SIO_56..63
        for (uint i = 0; i < 8; i++)
            GpioMux[i].Add(FuncselSio, $"SIO_{56 + i}", sio.GpioHi[24 + i]);

        AddRegister(0x200, "IRQSUMMARY_PROC0_SECURE");
        AddRegister(0x204, "IRQSUMMARY_PROC0_NONSECURE");
        AddRegister(0x208, "IRQSUMMARY_PROC1_SECURE");
        AddRegister(0x20c, "IRQSUMMARY_PROC1_NONSECURE");
        AddRegister(0x210, "IRQSUMMARY_COMA_WAKE_SECURE");
        AddRegister(0x214, "IRQSUMMARY_COMA_WAKE_NONSECURE");
        AddRegister(0x218, "INTR");
        AddRegister(0x21c, "PROC0_INTE");
        AddRegister(0x220, "PROC0_INTF");
        AddRegister(0x224, "PROC0_INTS");
        AddRegister(0x228, "PROC1_INTE");
        AddRegister(0x22c, "PROC1_INTF");
        AddRegister(0x230, "PROC1_INTS");
        AddRegister(0x234, "DORMANT_WAKE_INTE");
        AddRegister(0x238, "DORMANT_WAKE_INTF");
        AddRegister(0x23c, "DORMANT_WAKE_INTS");
    }
}
