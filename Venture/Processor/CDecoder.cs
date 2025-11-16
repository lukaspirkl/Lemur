using Venture.Processor.CFormats;
using Venture.Processor.Formats;

namespace Venture.Processor;

public class CDecoder : IDecoder
{
    private readonly IDecoder innerDecoder;
    private readonly CFormatFactoryBase[] formatFactories;

    public CDecoder(IDecoder innerDecoder)
    {
        this.innerDecoder = innerDecoder;

        formatFactories =
        [
            new CIFormatFactory(),
            new CSFormatFactory(),
            new CLFormatFactory(),
            new CSSFormatFactory(),
            new CRFormatFactory(),
            new CAFormatFactory(),
            new CBFormatFactory(),
            new CJFormatFactory(),
        ];
    }

    public FormatBase Decode(uint instruction)
    {
        uint op = instruction.ExtractBits(0, 2);

        // Instruction is not compressed
        if (op == 0b11)
        {
            return innerDecoder.Decode(instruction);
        }

        // Get just first half of the instruction
        instruction = instruction.ExtractBits(0, 16);

        var quadrant = instruction.ExtractBits(0, 2);
        var funct3 = instruction.ExtractBits(13, 3);
        foreach (var factory in formatFactories)
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

        throw new NotImplementedException($"Unknown compressed instruction {instruction.ToHex(4)}");
    }

}
