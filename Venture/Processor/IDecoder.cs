using Venture.Processor.Formats;

namespace Venture.Processor
{
    public interface IDecoder
    {
        FormatBase Decode(uint instruction);
    }
}