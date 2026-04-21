namespace Lemur.Csr;

public interface ICsrWindowed
{
    int  WindowCount   { get; }
    int  BitsPerWindow { get; }
    int  BitsPerItem   { get; }  // 1 for meiea/meifa/meipa (one flag per IRQ), 4 for meipra (priority nibble per IRQ)
    uint PeekWindow(int index);
}
