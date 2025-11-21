using Venture.Processor;

namespace Venture;

internal class Program
{
    private static void Main(string[] args)
    {
        bool isRunning = true;

        var bootRom = new ReadWriteMemory("bootrom", 0x00000000, 1024 * 32); // 32kB
        bootRom.Load(@"Blink\riscv-bootrom.elf");

        var bootRam = new ReadWriteMemory("bootram", 0x400e0000, 1024); // 1kB

        var ram = new ReadWriteMemory("ram", 0x20000000, 1024 * 520); // 520kB

        var flash = new ReadWriteMemory("flash", 0x10000000, 1024 * 1024 * 2); // 2MB
        flash.Load(@"Blink\KeySquareBlink.elf");

        // Chapter 7. Resets
        var reset = new ResetRegister();
        var atomic_set = new ReadWriteMemory("atomic set", 0x40022000, 0xFFF);
        var atomic_clear = new ReadWriteMemory("atomic clear", 0x40023000, 0xFFF);

        // 9.11.2 IO - QSPI Bank
        var qspi_bank = new ReadWriteMemory("reset", 0x40030000, 0x23C);


        var m = new Memory([bootRom, bootRam, ram, flash, reset, atomic_set, atomic_clear, qspi_bank]);

        var e = new Processor.Processor(m);

        e.PC = 0x10000036; // Start at reset vector

        e.CSR.Set(0xfbe5, 0x00008000); // TODO: this should be 0xbe5 - something is wrong with CSR instructions

        while (isRunning)
        {
            e.Step();
        }
    }
}
