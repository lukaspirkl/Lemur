# Venture — RP2350 RISC-V Emulator

A C# emulator of the Raspberry Pi RP2350 chip's RISC-V core (Hazard3). The CPU is nearly fully implemented. Peripherals use a shared framework but most are stubs or partially implemented. The Avalonia UI visualizes peripheral state and will host external devices (displays, buttons, etc.).

Datasheets are available as markdown files organized in nested directories per chapters.
- `Docs/hazard3/` — datasheet for Hazard3 RISC-V processor
- `Docs/rp2350/` — datasheet for RP2350 chip

## Project Layout

```
Venture/           - Core library: CPU, bus, memory, peripherals
VentureUI/         - Avalonia MVVM UI
Tests/             - Unit and integration tests
GdbDiff/           - GDB remote debug integration
Docs/              - Reference documents
```

Key namespaces:
- `Venture.Processor` — Hazard3 CPU, instruction decoding, registers, CSRs
- `Venture.Peripherals` — All peripheral implementations
- `Venture` — Bus fabric, memory, interfaces, DI extensions
- `VentureUI` — ViewModels and views

## Memory Map

| Address Range         | Region       | Size    |
|-----------------------|--------------|---------|
| `0x00000000–0x00007FFF` | ROM        | 32 KB   |
| `0x10000000–0x13FFFFFF` | XIP (flash)| 2 MB    |
| `0x20000000–0x2007FFFF` | SRAM       | 520 KB  |
| `0x40000000–0x401FFFFF` | APB Peripherals | —  |
| `0x50000000–0x507FFFFF` | AHB Peripherals (DMA, USB, PIO) | — |
| `0xD0000000–0xD001FFFF` | SIO        | —       |

XIP maps a single 2 MB physical flash; the upper address bits select aliasing mode.  
Atomic-operation aliases: peripheral base + `0x1000` (XOR), `+0x2000` (OR), `+0x3000` (AND-NOT), `+0x4000` (normal, no lane replication).

## Core Architecture

### Bus Fabric (`BusFabric.cs`)

Implements `IBusFabric`. Holds all `IAddressableResource` instances and routes reads/writes by address range. Enforces alignment (`address % accessWidth == 0`); throws `RiscVException` on fault.

### CPU (`Hazard3Processor.cs`)

- `Step()` — fetch → decode → execute loop
- `Memory` — `IBusFabric` reference used for all load/store/fetch
- `Registers` — 32 GPRs (x0 hardwired to zero)
- `CSR` — dictionary-based CSRs; no privilege enforcement
- `PC` starts at `0x00007DFC`
- Events: `EBreak`, `ECall`, `Stopped`
- Exceptions write MEPC/MCAUSE/MTVAL then jump to MTVEC

---

## Peripheral Framework

### Core Interface

```csharp
public interface IAddressableResource
{
    uint BaseAddress { get; }
    uint Size { get; }
    byte[] Read(uint address, int count);
    void Write(uint address, byte[] data);
}
```

Every memory region and peripheral implements this. BusFabric discovers them automatically via DI.

### Word-Oriented Peripherals — `PeripheralBase`

Inherit from `PeripheralBase` for register-mapped peripherals (the standard case for APB/AHB peripherals).

```csharp
public class MyPeripheral : PeripheralBase
{
    public MyPeripheral() : base(baseAddress: 0x40050000, size: 0x1000)
    {
        Registers[0x00] = new Register32()
            .Field(0, 8, () => _value, v => _value = v)   // bits 0-7
            .Field(8, getter: () => SomeStatus)             // read-only bit 8
            .OnWrite(_ => TriggerSideEffect());
    }

    protected override uint HandleRead(uint offset) => base.HandleRead(offset);
    protected override void HandleWrite(uint offset, uint value) => base.HandleWrite(offset, value);
}
```

- Override `HandleRead`/`HandleWrite` only for registers with non-trivial decode logic.
- Atomic aliases (`+0x1000/2000/3000/4000`) are handled automatically by the base class — no extra code needed.
- Undefined offsets return 0 on read and are silently ignored on write (unless you override).

### Byte-Oriented Peripherals — `BytePeripheralBase`

Used when the hardware has byte-granular registers (e.g., OTP). Same pattern but `HandleRead`/`HandleWrite` work with single bytes.

### Register Fields — `Register32`

```csharp
var reg = new Register32()
    .Field(lsb: 0,  width: 4,  getter: () => nibble,   setter: v => nibble = (byte)v)
    .Field(bit: 4,              getter: () => flag,      setter: v => flag = v != 0)
    .Field<MyEnum>(lsb: 8, width: 2, getter: () => mode, setter: v => mode = v)
    .OnRead(() => /* compute value before read */)
    .OnWrite(v => /* side-effect after write */);
```

Fields are encoded/decoded automatically. `OnWrite` receives the full register value after all fields are applied.

### Registering Peripherals (`RP2350ServiceCollectionExtensions.cs`)

All peripherals are registered here. Add new peripherals in the correct address order:

```csharp
// Implemented peripheral:
services.AddPeripheral(0x40050000, "FOO_BASE").Implementation<FooPeripheral>();

// Stub for unimplemented peripheral:
services.AddPeripheral(0x40060000, "BAR_BASE").Unimplemented();

// Memory:
services.AddMemory("BOOTRAM", 0x400E0000, 1024);
```

---

## Peripheral Implementation Checklist

When implementing a new peripheral:

1. **Create** `Venture/Peripherals/MyPeripheral.cs` inheriting `PeripheralBase` (or `BytePeripheralBase`).
2. **Define registers** in the constructor using `Register32` with field getters/setters and `OnWrite` hooks.
3. **Implement side effects** (DMA triggers, interrupt assertions, GPIO changes) in `OnWrite` callbacks or `HandleWrite` overrides.
4. **Expose events** for data/state the UI or external devices need to observe (pattern: `public event Action<T>? SomethingHappened;`).
5. **Replace the stub** in `RP2350ServiceCollectionExtensions.cs`: change `.Unimplemented()` to `.Implementation<MyPeripheral>()`.
6. **Add a ViewModel** in `VentureUI/ViewModels/` if the peripheral has visualizable state (see UI section below).
7. **Write tests** in `Tests/` using `RP2350Builder.Create()`.

---

## GPIO & External Devices

### Interfaces

```csharp
public interface IGpioLine
{
    GpioValue Value { get; }
    event Action<GpioValue>? Changed;
}

public enum GpioValue { HiZ, High, Low }

public interface IGpioSource
{
    IGpioLine GetGpioLine(int index);
}
```

### Wiring an External Device to GPIO

`UserBankIO` holds 48 `MuxedGpioLine` instances. Each line multiplexes outputs from SIO and any external device:

```csharp
// In UserBankIO or its setup:
muxedLine.Add(muxId: 99, name: "MyDevice", line: myDeviceOutputLine);
```

To **observe** pin state changes from firmware (e.g., drive an external display):

```csharp
var gpioSource = resources.OfType<IGpioSource>().First();
var line = gpioSource.GetGpioLine(pinIndex);
line.Changed += newValue => UpdateDisplay(newValue);
```

To **inject** a signal into the CPU (e.g., a button press):

- Drive a `MuxedGpioLine` from an external `IGpioLine` implementation.
- Or write directly to SIO GPIO input registers when the SIO reads that pin.

### Pattern for an External Device

```csharp
public class MyButton : IGpioLine
{
    private GpioValue _value = GpioValue.HiZ;
    public GpioValue Value => _value;
    public event Action<GpioValue>? Changed;

    public void Press()   => SetValue(GpioValue.Low);
    public void Release() => SetValue(GpioValue.High);

    private void SetValue(GpioValue v)
    {
        _value = v;
        Changed?.Invoke(v);
    }
}
```

---

## UI Integration

### Adding a Tab for a New Peripheral

1. Create `VentureUI/ViewModels/MyPeripheralViewModel.cs` implementing `IPeripheralTab`.
2. Inject peripheral via `IEnumerable<IAddressableResource>`:

```csharp
public class MyPeripheralViewModel : IPeripheralTab
{
    public string Header => "MyPeripheral";

    public MyPeripheralViewModel(IEnumerable<IAddressableResource> resources)
    {
        var periph = resources.OfType<MyPeripheral>().FirstOrDefault();
        if (periph == null) return;
        periph.SomethingHappened += data => /* update ObservableProperty */;
    }
}
```

3. Register the ViewModel with DI in `VentureUI` and add it to the tab collection in `MainWindowViewModel`.
4. Create the corresponding Avalonia View (`MyPeripheralView.axaml`).

---

## Testing

Use `RP2350Builder.Create()` to get a fully wired emulator instance:

```csharp
var emu = RP2350Builder.Create();
emu.Processor.Memory.Write(0x20000000, BitConverter.GetBytes(0xDEADBEEF));
var result = emu.Processor.Memory.Read(0x20000000, 4);
```

Use `InstructionBuilder` for encoding raw RISC-V instructions in tests.

---

## Conventions

- Peripheral files live in `Venture/Peripherals/`.
- One file per peripheral class; filename matches class name.
- Address constants come from `RP2350ServiceCollectionExtensions` — do not hardcode them inside peripheral classes.
- Peripheral constructors take only what DI provides; avoid static state.
- Side effects (events, GPIO changes) happen in `OnWrite` callbacks, not in field setters.
- UI ViewModels must not reference peripheral internals directly — only subscribe to events or read public state.
- Tests go in `Tests/` and must use `RP2350Builder` rather than constructing peripherals in isolation.
