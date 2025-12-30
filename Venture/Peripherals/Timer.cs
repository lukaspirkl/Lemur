using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Venture.Peripherals;

public class Timer : PeripheralBase
{
    private readonly ILogger<Timer> logger;

    private static Stopwatch timer;

    static Timer()
    {
        timer = Stopwatch.StartNew();
    }

    private static long GetMicroseconds()
    {
        return timer.ElapsedTicks * 1_000_000 / Stopwatch.Frequency;
    }

    public Timer(uint baseAddress, ILogger<Timer> logger) 
        : base(baseAddress)
    {
        this.logger = logger;
    }

    protected override uint HandleRead(uint offset)
    {   
        logger.LogWarning("Reading from unhandled offset {offset} in Timer", offset.ToHex());

        if (offset == 0x24)
        {
            return (uint)((ulong)GetMicroseconds() >> 32);
        }
        else if (offset == 0x28)
        {
             return (uint)(GetMicroseconds() & 0xFFFFFFFF);
        }

        return 0;
    }

    protected override void HandleWrite(uint offset, uint value)
    {
        logger.LogWarning("Writing to unhandled offset {offset} data {data} in Timer", offset.ToHex(), value.ToHex());
    }
}
