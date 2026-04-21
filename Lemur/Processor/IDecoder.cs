using Lemur.Processor.Formats;

namespace Lemur.Processor
{
    public interface IDecoder
    {
        FormatBase Decode(uint instruction);
    }
}