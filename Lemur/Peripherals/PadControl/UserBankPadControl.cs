using Lemur.Peripherals.UserBankIO;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Lemur.Peripherals.PadControl;

public class UserBankPadControl : PeripheralBase
{
    const int LINE_COUNT = 48;

    private readonly SignalLine[] m_SignalLines = Enumerable.Range(0, LINE_COUNT).Select(_ => new SignalLine()).ToArray();
    private readonly PadBridge[]  m_Bridges     = new PadBridge[LINE_COUNT];

    public Voltage VoltageSelect { get; set; }

    public UserBankPadControl(uint baseAddress, string name, ILogger<UserBankPadControl> logger,
        UserBankIOPeripheral userBankIO, IElapsedTime elapsedTime) : base(baseAddress, name, logger)
    {
        AddRegister(0x00, "VOLTAGE_SELECT")
            .Field(0, 1, () => VoltageSelect, value => VoltageSelect = value);

        for (uint i = 0; i < LINE_COUNT; i++)
        {
            var reg = AddRegister(4 + (i * 4), $"GPIO{i}");
            m_Bridges[i] = new PadBridge(reg, userBankIO.GpioMux[i], m_SignalLines[i], elapsedTime);
        }

        AddRegister(0xc4, "SWCLK");
        AddRegister(0xc8, "SWD");
    }

    public SignalLine GetSignalLine(int index) => m_SignalLines[index];

    public enum Voltage : byte
    {
        V3_3 = 0x0,
        V1_8 = 0x1,
    }
}
