using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace Venture;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Listen(IPAddress.Loopback, 3333, listenOptions =>
            {
                listenOptions.UseConnectionHandler<GdbConnectionHandler>();
            });
        });

        //builder.Services.AddHostedService<RP2350Emulator>();

        var app = builder.Build();

        app.Run();
    }
}
