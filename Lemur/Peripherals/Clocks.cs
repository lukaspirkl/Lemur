using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals;

public class Clocks : PeripheralBase
{
    // ─── Known oscillator / PLL frequencies ──────────────────────────────────

    private const double XOSC_HZ    = 12_000_000.0;
    private const double PLL_SYS_HZ = 150_000_000.0;
    private const double PLL_USB_HZ = 48_000_000.0;
    private const double ROSC_HZ    = 6_000_000.0;
    private const double LPOSC_HZ   = 32_768.0;

    // ─── CLK_REF ──────────────────────────────────────────────────────────────

    enum RefSource
    {
        ROSC_CLKSRC_PH = 0x0,
        CLKSRC_CLK_REF_AUX = 0x1,
        XOSC_CLKSRC = 0x2,
        LPOSC_CLKSRC = 0x3
    }

    private RefSource m_RefSource = RefSource.ROSC_CLKSRC_PH;

    enum RefAuxSource
    {
        CLKSRC_PLL_USB = 0x0,
        CLKSRC_GPIN0 = 0x1,
        CLKSRC_GPIN1 = 0x2,
        CLKSRC_PLL_USB_PRIMARY_REF_OPCG = 0x3
    }

    private RefAuxSource m_RefAuxSource = RefAuxSource.CLKSRC_PLL_USB;

    // ─── CLK_SYS ──────────────────────────────────────────────────────────────

    enum SysSource
    {
        CLK_REF = 0x0,
        CLKSRC_CLK_SYS_AUX = 0x1,
    }

    private SysSource m_SysSource = SysSource.CLK_REF;

    // CLK_SYS_CTRL AUXSRC bits [7:5]:
    //   0 → CLKSRC_PLL_SYS, 1 → CLKSRC_PLL_USB, 2 → ROSC, 3 → XOSC, 4/5 → GPIN
    private uint m_SysAuxSrc;

    // ─── CLK_PERI ─────────────────────────────────────────────────────────────

    // CLK_PERI_CTRL AUXSRC bits [7:5]:
    //   0 → CLK_SYS, 1 → CLKSRC_PLL_SYS, 2 → CLKSRC_PLL_USB,
    //   3 → ROSC_CLKSRC_PH, 4 → XOSC_CLKSRC, 5/6 → GPIN
    private uint m_PeriAuxSrc;
    private bool m_PeriEnabled;

    // CLK_PERI_DIV INT bits [17:16]: 2-bit divisor, 0 → 4, otherwise face value
    private uint m_PeriDiv = 1;

    // ─── Frequency API ────────────────────────────────────────────────────────

    private double ClkRefFrequency => m_RefSource switch
    {
        RefSource.CLKSRC_CLK_REF_AUX => m_RefAuxSource == RefAuxSource.CLKSRC_PLL_USB ? PLL_USB_HZ : XOSC_HZ,
        RefSource.XOSC_CLKSRC        => XOSC_HZ,
        RefSource.LPOSC_CLKSRC       => LPOSC_HZ,
        _                            => ROSC_HZ
    };

    private double ClkSysFrequency => m_SysSource == SysSource.CLKSRC_CLK_SYS_AUX
        ? m_SysAuxSrc switch
        {
            0 => PLL_SYS_HZ,
            1 => PLL_USB_HZ,
            2 => ROSC_HZ,
            3 => XOSC_HZ,
            _ => PLL_SYS_HZ
        }
        : ClkRefFrequency;

    /// <summary>
    /// Current clk_peri frequency in Hz, derived from CLK_PERI_CTRL and CLK_PERI_DIV.
    /// Used by peripherals (e.g. UART) that clock from clk_peri.
    /// </summary>
    public double ClkPeriFrequency
    {
        get
        {
            double src = m_PeriAuxSrc switch
            {
                0 => ClkSysFrequency,
                1 => PLL_SYS_HZ,
                2 => PLL_USB_HZ,
                3 => ROSC_HZ,
                4 => XOSC_HZ,
                _ => ClkSysFrequency
            };
            uint div = m_PeriDiv == 0 ? 4u : m_PeriDiv;
            return src / div;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    public Clocks(uint baseAddress, string name, ILogger<Clocks> logger) : base(baseAddress, name, logger)
    {
        AddRegister(0x30, "CLK_REF_CTRL")
            .Field(5, 2, () => m_RefAuxSource, v => m_RefAuxSource = v)
            .Field(0, 2, () => m_RefSource,    v => m_RefSource    = v);

        AddRegister(0x34, "CLK_REF_DIV", 0x00010000);

        AddRegister(0x38, "CLK_REF_SELECTED")
            .OnRead(() => 1u << (int)m_RefSource);

        AddRegister(0x3c, "CLK_SYS_CTRL")
            .Field(5, 3, () => m_SysAuxSrc,  v => m_SysAuxSrc  = v)
            .Field(0, 1, () => m_SysSource,   v => m_SysSource  = v);

        AddRegister(0x40, "CLK_SYS_DIV", 0x00010000);

        AddRegister(0x44, "CLK_SYS_SELECTED")
            .OnRead(() => 1u << (int)m_SysSource);

        // CLK_PERI_CTRL — AUXSRC at bits [7:5], ENABLE at bit 11
        AddRegister(0x48, "CLK_PERI_CTRL")
            .Field(lsb: 11, getter: () => m_PeriEnabled, setter: v => m_PeriEnabled = v)
            .Field(5, 3, () => m_PeriAuxSrc, v => m_PeriAuxSrc = v);

        // CLK_PERI_DIV — INT at bits [17:16], 2-bit, reset = 1
        AddRegister(0x4c, "CLK_PERI_DIV", 0x00010000)
            .Field(16, 2, () => m_PeriDiv, v => m_PeriDiv = v);

        // CLK_PERI_SELECTED — no glitchless mux, hardwired to 1
        AddRegister(0x50, "CLK_PERI_SELECTED")
            .OnRead(() => 1u);

        AddRegister(0x58, "CLK_HSTX_DIV", 0x00010000);

        AddRegister(0x64, "CLK_USB_DIV", 0x00010000);

        AddRegister(0x70, "CLK_ADC_DIV", 0x00010000);
    }
}
