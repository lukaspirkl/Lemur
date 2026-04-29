using System.Linq;
using Lemur.Peripherals.IoQSPI;
using Lemur.Peripherals.PadControl;
using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals;

// Pad control for the USB/QSPI bank (PADS_QSPI_BASE).
// Analogous to UserBankPadControl for Bank 0.
//
// Covers 6 of the 8 IO_QSPI pins — USB DP/DM have their own pad control elsewhere.
// Signal lines are indexed by IoQspiPeripheral.GpioMux index (2–7):
//
//   IoQspi index  Pad register      Reset pull
//   2  QSPI_SCLK  GPIO_QSPI_SCLK   pull-down
//   3  QSPI_SS    GPIO_QSPI_SS      pull-up   ← BOOTSEL pin, high = normal boot
//   4  QSPI_SD0   GPIO_QSPI_SD0     pull-down
//   5  QSPI_SD1   GPIO_QSPI_SD1     pull-down
//   6  QSPI_SD2   GPIO_QSPI_SD2     pull-up
//   7  QSPI_SD3   GPIO_QSPI_SD3     pull-up
public class PadsQSPI : PeripheralBase
{
    // Indexed by IoQspiPeripheral.GpioMux index; slots 0–1 (USB) are null.
    private readonly SignalLine?[] m_SignalLines = new SignalLine?[8];

    public Voltage VoltageSelect { get; set; }

    public PadsQSPI(uint baseAddress, string name, ILogger<PadsQSPI> logger,
        IoQspiPeripheral ioQspi, IElapsedTime elapsedTime)
        : base(baseAddress, name, logger)
    {
        AddRegister(0x00, "VOLTAGE_SELECT")
            .Field(0, 1, () => VoltageSelect, v => VoltageSelect = v);

        // Register order per datasheet: SCLK(2), SD0(4), SD1(5), SD2(6), SD3(7), SS(3)
        // IE=1 at reset for all Bank 1 pads (unlike Bank 0 which defaults IE=0).
        // SCLK/SD0/SD1: pull-down;  SD2/SD3/SS: pull-up.
        AddPad(0x04, "GPIO_QSPI_SCLK", ioQspi, ioQspiIndex: 2, elapsedTime, pullUp: false);
        AddPad(0x08, "GPIO_QSPI_SD0",  ioQspi, ioQspiIndex: 4, elapsedTime, pullUp: false);
        AddPad(0x0c, "GPIO_QSPI_SD1",  ioQspi, ioQspiIndex: 5, elapsedTime, pullUp: false);
        AddPad(0x10, "GPIO_QSPI_SD2",  ioQspi, ioQspiIndex: 6, elapsedTime, pullUp: true);
        AddPad(0x14, "GPIO_QSPI_SD3",  ioQspi, ioQspiIndex: 7, elapsedTime, pullUp: true);
        AddPad(0x18, "GPIO_QSPI_SS",   ioQspi, ioQspiIndex: 3, elapsedTime, pullUp: true);
    }

    // Returns the signal line for the given IoQspiPeripheral.GpioMux index (2–7), or null for USB (0–1).
    public SignalLine? GetSignalLine(int ioQspiIndex) => m_SignalLines[ioQspiIndex];

    private void AddPad(uint offset, string padName, IoQspiPeripheral ioQspi,
        int ioQspiIndex, IElapsedTime elapsedTime, bool pullUp)
    {
        var line = new SignalLine();
        m_SignalLines[ioQspiIndex] = line;
        var reg = AddRegister(offset, padName);
        // IE resets to 1 for Bank 1; pull direction depends on the pin.
        new PadBridge(reg, ioQspi.GpioMux[ioQspiIndex], line, elapsedTime,
            initialInputEnable:    true,
            initialPullUpEnable:   pullUp,
            initialPullDownEnable: !pullUp);
    }

    public enum Voltage : byte
    {
        V3_3 = 0x0,
        V1_8 = 0x1,
    }
}
