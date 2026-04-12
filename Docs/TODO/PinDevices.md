# TODO: Pin Devices

Feature that parses `BinaryInfo.Pins` labels to show interactive UI controls for physical
components wired to GPIO pins (LEDs, buttons, switches).

**Implemented:** LED indicator (output-only, active-high assumed)

---

## 1. GPIO input path is broken — buttons/switches will not work

`SIO.GPIO_IN` (offset `0x004`) is hardcoded to `0x02000000` (`SIO.cs:22`).
It never reads actual pin state, so firmware polling `GPIO_IN` always gets a constant.

### What needs to change

- `SIO` needs access to `UserBankIO` (or its `MuxedGpioLine[]` array) so that `GPIO_IN` reads
  the current value of each mux line when the register is read.
- The simplest fix: make `GPIO_IN` an `.OnRead()`-computed register that iterates
  `userBankIO.GetGpioLine(i).Value` for i = 0..31 and packs into a uint.
- Same for `GPIO_HI_IN` (offsets `0x008`) for pins 32..47.

### Datasheet questions to resolve first

1. **Does `GPIO_IN` reflect output pins too?**  
   On RP2040, `GPIO_IN` shows the actual pad voltage, so output pins read back their driven
   value. Confirm this is the same on RP2350.

2. **Does `INOVER` affect `GPIO_IN`?**  
   On RP2040, `INOVER` only affects the peripheral input, not the raw `GPIO_IN` register.
   If the same applies to RP2350, `INOVER` can be ignored for `GPIO_IN` reads (already the
   case in the current code — `INOVER` is parsed but not applied anywhere).

---

## 2. No mechanism to inject external input into a pin

To simulate a button, the UI needs to drive a GPIO line that firmware sees in `GPIO_IN`.
Currently there is no "external signal" slot in `MuxedGpioLine`.

### Design sketch

Add a `ManualGpioLine` class (an `IGpioLine` that can be driven by code):

```csharp
public class ManualGpioLine : IGpioLine
{
    private GpioValue _value = GpioValue.HiZ;
    public GpioValue Value => _value;
    public event Action<GpioValue>? Changed;

    public void Set(GpioValue v)
    {
        if (_value == v) return;
        _value = v;
        Changed?.Invoke(v);
    }
}
```

Wire one per GPIO in `UserBankIO` as an "external" input overlay.  
The key question (see §1) is whether `GPIO_IN` needs to merge this with the SIO-driven output
or pick one based on OE state. Likely: if OE is set (output), `GPIO_IN` = SIO driven value;
if OE is clear (input), `GPIO_IN` = ManualGpioLine value.

---

## 3. `OUTOVER` / `OEOVER` / `INOVER` not applied

`GpioControl` parses these fields but never uses them (`UserBankIO.cs:279–285`).

- **`OUTOVER`** — can invert or force the output value. Affects LED correctness when firmware
  uses inverted output. Low priority until a real program exercises it.
- **`OEOVER`** — can force output-enable regardless of SIO `GPIO_OE`. Needed for correct
  input/output direction detection.
- **`INOVER`** — affects the peripheral's view of the input (not `GPIO_IN`). Low priority.

---

## 4. Button and switch UI controls

Once §1 and §2 are resolved, add to `PinDevicesViewModel.Apply()`:

```csharp
if (pin.Label.Contains("button", StringComparison.OrdinalIgnoreCase))
{
    var line = /* ManualGpioLine for this pin */;
    Buttons.Add(new ButtonViewModel(pin.Label, pin.Pins, line));
}
if (pin.Label.Contains("switch", StringComparison.OrdinalIgnoreCase))
{
    var line = /* ManualGpioLine for this pin */;
    Switches.Add(new SwitchViewModel(pin.Label, pin.Pins, line));
}
```

`ButtonViewModel` — exposes a `PressCommand` / `ReleaseCommand` (momentary, drives line Low
while held; releases to High assuming active-low).  
`SwitchViewModel` — exposes a `ToggleCommand` (persistent on/off state).

Active-low is the default (matches RP2350 board conventions and pull-up-to-high pad defaults).
Could extend the label convention to support active-high: e.g. `"button_high"` or `"button+"`.

---

## 5. LED active-low support

Current `LedViewModel` assumes active-high (`High` = on).  
Some designs use active-low LEDs (e.g., LED connected between pin and VCC).

Extend label convention when needed: `"LED_n"` or `"LED-"` = active-low.
