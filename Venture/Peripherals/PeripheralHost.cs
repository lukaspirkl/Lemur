namespace Venture.Peripherals;

public class SIO : Peripheral
{
    public SIO() : base(0xd0000000)
    {
    }

    public override uint Read(uint address)
    {
        var offset = address - StartAddress;
        
        if (offset == 0x000) // CPUID Register
        {
            Console.WriteLine("Read CPUID Register");
            return 0;
        }

        throw new NotImplementedException();
    }

    public override void Write(uint address, uint value)
    {
        throw new NotImplementedException();
    }
}

public abstract class Peripheral
{
    public uint StartAddress { get; }

    public Peripheral(uint startAddress)
    {
        StartAddress = startAddress;
    }

    public abstract uint Read(uint address);
    public abstract void Write(uint address, uint value);
}

public class PeriphheralHost : IAddressableResource
{
    private readonly Peripheral peripheral;

    public uint StartAddress => peripheral.StartAddress;

    public uint Size => 0x4000;

    public PeriphheralHost(Peripheral peripheral)
    {
        this.peripheral = peripheral;
    }

    public byte[] Read(uint address, int count)
    {
        if (count % 4 != 0)
        {
            throw new ArgumentException(nameof(count), $"Unaligned memory access (address: {address.ToHex()})");
        }

        var result = new List<byte>();
        for (int i = 0; i < count / 4; i++)
        {
            result.AddRange(BitConverter.GetBytes(peripheral.Read(address)));
            
        }
        return result.ToArray();
    }

    public void Write(uint address, byte[] data)
    {
        if (data.Length % 4 != 0)
        {
            throw new ArgumentException(nameof(data), $"Unaligned memory access (address: {address.ToHex()})");
        }

        for (int i = 0; i < data.Length / 4; i++)
        {
            peripheral.Write(address, data[i * 4]);
        }
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
