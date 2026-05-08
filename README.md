# Lemur

<img src="logo.png" alt="Lemur logo" width=150>

**A RISC-V emulator of the Raspberry Pi RP2350 — written in C# / .NET 10.**

Lemur runs RP2350 firmware against the real Raspberry Pi A2 boot ROM, executes the
Hazard3 RISC-V core, and exposes a GDB server so you can debug your firmware
from any GDB the same way you would on real hardware.

---

> [!WARNING]
> **Early-stage development.**
>
> Lemur is a work-in-progress hobby project. The CPU core is in good shape and many
> simple firmware programs run end-to-end, but **most peripherals are stubs or only
> partially implemented**. Expect missing registers, missing interrupts, missing
> timing, and outright bugs. Do not use Lemur for anything that depends on
> hardware-accurate behaviour.

---

## What it can do today

- Execute Hazard3 RISC-V firmware built with the official Pico SDK against the real
  RP2350 A2 boot ROM.
- Act as a **GDB remote target** — drop-in replacement for `openocd` from GDB's
  point of view (see [GDB debugging](#gdb-debugging) below).
- Drive a small Avalonia UI that visualises GPIO pin state, UART traffic, CSR
  contents, and a basic logic analyser, and lets you wire virtual devices
  (LEDs, buttons, switches, serial terminals) to the chip's pins.
- Run the included example binaries (`Examples/blink_simple.bin`,
  `hello_uart.bin`, `hello_gpio_irq.bin`, …).

## RP2350 feature coverage

Legend: ✅ implemented · 🟡 partial / mock · ❌ not implemented

### Processor & core architecture

| Feature | Status | Notes |
| --- | --- | --- |
| Hazard3 RV32I base ISA | ✅ | |
| `M` extension (multiply/divide) | ✅ | |
| `A` extension (atomics, LR/SC) | ✅ | |
| `C` extension (compressed) | ✅ | |
| `Zba` / `Zbb` / `Zbs` (bit manip) | ✅ | |
| `Zbkb` / `Zcb` / `Zcmp` | ✅ | |
| Hazard3 custom: `Xh3bextm`, `Xh3power`, `Xh3irq` | ✅ | |
| Machine + User privilege modes | ✅ | |
| Standard M-mode CSRs (mstatus, mtvec, mepc, mcause, mip, mie, …) | ✅ | |
| PMP (Physical Memory Protection) | ✅ | Configurable region count and grain |
| Performance counters (mcycle / minstret) | ✅ | |
| Debug-mode CSRs (`dcsr`, `dpc`) | 🟡 | Minimal — used by the GDB stub |
| Triggers (`tselect`, …) | 🟡 | Stub entries only |
| Second Hazard3 core (dual-core) | ❌ | Only core 0 is emulated |
| Cortex-M33 cores (the chip's other personality) | ❌ | RISC-V mode only |

### Memory map

| Region | Status | Notes |
| --- | --- | --- |
| Boot ROM (32 KB) | ✅ | Real RP2350 A2 boot ROM is embedded |
| SRAM (520 KB) | ✅ | |
| XIP / external flash (up to 16 MB window, 2 MB backing) | 🟡 | Basic read/write; cache and aliases not modelled |
| USB DPRAM (4 KB) | 🟡 | Modelled as plain RAM |

### Peripherals

| Peripheral | Status | Notes |
| --- | --- | --- |
| SIO (GPIO low/high banks, output/OE/XOR) | ✅ | |
| IO_BANK0 (user-bank GPIO mux) | ✅ | |
| IO_QSPI (USB/QSPI bank GPIO mux) | ✅ | |
| PADS_BANK0 / PADS_QSPI | ✅ | |
| UART0 / UART1 (PL011) | ✅ | TX serialisation, RX, baud, IRQs |
| Watchdog | ✅ | Countdown, reset, scratch regs, REASON |
| Boot RAM (incl. boot locks) | ✅ | |
| TRNG | ✅ | Backed by `RandomNumberGenerator` |
| SHA-256 accelerator | ✅ | Instant digest (no timing) |
| TIMER0 / TIMER1 | 🟡 | Free-running counter only; alarms / IRQs not wired |
| CLOCKS | 🟡 | Source/divider readback; no real clock-tree simulation |
| RESETS | 🟡 | RESET_DONE always reports "out of reset" |
| XOSC, PLL_SYS, PLL_USB, ROSC | 🟡 | Status bits faked so SDK init proceeds |
| XIP_QMI | 🟡 | Identification register only |
| POWMAN | 🟡 | Enough to satisfy SDK boot path |
| OTP / OTP_DATA | 🟡 | A handful of fixed values |
| USBCTRL_REGS | 🟡 | Single status register faked |
| SYSINFO, SYSCFG, PSM, ACCESSCTRL, BUSCTRL | ❌ | Unimplemented stub |
| SPI0 / SPI1 | ❌ | Unimplemented stub |
| I2C0 / I2C1 | ❌ | Unimplemented stub |
| ADC | ❌ | Unimplemented stub |
| PWM | ❌ | Unimplemented stub |
| PIO0 / PIO1 / PIO2 | ❌ | Unimplemented stub |
| DMA | ❌ | Unimplemented stub |
| HSTX_CTRL / HSTX_FIFO / XIP_CTRL / XIP_AUX | ❌ | Unimplemented stub |
| TICKS | ❌ | Unimplemented stub |
| TBMAN, DFT, GLITCH_DETECTOR | ❌ | Unimplemented stub |
| CoreSight components (ROM table, AHB-AP, CTI, TPIU, trace, …) | ❌ | Unimplemented stub |

> "Unimplemented stub" means the address range is decoded and reads return 0
> without crashing, but no register behaviour is modelled. "Partial / mock"
> means specific registers return values that allow the SDK or boot ROM to make
> progress, but the peripheral does not really do anything.

## GDB debugging

Lemur ships a GDB remote-protocol server on **TCP port 3333** — the same port
OpenOCD listens on by default. As far as GDB is concerned, Lemur is a **drop-in
replacement for `openocd`**: just point GDB at it.

```gdb
file your_firmware.elf
set architecture riscv:rv32
target remote localhost:3333
```

The supplied [`GDB/sim.gdb`](GDB/sim.gdb) script does exactly this. You can use
plain `riscv32-unknown-elf-gdb` (or the `gdb-multiarch` shipped with the Pico
toolchain); no OpenOCD process is required.

Supported GDB commands include reading/writing registers and CSRs, reading/writing
memory, software breakpoints, single-step, continue, and reset.

## Building & running

Requires the **.NET 10 SDK**.

```sh
dotnet run --project Lemur/Lemur.csproj
```

The Avalonia window opens and the GDB server starts listening on
`localhost:3333`. Load a firmware image via GDB's `load` command (or use one of
the example `.bin` files under `Examples/`).

## Repository layout

| Path | Purpose |
| --- | --- |
| `Lemur/` | The emulator and Avalonia UI (main project) |
| `Tests/` | xUnit tests, including official RISC-V compliance suite |
| `GdbDiff/` | Tool that runs Lemur and real hardware in lock-step and diffs state |
| `Docs/` | RP2350 and Hazard3 datasheets, split into per-chapter Markdown |
| `Examples/` | Pre-built example firmware images |
