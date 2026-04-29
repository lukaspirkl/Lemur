using Lemur.Csr;
using Lemur.ExternalDevices;
using Lemur.Peripherals;
using Lemur.Peripherals.PadControl;
using Lemur.Peripherals.Sio;
using Lemur.Peripherals.Uart;
using Lemur.Peripherals.UserBankIO;
using Lemur.Processor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Lemur;

public static class RP2350ServiceCollectionExtensions
{
    public static IServiceCollection AddRP2350Emulator(this IServiceCollection services)
    {
        services.AddSingleton<RP2350Emulator>();
        services.AddHostedService(x => x.GetRequiredService<RP2350Emulator>());
        services.AddSingleton<IDebuggable>(x => x.GetRequiredService<RP2350Emulator>());

        services.AddSingleton<BinaryInfoService>();
        services.AddSingleton<Hazard3Processor>();
        services.AddSingleton<IBusFabric, BusFabric>();
        services.AddSingleton<Registers>();
        services.AddSingleton<CsrController>();
        services.AddSingleton<MemoryFactory>();
        services.AddSingleton<UnimplementedPeripheralFactory>();

        // 0x00000000 - ROM
        services.AddMemory("ROM", 0x00000000, 1024 * 32, m => 
        {
            var stream = typeof(RP2350ServiceCollectionExtensions).Assembly.GetManifestResourceStream("Lemur.Bootrom.A2.bootrom-combined.bin");
            if (stream == null)
            {
                throw new InvalidOperationException("Unable to get bootrom from resources");
            }
            m.Load(stream);
        }, isReadonly: true); // 32kB


        // 0x10000000 - XIP
        services.AddXIP(1024 * 1024 * 2, m => { }); // 2MB


        // 0x20000000 - SRAM
        services.AddMemory("SRAM", 0x20000000, 1024 * 520); // 520kB


        // 0x40000000 - APB Peripherals
        services.AddPeripheral(0x40000000, "SYSINFO_BASE").Unimplemented();
        services.AddPeripheral(0x40008000, "SYSCFG_BASE").Unimplemented();
        services.AddPeripheral(0x40010000, "CLOCKS_BASE").Implementation<Clocks>();
        services.AddPeripheral(0x40018000, "PSM_BASE").Unimplemented();
        services.AddPeripheral(0x40020000, "RESETS_BASE").Implementation<Resets>();
        services.AddPeripheral(0x40028000, "IO_BANK0_BASE").Implementation<UserBankIOPeripheral>();
        services.AddPeripheral(0x40030000, "IO_QSPI_BASE").Unimplemented();//.Implementation<IoQSPI>();
        services.AddPeripheral(0x40038000, "PADS_BANK0_BASE").Implementation<UserBankPadControl>();
        services.AddPeripheral(0x40040000, "PADS_QSPI_BASE").Implementation<PadsQSPI>();
        services.AddPeripheral(0x40048000, "XOSC_BASE").Implementation<XOSC>();
        services.AddPeripheral(0x40050000, "PLL_SYS_BASE").Implementation<PPLSYS>();
        services.AddPeripheral(0x40058000, "PLL_USB_BASE").Implementation<PPLUSB>();
        services.AddPeripheral(0x40060000, "ACCESSCTRL_BASE").Unimplemented();
        services.AddPeripheral(0x40068000, "BUSCTRL_BASE").Unimplemented();
        services.AddPeripheral(0x40070000, "UART0_BASE").Implementation<UART0>();
        services.AddPeripheral(0x40078000, "UART1_BASE").Implementation<UART1>();
        services.AddPeripheral(0x40080000, "SPI0_BASE").Unimplemented();
        services.AddPeripheral(0x40088000, "SPI1_BASE").Unimplemented();
        services.AddPeripheral(0x40090000, "I2C0_BASE").Unimplemented();
        services.AddPeripheral(0x40098000, "I2C1_BASE").Unimplemented();
        services.AddPeripheral(0x400a0000, "ADC_BASE").Unimplemented();
        services.AddPeripheral(0x400a8000, "PWM_BASE").Unimplemented();
        services.AddPeripheral(0x400b0000, "TIMER0_BASE").Implementation<Timer0>();
        services.AddPeripheral(0x400b8000, "TIMER1_BASE").Implementation<Timer1>();
        services.AddPeripheral(0x400c0000, "HSTX_CTRL_BASE").Unimplemented();
        services.AddPeripheral(0x400c8000, "XIP_CTRL_BASE").Unimplemented();
        services.AddPeripheral(0x400d0000, "XIP_QMI_BASE").Implementation<XIPQMI>();
        services.AddPeripheral(0x400d8000, "WATCHDOG_BASE").Implementation<Watchdog>();
        services.AddPeripheral(0x400e0000, "BOOTRAM_BASE").Implementation<BootRAM>();
        services.AddPeripheral(0x400e8000, "ROSC_BASE").Unimplemented();
        services.AddPeripheral(0x400f0000, "TRNG_BASE").Unimplemented();
        services.AddPeripheral(0x400f8000, "SHA256_BASE").Implementation<Sha256>();
        services.AddPeripheral(0x40100000, "POWMAN_BASE").Implementation<Powman>();
        services.AddPeripheral(0x40108000, "TICKS_BASE").Unimplemented();
        services.AddPeripheral(0x40120000, "OTP_BASE").Implementation<OTP>();
        services.AddPeripheral(0x40130000, "OTP_DATA_BASE").Implementation<OTPData>();
        services.AddPeripheral(0x40134000, "OTP_DATA_RAW_BASE").Unimplemented();
        services.AddPeripheral(0x40138000, "OTP_DATA_GUARDED_BASE").Unimplemented();
        services.AddPeripheral(0x4013c000, "OTP_DATA_RAW_GUARDED_BASE").Unimplemented();
        services.AddPeripheral(0x40140000, "CORESIGHT_PERIPH_BASE").Unimplemented();
        services.AddPeripheral(0x40140000, "CORESIGHT_ROMTABLE_BASE").Unimplemented();
        services.AddPeripheral(0x40142000, "CORESIGHT_AHB_AP_CORE0_BASE").Unimplemented();
        services.AddPeripheral(0x40144000, "CORESIGHT_AHB_AP_CORE1_BASE").Unimplemented();
        services.AddPeripheral(0x40146000, "CORESIGHT_TIMESTAMP_GEN_BASE").Unimplemented();
        services.AddPeripheral(0x40147000, "CORESIGHT_ATB_FUNNEL_BASE").Unimplemented();
        services.AddPeripheral(0x40148000, "CORESIGHT_TPIU_BASE").Unimplemented();
        services.AddPeripheral(0x40149000, "CORESIGHT_CTI_BASE").Unimplemented();
        services.AddPeripheral(0x4014a000, "CORESIGHT_APB_AP_RISCV_BASE").Unimplemented();
        services.AddPeripheral(0x40150000, "DFT_BASE").Unimplemented();
        services.AddPeripheral(0x40158000, "GLITCH_DETECTOR_BASE").Unimplemented();
        services.AddPeripheral(0x40160000, "TBMAN_BASE").Unimplemented();

        
        // 0x50000000 - AHB Peripherals
        services.AddPeripheral(0x50000000, "DMA_BASE").Unimplemented();

        //services.AddPeripheral(0x50100000, "USBCTRL_BASE").Unimplemented();
        //services.AddPeripheral(0x50100000, "USBCTRL_DPRAM_BASE").Unimplemented();
        services.AddMemory("USB DPRAM", 0x50100000, 1024 * 4); // 4kB

        services.AddPeripheral(0x50110000, "USBCTRL_REGS_BASE").Implementation<USBCtrlRegs>();
        services.AddPeripheral(0x50200000, "PIO0_BASE").Unimplemented();
        services.AddPeripheral(0x50300000, "PIO1_BASE").Unimplemented();
        services.AddPeripheral(0x50400000, "PIO2_BASE").Unimplemented();
        services.AddPeripheral(0x50500000, "XIP_AUX_BASE").Unimplemented();
        services.AddPeripheral(0x50600000, "HSTX_FIFO_BASE").Unimplemented();
        services.AddPeripheral(0x50700000, "CORESIGHT_TRACE_BASE").Unimplemented();


        // 0xd0000000 - SIO
        services.AddPeripheral(0xd0000000, "SIO_BASE").Implementation<SioPeripheral>();
        services.AddPeripheral(0xd0020000, "SIO_NONSEC_BASE").Unimplemented();


        // 0xe0000000 - Cortex-M33 private registers

        return services;
    }

    private class PeripheralRegistration
    {
        private readonly IServiceCollection m_Services;
        private readonly uint m_BaseAddress;
        private readonly string m_Name;

        public PeripheralRegistration(IServiceCollection services, uint baseAddress, string name)
        {
            m_Services = services;
            m_BaseAddress = baseAddress;
            m_Name = name;
        }

        public IServiceCollection Implementation<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>() where T : class, IAddressableResource
        {
            m_Services.AddSingleton<T>(provider => ActivatorUtilities.CreateInstance<T>(provider, [m_BaseAddress, m_Name]));
            m_Services.AddSingleton<IAddressableResource, T>(provider => provider.GetRequiredService<T>());
            if (typeof(T).IsAssignableTo(typeof(ITickable)))
            {
                m_Services.AddSingleton<ITickable>(provider => (ITickable)provider.GetRequiredService<T>());
            }
            return m_Services;
        }

        public IServiceCollection Unimplemented(uint? size = null)
        {
            m_Services.AddSingleton<IAddressableResource>(sp => sp.GetRequiredService<UnimplementedPeripheralFactory>().Create(m_BaseAddress, m_Name, size));
            return m_Services;
        }
    }

    private static PeripheralRegistration AddPeripheral(this IServiceCollection services, uint baseAddress, string name)
    {
        return new PeripheralRegistration(services, baseAddress, name);
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
        // Register XIP as a typed singleton so BinaryInfoService can depend on it directly,
        // then expose it as IAddressableResource so BusFabric discovers it.
        services.AddSingleton<XIP>(sp =>
        {
            var memory = sp.GetRequiredService<MemoryFactory>().Create("XIP", 0x10000000, size, isReadonly: false);
            init?.Invoke(memory);
            return new XIP(memory, sp.GetRequiredService<ILogger<XIP>>());
        });
        services.AddSingleton<IAddressableResource, XIP>(sp => sp.GetRequiredService<XIP>());
        return services;
    }

}
