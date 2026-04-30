using Lemur;

namespace Tests.SignalExperiment;

/// <summary>
/// Tests for Wire state resolution and the Changed event.
/// Pins are used as the mechanism to drive the wire — direct Wire.UpdatePinDrive
/// is internal, so all state changes go through Pin.SetOutput / Pin.SetPull.
/// </summary>
public class WireTests
{
    private static readonly TimeSpan T0 = TimeSpan.Zero;
    private static readonly TimeSpan T1 = TimeSpan.FromMilliseconds(1);

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void State_IsNull_WhenNoDrivers()
    {
        var wire = new Wire();
        Assert.Null(wire.State);
    }

    [Fact]
    public void State_IsNull_WhenOnlyHiZPinConnected()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        // Pin has no output and no pull — fully HiZ
        pin.Connect(wire, T0);
        Assert.Null(wire.State);
    }

    // ── Strong drivers ────────────────────────────────────────────────────────

    [Fact]
    public void State_IsTrue_WhenSingleStrongHighDriver()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.Connect(wire, T0);
        pin.SetOutput(true, T0);
        Assert.True(wire.State);
    }

    [Fact]
    public void State_IsFalse_WhenSingleStrongLowDriver()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.Connect(wire, T0);
        pin.SetOutput(false, T0);
        Assert.False(wire.State);
    }

    [Fact]
    public void State_ReturnsToNull_WhenStrongDriverGoesHiZ()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.Connect(wire, T0);
        pin.SetOutput(true, T0);
        pin.SetOutput(null, T1);
        Assert.Null(wire.State);
    }

    // ── Weak drivers (pulls) ──────────────────────────────────────────────────

    [Fact]
    public void State_IsTrue_WhenOnlyWeakPullUp()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.SetPull(PullDirection.Up, T0);
        pin.Connect(wire, T0);
        Assert.True(wire.State);
    }

    [Fact]
    public void State_IsFalse_WhenOnlyWeakPullDown()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.SetPull(PullDirection.Down, T0);
        pin.Connect(wire, T0);
        Assert.False(wire.State);
    }

    [Fact]
    public void StrongHighOverridesWeakPullDown()
    {
        var wire    = new Wire();
        var driver  = new Pin("Device - Driver");
        var passive = new Pin("Device - Passive");

        passive.SetPull(PullDirection.Down, T0);
        passive.Connect(wire, T0);
        Assert.False(wire.State); // only weak-low so far

        driver.SetOutput(true, T0);
        driver.Connect(wire, T0);
        Assert.True(wire.State);  // strong-high wins
    }

    [Fact]
    public void StrongLowOverridesWeakPullUp()
    {
        var wire    = new Wire();
        var driver  = new Pin("Device - Driver");
        var passive = new Pin("Device - Passive");

        passive.SetPull(PullDirection.Up, T0);
        passive.Connect(wire, T0);
        Assert.True(wire.State); // only weak-high so far

        driver.SetOutput(false, T0);
        driver.Connect(wire, T0);
        Assert.False(wire.State); // strong-low wins
    }

    [Fact]
    public void WireFloats_WhenStrongDriverDisconnects_AndOnlyPullRemains()
    {
        var wire   = new Wire();
        var driver = new Pin("Device - Driver");
        driver.SetOutput(true, T0);
        driver.Connect(wire, T0);
        // Now disconnect — wire should fall back to null (no pull on either pin)
        driver.Disconnect(T1);
        Assert.Null(wire.State);
    }

    // ── Conflict detection ────────────────────────────────────────────────────

    [Fact]
    public void Throws_OnStrongHighVsStrongLow()
    {
        var wire = new Wire();
        var high = new Pin("Device - High");
        var low  = new Pin("Device - Low");

        high.SetOutput(true,  T0);
        high.Connect(wire, T0);

        low.SetOutput(false, T0);
        Assert.Throws<InvalidOperationException>(() => low.Connect(wire, T0));
    }

    [Fact]
    public void Throws_OnOpposingPullsWithNoStrongDriver()
    {
        var wire = new Wire();
        var up   = new Pin("Device - Up");
        var down = new Pin("Device - Down");

        up.SetPull(PullDirection.Up, T0);
        up.Connect(wire, T0);

        down.SetPull(PullDirection.Down, T0);
        Assert.Throws<InvalidOperationException>(() => down.Connect(wire, T0));
    }

    // ── Changed event ─────────────────────────────────────────────────────────

    [Fact]
    public void Changed_Fires_WhenStateTransitions()
    {
        var wire   = new Wire();
        var pin    = new Pin("Device - A");
        pin.Connect(wire, T0);

        SignalChange? received = null;
        wire.Changed += c => received = c;

        pin.SetOutput(true, T1);

        Assert.NotNull(received);
        Assert.Equal(T1,   received.Time);
        Assert.True(received.NewState);
        Assert.Null(received.OldState);
    }

    [Fact]
    public void Changed_Fires_WhenStateIsUnchanged()
    {
        var wire  = new Wire();
        var pin   = new Pin("Device - A");
        pin.SetOutput(true, T0);
        pin.Connect(wire, T0);

        int count = 0;
        wire.Changed += _ => count++;

        pin.SetOutput(true, T1); // same value — no transition
        Assert.Equal(1, count);
    }

    [Fact]
    public void Changed_CarriesFutureTimestamp()
    {
        var wire   = new Wire();
        var pin    = new Pin("Device - A");
        pin.Connect(wire, T0);

        var future = TimeSpan.FromSeconds(5);
        SignalChange? received = null;
        wire.Changed += c => received = c;

        pin.SetOutput(true, future);

        Assert.Equal(future, received?.Time);
    }

    // ── Name derivation ───────────────────────────────────────────────────────

    [Fact]
    public void Name_IsDefaultLabel_WhenNoPinsConnected()
    {
        var wire = new Wire();
        Assert.Equal("(no pins)", wire.Name);
    }

    [Fact]
    public void Name_ShowsSinglePinName()
    {
        var wire = new Wire();
        var pin  = new Pin("RP2350 - GPIO0");
        pin.Connect(wire, T0);
        Assert.Equal("RP2350 - GPIO0", wire.Name);
    }

    [Fact]
    public void Name_ShowsBothPinNames_WhenTwoPinsConnected()
    {
        var wire = new Wire();
        var a    = new Pin("RP2350 - GPIO0");
        var b    = new Pin("SerialTerminal - RX");
        a.Connect(wire, T0);
        b.Connect(wire, T0);
        Assert.Equal("RP2350 - GPIO0 ↔ SerialTerminal - RX", wire.Name);
    }

    [Fact]
    public void Name_UpdatesDynamically_AfterDisconnect()
    {
        var wire = new Wire();
        var a    = new Pin("RP2350 - GPIO0");
        var b    = new Pin("SerialTerminal - RX");
        a.Connect(wire, T0);
        b.Connect(wire, T0);

        a.Disconnect(T1);
        Assert.Equal("SerialTerminal - RX", wire.Name);
    }
}
