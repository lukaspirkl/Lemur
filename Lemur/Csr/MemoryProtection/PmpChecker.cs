using System.Numerics;
using Lemur.Processor;

namespace Lemur.Csr.MemoryProtection;

public enum AccessType { Read, Write, Execute }

// Evaluates PMP regions against a physical address and access type.
//
// Region priority: lowest index wins (region 0 = highest priority).
// M-mode is subject to PMP only when pmpcfg.L=1 or pmpcfgm0.M[i]=1
// (Hazard3 Xh3pmpm extension). Default on no match: M-mode allow, U-mode deny.
//
// MPRV (mstatus bit 17): when set and current mode is M, load/store checks
// use MPP as effective privilege. Instruction fetch always uses current mode.
//
// TOR mode (A=1) is only active when RiscVConfig.TorEnabled = true.
// With the Hazard3 default (TorEnabled = false), A=1 is sanitised to OFF at
// pmpcfg write time and therefore never reaches the matching logic.
public class PmpChecker
{
    private readonly CsrController m_Csr;
    private readonly Hazard3Processor m_Processor;
    private readonly RiscVConfig m_Config;

    public PmpChecker(CsrController csr, Hazard3Processor processor, RiscVConfig? config = null)
    {
        m_Csr = csr;
        m_Processor = processor;
        m_Config = config ?? new RiscVConfig();
    }

    public bool IsPermitted(uint address, AccessType type)
    {
        PrivilegeMode current = m_Processor.CurrentPrivilege;

        // MPRV: loads/stores in M-mode use MPP as effective privilege.
        PrivilegeMode effective =
            type != AccessType.Execute && m_Csr.Mstatus.Mprv && current == PrivilegeMode.Machine
                ? (PrivilegeMode)m_Csr.Mstatus.Mpp
                : current;

        int pmpRegions = m_Config.PmpRegionsCount;
        bool transposed = m_Config.TransposedPmpBits;

        for (int i = 0; i < pmpRegions; i++)
        {
            byte cfg = RegionCfg(i);
            byte a = (byte)((cfg >> 3) & 0x3);
            if (a == 0) continue; // OFF

            if (!MatchesAddress(i, a, address)) continue;

            // A matching region was found. In M-mode it only applies when
            // the L bit (cfg[7]) or the corresponding pmpcfgm0 bit is set.
            bool modeApplies = effective != PrivilegeMode.Machine
                || (cfg & 0x80) != 0
                || ((m_Csr.Pmpcfgm0.M >> i) & 1) != 0;

            if (!modeApplies) return true; // M-mode bypass

            // RP2350-E6: RWX bits are transposed (R,W,X instead of X,W,R).
            // Standard: Bit 0=R, Bit 1=W, Bit 2=X.
            // Transposed: Bit 0=X, Bit 1=W, Bit 2=R.
            if (transposed)
            {
                return type switch
                {
                    AccessType.Read    => (cfg & 0x04) != 0,
                    AccessType.Write   => (cfg & 0x02) != 0,
                    AccessType.Execute => (cfg & 0x01) != 0,
                    _                  => false,
                };
            }

            return type switch
            {
                AccessType.Read    => (cfg & 0x01) != 0,
                AccessType.Write   => (cfg & 0x02) != 0,
                AccessType.Execute => (cfg & 0x04) != 0,
                _                  => false,
            };
        }

        // No region matched: M-mode is implicitly allowed, U-mode denied.
        return effective == PrivilegeMode.Machine;
    }

    private byte RegionCfg(int region)
    {
        uint word = m_Csr.Pmpcfg[region / 4].Read();
        return (byte)(word >> ((region % 4) * 8));
    }

    private bool MatchesAddress(int region, byte a, uint address)
    {
        uint pmpaddr = m_Csr.Pmpaddr[region].Read();

        if (a == 1) // TOR — top-of-range: lower <= address < upper
        {
            // Only reachable when TorEnabled=true; PmpcfgEntry sanitises A=1→0 otherwise.
            uint lower = region == 0 ? 0u : m_Csr.Pmpaddr[region - 1].Read() << 2;
            uint upper = pmpaddr << 2;
            return address >= lower && address < upper;
        }

        if (a == 2) // NA4 — 4-byte aligned, exactly 4 bytes
            return (address >> 2) == pmpaddr;

        // NAPOT — naturally aligned power-of-two, minimum 8 bytes.
        // pmpaddr trailing 1-bit count k → size = 2^(k+3) bytes.
        // base = (pmpaddr with k+1 low bits cleared) << 2
        int k = BitOperations.TrailingZeroCount(~pmpaddr);
        if (k >= 29) return true; // 2^32 or larger — covers whole address space

        uint sizeMask = (1u << (k + 3)) - 1u;
        uint baseAddr = (pmpaddr & ~((1u << (k + 1)) - 1u)) << 2;
        return (address & ~sizeMask) == baseAddr;
    }
}
