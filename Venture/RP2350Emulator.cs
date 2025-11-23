using Microsoft.Extensions.Hosting;
using Venture.Processor;

namespace Venture;

public class RP2350Emulator : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool isRunning = true;

        var bootRom = new ReadWriteMemory("bootrom", 0x00000000, 1024 * 32); // 32kB
        bootRom.Load(@"Blink\riscv-bootrom.elf");

        var bootRam = new ReadWriteMemory("bootram", 0x400e0000, 1024); // 1kB

        var ram = new ReadWriteMemory("ram", 0x20000000, 1024 * 520); // 520kB

        var flash = new ReadWriteMemory("flash", 0x10000000, 1024 * 1024 * 2); // 2MB
        flash.Load(@"Blink\KeySquareBlink.elf");

        var m = new Memory([
            bootRom,
            bootRam,
            ram,
            flash,
            new ResetRegister(),  // Chapter 7. Resets
            new Peripheral(0x4003_0000), // 9.11.2 IO - QSPI Bank
            new Peripheral(0x5011_0000), // 12.7.5 - USB Registers
            new ClocksRegister(), // 8.1.6 - Clocks Registers
            new XOscRegister(), // 8.2.8 - XOSC Registers
        ]);

        var e = new Processor.Processor(m);

        e.PC = 0x10000036; // Start at reset vector

        e.CSR.Set(0xfbe5, 0x00008000); // TODO: this should be 0xbe5 - something is wrong with CSR instructions

        while (isRunning)
        {
            e.Step();
        }

        return Task.CompletedTask;
    }
}
