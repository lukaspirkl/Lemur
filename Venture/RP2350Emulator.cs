using Microsoft.Extensions.Hosting;
using Venture.Processor;

namespace Venture;

public class RP2350Emulator : BackgroundService, IDebuggable
{
    private readonly Hazard3Processor processor;
    private readonly RegistersWrapper registers;

    private bool running = false;
    private Task? run;

    public RP2350Emulator(Hazard3Processor processor)
    {
        this.processor = processor;

        registers = new RegistersWrapper(processor);

        processor.PC = 0x00007dfc; // riscv_entry_point - it is always on this address

        processor.CSR.Set(0xbe5, 0x00008000);

        // This is required for the hint tests. Machine Timer Interrupt Pending (MTIP) bit should be set to 1.
        // https://riscv-software-src.github.io/riscv-unified-db/manual/html/isa/isa_20240411/csrs/mip.html#mip-MTIP-def
        processor.CSR.Set(0x344, 0x00000080);
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

    public byte[] MemoryRead(uint address, int count)
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
