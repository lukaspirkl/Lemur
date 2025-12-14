using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Net;
using Venture.Debug;
using Venture.Peripherals;
using Venture.Processor;

namespace Venture;

internal class Program
{
    private static readonly string logFile = "Venture.clef";

    private static async Task Main(string[] args)
    {
        if (File.Exists(logFile))
        {
            File.Delete(logFile);
        }

        var builder = WebApplication.CreateSlimBuilder(args);

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Listen(IPAddress.Any, 3333, listenOptions =>
            {
                listenOptions.UseConnectionHandler<GdbConnectionHandler>();
            });
        });


        builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
            .MinimumLevel.Is(LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Async(a => a.File(new CompactJsonFormatter(), logFile))
            .WriteTo.Console());

        builder.Services.AddRP2350Emulator();

        var app = builder.Build();

        await app.RunAsync();
    }
}

public class XIP : IAddressableResource
{
    private readonly Memory memory;
    private readonly ILogger<XIP> logger;

    public uint StartAddress => 0x10000000;

    public uint Size => 0x10000000;

    public XIP(Memory memory, ILogger<XIP> logger)
    {
        this.memory = memory;
        this.logger = logger;
    }

    public byte[] Read(uint address, int count)
    {
        var offset = (address - StartAddress) % 0x0400_0000;
        
        var type = (address - StartAddress) / 0x0400_0000;
        if (type != 1)
        {
            logger.LogWarning("Reading from XIP address: {address} offset: {offset} type: {type}", address, offset, type);
        }

        return memory.Read(StartAddress + offset, count);
    }

    public void Write(uint address, byte[] data)
    {
        var offset = (address - StartAddress) % 0x0400_0000;

        var type = (address - StartAddress) / 0x0400_0000;
        if (type != 1)
        {
            logger.LogWarning("Writing to XIP address: {address} offset: {offset} type: {type}", address, offset, type);
        }

        memory.Write(StartAddress + offset, data);
    }
}

public static class RP2350ServiceCollectionExtensions
{
    private static IServiceCollection AddUnimpelentedPeripheral(this IServiceCollection services, uint address, string name, uint? size = null)
    {
        services.AddSingleton<IAddressableResource>(sp => sp.GetRequiredService<UnimplementedPeripheralFactory>().Create(address, name, size));
        return services;
    }

    private static IServiceCollection AddPeripheral<T>(this IServiceCollection services) where T: class, IAddressableResource
    {
        services.AddSingleton<IAddressableResource, T>();
        return services;
    }

    private static IServiceCollection AddMemory(this IServiceCollection services, string name, uint address, uint size, Action<Memory>? init = null, bool isReadonly = false)
    {
        services.AddSingleton<IAddressableResource>(sp => 
        {
            var memory = sp.GetRequiredService<MemoryFactory>().Create(name, address, size, isReadonly);
            init?.Invoke(memory);
            return memory;
        });

        return services;
    }

    private static IServiceCollection AddXIP(this IServiceCollection services, uint size, Action<Memory>? init = null)
    {
        services.AddSingleton<IAddressableResource>(sp =>
        {
            // This is not really read only but I want to catch possible issues
            var memory = sp.GetRequiredService<MemoryFactory>().Create("XIP", 0x10000000, size, isReadonly: true);
            init?.Invoke(memory);
            return new XIP(memory, sp.GetRequiredService<ILogger<XIP>>());
        });
        return services;
    }

    public static IServiceCollection AddRP2350Emulator(this IServiceCollection services)
    {
        services.AddSingleton<RP2350Emulator>();
        services.AddHostedService(x => x.GetRequiredService<RP2350Emulator>());
        services.AddSingleton<IDebuggable>(x => x.GetRequiredService<RP2350Emulator>());

        services.AddSingleton<Hazard3Processor>();
        services.AddSingleton<IBusFabric, BusFabric>();
        services.AddSingleton<Registers>();
        services.AddSingleton<CSR>();
        services.AddSingleton<MemoryFactory>();
        services.AddSingleton<UnimplementedPeripheralFactory>();


        // 0x00000000 - ROM
        services.AddMemory("ROM", 0x00000000, 1024 * 32, m => m.LoadBin(@"Blink\A2\bootrom-combined.bin"), isReadonly: true); // 32kB


        // 0x10000000 - XIP
        services.AddXIP(1024 * 1024 * 2, m => m.LoadBin(@"Blink\KeySquareBlink.bin")); // 2MB
        

        // 0x20000000 - SRAM
        services.AddMemory("SRAM", 0x20000000, 1024 * 520); // 520kB

        
        // 0x40000000 - APB Peripherals
        services.AddUnimpelentedPeripheral(0x40000000, "SYSINFO_BASE");
        services.AddUnimpelentedPeripheral(0x40008000, "SYSCFG_BASE");
        services.AddUnimpelentedPeripheral(0x40010000, "CLOCKS_BASE");
        services.AddUnimpelentedPeripheral(0x40018000, "PSM_BASE");
        services.AddPeripheral<Resets>(); //new UnimplementedPeripheral(0x40020000, "RESETS_BASE"),
        services.AddUnimpelentedPeripheral(0x40028000, "IO_BANK0_BASE");
        services.AddUnimpelentedPeripheral(0x40030000, "IO_QSPI_BASE");
        services.AddUnimpelentedPeripheral(0x40038000, "PADS_BANK0_BASE");
        services.AddPeripheral<PadsQSPI>(); // services.AddUnimpelentedPeripheral(0x40040000, "PADS_QSPI_BASE");
        services.AddUnimpelentedPeripheral(0x40048000, "XOSC_BASE");
        services.AddUnimpelentedPeripheral(0x40050000, "PLL_SYS_BASE");
        services.AddUnimpelentedPeripheral(0x40058000, "PLL_USB_BASE");
        services.AddUnimpelentedPeripheral(0x40060000, "ACCESSCTRL_BASE");
        services.AddUnimpelentedPeripheral(0x40068000, "BUSCTRL_BASE");
        services.AddUnimpelentedPeripheral(0x40070000, "UART0_BASE");
        services.AddUnimpelentedPeripheral(0x40078000, "UART1_BASE");
        services.AddUnimpelentedPeripheral(0x40080000, "SPI0_BASE");
        services.AddUnimpelentedPeripheral(0x40088000, "SPI1_BASE");
        services.AddUnimpelentedPeripheral(0x40090000, "I2C0_BASE");
        services.AddUnimpelentedPeripheral(0x40098000, "I2C1_BASE");
        services.AddUnimpelentedPeripheral(0x400a0000, "ADC_BASE");
        services.AddUnimpelentedPeripheral(0x400a8000, "PWM_BASE");
        services.AddUnimpelentedPeripheral(0x400b0000, "TIMER0_BASE");
        services.AddUnimpelentedPeripheral(0x400b8000, "TIMER1_BASE");
        services.AddUnimpelentedPeripheral(0x400c0000, "HSTX_CTRL_BASE");
        services.AddUnimpelentedPeripheral(0x400c8000, "XIP_CTRL_BASE");
        services.AddPeripheral<XIPQMI>(); // services.AddUnimpelentedPeripheral(0x400d0000, "XIP_QMI_BASE");
        services.AddUnimpelentedPeripheral(0x400d8000, "WATCHDOG_BASE");
        services.AddPeripheral<BootRAM>(); //services.AddMemory("bootRAM", 0x400e0000, 1024); // 1kB + BOOTRAM_BASE register
        services.AddUnimpelentedPeripheral(0x400e8000, "ROSC_BASE");
        services.AddUnimpelentedPeripheral(0x400f0000, "TRNG_BASE");
        services.AddPeripheral<Sha256>(); //new UnimplementedPeripheral(0x400f8000, "SHA256_BASE"),
        services.AddPeripheral<Powman>(); //new UnimplementedPeripheral(0x40100000, "POWMAN_BASE"),
        services.AddUnimpelentedPeripheral(0x40108000, "TICKS_BASE");
        services.AddPeripheral<OTP>(); // services.AddUnimpelentedPeripheral(0x40120000, "OTP_BASE");
        services.AddPeripheral<OTPData>(); //services.AddUnimpelentedPeripheral(0x40130000, "OTP_DATA_BASE");
        services.AddUnimpelentedPeripheral(0x40134000, "OTP_DATA_RAW_BASE");
        services.AddUnimpelentedPeripheral(0x40138000, "OTP_DATA_GUARDED_BASE");
        services.AddUnimpelentedPeripheral(0x4013c000, "OTP_DATA_RAW_GUARDED_BASE");
        services.AddUnimpelentedPeripheral(0x40140000, "CORESIGHT_PERIPH_BASE");
        services.AddUnimpelentedPeripheral(0x40140000, "CORESIGHT_ROMTABLE_BASE");
        services.AddUnimpelentedPeripheral(0x40142000, "CORESIGHT_AHB_AP_CORE0_BASE");
        services.AddUnimpelentedPeripheral(0x40144000, "CORESIGHT_AHB_AP_CORE1_BASE");
        services.AddUnimpelentedPeripheral(0x40146000, "CORESIGHT_TIMESTAMP_GEN_BASE");
        services.AddUnimpelentedPeripheral(0x40147000, "CORESIGHT_ATB_FUNNEL_BASE");
        services.AddUnimpelentedPeripheral(0x40148000, "CORESIGHT_TPIU_BASE");
        services.AddUnimpelentedPeripheral(0x40149000, "CORESIGHT_CTI_BASE");
        services.AddUnimpelentedPeripheral(0x4014a000, "CORESIGHT_APB_AP_RISCV_BASE");
        services.AddUnimpelentedPeripheral(0x40150000, "DFT_BASE");
        services.AddUnimpelentedPeripheral(0x40158000, "GLITCH_DETECTOR_BASE");
        services.AddUnimpelentedPeripheral(0x40160000, "TBMAN_BASE");

        
        // 0x50000000 - AHB Peripherals
        services.AddUnimpelentedPeripheral(0x50000000, "DMA_BASE");

        //services.AddUnimpelentedPeripheral(0x50100000, "USBCTRL_BASE");
        //services.AddUnimpelentedPeripheral(0x50100000, "USBCTRL_DPRAM_BASE");
        services.AddMemory("USB DPRAM", 0x50100000, 1024 * 4); // 4kB

        services.AddUnimpelentedPeripheral(0x50110000, "USBCTRL_REGS_BASE");
        services.AddUnimpelentedPeripheral(0x50200000, "PIO0_BASE");
        services.AddUnimpelentedPeripheral(0x50300000, "PIO1_BASE");
        services.AddUnimpelentedPeripheral(0x50400000, "PIO2_BASE");
        services.AddUnimpelentedPeripheral(0x50500000, "XIP_AUX_BASE");
        services.AddUnimpelentedPeripheral(0x50600000, "HSTX_FIFO_BASE");
        services.AddUnimpelentedPeripheral(0x50700000, "CORESIGHT_TRACE_BASE");


        // 0xd0000000 - SIO
        services.AddPeripheral<SIO>(); //new UnimplementedPeripheral(0xd0000000, "SIO_BASE"),
        services.AddUnimpelentedPeripheral(0xd0020000, "SIO_NONSEC_BASE");


        // 0xe0000000 - Cortex-M33 private registers

        return services;
    }
}


// https://github.com/andrewlock/blog-comments/discussions/229#discussioncomment-10187328