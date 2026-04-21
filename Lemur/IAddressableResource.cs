namespace Lemur;

public interface IAddressableResource
{
    uint BaseAddress { get; }
    uint Size { get; }
    byte[] Read(uint address, int count);
    void Write(uint address, byte[] data);
}
