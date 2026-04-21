using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Lemur.Peripherals;

public class Timer0 : Timer
{
    public Timer0(uint baseAddress, string name, ILogger<Timer0> logger) : base(baseAddress, name, logger)
    {
    }
}

public class Timer1 : Timer
{
    public Timer1(uint baseAddress, string name, ILogger<Timer1> logger) : base(baseAddress, name, logger)
    {
    }
}


public class Timer : PeripheralBase
{
    private Stopwatch m_Timer;

    public Timer(uint baseAddress, string name, ILogger<Timer> logger) : base(baseAddress, name, logger)
    {
        m_Timer = Stopwatch.StartNew();

        AddRegister(0x00, "TIMEHW");
        AddRegister(0x04, "TIMELW");
        AddRegister(0x08, "TIMEHR");
        AddRegister(0x0c, "TIMELR");
        AddRegister(0x10, "ALARM0");
        AddRegister(0x14, "ALARM1");
        AddRegister(0x18, "ALARM2");
        AddRegister(0x1c, "ALARM3");
        AddRegister(0x20, "ARMED");
        AddRegister(0x24, "TIMERAWH").OnRead(() => (uint)((ulong)GetMicroseconds() >> 32));
        AddRegister(0x28, "TIMERAWL").OnRead(() => (uint)(GetMicroseconds() & 0xFFFFFFFF));
        AddRegister(0x2c, "DBGPAUSE");
        AddRegister(0x30, "PAUSE");
        AddRegister(0x34, "LOCKED");
        AddRegister(0x38, "SOURCE");
        AddRegister(0x3c, "INTR");
        AddRegister(0x40, "INTE");
        AddRegister(0x44, "INTF");
        AddRegister(0x48, "INTS");
    }

    private long GetMicroseconds()
    {
        return m_Timer.ElapsedTicks * 1_000_000 / Stopwatch.Frequency;
    }
}
