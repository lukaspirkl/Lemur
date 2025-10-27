namespace Venture;

internal class Program
{
    private static void Main(string[] args)
    {
        var m = new Memory(@"Blink\KeySquareBlink.elf", 0x10000000, 1024 * 1024 * 2);
        var e = new Emulator(m);
        bool isRunning = true;

        e.ECall += (s, a) =>
        {
            isRunning = false;
        };

        while (isRunning)
        {
            e.ExecuteInstruction();
        }
    }
}
