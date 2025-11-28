using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Venture.Debug;

namespace Venture;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Listen(IPAddress.Any, 3333, listenOptions =>
            {
                listenOptions.UseConnectionHandler<GdbConnectionHandler>();
            });
        });

        builder.Services.AddSingleton<RP2350Emulator>();
        builder.Services.AddHostedService(x => x.GetRequiredService<RP2350Emulator>());
        builder.Services.AddSingleton<IEmulator>(x => x.GetRequiredService<RP2350Emulator>());

        var app = builder.Build();

        app.Run();
    }
}
