using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class UserBankPadControl : PeripheralBase, IGpioSource
{
    private readonly UserBankIO m_UserBankIO;

    public PadControl[] Pads { get; }
    public PadControl SWCLK { get; }
    public PadControl SWD { get; }
    public Voltage VoltageSelect { get; set; }

    public UserBankPadControl(uint baseAddress, string name, ILogger<UserBankPadControl> logger, UserBankIO userBankIO) : base(baseAddress, name, logger)
    {
        AddRegister(0x00, "VOLTAGE_SELECT")
            .Field(0, 1, () => VoltageSelect, value => VoltageSelect = value);

        Pads = new PadControl[48];
        for (uint i = 0; i < Pads.Length; i++)
        {
            Pads[i] = new PadControl(AddRegister(4 + (i * 4), $"GPIO{i}"));
        }

        SWCLK = new PadControl(AddRegister(0xc4, "SWCLK"));
        SWD = new PadControl(AddRegister(0xc8, "SWD"));

        m_UserBankIO = userBankIO;
    }

    public IGpioLine GetGpioLine(int index)
    {
        return m_UserBankIO.GetGpioLine(index);
    }

    public enum Voltage : byte
    {
        V3_3 = 0x0,
        V1_8 = 0x1,
    }
}

public class PadControl
{
    public enum Drive : byte
    {
        Drive2MA = 0x0,
        Drive4MA = 0x1,
        Drive8MA = 0x2,
        Drive12MA = 0x3,
    }

    public PadControl(Register32 reg)
    {
        reg.Field(8, () => IsolationControl, value => IsolationControl = value)
           .Field(7, () => OutputDisable, value => OutputDisable = value)
           .Field(6, () => InputEnable, value => InputEnable = value)
           .Field(4, 2, () => DriveStrenght, value => DriveStrenght = value)
           .Field(3, () => PullUpEnable, value => PullUpEnable = value)
           .Field(2, () => PullDownEnable, value => PullDownEnable = value)
           .Field(1, () => SlewRateFast, value => SlewRateFast = value)
           .Field(0, () => SlewRateFast, value => SlewRateFast = value);
    }

    /// <summary>
    /// SLEWFAST: Slew rate control. true = Fast, false = Slow
    /// </summary>
    public bool SlewRateFast { get; set; } = false;

    /// <summary>
    /// SCHMITT: Enable schmitt trigger
    /// </summary>
    public bool EnableSchmittTrigger { get; set; } = true;

    /// <summary>
    /// PDE: Pull down enable
    /// </summary>
    public bool PullDownEnable { get; set; } = true;

    /// <summary>
    /// PUE: Pull up enable
    /// </summary>
    public bool PullUpEnable { get; set; } = false;

    /// <summary>
    /// DRIVE: Drive strength
    /// </summary>
    public Drive DriveStrenght { get; set; } = Drive.Drive4MA;

    /// <summary>
    /// IE: Input enable
    /// </summary>
    public bool InputEnable { get; set; } = false;

    /// <summary>
    /// OD: Output disable. Has priority over output enable from peripherals
    /// </summary>
    public bool OutputDisable { get; set; } = false;

    /// <summary>
    /// ISO: Pad isolation control. Remove this once the pad is configured by software
    /// </summary>
    public bool IsolationControl { get; set; } = true;
}