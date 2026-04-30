using Lemur;

namespace Tests.SignalExperiment;

/// <summary>
/// Integration tests that exercise Pin and Wire together.
///
/// Includes both correctness tests and usage demonstrations that show the
/// patterns device code and wiring code are expected to follow.
/// </summary>
public class IntegrationTests
{
    private static readonly TimeSpan T0 = TimeSpan.Zero;
    private static readonly TimeSpan T1 = TimeSpan.FromMilliseconds(1);
    private static readonly TimeSpan T2 = TimeSpan.FromMilliseconds(2);

    // ── Connect pushes stored state immediately ────────────────────────────────

    [Fact]
    public void Connect_PushesStoredOutput_IntoWire()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - TX");
        pin.SetOutput(true, T0); // stored while disconnected

        pin.Connect(wire, T1);

        Assert.True(wire.State); // propagated on connect
    }

    [Fact]
    public void Connect_PushesStoredPull_IntoWire()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - TX");
        pin.SetPull(PullDirection.Down, T0); // stored while disconnected

        pin.Connect(wire, T1);

        Assert.False(wire.State);
    }

    [Fact]
    public void PinState_EqualsWireState_AfterConnect()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.SetOutput(true, T0);
        pin.Connect(wire, T0);

        Assert.Equal(wire.State, pin.State);
    }

    // ── Changed event at connection boundary ──────────────────────────────────

    [Fact]
    public void PinChanged_Fires_WhenConnectChangesState()
    {
        // Pin was floating (null), wire is driven high by another pin.
        // Connecting should fire Pin.Changed: null → true.
        var wire   = new Wire();
        var driver = new Pin("Device - Driver");
        driver.SetOutput(true, T0);
        driver.Connect(wire, T0);

        var observer = new Pin("Device - Observer"); // no output, no pull → state = null
        SignalChange? received = null;
        observer.Changed += c => received = c;

        observer.Connect(wire, T1);

        Assert.NotNull(received);
        Assert.True(received.NewState);
        Assert.Null(received.OldState);
    }

    [Fact]
    public void PinChanged_DoesNotFire_WhenConnectDoesNotChangeState()
    {
        // Pin has pull-up → local state = true. Wire already = true (another strong-high).
        // Connecting should not fire Changed since state remains true.
        var wire   = new Wire();
        var driver = new Pin("Device - Driver");
        driver.SetOutput(true, T0);
        driver.Connect(wire, T0);

        var observer = new Pin("Device - Observer");
        observer.SetPull(PullDirection.Up, T0); // local state = true

        int count = 0;
        observer.Changed += _ => count++;

        observer.Connect(wire, T1); // wire = true, local = true → no change
        Assert.Equal(0, count);
    }

    // ── Disconnect restores local state ───────────────────────────────────────

    [Fact]
    public void Disconnect_RemovesDrive_FromWire()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.SetOutput(true, T0);
        pin.Connect(wire, T0);
        Assert.True(wire.State);

        pin.Disconnect(T1);
        Assert.Null(wire.State); // no other drivers
    }

    [Fact]
    public void PinState_FallsBackToLocal_AfterDisconnect()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.SetPull(PullDirection.Up, T0); // local = true

        var driver = new Pin("Device - Driver");
        driver.SetOutput(false, T0);
        driver.Connect(wire, T0);
        pin.Connect(wire, T0);

        Assert.False(pin.State); // wire is driven low — overrides pull

        pin.Disconnect(T1);
        Assert.True(pin.State); // back to local pull-up state
    }

    [Fact]
    public void PinChanged_Fires_WhenDisconnectChangesState()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        // No pull: local state = null
        pin.SetOutput(true, T0);
        pin.Connect(wire, T0);

        pin.SetOutput(null, T0); // now HiZ, wire = null, local = null — same, no event

        // Reset: drive pin high via wire so disconnect changes pin state
        var driver = new Pin("Device - Driver");
        driver.SetOutput(true, T0);
        driver.Connect(wire, T0);
        // pin has no output, no pull → pin.State = wire.State = true

        SignalChange? received = null;
        pin.Changed += c => received = c;

        pin.Disconnect(T1); // pin.State was true (from wire), now null (local)

        Assert.NotNull(received);
        Assert.Null(received.NewState);
        Assert.True(received.OldState);
    }

    // ── Multiple pins on the same wire ────────────────────────────────────────

    [Fact]
    public void MultipleObservers_AllReceiveChanged_WhenDriverChanges()
    {
        var wire    = new Wire();
        var driver  = new Pin("Chip - GPIO0");
        var obs1    = new Pin("Device1 - IN");
        var obs2    = new Pin("Device2 - IN");

        obs1.Connect(wire, T0);
        obs2.Connect(wire, T0);

        var obs1Changes = new List<SignalChange>();
        var obs2Changes = new List<SignalChange>();
        obs1.Changed += c => obs1Changes.Add(c);
        obs2.Changed += c => obs2Changes.Add(c);

        driver.SetOutput(true, T0);
        driver.Connect(wire, T1);

        Assert.Single(obs1Changes);
        Assert.Single(obs2Changes);
        Assert.True(obs1Changes[0].NewState);
        Assert.True(obs2Changes[0].NewState);
    }

    [Fact]
    public void LateJoin_PinSeesCurrentWireState_Immediately()
    {
        // Wire is already driven high. A pin connecting later gets the current state.
        var wire   = new Wire();
        var driver = new Pin("Chip - GPIO0");
        driver.SetOutput(true, T0);
        driver.Connect(wire, T0);

        var latecomer = new Pin("Device - IN");
        latecomer.Connect(wire, T1);

        Assert.True(latecomer.State);
    }

    [Fact]
    public void LateJoin_FiresChangedOnOtherPins_IfWireStateChanges()
    {
        // Wire is floating. A new pin with pull-up connects — wire transitions null→true.
        // The already-connected observer pin should fire Changed.
        var wire     = new Wire();
        var observer = new Pin("Device - Observer"); // HiZ
        observer.Connect(wire, T0);
        Assert.Null(observer.State);

        SignalChange? received = null;
        observer.Changed += c => received = c;

        var pullUp = new Pin("Device - PullUp");
        pullUp.SetPull(PullDirection.Up, T0);
        pullUp.Connect(wire, T1); // wire transitions null → true

        Assert.NotNull(received);
        Assert.True(received.NewState);
        Assert.Null(received.OldState);
        Assert.Equal(T1, received.Time);
    }

    // ── Reconnect to a different wire ─────────────────────────────────────────

    [Fact]
    public void Reconnect_RemovesDrive_FromOldWire_AndAddsToNewWire()
    {
        var wireA = new Wire();
        var wireB = new Wire();
        var pin   = new Pin("Device - A");

        pin.SetOutput(true, T0);
        pin.Connect(wireA, T0);
        Assert.True(wireA.State);
        Assert.Null(wireB.State);

        pin.Connect(wireB, T1); // reconnect
        Assert.Null(wireA.State); // removed from wireA
        Assert.True(wireB.State); // added to wireB
    }

    // ── Symmetric API — Wire.Connect works identically to Pin.Connect ─────────

    [Fact]
    public void WireConnect_IsEquivalentTo_PinConnect()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.SetOutput(true, T0);

        wire.Connect(pin, T0); // called from wire side

        Assert.True(wire.State);
        Assert.Equal(wire, pin.Wire);
    }

    [Fact]
    public void WireDisconnect_IsEquivalentTo_PinDisconnect()
    {
        var wire = new Wire();
        var pin  = new Pin("Device - A");
        pin.SetOutput(true, T0);
        wire.Connect(pin, T0);

        wire.Disconnect(pin, T1);

        Assert.Null(wire.State);
        Assert.Null(pin.Wire);
    }

    // ── Usage demonstration: UART TX idle-high ────────────────────────────────
    //
    // Scenario: A SerialTerminal drives its TX pin high (idle) at construction time,
    // before any wire is attached. The moment the wire is connected the idle-high
    // state propagates to all other pins on the wire.
    //
    [Fact]
    public void Scenario_UartTxIdleHigh_PropagatesOnConnect()
    {
        // -- Device construction (no wiring yet) --
        var txPin = new Pin("SerialTerminal - TX");
        txPin.SetOutput(true, T0); // UART idles high; wire not yet attached
        Assert.True(txPin.State);  // local state is correct before wiring

        // -- Emulator GPIO pin (also disconnected) --
        var gpioRx = new Pin("RP2350 - GPIO1");
        Assert.Null(gpioRx.State); // floating before wiring

        // -- Wiring set up at runtime --
        var wire = new Wire();
        var gpioRxChanges = new List<SignalChange>();
        gpioRx.Changed += c => gpioRxChanges.Add(c);

        txPin.Connect(wire,   T1);
        gpioRx.Connect(wire,  T1);

        // Both pins should now see the idle-high state
        Assert.True(txPin.State);
        Assert.True(gpioRx.State);

        // gpioRx received exactly one Change event: null → true
        Assert.Single(gpioRxChanges);
        Assert.True(gpioRxChanges[0].NewState);
        Assert.Null(gpioRxChanges[0].OldState);
    }

    // ── Usage demonstration: Switch connected to GPIO ─────────────────────────
    //
    // Scenario: a Switch drives a wire high or low. The GPIO pin has a pull-down
    // so when the switch is off (HiZ) the line reads false.
    //
    [Fact]
    public void Scenario_SwitchAndGpioWithPullDown()
    {
        var switchPin = new Pin("Switch - Out");
        var gpioPin   = new Pin("RP2350 - GPIO5");
        gpioPin.SetPull(PullDirection.Down, T0); // input pull-down on chip side

        var wire = new Wire();
        switchPin.Connect(wire, T0);
        gpioPin.Connect(wire,   T0);

        // Switch off (HiZ) — pull-down wins
        Assert.False(gpioPin.State);

        // Switch pressed (drive high)
        var changes = new List<SignalChange>();
        gpioPin.Changed += c => changes.Add(c);

        switchPin.SetOutput(true, T1);
        Assert.True(gpioPin.State);
        Assert.Single(changes);
        Assert.True(changes[0].NewState);

        // Switch released (HiZ again) — pull-down pulls back low
        switchPin.SetOutput(null, T2);
        Assert.False(gpioPin.State);
        Assert.Equal(2, changes.Count);
        Assert.False(changes[1].NewState);
    }
}
