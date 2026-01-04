using Venture.Processor.Formats;

namespace Venture.Processor;

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

        throw new NotImplementedException($"Unknown instruction format for opcode {opcode.ToBin(7)} (instruction {instruction.ToHex()})");
    }
}
