# Architecture Review — Venture RP2350 Emulator

## Current State Summary

The emulator is well-structured overall. The DI-based composition, `IAddressableResource` interface for the bus fabric, `Register32` fluent API, and event-driven GPIO model are solid foundations. The main issues are around **coupling between processor and interrupt controller**, **peripheral IRQ delivery**, and **UI-to-peripheral coupling via concrete types**. These make isolated testing and extension harder than necessary.

---

## Issues & Actionable Improvements

### 1. Processor <-> IrqController Bidirectional Coupling

**Problem:** `IrqController` takes `Hazard3Processor` in its constructor and directly manipulates `processor.CSR` to register hooks. The processor in turn holds callback references (`MeiTrapGate`, `OnMeiVectorEntry`) that point back to the IrqController. Neither can be instantiated or tested without the other.

**Impact:** Cannot unit-test processor trap logic without a real IrqController. Cannot test IrqController priority/preemption logic without a real processor.

**Improvement:** Extract an interface (e.g., `IInterruptController`) that the processor depends on. The processor should call into this interface during `CheckInterrupts()` and trap entry. The IrqController implements it. CSR hook registration should go through a well-defined registration method on the CSR class rather than the IrqController reaching into processor internals.

This way you can test the processor with a stub interrupt controller that returns canned values, and test the IrqController with a mock CSR store.

---

### 2. Peripherals Depend on Concrete IrqController

**Problem:** UART0, UART1, SIO, UserBankIO, Timer all take `IrqController` as a constructor parameter to call `RaiseIrq()` / `ClearIrq()`. This is a concrete class, not an interface.

**Impact:** Cannot test any IRQ-generating peripheral without constructing the full IrqController (which requires the processor — see issue #1). Testing a peripheral like UART means building the entire emulator.

**Improvement:** Extract a simple `IIrqController` interface with `RaiseIrq(int)` and `ClearIrq(int)`. Peripherals depend on this interface. Tests can provide a trivial mock that just records which IRQs were raised.

---

### 3. Processor.Memory Set After Construction

**Problem:** `Hazard3Processor.Memory` is a public property assigned by `RP2350Emulator` after construction. The processor can exist in an invalid state (null memory) between construction and wiring.

**Impact:** Construction-time invariant is broken. Possible NullReferenceException if someone calls `Step()` too early. Hard to reason about object lifecycle.

**Improvement:** Inject `IBusFabric` into the processor constructor. This requires adjusting the DI registration order (BusFabric needs peripherals, but processor doesn't need to be a peripheral). If there's a circular dependency, break it with `Lazy<IBusFabric>` or by having `RP2350Emulator` not be the one wiring it.

---

### 4. UserBankIO <-> SIO Tight Coupling & Post-Construction GPIO Wiring

**Problem:** `UserBankIO` takes `SIO` as a concrete constructor dependency to wire GPIO mux lines. GPIO sources are added via `MuxedGpioLine.Add()` calls after construction, requiring specific instantiation order.

**Impact:** GPIO wiring is fragile and order-dependent. External devices (LCD, buttons) must be wired in the same post-construction phase. Testing GPIO routing requires building both SIO and UserBankIO.

**Improvement:** Define an `IGpioController` interface that `UserBankIO` implements, covering GPIO line access and mux registration. External device wiring should go through this interface. Consider a builder or registration step that validates all GPIO connections are complete before the emulator starts, rather than relying on construction order.

---

### 5. UI ViewModels Use `OfType<ConcretePeripheral>()` for Discovery

**Problem:** ViewModels receive `IEnumerable<IAddressableResource>` and then fish out specific peripherals using `OfType<UART0>()`, `OfType<UserBankIO>()`, etc. This is a direct dependency on concrete peripheral types.

**Impact:** Cannot provide a mock UART to test UARTViewModel without subclassing UART0. Cannot have alternative peripheral implementations. UI assembly has a compile-time dependency on every peripheral class it visualizes.

**Improvement:** Two options (pick based on complexity):

- **Option A — Named peripheral interfaces:** Define small interfaces like `IUartPeripheral` (with `ReceivedData` event, `SendByte` method) that UART0 implements. ViewModels depend on these interfaces. This is the cleanest but most work.

- **Option B — Peripheral registry with string keys:** Provide an `IPeripheralRegistry` that maps names (e.g., "UART0") to `IAddressableResource` instances. ViewModels query by name and cast. Less type-safe but simpler to implement.

Recommendation: Option A for peripherals that have UI (UART, GPIO, Pins). The rest don't need it yet.

---

### 6. CsrViewModel Takes Concrete Hazard3Processor

**Problem:** `CsrViewModel` constructor takes `Hazard3Processor` directly to access `.CSR`. This is the only ViewModel that depends on the processor type.

**Impact:** Cannot unit-test CSR display logic without building a real processor.

**Improvement:** `CsrViewModel` should depend on an interface (or the existing `IDebuggable`) that exposes CSR read access. Since `IDebuggable` already has `GetCSR`/`SetCSR`, this may just be a matter of using it instead of the concrete type.

---

### 7. RP2350Emulator Does Too Much

**Problem:** `RP2350Emulator` is the composition root — it wires `Processor.Memory`, subscribes to `EBreak`/`ECall`, manages the run loop, and implements `IDebuggable`. It's doing lifecycle management, event routing, and debugging API in one class.

**Impact:** Any test that needs `IDebuggable` gets the full emulator with run loop and event handling. Hard to test debugging features in isolation.

**Improvement:** Consider separating concerns:
- Keep `RP2350Emulator` as the lifecycle/run-loop manager.
- Extract the `IDebuggable` implementation into a thin adapter that wraps processor + bus.
- Move the processor-memory wiring into DI (see issue #3).

This is lower priority — the current structure works, but it would improve testability of debug features.

---

### 8. No Interface for Clock/Tick Coordination

**Problem:** Timer peripherals and the processor all have independent notions of time. There's no shared clock or tick mechanism. Peripherals that need to count cycles or schedule events have no standard way to do so.

**Impact:** Adding peripherals with timing requirements (PWM, PIO, SPI with baud rates) requires ad-hoc solutions. Makes it hard to implement a global "step N cycles" test helper.

**Improvement:** Introduce a simple `IClock` or `ITickSource` interface that the processor advances on each `Step()`. Peripherals that need timing can subscribe. This doesn't need to be cycle-accurate — even a simple counter that peripherals can poll would help. This is forward-looking for PIO/PWM implementation.

---

### 9. Static ViewModel Registration in Program.cs

**Problem:** `RegisterViewModels()` is a hardcoded list of ViewModel-View pairs. Adding a new peripheral tab requires editing this method.

**Impact:** Minor inconvenience now, but grows as more peripherals get UI. No way for an external device plugin to contribute UI.

**Improvement:** Use assembly scanning or attribute-based registration (e.g., `[PeripheralView(typeof(MyView))]` on ViewModels). The ViewLocator already handles the mapping — it just needs a discovery mechanism. Low priority but worth doing when the tab count grows past ~10.

---

### 10. Tests Require Full Emulator — No Lightweight Test Harness

**Problem:** `RP2350Builder.Create()` builds the complete emulator with all 80+ peripherals. Every test pays the cost of constructing every peripheral, even when testing a single instruction or a single peripheral register.

**Impact:** Slow test startup. Impossible to test a peripheral in isolation — e.g., testing UART register logic requires constructing the entire memory map, processor, IrqController, GPIO, etc.

**Improvement:** After fixing issues #1-#4, create a lightweight builder that constructs only what's needed:

```csharp
// Test just the processor with minimal bus
var cpu = TestHarness.ProcessorWithMemory(sramSize: 4096);

// Test a peripheral with mock IRQ
var uart = TestHarness.Peripheral<UART0>(mockIrq: true);

// Test the full stack (equivalent to current RP2350Builder)
var emu = TestHarness.FullEmulator();
```

This becomes possible once peripherals don't require concrete IrqController/Processor references.

---

### 11. Event Subscription Leaks — No Cleanup

**Problem:** ViewModels subscribe to peripheral events (`uart.ReceivedData += ...`, `padControl.PadControlChanged += ...`) in constructors but never unsubscribe. Singletons mask this since nothing is disposed, but it would become a problem if VMs are ever recreated.

**Impact:** Not a bug today (singleton lifetime), but violates IDisposable patterns and will become a bug if the architecture evolves toward per-session or per-tab lifecycle.

**Improvement:** Implement `IDisposable` on ViewModels, unsubscribe in `Dispose()`. Low priority but good hygiene.

---

### 12. GdbDiff Duplicates Extension Methods

**Problem:** `GdbDiff/Extensions.cs` duplicates `ToHex()`/`ToBin()` formatting utilities that likely exist in the main Venture project.

**Impact:** Minor code duplication. Drift risk.

**Improvement:** Move shared formatting utilities to `Venture` (the core library) and reference them from GdbDiff. Trivial fix.

---

## Priority Order

| Priority | Issue | Effort | Impact on Testability |
|----------|-------|--------|-----------------------|
| **1** | #2 — Extract `IIrqController` interface | Small | High — unlocks peripheral testing |
| **2** | #1 — Break Processor <-> IrqController cycle | Medium | High — unlocks processor testing |
| **3** | #3 — Inject IBusFabric into processor | Small | Medium — enforces invariants |
| **4** | #10 — Lightweight test harness | Medium | High — fast isolated tests |
| **5** | #5 — Peripheral interfaces for UI | Medium | Medium — unlocks VM testing |
| **6** | #4 — GPIO controller interface | Medium | Medium — enables external device testing |
| **7** | #6 — CsrViewModel uses IDebuggable | Small | Low — single VM fix |
| **8** | #8 — Clock/tick coordination | Medium | Medium — future peripheral support |
| **9** | #7 — Split RP2350Emulator | Medium | Low — cleaner but works today |
| **10** | #9 — Auto-discover ViewModels | Small | Low — convenience |
| **11** | #11 — Event cleanup in VMs | Small | Low — future-proofing |
| **12** | #12 — Deduplicate GdbDiff extensions | Trivial | None |

## Principles Applied

- **Single Responsibility:** Issues #1, #7 — components doing too much or holding bidirectional responsibilities
- **Open/Closed:** Issues #5, #9 — extending requires modifying existing code
- **Liskov Substitution:** Issue #5 — ViewModels depend on concrete types, preventing substitution
- **Interface Segregation:** Issues #2, #4, #6 — missing focused interfaces force dependencies on large concrete classes
- **Dependency Inversion:** Issues #1, #2, #3, #5 — high-level modules depend on low-level concrete implementations
- **KISS:** Issue #10 — tests are more complex than necessary because there's only one way to build the system
