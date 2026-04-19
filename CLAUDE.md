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
```
