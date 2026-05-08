using Lemur.Processor.CFormats;
using Lemur.Processor.Formats;
using System;

namespace Lemur.Processor;

public class CDecoder : IDecoder
{
    private readonly IDecoder m_InnerDecoder;
    private readonly CFormatFactoryBase[] m_FormatFactories;

    public CDecoder(IDecoder innerDecoder)
    {
        m_InnerDecoder = innerDecoder;

        m_FormatFactories =
        [
            new CIFormatFactory(),
            new CSFormatFactory(),
            new CLFormatFactory(),
            new CSSFormatFactory(),
            new CRFormatFactory(),
            new CAFormatFactory(),
            new CBFormatFactory(),
            new CJFormatFactory(),
            new ZcbExtensionFormatFactory(),
            new ZcmpFormatFactory(),
        ];
    }

    public FormatBase Decode(uint instruction)
    {
        uint op = instruction.ExtractBits(0, 2);

        // Instruction is not compressed
        if (op == 0b11)
        {
            return m_InnerDecoder.Decode(instruction);
        }

        // Get just first half of the instruction
        instruction = instruction.ExtractBits(0, 16);

        var quadrant = instruction.ExtractBits(0, 2);
        var funct3 = instruction.ExtractBits(13, 3);
        foreach (var factory in m_FormatFactories)
        {
            if (factory.ForQuadrant.Contains(quadrant) && factory.ForFunct3.Contains(funct3))
            {
                var format = factory.Decode(funct3, instruction);
                if (format != null)
                {
                    return format;
                }
            }
        }

        throw new RiscVException(ExceptionCause.IllegalInstruction, instruction, $"Unknown compressed instruction {instruction.ToHex(4)}");
    }

}
