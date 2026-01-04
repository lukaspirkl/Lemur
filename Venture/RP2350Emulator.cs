using Microsoft.Extensions.Hosting;
using Venture.Processor;

namespace Venture;

public class RP2350Emulator : BackgroundService, IDebuggable
{
    private readonly Hazard3Processor processor;
    private readonly RegistersWrapper registers;

    private bool running = false;
    private Task? run;

    public HashSet<uint> Brakpoints { get; } = new HashSet<uint>();

    public event EventHandler? Stopped;

    public RP2350Emulator(Hazard3Processor processor)
    {
        this.processor = processor;
        registers = new RegistersWrapper(processor);
        Reset();
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
                if (Brakpoints.Contains(processor.PC))
                {
                    break;
                }
            }

            run = null;
            Stopped?.Invoke(this, EventArgs.Empty);
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

    public void Reset()
    {
        processor.PC = 0x00007dfc; // riscv_entry_point - it is always on this address

        SetCSR(0x300, 0x00001808); //MSTATUS
        SetCSR(0xBE5, 0x00008000); //meicontext
    }

    public void MemoryWrite(uint address, byte[] data)
    {
        var misalignement = address % 4;
        if (misalignement == 1 && data.Length == 1)
        {
            processor.Memory.Write(address, data);
            return;
        }

        uint offset = 0;

        if (misalignement == 1)
        {
            offset = 1;
            processor.Memory.Write(address + offset, data.Skip((int)offset).Take(2).ToArray());
            offset += 2;
        }

        if (misalignement == 2)
        {
            processor.Memory.Write(address + offset, data.Take(2).ToArray());
            offset += 2;
        }

        if (misalignement == 3)
        {
            processor.Memory.Write(address + offset, data.Take(1).ToArray());
            offset += 1;
        }

        while (offset < data.Length)
        {
            if (data.Length - offset == 3)
            {
                processor.Memory.Write(address + offset, data.Skip((int)offset + 2).ToArray());
            }
            else
            {
                processor.Memory.Write(address + offset, data.Skip((int)offset).Take(4).ToArray());
            }
            offset += 4;
        }

    }

    public byte[] MemoryRead(uint address, int count)
    {
        var endAddress = address + count;
        var result = new List<byte>();

        var misalignement = address % 4;
        if (misalignement != 0)
        {
            result.AddRange(processor.Memory.Read(address - misalignement, 4).Skip((int)misalignement));
            address += misalignement;
        }

        while (address < endAddress)
        {
            result.AddRange(processor.Memory.Read(address, 4));
            address += 4;
        }

        return result.Take(count).ToArray();
    }

    public uint GetCSR(ushort index)
    {
        return processor.CSR.Get(index);
    }

    public void SetCSR(ushort index, uint value)
    {
        processor.CSR.Set(index, value);
    }

    public IRegisters Registers => registers;

    /// <summary>
    /// Aggregate all registers with PC register as the last one
    /// </summary>
    private class RegistersWrapper : IRegisters
    {
        private readonly Hazard3Processor processor;

        public RegistersWrapper(Hazard3Processor processor)
        {
            this.processor = processor;
        }

        public uint Length => processor.Registers.Length + 1;

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
