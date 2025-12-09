using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net;
using Venture.Debug;
using Venture.Peripherals;
using Venture.Processor;

namespace Venture;

internal class Program
{
    private static OpenTelemetryDebugListener? _otelDebug;

    public static readonly ActivitySource ActivitySource = new ActivitySource("RP2350");

    private static async Task Main(string[] args)
    {
        _otelDebug = new OpenTelemetryDebugListener();

        var builder = WebApplication.CreateSlimBuilder(args);

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Listen(IPAddress.Any, 3333, listenOptions =>
            {
                listenOptions.UseConnectionHandler<GdbConnectionHandler>();
            });
        });


        builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource =>
        {
            resource.AddService("RP2350Emulator");
        })
        .WithTracing(c =>
        {
            c.AddAspNetCoreInstrumentation();
            c.AddSource("RP2350");
            c.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("https://seq.prkl.cz/ingest/otlp/v1/traces");
                o.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
            c.AddConsoleExporter();
        });
        //.WithMetrics(c =>
        //{
        //    c.AddAspNetCoreInstrumentation();
        //    c.AddOtlpExporter(o =>
        //    {
        //        o.Endpoint = new Uri("https://seq.prkl.cz/ingest/otlp/v1/metrics");
        //        o.Protocol = OtlpExportProtocol.HttpProtobuf;
        //    });
        //    c.AddConsoleExporter();
        //});

        builder.Logging.AddOpenTelemetry(c =>
        {
            c.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("https://seq.prkl.cz/ingest/otlp/v1/logs");
                o.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
            c.AddConsoleExporter();
        });


        builder.Services.AddRP2350Emulator();

        var app = builder.Build();

        using var activity = Program.ActivitySource.StartActivity("Main");
        await app.RunAsync();
    }
}


public static class RP2350ServiceCollectionExtensions
{
    private static IServiceCollection AddUnimpelentedPeripheral(this IServiceCollection services, uint address, string name)
    {
        services.AddSingleton<IAddressableResource>(sp => sp.GetRequiredService<UnimplementedPeripheralFactory>().Create(address, name));
        return services;
    }

    private static IServiceCollection AddUnimpelentedPeripheral(this IServiceCollection services, uint address, uint size, string name)
    {
        services.AddSingleton<IAddressableResource>(sp => sp.GetRequiredService<UnimplementedPeripheralFactory>().Create(address, name, size));
        return services;
    }

    private static IServiceCollection AddPeripheral<T>(this IServiceCollection services) where T: class, IAddressableResource
    {
        services.AddSingleton<IAddressableResource, T>();
        return services;
    }

    public static IServiceCollection AddRP2350Emulator(this IServiceCollection services)
    {
        services.AddSingleton<RP2350Emulator>();
        services.AddHostedService(x => x.GetRequiredService<RP2350Emulator>());
        services.AddSingleton<IEmulator>(x => x.GetRequiredService<RP2350Emulator>());

        services.AddSingleton<Hazard3Processor>();
        services.AddSingleton<IBusFabric, BusFabric>();
        services.AddSingleton<Registers>();

        services.AddSingleton<UnimplementedPeripheralFactory>();

        // 0x00000000 - ROM
        var rom = new Memory("ROM", 0x00000000, 1024 * 32); // 32kB
        rom.LoadBin(@"Blink\A2\bootrom-combined.bin");
        services.AddSingleton<IAddressableResource>(rom);

        // 0x10000000 - XIP
        var xip = new Memory("XIP", 0x10000000, 1024 * 1024 * 2); // 2MB
        xip.LoadElf(@"Blink\KeySquareBlink.elf");
        services.AddSingleton<IAddressableResource>(xip);

        // 0x20000000 - SRAM
        services.AddSingleton<IAddressableResource>(new Memory("SRAM", 0x20000000, 1024 * 520)); // 520kB

        // 0x40000000 - APB Peripherals
        services.AddUnimpelentedPeripheral(0x40000000, "SYSINFO_BASE");
        services.AddUnimpelentedPeripheral(0x40008000, "SYSCFG_BASE");
        services.AddUnimpelentedPeripheral(0x40010000, "CLOCKS_BASE");
        services.AddUnimpelentedPeripheral(0x40018000, "PSM_BASE");
        services.AddPeripheral<Resets>();
        //new UnimplementedPeripheral(0x40020000, "RESETS_BASE"),
        services.AddUnimpelentedPeripheral(0x40028000, "IO_BANK0_BASE");
        services.AddUnimpelentedPeripheral(0x40030000, "IO_QSPI_BASE");
        services.AddUnimpelentedPeripheral(0x40038000, "PADS_BANK0_BASE");
        services.AddUnimpelentedPeripheral(0x40040000, "PADS_QSPI_BASE");
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
        services.AddUnimpelentedPeripheral(0x400d0000, "XIP_QMI_BASE");
        services.AddUnimpelentedPeripheral(0x400d8000, "WATCHDOG_BASE");
        services.AddSingleton<IAddressableResource>(new Memory("bootRAM", 0x400e0000, 1024)); // 1kB
        services.AddUnimpelentedPeripheral(0x400e0800, 0x02c, "BOOTRAM_BASE");
        services.AddUnimpelentedPeripheral(0x400e8000, "ROSC_BASE");
        services.AddUnimpelentedPeripheral(0x400f0000, "TRNG_BASE");
        services.AddPeripheral<Sha256>();
        //new UnimplementedPeripheral(0x400f8000, "SHA256_BASE"),
        services.AddPeripheral<Powman>();
        //new UnimplementedPeripheral(0x40100000, "POWMAN_BASE"),
        services.AddUnimpelentedPeripheral(0x40108000, "TICKS_BASE");
        services.AddUnimpelentedPeripheral(0x40120000, "OTP_BASE");
        services.AddUnimpelentedPeripheral(0x40130000, "OTP_DATA_BASE");
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
        services.AddUnimpelentedPeripheral(0x50100000, "USBCTRL_BASE");
        services.AddUnimpelentedPeripheral(0x50100000, "USBCTRL_DPRAM_BASE");
        services.AddUnimpelentedPeripheral(0x50110000, "USBCTRL_REGS_BASE");
        services.AddUnimpelentedPeripheral(0x50200000, "PIO0_BASE");
        services.AddUnimpelentedPeripheral(0x50300000, "PIO1_BASE");
        services.AddUnimpelentedPeripheral(0x50400000, "PIO2_BASE");
        services.AddUnimpelentedPeripheral(0x50500000, "XIP_AUX_BASE");
        services.AddUnimpelentedPeripheral(0x50600000, "HSTX_FIFO_BASE");
        services.AddUnimpelentedPeripheral(0x50700000, "CORESIGHT_TRACE_BASE");

        // 0xd0000000 - SIO
        //new UnimplementedPeripheral(0xd0000000, "SIO_BASE"),
        services.AddPeripheral<SIO>();
        services.AddUnimpelentedPeripheral(0xd0020000, "SIO_NONSEC_BASE");

        // 0xe0000000 - Cortex-M33 private registers

        return services;
    }
}


internal class OpenTelemetryDebugListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        // STRICT FILTER: Only enable sources that start with "OpenTelemetry-"
        if (eventSource.Name != null && eventSource.Name.StartsWith("OpenTelemetry-"))
        {
            EnableEvents(eventSource, EventLevel.Verbose);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        // Double check to be sure (in case events were enabled before the filter was applied)
        if (!eventData.EventSource.Name.StartsWith("OpenTelemetry-"))
        {
            return;
        }

        // Try to format the message using the standard formatted string
        string message;
        if (eventData.Message != null)
        {
            try
            {
                message = string.Format(eventData.Message, eventData.Payload?.ToArray() ?? Array.Empty<object>());
            }
            catch
            {
                // Fallback if formatting fails
                message = eventData.Message;
            }
        }
        else
        {
            // If there is no message, just join the payload parts
            message = eventData.Payload != null
                ? string.Join(", ", eventData.Payload)
                : "No payload";
        }

        Console.WriteLine($"[OTEL-DEBUG] [{eventData.EventSource.Name}] {message}");
    }
}