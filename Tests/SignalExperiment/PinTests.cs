using Lemur;

namespace Tests.SignalExperiment;

/// <summary>
/// Tests for Pin behaviour when disconnected from any wire.
/// These validate that the pin's local state resolution and Changed event work
/// correctly in isolation — before any wiring is set up.
/// </summary>
public class PinTests
{
    private static readonly TimeSpan T0 = TimeSpan.Zero;
    private static readonly TimeSpan T1 = TimeSpan.FromMilliseconds(1);
    private static readonly TimeSpan T2 = TimeSpan.FromMilliseconds(2);

    // ── Identity ──────────────────────────────────────────────────────────────

    [Fact]
    public void Name_IsSetFromConstructor()
    {
        var pin = new Pin("RP2350 - GPIO5");
        Assert.Equal("RP2350 - GPIO5", pin.Name);
    }

    // ── Default (disconnected, no output, no pull) ────────────────────────────

    [Fact]
    public void State_IsNull_WhenDisconnectedAndNoOutputNoPull()
    {
        var pin = new Pin("Device - A");
        Assert.Null(pin.State);
    }

    [Fact]
    public void Wire_IsNull_WhenNotConnected()
    {
        var pin = new Pin("Device - A");
        Assert.Null(pin.Wire);
    }

    // ── Pull-only state (no output) ───────────────────────────────────────────

    [Fact]
    public void State_IsTrue_WhenPullUpAndNoOutput()
    {
        var pin = new Pin("Device - A");
        pin.SetPull(PullDirection.Up, T0);
        Assert.True(pin.State);
    }

    [Fact]
    public void State_IsFalse_WhenPullDownAndNoOutput()
    {
        var pin = new Pin("Device - A");
        pin.SetPull(PullDirection.Down, T0);
        Assert.False(pin.State);
    }

    [Fact]
    public void State_IsNull_WhenPullClearedToNone()
    {
        var pin = new Pin("Device - A");
        pin.SetPull(PullDirection.Up, T0);
        pin.SetPull(PullDirection.None, T1);
        Assert.Null(pin.State);
    }

    // ── Output overrides pull ─────────────────────────────────────────────────

    [Fact]
    public void State_IsTrue_WhenOutputTrue_RegardlessOfPull()
    {
        var pin = new Pin("Device - A");
        pin.SetPull(PullDirection.Down, T0);
        pin.SetOutput(true, T0);
        Assert.True(pin.State);
    }

    [Fact]
    public void State_IsFalse_WhenOutputFalse_RegardlessOfPull()
    {
        var pin = new Pin("Device - A");
        pin.SetPull(PullDirection.Up, T0);
        pin.SetOutput(false, T0);
        Assert.False(pin.State);
    }

    [Fact]
    public void State_FallsBackToPull_WhenOutputClearedToNull()
    {
        var pin = new Pin("Device - A");
        pin.SetPull(PullDirection.Up, T0);
        pin.SetOutput(false, T0); // drive low
        pin.SetOutput(null, T1);  // stop driving — pull takes over
        Assert.True(pin.State);
    }

    // ── Changed fires when disconnected ───────────────────────────────────────

    [Fact]
    public void Changed_Fires_WhenOutputChangesWhileDisconnected()
    {
        var pin = new Pin("Device - A");

        SignalChange? received = null;
        pin.Changed += c => received = c;

        pin.SetOutput(true, T1);

        Assert.NotNull(received);
        Assert.True(received.NewState);
        Assert.Null(received.OldState);
        Assert.Equal(T1, received.Time);
    }

    [Fact]
    public void Changed_Fires_WhenPullChangesStateWhileDisconnected()
    {
        var pin = new Pin("Device - A");

        SignalChange? received = null;
        pin.Changed += c => received = c;

        pin.SetPull(PullDirection.Up, T1);

        Assert.NotNull(received);
        Assert.True(received.NewState);
        Assert.Null(received.OldState);
    }

    [Fact]
    public void Changed_Fires_WhenOutputDoesNotChangeState()
    {
        var pin = new Pin("Device - A");
        pin.SetOutput(true, T0);

        int count = 0;
        pin.Changed += _ => count++;

        pin.SetOutput(true, T1); // same value, no state change
        Assert.Equal(1, count);
    }

    [Fact]
    public void Changed_DoesNotFire_WhenPullSetButOutputAlreadyDrives()
    {
        // Output = true already determines state = true.
        // Changing the pull from None to Down does not change State (output still true).
        var pin = new Pin("Device - A");
        pin.SetOutput(true, T0);

        int count = 0;
        pin.Changed += _ => count++;

        pin.SetPull(PullDirection.Down, T1); // pull irrelevant while output is active
        Assert.Equal(0, count);
    }

    [Fact]
    public void Changed_FiresMultipleTimes_ForSequentialTransitions()
    {
        var pin = new Pin("Device - A");
        var changes = new List<SignalChange>();
        pin.Changed += c => changes.Add(c);

        pin.SetOutput(true,  T0);
        pin.SetOutput(false, T1);
        pin.SetOutput(null,  T2);

        Assert.Equal(3, changes.Count);
        Assert.Equal((null,  (bool?)true),  (changes[0].OldState, changes[0].NewState));
        Assert.Equal((true,  (bool?)false), (changes[1].OldState, changes[1].NewState));
        Assert.Equal((false, (bool?)null),  (changes[2].OldState, changes[2].NewState));
    }

    [Fact]
    public void Changed_CarriesTimestamp_MatchingTheCallSite()
    {
        var pin = new Pin("Device - A");
        var future = TimeSpan.FromSeconds(10);

        SignalChange? received = null;
        pin.Changed += c => received = c;

        pin.SetOutput(true, future);

        Assert.Equal(future, received?.Time);
    }

    // ── Output and pull are stored even without a wire ────────────────────────

    [Fact]
    public void Output_IsStoredBeforeConnection()
    {
        var pin = new Pin("Device - A");
        pin.SetOutput(true, T0);
        Assert.True(pin.Output);
    }

    [Fact]
    public void Pull_IsStoredBeforeConnection()
    {
        var pin = new Pin("Device - A");
        pin.SetPull(PullDirection.Up, T0);
        Assert.Equal(PullDirection.Up, pin.Pull);
    }
}
