using Microsoft.Extensions.Logging;

namespace Venture.Processor;

public class Hazard3Processor
{
    private readonly IDecoder m_Decoder = new CDecoder(new Decoder());
    private readonly IEmuLogger<Hazard3Processor> m_Logger;
    private bool m_IsPCModified = false;

    // -------------------------------------------------------------------------
    // MCYCLE / MINSTRET — 64-bit hardware performance counters
    // Spec: RISC-V Privileged ISA Section 3.1.10 — Hardware Performance Monitor
    //
    // m_Cycle   increments every Step() (whether or not an instruction retired).
    // m_Instret increments only when an instruction retires successfully.
    // Both are exposed as pairs of 32-bit CSRs via getter hooks registered below.
    // Software may reset them by writing to the CSR addresses.
    // -------------------------------------------------------------------------
    private ulong m_Cycle   = 0;
    private ulong m_Instret = 0;

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
    public CSR CSR { get; }

    public event Action? EBreak;
    public void RaiseEBreak() => EBreak?.Invoke();

    public event Action? ECall;
    public void RaiseECall() => ECall?.Invoke();

    // -------------------------------------------------------------------------
    // IrqController callbacks (Hazard3 Xh3irq hooks)
    //
    // These delegates let the external interrupt controller (IrqController)
    // supply priority-aware decisions that Hazard3Processor itself cannot
    // make without knowing the MEIPRA/MEIFA state:
    //
    //   MeiTrapGate      — returns true if an IRQ with priority >= the given
    //                      MEICONTEXT.PREEMPT value exists, i.e. CheckInterrupts
    //                      should actually take the MEI trap. When null, we
    //                      fall back to the simple "preempt == 0" approximation.
    //
    //   OnMeiVectorEntry — called immediately before jumping to the MEI vector.
    //                      The hook pushes the preemption priority stack and
    //                      sets PREEMPT to (incoming_irq_priority + 1).
    //                      When null, we fall back to a priority-0 push.
    // -------------------------------------------------------------------------
    public Func<uint, bool>? MeiTrapGate      { get; set; }
    public Action?           OnMeiVectorEntry { get; set; }

    public uint PC
    {
        get;
        set
        {
            m_IsPCModified = true;
            field = value;
        }
    }

    public Hazard3Processor(IEmuLogger<Hazard3Processor> logger, Registers registers, CSR csr)
    {
        m_Logger = logger;
        Registers = registers;
        CSR = csr;

        // MCYCLE / MCYCLEH (0xB00 / 0xB80) — cycle counter
        // Getter returns live value from m_Cycle; setter allows software to reset the counter.
        csr.GetEntry(CSR.MCYCLE)!.LiveValue(
            getter: () => (uint)(m_Cycle & 0xFFFF_FFFFu),
            setter: v  => m_Cycle = (m_Cycle & 0xFFFF_FFFF_0000_0000UL) | v);
        csr.GetEntry(CSR.MCYCLEH)!.LiveValue(
            getter: () => (uint)(m_Cycle >> 32),
            setter: v  => m_Cycle = (m_Cycle & 0x0000_0000_FFFF_FFFFUL) | ((ulong)v << 32));

        // MINSTRET / MINSTRETH (0xB02 / 0xB82) — instructions-retired counter
        csr.GetEntry(CSR.MINSTRET)!.LiveValue(
            getter: () => (uint)(m_Instret & 0xFFFF_FFFFu),
            setter: v  => m_Instret = (m_Instret & 0xFFFF_FFFF_0000_0000UL) | v);
        csr.GetEntry(CSR.MINSTRETH)!.LiveValue(
            getter: () => (uint)(m_Instret >> 32),
            setter: v  => m_Instret = (m_Instret & 0x0000_0000_FFFF_FFFFUL) | ((ulong)v << 32));
    }

    /// <summary>
    /// Sets or clears a single bit in MIP (Machine Interrupt Pending).
    /// Called by peripherals and the IRQ controller to signal interrupt lines.
    ///
    /// Relevant bits (see <see cref="CSR.MxP_MSIP_BIT"/> etc.):
    ///   bit 3  (MSIP)  — software interrupt
    ///   bit 7  (MTIP)  — machine timer interrupt (MTIME >= MTIMECMP)
    ///   bit 11 (MEIP)  — external interrupt (any peripheral IRQ line asserted)
    ///
    /// Uses RawSet so peripheral code does not go through the software CSR path.
    /// </summary>
    public void SetMip(int bit, bool value)
    {
        var mip = CSR.RawGet(CSR.MIP);
        if (value)
            mip |=  (1u << bit);
        else
            mip &= ~(1u << bit);
        CSR.RawSet(CSR.MIP, mip);
    }

    public void Step()
    {
        using (m_Logger.BeginScope("PC: {PC}", PC.ToHex()))
        {
            var currentPC = PC;

            // MCYCLE increments every step, even if the instruction traps.
            m_Cycle++;

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
                    m_Instret++;
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
        var mstatus = CSR.RawGet(CSR.MSTATUS);

        // Global interrupt gate: MSTATUS.MIE must be set.
        if ((mstatus & (1u << CSR.MSTATUS_MIE_BIT)) == 0)
            return;

        var mip = CSR.RawGet(CSR.MIP);
        var mie = CSR.RawGet(CSR.MIE);
        var pending = mip & mie;

        if (pending == 0)
            return;

        // Select highest-priority pending interrupt.
        // Spec Table 3.7: MEI(11) > MSI(3) > MTI(7)
        //
        // For MEI (external): gate on MEICONTEXT.PREEMPT — only take the interrupt if
        // there is an eligible IRQ (priority >= PREEMPT). Since all IRQ priorities are 0,
        // the external interrupt is only taken when PREEMPT == 0.
        // Spec §3.8.6.1.4: "must be greater than or equal to MEICONTEXT.PREEMPT".
        int cause = -1;
        if ((pending & (1u << CSR.MxP_MEIP_BIT)) != 0)
        {
            uint preempt = (CSR.RawGet(CSR.MEICONTEXT) >> 16) & 0x1Fu;
            bool eligible = MeiTrapGate != null ? MeiTrapGate(preempt) : preempt == 0;
            if (eligible) cause = CSR.MxP_MEIP_BIT;
        }
        if (cause < 0 && (pending & (1u << CSR.MxP_MSIP_BIT)) != 0) cause = CSR.MxP_MSIP_BIT;
        if (cause < 0 && (pending & (1u << CSR.MxP_MTIP_BIT)) != 0) cause = CSR.MxP_MTIP_BIT;
        if (cause < 0) return;

        // Spec Section 8.2: interrupt entry clears the LR/SC reservation.
        Reservation = null;

        // MCAUSE bit 31 = 1 signals an interrupt (as opposed to a synchronous exception).
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
        CSR.RawSet(CSR.MEPC,   epc);
        CSR.RawSet(CSR.MCAUSE, cause);
        //CSR.RawSet(CSR.MTVAL,  tval); // Hazard3 has MTVAL hardwired to zero so we are not setting it here.

        // Save MIE → MPIE, then clear MIE.
        // Spec Section 3.1.6.1: "When a trap is taken ... MIE is set to 0, and MPIE = old MIE."
        var mstatus = CSR.RawGet(CSR.MSTATUS);
        var mie = (mstatus >> CSR.MSTATUS_MIE_BIT) & 1u;
        mstatus = (mstatus & ~(1u << CSR.MSTATUS_MPIE_BIT))  // clear MPIE
                | (mie << CSR.MSTATUS_MPIE_BIT);               // write old MIE into MPIE
        mstatus &= ~(1u << CSR.MSTATUS_MIE_BIT);               // clear MIE
        CSR.RawSet(CSR.MSTATUS, mstatus);

        // Xh3irq: update MEICONTEXT on trap entry.
        // Spec §3.8.6: "mreteirq ... is set on entering the external interrupt
        // vector, cleared by mret, and cleared upon taking any trap other than
        // an external interrupt."
        if (isInterrupt && (cause & 0x7FFF_FFFFu) == CSR.MxP_MEIP_BIT)
        {
            // MEI trap: delegate the preemption-stack push to the IRQ
            // controller, which knows the incoming IRQ's priority. Fall back
            // to a priority-0 push when no controller is wired up.
            if (OnMeiVectorEntry != null)
            {
                OnMeiVectorEntry();
            }
            else
            {
                var meicontext = CSR.RawGet(CSR.MEICONTEXT);
                uint pppreempt = (meicontext >> 28) & 0xFu;
                uint ppreempt  = (meicontext >> 24) & 0xFu;
                uint preempt   = (meicontext >> 16) & 0x1Fu;
                meicontext &= 0x0000_FFFFu;
                meicontext |= (ppreempt << 28);
                meicontext |= (preempt  << 24);
                meicontext |= (1u       << 16); // PREEMPT = priority 0 + 1
                meicontext |= 1u;               // MRETEIRQ = 1
                CSR.RawSet(CSR.MEICONTEXT, meicontext);
            }
        }
        else
        {
            // Non-MEI trap (other interrupts or synchronous exceptions):
            // clear MRETEIRQ so the matching MRET won't pop the preempt stack.
            var meicontext = CSR.RawGet(CSR.MEICONTEXT);
            CSR.RawSet(CSR.MEICONTEXT, meicontext & ~1u);
        }

        // Compute trap handler PC from MTVEC.
        // Bits [1:0] = mode; bits [31:2] = BASE (guaranteed 4-byte aligned by hardware).
        var mtvec   = CSR.RawGet(CSR.MTVEC);
        var mode    = mtvec & 0x3u;
        var baseAddr = mtvec & ~0x3u;

        if (isInterrupt && mode == CSR.MTVEC_MODE_VECTORED)
        {
            // Vectored interrupt: BASE + 4 × interrupt_cause_number.
            // The `cause` argument has bit 31 set to mark it as an interrupt
            // (MCAUSE layout), so we mask it off to get the raw cause number
            // (e.g. MEI=11 → offset 0x2C, MTI=7 → offset 0x1C, MSI=3 → 0x0C).
            // Synchronous exceptions still go to BASE even in vectored mode.
            PC = baseAddr + 4u * (cause & 0x7FFF_FFFFu);
        }
        else
        {
            // Direct mode, or synchronous exception in vectored mode.
            PC = baseAddr;
        }
    }
}
