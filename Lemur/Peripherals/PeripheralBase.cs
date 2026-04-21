using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemur.Peripherals;

public abstract class BytePeripheralBase : IAddressableResource
{
    protected readonly ILogger m_Logger;

    public uint BaseAddress { get; }

    public uint Size => 0x5000;

    protected BytePeripheralBase(uint baseAddress, string name, ILogger logger)
    {
        BaseAddress = baseAddress;
        m_Logger = logger;
    }

    public byte[] Read(uint address, int count)
    {
        var offset = (address - BaseAddress) % 0x1000;

        var result = new List<byte>();
        for (uint i = 0; i < count; i++)
        {
            result.Add(HandleRead(offset + i));
        }
        return result.ToArray();
    }

    protected abstract byte HandleRead(uint offset);

    public void Write(uint address, byte[] data)
    {
        var offset = (address - BaseAddress) % 0x1000;
        var type = (address - BaseAddress) / 0x1000;

        for (int i = 0; i < data.Length; i++)
        {
            
            switch (type)
            {
                case 0:
                    // normal write
                    HandleWrite((uint)(offset + i), data[i]);
                    break;

                case 1:
                    // Atomic XOR on write
                    HandleWrite((uint)(offset + i), (byte)(HandleRead((uint)(offset + i)) ^ data[i]));
                    break;

                case 2:
                    // Atomic bitmask SET (dst |= mask)
                    HandleWrite((uint)(offset + i), (byte)(HandleRead((uint)(offset + i)) | data[i]));
                    break;

                case 3:
                    // Atomic bitmask CLEAR (dst &= ~mask)
                    HandleWrite((uint)(offset + i), (byte)(HandleRead((uint)(offset + i)) & ~data[i]));
                    break;

                case 4:
                    // normal write
                    HandleWrite((uint)(offset + i), data[i]);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(address), "Address out of range.");
            }
        }
    }

    protected abstract void HandleWrite(uint offset, byte data);
}

public abstract class PeripheralBase : IAddressableResource
{
    private readonly Dictionary<uint, Register32> m_Registers = new();
    protected readonly string m_Name;
    protected readonly ILogger m_Logger;

    public uint BaseAddress { get; }

    public uint Size => 0x5000;

    protected PeripheralBase(uint baseAddress, string name, ILogger logger)
    {
        BaseAddress = baseAddress;
        m_Name = name;
        m_Logger = logger;
    }

    protected Register32 AddRegister(uint offset, string registerName, uint resetValue = 0)
    {
        var reg = new Register32(m_Logger, m_Name, registerName, resetValue);
        m_Registers[offset] = reg;
        return reg;
    }

    public byte[] Read(uint address, int count)
    {
        var offset = (address - BaseAddress) % 0x1000;
        var type = (address - BaseAddress) / 0x1000;

        if (type == 0 || type == 4)
        {
            var misalignedDistance = offset % 4;
            var alignedOffset = offset - misalignedDistance;

            return BitConverter.GetBytes(HandleRead(alignedOffset)).Skip((int)misalignedDistance).Take(count).ToArray();
        }
        else
        {
            return new byte[count];
        }
    }

    protected virtual uint HandleRead(uint offset)
    {
        if (m_Registers.TryGetValue(offset, out var r))
        {
            return r.Read();
        }
        else
        {
            return 0;
        }
    }

    public void Write(uint address, byte[] data)
    {
        var offset = (address - BaseAddress) % 0x1000;
        var type = (address - BaseAddress) / 0x1000;

        var misalignedDistance = offset % 4;
        var alignedOffset = offset - misalignedDistance;

        byte[] full = new byte[4];

        if (type == 4)
        {    
            // Zeroes are used in invalid lanes
            data.CopyTo(full, misalignedDistance);
        }
        else
        {
            // Duplicate values to invalid lanes
            if (data.Length == 1)
            {
                full[0] = data[0];
                full[1] = data[0];
                full[2] = data[0];
                full[3] = data[0];
            }
            else if (data.Length == 2)
            {
                full[0] = data[0];
                full[1] = data[1];
                full[2] = data[0];
                full[3] = data[1];
            }
            else if (data.Length == 4)
            {
                full[0] = data[0];
                full[1] = data[1];
                full[2] = data[2];
                full[3] = data[3];
            }
        }

        var value = BitConverter.ToUInt32(full);

        switch (type)
        {
            case 0:
                // normal write
                HandleWrite(alignedOffset, value);
                break;

            case 1:
                // Atomic XOR on write
                HandleWrite(alignedOffset, HandleRead(alignedOffset) ^ value);
                break;

            case 2:
                // Atomic bitmask SET (dst |= mask)
                HandleWrite(alignedOffset, HandleRead(alignedOffset) | value);
                break;

            case 3:
                // Atomic bitmask CLEAR (dst &= ~mask)
                HandleWrite(alignedOffset, HandleRead(alignedOffset) & ~value);
                break;

            case 4:
                // normal write (but with zeros instead replicated value)
                HandleWrite(alignedOffset, value);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range.");
        }
    }

    protected virtual void HandleWrite(uint offset, uint data)
    {
        if (m_Registers.TryGetValue(offset, out var r))
        {
            r.Write(data);
        }
    }
}
