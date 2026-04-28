using System;
using System.Diagnostics;

namespace Lemur;

public interface IElapsedTime
{
    TimeSpan Now { get; }
}

public class ElapsedTime : IElapsedTime
{
    private readonly Stopwatch m_Stopwatch = Stopwatch.StartNew();

    public TimeSpan Now => m_Stopwatch.Elapsed;
}

public class NullElapsedTime : IElapsedTime
{
    public TimeSpan Now => TimeSpan.Zero;
}