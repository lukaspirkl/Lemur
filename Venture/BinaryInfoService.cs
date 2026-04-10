using Microsoft.Extensions.Logging;

namespace Venture;

/// <summary>
/// Watches the XIP flash region for writes, debounces rapid GDB chunk uploads,
/// and fires <see cref="MetadataChanged"/> once the write stream settles.
/// Both the menu-load path and the GDB path write to the same XIP region,
/// so both are handled automatically here.
/// </summary>
public class BinaryInfoService : IDisposable
{
    private readonly IDebuggable m_System;
    private readonly ILogger<BinaryInfoService> m_Logger;
    private Timer? m_DebounceTimer;
    private const int DebounceMs = 400;

    /// <summary>
    /// Fired after flash writes settle. Value is null when no valid metadata
    /// header was found in the uploaded binary.
    /// </summary>
    public event Action<BinaryInfo?>? MetadataChanged;

    public BinaryInfoService(IDebuggable system, XIP xip, ILogger<BinaryInfoService> logger)
    {
        m_System = system;
        m_Logger = logger;
        xip.Written += OnFlashWritten;
    }

    private void OnFlashWritten()
    {
        // Reset the debounce window each time a write arrives.
        m_DebounceTimer?.Dispose();
        m_DebounceTimer = new Timer(_ => Rescan(), null, DebounceMs, Timeout.Infinite);
    }

    private void Rescan()
    {
        m_Logger.LogInformation("Binary info scan started");

        var info = BinaryInfoReader.ProcessBinary(m_System);

        if (info != null)
            m_Logger.LogInformation("Binary info scan complete: {Name} {Version}", info.ProgramName, info.ProgramVersion);
        else
            m_Logger.LogInformation("Binary info scan complete: no metadata header found");

        MetadataChanged?.Invoke(info);
    }

    public void Dispose() => m_DebounceTimer?.Dispose();
}
