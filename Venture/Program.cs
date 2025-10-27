namespace Venture;

internal class Program
{
    private static void Main(string[] args)
    {
        bool isRunning = true;

        var m = new Memory(@"Blink\KeySquareBlink.elf", 0x10000000, 1024 * 1024 * 2);
        var e = new Emulator(m);
        e.AddRW32I();

        while (isRunning)
        {
            e.Step();
        }
    }
}
