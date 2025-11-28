using Microsoft.Extensions.Logging;

namespace Tests;

public class NullLogger<T> : ILogger<T>
{
    private class Disposable : IDisposable
    {
        public void Dispose() { }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return new Disposable();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
    }
}