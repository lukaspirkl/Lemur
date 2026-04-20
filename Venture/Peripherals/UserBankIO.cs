using Microsoft.Extensions.Logging;
using Venture.Csr;



// Alias to avoid ambiguity between the GpioControl class and the GpioControl[] property on UserBankIO.
using GpioInOver = Venture.Peripherals.GpioControl.InOver;

namespace Venture.Peripherals;

public class UserBankIO : PeripheralBase, IGpioSource
{
    private readonly MuxedGpioLine[] m_GpioLines = Enumerable.Range(0, 48).Select(x => new MuxedGpioLine()).ToArray();
    private readonly ManualGpioLine[] m_ManualInputLines = Enumerable.Range(0, 48).Select(_ => new ManualGpioLine()).ToArray();

    public GpioControl[] GpioControl { get; }

    // -------------------------------------------------------------------------
    // GPIO interrupt state
    // Spec: RP2350 Datasheet §9.5 — "Interrupts"
    //
    // Each GPIO pin can generate an interrupt on four events:
    //   LEVEL_LOW  (bit 0 of pin's 4-bit slot) — combinational; pin is currently low.
    //   LEVEL_HIGH (bit 1) — combinational; pin is currently high.
    //   EDGE_LOW   (bit 2) — latched; set on High→Low transition; write-1-to-clear.
    //   EDGE_HIGH  (bit 3) — latched; set on Low→High transition; write-1-to-clear.
    //
    // Register layout: 8 pins per 32-bit INTR register, 4 bits per pin.
    //   INTR0: pins  0– 7  (offset 0x230)
    //   INTR1: pins  8–15  (offset 0x234)
    //   INTR2: pins 16–23  (offset 0x238)
    //   INTR3: pins 24–31  (offset 0x23c)
    //   INTR4: pins 32–39  (offset 0x240)
    //   INTR5: pins 40–47  (offset 0x244)
    //
    // For each interrupt destination (proc 0, proc 1, dormant_wake) there are three
    // register arrays:
    //   INTE — enable mask (software configurable).
    //   INTF — force bits  (software can force interrupts for testing).
    //   INTS — status      = (INTR | INTF) & INTE  (read-only; hardware computed).
    //
    // If any bit in any PROC0_INTS[0..5] register is set, IO_IRQ_BANK0 (IRQ 21) is
    // asserted. It de-asserts when all INTS bits go to zero.
    // PROC1 uses the same logic but would target a core-1 IRQ (not connected yet).
    //
    // Edge bit mask within a 32-bit INTR word:
    //   Each pin occupies a 4-bit nibble: [EDGE_HIGH | EDGE_LOW | LEVEL_HIGH | LEVEL_LOW]
    //   Edge bits are bits 2,3 of each nibble → mask = 0xCCCC_CCCC
    // -------------------------------------------------------------------------

    private const int IntrRegCount = 6;
    private const int PinsPerIntrReg = 8;
    private const uint EdgeBitMask = 0xCCCC_CCCC; // bits 2,3 of every 4-bit nibble

    // Latched edge interrupt bits. Only bits matching EdgeBitMask are meaningful.
    private readonly uint[] m_IntrEdgeLatch = new uint[IntrRegCount];

    // Previous pin values for edge detection (true = High).
    private readonly bool[] m_PrevPinValues = new bool[48];

    // Per-destination interrupt enable and force registers.
    private readonly uint[] m_Proc0Inte = new uint[IntrRegCount];
    private readonly uint[] m_Proc0Intf = new uint[IntrRegCount];
    private readonly uint[] m_Proc1Inte = new uint[IntrRegCount];
    private readonly uint[] m_Proc1Intf = new uint[IntrRegCount];

    private readonly CsrController m_CsrController;

    public UserBankIO(uint baseAddress, string name, ILogger<UserBankIO> logger, SIO sio, CsrController csrController)
        : base(baseAddress, name, logger)
    {
        m_CsrController = csrController;

        for (int i = 0; i < 48; i++)
        {
            m_GpioLines[i].Add(0x05, "SIO", sio.GetGpioLine(i));
            m_GpioLines[i].ExternalInput = m_ManualInputLines[i];
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

        // -------------------------------------------------------------------------
        // INTR0..5 — raw interrupt status (0x230–0x244)
        //
        // Read: returns LEVEL bits (combinational from current pin state) ORed with
        //       latched EDGE bits from m_IntrEdgeLatch.
        // Write: write-1-to-clear EDGE bits only (LEVEL bits are read-only).
        //        Level bits (bits 0,1 of each nibble) cannot be cleared by software.
        // -------------------------------------------------------------------------
        for (int i = 0; i < IntrRegCount; i++)
        {
            int idx = i;
            AddRegister(0x230 + (uint)(i * 4), $"INTR{i}")
                .OnRead(() => ReadIntr(idx))
                .OnWrite(v => WriteIntrEdgeClear(idx, v));
        }

        // -------------------------------------------------------------------------
        // PROC0_INTE0..5 — proc 0 interrupt enable (0x248–0x25c)
        // PROC0_INTF0..5 — proc 0 interrupt force   (0x260–0x274)
        // PROC0_INTS0..5 — proc 0 interrupt status  (0x278–0x28c)  [read-only]
        // -------------------------------------------------------------------------
        for (int i = 0; i < IntrRegCount; i++)
        {
            int idx = i;
            AddRegister(0x248 + (uint)(i * 4), $"PROC0_INTE{i}")
                .OnWrite(v => { m_Proc0Inte[idx] = v; UpdateProc0Irq(); });
            AddRegister(0x260 + (uint)(i * 4), $"PROC0_INTF{i}")
                .OnWrite(v => { m_Proc0Intf[idx] = v; UpdateProc0Irq(); });
            AddRegister(0x278 + (uint)(i * 4), $"PROC0_INTS{i}")
                .OnRead(() => ReadProc0Ints(idx));
        }

        // -------------------------------------------------------------------------
        // PROC1_INTE0..5 — proc 1 interrupt enable (0x290–0x2a4)
        // PROC1_INTF0..5 — proc 1 interrupt force   (0x2a8–0x2bc)
        // PROC1_INTS0..5 — proc 1 interrupt status  (0x2c0–0x2d4)  [read-only]
        //
        // These will drive IO_IRQ_BANK0 on core 1 when dual-core support is added.
        // For now the INTS registers are computed correctly but no IRQ is raised.
        // -------------------------------------------------------------------------
        for (int i = 0; i < IntrRegCount; i++)
        {
            int idx = i;
            AddRegister(0x290 + (uint)(i * 4), $"PROC1_INTE{i}")
                .OnWrite(v => { m_Proc1Inte[idx] = v; });
            AddRegister(0x2a8 + (uint)(i * 4), $"PROC1_INTF{i}")
                .OnWrite(v => { m_Proc1Intf[idx] = v; });
            AddRegister(0x2c0 + (uint)(i * 4), $"PROC1_INTS{i}")
                .OnRead(() => ReadProc1Ints(idx));
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

        // Subscribe to each GPIO line's Changed event for edge detection.
        // The MuxedGpioLine raises Changed whenever its selected source changes.
        for (int i = 0; i < 48; i++)
        {
            int pinIdx = i;
            m_GpioLines[i].Changed += value => OnGpioLineChanged(pinIdx, value);
        }

        // Complete the SIO ↔ UserBankIO wiring.
        // SIO is constructed first (DI order), so at this point SIO has no input source yet.
        // Calling this here ensures GPIO_IN reflects live pad values for the lifetime of both objects.
        sio.SetGpioInputSource(this);
    }

    public IGpioLine GetGpioLine(int index) => m_GpioLines[index];

    /// <summary>
    /// Returns the <see cref="ManualGpioLine"/> for the given pin, allowing external code
    /// (e.g. a simulated button in the UI) to drive the pin's input value.
    /// The manual value is visible in GPIO_IN only when the pin's output-enable is clear.
    /// </summary>
    public ManualGpioLine GetManualInputLine(int index) => m_ManualInputLines[index];

    /// <summary>
    /// Returns the current input value of a GPIO pin after applying the INOVER override
    /// from the pin's GPIO_CTRL register.
    ///
    /// Spec: RP2350 Datasheet §9.3 (GPIO control — INOVER field)
    ///   NORMAL (0) — pass through the pad value unchanged
    ///   INVERT (1) — invert the pad value (High↔Low; HiZ stays HiZ)
    ///   LOW    (2) — force the peripheral input to 0 regardless of pad state
    ///   HIGH   (3) — force the peripheral input to 1 regardless of pad state
    ///
    /// Used by SIO's GPIO_IN / GPIO_HI_IN read path so that firmware which configures
    /// INOVER observes the overridden value when polling the SIO registers.
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
                _              => GpioValue.HiZ, // HiZ has no meaningful inverse
            },
            GpioInOver.LOW  => GpioValue.Low,
            GpioInOver.HIGH => GpioValue.High,
            _               => raw,
        };
    }

    // -------------------------------------------------------------------------
    // GPIO interrupt helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called whenever a GPIO line changes value.
    /// Detects rising/falling edges and sets the corresponding latch bits in
    /// m_IntrEdgeLatch, then re-evaluates the IRQ lines.
    /// </summary>
    private void OnGpioLineChanged(int pinIdx, GpioValue newValue)
    {
        bool isHigh = newValue == GpioValue.High;
        bool wasHigh = m_PrevPinValues[pinIdx];
        m_PrevPinValues[pinIdx] = isHigh;

        int regIdx    = pinIdx / PinsPerIntrReg;
        int bitOffset = (pinIdx % PinsPerIntrReg) * 4;

        if (!wasHigh && isHigh)
        {
            // Rising edge: set EDGE_HIGH latch (bit 3 of this pin's nibble).
            m_IntrEdgeLatch[regIdx] |= 1u << (bitOffset + 3);
        }
        else if (wasHigh && !isHigh)
        {
            // Falling edge: set EDGE_LOW latch (bit 2 of this pin's nibble).
            m_IntrEdgeLatch[regIdx] |= 1u << (bitOffset + 2);
        }

        UpdateProc0Irq();
        // UpdateProc1Irq() — omitted until dual-core support is added.
    }

    /// <summary>
    /// Computes the INTR register for a group of 8 GPIO pins.
    ///
    /// LEVEL bits are combinational (computed from current pad state on every read).
    /// EDGE bits are latched (held in m_IntrEdgeLatch until cleared by software).
    ///
    /// Bit layout per pin (4 bits, starting at bitOffset = (pin % 8) * 4):
    ///   bitOffset+0 = LEVEL_LOW  — pin is currently logic 0
    ///   bitOffset+1 = LEVEL_HIGH — pin is currently logic 1
    ///   bitOffset+2 = EDGE_LOW   — latched High→Low transition
    ///   bitOffset+3 = EDGE_HIGH  — latched Low→High transition
    /// </summary>
    private uint ReadIntr(int regIdx)
    {
        // Start with edge latches; OR in combinational level bits below.
        uint result = m_IntrEdgeLatch[regIdx];

        for (int j = 0; j < PinsPerIntrReg; j++)
        {
            int pinIdx = regIdx * PinsPerIntrReg + j;
            if (pinIdx >= 48) break;

            int bitOffset = j * 4;
            bool isHigh   = ReadInputValue(pinIdx) == GpioValue.High;

            if (!isHigh) result |= 1u << bitOffset;       // LEVEL_LOW  (bit 0)
            if (isHigh)  result |= 1u << (bitOffset + 1); // LEVEL_HIGH (bit 1)
        }

        return result;
    }

    /// <summary>
    /// Handles writes to INTR registers.
    /// Writing 1 to an EDGE bit clears the latch (write-1-to-clear semantics).
    /// Writing 1 to a LEVEL bit is ignored — level bits are read-only combinational.
    ///
    /// Spec §9.5: "The edge interrupts are stored in the INTR register and can be
    /// cleared by writing to the INTR register."
    /// </summary>
    private void WriteIntrEdgeClear(int regIdx, uint value)
    {
        // EdgeBitMask selects bits 2,3 of each 4-bit nibble; ignore level-bit writes.
        m_IntrEdgeLatch[regIdx] &= ~(value & EdgeBitMask);
        UpdateProc0Irq();
        // UpdateProc1Irq() — omitted until dual-core support is added.
    }

    /// <summary>
    /// Computes PROC0_INTS for the given register index.
    /// INTS = (INTR | INTF) &amp; INTE — final masked status seen by core 0's interrupt controller.
    /// </summary>
    private uint ReadProc0Ints(int regIdx) =>
        (ReadIntr(regIdx) | m_Proc0Intf[regIdx]) & m_Proc0Inte[regIdx];

    /// <summary>
    /// Computes PROC1_INTS for the given register index.
    /// </summary>
    private uint ReadProc1Ints(int regIdx) =>
        (ReadIntr(regIdx) | m_Proc1Intf[regIdx]) & m_Proc1Inte[regIdx];

    /// <summary>
    /// Re-evaluates IO_IRQ_BANK0 (IRQ 21) for core 0.
    /// The IRQ is asserted if any bit in any PROC0_INTS register is set,
    /// and de-asserted when all PROC0_INTS registers are zero.
    ///
    /// Spec §9.5: "Each interrupt output has its own array of enable registers (INTE)
    /// which configures which GPIO events cause the interrupt to assert. The interrupt
    /// asserts when at least one enabled event occurs."
    /// </summary>
    private void UpdateProc0Irq()
    {
        bool anyPending = false;
        for (int i = 0; i < IntrRegCount && !anyPending; i++)
            anyPending = ReadProc0Ints(i) != 0;

        m_CsrController.Meipa.SetHardwarePending(CsrController.IO_IRQ_BANK0, anyPending);
    }
}

public class MuxedGpioLine : IGpioLine
{
    private readonly Dictionary<uint, IGpioLine> m_Lines = new();
    private readonly Dictionary<uint, string> m_Names = new();

    private IGpioLine? m_CurrentLine;
    private IGpioLine? m_ExternalInput;

    public void Add(ushort function, string name, IGpioLine line)
    {
        m_Lines.Add(function, line);
        m_Names.Add(function, name);
    }

    /// <summary>
    /// Optional external input that shows through when the selected mux source is HiZ
    /// (i.e., the pin is configured as an input). Used to inject simulated button/switch
    /// signals that firmware can read via GPIO_IN.
    /// </summary>
    public IGpioLine? ExternalInput
    {
        set
        {
            if (m_ExternalInput != null)
                m_ExternalInput.Changed -= OnExternalInputChanged;
            m_ExternalInput = value;
            if (value != null)
                value.Changed += OnExternalInputChanged;
        }
    }

    private void OnExternalInputChanged(GpioValue v)
    {
        // Only propagate when the selected mux source is HiZ (pin is in input mode).
        if ((m_CurrentLine?.Value ?? GpioValue.HiZ) == GpioValue.HiZ)
            Changed?.Invoke(v);
    }

    public string Name => m_Names.GetValueOrDefault(Function, "NULL");

    public uint Function
    {
        get => field;
        set
        {
            if (field != value)
            {
                var oldEffective = Value;

                if (m_CurrentLine != null)
                    m_CurrentLine.Changed -= ChangedHandler;

                if (m_Lines.TryGetValue(value, out var newLine))
                {
                    m_CurrentLine = newLine;
                    newLine.Changed += ChangedHandler;
                }
                else
                {
                    m_CurrentLine = null;
                }

                field = value;

                var newEffective = Value;
                if (oldEffective != newEffective)
                    Changed?.Invoke(newEffective);

                FunctionChanged?.Invoke();
            }
        }
    }

    private void ChangedHandler(GpioValue drive)
    {
        // When transitioning to HiZ, fall back to external input value if available.
        var effective = drive == GpioValue.HiZ ? (m_ExternalInput?.Value ?? GpioValue.HiZ) : drive;
        Changed?.Invoke(effective);
    }

    public GpioValue Value
    {
        get
        {
            var driven = m_CurrentLine?.Value ?? GpioValue.HiZ;
            return driven == GpioValue.HiZ ? (m_ExternalInput?.Value ?? GpioValue.HiZ) : driven;
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
        /// don't invert the peri input
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
        /// don't invert the interrupt
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
