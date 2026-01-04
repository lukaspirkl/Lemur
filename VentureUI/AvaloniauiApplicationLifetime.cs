using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace VentureUI;

// https://github.com/NeverMorewd/Hosting.Avaloniaui

/// <summary>
/// Provides an <see cref="IHostLifetime"/> implementation that manages the lifetime of a Avaloniaui classic desktop <see cref="Application"/> <see cref="IControlledApplicationLifetime"/>.
/// The <typeparamref name="TApplication"/> instance is created during startup and shut down when the host is stopped.
/// </summary>
/// <typeparam name="TApplication">The type of Avaloniaui Application <see cref="Application"/> to manage.</typeparam>
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class AvaloniauiApplicationLifetime<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TApplication> : IHostLifetime
    where TApplication : Application
{
    private readonly IHostApplicationLifetime m_ApplicationLifetime;
    private readonly TaskCompletionSource<object?> m_ApplicationExited = new();
    private TApplication m_Application;

    public AvaloniauiApplicationLifetime(IHostApplicationLifetime applicationLifetime, TApplication application)
    {
        m_Application = application;
        m_ApplicationLifetime = applicationLifetime;
    }

    /// <inheritdoc />
    public async Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        var ready = new TaskCompletionSource<object?>();
        using var registration = cancellationToken.Register(() => ready.TrySetCanceled(cancellationToken));

        if (m_Application.ApplicationLifetime is IControlledApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Startup += (_, _) => ready.TrySetResult(null);
            desktopLifetime.Exit += (_, _) =>
            {
                m_ApplicationExited.TrySetResult(null);
                m_ApplicationLifetime.StopApplication();
            };
        }
        else
        {
            ready.TrySetException(new InvalidOperationException("Generic host support classic desktop only!"));
        }
        await ready.Task;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!m_ApplicationExited.Task.IsCompleted)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dispatcher.UIThread.BeginInvokeShutdown(DispatcherPriority.Normal);
        }
        return m_ApplicationExited.Task;
    }
}
