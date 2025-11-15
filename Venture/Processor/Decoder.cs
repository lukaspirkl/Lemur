using Venture.Processor.Formats;

namespace Venture.Processor;

public class Decoder : IDecoder
{
    private readonly FormatFactoryBase[] formatFactories;

    public Decoder()
    {
        formatFactories =
        [
            new UFormatFactory(),
            new JFormatFactory(),
            new BFormatFactory(),
            new RFormatFactory(),
            new SFormatFactory(),
            new IFormatFactory(),
        ];
    }

    public FormatBase Decode(uint instruction)
    {
        var opcode = instruction.ExtractBits(0, 7);
        foreach (var factory in formatFactories)
        {
            if (factory.ForOpcodes.Contains(opcode))
            {
                return factory.Decode(instruction);
            }
        }

        throw new NotImplementedException($"Unknown instruction format for opcode {opcode.ToBin(7)}");
    }
}
