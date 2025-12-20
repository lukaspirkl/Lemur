using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Venture;
using Venture.Debug;

namespace Tests
{
    public static class RP2350Builder
    {
        public static IDebuggable Create()
        {
            return CreateEmulator();
        }

        public static IDebuggable CreateEmulator()
        {
            var services = new ServiceCollection();
            services.AddTransient(typeof(ILogger<>), typeof(FakeLogger<>));
            services.AddTransient(typeof(IEmuLogger<>), typeof(ConsoleEmuLogger<>));
            services.AddRP2350Emulator();
            var sp = services.BuildServiceProvider();
            return sp.GetRequiredService<IDebuggable>();
        }

        public static IDebuggable CreateOpenOCD()
        {
            var rp2350 = new RP2350GDB("127.0.0.1", 50000);
            rp2350.Reset();
            return rp2350;
        }
    }
}
