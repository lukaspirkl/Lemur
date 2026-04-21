using Lemur.Processor.Formats;
using System;

namespace Lemur.Processor;

public class Decoder : IDecoder
{
    private readonly FormatFactoryBase[] m_FormatFactories;

    public Decoder()
    {
        m_FormatFactories =
        [
            new Xh3powerExtensionFormatFactory(),
            new Xh3bextmExtensionFormatFactory(),
            new UFormatFactory(),
            new JFormatFactory(),
            new BFormatFactory(),
            new RFormatFactory(),
            new SFormatFactory(),
            new IFormatFactory(),
            new AExtensionFormatFactory(),
            new BExtensionFormatFactory(),
            new MExtensionFormatFactory(),
        ];
    }

    public FormatBase Decode(uint instruction)
    {

        var opcode = instruction.ExtractBits(0, 7);
        foreach (var factory in m_FormatFactories)
        {
            if (factory.ForOpcodes.Contains(opcode))
            {
                var format = factory.Decode(instruction);
                if (format != null)
                {
                    return format;
                }
            }
        }

        // Unrecognised instruction — raise an illegal-instruction exception so the CPU trap
        // handler can deal with it gracefully. MTVAL receives the raw instruction word so that
        // the handler (or a debugger) can inspect what was fetched.
        // Spec: RISC-V Privileged ISA Section 3.1.15 — mtval for illegal instruction.
        throw new RiscVException(ExceptionCause.IllegalInstruction, instruction,
            $"Unknown instruction format for opcode {opcode.ToBin(7)} (instruction {instruction.ToHex()})");
    }
}
