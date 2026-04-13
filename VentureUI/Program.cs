using Avalonia;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Venture;
using Venture.Debug;
using Venture.Peripherals;

namespace VentureUI;

internal class Program
{
    private static readonly string m_LogFile = "Venture.clef";

    [STAThread]
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public static async Task Main(string[] args)
    {
        if (File.Exists(m_LogFile))
        {
            File.Delete(m_LogFile);
        }

        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Configuration
            .AddCommandLine(args)
            .AddEnvironmentVariables()
            .AddInMemoryCollection();

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Listen(IPAddress.Any, 3333, listenOptions =>
            {
                listenOptions.UseConnectionHandler<GdbConnectionHandler>();
            });
        });


        builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
            //.MinimumLevel.Is(LogEventLevel.Fatal)
            .MinimumLevel.Is(LogEventLevel.Verbose)
            //.MinimumLevel.Is(LogEventLevel.Information)
            //.MinimumLevel.Override<GdbConnectionHandler>(LogEventLevel.Verbose)
            .MinimumLevel.Override<BinaryInfoService>(LogEventLevel.Verbose)
            .MinimumLevel.Override<SIO>(LogEventLevel.Verbose)
            .MinimumLevel.Override<UserBankIO>(LogEventLevel.Verbose)
            .MinimumLevel.Override<UserBankPadControl>(LogEventLevel.Verbose)
            .MinimumLevel.Override<Timer0>(LogEventLevel.Verbose)
            .MinimumLevel.Override<Timer1>(LogEventLevel.Verbose)
            .MinimumLevel.Override<Timer>(LogEventLevel.Verbose)
            .MinimumLevel.Override<UART0>(LogEventLevel.Verbose)
            .MinimumLevel.Override<UART1>(LogEventLevel.Verbose)
            .MinimumLevel.Override<UART>(LogEventLevel.Verbose)
            .Enrich.FromLogContext()
            .WriteTo.Async(a => a.File(new CompactJsonFormatter(), m_LogFile))
            //.WriteTo.Console()
        );

        builder.Services.AddTransient(typeof(IEmuLogger<>), typeof(EmuLogger<>));
        builder.Services.AddRP2350Emulator();


        builder.Services.AddSingleton<ViewLocator>();
        RegisterViewModels(new ViewModelCollection(builder.Services));

        var exitCode = await RunAppAsync(builder);
        Debug.WriteLine($"exitCode:{exitCode}");
    }

    public static void RegisterViewModels(IViewModelCollection collection)
    {
        collection.AddSingletonViewModel<MainWindowViewModel, MainWindowView>();
        collection.AddSingletonViewModel<BinaryInfoViewModel, BinaryInfoView>();
        collection.AddSingletonViewModel<PinDevicesViewModel, PinDevicesView>();
        collection.AddSingletonViewModel<PinsViewModel, PinsView>();
        collection.AddSingletonViewModel<UserBankIOViewModel, UserBankIOView>();
        collection.AddSingletonViewModel<UARTViewModel, UARTView>();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseViewLocatorForPreviewer()
            .WithInterFont()
            .LogToTrace();


    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static Task<int> RunAppAsync(WebApplicationBuilder hostBuilder)
    {
        hostBuilder.Services.AddAppBuilder(BuildAvaloniaApp);
        var appHost = hostBuilder.Build();
        var application = appHost.Services.GetRequiredService<Application>();
        application.DataTemplates.Add(appHost.Services.GetRequiredService<ViewLocator>());
        return appHost.RunAvaloniaAppAsync(() => new MainWindowView() { DataContext = appHost.Services.GetRequiredService<MainWindowViewModel>() });
    }
}
