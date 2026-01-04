using Microsoft.Extensions.Logging;

namespace Venture;

public interface IEmuLogger : ILogger
{
    void LogInstructionExecute(uint pc, uint instruction, string mnemonic);
    void LogRegisterSet(uint index, uint value);
    void LogMemoryWrite(uint address, byte[] data);
    void LogMemoryRead(uint address, int count);
    void LogCSRSet(ushort key, uint value);
    void LogCSRGet(ushort key, uint value);
}

public interface IEmuLogger<out TCategoryName> : IEmuLogger, ILogger<TCategoryName>
{
}


public class EmuLogger<TCategoryName> : IEmuLogger<TCategoryName>
{
    private readonly ILogger<TCategoryName> logger;

    public EmuLogger(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<TCategoryName>();
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull 
        => logger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) 
        => logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) 
        => logger.Log(logLevel, eventId, state, exception, formatter);

    public void LogCSRGet(ushort key, uint value)
    {
        logger.LogInformation("csr[{reg}] -> {value} (get)", ((uint)key).ToHex(4), value.ToHex());
    }

    public void LogCSRSet(ushort key, uint value)
    {
        logger.LogInformation("csr[{reg}] <- {value} (set)", ((uint)key).ToHex(4), value.ToHex());
    }

    public void LogInstructionExecute(uint pc, uint instruction, string mnemonic)
    {
        logger.LogInformation("Execute PC:{PC} instruction:{instruction} - {mnemonic}", pc.ToHex(), instruction.ToHex(), mnemonic);
    }

    public void LogMemoryRead(uint address, int count)
    {
        logger.LogDebug("Reading from memory {address} (count: {count})", address.ToHex(), count);
    }

    public void LogMemoryWrite(uint address, byte[] data)
    {
        logger.LogDebug("Write to memory {address} value {data}", address.ToHex(), data.ToHex());
    }

    public void LogRegisterSet(uint index, uint value)
    {
        logger.LogDebug("x{index} {value}", index, value.ToHex());
    }
}

public class NullEmuLogger<T> : IEmuLogger<T>
{
    private class Disposable : IDisposable
    {
        public void Dispose()
        {
        }
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

    public void LogCSRGet(ushort key, uint value)
    {
    }

    public void LogCSRSet(ushort key, uint value)
    {
    }

    public void LogInstructionExecute(uint pc, uint instruction, string mnemonic)
    {
    }

    public void LogMemoryRead(uint address, int count)
    {
    }

    public void LogMemoryWrite(uint address, byte[] data)
    {
    }

    public void LogRegisterSet(uint index, uint value)
    {
    }
}