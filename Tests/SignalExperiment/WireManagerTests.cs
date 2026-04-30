using Lemur;

namespace Tests.SignalExperiment;

/// <summary>
/// Tests for WireManager — the registry and factory for Wire instances.
///
/// Since <see cref="WireManager.CreateWire"/> is protected (only concrete wiring
/// subclasses should create wires), tests use a small inner subclass to gain access.
/// This mirrors how production code will work: a concrete WireManager subclass
/// creates and wires up devices in its constructor.
/// </summary>
public class WireManagerTests
{
    // A minimal concrete WireManager used by tests.
    // Production code would have a more elaborate subclass that also creates devices.
    private sealed class TestWireManager : WireManager
    {
        public Wire AddWire()         => CreateWire();
        public void DeleteWire(Wire w) => RemoveWire(w);
    }

    private static readonly TimeSpan T0 = TimeSpan.Zero;

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void Wires_IsEmpty_WhenCreated()
    {
        var wm = new TestWireManager();
        Assert.Empty(wm.Wires);
    }

    // ── CreateWire ────────────────────────────────────────────────────────────

    [Fact]
    public void CreateWire_AddsWire_ToWiresList()
    {
        var wm   = new TestWireManager();
        var wire = wm.AddWire();
        Assert.Single(wm.Wires);
        Assert.Contains(wire, wm.Wires);
    }

    [Fact]
    public void CreateWire_AddsMultipleWires()
    {
        var wm = new TestWireManager();
        var a  = wm.AddWire();
        var b  = wm.AddWire();
        Assert.Equal(2, wm.Wires.Count);
        Assert.Contains(a, wm.Wires);
        Assert.Contains(b, wm.Wires);
    }

    [Fact]
    public void WiresChanged_Fires_WhenWireCreated()
    {
        var wm    = new TestWireManager();
        int count = 0;
        wm.WiresChanged += () => count++;

        wm.AddWire();
        Assert.Equal(1, count);
    }

    [Fact]
    public void WiresChanged_FiresOnce_PerCreation()
    {
        var wm    = new TestWireManager();
        int count = 0;
        wm.WiresChanged += () => count++;

        wm.AddWire();
        wm.AddWire();
        wm.AddWire();
        Assert.Equal(3, count);
    }

    // ── RemoveWire ────────────────────────────────────────────────────────────

    [Fact]
    public void RemoveWire_RemovesWire_FromWiresList()
    {
        var wm   = new TestWireManager();
        var wire = wm.AddWire();

        wm.DeleteWire(wire);
        Assert.Empty(wm.Wires);
    }

    [Fact]
    public void WiresChanged_Fires_WhenWireRemoved()
    {
        var wm   = new TestWireManager();
        var wire = wm.AddWire();

        int count = 0;
        wm.WiresChanged += () => count++;

        wm.DeleteWire(wire);
        Assert.Equal(1, count);
    }

    [Fact]
    public void RemoveWire_DoesNotFire_WhenWireNotInList()
    {
        var wm      = new TestWireManager();
        var foreign = new Wire(); // not created by this manager

        int count = 0;
        wm.WiresChanged += () => count++;

        wm.DeleteWire(foreign);
        Assert.Equal(0, count);
    }

    // ── Usage demonstration: hardcoded wiring subclass ────────────────────────
    //
    // Shows the pattern a concrete WireManager would follow. The wiring is set up
    // in the constructor and all wires are visible to the registry immediately.
    //
    [Fact]
    public void Scenario_HardcodedWiringSubclass()
    {
        var emulatorGpio0 = new Pin("RP2350 - GPIO0");
        var emulatorGpio1 = new Pin("RP2350 - GPIO1");
        var terminalRx    = new Pin("SerialTerminal - RX");
        var terminalTx    = new Pin("SerialTerminal - TX");

        // Terminal TX idles high (UART idle state)
        terminalTx.SetOutput(true, T0);

        // Create a hardcoded wiring manager, passing in devices' pins
        var wm = new HardcodedWiring(emulatorGpio0, terminalRx, emulatorGpio1, terminalTx);

        // Two wires should have been created and registered
        Assert.Equal(2, wm.Wires.Count);

        // GPIO0 wire: chip drives terminal RX
        Assert.True(emulatorGpio0.Wire != null);
        Assert.Same(emulatorGpio0.Wire, terminalRx.Wire);

        // GPIO1 wire: terminal TX drives chip RX; TX idle-high propagated
        Assert.True(emulatorGpio1.Wire != null);
        Assert.Same(emulatorGpio1.Wire, terminalTx.Wire);
        Assert.True(emulatorGpio1.State); // received idle-high from terminal TX
    }

    // Demonstrates a minimal hardcoded WireManager subclass.
    private sealed class HardcodedWiring : WireManager
    {
        public HardcodedWiring(Pin gpio0, Pin termRx, Pin gpio1, Pin termTx)
        {
            var uart0Tx = CreateWire();
            uart0Tx.Connect(gpio0,  T0);
            uart0Tx.Connect(termRx, T0);

            var uart0Rx = CreateWire();
            uart0Rx.Connect(gpio1,  T0);
            uart0Rx.Connect(termTx, T0);
        }
    }
}
