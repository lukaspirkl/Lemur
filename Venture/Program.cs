using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Net;
using Venture.Debug;
using Venture.Peripherals;

namespace Venture;

internal class Program
{
    private static readonly string logFile = "Venture.clef";

    private static async Task Main(string[] args)
    {
        if (File.Exists(logFile))
        {
            File.Delete(logFile);
        }

        var builder = WebApplication.CreateSlimBuilder(args);

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Listen(IPAddress.Any, 3333, listenOptions =>
            {
                listenOptions.UseConnectionHandler<GdbConnectionHandler>();
            });
        });


        builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
            .MinimumLevel.Is(LogEventLevel.Fatal)
            //.MinimumLevel.Override<GdbConnectionHandler>(LogEventLevel.Verbose)
            .MinimumLevel.Override<SIO>(LogEventLevel.Verbose)
            .MinimumLevel.Override<UserBankIO>(LogEventLevel.Verbose)
            .MinimumLevel.Override<UserBankPadControl>(LogEventLevel.Verbose)
            .Enrich.FromLogContext()
            .WriteTo.Async(a => a.File(new CompactJsonFormatter(), logFile))
            .WriteTo.Console());

        builder.Services.AddTransient(typeof(IEmuLogger<>), typeof(EmuLogger<>));

        builder.Services.AddRP2350Emulator();

        var app = builder.Build();

        await app.RunAsync();
    }
}
