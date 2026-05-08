using Lemur.Csr.MemoryProtection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemur;

public class BusFabric : IBusFabric
{
    private readonly IEnumerable<IAddressableResource> m_Resources;
    private readonly IEmuLogger<BusFabric> m_Logger;
    private readonly PmpChecker m_Pmp;

    public BusFabric(IEnumerable<IAddressableResource> resources, IEmuLogger<BusFabric> logger, PmpChecker pmp)
    {
        if (!BitConverter.IsLittleEndian)
        {
            throw new InvalidOperationException("This platform is not little endian.");
        }

        m_Resources = resources;
        m_Logger = logger;
        m_Pmp = pmp;
    }

    private bool CanHandle(IAddressableResource resource, uint address)
    {
        return address - resource.BaseAddress >= 0 && address - resource.BaseAddress < resource.Size; ;
    }

    public void Write(uint address, byte[] data)
    {
        m_Logger.LogMemoryWrite(address, data);

        if (data.Length != 1 && data.Length != 2 && data.Length != 4)
        {
            throw new RiscVException(ExceptionCause.StoreAddressMisaligned, address, $"It is possible to write only word, halfword, or byte. (address: {address.ToHex()} data: {data.ToHex()})");
        }

        if (address % data.Length != 0)
        {
            throw new RiscVException(ExceptionCause.StoreAddressMisaligned, address, $"Misaligned address {address.ToHex()} when writing {data.Length} bytes.");
        }

        if (!m_Pmp.IsPermitted(address, AccessType.Write))
        {
            throw new RiscVException(ExceptionCause.StoreAccessFault, address, $"PMP denied write at {address.ToHex()}");
        }

        var segment = m_Resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new RiscVException(ExceptionCause.StoreAccessFault, address, $"No resource at {address.ToHex()}");
        }

        segment.Write(address, data);
    }

    public uint ReadInstruction(uint address)
    {
        if (address % 2 != 0)
        {
            throw new RiscVException(ExceptionCause.InstructionAddressMisaligned, address, $"Misaligned address {address.ToHex()} when reading instruction.");
        }

        // Fetch one halfword at a time. PMP and resource availability apply
        // independently to each halfword: a 32-bit instruction at a 2-byte-aligned
        // (but not 4-byte-aligned) address may straddle a PMP region boundary,
        // and the upper halfword must be checked even if the lower was permitted.
        ushort low = ReadInstructionHalfword(address);

        // Compressed instruction (low 2 bits != 0b11) — upper halfword is not fetched.
        if ((low & 0x3) != 0x3)
        {
            return low;
        }

        ushort high = ReadInstructionHalfword(address + 2);
        return ((uint)high << 16) | low;
    }

    private ushort ReadInstructionHalfword(uint address)
    {
        if (!m_Pmp.IsPermitted(address, AccessType.Execute))
        {
            throw new RiscVException(ExceptionCause.InstructionAccessFault, address, $"PMP denied fetch at {address.ToHex()}");
        }

        var segment = m_Resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new RiscVException(ExceptionCause.InstructionAccessFault, address, $"No resource at {address.ToHex()}");
        }

        return BitConverter.ToUInt16(segment.Read(address, 2));
    }

    // AMO read-half: same word read as Read(addr,4), but reports any
    // alignment, PMP, or resource failure as Store/AMO so that the whole AMO
    // is reported atomically as a store. Pre-checks W permission too — per
    // RISC-V Atomic ISA, an AMO faults before any side effects if either half
    // of its access would be denied.
    public uint ReadWordForAmo(uint address)
    {
        if (address % 4 != 0)
        {
            throw new RiscVException(ExceptionCause.StoreAddressMisaligned, address, $"Misaligned AMO address {address.ToHex()}");
        }

        if (!m_Pmp.IsPermitted(address, AccessType.Read) || !m_Pmp.IsPermitted(address, AccessType.Write))
        {
            throw new RiscVException(ExceptionCause.StoreAccessFault, address, $"PMP denied AMO at {address.ToHex()}");
        }

        var segment = m_Resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new RiscVException(ExceptionCause.StoreAccessFault, address, $"No resource at {address.ToHex()} for AMO");
        }

        return BitConverter.ToUInt32(segment.Read(address, 4));
    }

    public byte[] Read(uint address, int count)
    {
        m_Logger.LogMemoryRead(address, count);

        if (count != 1 && count != 2 && count != 4)
        {
            throw new RiscVException(ExceptionCause.LoadAddressMisaligned, address, $"It is possible to read only word, halfword, or byte. (address: {address.ToHex()} count: {count})");
        }

        if (address % count != 0)
        {
            throw new RiscVException(ExceptionCause.LoadAddressMisaligned, address, $"Misaligned address {address.ToHex()} when reading {count} bytes.");
        }

        if (!m_Pmp.IsPermitted(address, AccessType.Read))
        {
            throw new RiscVException(ExceptionCause.LoadAccessFault, address, $"PMP denied read at {address.ToHex()}");
        }

        var segment = m_Resources.FirstOrDefault(x => CanHandle(x, address));
        if (segment == null)
        {
            throw new RiscVException(ExceptionCause.LoadAccessFault, address, $"No resource at {address.ToHex()}");
        }

        return segment.Read(address, count);
    }
}
