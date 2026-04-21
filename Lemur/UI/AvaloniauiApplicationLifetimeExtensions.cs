using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Lemur.UI;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public static class AvaloniauiApplicationLifetimeExtensions
{
    /// <summary>
    /// Configures the host to use avaloniaui application lifetime.
    /// Also configures the <typeparamref name="TApplication"/> as a singleton.
    /// </summary>
    /// <param name="appBuilderFactory"><see cref="AppBuilder.Configure{TApplication}()"/></param>
    public static IServiceCollection AddAppBuilder(this IServiceCollection services, Func<AppBuilder> appBuilderFactory)
    {
        var appBuilder = appBuilderFactory();
        if (appBuilder.Instance == null || appBuilder.Instance.ApplicationLifetime == null)
        {
            var lifetime = new ClassicDesktopStyleApplicationLifetime();
            appBuilder = appBuilder.SetupWithLifetime(lifetime);
        }

        if (appBuilder.Instance is null)
        {
            throw new InvalidOperationException("AppBuilder.Instance is null after SetupWithLifetime!");
        }

        return services
                .AddSingleton<Application>(appBuilder.Instance)
                .AddSingleton<AppBuilder>(appBuilder)
                .AddSingleton<IHostLifetime, AvaloniauiApplicationLifetime<Application>>();
    }

    /// <summary>
    /// Runs the avaloniaui application along with the .NET generic host.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static Task<int> RunAvaloniaAppAsync(this IHost host, Func<Window> mainWindowFactory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = host ?? throw new ArgumentNullException(nameof(host));
        var builder = host.Services.GetRequiredService<AppBuilder>();

        if (builder.Instance is null || builder.Instance.ApplicationLifetime is null)
        {
            builder = builder.SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime());
        }

        if (builder.Instance!.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.MainWindow = mainWindowFactory();

            return RunHostAsync(host, desktopLifetime, cancellationToken);
        }

        throw new InvalidOperationException("Generic host support classic desktop only!");
    }

    private static Task<int> RunHostAsync(IHost host, ClassicDesktopStyleApplicationLifetime lifetime, CancellationToken cancellationToken = default)
    {
        var cts = new CancellationTokenSource();
        if (!cancellationToken.CanBeCanceled)
        {
            cancellationToken = cts.Token;
        }

        _ = host.RunAsync(token: cancellationToken);
        return Task.FromResult(lifetime.Start(lifetime.Args ?? []));
    }
}
