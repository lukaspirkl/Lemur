namespace Venture.Processor;

/// <summary>
/// Minimal CSR read/write interface used by the processor and interrupt controller.
/// Intentionally narrow — callers that need metadata or direct entry access use <see cref="CSR"/>.
/// </summary>
public interface ICsrAccess
{
    uint Get(ushort address);
    void Set(ushort address, uint value);
}
