namespace Venture.Peripherals;

public abstract class Peripheral32 : PeripheralBase
{
    protected Peripheral32(uint startAddress) : base(startAddress)
    {
    }

    protected override byte[] HandleRead(uint offset, int count)
    {
        if (count % 4 != 0)
        {
            throw new ArgumentException(nameof(count), $"Unaligned memory access in {GetType().Name} (offset: {offset.ToHex()})");
        }

        var result = new List<byte>();
        for (int i = 0; i < count / 4; i++)
        {
            result.AddRange(BitConverter.GetBytes(HandleRead(offset)));

        }
        return result.ToArray();
    }

    protected abstract uint HandleRead(uint offset);

    protected override void HandleWrite(uint offset, byte[] data)
    {
        if (data.Length % 4 != 0)
        {
            throw new ArgumentException(nameof(data), $"Unaligned memory access in {GetType().Name} (address: {offset.ToHex()})");
        }

        for (int i = 0; i < data.Length / 4; i++)
        {
            HandleWrite(offset, data[i * 4]);
        }
    }

    protected abstract void HandleWrite(uint offset, uint data);
}

public abstract class PeripheralBase : IAddressableResource
{
    public uint StartAddress { get; }

    public uint Size => 0x4000;

    protected PeripheralBase(uint startAddress)
    {
        StartAddress = startAddress;
    }

    public byte[] Read(uint address, int count)
    {
        if (count != 4)
        {
            throw new InvalidOperationException();
        }

        return HandleRead((address - StartAddress) % 0x1000, count);
    }

    protected abstract byte[] HandleRead(uint offset, int count);

    public void Write(uint address, byte[] data)
    {
        if (data.Length != 4)
        {
            throw new InvalidOperationException();
        }

        // TODO: Handle narrow write
        // To disable this behaviour on RP2350, set bit 14 of the address by accessing the peripheral at an offset
        // of +0x4000. This causes invalid byte lanes to be driven to zero, rather than being driven with replicated
        // data.In some situations, such as DMA of 8 - bit values to the PWM peripheral, the default
        // replication behaviour is not desirable.

        var offset = (address - StartAddress) % 0x1000;
        var type = (address - StartAddress) / 0x1000;

        switch (type)
        {
            case 0:
                // normal write
                HandleWrite(offset, data);
                break;

            case 1:
                // Atomic XOR on write
                ApplyAtomic(offset, data, (byte dst, byte mask) => (byte)(dst ^ mask));
                break;

            case 2:
                // Atomic bitmask SET (dst |= mask)
                ApplyAtomic(offset, data, (byte dst, byte mask) => (byte)(dst | mask));
                break;

            case 3:
                // Atomic bitmask CLEAR (dst &= ~mask)
                ApplyAtomic(offset, data, (byte dst, byte mask) => (byte)(dst & ~mask));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range.");
        }
    }

    protected abstract void HandleWrite(uint offset, byte[] data);

    private void ApplyAtomic(uint offset, byte[] data, Func<byte, byte, byte> op)
    {
        var newData = new byte[data.Length];
        var current = HandleRead(offset, data.Length);
        
        for (uint i = 0; i < data.Length; i++)
        {
            newData[i] = op(current[i], data[i]);
        }

        HandleWrite(offset, data);
    }
}


//public class PeripheralHost : IAddressableResource
//{
//    private readonly Peripheral peripheral;

//    public uint StartAddress => peripheral.StartAddress;
//    public uint Size => 0x4000;

//    //byte[] memory = new byte[0x1000]; // = 4096 = 4kB

//    public PeripheralHost(Peripheral peripheral)
//    {
//        this.peripheral = peripheral;
//    }

//    public byte[] Read(uint address, int count)
//    {
//        var adr = (address - StartAddress) % 0x1000;
//        // TODO: check if address is propertly aligned (unit test)

//        // TODO: This should be in loop because caunt can be more than 4
//        return BitConverter.GetBytes(peripheral.Read(address));
//    }

//    public void Write(uint address, byte[] data)
//    {
//        var adr = (address - StartAddress) % 0x1000;
//        var type = (address - StartAddress) / 0x1000;
//        // TODO: implement narrow write
//        // TODO: check if address is propertly aligned (unit test)
//        switch (type)
//        {
//            case 0:
//                // normal write
//                ApplyAtomic(adr, data, (uint dst, uint mask) => mask);
//                break;

//            case 1:
//                // Atomic XOR on write
//                ApplyAtomic(adr, data, (uint dst, uint mask) => dst ^ mask);
//                break;

//            case 2:
//                // Atomic bitmask SET (dst |= mask)
//                ApplyAtomic(adr, data, (uint dst, uint mask) => dst | mask);
//                break;

//            case 3:
//                // Atomic bitmask CLEAR (dst &= ~mask)
//                ApplyAtomic(adr, data, (uint dst, uint mask) => dst & ~mask);
//                break;

//            default:
//                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range.");
//        }
//    }

//    private void ApplyAtomic(uint adr, byte[] data, Func<uint, uint, uint> op)
//    {
//        for (uint i = 0; i < data.Count; i += 4)
//        {
//            uint mask = BitConverter.ToUInt32(data.Array!, (int)i);
//            uint current = peripheral.Read(adr + i);
//            current = op(current, mask);
//            peripheral.Write(adr + i, current);
//        }
//    }
//}
