using Microsoft.Extensions.Hosting;
using Venture.Processor;

namespace Venture;

public class RP2350Emulator : BackgroundService, IEmulator
{
    private readonly Hazard3Processor processor;
    private readonly Memory memory;
    private readonly RegistersWrapper registers;

    private bool running = false;
    private Task? run;

    public RP2350Emulator()
    {
        var bootRom = new ReadWriteMemory("bootrom", 0x00000000, 1024 * 32); // 32kB
        //bootRom.LoadElf(@"Blink\riscv-bootrom.elf");
        bootRom.LoadBin(@"Blink\bootrom-combined.bin");

        var bootRam = new ReadWriteMemory("bootram", 0x400e0000, 1024); // 1kB

        var ram = new ReadWriteMemory("ram", 0x20000000, 1024 * 520); // 520kB

        var flash = new ReadWriteMemory("flash", 0x10000000, 1024 * 1024 * 2); // 2MB
        flash.LoadElf(@"Blink\KeySquareBlink.elf");

        memory = new Memory([
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

        processor = new Hazard3Processor(memory);

        //processor.PC = 0x10000036; // Start at reset vector

        processor.CSR.Set(0xfbe5, 0x00008000); // TODO: this should be 0xbe5 - something is wrong with CSR instructions

        registers = new RegistersWrapper(processor);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: When running on background it should probably be done here to handle exeptions correctly
        // Or maybe this shouldn't be BacgroundService at all
        return Task.CompletedTask;
    }

    public void Step()
    {
        if (run != null)
        {
            throw new InvalidOperationException("Already running");
        }

        processor.Step();
    }

    public void Run()
    {
        if (run != null)
        {
            throw new InvalidOperationException("Already running");
        }

        running = true;
        run = Task.Run(() =>
        {
            // TODO: This is wrong! It will swallow exception until Stop() is called!
            while (running)
            {
                processor.Step();
            }
        });
    }

    public void Stop()
    {
        if (run == null)
        {
            throw new InvalidOperationException("Already stopped");
        }

        running = false;
        run.Wait();
        run = null;
    }


    public void MemoryWrite(uint address, byte[] data)
    {
        processor.Memory.Write(address, data);
    }

    public ArraySegment<byte> MemoryRead(uint address, int count)
    {
        return processor.Memory.Read(address, count);
    }

    public IIndexable<uint> Registers => registers;

    /// <summary>
    /// Aggregate all registers with PC register as the last one
    /// </summary>
    private class RegistersWrapper : IIndexable<uint>
    {
        private readonly Hazard3Processor processor;

        public RegistersWrapper(Hazard3Processor processor)
        {
            this.processor = processor;
        }

        public int Length => processor.Registers.Length + 1;

        public uint this[uint index]
        {
            get
            {
                if (index == processor.Registers.Length)
                {
                    return processor.PC;
                }

                return processor.Registers[index];
            }
            set
            {
                if (index == processor.Registers.Length)
                {
                    processor.PC = value;
                    return;
                }

                processor.Registers[index] = value;
            }
        }

    }
}
