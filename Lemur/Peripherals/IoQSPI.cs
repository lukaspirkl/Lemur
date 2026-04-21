using Microsoft.Extensions.Logging;
using GpioInOver = Lemur.Peripherals.GpioControl.InOver;

namespace Lemur.Peripherals;

// RP2350 Datasheet §9.4 — "IO_QSPI"
//
// Manages the 6 dedicated QSPI GPIO pins:
//   0  QSPI_SCLK  — flash clock
//   1  QSPI_SS_N  — flash chip-select (active-low; doubles as BOOTSEL)
//   2  QSPI_SD0   — flash data 0
//   3  QSPI_SD1   — flash data 1
//   4  QSPI_SD2   — flash data 2
//   5  QSPI_SD3   — flash data 3
//
// Register layout mirrors IO_BANK0: each pin has a STATUS word followed by
// a CTRL word (FUNCSEL / OUTOVER / OEOVER / INOVER / IRQOVER).
//
// FUNCSEL values for the QSPI bank:
//   0x00  XIP   — driven by the flash controller (default at power-on)
//   0x05  SIO   — driven by software via SIO GPIO_HI_OUT
//   0x1f  NULL  — pin disconnected
//
// SIO reads QSPI input values via GPIO_HI_IN bits [31:26]:
//   bit 26  QSPI_SCLK
//   bit 27  QSPI_SS_N
//   bit 28  QSPI_SD0
//   bit 29  QSPI_SD1
//   bit 30  QSPI_SD2
//   bit 31  QSPI_SD3
//
// At power-on the flash controller holds the pins at their idle state:
//   QSPI_SCLK = LOW   (clock idle)
//   QSPI_SS_N = HIGH  (chip-select deasserted; on-board pull-up)
//   QSPI_SD0  = LOW
//   QSPI_SD1  = LOW
//   QSPI_SD2  = HIGH  (on-board pull-up)
//   QSPI_SD3  = HIGH  (on-board pull-up)
// This gives GPIO_HI_IN = 0xC8000000 with no software overrides.
//
// The idle-line source is registered at FUNCSEL=0x00 (XIP). GpioControl
// normally defaults FUNCSEL to 0x1f (NULL), so the constructor overrides
// it to 0x00 immediately after creating each GpioControl instance.

public class IoQSPI : PeripheralBase
{
    // Pin indices — kept public so external code (e.g. tests) can reference them by name.
    public const int SCLK = 0;
    public const int SS_N = 1;
    public const int SD0  = 2;
    public const int SD1  = 3;
    public const int SD2  = 4;
    public const int SD3  = 5;
    private const int PinCount = 6;

    private static readonly string[] s_PinNames =
        ["QSPI_SCLK", "QSPI_SS_N", "QSPI_SD0", "QSPI_SD1", "QSPI_SD2", "QSPI_SD3"];

    // Idle-state values driven by the flash controller when XIP is not
    // actively transferring. SS_N, SD2 and SD3 have on-board pull-ups.
    private static readonly GpioValue[] s_XipIdleValues =
    [
        GpioValue.Low,   // QSPI_SCLK — idle low
        GpioValue.High,  // QSPI_SS_N — deasserted / BOOTSEL not pressed
        GpioValue.Low,   // QSPI_SD0
        GpioValue.Low,   // QSPI_SD1
        GpioValue.High,  // QSPI_SD2  — pull-up
        GpioValue.High,  // QSPI_SD3  — pull-up
    ];

    private readonly MuxedGpioLine[] m_GpioLines = new MuxedGpioLine[PinCount];
    public GpioControl[] GpioControl { get; }

    public IoQSPI(uint baseAddress, string name, ILogger<IoQSPI> logger, SIO sio)
        : base(baseAddress, name, logger)
    {
        GpioControl = new GpioControl[PinCount];

        for (int i = 0; i < PinCount; i++)
        {
            m_GpioLines[i] = new MuxedGpioLine();

            // Idle-state line for FUNCSEL=0x00 (XIP). Provides the correct
            // default input levels that the bootrom expects to read via GPIO_HI_IN
            // before firmware reconfigures these pins.
            var xipIdleLine = new GpioLine { Value = s_XipIdleValues[i] };
            m_GpioLines[i].Add(0x00, "XIP", xipIdleLine);
        }

        for (uint i = 0; i < PinCount; i++)
        {
            AddRegister(i * 8,     $"GPIO_{s_PinNames[i]}_STATUS");
            GpioControl[i] = new GpioControl(
                AddRegister(i * 8 + 4, $"GPIO_{s_PinNames[i]}_CTRL"),
                m_GpioLines[i]);

            // GpioControl defaults FUNCSEL to 0x1f (NULL). Override to 0x00 (XIP)
            // so the idle-state line above is selected from power-on.
            GpioControl[i].FUNCSEL = 0x00;
        }

        // Wire into SIO so that GPIO_HI_IN bits [31:26] reflect live QSPI pin states.
        // SIO is constructed first (DI order), so SetQspiInputSource completes the wiring.
        sio.SetQspiInputSource(this);
    }

    /// <summary>
    /// Returns the current input value for a QSPI pin after applying the INOVER
    /// override from the pin's CTRL register. Mirrors UserBankIO.ReadInputValue.
    /// </summary>
    public GpioValue ReadInputValue(int pin)
    {
        var raw = m_GpioLines[pin].Value;
        return GpioControl[pin].INOVER switch
        {
            GpioInOver.NORMAL => raw,
            GpioInOver.INVERT => raw switch
            {
                GpioValue.High => GpioValue.Low,
                GpioValue.Low  => GpioValue.High,
                _              => GpioValue.HiZ,
            },
            GpioInOver.LOW  => GpioValue.Low,
            GpioInOver.HIGH => GpioValue.High,
            _               => raw,
        };
    }
}
