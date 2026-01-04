using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class SIO : PeripheralBase, IGpioSource
{
    private readonly GpioLine[] gpioLines = Enumerable.Range(0, 48).Select(x => new GpioLine()).ToArray();

    private readonly bool[] gpioValues = new bool[48];
    private readonly bool[] gpioEnable = new bool[48];

    public IGpioLine GetGpioLine(int index)
    {
        return gpioLines[index];
    }

    public SIO(uint baseAddress, string name, ILogger<SIO> logger) : base(baseAddress, name, logger)
    {
        AddRegister(0x000, "CPUID", 0);

        // TODO: This should read values from the pads
        AddRegister(0x004, "GPIO_IN", 0x02000000);
        AddRegister(0x008, "GPIO_HI_IN", 0xC8000000);

        GpioOutFields(AddRegister(0x010, "GPIO_OUT"), (i, value) => value);
        GpioHiOutFields(AddRegister(0x014, "GPIO_HI_OUT"), (i, value) => value);
        GpioOutFields(AddRegister(0x018, "GPIO_OUT_SET").OnRead(() => 0), (i, value) => value ? true : gpioValues[i]);
        GpioHiOutFields(AddRegister(0x01c, "GPIO_HI_OUT_SET").OnRead(() => 0), (i, value) => value ? true : gpioValues[i]);
        GpioOutFields(AddRegister(0x020, "GPIO_OUT_CLR").OnRead(() => 0), (i, value) => value ? false : gpioValues[i]);
        GpioHiOutFields(AddRegister(0x024, "GPIO_HI_OUT_CLR").OnRead(() => 0), (i, value) => value ? false : gpioValues[i]);
        GpioOutFields(AddRegister(0x028, "GPIO_OUT_XOR").OnRead(() => 0), (i, value) => gpioValues[i] ^ value);
        GpioHiOutFields(AddRegister(0x02c, "GPIO_HI_OUT_XOR").OnRead(() => 0), (i, value) => gpioValues[i] ^ value);

        GpioOeFields(AddRegister(0x030, "GPIO_OE"), (i, value) => value);
        GpioHiOeFields(AddRegister(0x034, "GPIO_HI_OE"), (i, value) => value);
        GpioOeFields(AddRegister(0x038, "GPIO_OE_SET").OnRead(() => 0), (i, value) => value ? true : gpioEnable[i]);
        GpioHiOeFields(AddRegister(0x03c, "GPIO_HI_OE_SET").OnRead(() => 0), (i, value) => value ? true : gpioEnable[i]);
        GpioOeFields(AddRegister(0x040, "GPIO_OE_CLR").OnRead(() => 0), (i, value) => value ? false : gpioEnable[i]);
        GpioHiOeFields(AddRegister(0x044, "GPIO_HI_OE_CLR").OnRead(() => 0), (i, value) => value ? false : gpioEnable[i]);
        GpioOeFields(AddRegister(0x048, "GPIO_OE_XOR").OnRead(() => 0), (i, value) => gpioEnable[i] ^ value);
        GpioHiOeFields(AddRegister(0x04c, "GPIO_HI_OE_XOR").OnRead(() => 0), (i, value) => gpioEnable[i] ^ value);

        AddRegister(0x050, "FIFO_ST");
        AddRegister(0x054, "FIFO_WR");
        AddRegister(0x058, "FIFO_RD");
        AddRegister(0x05c, "SPINLOCK_ST");
        AddRegister(0x080, "INTERP0_ACCUM0");
        AddRegister(0x084, "INTERP0_ACCUM1");
        AddRegister(0x088, "INTERP0_BASE0");
        AddRegister(0x08c, "INTERP0_BASE1");
        AddRegister(0x090, "INTERP0_BASE2");
        AddRegister(0x094, "INTERP0_POP_LANE0");
        AddRegister(0x098, "INTERP0_POP_LANE1");
        AddRegister(0x09c, "INTERP0_POP_FULL");
        AddRegister(0x0a0, "INTERP0_PEEK_LANE0");
        AddRegister(0x0a4, "INTERP0_PEEK_LANE1");
        AddRegister(0x0a8, "INTERP0_PEEK_FULL");
        AddRegister(0x0ac, "INTERP0_CTRL_LANE0");
        AddRegister(0x0b0, "INTERP0_CTRL_LANE1");
        AddRegister(0x0b4, "INTERP0_ACCUM0_ADD");
        AddRegister(0x0b8, "INTERP0_ACCUM1_ADD");
        AddRegister(0x0bc, "INTERP0_BASE_1AND0");
        AddRegister(0x0c0, "INTERP1_ACCUM0");
        AddRegister(0x0c4, "INTERP1_ACCUM1");
        AddRegister(0x0c8, "INTERP1_BASE0");
        AddRegister(0x0cc, "INTERP1_BASE1");
        AddRegister(0x0d0, "INTERP1_BASE2");
        AddRegister(0x0d4, "INTERP1_POP_LANE0");
        AddRegister(0x0d8, "INTERP1_POP_LANE1");
        AddRegister(0x0dc, "INTERP1_POP_FULL");
        AddRegister(0x0e0, "INTERP1_PEEK_LANE0");
        AddRegister(0x0e4, "INTERP1_PEEK_LANE1");
        AddRegister(0x0e8, "INTERP1_PEEK_FULL");
        AddRegister(0x0ec, "INTERP1_CTRL_LANE0");
        AddRegister(0x0f0, "INTERP1_CTRL_LANE1");
        AddRegister(0x0f4, "INTERP1_ACCUM0_ADD");
        AddRegister(0x0f8, "INTERP1_ACCUM1_ADD");
        AddRegister(0x0fc, "INTERP1_BASE_1AND0");

        foreach (uint i in Enumerable.Range(0, 32))
        {
            AddRegister(0x100 + (i * 4), $"SPINLOCK{i}");
        }
        
        AddRegister(0x180, "DOORBELL_OUT_SET");
        AddRegister(0x184, "DOORBELL_OUT_CLR");
        AddRegister(0x188, "DOORBELL_IN_SET");
        AddRegister(0x18c, "DOORBELL_IN_CLR");
        AddRegister(0x190, "PERI_NONSEC");
        AddRegister(0x1a0, "RISCV_SOFTIRQ");
        AddRegister(0x1a4, "MTIME_CTRL");
        AddRegister(0x1b0, "MTIME");
        AddRegister(0x1b4, "MTIMEH");
        AddRegister(0x1b8, "MTIMECMP");
        AddRegister(0x1bc, "MTIMECMPH");
        AddRegister(0x1c0, "TMDS_CTRL");
        AddRegister(0x1c4, "TMDS_WDATA");
        AddRegister(0x1c8, "TMDS_PEEK_SINGLE");
        AddRegister(0x1cc, "TMDS_POP_SINGLE");
        AddRegister(0x1d0, "TMDS_PEEK_DOUBLE_L0");
        AddRegister(0x1d4, "TMDS_POP_DOUBLE_L0");
        AddRegister(0x1d8, "TMDS_PEEK_DOUBLE_L1");
        AddRegister(0x1dc, "TMDS_POP_DOUBLE_L1");
        AddRegister(0x1e0, "TMDS_PEEK_DOUBLE_L2");
        AddRegister(0x1e4, "TMDS_POP_DOUBLE_L2");
    }

    private void GpioOutFields(Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        foreach (var i in Enumerable.Range(0, 32))
        {
            GpioOutField(i, i, reg, computeNewValue);
        }
    }

    private void GpioHiOutFields(Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        foreach (var i in Enumerable.Range(32, 16))
        {
            GpioOutField(i - 32, i, reg, computeNewValue);
        }
    }

    private void GpioOutField(int field, int i, Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        reg.Field(field, () => gpioValues[i], value =>
        {
            gpioValues[i] = computeNewValue(i, value);
            if (gpioEnable[i])
            {
                gpioLines[i].Value = gpioValues[i] ? GpioValue.High : GpioValue.Low;
            }
        });
    }

    private void GpioOeFields(Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        foreach (var i in Enumerable.Range(0, 32))
        {
            GpioOeField(i, i, reg, computeNewValue);
        }
    }

    private void GpioHiOeFields(Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        foreach (var i in Enumerable.Range(32, 16))
        {
            GpioOeField(i - 32, i, reg, computeNewValue);
        }
    }

    private void GpioOeField(int field, int i, Register32 reg, Func<int, bool, bool> computeNewValue)
    {
        reg.Field(field, () => gpioEnable[i], value =>
        {
            gpioEnable[i] = computeNewValue(i, value);
            gpioLines[i].Value = gpioEnable[i] ? (gpioValues[i] ? GpioValue.High : GpioValue.Low) : GpioValue.HiZ;
        });
    }
}
