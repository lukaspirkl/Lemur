using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Venture;
using Venture.Debug;

namespace Tests
{
    public static class RP2350Builder
    {
        public static IDebuggable CreateEmulator()
        {
            var services = new ServiceCollection();
            services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
            services.AddRP2350Emulator();
            var sp = services.BuildServiceProvider();
            return sp.GetRequiredService<IDebuggable>();
        }

        public static IDebuggable CreateOpenOCD()
        {
            return new RP2350GDB("127.0.0.1", 50000);
        }
    }
}
