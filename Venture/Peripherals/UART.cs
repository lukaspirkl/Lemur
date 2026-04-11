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

/// <summary>
/// Arm PrimeCell PL011 UART (revision r1p5), as instantiated twice in RP2350.
///
/// The UART serialises parallel register data into a TX bit stream and
/// deserialises a received RX bit stream back into register data. It uses
/// separate 32-entry TX and RX FIFOs and generates 11 maskable interrupt
/// sources combined into one UARTINTR output.
///
/// Emulator simplifications and limitations:
///   - No real bit-level serialisation. TX is instant: writing UARTDR
///     immediately fires <see cref="ReceivedData"/>. The baud rate registers
///     are stored for SDK readback but have no effect on timing.
///   - No serial line error simulation. The framing/parity/break/overrun error
///     bits in UARTDR and UARTRSR are always 0.
///   - Interrupt register state (UARTRIS, UARTMIS, UARTICR) is maintained
///     correctly and readable by software. However, no interrupt is actually
///     delivered to the CPU because the emulator has no NVIC or CPU interrupt
///     infrastructure yet.
///     TODO: Wire UARTINTR to CPU MIP.MEIP when CPU interrupt support lands.
///   - Hardware flow control (RTS/CTS) requires GPIO wiring through UserBankIO.
///     The CTSEN/RTSEN bits are stored but have no behavioural effect yet.
///     TODO: Connect nUARTRTS output and nUARTCTS input via UserBankIO mux.
///   - IrDA SIR mode (SIREN/SIRLP) is not supported on RP2350 and is ignored.
///   - Modem signals (RI, DCD, DSR, CTS status in UARTFR) are not connected;
///     they always read as 0.
///
/// Enable-bit policy (UARTEN / TXE / RXE in UARTCR):
///   In real hardware these gate the UARTTXD/UARTRXD pin drivers. The emulator
///   ignores them for the <see cref="ReceivedData"/> event and
///   <see cref="EnqueueRxByte"/> path, because those represent internal
///   peripheral monitoring (e.g., the UI terminal), not physical pin activity.
///   A future pin-connected implementation should gate on these bits.
/// </summary>
public class UART : PeripheralBase
{
    // ─── Public surface ──────────────────────────────────────────────────────

    /// <summary>
    /// Raised whenever the CPU writes a byte to the TX data register (UARTDR).
    /// In real hardware this would drive the UARTTXD pin at the configured baud
    /// rate; in the emulator it fires synchronously and immediately.
    /// Subscribers (e.g., the UI terminal) receive every transmitted character.
    /// Not raised when loopback is active — the byte is fed into the RX FIFO instead.
    /// </summary>
    public event Action<char>? ReceivedData;

    /// <summary>
    /// Inject a byte into the RX FIFO as if it arrived on the UARTRXD pin.
    /// Read out by the CPU via UARTDR. Bytes beyond the FIFO capacity (32 in
    /// FIFO mode, 1 in character mode) are silently dropped — this mirrors the
    /// hardware overrun behaviour where the shift register is overwritten but
    /// the FIFO contents remain valid.
    /// </summary>
    public void EnqueueRxByte(byte b)
    {
        if (m_RxFifo.Count >= RxFifoCapacity)
        {
            // Hardware overrun: the shift register value is lost and the OE bit
            // would be set on the next read from UARTDR. The emulator drops the
            // byte silently since error bits are not modelled.
            return;
        }

        m_RxFifo.Enqueue(b);
        UpdateRxInterruptStatus();
    }

    // ─── LCR_H fields (Line Control Register) ────────────────────────────────

    // FEN (bit 4): FIFO enable.
    //   0 = character mode — TX and RX each act as a 1-byte holding register.
    //   1 = FIFO mode (normal) — TX 32×8, RX 32×12.
    // Affects RXFE/RXFF flags, FIFO capacity, and interrupt thresholds.
    private bool m_Fen;

    // WLEN (bits 6:5): data word length.
    //   0b00=5 bits, 0b01=6, 0b10=7, 0b11=8.
    // Stored for SDK readback; the emulator always transfers full bytes.
    private uint m_Wlen;

    // STP2 (bit 3): two stop bits instead of one.
    // Stored for readback; the emulator does not model stop bits.
    private bool m_Stp2;

    // PEN (bit 1): parity enable.
    // EPS (bit 2): even parity (0=odd, 1=even). Only relevant when PEN=1.
    // SPS (bit 7): stick parity — forces parity bit to a fixed level.
    // All stored for readback; parity is not checked or generated in the emulator.
    private bool m_Pen, m_Eps, m_Sps;

    // BRK (bit 0): send break — holds UARTTXD LOW for longer than one full frame.
    // Stored for readback. No TX pin in the emulator, so no effect.
    private bool m_Brk;

    // ─── CR fields (Control Register) ────────────────────────────────────────

    // UARTEN (bit 0): master UART enable.
    // In hardware, disabling mid-frame completes the current character first.
    // The emulator ignores this for the ReceivedData / EnqueueRxByte paths.
    private bool m_Uarten;

    // TXE (bit 8) / RXE (bit 9): independent TX / RX section enables.
    // Reset value is 1 for both (the UART boots with TX and RX enabled).
    // The emulator ignores these for internal data paths; see class-level doc.
    private bool m_Txe = true;
    private bool m_Rxe = true;

    // LBE (bit 7): loopback enable.
    // When set, TX data is fed directly into the RX FIFO instead of the TX pin.
    // Implemented: TransmitData enqueues into m_RxFifo rather than firing ReceivedData.
    private bool m_Lbe;

    // RTSEN (bit 14): RTS hardware flow control enable.
    //   When set, nUARTRTS is de-asserted once the RX FIFO reaches the RXIFLSEL
    //   threshold, signalling the remote sender to pause.
    // CTSEN (bit 15): CTS hardware flow control enable.
    //   When set, TX only proceeds while nUARTCTS is asserted.
    // Both stored but have no effect until GPIO wiring is implemented.
    // TODO: On RTSEN change, assert/de-assert the nUARTRTS GPIO line via UserBankIO.
    // TODO: On TX, gate transmission on the nUARTCTS GPIO input line when CTSEN=1.
    private bool m_Rtsen, m_Ctsen;

    // RTS (bit 11), DTR (bit 10), OUT1 (bit 12), OUT2 (bit 13): modem outputs.
    // Stored for readback.
    // TODO: Drive the corresponding GPIO pins via UserBankIO mux.
    private bool m_Rts, m_Dtr, m_Out1, m_Out2;

    // ─── IFLS fields (Interrupt FIFO Level Select) ────────────────────────────

    // TXIFLSEL (bits 2:0): TX interrupt fires when TX FIFO drops to or below
    // this fill level. 0b000=1/8 (≤4), 0b001=1/4 (≤8), 0b010=1/2 (≤16, default),
    // 0b011=3/4 (≤24), 0b100=7/8 (≤28). Values 0b101–0b111 reserved.
    private uint m_TxIflSel = 0x2;

    // RXIFLSEL (bits 5:3): RX interrupt fires when RX FIFO reaches or exceeds
    // this fill level. Same encoding. Default 0b010 = 1/2 full = 16 entries.
    private uint m_RxIflSel = 0x2;

    // ─── Interrupt state ─────────────────────────────────────────────────────

    // IMSC: interrupt mask register (bits 10:0). A 1 enables the source.
    //   UARTMIS = UARTRIS & UARTIMSC — only masked-in sources reach the CPU.
    private uint m_Imsc;

    // TXRIS (UARTRIS bit 5): raw TX interrupt status. TRANSITION-BASED.
    //   Asserted after the TX FIFO drains to/below the TXIFLSEL threshold.
    //   The specification states the interrupt is based on a transition through
    //   the threshold level, not on the level itself: if UARTEN and TXIM are
    //   enabled before any data is written, the interrupt is NOT asserted.
    //   It is only set after a byte is written and the FIFO empties again.
    //   In the emulator TX is instant, so TXRIS is asserted after every
    //   TransmitData call and cleared by writing UARTICR.TXIC.
    private bool m_TxRis;

    // RXRIS (UARTRIS bit 4): raw RX interrupt status. LEVEL-BASED.
    //   Asserted when m_RxFifo.Count reaches the RXIFLSEL threshold.
    //   De-asserted when the FIFO drains below that threshold.
    //   UARTICR.RXIC can manually clear it, but if the FIFO is still at or
    //   above the threshold, it will re-assert on the very next status check.
    private bool m_RxRis;

    // RTRIS (UARTRIS bit 6): receive timeout interrupt.
    //   Fires when the RX FIFO is non-empty and no new data has arrived for
    //   32 bit periods. This requires a background timer or cycle-accurate
    //   emulation, neither of which is available.
    //   TODO: Implement when emulation timing infrastructure is in place.

    // Error interrupt bits (UARTRIS bits 10:7 — OERIS/BERIS/PERIS/FERIS):
    //   Overrun, break, parity, framing errors on the serial line.
    //   The emulator never generates real serial errors, so always 0.

    // Modem status interrupt bits (UARTRIS bits 3:0):
    //   Triggered by changes on nUARTCTS/DCD/DSR/RI.
    //   RP2350 does not support modem mode; always 0.

    // ─── Baud rate registers ─────────────────────────────────────────────────

    // UARTIBRD (bits 15:0): integer baud rate divisor.
    //   BRD_int = floor(UARTCLK / (16 × baud_rate)).
    //   Stored for SDK readback (uart_set_baudrate writes then reads back).
    //   Both IBRD and FBRD are latched into the internal divider on the next
    //   write to UARTLCR_H (even a dummy write with no field changes).
    private uint m_IBrd;

    // UARTFBRD (bits 5:0): fractional baud rate divisor.
    //   BRD_frac = round(BRD_fraction × 64). Together with IBRD gives a
    //   22-bit baud rate divisor for fine-grained baud rate generation.
    private uint m_FBrd;

    // ─── IrDA ────────────────────────────────────────────────────────────────

    // UARTILPR (bits 7:0): IrDA low-power counter divisor.
    //   IrDA SIR mode is not supported on RP2350 (the PL011 feature exists
    //   but is not connected). Stored only for register completeness.
    private uint m_IlpDvsr;

    // ─── RX FIFO ─────────────────────────────────────────────────────────────

    // In real hardware the RX FIFO is 32×12 bits: 8 data bits plus 4 error
    // flag bits (OE/BE/PE/FE). The emulator uses a simple byte queue because
    // error flags are never set. ConcurrentQueue is used because EnqueueRxByte
    // may be called from a different thread (e.g., the UI or a test harness).
    private readonly ConcurrentQueue<byte> m_RxFifo = new();

    // ─────────────────────────────────────────────────────────────────────────

    public UART(uint baseAddress, string name, ILogger<UART> logger) : base(baseAddress, name, logger)
    {
        // UARTDR (0x000) — Data Register
        //   Write (TX): bits 7:0 carry the byte to transmit. Upper bits reserved.
        //   Read  (RX): bits 7:0 = received data from the RX FIFO.
        //               bits 11:8 = error flags for that byte (FE/PE/BE/OE).
        //               Error flags are always 0 in the emulator.
        //               Reading from an empty FIFO yields 0 (undefined in hardware).
        AddRegister(0x000, "UARTDR")
            .Field(0, 8, ReceiveData, TransmitData);

        // UARTRSR/UARTECR (0x004) — Receive Status Register / Error Clear Register
        //   Read:  bits 3:0 mirror the error flags of the most recently read RX
        //          byte (OE=3, BE=2, PE=1, FE=0). Always 0 in the emulator.
        //   Write: any write (the value is ignored) clears all four error bits.
        //          Hardware uses this to acknowledge a reception error before
        //          continuing. The write is a no-op here since errors never occur.
        AddRegister(0x004, "UARTRSR");

        // UARTFR (0x018) — Flag Register (read-only)
        //   Polled by the SDK before every TX (wait for !TXFF) and RX (wait for !RXFE).
        //   Bit 0 CTS:  nUARTCTS modem input (1 when pin is LOW). No GPIO wiring → 0.
        //   Bit 1 DSR:  nUARTDSR modem input. Not on RP2350 → 0.
        //   Bit 2 DCD:  nUARTDCD modem input. Not on RP2350 → 0.
        //   Bit 3 BUSY: set while the TX shift register is active. TX is instant → 0.
        //   Bit 4 RXFE: RX FIFO empty.
        //   Bit 5 TXFF: TX FIFO full. TX is instant, never truly full → 0.
        //   Bit 6 RXFF: RX FIFO full.
        //   Bit 7 TXFE: TX FIFO empty. TX is instant, always empty → 1.
        //   Bit 8 RI:   nUARTRI ring indicator. Not on RP2350 → 0.
        AddRegister(0x018, "UARTFR")
            .Field(lsb: 0, getter: () => false)                 // CTS
            .Field(lsb: 1, getter: () => false)                 // DSR
            .Field(lsb: 2, getter: () => false)                 // DCD
            .Field(lsb: 3, getter: () => false)                 // BUSY
            .Field(lsb: 4, getter: () => m_RxFifo.IsEmpty)     // RXFE
            .Field(lsb: 5, getter: () => false)                 // TXFF
            .Field(lsb: 6, getter: () => RxFifoFull)           // RXFF
            .Field(lsb: 7, getter: () => true)                  // TXFE
            .Field(lsb: 8, getter: () => false);                // RI

        // UARTILPR (0x020) — IrDA Low-Power Counter Register
        //   Bits 7:0: ILPDVSR. IrDA not supported on RP2350; stored only.
        AddRegister(0x020, "UARTILPR")
            .Field(0, 8, () => m_IlpDvsr, v => m_IlpDvsr = v);

        // UARTIBRD (0x024) — Integer Baud Rate Register
        //   Bits 15:0: BAUD_DIVINT. The SDK writes and reads this back.
        //   Note: changes to IBRD/FBRD are not applied to the internal baud
        //   rate counter until a subsequent write to UARTLCR_H occurs.
        AddRegister(0x024, "UARTIBRD")
            .Field(0, 16, () => m_IBrd, v => m_IBrd = v);

        // UARTFBRD (0x028) — Fractional Baud Rate Register
        //   Bits 5:0: BAUD_DIVFRAC.
        AddRegister(0x028, "UARTFBRD")
            .Field(0, 6, () => m_FBrd, v => m_FBrd = v);

        // UARTLCR_H (0x02C) — Line Control Register
        //   Controls the serial frame format. A write here also latches the
        //   current IBRD/FBRD values into the baud rate generator (the SDK
        //   performs a dummy LCR_H write after changing the baud rate divisors).
        //   Bit 0 BRK:  send break (holds UARTTXD LOW ≥ one frame). No pin → no-op.
        //   Bit 1 PEN:  parity enable.
        //   Bit 2 EPS:  even parity select (0=odd, 1=even).
        //   Bit 3 STP2: two stop bits.
        //   Bit 4 FEN:  FIFO enable (0=character mode 1-byte, 1=FIFO mode 32-byte).
        //   Bits 6:5 WLEN: word length (0b00=5b, 0b01=6b, 0b10=7b, 0b11=8b).
        //   Bit 7 SPS:  stick parity.
        AddRegister(0x02C, "UARTLCR_H")
            .Field(lsb: 0, getter: () => m_Brk,  setter: v => m_Brk  = v)  // BRK
            .Field(lsb: 1, getter: () => m_Pen,  setter: v => m_Pen  = v)  // PEN
            .Field(lsb: 2, getter: () => m_Eps,  setter: v => m_Eps  = v)  // EPS
            .Field(lsb: 3, getter: () => m_Stp2, setter: v => m_Stp2 = v)  // STP2
            .Field(lsb: 4, getter: () => m_Fen,  setter: v => m_Fen  = v)  // FEN
            .Field(5, 2,   getter: () => m_Wlen, setter: v => m_Wlen = v)  // WLEN [6:5]
            .Field(lsb: 7, getter: () => m_Sps,  setter: v => m_Sps  = v); // SPS

        // UARTCR (0x030) — Control Register
        //   Master enable and per-direction enables. TXE and RXE reset to 1.
        //   Bit 0  UARTEN: master UART enable.
        //   Bit 1  SIREN:  IrDA SIR enable — not supported on RP2350; ignored.
        //   Bit 2  SIRLP:  IrDA low-power — not supported; ignored.
        //   Bits 6:3: reserved.
        //   Bit 7  LBE:    loopback — routes TX back into RX FIFO internally.
        //   Bit 8  TXE:    transmit section enable.
        //   Bit 9  RXE:    receive section enable.
        //   Bit 10 DTR:    modem output. TODO: drive GPIO pin.
        //   Bit 11 RTS:    modem output. TODO: drive GPIO pin.
        //   Bit 12 OUT1:   modem output. TODO: drive GPIO pin.
        //   Bit 13 OUT2:   modem output. TODO: drive GPIO pin.
        //   Bit 14 RTSEN:  RTS flow control. TODO: wire nUARTRTS GPIO line.
        //   Bit 15 CTSEN:  CTS flow control. TODO: gate TX on nUARTCTS GPIO line.
        AddRegister(0x030, "UARTCR", resetValue: (1u << 8) | (1u << 9)) // TXE=1, RXE=1
            .Field(lsb:  0, getter: () => m_Uarten, setter: v => m_Uarten = v)
            .Field(lsb:  7, getter: () => m_Lbe,    setter: v => m_Lbe    = v)
            .Field(lsb:  8, getter: () => m_Txe,    setter: v => m_Txe    = v)
            .Field(lsb:  9, getter: () => m_Rxe,    setter: v => m_Rxe    = v)
            .Field(lsb: 10, getter: () => m_Dtr,    setter: v => m_Dtr    = v)
            .Field(lsb: 11, getter: () => m_Rts,    setter: v => m_Rts    = v)
            .Field(lsb: 12, getter: () => m_Out1,   setter: v => m_Out1   = v)
            .Field(lsb: 13, getter: () => m_Out2,   setter: v => m_Out2   = v)
            .Field(lsb: 14, getter: () => m_Rtsen,  setter: v => m_Rtsen  = v)
            .Field(lsb: 15, getter: () => m_Ctsen,  setter: v => m_Ctsen  = v);

        // UARTIFLS (0x034) — Interrupt FIFO Level Select
        //   TXIFLSEL (bits 2:0): TX interrupt fires when TX FIFO is at or below
        //     this fraction of its 32-entry capacity.
        //   RXIFLSEL (bits 5:3): RX interrupt fires when RX FIFO is at or above
        //     this fraction.
        //   Encoding (both fields): 0b000=1/8 (4), 0b001=1/4 (8), 0b010=1/2 (16),
        //     0b011=3/4 (24), 0b100=7/8 (28). Values 0b101–0b111 reserved.
        //   Reset default 0b010_010: both thresholds at 1/2 full.
        AddRegister(0x034, "UARTIFLS", resetValue: 0b010_010u)
            .Field(0, 3, () => m_TxIflSel, v => m_TxIflSel = v)  // TXIFLSEL [2:0]
            .Field(3, 3, () => m_RxIflSel, v => m_RxIflSel = v); // RXIFLSEL [5:3]

        // UARTIMSC (0x038) — Interrupt Mask Set/Clear
        //   A 1 in a bit position enables that interrupt source to assert UARTINTR.
        //   A 0 silences it. UARTMIS = UARTRIS & UARTIMSC.
        //   Bits 3:0  — modem interrupts (always 0 in emulator, masking has no effect)
        //   Bit  4    — RXIM:  RX FIFO at threshold
        //   Bit  5    — TXIM:  TX FIFO below threshold (transition-based)
        //   Bit  6    — RTIM:  receive timeout (TODO: not implemented)
        //   Bit  7    — FEIM:  framing error (always 0 in emulator)
        //   Bit  8    — PEIM:  parity error  (always 0 in emulator)
        //   Bit  9    — BEIM:  break error   (always 0 in emulator)
        //   Bit  10   — OEIM:  overrun error (always 0 in emulator)
        AddRegister(0x038, "UARTIMSC")
            .Field(0, 11, () => m_Imsc, v => m_Imsc = v);

        // UARTRIS (0x03C) — Raw Interrupt Status (read-only)
        //   Same bit layout as UARTIMSC. Reflects interrupt source state before
        //   masking. Fully recomputed on every read.
        //   NOTE: In real hardware UARTINTR (the ORed masked output) is wired to
        //   the platform interrupt controller (NVIC). The emulator maintains this
        //   register correctly for polling, but no CPU interrupt is generated.
        //   TODO: Assert CPU MIP.MEIP when any bit in UARTMIS is set, once the
        //         CPU interrupt infrastructure is available.
        AddRegister(0x03C, "UARTRIS")
            .OnRead(ComputeRawInterruptStatus);

        // UARTMIS (0x040) — Masked Interrupt Status (read-only)
        //   UARTMIS = UARTRIS & UARTIMSC. The interrupt handler reads this to
        //   determine which enabled source fired before clearing via UARTICR.
        AddRegister(0x040, "UARTMIS")
            .OnRead(() => ComputeRawInterruptStatus() & m_Imsc);

        // UARTICR (0x044) — Interrupt Clear Register (write-only; reads as 0)
        //   Writing 1 to a bit clears the corresponding UARTRIS bit.
        //   RXIC (bit 4): has no lasting effect while the RX FIFO remains at or
        //     above the threshold — RXRIS re-asserts immediately on the next check.
        //   TXIC (bit 5): clears the TX transition latch (m_TxRis). TXRIS can only
        //     re-assert after the next TransmitData call.
        //   RTIC (bit 6): TODO when receive timeout is implemented.
        //   Bits 7–10 (error clears): no-op, error bits are always 0 here.
        AddRegister(0x044, "UARTICR")
            .OnRead(() => 0u)
            .OnWrite(ClearInterrupts);

        // UARTDMACR (0x048) — DMA Control Register
        //   Bit 0 RXDMAE: enable DMA requests from the RX FIFO.
        //   Bit 1 TXDMAE: enable DMA requests from the TX FIFO.
        //   Bit 2 DMAONERR: suppress DMA when error interrupt is asserted.
        //   The SDK writes TXDMAE|RXDMAE in uart_init; stored silently here.
        //   TODO: Connect to DMA peripheral request lines when DMA is implemented.
        AddRegister(0x048, "UARTDMACR");

        // ─── Peripheral and cell identification registers ─────────────────────
        // Read-only, fixed values. Software reads these to verify it is talking
        // to a genuine ARM PL011 UART (revision r1p5 on RP2350).
        //
        // PeriphID encodes: part number (PL011 = 0x011), designer (ARM = 0x41),
        // revision (r1p5 = 0x3). CellID = 0xB105F00D identifies an ARM AMBA component.

        // UARTPERIPHID0 (0xFE0): PARTNUMBER0 [7:0] = 0x11
        AddRegister(0xFE0, "UARTPERIPHID0", resetValue: 0x11);
        // UARTPERIPHID1 (0xFE4): DESIGNER0 [7:4] = 0x1, PARTNUMBER1 [3:0] = 0x0 → 0x10
        AddRegister(0xFE4, "UARTPERIPHID1", resetValue: 0x10);
        // UARTPERIPHID2 (0xFE8): REVISION [7:4] = 0x3 (r1p5), DESIGNER1 [3:0] = 0x4 → 0x34
        AddRegister(0xFE8, "UARTPERIPHID2", resetValue: 0x34);
        // UARTPERIPHID3 (0xFEC): 0x00
        AddRegister(0xFEC, "UARTPERIPHID3", resetValue: 0x00);
        // UARTPCELLID0–3 (0xFF0–0xFFC): standard ARM cell ID 0x0D / 0xF0 / 0x05 / 0xB1
        AddRegister(0xFF0, "UARTPCELLID0", resetValue: 0x0D);
        AddRegister(0xFF4, "UARTPCELLID1", resetValue: 0xF0);
        AddRegister(0xFF8, "UARTPCELLID2", resetValue: 0x05);
        AddRegister(0xFFC, "UARTPCELLID3", resetValue: 0xB1);
    }

    // ─── UARTDR read/write handlers ───────────────────────────────────────────

    /// <summary>
    /// Called when the CPU writes to UARTDR (transmit path).
    /// Only the low 8 bits are used; upper bits are reserved and ignored.
    /// </summary>
    private void TransmitData(uint data)
    {
        var b = (byte)(data & 0xFF);

        if (m_Lbe)
        {
            // Loopback mode: route the transmitted byte directly into the RX FIFO
            // instead of the TX pin. This mirrors real hardware where UARTTXD is
            // internally connected to UARTRXD when LBE is set. Used for self-test.
            if (m_RxFifo.Count < RxFifoCapacity)
            {
                m_RxFifo.Enqueue(b);
                UpdateRxInterruptStatus();
            }
        }
        else
        {
            // Normal TX: deliver the character to external observers (e.g., UI terminal).
            ReceivedData?.Invoke((char)b);
        }

        // TXRIS uses transition-based semantics: it asserts when the TX FIFO drains
        // to or below the TXIFLSEL threshold. In the emulator TX is instant, so the
        // FIFO always transitions to empty on every write — asserting TXRIS each time.
        // TXRIS starts at 0 and only sets after the first transmission (i.e., it is
        // not pre-asserted just because the FIFO happens to be empty at boot).
        m_TxRis = true;
    }

    /// <summary>
    /// Called when the CPU reads UARTDR (receive path).
    /// Dequeues one byte from the RX FIFO. Error bits 11:8 are always 0.
    /// Returns 0 when the FIFO is empty (undefined in hardware; 0 is a safe default).
    /// </summary>
    private uint ReceiveData()
    {
        if (m_RxFifo.TryDequeue(out var b))
        {
            // After a dequeue the FIFO level may have dropped below the threshold,
            // which would de-assert RXRIS.
            UpdateRxInterruptStatus();
            // Error bits 11:8 (OE/BE/PE/FE) are always 0 in the emulator because
            // we don't simulate serial line signal integrity.
            return b;
        }

        return 0;
    }

    // ─── Interrupt helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the current raw interrupt status word (UARTRIS).
    /// Called on every read of UARTRIS and UARTMIS.
    /// </summary>
    private uint ComputeRawInterruptStatus()
    {
        uint ris = 0;

        // Bit 4: RXRIS — level-based, mirrors m_RxRis which tracks FIFO vs threshold.
        if (m_RxRis) ris |= 1u << 4;

        // Bit 5: TXRIS — transition-based latch set after each TX drains the FIFO.
        if (m_TxRis) ris |= 1u << 5;

        // Bit 6: RTRIS — receive timeout (32 bit periods of RX idle while FIFO non-empty).
        // TODO: implement with a timer. Always 0 for now.

        // Bits 7–10: error interrupts (FERIS/PERIS/BERIS/OERIS). Always 0 — no serial errors.

        // Bits 3–0: modem status interrupts. Always 0 — RP2350 has no modem signals.

        return ris;
    }

    /// <summary>
    /// Handles a write to UARTICR. Writing 1 to a bit clears the corresponding
    /// raw interrupt status bit. Writing 0 has no effect.
    /// </summary>
    private void ClearInterrupts(uint icr)
    {
        if ((icr & (1u << 4)) != 0)
        {
            // RXIC: clear the RX interrupt latch.
            // If the RX FIFO is still at or above the threshold, this has no lasting
            // effect — UpdateRxInterruptStatus will re-assert m_RxRis on the next call.
            m_RxRis = false;
        }

        if ((icr & (1u << 5)) != 0)
        {
            // TXIC: clear the TX transition latch. TXRIS will re-assert only after
            // the next byte is transmitted.
            m_TxRis = false;
        }

        // RTIC (bit 6): TODO when receive timeout is implemented.
        // Bits 7–10 (error clears): no-op; errors are never set in the emulator.
        // Bits 3–0 (modem clears): no-op; modem signals not present on RP2350.
    }

    /// <summary>
    /// Recomputes RXRIS from the current RX FIFO level and the programmed threshold.
    /// Called after every enqueue and dequeue so that the interrupt status stays
    /// in sync with the FIFO without requiring any polling.
    /// </summary>
    private void UpdateRxInterruptStatus()
    {
        // In FIFO mode (FEN=1) the threshold is a fraction of the 32-entry FIFO.
        // In character mode (FEN=0) the threshold is 1 — any data triggers the interrupt.
        m_RxRis = m_RxFifo.Count >= RxFifoThreshold;
    }

    // ─── FIFO helpers ─────────────────────────────────────────────────────────

    /// <summary>Maximum RX FIFO depth: 32 in FIFO mode, 1 in character mode.</summary>
    private int RxFifoCapacity => m_Fen ? 32 : 1;

    /// <summary>RXFF: RX FIFO is full (at capacity).</summary>
    private bool RxFifoFull => m_RxFifo.Count >= RxFifoCapacity;

    /// <summary>
    /// Number of RX FIFO entries that trigger RXRIS.
    /// In character mode (FEN=0) the threshold is always 1 (any data).
    /// In FIFO mode the RXIFLSEL encoding maps to fractions of 32:
    ///   0b000 = 1/8 = 4, 0b001 = 1/4 = 8, 0b010 = 1/2 = 16,
    ///   0b011 = 3/4 = 24, 0b100 = 7/8 = 28.
    /// </summary>
    private int RxFifoThreshold => m_Fen
        ? m_RxIflSel switch
        {
            0 => 4,
            1 => 8,
            2 => 16,
            3 => 24,
            4 => 28,
            _ => 16
        }
        : 1;
}
