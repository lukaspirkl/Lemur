namespace Venture;

public interface IAddressableResource
{
    uint StartAddress { get; }
    uint Size { get; }
    byte[] Read(uint address, int count);
    void Write(uint address, byte[] data);
}
