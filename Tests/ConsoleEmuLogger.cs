using Microsoft.Extensions.Logging;
using Lemur;

namespace Tests;

public class ConsoleEmuLogger<T> : IEmuLogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
    }

    public void LogCSRGet(ushort key, uint value)
    {
    }

    public void LogCSRSet(ushort key, uint value)
    {
        if (key != 0x342 && key != 0x343)
        {
            Console.WriteLine($"{csrNames.GetValueOrDefault(key, ((uint)key).ToHex(4))} {value.ToHex()}");
        }
    }

    public void LogInstructionExecute(uint pc, uint instruction, string mnemonic)
    {
        Console.WriteLine($"PC: {pc.ToHex()} ({instruction.ToHex()})");
    }

    public void LogMemoryRead(uint address, int count)
    {
    }

    public void LogMemoryWrite(uint address, byte[] data)
    {
        Console.WriteLine($"mem {address.ToHex()} {data.ToHex()}");
    }

    public void LogRegisterSet(uint index, uint value)
    {
        Console.WriteLine($"x{index}{(index.ToString().Length == 1 ? " " : "")} {value.ToHex()}");
    }

    private Dictionary<ushort, string> csrNames = new Dictionary<ushort, string>
    {
        { 0x305, "mtvec" },
        { 0x340, "mscratch" },
        { 0x341, "epc" },
    };
}