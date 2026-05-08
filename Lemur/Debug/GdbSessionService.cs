using System;
using System.Threading;

namespace Lemur.Debug;

public class GdbSessionService
{
    private int m_ActiveConnection = 0;

    public event Action? Connected;
    public event Action? Disconnected;
    public event Action? EmulatorStarted;

    public bool TryAcquire()
    {
        return Interlocked.CompareExchange(ref m_ActiveConnection, 1, 0) == 0;
    }

    public void Release()
    {
        Interlocked.Exchange(ref m_ActiveConnection, 0);
        Disconnected?.Invoke();
    }

    internal void NotifyConnected() => Connected?.Invoke();
    internal void NotifyEmulatorStarted() => EmulatorStarted?.Invoke();
}
