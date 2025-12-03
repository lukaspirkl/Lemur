using Microsoft.Extensions.Hosting;
using Venture.Peripherals;
using Venture.Processor;

namespace Venture;

public class RP2350Emulator : BackgroundService, IEmulator
{
    private readonly Hazard3Processor processor;
    private readonly BusFabric bus;
    private readonly RegistersWrapper registers;

    private bool running = false;
    private Task? run;

    public RP2350Emulator()
    {
        var rom = new Memory("ROM", 0x00000000, 1024 * 32); // 32kB
        //rom.LoadBin(@"Blink\bootrom-combined.bin");
        rom.LoadBin(@"Blink\bootrom-dumped.bin");

        var xip = new Memory("XIP", 0x10000000, 1024 * 1024 * 2); // 2MB
        xip.LoadElf(@"Blink\KeySquareBlink.elf");

        var resources = new IAddressableResource[]
        {
            // 0x00000000 - ROM
            rom,
            // 0x10000000 - XIP
            xip,
            // 0x20000000 - SRAM
            new Memory("SRAM", 0x20000000, 1024 * 520), // 520kB
            // 0x40000000 - APB Peripherals
            new UnimplementedPeripheral(0x40000000, "SYSINFO_BASE"),
            new UnimplementedPeripheral(0x40008000, "SYSCFG_BASE"),
            new UnimplementedPeripheral(0x40010000, "CLOCKS_BASE"),
            new UnimplementedPeripheral(0x40018000, "PSM_BASE"),
            new UnimplementedPeripheral(0x40020000, "RESETS_BASE"),
            new UnimplementedPeripheral(0x40028000, "IO_BANK0_BASE"),
            new UnimplementedPeripheral(0x40030000, "IO_QSPI_BASE"),
            new UnimplementedPeripheral(0x40038000, "PADS_BANK0_BASE"),
            new UnimplementedPeripheral(0x40040000, "PADS_QSPI_BASE"),
            new UnimplementedPeripheral(0x40048000, "XOSC_BASE"),
            new UnimplementedPeripheral(0x40050000, "PLL_SYS_BASE"),
            new UnimplementedPeripheral(0x40058000, "PLL_USB_BASE"),
            new UnimplementedPeripheral(0x40060000, "ACCESSCTRL_BASE"),
            new UnimplementedPeripheral(0x40068000, "BUSCTRL_BASE"),
            new UnimplementedPeripheral(0x40070000, "UART0_BASE"),
            new UnimplementedPeripheral(0x40078000, "UART1_BASE"),
            new UnimplementedPeripheral(0x40080000, "SPI0_BASE"),
            new UnimplementedPeripheral(0x40088000, "SPI1_BASE"),
            new UnimplementedPeripheral(0x40090000, "I2C0_BASE"),
            new UnimplementedPeripheral(0x40098000, "I2C1_BASE"),
            new UnimplementedPeripheral(0x400a0000, "ADC_BASE"),
            new UnimplementedPeripheral(0x400a8000, "PWM_BASE"),
            new UnimplementedPeripheral(0x400b0000, "TIMER0_BASE"),
            new UnimplementedPeripheral(0x400b8000, "TIMER1_BASE"),
            new UnimplementedPeripheral(0x400c0000, "HSTX_CTRL_BASE"),
            new UnimplementedPeripheral(0x400c8000, "XIP_CTRL_BASE"),
            new UnimplementedPeripheral(0x400d0000, "XIP_QMI_BASE"),
            new UnimplementedPeripheral(0x400d8000, "WATCHDOG_BASE"),
            new Memory("bootRAM", 0x400e0000, 1024), // 1kB
            //new UnimplementedPeripheral(0x400e0000, "BOOTRAM_BASE"),
            //new UnimplementedPeripheral(0x400e0400, "BOOTRAM_END"),
            new UnimplementedPeripheral(0x400e8000, "ROSC_BASE"),
            new UnimplementedPeripheral(0x400f0000, "TRNG_BASE"),
            new UnimplementedPeripheral(0x400f8000, "SHA256_BASE"),
            new UnimplementedPeripheral(0x40100000, "POWMAN_BASE"),
            new UnimplementedPeripheral(0x40108000, "TICKS_BASE"),
            new UnimplementedPeripheral(0x40120000, "OTP_BASE"),
            new UnimplementedPeripheral(0x40130000, "OTP_DATA_BASE"),
            new UnimplementedPeripheral(0x40134000, "OTP_DATA_RAW_BASE"),
            new UnimplementedPeripheral(0x40138000, "OTP_DATA_GUARDED_BASE"),
            new UnimplementedPeripheral(0x4013c000, "OTP_DATA_RAW_GUARDED_BASE"),
            new UnimplementedPeripheral(0x40140000, "CORESIGHT_PERIPH_BASE"),
            new UnimplementedPeripheral(0x40140000, "CORESIGHT_ROMTABLE_BASE"),
            new UnimplementedPeripheral(0x40142000, "CORESIGHT_AHB_AP_CORE0_BASE"),
            new UnimplementedPeripheral(0x40144000, "CORESIGHT_AHB_AP_CORE1_BASE"),
            new UnimplementedPeripheral(0x40146000, "CORESIGHT_TIMESTAMP_GEN_BASE"),
            new UnimplementedPeripheral(0x40147000, "CORESIGHT_ATB_FUNNEL_BASE"),
            new UnimplementedPeripheral(0x40148000, "CORESIGHT_TPIU_BASE"),
            new UnimplementedPeripheral(0x40149000, "CORESIGHT_CTI_BASE"),
            new UnimplementedPeripheral(0x4014a000, "CORESIGHT_APB_AP_RISCV_BASE"),
            new UnimplementedPeripheral(0x40150000, "DFT_BASE"),
            new UnimplementedPeripheral(0x40158000, "GLITCH_DETECTOR_BASE"),
            new UnimplementedPeripheral(0x40160000, "TBMAN_BASE"),
            // 0x50000000 - AHB Peripherals
            new UnimplementedPeripheral(0x50000000, "DMA_BASE"),
            new UnimplementedPeripheral(0x50100000, "USBCTRL_BASE"),
            new UnimplementedPeripheral(0x50100000, "USBCTRL_DPRAM_BASE"),
            new UnimplementedPeripheral(0x50110000, "USBCTRL_REGS_BASE"),
            new UnimplementedPeripheral(0x50200000, "PIO0_BASE"),
            new UnimplementedPeripheral(0x50300000, "PIO1_BASE"),
            new UnimplementedPeripheral(0x50400000, "PIO2_BASE"),
            new UnimplementedPeripheral(0x50500000, "XIP_AUX_BASE"),
            new UnimplementedPeripheral(0x50600000, "HSTX_FIFO_BASE"),
            new UnimplementedPeripheral(0x50700000, "CORESIGHT_TRACE_BASE"),
            // 0xd0000000 - SIO
            //new UnimplementedPeripheral(0xd0000000, "SIO_BASE"),
            new PeriphheralHost(new SIO()),
            new UnimplementedPeripheral(0xd0020000, "SIO_NONSEC_BASE"),
            // 0xe0000000 - Cortex-M33 private registers
        };

        bus = new BusFabric(resources);
        processor = new Hazard3Processor(bus);
        registers = new RegistersWrapper(processor);

        processor.PC = 0x00007dfc; // riscv_entry_point - it is always on this address

        processor.CSR.Set(0xfbe5, 0x00008000); // TODO: this should be 0xbe5 - something is wrong with CSR instructions
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
