using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Venture;
using Venture.Debug;

namespace Tests;

public static class RP2350Builder
{
    public static IDebuggable Create()
    {
        return CreateEmulator();
        //return CreateOpenOCD();
    }

    public static IDebuggable CreateEmulator()
    {
        var sp = CreateServiceProvider();
        return sp.GetRequiredService<IDebuggable>();
    }

    /// <summary>
    /// Builds and returns the full DI container for the emulator.
    /// Use this when a test needs access to internal services that are not
    /// exposed through <see cref="IDebuggable"/>, such as <see cref="IrqController"/>.
    /// Dispose the returned <see cref="ServiceProvider"/> at the end of the test.
    /// </summary>
    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddTransient(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddTransient(typeof(IEmuLogger<>), typeof(ConsoleEmuLogger<>));
        services.AddRP2350Emulator();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    public static IDebuggable CreateOpenOCD()
    {
        var rp2350 = new RP2350GDB("127.0.0.1", 50000);
        rp2350.Reset();
        return rp2350;
    }
}

public static class DebuggableExtensions
{
    extension(IDebuggable sut)
    {
        public uint PC
        {
            get { return sut.Registers[32]; }
            set { sut.Registers[32] = value; }
        }
    }
}
