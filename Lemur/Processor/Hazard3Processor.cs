using Lemur.Csr;
using Microsoft.Extensions.Logging;
using System;

namespace Lemur.Processor;

public class Hazard3Processor
{
    private readonly IDecoder m_Decoder = new CDecoder(new Decoder());
    private readonly IEmuLogger<Hazard3Processor> m_Logger;
    private bool m_IsPCModified = false;

    // -------------------------------------------------------------------------
    // LR/SC reservation set — single word address
    // Spec: RISC-V Unprivileged ISA Section 8.2 — Load-Reserved/Store-Conditional
    //
    // null  = no active reservation.
    // value = reserved physical address (word-aligned).
    //
    // The reservation is cleared on:
    //   - Any successful or failed SC.W at any address.
    //   - Any trap (exception or interrupt) entry.
    // -------------------------------------------------------------------------
    public uint? Reservation { get; set; }

    public IBusFabric Memory { get; set; } = new NullBusFabric();
    public Registers Registers { get; }
    public CsrController CSR { get; }

    public event Action? EBreak;
    public void RaiseEBreak() => EBreak?.Invoke();

    public event Action? ECall;
    public void RaiseECall() => ECall?.Invoke();

    public uint PC
    {
        get;
        set
        {
            m_IsPCModified = true;
            field = value;
        }
    }

    public Hazard3Processor(IEmuLogger<Hazard3Processor> logger, Registers registers, CsrController csr)
    {
        m_Logger = logger;
        Registers = registers;
        CSR = csr;
    }

    public void Step()
    {
        using (m_Logger.BeginScope("PC: {PC}", PC.ToHex()))
        {
            var currentPC = PC;

            // MCYCLE increments every step, even if the instruction traps.
            CSR.Mcycle.m_Counter.Value++;

            try
            {
                var instruction = Memory.ReadInstruction(PC);
                var format = m_Decoder.Decode(instruction);

                using (m_Logger.BeginScope("Execute {instruction} {mnemonic}", instruction.ToHex(), format.Mnemonic))
                {
                    m_Logger.LogInstructionExecute(PC, instruction, format.Mnemonic);

                    m_IsPCModified = false;
                    format.Execute(this);
                    if (!m_IsPCModified)
                    {
                        PC = PC + format.StepSize;
                    }

                    // Instruction retired successfully.
                    CSR.Minstret.m_Counter.Value++;
                }
            }
            catch (RiscVException e)
            {
                // Spec Section 8.2: any trap clears the LR/SC reservation.
                Reservation = null;

                EnterTrap(currentPC, (uint)e.Cause, e.TrapValue, isInterrupt: false);
            }
            catch (Exception e)
            {
                m_Logger.LogCritical(e, "Unexpected exception while running instruction at {PC}", PC);
                throw;
            }

            // Check for pending interrupts after every step.
            // Spec Section 3.1.9: an interrupt is taken when MSTATUS.MIE=1 AND
            // the corresponding bit is set in both MIP and MIE.
            CheckInterrupts();
        }
    }

    /// <summary>
    /// Checks MIP × MIE for any pending-and-enabled interrupts and takes the
    /// highest-priority one if the global enable (MSTATUS.MIE) is set.
    ///
    /// Priority order per spec Table 3.7 (highest priority first):
    ///   MEI (external, bit 11) > MSI (software, bit 3) > MTI (timer, bit 7)
    /// </summary>
    private void CheckInterrupts()
    {
        // Global interrupt gate: MSTATUS.MIE must be set.
        if (!CSR.Mstatus.Mie) return;

        uint mip = CSR.Mip.Read();
        uint mie = CSR.Mie.Read();
        uint pending = mip & mie;
        if (pending == 0) return;

        // Select highest-priority pending interrupt.
        // Spec Table 3.7: MEI(11) > MSI(3) > MTI(7)
        //
        // MipEntry.Meip is already gated by MEICONTEXT.PREEMPT, so bit 11 in
        // mip is only set when there is an eligible IRQ (priority >= PREEMPT).
        int cause = -1;
        if ((pending & (1u << 11)) != 0) cause = 11; // MEIP
        if (cause < 0 && (pending & (1u << 3)) != 0) cause = 3;  // MSIP
        if (cause < 0 && (pending & (1u << 7)) != 0) cause = 7;  // MTIP
        if (cause < 0) return;

        Reservation = null;
        EnterTrap(PC, 0x8000_0000u | (uint)cause, 0, isInterrupt: true);
    }

    /// <summary>
    /// Executes the common trap-entry sequence defined in Privileged ISA Section 3.1.7–3.1.9:
    ///   1. Save <paramref name="epc"/> to MEPC.
    ///   2. Write MCAUSE and MTVAL.
    ///   3. Move MSTATUS.MIE → MPIE and clear MIE (disables further interrupts).
    ///   4. Jump to the trap vector address derived from MTVEC.
    ///
    /// For synchronous exceptions <paramref name="epc"/> is the faulting instruction address.
    /// For interrupts it is the address of the next instruction to be executed.
    ///
    /// MTVEC modes (bits [1:0]):
    ///   0 — Direct:   all traps jump to BASE.
    ///   1 — Vectored: interrupts jump to BASE + 4×cause; exceptions still jump to BASE.
    /// </summary>
    private void EnterTrap(uint epc, uint cause, uint tval, bool isInterrupt)
    {
        CSR.Mepc.Write(epc);
        CSR.Mcause.Write(cause);
        // MTVAL is hardwired to zero on Hazard3; no write needed.

        // Save MIE → MPIE, then clear MIE.
        // Spec Section 3.1.6.1: "When a trap is taken ... MIE is set to 0, and MPIE = old MIE."
        CSR.Mstatus.Mpie = CSR.Mstatus.Mie;
        CSR.Mstatus.Mie  = false;

        // Xh3irq: update MEICONTEXT on trap entry.
        // Spec §3.8.6: "mreteirq ... is set on entering the external interrupt
        // vector, cleared by mret, and cleared upon taking any trap other than
        // an external interrupt."
        if (isInterrupt && (cause & 0x7FFF_FFFFu) == 11u) // MEI
        {
            // Push the preemption-priority stack. Peek MEINEXT (no side-effects)
            // to find the incoming IRQ's priority for the new PREEMPT value.
            uint meinext = CSR.Meinext.Peek();
            bool noIrq   = (meinext & 0x8000_0000u) != 0;
            int  irq     = noIrq ? 0 : (int)((meinext >> 2) & 0x1FFu);
            CSR.Meicontext.OnExternalVectorEntry(noIrq, irq, CSR.Meipra);
        }
        else
        {
            // Non-MEI trap: clear MRETEIRQ so MRET won't pop the preempt stack.
            CSR.Meicontext.Write(CSR.Meicontext.Read() & ~1u);
        }

        // Compute trap handler PC from MTVEC.
        // Bits [1:0] = mode; bits [31:2] = BASE (4-byte aligned by hardware).
        uint baseAddr = CSR.Mtvec.Base << 2;

        if (isInterrupt && CSR.Mtvec.Vectored)
            PC = baseAddr + 4u * (cause & 0x7FFF_FFFFu);
        else
            PC = baseAddr;
    }
}
