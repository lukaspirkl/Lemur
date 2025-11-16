using Venture.Processor;

namespace Venture;

internal class Program
{
    private static void Main(string[] args)
    {
        bool isRunning = true;

        var m = new Memory([@"Blink\riscv-bootrom.elf", @"Blink\KeySquareBlink.elf"], 
            0x10000000, 1024 * 1024 * 2, 
            0x20000000, 1024 * 520, 
            0x00000000, 1024 * 32);

        var e = new Processor.Processor(m);

        e.PC = 0x10000036; // Start at reset vector

        e.CSR.Set(0xfbe5, 0x00008000); // TODO: this should be 0xbe5 - something is wrong with CSR instructions

        while (isRunning)
        {
            e.Step();
        }
    }
}
