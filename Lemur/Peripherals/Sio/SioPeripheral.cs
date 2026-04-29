using System;
using Lemur.Peripherals.PadControl;
using Microsoft.Extensions.Logging;

namespace Lemur.Peripherals.Sio;

public class SioPeripheral : PeripheralBase
{
    // GpioLo[i] → GPIO_OUT bit i / OE bit i  (GPIO0..31)
    // GpioHi[i] → GPIO_HI_OUT bit i / OE bit i, indexed by bit position:
    //   [0..15]  = GPIO32..47   — registered in UserBankIOPeripheral (funcsel 5)
    //   [16..23] = reserved     — never connected
    //   [24..31] = USB/QSPI    — registered in IoQspiPeripheral (funcsel 5)
    public SioGpioFunction[] GpioLo { get; } = new SioGpioFunction[32];
    public SioGpioFunction[] GpioHi { get; } = new SioGpioFunction[32];

    private uint m_OutLo, m_OutHi, m_OeLo, m_OeHi;

    private readonly IElapsedTime m_ElapsedTime;

    // Pad-level signal lines for GPIO_IN / GPIO_HI_IN.
    // Per spec §9.8: input registers read pad state regardless of funcsel.
    // Populated by RegisterPadControls() after all peripherals are constructed.
    private readonly SignalLine?[] m_LoLines = new SignalLine?[32];  // GPIO_IN  bits 0–31
    private readonly SignalLine?[] m_HiLines = new SignalLine?[32];  // GPIO_HI_IN bits 0–31

    public SioPeripheral(uint baseAddress, string name, ILogger<SioPeripheral> logger, IElapsedTime elapsedTime)
        : base(baseAddress, name, logger)
    {
        m_ElapsedTime = elapsedTime;

        for (int i = 0; i < 32; i++) GpioLo[i] = new SioGpioFunction();
        for (int i = 0; i < 32; i++) GpioHi[i] = new SioGpioFunction();

        // ── GPIO inputs (read-only) ────────────────────────────────────────
        AddRegister(0x000, "CPUID")      .OnRead(() => 0);
        AddRegister(0x004, "GPIO_IN")    .OnRead(() => ReadLineMask(m_LoLines));
        AddRegister(0x008, "GPIO_HI_IN") .OnRead(() => ReadLineMask(m_HiLines));

        // ── GPIO outputs ───────────────────────────────────────────────────
        AddRegister(0x010, "GPIO_OUT")        .OnWrite(v => UpdateOutLo(v))            .OnRead(() => m_OutLo);
        AddRegister(0x014, "GPIO_HI_OUT")     .OnWrite(v => UpdateOutHi(v))            .OnRead(() => m_OutHi);
        AddRegister(0x018, "GPIO_OUT_SET")    .OnWrite(v => UpdateOutLo(m_OutLo |  v)) .OnRead(() => m_OutLo);
        AddRegister(0x01c, "GPIO_HI_OUT_SET") .OnWrite(v => UpdateOutHi(m_OutHi |  v)) .OnRead(() => m_OutHi);
        AddRegister(0x020, "GPIO_OUT_CLR")    .OnWrite(v => UpdateOutLo(m_OutLo & ~v)) .OnRead(() => m_OutLo);
        AddRegister(0x024, "GPIO_HI_OUT_CLR") .OnWrite(v => UpdateOutHi(m_OutHi & ~v)) .OnRead(() => m_OutHi);
        AddRegister(0x028, "GPIO_OUT_XOR")    .OnWrite(v => UpdateOutLo(m_OutLo ^  v)) .OnRead(() => m_OutLo);
        AddRegister(0x02c, "GPIO_HI_OUT_XOR") .OnWrite(v => UpdateOutHi(m_OutHi ^  v)) .OnRead(() => m_OutHi);

        // ── GPIO output enables ────────────────────────────────────────────
        AddRegister(0x030, "GPIO_OE")         .OnWrite(v => UpdateOeLo(v))             .OnRead(() => m_OeLo);
        AddRegister(0x034, "GPIO_HI_OE")      .OnWrite(v => UpdateOeHi(v))             .OnRead(() => m_OeHi);
        AddRegister(0x038, "GPIO_OE_SET")     .OnWrite(v => UpdateOeLo(m_OeLo |  v))   .OnRead(() => m_OeLo);
        AddRegister(0x03c, "GPIO_HI_OE_SET")  .OnWrite(v => UpdateOeHi(m_OeHi |  v))  .OnRead(() => m_OeHi);
        AddRegister(0x040, "GPIO_OE_CLR")     .OnWrite(v => UpdateOeLo(m_OeLo & ~v))   .OnRead(() => m_OeLo);
        AddRegister(0x044, "GPIO_HI_OE_CLR")  .OnWrite(v => UpdateOeHi(m_OeHi & ~v))  .OnRead(() => m_OeHi);
        AddRegister(0x048, "GPIO_OE_XOR")     .OnWrite(v => UpdateOeLo(m_OeLo ^  v))   .OnRead(() => m_OeLo);
        AddRegister(0x04c, "GPIO_HI_OE_XOR")  .OnWrite(v => UpdateOeHi(m_OeHi ^  v))  .OnRead(() => m_OeHi);

        // ── Inter-core FIFO ────────────────────────────────────────────────
        AddRegister(0x050, "FIFO_ST");
        AddRegister(0x054, "FIFO_WR");
        AddRegister(0x058, "FIFO_RD");

        // ── Spinlock state ─────────────────────────────────────────────────
        AddRegister(0x05c, "SPINLOCK_ST");

        // ── Interpolators ──────────────────────────────────────────────────
        AddInterpolator(0, 0x080);
        AddInterpolator(1, 0x0c0);

        // ── Spinlocks 0–31 ────────────────────────────────────────────────
        for (uint i = 0; i < 32; i++)
            AddRegister(0x100 + i * 4, $"SPINLOCK{i}");

        // ── Doorbells ─────────────────────────────────────────────────────
        AddRegister(0x180, "DOORBELL_OUT_SET");
        AddRegister(0x184, "DOORBELL_OUT_CLR");
        AddRegister(0x188, "DOORBELL_IN_SET");
        AddRegister(0x18c, "DOORBELL_IN_CLR");

        // ── Security / RISC-V platform ────────────────────────────────────
        AddRegister(0x190, "PERI_NONSEC");
        AddRegister(0x1a0, "RISCV_SOFTIRQ");
        AddRegister(0x1a4, "MTIME_CTRL");
        AddRegister(0x1b0, "MTIME");
        AddRegister(0x1b4, "MTIMEH");
        AddRegister(0x1b8, "MTIMECMP");
        AddRegister(0x1bc, "MTIMECMPH");

        // ── TMDS encoder ──────────────────────────────────────────────────
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

    // Called from RP2350Emulator after all peripherals are constructed.
    // Provides pad-level signal lines so GPIO_IN / GPIO_HI_IN read physical pad state
    // regardless of funcsel, as specified in §9.8.
    public void RegisterPadControls(UserBankPadControl bank0, PadsQSPI qspi)
    {
        // GPIO_IN: bits 0–31 = Bank 0 GPIO 0–31
        for (int i = 0; i < 32; i++)
            m_LoLines[i] = bank0.GetSignalLine(i);

        // GPIO_HI_IN: bits 0–15 = Bank 0 GPIO 32–47
        for (int i = 0; i < 16; i++)
            m_HiLines[i] = bank0.GetSignalLine(32 + i);

        // GPIO_HI_IN: bits 24–31 = IoQspi pins 0–7 (USB DP/DM at 24–25 have no PadsQSPI entry → null → 0)
        for (int i = 0; i < 8; i++)
            m_HiLines[24 + i] = qspi.GetSignalLine(i);
    }

    private void AddInterpolator(int n, uint base_)
    {
        AddRegister(base_ + 0x00, $"INTERP{n}_ACCUM0");
        AddRegister(base_ + 0x04, $"INTERP{n}_ACCUM1");
        AddRegister(base_ + 0x08, $"INTERP{n}_BASE0");
        AddRegister(base_ + 0x0c, $"INTERP{n}_BASE1");
        AddRegister(base_ + 0x10, $"INTERP{n}_BASE2");
        AddRegister(base_ + 0x14, $"INTERP{n}_POP_LANE0");
        AddRegister(base_ + 0x18, $"INTERP{n}_POP_LANE1");
        AddRegister(base_ + 0x1c, $"INTERP{n}_POP_FULL");
        AddRegister(base_ + 0x20, $"INTERP{n}_PEEK_LANE0");
        AddRegister(base_ + 0x24, $"INTERP{n}_PEEK_LANE1");
        AddRegister(base_ + 0x28, $"INTERP{n}_PEEK_FULL");
        AddRegister(base_ + 0x2c, $"INTERP{n}_CTRL_LANE0");
        AddRegister(base_ + 0x30, $"INTERP{n}_CTRL_LANE1");
        AddRegister(base_ + 0x34, $"INTERP{n}_ACCUM0_ADD");
        AddRegister(base_ + 0x38, $"INTERP{n}_ACCUM1_ADD");
        AddRegister(base_ + 0x3c, $"INTERP{n}_BASE_1AND0");
    }

    private void UpdateOutLo(uint v) { m_OutLo = v; ApplyAll(GpioLo, m_OutLo, m_OeLo); }
    private void UpdateOutHi(uint v) { m_OutHi = v; ApplyAll(GpioHi, m_OutHi, m_OeHi); }
    private void UpdateOeLo(uint v)  { m_OeLo  = v; ApplyAll(GpioLo, m_OutLo, m_OeLo); }
    private void UpdateOeHi(uint v)  { m_OeHi  = v; ApplyAll(GpioHi, m_OutHi, m_OeHi); }

    private void ApplyAll(SioGpioFunction[] functions, uint out_, uint oe)
    {
        var time = m_ElapsedTime.Now;
        for (int i = 0; i < 32; i++)
        {
            bool driven = ((oe   >> i) & 1) != 0;
            bool level  = ((out_ >> i) & 1) != 0;
            functions[i].Apply(time, driven ? level : null);
        }
    }

    private static uint ReadLineMask(SignalLine?[] lines)
    {
        uint result = 0;
        for (int i = 0; i < 32; i++)
            if (lines[i]?.State == true) result |= 1u << i;
        return result;
    }
}
