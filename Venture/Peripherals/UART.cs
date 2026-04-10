using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Venture.Peripherals;

public class UART0 : UART
{
    public UART0(uint baseAddress, string name, ILogger<UART0> logger) : base(baseAddress, name, logger)
    {
    }
}

public class UART1 : UART
{
    public UART1(uint baseAddress, string name, ILogger<UART1> logger) : base(baseAddress, name, logger)
    {
    }
}

public class UART : PeripheralBase
{
    public event Action<char>? ReceivedData;

    private readonly ConcurrentQueue<byte> _rxFifo = new();

    public void EnqueueRxByte(byte b) => _rxFifo.Enqueue(b);

    public UART(uint baseAddress, string name, ILogger<UART> logger) : base(baseAddress, name, logger)
    {
        AddRegister(0x000, "UARTDR").Field(0, 8, ReceiveData, TransmitData); // Data Register, UARTDR
        AddRegister(0x004, "UARTRSR"); // Receive Status Register/Error Clear Register, UARTRSR/UARTECR
        AddRegister(0x018, "UARTFR")  // Flag Register, UARTFR
            .Field(lsb: 4, getter: () => _rxFifo.IsEmpty)   // RXFE: receive FIFO empty
            .Field(lsb: 5, getter: () => false)              // TXFF: transmit FIFO full (never)
            .Field(lsb: 7, getter: () => true);              // TXFE: transmit FIFO empty (always)
        AddRegister(0x020, "UARTILPR"); // IrDA Low-Power Counter Register, UARTILPR
        AddRegister(0x024, "UARTIBRD"); // Integer Baud Rate Register, UARTIBRD
        AddRegister(0x028, "UARTFBRD"); // Fractional Baud Rate Register, UARTFBRD
        AddRegister(0x02c, "UARTLCR_H"); // Line Control Register, UARTLCR_H
        AddRegister(0x030, "UARTCR"); // Control Register, UARTCR
        AddRegister(0x034, "UARTIFLS"); // Interrupt FIFO Level Select Register, UARTIFLS
        AddRegister(0x038, "UARTIMSC"); // Interrupt Mask Set/Clear Register, UARTIMSC
        AddRegister(0x03c, "UARTRIS"); // Raw Interrupt Status Register, UARTRIS
        AddRegister(0x040, "UARTMIS"); // Masked Interrupt Status Register, UARTMIS
        AddRegister(0x044, "UARTICR"); // Interrupt Clear Register, UARTICR
        AddRegister(0x048, "UARTDMACR"); // DMA Control Register, UARTDMACR
        AddRegister(0xfe0, "UARTPERIPHID0"); // UARTPeriphID0 Register
        AddRegister(0xfe4, "UARTPERIPHID1"); // UARTPeriphID1 Register
        AddRegister(0xfe8, "UARTPERIPHID2"); // UARTPeriphID2 Register
        AddRegister(0xfec, "UARTPERIPHID3"); // UARTPeriphID3 Register
        AddRegister(0xff0, "UARTPCELLID0"); // UARTPCellID0 Register
        AddRegister(0xff4, "UARTPCELLID1"); // UARTPCellID1 Register
        AddRegister(0xff8, "UARTPCELLID2"); // UARTPCellID2 Register
        AddRegister(0xffc, "UARTPCELLID3"); // UARTPCellID3 Register
    }

    private void TransmitData(uint data)
    {
        ReceivedData?.Invoke((char)data);
    }

    private uint ReceiveData()
    {
        return _rxFifo.TryDequeue(out var b) ? b : 0u;
    }
}
