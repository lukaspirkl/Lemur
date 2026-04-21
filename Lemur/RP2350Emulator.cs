using Lemur.Processor;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lemur;

public class RP2350Emulator : BackgroundService, IDebuggable
{
    private readonly Hazard3Processor m_Processor;
    private readonly RegistersWrapper m_Registers;

    private bool m_Running = false;
    private Task? m_Run;

    public HashSet<uint> Brakpoints { get; } = new HashSet<uint>();

    public event Action? Stopped;

    public event Action? EBreak
    {
        add { m_Processor.EBreak += value; }
        remove { m_Processor.EBreak -= value; }
    }

    public RP2350Emulator(Hazard3Processor processor, IBusFabric busFabric)
    {
        m_Processor = processor;
        m_Processor.Memory = busFabric;
        m_Registers = new RegistersWrapper(processor);
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
        if (m_Run != null)
        {
            throw new InvalidOperationException("Already running");
        }

        m_Processor.Step();
    }

    public void Run()
    {
        if (m_Run != null)
        {
            throw new InvalidOperationException("Already running");
        }

        m_Running = true;
        m_Run = Task.Run(() =>
        {
            // TODO: This is wrong! It will swallow exception until Stop() is called!
            while (m_Running)
            {
                m_Processor.Step();
                if (Brakpoints.Contains(m_Processor.PC))
                {
                    break;
                }
            }

            m_Run = null;
            Stopped?.Invoke();
        });
    }

    public void RunTo(uint address)
    {
        while (m_Processor.PC != address)
            m_Processor.Step();
    }

    public void Stop()
    {
        if (m_Run == null)
        {
            throw new InvalidOperationException("Already stopped");
        }

        m_Running = false;
        m_Run.Wait();
        m_Run = null;
    }

    public void Reset()
    {
        m_Processor.PC = 0x00007dfc; // riscv_entry_point - it is always on this address

        SetCSR(0x300, 0x00001808); //MSTATUS
        SetCSR(0xBE5, 0x00008000); //meicontext
    }

    public void MemoryWrite(uint address, byte[] data)
    {
        var misalignement = address % 4;
        if (misalignement == 1 && data.Length == 1)
        {
            m_Processor.Memory.Write(address, data);
            return;
        }

        uint offset = 0;

        if (misalignement == 1)
        {
            offset = 1;
            m_Processor.Memory.Write(address + offset, data.Skip((int)offset).Take(2).ToArray());
            offset += 2;
        }

        if (misalignement == 2)
        {
            m_Processor.Memory.Write(address + offset, data.Take(2).ToArray());
            offset += 2;
        }

        if (misalignement == 3)
        {
            m_Processor.Memory.Write(address + offset, data.Take(1).ToArray());
            offset += 1;
        }

        while (offset < data.Length)
        {
            if (data.Length - offset == 3)
            {
                m_Processor.Memory.Write(address + offset, data.Skip((int)offset + 2).ToArray());
            }
            else
            {
                m_Processor.Memory.Write(address + offset, data.Skip((int)offset).Take(4).ToArray());
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
            result.AddRange(m_Processor.Memory.Read(address - misalignement, 4).Skip((int)misalignement));
            address += misalignement;
        }

        while (address < endAddress)
        {
            result.AddRange(m_Processor.Memory.Read(address, 4));
            address += 4;
        }

        return result.Take(count).ToArray();
    }

    public uint GetCSR(ushort index)
    {
        return m_Processor.CSR.Get(index);
    }

    public void SetCSR(ushort index, uint value)
    {
        m_Processor.CSR.Set(index, value);
    }

    public IRegisters Registers => m_Registers;

    /// <summary>
    /// Aggregate all registers with PC register as the last one
    /// </summary>
    private class RegistersWrapper : IRegisters
    {
        private readonly Hazard3Processor m_Processor;

        public RegistersWrapper(Hazard3Processor processor)
        {
            this.m_Processor = processor;
        }

        public uint Length => m_Processor.Registers.Length + 1;

        public uint this[uint index]
        {
            get
            {
                if (index == m_Processor.Registers.Length)
                {
                    return m_Processor.PC;
                }

                return m_Processor.Registers[index];
            }
            set
            {
                if (index == m_Processor.Registers.Length)
                {
                    m_Processor.PC = value;
                    return;
                }

                m_Processor.Registers[index] = value;
            }
        }
    }
}
