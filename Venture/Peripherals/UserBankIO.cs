using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class UserBankIO : PeripheralBase, IGpioSource
{
    private readonly MuxedGpioLine[] m_GpioLines = Enumerable.Range(0, 48).Select(x => new MuxedGpioLine()).ToArray();

    public GpioControl[] GpioControl { get; }

    public int GpioLineCount => 48;

    public UserBankIO(uint baseAddress, string name, ILogger<UserBankIO> logger, SIO sio) : base(baseAddress, name, logger)
    {
        for (int i = 0; i < 48; i++)
        {
            m_GpioLines[i].Add(0x05, "SIO", sio.GetGpioLine(i));
        }

        GpioControl = new GpioControl[48];
        for (uint i = 0; i <= 47; i++)
        {
            AddRegister(i * 8, $"GPIO{i}_STATUS");
            GpioControl[i] = new GpioControl(AddRegister(i * 8 + 4, $"GPIO{i}_CTRL"), m_GpioLines[i]);
        }

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

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x230 + (i * 4), $"INTR{i}");
        }

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x248 + (i * 4), $"PROC0_INTE{i}");
        }

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x260 + (i * 4), $"PROC0_INTF{i}");
        }

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x278 + (i * 4), $"PROC0_INTS{i}");
        }

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x290 + (i * 4), $"PROC1_INTE{i}");
        }

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x2a8 + (i * 4), $"PROC1_INTF{i}");
        }

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x2c0 + (i * 4), $"PROC1_INTS{i}");
        }

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x2d8 + (i * 4), $"DORMANT_WAKE_INTE{i}");
        }

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x2f0 + (i * 4), $"DORMANT_WAKE_INTF{i}");
        }

        for (uint i = 0; i <= 5; i++)
        {
            AddRegister(0x308 + (i * 4), $"DORMANT_WAKE_INTS{i}");
        }
    }


    public IGpioLine GetGpioLine(int index)
    {
        return m_GpioLines[index];
    }
}

public class MuxedGpioLine : IGpioLine
{
    private readonly Dictionary<uint, IGpioLine> m_Lines = new();
    private readonly Dictionary<uint, string> m_Names = new();

    private IGpioLine? m_CurrentLine;

    public void Add(ushort function, string name, IGpioLine line)
    {
        m_Lines.Add(function, line);
        m_Names.Add(function, name);
    }

    public string Name => m_Names.GetValueOrDefault(Function, "NULL");

    public uint Function
    {
        get => field;
        set
        {
            if (field != value)
            {
                if (m_CurrentLine != null)
                {
                    m_CurrentLine.Changed -= ChangedHandler;
                }

                var currentDrive = m_CurrentLine?.Value ?? GpioValue.HiZ;
                var newDrive = GpioValue.HiZ;

                if (m_Lines.TryGetValue(value, out var newLine))
                {
                    newDrive = newLine.Value;
                    m_CurrentLine = newLine;
                    newLine.Changed += ChangedHandler;
                }
                else
                {
                    m_CurrentLine = null;
                }

                field = value;

                if (currentDrive != newDrive)
                {
                    Changed?.Invoke(newDrive);
                }

                FunctionChanged?.Invoke();
            }
        }
    }

    private void ChangedHandler(GpioValue drive)
    {
        Changed?.Invoke(drive);
    }

    public GpioValue Value
    {
        get
        {
            return m_CurrentLine == null ? GpioValue.HiZ : m_CurrentLine.Value;
        }
    }

    public event Action<GpioValue>? Changed;
    public event Action? FunctionChanged;
}

public class GpioControl
{
    private readonly MuxedGpioLine m_MuxedGpioLine;

    public enum Function : ushort
    {
        JTAG_TCK = 0x00,
        SPI0_RX = 0x01,
        UART0_TX = 0x02,
        I2C0_SDA = 0x03,
        PWM_A_0 = 0x04,
        SIO_0 = 0x05,
        PIO0_0 = 0x06,
        PIO1_0 = 0x07,
        PIO2_0 = 0x08,
        XIP_SS_N_1 = 0x09,
        USB_MUXING_OVERCURR_DETECT = 0x0a,
        NULL = 0x1f,
    }

    public enum OutOver
    {
        /// <summary>
        /// drive output from peripheral signal selected by funcsel
        /// </summary>
        NORMAL = 0x0,
        /// <summary>
        /// drive output from inverse of peripheral signal selected by funcsel
        /// </summary>
        INVERT = 0x1,
        /// <summary>
        /// drive output low
        /// </summary>
        LOW = 0x2,
        /// <summary>
        /// drive output high
        /// </summary>
        HIGH = 0x3,
    }

    public enum OeOver
    {
        /// <summary>
        /// drive output enable from peripheral signal selected by funcsel
        /// </summary>
        NORMAL = 0x0,
        /// <summary>
        /// drive output enable from inverse of peripheral signal selected by funcsel
        /// </summary>
        INVERT = 0x1,
        /// <summary>
        /// disable output
        /// </summary>
        DISABLE = 0x2,
        /// <summary>
        /// enable output
        /// </summary>
        ENABLE = 0x3,
    }

    public enum InOver
    {
        /// <summary>
        /// don’t invert the peri input
        /// </summary>
        NORMAL = 0x0,
        /// <summary>
        /// invert the peri input
        /// </summary>
        INVERT = 0x1,
        /// <summary>
        /// drive peri input low
        /// </summary>
        LOW = 0x2,
        /// <summary>
        /// drive peri input high
        /// </summary>
        HIGH = 0x3,
    }

    public enum IrqOver
    {
        /// <summary>
        /// don’t invert the interrupt
        /// </summary>
        NORMAL = 0x0,
        /// <summary>
        /// invert the interrupt
        /// </summary>
        INVERT = 0x1,
        /// <summary>
        /// drive interrupt low
        /// </summary>
        LOW = 0x2,
        /// <summary>
        /// drive interrupt high
        /// </summary>
        HIGH = 0x3,
    }

    /// <summary>
    /// selects pin function according to the gpio table
    /// </summary>
    public uint FUNCSEL
    {
        get { return field; } 
        set 
        {
            m_MuxedGpioLine.Function = value;
            field = value; 
        }
    }

    public OutOver OUTOVER { get; set; } = OutOver.NORMAL;

    public OeOver OEOVER { get; set; } = OeOver.NORMAL;

    public InOver INOVER { get; set; } = InOver.NORMAL;

    public IrqOver IRQOVER { get; set; } = IrqOver.NORMAL;

    public GpioControl(Register32 reg, MuxedGpioLine muxedGpioLine)
    {
        reg.Field(28, 2, () => IRQOVER, value => IRQOVER = value);
        reg.Field(16, 2, () => INOVER, value => INOVER = value);
        reg.Field(14, 2, () => OEOVER, value => OEOVER = value);
        reg.Field(12, 2, () => OUTOVER, value => OUTOVER = value);
        reg.Field(0, 5, () => FUNCSEL, value => FUNCSEL = value);
        this.m_MuxedGpioLine = muxedGpioLine;
        
        FUNCSEL = 0x1f; /// this default value represents null
    }
}